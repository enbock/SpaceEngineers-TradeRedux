using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using TradeEngineers.Inventory;

namespace TradeEngineers.TradeGoods
{
    public class PriceFinder
    {
        //TODO: Fill this Dictionary with a table of real prices
        private static Dictionary<MyDefinitionId, PriceModel> _prices = new Dictionary<MyDefinitionId, PriceModel>
            {
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Iron"), new PriceModel(0.001/2,false)}, //Erze werden abgebaut und nicht selbständig hergestellt!
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Nickel"), new PriceModel(0.002/2,false)}, //Dennoch Prod auf true setzten um damit Herstellungspreis = Verkauspreis
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Uranium"), new PriceModel(0.1/2,false)},
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Platinum"), new PriceModel(0.15/2,false)},
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Magnesium"), new PriceModel(0.1/2,false)},
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Silicon"), new PriceModel(0.01/2,false)},
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Cobalt"), new PriceModel(0.003/2,false)},
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Silver"), new PriceModel(0.005/2,false)},
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Gold"), new PriceModel(0.05/2,false)},                
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Ice"), new PriceModel(0.00001/2,false)},
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Scrap"), new PriceModel(0.001/2,false)}, //todo! Herausfinden wieviel es wert ist
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Organic"), new PriceModel(0.00001/2,false)}, //todo! Herausfinden wieviel es wert ist
                {new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Stone"), new PriceModel(0.00001/2,false)},
                {new MyDefinitionId(typeof(MyObjectBuilder_Ingot), "Stone"), new PriceModel(0.00001/2,false)}
                //{new MyDefinitionId(typeof(MyObjectBuilder_Component), "Limiterium"), new PriceModel(1,true)}          
            };        
        
        /// <summary>
        /// Vervollständigt die Liste und gibt eine Kopie zurück
        /// </summary>
        /// <param name="prodlist"></param>
        /// <returns></returns>
        public static Dictionary<MyDefinitionId, PriceModel> BuildPriceModelList(List<MyDefinitionId> prodlist)
        {
            Dictionary<MyDefinitionId, PriceModel> prices = new Dictionary<MyDefinitionId, PriceModel>(_prices);
            //ItemDefinitionFactory.Ores; //Predef

            var OreList = ItemDefinitionFactory.Ores;
            foreach (var itemid in OreList)
            {
                var prod = false;
                if (prices.ContainsKey(itemid)) continue;
                //if (prodlist.Contains(itemid)) prod = true; 
                prices.Add(itemid, new PriceModel(0.005 / 2, prod)); //{ new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Silver"), new PriceModel(0.005 / 2, false)},
            }

            var IngotList = ItemDefinitionFactory.Ingots;
            foreach(var itemid in IngotList)
            {
                var prod = false;
                if (prices.ContainsKey(itemid)) continue;
                if (prodlist.Contains(itemid)) prod = true;
                
                double actprice = 0;
                var useditemsdict = ItemDefinitionFactory.GetRecipeInput(itemid);
                foreach(var key in useditemsdict.Keys)
                {
                    if (!prices.ContainsKey(key)) continue; //throw new System.Exception(itemid.ToString() +" \n" + key.ToString());
                    var model = prices[key];
                    var amount = useditemsdict[key];

                    actprice += model.Price * amount;
                }
                prices.Add(itemid, new PriceModel(actprice, prod));
            }
            
            var ComponentsList = ItemDefinitionFactory.Components;
            foreach (var itemid in ComponentsList)
            {
                var prod = false;
                if (prices.ContainsKey(itemid)) continue;
                if (prodlist.Contains(itemid)) prod = true;

                double actprice = 0;
                var useditemsdict = ItemDefinitionFactory.GetRecipeInput(itemid);
                foreach (var key in useditemsdict.Keys)
                {
                    if (!prices.ContainsKey(key)) continue; //throw new System.Exception(itemid.ToString() +" \n" + key.ToString());
                    var model = prices[key];
                    var amount = useditemsdict[key];

                    actprice += model.Price * amount;
                }
                prices.Add(itemid, new PriceModel(actprice, prod));
            }
            var Toollist = ItemDefinitionFactory.PlayerTools;
            foreach (var itemid in Toollist)
            {
                var prod = false;
                if (prices.ContainsKey(itemid)) continue;
                if (prodlist.Contains(itemid)) prod = true;

                double actprice = 0;
                var useditemsdict = ItemDefinitionFactory.GetRecipeInput(itemid);
                foreach (var key in useditemsdict.Keys)
                {
                    if (!prices.ContainsKey(key)) continue; //throw new System.Exception(itemid.ToString() +" \n" + key.ToString());
                    var model = prices[key];
                    var amount = useditemsdict[key];

                    actprice += model.Price * amount;
                }
                prices.Add(itemid, new PriceModel(actprice, prod));
            }

            var Ammolist = ItemDefinitionFactory.Ammunitions;
            foreach (var itemid in Ammolist)
            {
                var prod = false;
                if (prices.ContainsKey(itemid)) continue;
                if (prodlist.Contains(itemid)) prod = true;

                double actprice = 0;
                var useditemsdict = ItemDefinitionFactory.GetRecipeInput(itemid);
                foreach (var key in useditemsdict.Keys)
                {
                    if (!prices.ContainsKey(key)) continue; //throw new System.Exception(itemid.ToString() +" \n" + key.ToString());
                    var model = prices[key];
                    var amount = useditemsdict[key];

                    actprice += model.Price * amount;
                }
                prices.Add(itemid, new PriceModel(actprice, prod));
            }

            return prices;
        }       

        public static PriceModel GetPrice(MyDefinitionId item)
        {            
            if (_prices.ContainsKey(item))
            {
                return _prices.FirstOrDefault(kvp => kvp.Key.Equals(item)).Value;
            }

            return new PriceModel(1);            
        }
        
    }
}
