using System.Numerics;
using Bliss.CSharp;
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

    public static void Update()
    {
        // Update server and client
        Server?.Update();
        Client?.Update();
    }

    public static void CreateServer(ushort slots, string levelName)
    {
        Server = new Server();
        Server.Start(7777, slots);
        
        // Register server-side message handlers
        Server.MessageReceived += HandleServerMessageReceived;
        
        Logger.Info($"[SERVER] Server started on port 7777 with {slots} slots");
        
        Server.ClientConnected += (sender, args) =>
        {
            Logger.Info($"[SERVER] Client {args.Client.Id} connected");
            
            int sceneInt = 0;
            
            switch (levelName)
            {
                case "Level 1":
                    sceneInt = 1;
                    break;
                
                case "Level 2":
                    sceneInt = 2;
                    break;
            }
            
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
            Logger.Info($"[SERVER] Client {args.Client.Id} disconnected");
            
            // Remove from server's player list
            NetworkedPlayers.Remove(args.Client.Id);
            
            // Notify all clients to remove this player
            Message despawnMessage = Message.Create(MessageSendMode.Reliable, 4);
            despawnMessage.AddUShort(args.Client.Id);
            Server.SendToAll(despawnMessage);
        };
        
        Client = new Client();
        Client.Connected += OnClientConnected;
        Client.ConnectionFailed += OnClientConnectionFailed;
        Client.Disconnected += OnClientDisconnected;
        Client.Connect("127.0.0.1:7777");
        Logger.Info("[CLIENT] Host connecting to own server at 127.0.0.1:7777");
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
        }
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
        
        // Make sure to use the provided IP, not hardcoded localhost
        if (!ip.Contains(":"))
        {
            ip += ":7777"; // Add default port if not specified
        }
        Client.Connect(ip);
        Logger.Info($"[CLIENT] Connecting to server at {ip}");
    }
    
    private static void OnClientConnected(object sender, EventArgs e)
    {
        Logger.Info("[CLIENT] Successfully connected to server!");
    }
    
    private static void OnClientConnectionFailed(object sender, EventArgs e)
    {
        Logger.Error("[CLIENT] Failed to connect to server!");
    }
    
    private static void OnClientDisconnected(object sender, DisconnectedEventArgs e)
    {
        Logger.Warn($"[CLIENT] Disconnected from server! Reason: {e.Reason}");
        
        // Clean up all networked players
        foreach (var player in NetworkedPlayers.Values)
        {
            player.Dispose();
        }
        NetworkedPlayers.Clear();
        
        // Optional: Switch to a background scene first if needed
        
        // Show the "Host Left" GUI with background image
        // (Make sure your HostLeavedGui includes a background Sprite component)
        GuiManager.SetGui(new HostLeavedGui());
        
        Logger.Info("[CLIENT] Host left - showing HostLeavedGui");
    }
    
    public static void Cleanup()
    {
        Logger.Info("[NETWORK] Starting cleanup...");
        
        // If we're a client (not hosting), disconnect cleanly so server knows we left
        if (Client != null && Client.IsConnected && (Server == null || !Server.IsRunning))
        {
            Logger.Info("[NETWORK] Client disconnecting from server");
            Client.Disconnect();
        }
        
        // Stop server FIRST if we're hosting - this will disconnect all clients
        if (Server != null && Server.IsRunning)
        {
            Logger.Info("[NETWORK] Stopping server - this will disconnect all clients");
            Server.Stop();
            Server = null;
        }
        
        // Disconnect our own client if we were hosting
        if (Client != null && Client.IsConnected)
        {
            Logger.Info("[NETWORK] Disconnecting host's client");
            Client.Disconnect();
            Client = null;
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
    [MessageHandler(1)]
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
    [MessageHandler(2)]
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
    [MessageHandler(3)]
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
    [MessageHandler(4)]
    private static void HandlePlayerDespawn(Message message)
    {
        ushort playerId = message.GetUShort();
        
        Logger.Info($"[DESPAWN] Received despawn message for player {playerId}");
        
        if (NetworkedPlayers.ContainsKey(playerId))
        {
            NetworkedPlayers[playerId].Dispose();
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
}