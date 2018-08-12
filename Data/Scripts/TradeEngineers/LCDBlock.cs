using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.ModAPI;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using TradeEngineers.SerializedTradeStorage;
using TradeEngineers.InputOutput;
using System.Text;
using TradeEngineers.PluginApi;

namespace TradeEngineers
{
    [MyEntityComponentDescriptor(typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_TextPanel), false)]
    public class LCDBlock : MyGameLogicComponent
    {
        public bool DEBUG = false;

        private VRage.ObjectBuilders.MyObjectBuilder_EntityBase _objectBuilder;
        private DateTime DisplayUpdateTime = DateTime.MinValue;
        private DateTime lastUpdate = DateTime.MinValue;
        private DateTime tradetimeout = DateTime.MinValue;
        private DateTime ProdCycleUpdateTime = DateTime.MinValue;

        private System.IO.TextWriter logger = null;
        private String timeofload = "" + DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + " " + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second;
        public Sandbox.ModAPI.IMyTextPanel myLcd;

        private DateTime StationLastSaved = DateTime.MinValue;
        public StationBase Station = null;

        //private string lastlcdtext = "";

        public override void Close()
        {
            if (Entity != null && myLcd != null && myLcd.IsFunctional && myLcd.IsWorking && myLcd.Enabled)
                Save();

            myLcd = null;
            if (logger != null)
                logger.Close();
        }

        public override void Init(VRage.ObjectBuilders.MyObjectBuilder_EntityBase objectBuilder)
        {
            log("Init...");

            _objectBuilder = objectBuilder;

            myLcd = (Entity as Sandbox.ModAPI.IMyTextPanel);
            if (myLcd != null && myLcd.BlockDefinition.SubtypeName.Contains("TradeInput"))
            {
                NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }

        }

        public override void MarkForClose()
        {

        }

        public override void UpdateAfterSimulation()
        {
        }

        public override void UpdateAfterSimulation10()
        {
        }

        public override void UpdateAfterSimulation100()
        {
        }

        public override void UpdateBeforeSimulation10()
        {
            if (myLcd == null) return;

            if (!NetWorkTransmitter.IsSinglePlayerOrServer() ?? true) return;

            //Only runs max 10 times a second
            //if ((DateTime.Now - lastUpdate) < TimeSpan.FromMilliseconds(200)) return;

            //Wenn nicht Defekt und Energie 
            if (myLcd.IsFunctional && myLcd.IsWorking && myLcd.Enabled && Station != null)
            {
                try
                {
                    if (DateTime.Now.Subtract(StationLastSaved).TotalSeconds > 60) /// Save Trade Station Object every 5 min or so
                    {
                        Save();
                    }
                    if ((DateTime.Now - DisplayUpdateTime) > TimeSpan.FromMilliseconds(1000))
                    {
                        /*
                        if (!string.IsNullOrWhiteSpace(myLcd.CustomName) && myLcd.CustomName.StartsWith("SETUP:")) // <---- eher für Reset geeignet!
                            Station.SetupStation(myLcd, true);//second param: color

                        if (!string.IsNullOrWhiteSpace(myLcd.GetPublicTitle()) && myLcd.GetPublicTitle().StartsWith("SETUP:"))
                            Station.SetupStation(myLcd, true);//second param: color
                        */
                        DisplayUpdateTime = DateTime.Now;

                        LCDOutput.FillSellBuyOnLcds(myLcd, Station, true);
                    }
                    //Production Update alle 1Mins ? Hier müssen wir wohl etwas experimentieren sobald alles drumherum funktioniert
                    int produpdatetime = 10; //[s]
                    if ((DateTime.Now - ProdCycleUpdateTime) > TimeSpan.FromSeconds(produpdatetime))
                    {
                        Station.HandleProdCycle(produpdatetime);
                        ProdCycleUpdateTime = DateTime.Now;
                    }
                    //MyAPIGateway.Utilities.ShowMessage("Last Prod", (DateTime.Now - ProdCycleUpdateTime).TotalSeconds.ToString());

                    IMyCubeGrid _grid = (IMyCubeGrid)myLcd.GetTopMostParent();
                    if (_grid != null)
                    {
                        List<IMySlimBlock> cargoblocks = new List<IMySlimBlock>();
                        _grid.GetBlocks(cargoblocks, e => e != null && e.FatBlock != null && e.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_CargoContainer));

                        Station.HandleCargos(cargoblocks);

                    }
                }
                catch (Exception ex)
                {
                    MyAPIGateway.Utilities.ShowMessage("Ex", ex.Message);
                    //MyAPIGateway.Utilities.ShowMissionScreen("Exception occurred!", null, null, ex.Message);
                }

            }
            else
            {
                if ((DateTime.Now - DisplayUpdateTime) > TimeSpan.FromSeconds(10)) //Soll Änderungen am Modus ermöglichen
                {
                    Load(); //Achtung wenn station schon belegt ist wird keine neue gewählt
                    DisplayUpdateTime = DateTime.Now;
                }
            }

            lastUpdate = DateTime.Now;
        }

        public override void UpdateBeforeSimulation()
        {
        }

        public override void UpdateBeforeSimulation100()
        {
        }

        public override void UpdateOnceBeforeFrame()
        {

            var server = NetWorkTransmitter.IsSinglePlayerOrServer();
            if (server.HasValue && server.Value)
            {
                //MyAPIGateway.Utilities.ShowMessage("TradeEngineers", "UpdateOnceBeforeFrame");
                Load();
                StationManager.Register(this);
                NeedsUpdate = MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
            else if (!server.HasValue)
            {
                NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }

        }

        // return the object defined in Init()
        public override VRage.ObjectBuilders.MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return _objectBuilder;
            //return copy ? (VRage.ObjectBuilders.MyObjectBuilder_EntityBase)_objectBuilder.Clone() : _objectBuilder;
        }
        
        private void Load()
        {
            if (myLcd == null)
                return;

            if ((myLcd as Sandbox.ModAPI.IMyTerminalBlock) == null)
            {
                MyAPIGateway.Utilities.ShowMessage("TradeEngineers", "Lcd is no Terminal Block, thus no custom data field");
                return;
            }


            var stationData = (myLcd as Sandbox.ModAPI.IMyTerminalBlock).CustomData;
            if (!string.IsNullOrWhiteSpace(stationData) && stationData.Trim().StartsWith("<?xml"))
            {
                var endStartTag = stationData.IndexOf("?>");
                try
                {
                    if (stationData.IndexOf(Definitions.DataFormat, endStartTag + 1, 200) == -1)
                    {
                        MyAPIGateway.Utilities.ShowMessage("TradeEngineers", "The persisted station definition was in an old format. (" + myLcd.CustomName + ") Station will be reset to defaults!");
                        myLcd.CustomData = string.Empty;
                        Station = null;
                        throw new InvalidOperationException("Old format");
                    }

                    //The SE XMLSerializer wont detect the subclass needed by parsing XML, thus we need to specify the type!                 
                    /*if (stationData.IndexOf("<StationBase", endStartTag + 1, 40) != -1)
                    {
                        Station = MyAPIGateway.Utilities.SerializeFromXML<StationBase>(stationData);
                        StationLastSaved = DateTime.Now;
                    }
                    else if (stationData.IndexOf("<RefineryStation", endStartTag + 1, 40) != -1)
                    {
                        Station = MyAPIGateway.Utilities.SerializeFromXML<RefineryStation>(stationData);
                        StationLastSaved = DateTime.Now;
                    }
                    else if (stationData.IndexOf("<FactoryStation", endStartTag + 1, 40) != -1)
                    {
                        Station = MyAPIGateway.Utilities.SerializeFromXML<FactoryStation>(stationData);
                        StationLastSaved = DateTime.Now;
                    }
                    else if (stationData.IndexOf("<ShipYardStation", endStartTag + 1, 40) != -1)
                    {
                        Station = MyAPIGateway.Utilities.SerializeFromXML<ShipYardStation>(stationData);
                        StationLastSaved = DateTime.Now;
                    }
                    else if (stationData.IndexOf("<SteelMillStation", endStartTag + 1, 40) != -1)
                    {
                        Station = MyAPIGateway.Utilities.SerializeFromXML<SteelMillStation>(stationData);
                        StationLastSaved = DateTime.Now;
                    }
                    else */
                    if (stationData.IndexOf("<TradeStation", endStartTag + 1, 40) != -1)
                    {
                        Station = MyAPIGateway.Utilities.SerializeFromXML<TradeStation>(stationData);
                        StationLastSaved = DateTime.Now;
                    }
                    /*
                    else if (stationData.IndexOf("<BankStation", endStartTag + 1, 40) != -1)
                    {
                        Station = MyAPIGateway.Utilities.SerializeFromXML<BankStation>(stationData);
                        StationLastSaved = DateTime.Now;
                    }*/

                }
                catch (InvalidOperationException e)
                {
                    var grid = (myLcd.GetTopMostParent() as IMyCubeGrid);
                    MyAPIGateway.Utilities.ShowMessage("TradeEngineers", "ERROR deserializing XML for '" + (grid.CustomName ?? grid.Name) + "': " + e.Message);
                }
            }


            if (Station == null)
            {
                try
                {
                    Station = StationBase.Factory(myLcd.CustomName ?? myLcd.CustomNameWithFaction, myLcd.OwnerId);
                    Save();
                    myLcd.WritePublicText(Station.StationTyp);
                }
                catch (ArgumentException)
                {
                    myLcd.WritePublicText("StationTypeError: Name the Block is not expected.\n (" + myLcd.CustomName + " / " + myLcd.CustomNameWithFaction + ")\n");
                    //myLcd.WritePublicText("Plz reload your game after config (will be fixed soon)\n", true);
                    //myLcd.WritePublicText("CustomTypes are planed for the future", true);
                }
            }

        }

        private void Save()
        {

            if (Station == null)
            {
                //MyAPIGateway.Utilities.ShowMessage("TradeEngineers", "ERROR: Station block has no initialized settings!");
                StationLastSaved = DateTime.MaxValue;
                return;
            }
            if (myLcd == null)
            {
                //MyAPIGateway.Utilities.ShowMessage("TradeEngineers", "ERROR: Lcd panel does not exist anymore!");
                StationLastSaved = DateTime.MaxValue;
                return;
            }


            try
            {
                (myLcd as Sandbox.ModAPI.IMyTerminalBlock).CustomData = MyAPIGateway.Utilities.SerializeToXML(Station);
                StationLastSaved = DateTime.Now;
                //MyAPIGateway.Utilities.ShowMessage("TE", "Saved station.");
            }
            catch (Exception e)
            {
                var grid = (myLcd.GetTopMostParent() as IMyCubeGrid);
                MyAPIGateway.Utilities.ShowMessage("TradeEngineers", "ERROR serializing XML for '" + (grid.CustomName ?? grid.Name) + "': " + e.Message);
            }

        }
        
        private void log(string text)
        {
            if (logger == null)
            {
                try
                {
                    logger = MyAPIGateway.Utilities.WriteFileInLocalStorage(this.GetType().Name + "-" + timeofload + ".log", this.GetType());
                }
                catch (Exception)
                {
                    MyAPIGateway.Utilities.ShowMessage("TradeEngineers IO", "Could not open the log file:" + this.GetType().Name + "-" + timeofload + ".log");
                    return;
                }
            }

            String datum = DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
            logger.WriteLine(datum + ": " + text);
            logger.Flush();

        }

    }
}

