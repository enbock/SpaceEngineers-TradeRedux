using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Elitesuppe.Trade;
using EliteSuppe.Trade.Items;

namespace EliteSuppe.Trade.Stations
{
    [Serializable]
    [XmlRoot(Namespace = Definitions.Version)]
    public class FactoryStation : StationBase
    {
        public List<Recipe> Recipes = new List<Recipe>();
        public double Credits = 0;

        public FactoryStation()
        {
            
        }
        
        protected FactoryStation(long ownerId, string type) : base(ownerId, type)
        {
            
        }
    }
}