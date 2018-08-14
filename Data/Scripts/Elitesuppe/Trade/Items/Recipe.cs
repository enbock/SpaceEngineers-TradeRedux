using System;
using System.Collections.Generic;

namespace EliteSuppe.Trade.Items
{
    [Serializable]
    public class Recipe
    {
        public List<Item> RequiredGoods;
        public List<Item> ProducingGoods;
        public double Time;

        public Recipe()
        {
        }

        public Recipe(List<Item> requiredGoods, List<Item> producingGoods, double time = 1f)
        {
            RequiredGoods = requiredGoods;
            ProducingGoods = producingGoods;
            Time = time;
        }
    }
}