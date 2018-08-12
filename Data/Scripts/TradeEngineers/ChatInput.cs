using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.ModAPI;
using VRage.Game.ModAPI;
using System.Text.RegularExpressions;
using TradeEngineers.PluginApi;
using VRageMath;
using VRage.Game;
using System.Text;
using Sandbox.Definitions;

namespace TradeEngineers
{
    [VRage.Game.Components.MySessionComponentDescriptor(VRage.Game.Components.MyUpdateOrder.BeforeSimulation)]
    class ChatInput : VRage.Game.Components.MySessionComponentBase

    {

        private bool _IsLoaded = false;

        public override void UpdateBeforeSimulation()
        {
            if (!_IsLoaded)
            {
                init();
            }
        }

        public void init()
        {
            _IsLoaded = true;
            MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;
            MyAPIGateway.Utilities.ShowMessage("TradeEngineers", "Mod loaded. Type '/te help' for help.");
        }

        private static Regex commandRegex = new Regex(@"\/(te)\s+([\S]+)([\s\S]+)*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private void Utilities_MessageEntered(string messageText, ref bool sendToOthers)
        {
            try
            {

                var match = commandRegex.Match(messageText);
                if (match.Success)
                {
                    if (match.Groups.Count > 2)
                    {
                        sendToOthers = HandleChatInput(match.Groups[2].ToString(), match.Groups[3].ToString().Trim().Split(' ')
                            .Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToArray());
                    }
                    else sendToOthers = HandleChatInput(match.Groups[2].ToString());
                }
                else if (messageText.Equals("/te", StringComparison.InvariantCultureIgnoreCase))
                {
                    //MyAPIGateway.Utilities.ShowMessage("TradeEngineers", "Type '/te help' for help");
                    HandleChatInput("help");
                }
            }
            catch (Exception e)
            {
                MyAPIGateway.Utilities.ShowMessage("Exception in TradeEngineers chat parser", e.Message);
            }
        }

        private bool HandleChatInput(string cmd, string[] args = null)
        {
            //MyAPIGateway.Utilities.ShowMessage("TE", "c:"+cmd);
            //if (args != null)
            //    foreach (var a in args)
            //    {
            //        MyAPIGateway.Utilities.ShowMessage("TE", "a:" + a);
            //    }

            if (cmd.Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                TradeEngineersHelp.ShowHelp(TradeEngineersHelp.HELPGENERAL, "You can always type /te help to get to this screen");
                return false;
            }

            /*if(cmd.Equals("reset", StringComparison.InvariantCultureIgnoreCase))
            {
                //reset the station next to the player

                if(NetWorkTransmitter.IsSinglePlayerOrServer() ?? true)
                {
                    var stationName = ChatWorkers.StationReset(MyAPIGateway.Session.Player.GetPosition());
                    if (stationName == null)
                    {
                        MyAPIGateway.Utilities.ShowMessage("TE", "No station found!");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage("TE", "Trade Station '" + stationName + "' has been reset!");
                    }
                }
                else
                {
                    try
                    {
                        NetWorkTransmitter.SlashCommand("reset");
                    }
                    catch (Exception e)
                    {
                        MyAPIGateway.Utilities.ShowMessage("TE", e.Message);
                    }                    
                }

                
                return false;
            }*/


            /*if (cmd.Equals("resetprices", StringComparison.InvariantCultureIgnoreCase))
            {

                //reset price model of the station next to the player

                if (NetWorkTransmitter.IsSinglePlayerOrServer() ?? true)
                {
                    var stationName = ChatWorkers.StationResetPrice(MyAPIGateway.Session.Player.GetPosition());
                    if (stationName == null)
                    {
                        MyAPIGateway.Utilities.ShowMessage("TE", "No station found!");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage("TE", "Trade Station '" + stationName + "' price model has been reset!");
                    }
                }
                else
                {
                    try
                    {
                        NetWorkTransmitter.SlashCommand("resetprices");
                    }
                    catch (Exception e)
                    {
                        MyAPIGateway.Utilities.ShowMessage("TE", e.Message);
                    }
               }
               
                return false;
            }*/

            if (cmd.Equals("cargosetup", StringComparison.InvariantCultureIgnoreCase))
            {
                //define all cargo containers for buy/sell on next station to player
                if (NetWorkTransmitter.IsSinglePlayerOrServer() ?? true)
                {
                    var stationName = ChatWorkers.CargoSetup(MyAPIGateway.Session.Player.GetPosition());
                    if (stationName == null)
                    {
                        MyAPIGateway.Utilities.ShowMessage("TE", "No station found!");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage("TE", "Trade Station '" + stationName + "' has been setup!");
                    }
                }
                else
                {
                    try
                    {
                        NetWorkTransmitter.SlashCommand("cargosetup");
                    }
                    catch (Exception e)
                    {
                        MyAPIGateway.Utilities.ShowMessage("TE", e.Message + e.StackTrace);
                    }
                }

                return false;

            }

            /*
            if (cmd.Equals("buyship", StringComparison.InvariantCultureIgnoreCase))
            {

                if(args.Length != 1)
                {
                    MyAPIGateway.Utilities.ShowMessage("TE", "Available Ships:");
                    var prefabs = Sandbox.Definitions.MyDefinitionManager.Static.GetPrefabDefinitions().Select(p => p.Key);
                    StringBuilder b = new StringBuilder();
                    foreach (var p in prefabs)
                    {
                        b.Append(p);
                        b.Append(",");
                    }

                    
                    MyAPIGateway.Utilities.ShowMessage("TE", b.ToString(0,b.Length-1));
                    return false;
                }

                //define all cargo containers for buy/sell on next station to player
                if (NetWorkTransmitter.IsSinglePlayerOrServer() ?? true)
                {
                    var stationName = ChatWorkers.SpawnShip(args[0], MyAPIGateway.Session.Player.GetPosition()+new Vector3D(500,0,0));
                    
                    if (stationName == null)
                    {
                        MyAPIGateway.Utilities.ShowMessage("TE", "No station found!");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage("TE", "Trade Station '" + stationName + "' price model has been reset!");
                    }
                }
                else
                {
                    try
                    {
                        NetWorkTransmitter.SlashCommand("buyship " + string.Join(" ",args));

                    }
                    catch (Exception e)
                    {
                        MyAPIGateway.Utilities.ShowMessage("TE", e.Message);
                    }
                }
               
                return false;
            }
            */

            /*mein debug-code-test-cmd
            // /te skl
            if (cmd.Equals("skl", StringComparison.InvariantCultureIgnoreCase))
            {
                //var player = new List<IMyPlayer>();
                //MyAPIGateway.Multiplayer.Players.GetPlayers(player);//, p => p.IdentityId.Equals(MyAPIGateway.Multiplayer.MyId));
                //MyAPIGateway.Utilities.ShowMessage("TE", ""+player.FirstOrDefault().SteamUserId+ " "+ MyAPIGateway.Multiplayer.MyId);
                MyAPIGateway.Utilities.ShowMessage("TE", "" + MyAPIGateway.Multiplayer.MyId);
                var clm = new ClientMessage { SendingPlayer = MyAPIGateway.Multiplayer.MyId };
                MyAPIGateway.Utilities.ShowMessage("TE", "" + clm.SendingPlayer);
                var bin = MyAPIGateway.Utilities.SerializeToBinary(clm);
                var cl = MyAPIGateway.Utilities.SerializeFromBinary<ClientMessage>(bin);
                MyAPIGateway.Utilities.ShowMessage("TE", "" + cl.SendingPlayer);


                var sphereAroundPlayer = new VRageMath.BoundingSphereD(MyAPIGateway.Session.Player.GetPosition(), 500);
                var ships = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphereAroundPlayer);

                List<IMyEntity> ordered = ships.Where(b => (b as IMyCubeGrid) != null)
                                                .OrderBy(a => (MyAPIGateway.Session.Player.GetPosition() - a.GetPosition()).Length())
                                                .ToList();

                Inventory.RunMethodsDev.doThingsWithTwoShips(ordered[0], null);
                return false;
            }*/

            return true;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;
            base.UnloadData();
        }

    }


