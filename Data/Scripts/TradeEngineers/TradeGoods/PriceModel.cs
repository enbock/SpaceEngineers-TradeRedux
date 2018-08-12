
namespace TradeEngineers.TradeGoods
{
    public class PriceModel
    {
        public PriceModel() { }

        public PriceModel(double productionPrice, bool willProduce = false, double minPercent = 0.6f, double maxPercent = 1.4f)
        {
            ProductionPrice = productionPrice;
            MinPercent = minPercent;
            MaxPercent = maxPercent;
            IsProducent = willProduce;
        }

        public bool IsProducent { get; set; } = false;

        public double ProductionPrice { get; set; } = 0;

        public double Price
        {
            get
            {
                return ProductionPrice;
            }
        }

        public double MinPercent { get; set; } = 0.75f;

        public double MaxPercent { get; set; } = 1.25f;

        public double GetBuyPrice(double cargoVolumePercent = 0.5)
        {
            cargoVolumePercent = cargoVolumePercent > 1 ? 1 : cargoVolumePercent;
            var preis = ProductionPrice * (1 - (1 - MinPercent) * cargoVolumePercent);
            if (!IsProducent) preis += ProductionPrice;
            return preis;
        }

        public double GerSellPrice(double cargoVolumePercent = 0.5)
        {
            cargoVolumePercent = cargoVolumePercent > 1 ? 1 : cargoVolumePercent;
            var preis = Price * (1 + (MaxPercent - 1) * (1 - cargoVolumePercent));
            return preis;
        }

        public override string ToString()
        {
            return Price + "(" + ProductionPrice + ");" + MinPercent + ";" + MaxPercent;
        }
    }
}
