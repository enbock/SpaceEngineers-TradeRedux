using System.Collections.Generic;
using System.Text;
using EliteSuppe.Trade.Stations;
using EliteSuppe.Trade.Stations.Output;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Elitesuppe.Trade
{
    public static class LcdOutput
    {
        public static void UpdateLcdOutput(IMyEntity entity, StationBase station, bool searchOnConnectedGrids = false)
        {
            IMyCubeGrid cubeGrid = entity.GetTopMostParent() as IMyCubeGrid;

            List<IMySlimBlock> textPanels = new List<IMySlimBlock>();
            if (cubeGrid == null) return;
            cubeGrid.GetBlocks(
                textPanels,
                e => e?.FatBlock != null && e.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_TextPanel)
            );

            if (searchOnConnectedGrids)
            {
                var connectedShips = GridApi.GetConnectedShips(entity);

                foreach (var connector in connectedShips.Keys)
                {
                    var grid = connectedShips[connector];
                    if (grid == null) continue;
                    List<IMySlimBlock> panels = new List<IMySlimBlock>();
                    grid.GetBlocks(
                        panels,
                        e => e?.FatBlock != null &&
                             e.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_TextPanel)
                    );
                    textPanels.AddRange(panels);
                }
            }
            if (textPanels.Count <= 0) return;

            Dictionary<string, StringBuilder> output = new Dictionary<string, StringBuilder>();
            IOutputRepresentor outputRepresentor = StationOutputFactory.CreateRepresentor(station);
            outputRepresentor.CreateOutput(output, cubeGrid);

            /*
            var buyBuilder = new StringBuilder();
            var sellBuilder = new StringBuilder();
            buyBuilder.AppendLine("StationName:" + Base.CustomName);
            buyBuilder.AppendLine("");
            //buyBuilder.AppendLine("StationType:" + Station.Type);
            buyBuilder.AppendLine("LastUpdate: " + DateTime.Now.ToString("HH.mm"));
            buyBuilder.AppendLine("Buying: (actual cargo)");

            sellBuilder.AppendLine("StationName:" + Base.CustomName);
            sellBuilder.AppendLine("");
            //sellBuilder.AppendLine("StationType:" + Station.Type);
            sellBuilder.AppendLine("LastUpdate: " + DateTime.Now.ToString("HH.mm"));
            sellBuilder.AppendLine("Selling: (actual cargo)");

            if (station is TradeStation)
            {
                TradeStation tradeStation = (TradeStation) station;
                var goodsSelling = tradeStation.Goods.Where(g => g.IsSell);
                foreach (var tradeItem in goodsSelling)
                {
                    sellBuilder.AppendLine(
                        tradeItem +
                        ": " +
                        (tradeItem.Price.GetSellPrice(tradeItem.CargoRatio)).ToString("0.00##") +
                        "$ (" +
                        (tradeItem.CargoRatio * 100).ToString("0.#") +
                        "% = " +
                        GetStringFromDouble(tradeItem.CurrentCargo) +
                        ")"
                    );
                }

                var goodsBuying = tradeStation.Goods.Where(g => g.IsBuy);
                foreach (var tradeItem in goodsBuying)
                {
                    buyBuilder.AppendLine(
                        tradeItem +
                        ": " +
                        (tradeItem.Price.GetBuyPrice(tradeItem.CargoRatio)).ToString("0.00##") +
                        "$ (" +
                        (tradeItem.CargoRatio * 100).ToString("0.#") +
                        "% = " +
                        GetStringFromDouble(tradeItem.CurrentCargo) +
                        ")"
                    );
                }
            }*/
            
            foreach (var lcd in textPanels)
            {
                var myLcd = lcd.FatBlock as IMyTextPanel;

                if (myLcd == null) continue;
                var title = myLcd.GetPublicTitle().ToLower();

                if (!title.Contains("info")) continue;

                foreach (KeyValuePair<string, StringBuilder> pair in output)
                {
                    string lcdTextInfo = "";

                    if (title.Contains(pair.Key.ToLower()))
                    {
                        lcdTextInfo = pair.Value.ToString();
                    }

                    if (lcdTextInfo == myLcd.GetPublicText()) continue;
                    
                    myLcd.WritePublicText(lcdTextInfo);
                    myLcd.ShowPublicTextOnScreen();
                }
            }
        }

        public static string GetStringFromDouble(double value)
        {
            string rtn = "";

            if (value >= 1E9)
                rtn = (value / 1E9).ToString("0") + "G";
            else if (value >= 1E6)
                rtn = (value / 1E6).ToString("0") + "M";
            else if (value >= 1E4) //Erst ab 10 000  wird k angezeigt
                rtn = (value / 1E3).ToString("0") + "k";
            else
                rtn = value.ToString("0");
            return rtn;
        }
    }
}