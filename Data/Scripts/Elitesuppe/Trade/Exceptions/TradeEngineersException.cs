using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
