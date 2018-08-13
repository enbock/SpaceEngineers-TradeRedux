using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Sandbox.ModAPI.Ingame;
using VRageMath;
using VRage.ModAPI;
using System.Reflection;
using VRage;
using VRage.Game.ModAPI;
using TradeRedux.Inventory;
using TradeRedux.TradeGoods;
using TradeRedux.SerializedTradeStorage;
using Sandbox.ModAPI;
using TradeRedux.SerializedTradeStorage.Stations;

namespace TradeRedux.InputOutput
{
    class LCDOutput
    {
        public static void FillSellBuyOnLcds(IMyEntity entity, StationBase Station, bool connectedgrids = false)
        {
            //todo: erst String zusammenbauen dann senden
            IMyCubeGrid Base = (entity.GetTopMostParent() as IMyCubeGrid);
            
            List<IMySlimBlock> txtpanels = new List<IMySlimBlock>();
            Base.GetBlocks(txtpanels, e => e != null && e.FatBlock != null && e.FatBlock.BlockDefinition.TypeId == typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_TextPanel));
                        
            if (connectedgrids)
            {
                var connectedshipslist = StationBase.GetConnectedShips(entity);

                foreach(var connector in connectedshipslist.Keys)
                {
                    //MyAPIGateway.Utilities.ShowMessage("1", "" + connectedshipslist.Count);
                    var grid = connectedshipslist[connector];
                    if (grid != null)
                    {
                        List<IMySlimBlock> addpanels = new List<IMySlimBlock>();
                        grid.GetBlocks(addpanels, e => e != null && e.FatBlock != null && e.FatBlock.BlockDefinition.TypeId == typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_TextPanel));
                        txtpanels.AddRange(addpanels);
                    }
                }
            }
            
            //
            if (txtpanels.Count > 0)
            {
                var buystringbuild = new StringBuilder();
                var sellstringbuild = new StringBuilder();
                buystringbuild.AppendLine("StationName:" + Base.CustomName);
                buystringbuild.AppendLine("StationType:" + Station.Type);
                buystringbuild.AppendLine("LastUpdate: " + DateTime.Now.ToString("HH.mm"));
                buystringbuild.AppendLine("Buying: (actual cargo)");

                sellstringbuild.AppendLine("StationName:" + Base.CustomName);
                sellstringbuild.AppendLine("StationType:" + Station.Type);
                sellstringbuild.AppendLine("LastUpdate: " + DateTime.Now.ToString("HH.mm"));
                sellstringbuild.AppendLine("Selling: (actual cargo)");
                var goodsselling = Station.Goods.Where(g => g.IsSell);
                foreach (var tradeitem in goodsselling)
                {
                    sellstringbuild.AppendLine(tradeitem.ToString() + ": " 
                         + (tradeitem.PriceModel.GerSellPrice(tradeitem.CargoRatio)).ToString("0.00##") + "$ ("
                         + (tradeitem.CargoRatio * 100).ToString("0.#") + "% = " 
                         + GetStringFromDouble(tradeitem.CurrentCargo) + ")" //+ tradeitem.MaxCargo.ToString("0.####") +                          
                         + (tradeitem.PriceModel.IsProducent ? " [P]" : "")); //P                                
                }
                
                var goodsbuing = Station.Goods.Where(g => g.IsBuy); 
                foreach (var tradeitem in goodsbuing)
                {
                    buystringbuild.AppendLine(tradeitem.ToString() + ": " 
                        + (tradeitem.PriceModel.GetBuyPrice(tradeitem.CargoRatio)).ToString("0.00##") + "$ ("
                        + (tradeitem.CargoRatio * 100).ToString("0.#") + "% = "
                        + GetStringFromDouble(tradeitem.CurrentCargo) + ")"); //tradeitem.MaxCargo.ToString("0.####") + ")"
                }
                

                foreach (var lcd in txtpanels)
                {
                    var myLcd = (lcd.FatBlock as IMyTextPanel);
                    string lcdtextinfo = "";
                    // myLcd.CustomData //Hier könnten zusätzliche Infos konfiguriert werden
                    var title = myLcd.GetPublicTitle().ToLower();
                    if (title.Contains("teinfo"))
                    {
                        if (title.Contains("buy"))
                        {
                            lcdtextinfo = buystringbuild.ToString();
                        }
                        else if (title.Contains("sell"))
                        {
                            lcdtextinfo = sellstringbuild.ToString();
                        }
                        else //Allgemeine Infos der station anzeigen lassen
                        {
                            var builder = new StringBuilder();
                            builder.AppendLine("StationName:" + Base.CustomName);
                            builder.AppendLine("StationType:" + Station.Type);
                            lcdtextinfo = builder.ToString();                            
                        }

                        if (lcdtextinfo != myLcd.GetPublicText())
                        {                            
                            myLcd.WritePublicText(lcdtextinfo);
                        }
                    }
                }
            }
        }
        public static string GetStringFromDouble(double value)
        {
            string rtn = "";

            if (value >= 1E9)
                rtn = (value / 1E9).ToString("0") + "G";
            if (value >= 1E6)
                rtn = (value / 1E6).ToString("0") + "M";
            else if (value >= 1E4) //Erst ab 10 000  wird k angezeigt
                rtn = (value / 1E3).ToString("0") + "k";
            else
                rtn = value.ToString("0");
            return rtn;
        }
    }
}
