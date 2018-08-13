
namespace Elitesuppe.Trade.TradeGoods
{
    public class PriceModel
    {
        public PriceModel() { }

        public PriceModel(double price, bool willProduce = false, double minPercent = 0.6f, double maxPercent = 1.4f)
        {
            Price = price;
            MinPercent = minPercent;
            MaxPercent = maxPercent;
            IsProducent = willProduce;
        }

        public bool IsProducent { get; set; } = false;

        public double Price { get; set; } = 0;

        public double MinPercent { get; set; } = 0.75f;

        public double MaxPercent { get; set; } = 1.25f;

        public double GetBuyPrice(double cargoVolumePercent = 0.5)
        {
            cargoVolumePercent = cargoVolumePercent > 1 ? 1 : cargoVolumePercent;
            var preis = Price * (1 - (1 - MinPercent) * cargoVolumePercent);
            if (!IsProducent) preis += Price;
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
            return Price + ";" + MinPercent + ";" + MaxPercent;
        }
    }
}
