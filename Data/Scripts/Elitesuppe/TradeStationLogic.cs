using System;
using System.Collections.Generic;
using Elitesuppe.Trade;
using Elitesuppe.Trade.Serialized.Stations;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace Elitesuppe
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel),false, "ElitesuppeTradeRedux_LargeLCDPanelWide")]
    public class TradeStationLogic : MyGameLogicComponent
    {
        private MyObjectBuilder_EntityBase _objectBuilder;
        private DateTime _displayUpdateTime = DateTime.MinValue;
        private DateTime _lastProductionUpdate = DateTime.MinValue;

        public IMyTextPanel LcdPanel;

        private DateTime _lastSaved = DateTime.MinValue;
        public StationBase Station;

        public override void Close()
        {
            if (Entity != null && LcdPanel != null && LcdPanel.IsFunctional && LcdPanel.IsWorking && LcdPanel.Enabled)
                Save(Station);

            LcdPanel = null;
            // Logger.Close();
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            _objectBuilder = objectBuilder;

            LcdPanel = (Entity as IMyTextPanel);
            if (LcdPanel != null)
            {
                //Log("Welcome to Trade Engineers Redux");
                //Log("Rename this Block to wanted Station Type. Example: 'TradeStation'");

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

            if (!NetworkTransmitter.IsSinglePlayerOrServer() ?? true) return;

            if (Station == null)
            {
                Station = Load();
                if (Station != null) StationManager.Register(this);
            }

            //Wenn nicht Defekt und Energie 
            if (!LcdPanel.IsFunctional || !LcdPanel.IsWorking || !LcdPanel.Enabled || Station == null) return;
            const int productionTime = 10;
            try
            {
                if (DateTime.Now.Subtract(_lastSaved).TotalSeconds > 60) // Save Trade Station Object every 5 min or so
                {
                    Save(Station);
                }
                if ((DateTime.Now - _displayUpdateTime) > TimeSpan.FromMilliseconds(1000))
                {
                    /*
                        if (!string.IsNullOrWhiteSpace(myLcd.CustomName) && myLcd.CustomName.StartsWith("SETUP:"))
                            Station.SetupStation(myLcd, true);//second param: color

                        if (!string.IsNullOrWhiteSpace(myLcd.GetPublicTitle()) && myLcd.GetPublicTitle().StartsWith("SETUP:"))
                            Station.SetupStation(myLcd, true);//second param: color
                        */
                    _displayUpdateTime = DateTime.Now;

                    LCDOutput.FillSellBuyOnLcds(LcdPanel, Station, true);
                }
                    
                //Production Update alle 1Mins
                if ((DateTime.Now - _lastProductionUpdate) > TimeSpan.FromSeconds(productionTime))
                {
                    Station.HandleProdCycle();
                    _lastProductionUpdate = DateTime.Now;
                }
                //MyAPIGateway.Utilities.ShowMessage("Last Prod", (DateTime.Now - ProdCycleUpdateTime).TotalSeconds.ToString());
                IMyCubeGrid grid = (IMyCubeGrid)LcdPanel.GetTopMostParent();
                if (grid == null) return;
                List<IMySlimBlock> cargoBlockList = new List<IMySlimBlock>();
                grid.GetBlocks(cargoBlockList, e => e != null && e.FatBlock != null && e.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_CargoContainer));

                Station.HandleCargo(cargoBlockList);
            }
            catch (Exception ex)
            {
                Log("Update Error:" + ex.Message);
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
            var server = NetworkTransmitter.IsSinglePlayerOrServer();
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
        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return _objectBuilder;
            //return copy ? (VRage.ObjectBuilders.MyObjectBuilder_EntityBase)_objectBuilder.Clone() : _objectBuilder;
        }

        private StationBase Load()
        {
            if (LcdPanel == null)
                return null;

            string stationData = LcdPanel.CustomData;

            if (!string.IsNullOrWhiteSpace(stationData) && stationData.Trim().StartsWith("<?xml"))
            {
                var tagEndOffset = stationData.IndexOf("?>", StringComparison.Ordinal);
                try
                {
                    if (stationData.IndexOf(Definitions.Version, tagEndOffset + 1, 400, StringComparison.Ordinal) == -1)
                    {
                        Log("The persisted station definition was in an old format. (" + LcdPanel.CustomName + ") Station will be reset to defaults!");
                        LcdPanel.CustomData = string.Empty;
                        throw new InvalidOperationException("Old format");
                    }

                    //The SE XMLSerializer wont detect the subclass needed by parsing XML, thus we need to specify the type!                 
                    if (stationData.IndexOf("<TradeStation", tagEndOffset + 1, 40, StringComparison.Ordinal) != -1)
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
            _lastSaved = DateTime.Now;

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

                LcdPanel.CustomData = MyAPIGateway.Utilities.SerializeToXML(Station);
                Log("Station saved.");
            }
            catch (Exception)
            {
                //var grid = (LcdPanel.GetTopMostParent() as IMyCubeGrid);
                //Log("ERROR serializing XML for '" + (grid.CustomName ?? grid.Name) + "': " + e.Message);
            }
        }

        private void Log(string text)
        {
            string line = text; //Logger.Log(text);
            
            if (LcdPanel == null) return;
            
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

