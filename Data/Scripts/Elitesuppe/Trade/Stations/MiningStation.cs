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

        public MiningStation()
        {
        }

        public MiningStation(long ownerId) : base(ownerId, StationType)
        {
            Recipes = new List<Recipe>
            {
                new Recipe(
                    new List<Item>
                    {
                        new Item("MyObjectBuilder_Ingot/Uranium", new Price(32f), 8, 0, 100000, 50),
                        new Item("MyObjectBuilder_Ore/Ice", new Price(12f), 800, 0, 250000, 50)
                    },
                    new List<Item>
                    {
                        new Item("MyObjectBuilder_Ore/Iron", new Price(100f), 0, 1, 10000, 0),
                        new Item("MyObjectBuilder_Ore/Nickel", new Price(100f), 0, 1, 10000, 0),
                        new Item("MyObjectBuilder_Ore/Cobalt", new Price(100f), 0, 1, 10000, 0),
                        new Item("MyObjectBuilder_Ore/Gold", new Price(200f), 0, 1, 10000, 0),
                        new Item("MyObjectBuilder_Ore/Silver", new Price(200f), 0, 1, 10000, 0),
                        new Item("MyObjectBuilder_Ore/Silicon", new Price(200f), 0, 1, 10000, 0),
                        new Item("MyObjectBuilder_Ore/Magnesium", new Price(200f), 0, 1, 10000, 0),
                        new Item("MyObjectBuilder_Ore/Platinum", new Price(300f), 0, 1, 10000, 0)
                    },
                    1f
                )
            };
        }
    }
}