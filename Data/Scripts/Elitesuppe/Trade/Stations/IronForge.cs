using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Elitesuppe.Trade;
using EliteSuppe.Trade.Items;

namespace EliteSuppe.Trade.Stations
{
    [Serializable]
    [XmlRoot(Namespace = Definitions.Version)]
    public class IronForge : FactoryStation
    {
        public const string StationType = "Elitesuppe_TradeRedux_IronForge";

        public IronForge() : base(StationType)
        {
        }

        public IronForge(bool init = false) : base(StationType)
        {
            if (!init) return; // deserialization created

            Recipes = new List<Recipe>
            {
                new Recipe(
                    new List<Item>
                    {
                        new Item("Ingot/Uranium", new Price(0.03, 0.5, 1.5), new Price(), 1000, 0, 25000),
                        new Item("Ore/Ice", new Price(0.0001, 0.5, 1.5), new Price(), 2000, 0, 25000),
                        new Item("Ingot/Iron", new Price(0.0002, 0.5, 1.5), new Price(), 1000, 0, 25000)
                    },
                    new List<Item>
                    {
                        new Item("Component/SteelPlate", new Price(), new Price(0.05f), 0, 1000, 100000)
                    },
                    20f
                ),
                new Recipe(
                    new List<Item>
                    {
                        new Item("Ingot/Uranium", new Price(0.03, 0.5, 1.5), new Price(), 1000, 0, 25000),
                        new Item("Ore/Ice", new Price(0.0001, 0.5, 1.5), new Price(), 2400, 0, 25000),
                        new Item("Ingot/Iron", new Price(0.0002, 0.5, 1.5), new Price(), 1000, 0, 25000)
                    },
                    new List<Item>
                    {
                        new Item("Component/Construction Component", new Price(), new Price(0.07f), 0, 1000, 100000)
                    },
                    24f
                ),
                new Recipe(
                    new List<Item>
                    {
                        new Item("Ingot/Uranium", new Price(0.03f, 0.5f, 1.5f), new Price(), 1000, 0, 25000),
                        new Item("Ore/Ice", new Price(0.0001, 0.5, 1.5), new Price(), 2200, 0, 25000),
                        new Item("Ingot/Iron", new Price(0.0002, 0.5, 1.5), new Price(), 1000, 0, 25000)
                    },
                    new List<Item>
                    {
                        new Item("Component/Interior Plate", new Price(), new Price(0.06f), 0, 1000, 100000)
                    },
                    22f
                )
            };
        }
    }
}