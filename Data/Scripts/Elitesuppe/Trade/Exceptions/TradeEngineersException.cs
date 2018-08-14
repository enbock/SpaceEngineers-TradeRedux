using System;

namespace Elitesuppe.Trade.Exceptions
{
    public class TradeEngineersException : Exception
    {
        public TradeEngineersException() : base("An Exception occurred in Trade engineers")
        {

        }

        public TradeEngineersException(string message) : base(message)
        {

        }
    }
}