    public static class ChatWorkers
    {
        private static IEnumerable<IMyCubeGrid> GetStationsInRange(Vector3D playerPositon)
        {
            var sphereAroundPlayer = new VRageMath.BoundingSphereD(playerPositon, 500);
            var ships = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphereAroundPlayer);

            return ships.Where(b => (b as IMyCubeGrid) != null)
                                            .OrderBy(a => (playerPositon - a.GetPosition()).Length())
                                            .Select(a => a as IMyCubeGrid);
        }
        public static string CargoSetup(Vector3D playerPositon)
        {
            foreach (var grid in GetStationsInRange(playerPositon))
            {
                var tradeStation = StationManager.GetStations().FirstOrDefault(ts => ts.TradeBlock.GetTopMostParent().EntityId == grid.GetTopMostParent().EntityId);

                if (tradeStation != null)
                {
                    //MyAPIGateway.Utilities.ShowMessage("TE", "Test " + playerPositon);                
                    tradeStation.Station.SetupStation(tradeStation.TradeBlock, true);
                    return (grid.CustomName ?? grid.Name);
                }
            }
            return null;
        }

        public static string SpawnShip(string prefabname, Vector3D targetLocation, long payingPlayer = 0, long EntityToGrantCredits = 0)
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(players, p => p.IdentityId.Equals(payingPlayer));

            var payer = players.FirstOrDefault();
            var tradeStation = StationManager.GetStations().FirstOrDefault(ts => ts.TradeBlock.GetTopMostParent().EntityId == EntityToGrantCredits);

            var prefabs = Sandbox.Definitions.MyDefinitionManager.Static.GetPrefabDefinitions().Where(p => p.Key.Equals(prefabname, StringComparison.InvariantCultureIgnoreCase));

            if (!prefabs.Any())
            {
                MyAPIGateway.Utilities.ShowMessage("TE", "No such prefab: " + prefabname);
                return null;
            }
            var prefab = prefabs.FirstOrDefault().Value;

            var requirements = new Dictionary<MyDefinitionId, double>();
            var blocks = prefab.CubeGrids.SelectMany(cg => cg.CubeBlocks.Select(cb => cb.GetId())).
                Select(def =>
                {
                    MyCubeBlockDefinition de11;
                    if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(def, out de11))
                        return de11;
                    return null;
                }).Where(def => def != null).SelectMany(def => def.Components.Select(c => c));

            foreach (var bd in blocks.GroupBy(def => def.Definition))
            {
                var count = bd.Sum(d => d.Count);
                var curreq = Inventory.ItemDefinitionFactory.GetRecipeInput(bd.Key.Id, true);
                foreach (var req in curreq)
                {
                    if (requirements.ContainsKey(req.Key))
                        requirements[req.Key] += req.Value * count;
                    else requirements.Add(req.Key, req.Value * count);
                }

            }

            foreach (var req in requirements)
            {
                MyAPIGateway.Utilities.ShowMessage("TE", "" + req.Key + ": " + req.Value);

            }
            return "done";
        }

    }
}
