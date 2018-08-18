using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using IMyShipConnector = Sandbox.ModAPI.IMyShipConnector;

namespace Elitesuppe.Trade
{
    public static class GridApi
    {
        public static Dictionary<IMyShipConnector, IMyCubeGrid> GetConnectedShips(IMyEntity entity)
        {
            var connections = new Dictionary<IMyShipConnector, IMyCubeGrid>();
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            (entity.GetTopMostParent() as IMyCubeGrid)?.GetBlocks(
                blocks,
                slim => slim.FatBlock is IMyShipConnector
            );

            foreach (var slim in blocks)
            {
                IMyShipConnector connector = slim.FatBlock as IMyShipConnector;
                if (!(slim.FatBlock is IMyShipConnector)) continue;
                if (connector.Status.Equals(MyShipConnectorStatus.Connected))
                {
                    connections.Add(connector, connector.OtherConnector.GetTopMostParent() as IMyCubeGrid);
                }
                else
                {
                    connections.Add(connector, null);
                }
            }

            return connections;
        }
    }
}