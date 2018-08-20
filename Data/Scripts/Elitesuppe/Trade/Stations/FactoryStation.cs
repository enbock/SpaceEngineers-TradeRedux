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
                all.AddRange(BufferStock);
                all.AddRange(ResourceStock);
                all.AddRange(ProductStock);

                return all;
            }
        }

        public IEnumerable<Item> BufferStock
        {
            get { return _bufferStock.Values; }
        }
        public IEnumerable<Item> ResourceStock
        {
            get { return _resourceStock.Values; }
        }
        public IEnumerable<Item> ProductStock
        {
            get { return _productStock.Values; }
        }

        public FactoryStation()
        {
        }

        protected FactoryStation(long ownerId, string type) : base(ownerId, type)
        {
        }

        public override void HandleProdCycle()
        {
            foreach (Recipe recipe in Recipes)
            {
                if (recipe.IsProducing) continue;
                if (!ResourcesOnStock(recipe.RequiredGoods)) continue;
                ReduceResourcesStock(recipe.RequiredGoods);
                recipe.IsProducing = true;
                recipe.ProducingStartedAt = DateTime.Now;
            }

            foreach (Recipe recipe in Recipes)
            {
                if (recipe.IsProducing == false) continue;
                if (!ResourcesOnStock(recipe.RequiredGoods)) continue;
                double producingTime = DateTime.Now.Subtract(recipe.ProducingStartedAt).TotalSeconds;
                if (producingTime < recipe.ProductionTimeInSeconds) continue;

                if (!OutputStockAvailable(recipe.ProducingGoods)) return;
                AddRecipeToStock(recipe.ProducingGoods);
                recipe.IsProducing = false;
                recipe.ProducingStartedAt = DateTime.MinValue;
            }
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

            if (loadedData != null)
            {
                if (loadedData.Credits.SerializedDefinition != Credits.SerializedDefinition)
                {
                    Credits = loadedData.Credits;
                }

                Recipes = loadedData.Recipes;
            }

            RefreshStock();
        }

        private void AddRecipeToStock(IEnumerable<Item> producingGoods)
        {
            foreach (Item good in producingGoods)
            {
                string definition = good.SerializedDefinition;
                double itemsToStock = good.Result;

                if (_bufferStock.ContainsKey(definition))
                    itemsToStock = AddProductToStock(_bufferStock[definition], itemsToStock);
                if (_productStock.ContainsKey(definition)) AddProductToStock(_productStock[definition], itemsToStock);
            }
        }

        private double AddProductToStock(Item stock, double amount)
        {
            double available = stock.CargoSize - stock.CurrentCargo;
            if (available < amount)
            {
                amount -= available;
                stock.CurrentCargo = stock.CargoSize;
            }
            else
            {
                stock.CurrentCargo += amount;
                amount = 0;
            }

            return amount;
        }

        private bool OutputStockAvailable(IEnumerable<Item> producingGoods)
        {
            foreach (Item good in producingGoods)
            {
                string definition = good.SerializedDefinition;
                double stockAvailable = 0;

                if (_bufferStock.ContainsKey(definition))
                {
                    Item stock = _bufferStock[definition];
                    stockAvailable += stock.CargoSize - stock.CurrentCargo;
                }

                if (_productStock.ContainsKey(definition))
                {
                    Item stock = _productStock[definition];
                    stockAvailable += stock.CargoSize - stock.CurrentCargo;
                }

                if (stockAvailable >= good.Result) return false;
            }

            return true;
        }

        private void ReduceResourcesStock(IEnumerable<Item> requiredGoods)
        {
            foreach (Item good in requiredGoods)
            {
                string definition = good.SerializedDefinition;
                double neededItems = good.Required;

                if (_bufferStock.ContainsKey(definition))
                    neededItems = ReduceStock(_bufferStock[definition], neededItems);
                if (_resourceStock.ContainsKey(definition))
                    ReduceStock(_resourceStock[definition], neededItems);
            }
        }

        private static double ReduceStock(Item stock, double amount)
        {
            if (stock.CurrentCargo < amount)
            {
                amount -= stock.CurrentCargo;
                stock.CurrentCargo = 0;
            }
            else
            {
                stock.CurrentCargo -= amount;
                amount = 0;
            }

            return amount;
        }

        private bool ResourcesOnStock(IEnumerable<Item> requiredGoods)
        {
            foreach (Item good in requiredGoods)
            {
                string definition = good.SerializedDefinition;
                double availableItems = 0;

                if (_bufferStock.ContainsKey(definition))
                {
                    availableItems += _bufferStock[definition].CurrentCargo;
                }

                if (_resourceStock.ContainsKey(definition))
                {
                    availableItems += _resourceStock[definition].CurrentCargo;
                }

                if (availableItems < good.Required) return false;
            }

            return true;
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

                return;
            }

            Item clone = requiredGood.Clone();
            targetStock.Add(serializedDefinition, clone);
        }
    }
}