using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Elitesuppe.Trade;
using EliteSuppe.Trade.Items;
using VRage.Game.ModAPI;

namespace EliteSuppe.Trade.Stations
{
    [Serializable]
    [XmlRoot(Namespace = Definitions.Version)]
    public class FactoryStation : StationBase
    {
        public List<Recipe> Recipes = new List<Recipe>();
        public Item Credits = new Item(Definitions.Credits, new Price(1f, 1f, 1f), true, true, 0f, 0f);
        
        private Dictionary<string, Item> _bufferStock = new Dictionary<string, Item>();
        private Dictionary<string, Item> _resourceStock = new Dictionary<string, Item>();
        private Dictionary<string, Item> _productStock = new Dictionary<string, Item>();

        public override List<Item> Goods
        {
            get
            {
                List<Item> all = new List<Item>();
                all.AddRange(_bufferStock.Values);
                all.AddRange(_resourceStock.Values);
                all.AddRange(_productStock.Values);

                return all;
            }
        }

        public FactoryStation()
        {
        }

        protected FactoryStation(long ownerId, string type) : base(ownerId, type)
        {
        }

        public override void HandleProdCycle()
        {
            // NotImplemented
        }

        public override void HandleBuySequenceOnCargo(IMyCubeBlock cargoBlock, Item item)
        {
            HandleBuySequenceOnCargo(cargoBlock, item, Credits);
        }
        
        public override void HandleSellSequenceOnCargo(IMyCubeBlock cargoBlock, Item item)
        {
            HandleSellSequenceOnCargo(cargoBlock, item, Credits);
        }

        public override void TakeSettingData(StationBase oldStationData)
        {
            FactoryStation loadedData = oldStationData as FactoryStation;

            if (loadedData == null) return;

            if (loadedData.Credits.SerializedDefinition != Credits.SerializedDefinition)
            {
                Credits = loadedData.Credits;
            }

            Recipes = loadedData.Recipes;

            RefreshStock();
        }

        private void RefreshStock()
        {
            Dictionary<string, Item> newBufferStock = new Dictionary<string, Item>();
            Dictionary<string, Item> newResourceStock = new Dictionary<string, Item>();
            Dictionary<string, Item> newProductStock = new Dictionary<string, Item>();

            CreateStockByRecipes(Recipes, newBufferStock, newResourceStock, newProductStock);
            UnifyStocksToBuffer(newBufferStock, newResourceStock, newProductStock);

            AddRuntimeDataToNewStock(newBufferStock, _bufferStock);
            AddRuntimeDataToNewStock(newResourceStock, _resourceStock);
            AddRuntimeDataToNewStock(newProductStock, _productStock);

            _bufferStock = newBufferStock;
            _resourceStock = newResourceStock;
            _productStock = newProductStock;
        }

        private static void AddRuntimeDataToNewStock(
            IDictionary<string, Item> targetStock,
            IDictionary<string, Item> currentStock
        )
        {
            foreach (KeyValuePair<string, Item> pair in targetStock)
            {
                if (currentStock.ContainsKey(pair.Key) == false) continue;

                Item stockItem = currentStock[pair.Key];
                pair.Value.CurrentCargo = stockItem.CurrentCargo;
            }
        }

        private static void CreateStockByRecipes(
            IEnumerable<Recipe> recipes,
            IDictionary<string, Item> newBufferStock,
            IDictionary<string, Item> newSourceStock,
            IDictionary<string, Item> newProductStock
        )
        {
            foreach (Recipe recipe in recipes)
            {
                foreach (Item requiredGood in recipe.RequiredGoods)
                {
                    if (requiredGood.IsBuy && requiredGood.IsSell)
                    {
                        AddToStock(newBufferStock, requiredGood);
                    }
                    else
                    {
                        AddToStock(newSourceStock, requiredGood);
                    }
                }

                foreach (Item producingGood in recipe.ProducingGoods)
                {
                    AddToStock(newProductStock, producingGood);
                }
            }
        }

        private static void UnifyStocksToBuffer(
            IDictionary<string, Item> newBufferStock,
            IDictionary<string, Item> newSourceStock,
            IDictionary<string, Item> newProductStock
        )
        {
            foreach (KeyValuePair<string, Item> pair in newProductStock)
            {
                if (!newBufferStock.ContainsKey(pair.Key)) continue;
                Item stockItem = newBufferStock[pair.Key];
                stockItem.CargoSize += pair.Value.CargoSize;
                newProductStock.Remove(pair.Key);
            }

            foreach (KeyValuePair<string, Item> pair in newSourceStock)
            {
                if (!newBufferStock.ContainsKey(pair.Key)) continue;
                Item stockItem = newBufferStock[pair.Key];
                stockItem.CargoSize += pair.Value.CargoSize;
                newSourceStock.Remove(pair.Key);
            }
        }

        private static void AddToStock(IDictionary<string, Item> targetStock, Item requiredGood)
        {
            string serializedDefinition = requiredGood.SerializedDefinition;
            if (targetStock.ContainsKey(serializedDefinition))
            {
                Item stockItem = targetStock[serializedDefinition];
                stockItem.CargoSize += requiredGood.CargoSize;
            }

            targetStock.Add(serializedDefinition, requiredGood.Clone());
        }
    }
}