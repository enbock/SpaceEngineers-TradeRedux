using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace Elitesuppe.Trade
{
    public interface IOutputRepresentor
    {
        void CreateOutput(Dictionary<string, StringBuilder> output, IMyCubeGrid grid);
    }
}