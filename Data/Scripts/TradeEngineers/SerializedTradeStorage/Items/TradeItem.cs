using System;
using VRage.Game;
using Sandbox.ModAPI;

namespace TradeEngineers
{
    [Serializable]
    public class TradeItem
    {
        public TradeItem() { }
        public TradeItem(MyDefinitionId itemType, TradeGoods.PriceModel priceModel, bool sell, bool buy, int cargoSize = 1000, int currentCargo = 500)
        {
            _definition = itemType;
            PriceModel = priceModel;
            CargoSize = cargoSize;
            CurrentCargo = currentCargo;
            IsSell = sell;
            IsBuy = buy;
        }
        public TradeItem(string itemType, TradeGoods.PriceModel priceModel, bool sell, bool buy, int cargoSize = 1000, int currentCargo = 500)
        {
            try
            {
                _definition = Inventory.ItemDefinitionFactory.DefinitionFromString(itemType);
                PriceModel = priceModel;
                CargoSize = cargoSize;
                CurrentCargo = currentCargo;
                IsSell = sell;
                IsBuy = buy;
            }
            catch (Exceptions.UnknownItemException)
            {
                //MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
            }
        }

        private MyDefinitionId _definition;
        public MyDefinitionId Definition
        {
            get
            {
                return _definition;
            }
        }

        public string SerializedDefinition
        {
            get
            {
                return Definition.ToString();
            }

            set
            {
                try
                {
                    _definition = Inventory.ItemDefinitionFactory.DefinitionFromString(value);
                }
                catch (Exceptions.UnknownItemException)
                {
                    //MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
                }
            }
        }
        public bool IsSell { get; set; }

        public bool IsBuy { get; set; }

        public double CurrentCargo { get; set; }

        public double CargoSize { get; set; } = double.MaxValue;

        public double CargoRatio
        {
            get
            {
                return 1f / CargoSize * CurrentCargo;
            }
        }

        public TradeGoods.PriceModel PriceModel = new TradeGoods.PriceModel(1.0);

        public override string ToString()
        {
            return Inventory.ItemDefinitionFactory.DefinitionToString(Definition);
        }
    }
}
