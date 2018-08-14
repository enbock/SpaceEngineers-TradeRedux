using System;
using System.Collections.Generic;

namespace EliteSuppe.Trade.Items
{
    [Serializable]
    public class Recipe
    {
        public List<Item> RequiredGoods { get; } = new List<Item>();
        public List<Item> ProducingGoods { get; } = new List<Item>();
        public double Time;

        public Recipe()
        {
        }

        public Recipe(List<Item> requiredGoods, List<Item> producingGoods, double time = 1D)
        {
            RequiredGoods = requiredGoods;
            ProducingGoods = producingGoods;
            Time = time;
        }
    }
}