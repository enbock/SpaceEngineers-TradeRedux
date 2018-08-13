using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;
using System.Threading;

namespace TradeRedux.Lib
{
    public class PrefabSpawner
    {

        /// <summary>
        /// Spawn a ship - Throws an Exception when area is blocked!
        /// </summary>
        /// <param name="beacon_text">Text to set the beacon (to show players a message)</param>
        /// <param name="prefab_identifier">Prefab file name (Files in Data/Prefabs)</param>
        /// <param name="position">Coordinates to spawn at</param>
        /// <param name="faceTowards">Coordinates to point ship at</param>
        /// <param name="ownerId">owner id (first npc found if owner==0, nobody if no npc found)</param>
        /// <param name="callAfterSpawned">function to be called with spawned cubegrid reference (cubes) => {...}</param>
        /// <returns></returns>
        public static bool SpawnShip(string beacon_text, string prefab_identifier, Vector3D position, Vector3D faceTowards, long ownerId = 0, Action<List<IMyCubeGrid>> callAfterSpawned = null)
        {
            var ic = new List<IMyCubeGrid>();

            var direction = (position - faceTowards);

            if (ownerId == 0)
            {
                ownerId = GetFirstNpcId();
            }

            BoundingSphereD sphere = new BoundingSphereD(position, 30);
            var l = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);


            if (l.Count != 0)
            {
                return false;
                //string test = l.ToList()[0].ToString();                
                //throw new Exception("Area of space " + position + " is blocked (" + l.Count + ")" + test); //return false; //throw new Exception("Area of space is blocked");
            }
            MyAPIGateway.PrefabManager.SpawnPrefab(ic,
                prefab_identifier,
                position,
                direction,
                Vector3D.CalculatePerpendicularVector(direction),
                Vector3.Zero,
                Vector3.Zero,
                beacon_text,
                SpawningOptions.RotateFirstCockpitTowardsDirection | SpawningOptions.UseGridOrigin,
                ownerId,
                false,
                () =>
                {
                    if (callAfterSpawned != null)
                    {
                        if (ic == null) return;//should never happen

                        if (ic.Count == 0)
                        {

                            var li = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);
                            if (li == null || li.Count == 0)
                            {
                                MyAPIGateway.Utilities.ShowMessage(":", "Prefab not spawned or not detected!");
                            }
                            else
                            {
                                var cubeGrid = (li[0] as IMyCubeGrid);
                                cubeGrid.ChangeGridOwnership(ownerId, MyOwnershipShareModeEnum.Faction);
                                callAfterSpawned.Invoke(li.Where(e => e is IMyCubeGrid).Select(e => e as IMyCubeGrid).ToList());
                            }
                        }
                        else
                        {
                            ic[0].ChangeGridOwnership(ownerId, MyOwnershipShareModeEnum.Faction);
                            callAfterSpawned.Invoke(ic.ToList());
                        }
                    }
                });

            return true;
        }

        private static long GetFirstNpcId()
        {
            List<IMyIdentity> ids = new List<IMyIdentity>();
            MyAPIGateway.Multiplayer.Players.GetAllIdentites(ids);
            var npc = ids.FirstOrDefault<IMyIdentity>(p => p.DisplayName.Contains("NPC"));

            if (npc == null)
                return 0;
            return npc.IdentityId;
        }
    }
}
