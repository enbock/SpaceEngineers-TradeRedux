using System;
using System.Collections.Generic;
using System.Linq;
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
        public Item Credits = new Item(Definitions.Credits);

        private Dictionary<string, Item> _stock = new Dictionary<string, Item>();

        public override List<Item> Goods
        {
            get { return new List<Item>(_stock.Values); }
        }

        public IEnumerable<Item> Stock
        {
            get { return _stock.Values; }
        }

        protected FactoryStation(string type) : base(type)
        {
        }

        public override void HandleProdCycle()
        {
            foreach (Recipe recipe in Recipes)
            {
                if (recipe.IsProducing) continue;
                if (!IsResourcesOnStock(recipe.RequiredGoods)) continue;
                ReduceResourcesStock(recipe.RequiredGoods);
                recipe.IsProducing = true;
                recipe.ProducingStartedAt = DateTime.Now;
            }

            foreach (Recipe recipe in Recipes)
            {
                if (recipe.IsProducing == false) continue;
                double producingTime = DateTime.Now.Subtract(recipe.ProducingStartedAt).TotalSeconds;
                if (producingTime < recipe.ProductionTimeInSeconds) continue;

                if (!IsOutputStockAvailable(recipe.ProducingGoods)) continue;

                AddProducedItemsToStock(recipe.ProducingGoods);
                recipe.IsProducing = false;
                recipe.ProducingStartedAt = DateTime.MinValue;
            }
        }

        public override void HandlePurchaseSequenceOnCargo(IMyCubeBlock cargoBlock, Item item)
        {
            HandlePurchaseSequenceOnCargo(cargoBlock, item, Credits);
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
                    loadedData.Credits.CurrentCargo = Credits.CurrentCargo;
                    Credits = loadedData.Credits;
                }

                SynchronizeRecipes(loadedData.Recipes);
            }

            RefreshStock();
        }
        
        public Dictionary<string, double> Progress()
        {
            Dictionary<Item, List<double>> statistic = new Dictionary<Item, List<double>>();
            foreach (Recipe recipe in Recipes)
            {
                if (!recipe.IsProducing) continue;
                double producingTime = DateTime.Now.Subtract(recipe.ProducingStartedAt).TotalSeconds;
                double finish = 100f / recipe.ProductionTimeInSeconds * producingTime;

                foreach (Item producingGood in recipe.ProducingGoods)
                {
                    if (!statistic.ContainsKey(producingGood))
                    {
                        statistic.Add(producingGood, new List<double>());
                    }

                    statistic[producingGood].Add(finish);
                }
            }

            Dictionary<string, double> progress = new Dictionary<string, double>();
            foreach (KeyValuePair<Item, List<double>> pair in statistic)
            {
                double finish = pair.Value.Sum() / pair.Value.Count;
                progress.Add(pair.Key.ToString(), finish > 100f ? 100f : finish);
            }

            return progress;
        }

        private void SynchronizeRecipes(List<Recipe> loadedRecipes)
        {
            foreach (Recipe loadedRecipe in loadedRecipes)
            {
                foreach (Recipe recipe in Recipes)
                {
                    if (loadedRecipe.Name != recipe.Name) continue;
                    loadedRecipe.ProducingStartedAt = recipe.ProducingStartedAt;
                    loadedRecipe.IsProducing = recipe.IsProducing;

                    break;
                }
            }

            Recipes = loadedRecipes;
        }

        private void AddProducedItemsToStock(IEnumerable<Item> producingGoods)
        {
            foreach (Item good in producingGoods)
            {
                string definition = good.SerializedDefinition;
                double itemsToStock = good.Result;

                _stock[definition].CurrentCargo += itemsToStock;
            }
        }

        private bool IsOutputStockAvailable(IEnumerable<Item> producingGoods)
        {
            foreach (Item good in producingGoods)
            {
                string definition = good.SerializedDefinition;

                if (!_stock.ContainsKey(definition)) continue;

                Item stock = _stock[definition];
                double stockAvailable = stock.CargoSize - stock.CurrentCargo;

                if (stockAvailable < good.Result) return false;
            }

            return true;
        }

        private void ReduceResourcesStock(IEnumerable<Item> requiredGoods)
        {
            foreach (Item good in requiredGoods)
            {
                string definition = good.SerializedDefinition;
                double neededItems = good.Required;

                if (_stock.ContainsKey(definition)) _stock[definition].CurrentCargo -= neededItems;
            }
        }

        private bool IsResourcesOnStock(IEnumerable<Item> requiredGoods)
        {
            foreach (Item good in requiredGoods)
            {
                string definition = good.SerializedDefinition;

                if (!_stock.ContainsKey(definition)) continue;

                if (_stock[definition].CurrentCargo < good.Required) return false;
            }

            return true;
        }

        private void RefreshStock()
        {
            Dictionary<string, Item> stock = new Dictionary<string, Item>();

            CreateStockByRecipes(Recipes, stock);
            AddRuntimeDataToNewStock(stock, _stock);

            _stock = stock;
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
            IDictionary<string, Item> stock
        )
        {
            foreach (Recipe recipe in recipes)
            {
                foreach (Item requiredGood in recipe.RequiredGoods)
                {
                    AddToStock(stock, requiredGood);
                }

                foreach (Item producingGood in recipe.ProducingGoods)
                {
                    AddToStock(stock, producingGood);
                }
            }
        }

        private static void AddToStock(IDictionary<string, Item> targetStock, Item good)
        {
            string serializedDefinition = good.SerializedDefinition;
            if (targetStock.ContainsKey(serializedDefinition))
            {
                Item stockItem = targetStock[serializedDefinition];
                stockItem.CargoSize += good.CargoSize;
                if (good.IsPurchasing) stockItem.PurchasePrice = good.PurchasePrice;
                if (good.IsSelling) stockItem.SellPrice = good.SellPrice;

                return;
            }

            Item clone = good.Clone();
            targetStock.Add(serializedDefinition, clone);
        }
    }
}