using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.ModAPI;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;
using Elitesuppe.Trade.Serialized.Stations;

namespace Elitesuppe.Trade
{
    static class LcdOutput
    {
        public static void FillSellBuyOnLcds(IMyEntity entity, StationBase Station, bool connectedgrids = false)
        {
            //todo: erst String zusammenbauen dann senden
            IMyCubeGrid Base = (entity.GetTopMostParent() as IMyCubeGrid);

            List<IMySlimBlock> textPanels = new List<IMySlimBlock>();
            if (Base == null) return;
            Base.GetBlocks(
                textPanels,
                e => e?.FatBlock != null &&
                     e.FatBlock.BlockDefinition.TypeId ==
                     typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_TextPanel)
            );

            if (connectedgrids)
            {
                var connectedShips = StationBase.GetConnectedShips(entity);

                foreach (var connector in connectedShips.Keys)
                {
                    //MyAPIGateway.Utilities.ShowMessage("1", "" + connectedshipslist.Count);
                    var grid = connectedShips[connector];
                    if (grid == null) continue;
                    List<IMySlimBlock> panels = new List<IMySlimBlock>();
                    grid.GetBlocks(
                        panels,
                        e => e?.FatBlock != null &&
                             e.FatBlock.BlockDefinition.TypeId ==
                             typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_TextPanel)
                    );
                    textPanels.AddRange(panels);
                }
            }

            //
            if (textPanels.Count <= 0) return;
            var buyBuilder = new StringBuilder();
            var sellBuilder = new StringBuilder();
            buyBuilder.AppendLine("StationName:" + Base.CustomName);
            buyBuilder.AppendLine("StationType:" + Station.Type);
            buyBuilder.AppendLine("LastUpdate: " + DateTime.Now.ToString("HH.mm"));
            buyBuilder.AppendLine("Buying: (actual cargo)");

            sellBuilder.AppendLine("StationName:" + Base.CustomName);
            sellBuilder.AppendLine("StationType:" + Station.Type);
            sellBuilder.AppendLine("LastUpdate: " + DateTime.Now.ToString("HH.mm"));
            sellBuilder.AppendLine("Selling: (actual cargo)");
            var goodsSelling = Station.Goods.Where(g => g.IsSell);
            foreach (var tradeItem in goodsSelling)
            {
                sellBuilder.AppendLine(
                    tradeItem +
                    ": " +
                    (tradeItem.PriceModel.GetSellPrice(tradeItem.CargoRatio)).ToString("0.00##") +
                    "$ (" +
                    (tradeItem.CargoRatio * 100).ToString("0.#") +
                    "% = " +
                    GetStringFromDouble(tradeItem.CurrentCargo) +
                    ")" +
                    (tradeItem.PriceModel.IsProducent ? " [P]" : "")
                );
            }

            var goodsBuying = Station.Goods.Where(g => g.IsBuy);
            foreach (var tradeItem in goodsBuying)
            {
                buyBuilder.AppendLine(
                    tradeItem +
                    ": " +
                    (tradeItem.PriceModel.GetBuyPrice(tradeItem.CargoRatio)).ToString("0.00##") +
                    "$ (" +
                    (tradeItem.CargoRatio * 100).ToString("0.#") +
                    "% = " +
                    GetStringFromDouble(tradeItem.CurrentCargo) +
                    ")"
                );
            }


            foreach (var lcd in textPanels)
            {
                var myLcd = (lcd.FatBlock as IMyTextPanel);
                // myLcd.CustomData //Hier könnten zusätzliche Infos konfiguriert werden
                if (myLcd == null) continue;
                var title = myLcd.GetPublicTitle().ToLower();
                if (!title.Contains("teinfo")) continue;
                var lcdTextInfo = "";
                if (title.Contains("buy"))
                {
                    lcdTextInfo = buyBuilder.ToString();
                }
                else if (title.Contains("sell"))
                {
                    lcdTextInfo = sellBuilder.ToString();
                }
                else //Allgemeine Infos der station anzeigen lassen
                {
                    var builder = new StringBuilder();
                    builder.AppendLine("StationName:" + Base.CustomName);
                    builder.AppendLine("StationType:" + Station.Type);
                    lcdTextInfo = builder.ToString();
                }

                if (lcdTextInfo != myLcd.GetPublicText())
                {
                    myLcd.WritePublicText(lcdTextInfo);
                }
            }
        }

        private static string GetStringFromDouble(double value)
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