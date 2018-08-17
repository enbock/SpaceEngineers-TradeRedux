using System;
using System.Collections.Generic;
using System.Linq;
using Elitesuppe.Trade.Exceptions;
using Sandbox.Definitions;
using VRage.Game;

namespace Elitesuppe.Trade.Inventory
{
    public static class ItemDefinitionFactory
    {
        public static MyDefinitionId DefinitionFromString(string definition)
        {
            if (string.IsNullOrWhiteSpace(definition) || definition.Contains("(null)"))
                throw new UnknownItemException("<Empty defition>");

            var input = definition.Trim();
            MyDefinitionId foundDefinitionId;
            if (MyDefinitionId.TryParse(input, out foundDefinitionId))
                return foundDefinitionId;

            Type objectBuilder;
            string subType = input;
            if (input.EndsWith("Ore", StringComparison.InvariantCultureIgnoreCase))
            {
                objectBuilder = typeof(MyObjectBuilder_Ore);
                subType = subType.Substring(0, subType.Length - 3);
            }
            else if (input.EndsWith("Ingot", StringComparison.InvariantCultureIgnoreCase))
            {
                objectBuilder = typeof(MyObjectBuilder_Ingot);
                subType = subType.Substring(0, subType.Length - 5);
            }
            else if (input.Equals("Stone", StringComparison.InvariantCultureIgnoreCase))
            {
                objectBuilder = typeof(MyObjectBuilder_Ore);
                subType = "Stone";
            }
            else objectBuilder = typeof(MyObjectBuilder_Component);


            var definitionId = new MyDefinitionId(objectBuilder, subType);
            if (!MyDefinitionManager.Static.GetAllDefinitions().Any(d => d.Id.Equals(definitionId)))
            {
                definitionId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), subType);
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
            if (definition.TypeId == typeof(MyObjectBuilder_Ore))
            {
                if (name.Equals("Stone", StringComparison.InvariantCultureIgnoreCase) ||
                    name.Equals("Ice", StringComparison.InvariantCultureIgnoreCase)
                )
                {
                    return name;
                }

                return name + " Ore";
            }

            if (definition.TypeId == typeof(MyObjectBuilder_Ingot))
            {
                MyBlueprintDefinitionBase ingot;
                if (MyDefinitionManager.Static.TryGetIngotBlueprintDefinition(definition, out ingot))
                {
                    return ingot.DisplayNameText;
                }

                return name + " Ingot ????";
            }

            if (definition.TypeId != typeof(MyObjectBuilder_Component)) return name;
            
            var blueprintDefinitionBase = MyDefinitionManager.Static.GetBlueprintDefinitions()
                .FirstOrDefault(bp => bp.Results.First().Id.Equals(definition));
            return blueprintDefinitionBase != null ? blueprintDefinitionBase.DisplayNameText : name;


        }

        public static Dictionary<MyDefinitionId, double> GetRecipeInput(
            MyDefinitionId definitionId,
            bool recurseToOres = false
        )
        {
            var comp = new Dictionary<MyDefinitionId, double>();

            var bd = MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(definitionId);

            if (bd != null &&
                bd.Prerequisites.Any() &&
                bd.Prerequisites.FirstOrDefault().Id.SubtypeName.Equals("Scrap"))
            {
                //iron gives Scrap, select the correct blueprint
                bd = MyDefinitionManager.Static.GetBlueprintDefinition(
                    new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), "IronOreToIngot")
                );
            }

            if (bd == null || !bd.Results.Any() || !bd.Prerequisites.Any()) return comp;
            var outAmount = bd.Results.FirstOrDefault().Amount;
            var normalizedOutAmount = outAmount.RawValue / 1000000f;

            foreach (var prerequisite in bd.Prerequisites)
            {
                if (!recurseToOres || prerequisite.Id.TypeId == typeof(MyObjectBuilder_Ore))
                {
                    var normalizedRequiredAmount = prerequisite.Amount.RawValue / 1000000f;
                    if (!comp.ContainsKey(prerequisite.Id))
                        comp.Add(prerequisite.Id, normalizedRequiredAmount / normalizedOutAmount);
                    else comp[prerequisite.Id] += normalizedRequiredAmount / normalizedOutAmount;
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

            return comp;
        }

        public static float GetProductionTimeInSeconds(MyDefinitionId definitionId)
        {
            var bd = MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(definitionId);

            if (bd != null &&
                bd.Prerequisites.Any() &&
                bd.Prerequisites.FirstOrDefault().Id.SubtypeName.Equals("Scrap"))
            {
                //iron gives Scrap, select the correct blueprint
                bd = MyDefinitionManager.Static.GetBlueprintDefinition(
                    new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), "IronOreToIngot")
                );
            }

            if (bd != null && bd.Results.Any() && bd.Prerequisites.Any())
            {
                return bd.BaseProductionTimeInSeconds;
            }

            return -1;
        }
    }
}