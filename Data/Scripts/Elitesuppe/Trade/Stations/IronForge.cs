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

        public IronForge()
        {
        }

        public IronForge(long ownerId) : base(ownerId, StationType)
        {
            Recipes = new List<Recipe>
            {
                new Recipe(
                    new List<Item>
                    {
                        new Item("MyObjectBuilder_Ingot/Uranium", new Price(32f), 1, 0, 25000, 50),
                        new Item("MyObjectBuilder_Ore/Ice", new Price(12f), 1, 0, 25000, 50),
                        new Item("MyObjectBuilder_Ingot/Iron", new Price(12f), 1, 0, 25000, 50)
                    },
                    new List<Item>
                    {
                        new Item("MyObjectBuilder_Component/SteelPlate", new Price(100f), 0, 1, 100000, 0)
                    },
                    1f
                ),
                new Recipe(
                    new List<Item>
                    {
                        new Item("MyObjectBuilder_Ingot/Uranium", new Price(32f), 1, 0, 25000, 50),
                        new Item("MyObjectBuilder_Ore/Ice", new Price(12f), 1, 0, 25000, 50),
                        new Item("MyObjectBuilder_Ingot/Iron", new Price(12f), 1, 0, 25000, 50)
                    },
                    new List<Item>
                    {
                        new Item("MyObjectBuilder_Component/Construction Component", new Price(100f), 0, 1, 100000, 0)
                    },
                    1f
                ),
                new Recipe(
                    new List<Item>
                    {
                        new Item("MyObjectBuilder_Ingot/Uranium", new Price(32f), 1, 0, 25000, 50),
                        new Item("MyObjectBuilder_Ore/Ice", new Price(12f), 1, 0, 25000, 50),
                        new Item("MyObjectBuilder_Ingot/Iron", new Price(12f), 1, 0, 25000, 50)
                    },
                    new List<Item>
                    {
                        new Item("MyObjectBuilder_Component/Interior Plate", new Price(100f), 0, 1, 100000, 0)
                    },
                    1f
                )
            };
        }
    }
}