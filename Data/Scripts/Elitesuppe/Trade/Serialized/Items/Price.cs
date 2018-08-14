using System;

namespace Elitesuppe.Trade.TradeGoods
{
    public class Price
    {
        public Price()
        {
        }

        public Price(double amount, double minPercent = 0.6f, double maxPercent = 1.4f)
        {
            Amount = amount;
            MinPercent = minPercent;
            MaxPercent = maxPercent;
        }

        public double Amount { get; set; } = 0;

        public double MinPercent { get; set; } = 0.10f;

        public double MaxPercent { get; set; } = 1.25f;

        public double GetBuyPrice(double cargoVolumePercent = 0.5)
        {
            cargoVolumePercent = cargoVolumePercent > 1 ? 1 : cargoVolumePercent;
            var preis = Amount * (1 - (1 - MinPercent) * cargoVolumePercent);
            return preis;
        }

        public double GetSellPrice(double cargoVolumePercent = 0.5)
        {
            cargoVolumePercent = cargoVolumePercent > 1 ? 1 : cargoVolumePercent;
            var preis = Amount * (1 + (MaxPercent - 1) * (1 - cargoVolumePercent));
            return preis;
        }

        public override string ToString()
        {
            return Amount + ";" + MinPercent + ";" + MaxPercent;
        }
    }
}