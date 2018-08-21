using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Elitesuppe.Trade;
using EliteSuppe.Trade.Items;

namespace EliteSuppe.Trade.Stations
{
    [Serializable]
    [XmlRoot(Namespace = Definitions.Version)]
    public class MiningStation : FactoryStation
    {
        public const string StationType = "Elitesuppe_TradeRedux_MiningStation";

        public MiningStation() : base(StationType)
        {
        }

        public MiningStation(bool init = false) : base(StationType)
        {
            if (!init) return; // deserialization created
            
            Recipes = new List<Recipe>
            {
                new Recipe(
                    new List<Item>
                    {
                        new Item("Ingot/Uranium", new Price(0.02f, 0.5f, 1.5f), new Price(), 8, 0, 100000),
                        new Item("Ore/Ice", new Price(12f), new Price(), 32, 0, 250000)
                    },
                    new List<Item>
                    {
                        new Item("Ore/Iron", new Price(), new Price(0.001f, 0.75f, 2f), 0, 1, 10000),
                        new Item("Ore/Nickel", new Price(), new Price(0.0084f, 0.75f, 2f), 0, 1, 10000),
                        new Item("Ore/Cobalt", new Price(), new Price(0.01f, 0.75f, 2f), 0, 1, 10000),
                        new Item("Ore/Gold", new Price(), new Price(0.3f, 0.75f, 2f), 0, 1, 10000),
                        new Item("Ore/Silver", new Price(), new Price(0.03f, 0.75f, 2f), 0, 1, 10000),
                        new Item("Ore/Silicon", new Price(), new Price(0.0044f, 0.75f, 2f), 0, 1, 10000),
                        new Item("Ore/Magnesium", new Price(), new Price(0.3f, 0.75f, 2f), 0, 1, 10000),
                        new Item("Ore/Platinum", new Price(), new Price(0.7f, 0.75f, 2f), 0, 1, 10000)
                    },
                    1f
                )
            };
        }
    }
}