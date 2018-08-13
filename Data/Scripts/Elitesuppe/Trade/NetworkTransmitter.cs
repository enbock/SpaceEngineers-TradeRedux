using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace Elitesuppe.Trade
{
    public static class NetworkTransmitter
    {
        private static readonly long ModMessageHandlerId = 82172351;
        private static readonly ushort NetMessageHandlerId = 1376;

        static NetworkTransmitter()
        {
            MyAPIGateway.Utilities.RegisterMessageHandler(ModMessageHandlerId, HandleModMessage);
            MyAPIGateway.Multiplayer.RegisterMessageHandler(
                (ushort) (NetMessageHandlerId + 1),
                ServerHandleNetMessage
            );
            MyAPIGateway.Multiplayer.RegisterMessageHandler(NetMessageHandlerId, ClientHandleNetMessage);
        }

        private static bool? _multiPlayerCacheState = null;

        public static bool? IsSinglePlayerOrServer()
        {
            if (_multiPlayerCacheState.HasValue)
                return _multiPlayerCacheState.Value;

            if (MyAPIGateway.Multiplayer == null)
                return null; //return null as long as the game session has not finished loading

            if (MyAPIGateway.Multiplayer.MultiplayerActive)
            {
                if (MyAPIGateway.Multiplayer.IsServer)
                {
                    _multiPlayerCacheState = true;
                    return true;
                }

                _multiPlayerCacheState = false;
                return false;
            }

            //should be offline, use value of MyAPIGateway.Session.IsServer
            _multiPlayerCacheState = MyAPIGateway.Session.IsServer;
            return MyAPIGateway.Session.IsServer;
        }

        private static void HandleModMessage(object message)
        {
        }

        private static void ServerHandleNetMessage(byte[] data)
        {
            var message = MyAPIGateway.Utilities.SerializeFromBinary<ClientMessage>(data);

            var player = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(player, p => p.SteamUserId.Equals(message.SendingPlayer));

            var sender = player.FirstOrDefault();
            if (sender == null)
            {
                MyAPIGateway.Utilities.ShowMessage("TE", "no player matching found: " + message.SendingPlayer);
                return;
            }


            switch (message.Method)
            {
                case MethodType.MethodSlashCommand:
                    try
                    {
                        switch (message.Message.ToLowerInvariant())
                        {
                            case "exampleAdmin":
                                /*if (CheckAdmin(sender))
                                {
                                    ChatWorkers.CargoSetup(sender.GetPosition());
                                }*/

                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        //MyAPIGateway.Utilities.ShowMessage("TE", "exception: "+e.Message);
                        SendToClient(
                            new ServerMessage {Method = MethodType.MethodChatMessage, Message = e.Message},
                            message.SendingPlayer
                        );
                        return;
                    }

                    break;
                case MethodType.MethodSpawnPrefab:
                    //MyAPIGateway.Utilities.ShowMessage("TE", "Spawn Prefab requested:"+message.Message);
                    break;
            }
        }

        private static void ClientHandleNetMessage(byte[] data)
        {
            var message = MyAPIGateway.Utilities.SerializeFromBinary<ServerMessage>(data);

            switch (message.Method)
            {
                case MethodType.MethodChatMessage:
                    MyAPIGateway.Utilities.ShowMessage("Server", message.Message);
                    break;
                default:
                    break;
            }
        }

        public static void SlashCommand(string command)
        {
            SendToServer(
                new ClientMessage
                {
                    SendingPlayer = MyAPIGateway.Multiplayer.MyId, Message = command,
                    Method = MethodType.MethodSlashCommand
                }
            );
        }

        public static void SpawnPrefab(string prefabName, Vector3D coords, long entityIdToGrantCredits = 0)
        {
        }

        private static bool SendToServer(ClientMessage msg)
        {
            return MyAPIGateway.Multiplayer.SendMessageToServer(
                (ushort) (NetMessageHandlerId + 1),
                MyAPIGateway.Utilities.SerializeToBinary(msg),
                true
            );
        }

        private static bool SendToClient(ServerMessage msg, ulong clientId)
        {
            if (clientId == 0)
            {
                return MyAPIGateway.Multiplayer.SendMessageToOthers(
                    NetMessageHandlerId,
                    MyAPIGateway.Utilities.SerializeToBinary(msg),
                    true
                );
            }

            return MyAPIGateway.Multiplayer.SendMessageTo(
                NetMessageHandlerId,
                MyAPIGateway.Utilities.SerializeToBinary(msg),
                clientId,
                true
            );
        }

        private static bool CheckAdmin(IMyPlayer player)
        {
            if (player.PromoteLevel < MyPromoteLevel.SpaceMaster)
            {
                SendToClient(
                    new ServerMessage
                        {Method = MethodType.MethodChatMessage, Message = "ERROR: no admin or spacemaster"},
                    player.SteamUserId
                );
                return false;
            }

            return true;
        }
    }

    [Serializable]
    [ProtoContract]
    public class ClientMessage
    {
        public ClientMessage()
        {
        }

        [ProtoMember] public ulong SendingPlayer { get; set; }
        [ProtoMember] public string Message { get; set; }
        [ProtoMember] public MethodType Method { get; set; }
    }

    [Serializable]
    [ProtoContract]
    public enum MethodType
    {
        MethodChatMessage = 0,
        MethodSlashCommand = 1,
        MethodSpawnPrefab = 2
    }

    [Serializable]
    [ProtoContract]
    public class ServerMessage
    {
        public ServerMessage()
        {
        }

        [ProtoMember] public string Message { get; set; }
        [ProtoMember] public MethodType Method { get; set; }
    }
}