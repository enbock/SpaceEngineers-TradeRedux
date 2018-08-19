using System;

namespace EliteSuppe.Trade.Items
{
    [Serializable]
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

        public double GetBuyPrice(double cargoVolumePercent = 0.5f)
        {
            return CalculatePrice(cargoVolumePercent);
        }

        public double GetSellPrice(double cargoVolumePercent = 0.5)
        {
            return CalculatePrice(cargoVolumePercent);
        }

        protected double CalculatePrice(double currentCargo = 0.5f)
        {
            currentCargo = currentCargo > 1f ? 1f : currentCargo;
            currentCargo = currentCargo < 0f ? 0f : currentCargo;

            double relation = MaxPercent - MinPercent;

            return Amount * (MinPercent + relation * (1f - currentCargo));
        }

        public override string ToString()
        {
            return Amount + ";" + MinPercent + ";" + MaxPercent;
        }

        public Price Clone()
        {
            return MemberwiseClone() as Price;
        }
    }
}