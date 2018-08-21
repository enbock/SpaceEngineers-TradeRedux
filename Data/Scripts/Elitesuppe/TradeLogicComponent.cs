using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Elitesuppe.Trade;
using EliteSuppe.Trade.Stations;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace Elitesuppe
{
    [MyEntityComponentDescriptor(
        typeof(MyObjectBuilder_TextPanel),
        false,
        new string[]
        {
            "Elitesuppe_TradeRedux_TradeStation",
            "Elitesuppe_TradeRedux_MiningStation",
            "Elitesuppe_TradeRedux_IronForge"
        }
    )]
    public class TradeLogicComponent : MyGameLogicComponent
    {
        private MyObjectBuilder_EntityBase _objectBuilder;
        private DateTime _lastProductionUpdate = DateTime.MinValue;

        public IMyTextPanel LcdPanel;

        private DateTime _lastSaved = DateTime.MinValue;
        public StationBase Station;

        public override void Close()
        {
            if (Entity != null && LcdPanel != null && LcdPanel.IsFunctional && LcdPanel.IsWorking && LcdPanel.Enabled)
                Save(Station);

            LcdPanel = null;
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

            // if no energy do nothing
            if (!LcdPanel.IsFunctional || !LcdPanel.IsWorking || !LcdPanel.Enabled || Station == null) return;

            const double refreshTime = 1f;
            try
            {
                if (DateTime.Now.Subtract(_lastSaved).TotalSeconds > 10f) Save(Station);

                if (DateTime.Now.Subtract(_lastProductionUpdate).TotalSeconds < refreshTime) return;

                Station.HandleProdCycle();
                _lastProductionUpdate = DateTime.Now;
                LcdOutput.UpdateLcdOutput(LcdPanel, Station);

                IMyCubeGrid grid = (IMyCubeGrid) LcdPanel.GetTopMostParent();
                if (grid == null) return;
                List<IMySlimBlock> cargoBlockList = new List<IMySlimBlock>();
                grid.GetBlocks(
                    cargoBlockList,
                    slimBlock => slimBlock?.FatBlock != null &&
                                 slimBlock.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_CargoContainer) &&
                                 LcdPanel.CubeGrid.EntityId ==
                                 slimBlock.FatBlock.CubeGrid.EntityId // only station container
                );

                Station.HandleCargo(cargoBlockList);
            }
            catch (Exception ex)
            {
                MyAPIGateway.Utilities.ShowNotification($"Error: {ex.Message}\n{ex.StackTrace}");
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
                        Log(
                            "The persisted station definition was in an old format. (" +
                            LcdPanel.CustomName +
                            ") Station will be reset to defaults!"
                        );
                        LcdPanel.CustomData = string.Empty;
                        throw new InvalidOperationException("Old format");
                    }

                    //The SE XMLSerializer wont detect the subclass needed by parsing XML, thus we need to specify the type!                 
                    if (stationData.IndexOf("<TradeStation", tagEndOffset + 1, 40, StringComparison.Ordinal) != -1)
                    {
                        return MyAPIGateway.Utilities.SerializeFromXML<TradeStation>(stationData);
                    }

                    if (stationData.IndexOf("<IronForge", tagEndOffset + 1, 40, StringComparison.Ordinal) != -1)
                    {
                        return MyAPIGateway.Utilities.SerializeFromXML<IronForge>(stationData);
                    }

                    if (stationData.IndexOf("<MiningStation", tagEndOffset + 1, 40, StringComparison.Ordinal) != -1)
                    {
                        return MyAPIGateway.Utilities.SerializeFromXML<MiningStation>(stationData);
                    }
                }
                catch (InvalidOperationException e)
                {
                    Log("ERROR deserializing: " + e.Message);
                }
            }

            try
            {
                StationBase station = StationBase.Factory(LcdPanel.BlockDefinition.SubtypeId); //, LcdPanel.OwnerId);
                //Log("Found:" + station.Type);
                return station;
            }
            catch (ArgumentException)
            {
                //Log("StationTypeError: Name the Block is not expected: " + LcdPanel.BlockDefinition.SubtypeId);
                // just wait for correct name
            }

            return null;
        }

        private void Save(StationBase station)
        {
            _lastSaved = DateTime.Now;

            if (station == null) return;
            if (LcdPanel == null) return;

            try
            {
                StationBase oldStationData = Load();
                Station.TakeSettingData(oldStationData);

                LcdPanel.CustomData = MyAPIGateway.Utilities.SerializeToXML(Station);
                Log("Station saved.");
            }
            catch (Exception exception)
            {
                MyAPIGateway.Utilities.ShowNotification("Error: Saving data: " + exception.Message, 10000, "Red");
            }
        }

        private void Log(string text)
        {
            return;
            string line = text; //Logger.Log(text);
            MyAPIGateway.Utilities.ShowMessage("TR-Log", line);

            if (LcdPanel == null) return;

            List<string> output = new List<string>();
            output.AddArray(LcdPanel.GetPublicText().Split('\n'));
            if (output.Count > 17)
            {
                while (output.Count > 16)
                {
                    output.RemoveAt(0);
                }

                LcdPanel.WritePublicText(string.Join("\n", output.ToArray()));
            }

            LcdPanel.WritePublicText(line + "\n", true);
        }
    }
}