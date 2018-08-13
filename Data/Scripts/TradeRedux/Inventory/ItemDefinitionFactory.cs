using Sandbox.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.ObjectBuilders;
using TradeRedux.Exceptions;
using Sandbox.Game.Weapons;

namespace TradeRedux.Inventory
{
    public static class ItemDefinitionFactory
    {
        public static VRage.Game.MyDefinitionId DefinitionFromString(string definition)
        {
            if (string.IsNullOrWhiteSpace(definition) || definition.Contains("(null)"))
                throw new UnknownItemException("<Empty defition>");

            var input = definition.Trim();
            VRage.Game.MyDefinitionId defId;
            if (VRage.Game.MyDefinitionId.TryParse(input, out defId))
                return defId;

            Type objectBuilder;
            string subType = input;
            if(input.Equals(Definitions.Credits, StringComparison.InvariantCultureIgnoreCase))
            {
                objectBuilder = typeof(MyObjectBuilder_Component);
                subType = Definitions.Credits;
            }
            else if (input.EndsWith("Ore", StringComparison.InvariantCultureIgnoreCase))
            {
                objectBuilder = typeof(VRage.Game.MyObjectBuilder_Ore);
                subType = subType.Substring(0, subType.Length - 3);
            }
            else if (input.EndsWith("Ingot", StringComparison.InvariantCultureIgnoreCase))
            {
                objectBuilder = typeof(VRage.Game.MyObjectBuilder_Ingot);
                subType = subType.Substring(0, subType.Length - 5);
            }
            else if (input.Equals("Stone", StringComparison.InvariantCultureIgnoreCase))
            {
                objectBuilder = typeof(VRage.Game.MyObjectBuilder_Ore);
                subType = "Stone";
            }            
            else objectBuilder = typeof(VRage.Game.MyObjectBuilder_Component);

                       
            var definitionId = new VRage.Game.MyDefinitionId(objectBuilder, subType);
            if (!MyDefinitionManager.Static.GetAllDefinitions().Any(d => d.Id.Equals(definitionId)))
            {
                definitionId = new VRage.Game.MyDefinitionId(typeof(VRage.Game.MyObjectBuilder_PhysicalGunObject), subType);
                if (MyDefinitionManager.Static.GetAllDefinitions().Any(d => d.Id.Equals(definitionId)))
                {
                    return definitionId;
                }                
                throw new UnknownItemException(definitionId);
            }
                
            
            return definitionId;
        }

        internal static string DefinitionToString(MyDefinitionId definition)
        {
            var name = definition.SubtypeName;
            if (definition.TypeId.Equals(typeof(VRage.Game.MyObjectBuilder_Ore)))
            {
                if(name.Equals("Stone", StringComparison.InvariantCultureIgnoreCase) ||
                    name.Equals("Ice", StringComparison.InvariantCultureIgnoreCase)
                    )
                {
                    return name;
                }
                return name + " Ore";
            }                
            if (definition.TypeId.Equals(typeof(VRage.Game.MyObjectBuilder_Ingot)))
                {
                    MyBlueprintDefinitionBase ingot;
                    if (MyDefinitionManager.Static.TryGetIngotBlueprintDefinition(definition, out ingot))
                    {
                        return ingot.DisplayNameText;
                    }
                    else return name + " Ingot ????";
                }                
            if (definition.TypeId.Equals(typeof(VRage.Game.MyObjectBuilder_Component)))
            {
                var componentblueprint = MyDefinitionManager.Static.GetBlueprintDefinitions().FirstOrDefault(bp => bp.Results.First().Id.Equals(definition));
                if (componentblueprint != null)
                    return componentblueprint.DisplayNameText;
                return name;
            }

                

            return name;
        }

        private static List<VRage.Game.MyDefinitionId> _ores;
        public static List<MyDefinitionId> Ores
        {
            get
            {
                if (_ores == null)
                {
                    _ores = new List<MyDefinitionId>();
                    string[] oreTypes;
                    MyDefinitionManager.Static.GetOreTypeNames(out oreTypes);

                    foreach (var oreType in oreTypes)
                    {
                        if(!oreType.Equals("Scrap") && !oreType.Equals("Organic"))
                            _ores.Add(new MyDefinitionId(typeof(MyObjectBuilder_Ore), oreType));                        
                    }
                }
                return _ores;
            }
        }

        private static List<VRage.Game.MyDefinitionId> _ingots;
        public static List<VRage.Game.MyDefinitionId> Ingots
        {
            get
            {
                if (_ingots == null)
                {
                    _ingots = new List<VRage.Game.MyDefinitionId>();
                    string[] ingotTypes;
                    MyDefinitionManager.Static.GetOreTypeNames(out ingotTypes);

                    foreach (var ingotType in ingotTypes)
                    {
                        if (!ingotType.Equals("Scrap") && !ingotType.Equals("Organic"))
                        {
                            MyBlueprintDefinitionBase ingot;
                            if (MyDefinitionManager.Static.TryGetIngotBlueprintDefinition(new VRage.Game.MyDefinitionId(typeof(VRage.Game.MyObjectBuilder_Ingot), ingotType), out ingot))
                                foreach (var res in ingot.Results)
                                    _ingots.Add(res.Id);
                        }
                    }
                }               
                return _ingots;
            }
        }

