using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.ModAPI;
using VRage.Game.ModAPI;
using System.Text.RegularExpressions;
using Elitesuppe.Trade;
using VRageMath;
using VRage.Game;
using System.Text;
using Sandbox.Definitions;

namespace Elitesuppe
{
    [VRage.Game.Components.MySessionComponentDescriptor(VRage.Game.Components.MyUpdateOrder.BeforeSimulation)]
    class Chat : VRage.Game.Components.MySessionComponentBase
    {
        private bool _isLoaded = false;

        public override void UpdateBeforeSimulation()
        {
            if (!_isLoaded)
            {
                Init();
            }
        }

        private void Init()
        {
            _isLoaded = true;
            MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;
            MyAPIGateway.Utilities.ShowMessage("Elitesuppe", "Mod loaded.");
        }

        private void Utilities_MessageEntered(string messageText, ref bool sendToOthers)
        {
            var commandRegex = new Regex(@"\/(te)\s+([\S]+)([\s\S]+)*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            try
            {
                var match = commandRegex.Match(messageText);
                if (match.Success)
                {
                    if (match.Groups.Count > 2)
                    {
                        sendToOthers = HandleChatInput(match.Groups[2].ToString(), match.Groups[3].ToString().Trim().Split(' ')
                            .Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToArray());
                    }
                    else sendToOthers = HandleChatInput(match.Groups[2].ToString());
                }
                else if (messageText.Equals("/tr", StringComparison.InvariantCultureIgnoreCase))
                {
                    HandleChatInput("help");
                }
            }
            catch (Exception e)
            {
                MyAPIGateway.Utilities.ShowMessage("Exception in TradeRedux chat parser", e.Message);
            }
        }

        private bool HandleChatInput(string cmd, string[] args = null)
        {
            if (!cmd.Equals("help", StringComparison.InvariantCultureIgnoreCase)) return true;
            
            MyAPIGateway.Utilities.ShowMissionScreen(
                "Trade Redux Help",
                null,
                "Trade Redux Help",
                "Welcome to Trade Redux. This mods allows trading in this universe. Go to a trade station, connect you ship and trade with the station. Have fun and become rich."
            );
            return false;

        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;
            base.UnloadData();
        }

    }

    public static class ChatWorkers
    {
        private static IEnumerable<IMyCubeGrid> GetStationsInRange(Vector3D playerPositon)
        {
            var sphereAroundPlayer = new VRageMath.BoundingSphereD(playerPositon, 500);
            var grids = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphereAroundPlayer);

            return grids
                .Where(b => (b as IMyCubeGrid) != null)
                .OrderBy(a => (playerPositon - a.GetPosition()).Length())
                .Select(a => a as IMyCubeGrid);
        }
    }
}
