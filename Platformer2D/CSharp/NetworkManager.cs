using System.Numerics;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Transformations;
using Platformer2D.CSharp.Entities;
using Platformer2D.CSharp.GUIs;
using Platformer2D.CSharp.Scenes.Levels;
using Riptide;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.Scenes;

namespace Platformer2D.CSharp;

public static class NetworkManager
{
    public static Server? Server;
    public static Client? Client;
    
    // Track which player ID belongs to this client
    public static ushort LocalPlayerId;
    
    // Dictionary to track all networked players by their client ID
    public static Dictionary<ushort, Player> NetworkedPlayers = new();
    
    // Flag to prevent showing HostLeavedGui during level transitions
    private static bool _isLevelTransition = false;
    
    // Public getter for level transition flag
    public static bool IsLevelTransition => _isLevelTransition;
    
    // Track current level for all clients
    private static string _currentLevel = "";
    
    // Connection callbacks
    private static Action? _onConnectionSuccess;
    private static Action<string>? _onConnectionFailed;

    public static void Update()
    {
        // Update server and client
        Server?.Update();
        Client?.Update();
    }

    public static void CreateServer(ushort slots, string levelName)
    {
        _currentLevel = levelName;
        
        Server = new Server();
        Server.Start(7777, slots);
        
        // Register server-side message handlers
        Server.MessageReceived += HandleServerMessageReceived;
        
        Logger.Info($"[SERVER] Server started on port 7777 with {slots} slots");
        
        Server.ClientConnected += (sender, args) =>
        {
            Logger.Info($"[SERVER] Client {args.Client.Id} connected");
            
            int sceneInt = GetSceneInt(_currentLevel);
            
            Message message = Message.Create(MessageSendMode.Reliable, 1);
            message.AddInt(sceneInt);
            message.AddUShort(args.Client.Id);
            
            // Send list of existing player IDs
            List<ushort> existingPlayerIds = new List<ushort>();
            for (ushort i = 1; i < args.Client.Id; i++)
            {
                if (NetworkedPlayers.ContainsKey(i))
                {
                    existingPlayerIds.Add(i);
                }
            }
            
            Logger.Info($"[SERVER] Sending {existingPlayerIds.Count} existing players to client {args.Client.Id}");
            
            message.AddInt(existingPlayerIds.Count);
            foreach (ushort playerId in existingPlayerIds)
            {
                message.AddUShort(playerId);
            }
            
            Server.Send(message, args.Client);
            
            // Notify all other clients about the new player
            Message spawnMessage = Message.Create(MessageSendMode.Reliable, 3);
            spawnMessage.AddUShort(args.Client.Id);
            Server.SendToAll(spawnMessage, args.Client.Id);
            
            Logger.Info($"[SERVER] Notified all clients about new player {args.Client.Id}");
        };
        
        Server.ClientDisconnected += (sender, args) =>
        {
            Logger.Info($"[SERVER] Client {args.Client.Id} disconnected - preparing despawn");
            
            // Remove from server's player list
            NetworkedPlayers.Remove(args.Client.Id);
            
            // Notify all REMAINING clients to remove this player
            Message despawnMessage = Message.Create(MessageSendMode.Reliable, 4);
            despawnMessage.AddUShort(args.Client.Id);
            
            // Send to all remaining clients (this excludes the disconnected client)
            Server.SendToAll(despawnMessage);
            
            // Force the server to process and send pending messages immediately
            Server.Update();
            
            Logger.Info($"[SERVER] Sent despawn message for player {args.Client.Id} to all remaining clients");
        };
        
        Client = new Client();
        Client.Connected += OnClientConnected;
        Client.ConnectionFailed += OnClientConnectionFailed;
        Client.Disconnected += OnClientDisconnected;
        Client.MessageReceived += HandleClientMessageReceived;
        Client.Connect("127.0.0.1:7777");
        Logger.Info("[CLIENT] Host connecting to own server at 127.0.0.1:7777");
    }
    
    // Helper method to convert level name to int
    private static int GetSceneInt(string levelName)
    {
        switch (levelName)
        {
            case "Level 1":
                return 1;
            case "Level 2":
                return 2;
            default:
                return 1;
        }
    }
    
    // Server-side message handler
    private static void HandleServerMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        ushort messageId = e.MessageId;
        
        Logger.Info($"[SERVER] Received message {messageId} from client {e.FromConnection.Id}");
        
