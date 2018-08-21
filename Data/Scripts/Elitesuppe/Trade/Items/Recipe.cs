using System;
using System.Collections.Generic;

namespace EliteSuppe.Trade.Items
{
    [Serializable]
    public class Recipe
    {
        public List<Item> RequiredGoods { get; } = new List<Item>();
        public List<Item> ProducingGoods { get; } = new List<Item>();
        public double ProductionTimeInSeconds;
        public bool IsProducing = false;
        public DateTime ProducingStartedAt = DateTime.MinValue;

        public string Name
        {
            get
            {
                string name = "";
                foreach (Item good in ProducingGoods)
                {
                    name += good.SerializedDefinition;
                }

                return name;
            }
        }

        public Recipe()
        {
        }

        public Recipe(List<Item> requiredGoods, List<Item> producingGoods, double productionTimeInSeconds = 1f)
        {
            RequiredGoods = requiredGoods;
            ProducingGoods = producingGoods;
            ProductionTimeInSeconds = productionTimeInSeconds;
        }
    }
}