        public static List<VRage.Game.MyDefinitionId> _components;
        public static List<VRage.Game.MyDefinitionId> Components
        {
            get
            {
                if (_components == null)
                {
                    _components = new List<VRage.Game.MyDefinitionId>();
                    foreach (var bd in MyDefinitionManager.Static.GetBlueprintDefinitions())
                    {
                        if (bd.InputItemType.Equals(typeof(MyObjectBuilder_Ingot)) && bd.Results.All(r => r.Id.TypeId.Equals(typeof(MyObjectBuilder_Component))))
                        {
                            foreach (var result in bd.Results)
                            {
                                _components.Add(result.Id);                                
                            }
                        }
                            
                           
                    }                  
                    _components = _components.Distinct().ToList();
                }
                return _components;
            }
        }

        public static List<VRage.Game.MyDefinitionId> _playerTools;
        public static List<VRage.Game.MyDefinitionId> PlayerTools
        {
            get
            {
                if (_playerTools == null)
                {
                    _playerTools = new List<VRage.Game.MyDefinitionId>();
                    foreach (var bd in MyDefinitionManager.Static.GetHandItemDefinitions())
                    {                       
                        if(bd.AvailableInSurvival && bd.Public)
                        {
                            if(!bd.GetObjectBuilder().Id.TypeIdString.Equals("MyObjectBuilder_GoodAIControlHandTool")
                                && !bd.GetObjectBuilder().Id.TypeIdString.Equals("MyObjectBuilder_CubePlacer"))                                
                                _playerTools.Add(bd.PhysicalItemId);
                        }
                            
                    }
                    _playerTools = _playerTools.Distinct().ToList();
                }
                return _playerTools;
            }
        }

        public static List<VRage.Game.MyDefinitionId> _ammunitions;
        public static List<VRage.Game.MyDefinitionId> Ammunitions
        {
            get
            {
                if (_ammunitions == null)
                {
                    _ammunitions = new List<VRage.Game.MyDefinitionId>();
                    foreach (var bd in MyDefinitionManager.Static.GetAllDefinitions().Where(def => def.Id.TypeId.Equals(typeof(MyObjectBuilder_AmmoMagazine))))
                    {
                        if (bd.AvailableInSurvival && bd.Public)
                        {
                            _ammunitions.Add(bd.Id);
                        }

                    }
                    _ammunitions = _ammunitions.Distinct().ToList();
                }
                return _ammunitions;
            }
        }


        public static Dictionary<MyDefinitionId, double> GetRecipeInput(MyDefinitionId definitionId, bool recurseToOres = false)
        {
            var comp = new Dictionary<MyDefinitionId, double>();

            var bd = MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(definitionId);
            
            if (bd != null && bd.Prerequisites.Any() && bd.Prerequisites.FirstOrDefault().Id.SubtypeName.Equals("Scrap"))
            {
                //iron gives Scrap, select the correct blueprint
                bd = MyDefinitionManager.Static.GetBlueprintDefinition(new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), "IronOreToIngot"));
            }

            if (bd != null && bd.Results.Any() && bd.Prerequisites.Any())
            {
                var outAmount = bd.Results.FirstOrDefault().Amount;
                var normalizedOutAmount = outAmount.RawValue / 1000000d;

                foreach (var prerequisite in bd.Prerequisites)
                {
                    if (!recurseToOres || prerequisite.Id.TypeId.Equals(typeof(MyObjectBuilder_Ore)))
                    {
                        var normalizedPrereqAmount = prerequisite.Amount.RawValue / 1000000d;
                        if (!comp.ContainsKey(prerequisite.Id))
                            comp.Add(prerequisite.Id, normalizedPrereqAmount / normalizedOutAmount);
                        else comp[prerequisite.Id] += normalizedPrereqAmount / normalizedOutAmount;
                    }
                    else
                    {
                        //recursion not yet functional! (gives scrap)
                        var subComponents = GetRecipeInput(prerequisite.Id, true);
                        foreach (var sc in subComponents)
                        {
                            if (!comp.ContainsKey(sc.Key))
                                comp.Add(sc.Key, sc.Value);
                            else comp[sc.Key] += sc.Value;
                        }
                    }
                }

            }                       

            return comp;
        }
        
        public static float GetProductionTimeInSeconds(MyDefinitionId definitionId)
        {
            var comp = new Dictionary<MyDefinitionId, double>();

            var bd = MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(definitionId);

            if (bd != null && bd.Prerequisites.Any() && bd.Prerequisites.FirstOrDefault().Id.SubtypeName.Equals("Scrap"))
            {
                //iron gives Scrap, select the correct blueprint
                bd = MyDefinitionManager.Static.GetBlueprintDefinition(new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), "IronOreToIngot"));
            }

            if (bd != null && bd.Results.Any() && bd.Prerequisites.Any())
            {
                return bd.BaseProductionTimeInSeconds;
            }

            return -1;
        }

    }
}
