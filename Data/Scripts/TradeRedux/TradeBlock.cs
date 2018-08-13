using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.ModAPI;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using TradeRedux.SerializedTradeStorage;
using TradeRedux.InputOutput;
using System.Text;
using TradeRedux.PluginApi;
using TradeRedux.SerializedTradeStorage.Stations;

namespace TradeRedux
{
    [MyEntityComponentDescriptor(
        typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_TextPanel),
        false,
        new string[]
        {
            "TradeRedux_SmallLCDPanelWide",
            "TradeRedux_LargeLCDPanelWide"
        }
    )]
    public class TradeBlock : MyGameLogicComponent
    {
        private VRage.ObjectBuilders.MyObjectBuilder_EntityBase _objectBuilder;
        private DateTime DisplayUpdateTime = DateTime.MinValue;
        private DateTime ProdctionCycleLastUpdate = DateTime.MinValue;

        private readonly String timeOfLoad = "" + DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + " " + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second;
        public Sandbox.ModAPI.IMyTextPanel LcdPanel;

        private DateTime StationLastSaved = DateTime.MinValue;
        public StationBase Station = null;

        public override void Close()
        {
            if (Entity != null && LcdPanel != null && LcdPanel.IsFunctional && LcdPanel.IsWorking && LcdPanel.Enabled)
                Save(Station);

            LcdPanel = null;
            Logger.Close();
        }

        public override void Init(VRage.ObjectBuilders.MyObjectBuilder_EntityBase objectBuilder)
        {
            _objectBuilder = objectBuilder;

            LcdPanel = (Entity as Sandbox.ModAPI.IMyTextPanel);
            if (LcdPanel != null)
            {
                Log("Welcome to Trade Engeneers Redux");
                Log("Rename this Block to wanted Station Type. Eample: 'TradeStation'");

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
            if (LcdPanel == null) return;

            LcdPanel.ShowPublicTextOnScreen();

            if (!NetWorkTransmitter.IsSinglePlayerOrServer() ?? true) return;

            if (Station == null)
            {
                Station = Load();
                if (Station != null) StationManager.Register(this);
            }

            //Wenn nicht Defekt und Energie 
            if (LcdPanel.IsFunctional && LcdPanel.IsWorking && LcdPanel.Enabled && Station != null)
            {
                try
                {
                    if (DateTime.Now.Subtract(StationLastSaved).TotalSeconds > 60) /// Save Trade Station Object every 5 min or so
                    {
                        Save(Station);
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

                        LCDOutput.FillSellBuyOnLcds(LcdPanel, Station, true);
                    }
                    
                    //Production Update alle 1Mins ? Hier müssen wir wohl etwas experimentieren sobald alles drumherum funktioniert
                    int produpdatetime = 10; //[s]
                    if ((DateTime.Now - ProdctionCycleLastUpdate) > TimeSpan.FromSeconds(produpdatetime))
                    {
                        Station.HandleProdCycle(produpdatetime);
                        ProdctionCycleLastUpdate = DateTime.Now;
                    }
                    //MyAPIGateway.Utilities.ShowMessage("Last Prod", (DateTime.Now - ProdCycleUpdateTime).TotalSeconds.ToString());
                    IMyCubeGrid _grid = (IMyCubeGrid)LcdPanel.GetTopMostParent();
                    if (_grid != null)
                    {
                        List<IMySlimBlock> cargoblocks = new List<IMySlimBlock>();
                        _grid.GetBlocks(cargoblocks, e => e != null && e.FatBlock != null && e.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_CargoContainer));

                        Station.HandleCargos(cargoblocks);

                    }
                }
                catch (Exception ex)
                {
                    Log("Update Error:" + ex.Message);
                }

            }
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

        private StationBase Load()
        {
            if (LcdPanel == null)
                return null;

            if ((LcdPanel as Sandbox.ModAPI.IMyTerminalBlock) == null)
            {
                Log("Lcd is no Terminal Block, thus no custom data field");
                return null;
            }



            string stationData = (LcdPanel as Sandbox.ModAPI.IMyTerminalBlock).CustomData;

            if (!string.IsNullOrWhiteSpace(stationData) && stationData.Trim().StartsWith("<?xml"))
            {
                var tagEndOffset = stationData.IndexOf("?>");
                try
                {
                    if (stationData.IndexOf(Definitions.DataFormat, tagEndOffset + 1, 400) == -1)
                    {
                        Log("The persisted station definition was in an old format. (" + LcdPanel.CustomName + ") Station will be reset to defaults!");
                        LcdPanel.CustomData = string.Empty;
                        throw new InvalidOperationException("Old format");
                    }

                    //The SE XMLSerializer wont detect the subclass needed by parsing XML, thus we need to specify the type!                 
                    if (stationData.IndexOf("<TradeStation", tagEndOffset + 1, 40) != -1)
                    {
                        return MyAPIGateway.Utilities.SerializeFromXML<TradeStation>(stationData);
                    }

                }
                catch (InvalidOperationException e)
                {
                    Log("ERROR deserializing: " + e.Message);
                }
            }

            try
            {
                return StationBase.Factory(LcdPanel.CustomName ?? LcdPanel.CustomNameWithFaction, LcdPanel.OwnerId);
            }
            catch (ArgumentException)
            {
                //Log("StationTypeError: Name the Block is not expected: " + LcdPanel.CustomName + " / " + LcdPanel.CustomNameWithFaction);
                // just wait for correct name
            }

            return null;
        }

        private void Save(StationBase station)
        {
            StationLastSaved = DateTime.Now;

            if (station == null)
            {
                return;
            }
            if (LcdPanel == null)
            {
                return;
            }

            try
            {
                StationBase oldStationData = Load();
                Station.TakeSettingData(oldStationData);

                (LcdPanel as Sandbox.ModAPI.IMyTerminalBlock).CustomData = MyAPIGateway.Utilities.SerializeToXML(Station);
                Log("Station saved.");
            }
            catch (Exception e)
            {
                var grid = (LcdPanel.GetTopMostParent() as IMyCubeGrid);
                Log("ERROR serializing XML for '" + (grid.CustomName ?? grid.Name) + "': " + e.Message);
            }
        }

        public void Log(string text)
        {
            string line = Logger.Log(text);
            if (LcdPanel != null)
            {
                List<string> output = new List<string>();
                output.AddArray(LcdPanel.GetPublicText().Split('\n'));
                if (output.Count > 18)
                {
                    while (output.Count > 17)
                    {
                        output.RemoveAt(0);
                    }
                    LcdPanel.WritePublicText(string.Join("\n", output.ToArray()));
                }
                LcdPanel.WritePublicText(line + "\n", true);
            }
        }

    }
}
