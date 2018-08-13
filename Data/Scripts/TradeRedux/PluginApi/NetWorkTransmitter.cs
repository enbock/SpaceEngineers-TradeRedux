using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRageMath;

namespace TradeRedux.PluginApi
{
    public class NetWorkTransmitter
    {
        private static long MODMESSAGEHANDLERID = 82172351;
        private static ushort NETMESSAGEHANDLERID = 1376;
        static NetWorkTransmitter()
        {
            //MyAPIGateway.Utilities.RegisterMessageHandler(MODMESSAGEHANDLERID, HandleModMessage);
            MyAPIGateway.Multiplayer.RegisterMessageHandler((ushort)(NETMESSAGEHANDLERID + 1), ServerHandleNetMessage);
            MyAPIGateway.Multiplayer.RegisterMessageHandler(NETMESSAGEHANDLERID, ClientHandleNetMessage);


        }

        private static bool? _multiPlayerCacheState = null;
        public static bool? IsSinglePlayerOrServer()
        {
            if (_multiPlayerCacheState.HasValue)
                return _multiPlayerCacheState.Value;

            if (MyAPIGateway.Multiplayer == null)
                return null;//return null as long as the game session has not finished loading

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

        //private static void HandleModMessage(object message)
        //{

        //}
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
                case MethodType.METHOD_SLASHCOMMAND:
                    try
                    {
                        switch (message.Message.ToLowerInvariant())
                        {
                            /*case "reset":
                                if (CheckAdmin(sender))
                                {
                                    ChatWorkers.StationReset(sender.GetPosition());
                                }
                                break;
                            case "resetprices":
                                if (CheckAdmin(sender))
                                {
                                    ChatWorkers.StationResetPrice(sender.GetPosition());
                                }
                                break;*/
                            case "cargosetup":
                                if (CheckAdmin(sender))
                                {
                                    ChatWorkers.CargoSetup(sender.GetPosition());
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        //MyAPIGateway.Utilities.ShowMessage("TE", "exception: "+e.Message);
                        SendToClient(new ServerMessage { Method = MethodType.METHOD_CHATMESSAGE, Message = e.Message }, message.SendingPlayer);
                        return;
                    }
                    break;
                /*
                 case MethodType.METHOD_SPAWNPREFAB:
                    MyAPIGateway.Utilities.ShowMessage("TE", "Spawn Prefab requested:"+message.Message);
                    break;
                    */
                default:
                    break;
            }
        }

        private static void ClientHandleNetMessage(byte[] data)
        {
            var message = MyAPIGateway.Utilities.SerializeFromBinary<ServerMessage>(data);

            switch (message.Method)
            {
                case MethodType.METHOD_CHATMESSAGE:
                    MyAPIGateway.Utilities.ShowMessage("Server", message.Message);
                    break;
                default:
                    break;
            }
        }

        public static void SlashCommand(string command)
        {
            SendToServer(new ClientMessage { SendingPlayer = MyAPIGateway.Multiplayer.MyId, Message = command, Method = MethodType.METHOD_SLASHCOMMAND });
        }

        public static void SpawnPrefab(string prefabName, Vector3D coords, long entityIdToGrantCredits = 0)
        {

        }

        private static bool SendToServer(ClientMessage msg)
        {
            return MyAPIGateway.Multiplayer.SendMessageToServer((ushort)(NETMESSAGEHANDLERID + 1), MyAPIGateway.Utilities.SerializeToBinary(msg), true);
        }

        private static bool SendToClient(ServerMessage msg, ulong clientId)
        {
            if (clientId == 0)
            {
                return MyAPIGateway.Multiplayer.SendMessageToOthers(NETMESSAGEHANDLERID, MyAPIGateway.Utilities.SerializeToBinary(msg), true);
            }
            return MyAPIGateway.Multiplayer.SendMessageTo(NETMESSAGEHANDLERID, MyAPIGateway.Utilities.SerializeToBinary(msg), clientId, true);
        }

        private static bool CheckAdmin(IMyPlayer player)
        {
            if (player.PromoteLevel < MyPromoteLevel.SpaceMaster)
            {
                SendToClient(new ServerMessage { Method = MethodType.METHOD_CHATMESSAGE, Message = "ERROR: no admin or spacemaster" }, player.SteamUserId);
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
        [ProtoMember]
        public ulong SendingPlayer { get; set; }
        [ProtoMember]
        public string Message { get; set; }
        [ProtoMember]
        public MethodType Method { get; set; }

    }
    [Serializable]
    [ProtoContract]
    public enum MethodType
    {
        METHOD_CHATMESSAGE = 0,
        METHOD_SLASHCOMMAND = 1,
        METHOD_SPAWNPREFAB = 2,
    }
    [Serializable]
    [ProtoContract]
    public class ServerMessage
    {
        public ServerMessage()
        {

        }
        [ProtoMember]
        public string Message { get; set; }
        [ProtoMember]
        public MethodType Method { get; set; }

    }
}