        switch (messageId)
        {
            case 2: // Position update
                HandleServerPositionUpdate(e.Message, e.FromConnection.Id);
                break;
            case 5: // Client disconnect request
                HandleClientDisconnectRequest(e.Message, e.FromConnection.Id);
                break;
            case 6: // Level completion
                HandleLevelCompletion(e.Message, e.FromConnection.Id);
                break;
        }
    }
    
    // Handle level completion from a client
    private static void HandleLevelCompletion(Message message, ushort fromClientId)
    {
        string nextLevel = message.GetString();
        
        Logger.Info($"[SERVER] Player {fromClientId} completed level, transitioning all players to {nextLevel}");
        
        _currentLevel = nextLevel;
        int sceneInt = GetSceneInt(nextLevel);
        
        // Remember all connected player IDs before transition
        List<ushort> connectedPlayers = new List<ushort>(NetworkedPlayers.Keys);
        Logger.Info($"[SERVER] Current players before transition: {string.Join(", ", connectedPlayers)}");
        
        // Send level transition message to ALL clients
        Message levelTransitionMessage = Message.Create(MessageSendMode.Reliable, 7);
        levelTransitionMessage.AddInt(sceneInt);
        Server.SendToAll(levelTransitionMessage);
        
        // Force server update to ensure message is sent
        Server.Update();
        
        Logger.Info($"[SERVER] Sent level transition to all clients: {nextLevel}");
        Logger.Info($"[SERVER] Players should recreate: {string.Join(", ", connectedPlayers)}");
    }
    
    // Handle when a client explicitly tells us they're disconnecting
    private static void HandleClientDisconnectRequest(Message message, ushort fromClientId)
    {
        ushort playerId = message.GetUShort();
        
        Logger.Info($"[SERVER] Client {fromClientId} (Player {playerId}) requested disconnect");
        
        foreach (var valuePair in NetworkedPlayers)
        {
            if (valuePair.Key == playerId)
            {
                SceneManager.ActiveScene?.RemoveEntity(valuePair.Value);
            }
        }
        
        // Remove from server's player list
        NetworkedPlayers.Remove(playerId);
        
        // Notify ALL OTHER clients to remove this player
        Message despawnMessage = Message.Create(MessageSendMode.Reliable, 4);
        despawnMessage.AddUShort(playerId);
        
        // Send to all clients EXCEPT the one disconnecting
        Server.SendToAll(despawnMessage, fromClientId);
        
        // Force immediate send
        Server.Update();
        
        Logger.Info($"[SERVER] Sent despawn message for player {playerId} to all other clients");
    }
    
    // Client-side message handler - routes messages to appropriate handlers
    private static void HandleClientMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        ushort messageId = e.MessageId;
        
        Logger.Info($"[CLIENT] Received message {messageId}");
        
        switch (messageId)
        {
            case 1: // Initial connection
                HandleInitialConnection(e.Message);
                break;
            case 2: // Position update
                HandlePlayerPositionUpdate(e.Message);
                break;
            case 3: // Spawn player
                HandlePlayerSpawn(e.Message);
                break;
            case 4: // Despawn player
                HandlePlayerDespawn(e.Message);
                break;
            case 7: // Level transition
                HandleLevelTransition(e.Message);
                break;
            default:
                Logger.Warn($"[CLIENT] Unknown message ID: {messageId}");
                break;
        }
    }
    
    // Handle level transition message from server
    private static void HandleLevelTransition(Message message)
    {
        int sceneInt = message.GetInt();
        
        Logger.Info($"[CLIENT] Received level transition to scene {sceneInt}");
        
        _isLevelTransition = true;
        
        // Remember all player IDs (except local)
        List<ushort> remotePlayerIds = new List<ushort>();
        foreach (var kvp in NetworkedPlayers)
        {
            if (kvp.Key != LocalPlayerId)
            {
                remotePlayerIds.Add(kvp.Key);
            }
        }
        
        Logger.Info($"[CLIENT] Remembered {remotePlayerIds.Count} remote players for recreation");
        
        // Clear all networked players from current scene
        foreach (var kvp in NetworkedPlayers.ToList())
        {
            if (SceneManager.ActiveScene != null)
            {
                SceneManager.ActiveScene.RemoveEntity(kvp.Value);
            }
            kvp.Value.Dispose();
        }
        NetworkedPlayers.Clear();
        
        // Load the new level
        switch (sceneInt)
        {
            case 1:
                Logger.Info("[CLIENT] Transitioning to Level1...");
                SceneManager.SetScene(new Level1());
                break;
            case 2:
                Logger.Info("[CLIENT] Transitioning to Level2...");
                SceneManager.SetScene(new Level2());
                break;
            default:
                Logger.Error($"[CLIENT] Unknown scene int: {sceneInt}");
                break;
        }
        
        // Recreate all players in new level
        if (SceneManager.ActiveScene != null)
        {
            // Recreate local player
            Player localPlayer = new Player(new Transform() { Translation = new Vector3(0, -16 * 2, 0) }, true);
            SceneManager.ActiveScene.AddEntity(localPlayer);
            NetworkedPlayers[LocalPlayerId] = localPlayer;
            
            Logger.Info($"[CLIENT] Recreated local player with ID {LocalPlayerId} in new level");
            
            // Recreate all remote players that were in the previous level
            foreach (ushort playerId in remotePlayerIds)
            {
                Player remotePlayer = new Player(new Transform() { Translation = new Vector3(0, -16 * 2, 0) }, false);
                SceneManager.ActiveScene.AddEntity(remotePlayer);
                NetworkedPlayers[playerId] = remotePlayer;
                
                Logger.Info($"[CLIENT] Recreated remote player with ID {playerId} in new level");
            }
        }
        
        _isLevelTransition = false;
        
        Logger.Info($"[CLIENT] Level transition complete. Total players: {NetworkedPlayers.Count}");
    }
    
    private static void HandleServerPositionUpdate(Message message, ushort fromClientId)
    {
        ushort playerId = message.GetUShort();
        float x = message.GetFloat();
        float y = message.GetFloat();
        float z = message.GetFloat();
        int poseType = message.GetInt();
        
        Logger.Info($"[SERVER] Received position from client {fromClientId}: Player {playerId} at ({x:F2}, {y:F2})");
        
        // Broadcast to all OTHER clients
        Message broadcastMessage = Message.Create(MessageSendMode.Unreliable, 2);
        broadcastMessage.AddUShort(playerId);
        broadcastMessage.AddFloat(x);
        broadcastMessage.AddFloat(y);
        broadcastMessage.AddFloat(z);
        broadcastMessage.AddInt(poseType);
        Server.SendToAll(broadcastMessage, fromClientId);
        
        Logger.Info($"[SERVER] Broadcasted player {playerId} position to all clients except {fromClientId}");
    }

    public static void JoinServer(string ip)
    {
        Client = new Client();
        Client.Connected += OnClientConnected;
        Client.ConnectionFailed += OnClientConnectionFailed;
        Client.Disconnected += OnClientDisconnected;
        Client.MessageReceived += HandleClientMessageReceived;
        
        // Make sure to use the provided IP, not hardcoded localhost
        if (!ip.Contains(":"))
        {
            ip += ":7777"; // Add default port if not specified
        }
        Client.Connect(ip);
        Logger.Info($"[CLIENT] Connecting to server at {ip}");
    }
    
    // Set callbacks for connection success/failure (used by JoinGui)
    public static void SetConnectionCallbacks(Action onSuccess, Action<string> onFailed)
    {
        _onConnectionSuccess = onSuccess;
        _onConnectionFailed = onFailed;
    }
    
    private static void OnClientConnected(object sender, EventArgs e)
    {
        Logger.Info("[CLIENT] Successfully connected to server!");
        
        // Call success callback if set
        _onConnectionSuccess?.Invoke();
        
        // Clear callbacks after use
        _onConnectionSuccess = null;
        _onConnectionFailed = null;
    }
    
    private static void OnClientConnectionFailed(object sender, EventArgs e)
    {
        Logger.Error("[CLIENT] Failed to connect to server!");
        
        // Call failure callback if set
        _onConnectionFailed?.Invoke("Unable to reach server");
        
        // Clear callbacks after use
        _onConnectionSuccess = null;
        _onConnectionFailed = null;
    }
    
    private static void OnClientDisconnected(object sender, DisconnectedEventArgs e)
    {
        Logger.Warn($"[CLIENT] Disconnected from server! Reason: {e.Reason}");
        
        // Don't show disconnect GUI during level transitions
        if (_isLevelTransition)
        {
            Logger.Info("[CLIENT] Ignoring disconnect during level transition");
            return;
        }
        
        // Clean up all networked players
        foreach (var player in NetworkedPlayers.Values)
        {
            player.Dispose();
        }
        NetworkedPlayers.Clear();
     
        GuiManager.SetGui(new HostLeavedGui());
    }
    
    public static void Cleanup()
    {
        Logger.Info("[NETWORK] Starting cleanup...");
        
        // If we're a client (not hosting), send disconnect message BEFORE actually disconnecting
        if (Client != null && Client.IsConnected && (Server == null || !Server.IsRunning))
        {
            Logger.Info("[NETWORK] Client sending disconnect message to server");
            
            // Send explicit disconnect message to server
            Message disconnectMessage = Message.Create(MessageSendMode.Reliable, 5);
            disconnectMessage.AddUShort(LocalPlayerId);
            Client.Send(disconnectMessage);
            
            // Give time for the message to be sent
            System.Threading.Thread.Sleep(200);
            
            Logger.Info("[NETWORK] Client disconnecting from server");
            Client.Disconnect();
            
            // Give the disconnect message time to be processed
            System.Threading.Thread.Sleep(200);
        }
        
        // If we're hosting, we need to handle this carefully
        if (Server != null && Server.IsRunning)
        {
            // First, send disconnect message from our own client
            if (Client != null && Client.IsConnected)
            {
                Logger.Info("[NETWORK] Host client sending disconnect message");
                
                // Send explicit disconnect message
                Message disconnectMessage = Message.Create(MessageSendMode.Reliable, 5);
                disconnectMessage.AddUShort(LocalPlayerId);
                Client.Send(disconnectMessage);
                
                // Give time for message to be sent
                System.Threading.Thread.Sleep(200);
                
                Logger.Info("[NETWORK] Host client disconnecting from own server");
                Client.Disconnect();
                
                // Process the disconnect on the server side
                Server.Update();
                
                // Give time for the despawn message to be sent to other clients
                System.Threading.Thread.Sleep(200);
                
                // Force one more server update to ensure all messages are sent
                Server.Update();
                System.Threading.Thread.Sleep(100);
                
                Client = null;
            }
            
            Logger.Info("[NETWORK] Stopping server - this will disconnect all remaining clients");
            Server.Stop();
            Server = null;
        }
        
        // Clean up all networked players
        foreach (var player in NetworkedPlayers.Values)
        {
            player.Dispose();
        }
        
        NetworkedPlayers.Clear();
        
        Logger.Info("[NETWORK] Full cleanup completed");
    }
    
    // Message 1: Initial connection - receive scene and player ID
    private static void HandleInitialConnection(Message message)
    {
        Logger.Info("[CLIENT] HandleInitialConnection called!");
        
        int sceneInt = message.GetInt();
        LocalPlayerId = message.GetUShort();
        
        Logger.Info($"[CLIENT] Received scene: {sceneInt}, LocalPlayerId: {LocalPlayerId}");
        
        int existingPlayerCount = message.GetInt();
        List<ushort> existingPlayerIds = new List<ushort>();
        for (int i = 0; i < existingPlayerCount; i++)
        {
            existingPlayerIds.Add(message.GetUShort());
        }
        
        Logger.Info($"[CLIENT] Existing players: {existingPlayerCount}");

        switch (sceneInt)
        {
            case 1:
                Logger.Info("[CLIENT] Loading Level1...");
                SceneManager.SetScene(new Level1());
                break;
            case 2:
                Logger.Info("[CLIENT] Loading Level2...");
                SceneManager.SetScene(new Level2());
                break;
            default:
                Logger.Error($"[CLIENT] Unknown scene int: {sceneInt}");
                break;
        }

        if (SceneManager.ActiveScene != null)
        {
            Logger.Info("[CLIENT] Scene loaded, creating players...");
            
            // Create local player
            Player localPlayer = new Player(new Transform() { Translation = new Vector3(0, -16 * 2, 0) }, true);
            SceneManager.ActiveScene.AddEntity(localPlayer);
            NetworkedPlayers[LocalPlayerId] = localPlayer;
            
            Logger.Info($"[CLIENT] Created local player with ID {LocalPlayerId}");
            
            // Create existing remote players
            foreach (ushort playerId in existingPlayerIds)
            {
                Player remotePlayer = new Player(new Transform() { Translation = new Vector3(0, -16 * 2, 0) }, false);
                SceneManager.ActiveScene.AddEntity(remotePlayer);
                NetworkedPlayers[playerId] = remotePlayer;
                
                Logger.Info($"[CLIENT] Created remote player with ID {playerId}");
            }
        }
        else
        {
            Logger.Error("[CLIENT] ActiveScene is null after SetScene!");
        }
    }
    
    // Message 2: Player position update (CLIENT receives broadcast from server)
    private static void HandlePlayerPositionUpdate(Message message)
    {
        ushort playerId = message.GetUShort();
        float x = message.GetFloat();
        float y = message.GetFloat();
        float z = message.GetFloat();
        int poseType = message.GetInt();
        
        Logger.Info($"[CLIENT RECV] Position update for player {playerId} at ({x:F2}, {y:F2})");
        
        // Update the player position if it's not our local player
        if (playerId != LocalPlayerId)
        {
            if (NetworkedPlayers.ContainsKey(playerId))
            {
                NetworkedPlayers[playerId].NetworkedPosition = new Vector3(x, y, z);
                NetworkedPlayers[playerId].NetworkedPoseType = (PlayerPoseType)poseType;
                Logger.Info($"[CLIENT RECV] Updated player {playerId} to ({x:F2}, {y:F2})");
            }
            else
            {
                Logger.Warn($"[CLIENT RECV] Player {playerId} not in dictionary! Available: {string.Join(", ", NetworkedPlayers.Keys)}");
            }
        }
        else
        {
            Logger.Info($"[CLIENT RECV] Ignoring update for local player {playerId}");
        }
    }
    
    // Message 3: Spawn new player
    private static void HandlePlayerSpawn(Message message)
    {
        ushort playerId = message.GetUShort();
        
        Logger.Info($"[SPAWN] Received spawn request for player {playerId}. LocalPlayerId: {LocalPlayerId}");
        
        if (playerId != LocalPlayerId && SceneManager.ActiveScene != null && !NetworkedPlayers.ContainsKey(playerId))
        {
            Player remotePlayer = new Player(new Transform() { Translation = new Vector3(0, -16 * 2, 0) }, false);
            SceneManager.ActiveScene.AddEntity(remotePlayer);
            NetworkedPlayers[playerId] = remotePlayer;
            
            Logger.Info($"[SPAWN] Created remote player {playerId}");
        }
        else
        {
            Logger.Warn($"[SPAWN] Skipped player {playerId} - Already exists or is local player");
        }
    }
    
    // Message 4: Despawn player
    private static void HandlePlayerDespawn(Message message)
    {
        ushort playerId = message.GetUShort();
        
        Logger.Info($"[DESPAWN] Received despawn message for player {playerId}");
        
        if (NetworkedPlayers.ContainsKey(playerId))
        {
            Player playerToRemove = NetworkedPlayers[playerId];
            
            // Remove from scene first
            if (SceneManager.ActiveScene != null)
            {
                SceneManager.ActiveScene.RemoveEntity(playerToRemove);
                Logger.Info($"[DESPAWN] Removed player {playerId} from scene");
            }
            
            // Then dispose and remove from dictionary
            playerToRemove.Dispose();
            NetworkedPlayers.Remove(playerId);
            
            Logger.Info($"[DESPAWN] Successfully despawned and removed player {playerId}");
        }
        else
        {
            Logger.Warn($"[DESPAWN] Player {playerId} not found in NetworkedPlayers dictionary");
        }
    }
    
    // Send player position update
    public static void SendPlayerPosition(Vector3 position, PlayerPoseType poseType)
    {
        if (Client != null && Client.IsConnected)
        {
            Message message = Message.Create(MessageSendMode.Unreliable, 2);
            message.AddUShort(LocalPlayerId);
            message.AddFloat(position.X);
            message.AddFloat(position.Y);
            message.AddFloat(position.Z);
            message.AddInt((int)poseType);
            Client.Send(message);
        }
    }
    
    // NEW: Send level completion notification to server
    public static void NotifyLevelComplete(string nextLevel)
    {
        if (Client != null && Client.IsConnected)
        {
            Logger.Info($"[CLIENT] Notifying server of level completion, next level: {nextLevel}");
            
            Message message = Message.Create(MessageSendMode.Reliable, 6);
            message.AddString(nextLevel);
            Client.Send(message);
        }
    }
}