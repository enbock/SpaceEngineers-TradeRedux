using System;

namespace EliteSuppe.Trade.Items
{
    [Serializable]
    public class Price
    {
        public double Amount = 0f;
        public double MinPercent = 1f;
        public double MaxPercent = 1f;
        
        public Price()
        {
        }

        public Price(double amount)
        {
            Amount = amount;
        }

        public Price(double amount, double minPercent, double maxPercent)
        {
            Amount = amount;
            MinPercent = minPercent;
            MaxPercent = maxPercent;
        }


        public double GetStockPrice(double cargoVolumePercent = 0.5f)
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