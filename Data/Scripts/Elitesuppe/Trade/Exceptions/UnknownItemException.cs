using VRage.Game;

namespace Elitesuppe.Trade.Exceptions
{
    public class UnknownItemException : TradeEngineersException
    {
        public UnknownItemException() : base("The item is unknown to the game")
        {

        }
        public UnknownItemException(MyDefinitionId id) : base($"The item '{id}' is unknown to the game")
        {

        }
        public UnknownItemException(string id) : base($"The item '{id}' is unknown to the game")
        {

        }
    }
}
