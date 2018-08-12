using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;

namespace TradeEngineers.Exceptions
{
    public class UnknownItemException : TradeEngineersException
    {
        public UnknownItemException() : base("The item is unknown to the game")
        {

        }
        public UnknownItemException(MyDefinitionId id) : base($"The item '{id}' is unknown to the game")
        {

        }
    }
}
