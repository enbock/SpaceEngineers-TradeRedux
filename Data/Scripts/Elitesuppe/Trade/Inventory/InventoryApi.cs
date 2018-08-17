using Sandbox.Game.Entities;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

namespace Elitesuppe.Trade.Inventory
{
    /// <summary>
    /// This is a Wrapper Class around the Space Engineers Inventory API - Just call the public static Methods on container blocks
    /// </summary>
    public static class InventoryApi
    {
        private static double _multi = 1000000; //1000000

        /// <summary>
        /// Add an item to an inventory
        /// </summary>
        /// <param name="inventory">Cubeblock that has an inventory (if multiple inventories like an assembler, the first inventory is chosen)</param>
        /// <param name="itemDefinition">Item Definition</param>
        /// <param name="amount">how many of item should be added</param>
        /// <returns>Amount of pieces actually added</returns>
        public static double AddToInventory(IMyCubeBlock inventory, MyDefinitionId itemDefinition, double amount)
        {
            var entity = (inventory as MyEntity);

            var firstInventory = entity.GetInventory(0);

            return AddToInventory(firstInventory, itemDefinition, amount);
        }

        private static double AddToInventory(IMyInventory inventory, MyDefinitionId itemDefinition, double amount)
        {
            var content = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject(itemDefinition);
            MyObjectBuilder_InventoryItem item = new MyObjectBuilder_InventoryItem
            {
                Amount = new MyFixedPoint {RawValue = (long) (amount * _multi)},
                PhysicalContent = content
            };

            if (!inventory.CanItemsBeAdded(item.Amount, itemDefinition)) return 0;

            inventory.AddItems(item.Amount, item.PhysicalContent);

            return amount;
        }

        /// <summary>
        /// Remove an item from an inventory
        /// </summary>
        /// <param name="inventory">Cubeblock that has an inventory (if multiple inventories like an assembler, the first inventory is chosen)</param>
        /// <param name="itemDefinition">Item Definition</param>
        /// <param name="amount">how many of item should be removed at maximum</param>
        /// <returns>Amount of pieces actually removed</returns>
        public static double RemoveFromInventory(IMyCubeBlock inventory, MyDefinitionId itemDefinition, double amount)
        {
            var entity = (inventory as MyEntity);

            var firstInventory = entity.GetInventory(0);

            return RemoveFromInventory(firstInventory, itemDefinition, amount);
        }

        public static double RemoveFromInventory(IMyInventory inventory, MyDefinitionId itemDefinition, double amount)
        {
            var content = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject(itemDefinition);
            MyObjectBuilder_InventoryItem item = new MyObjectBuilder_InventoryItem
            {
                Amount = new MyFixedPoint {RawValue = (long) (amount * _multi)},
                PhysicalContent = content
            };
            MyFixedPoint inventoryAmount = inventory.GetItemAmount(itemDefinition);

            if (inventoryAmount >= item.Amount)
            {
                inventory.RemoveItemsOfType(item.Amount, item.PhysicalContent);

                return amount;
            }

            if (inventoryAmount <= 0) return 0;

            inventory.RemoveItemsOfType(inventoryAmount, item.PhysicalContent);

            return inventoryAmount.RawValue == 0 ? 0 : inventoryAmount.RawValue / _multi;
        }

        /// <summary>
        /// Count items of type in inventory
        /// </summary>
        /// <param name="inventory">Cubeblock that has an inventory (if multiple inventories like an assembler, the first inventory is chosen)</param>
        /// <param name="itemDefinition">Item Definition</param>
        /// <returns>Amount of items of given type in target inventory</returns>
        public static double CountItemsInventory(IMyCubeBlock inventory, MyDefinitionId itemDefinition)
        {
            var entity = (inventory as MyEntity);

            var firstInventory = entity.GetInventory(0);

            return CountItemsInventory(firstInventory, itemDefinition);
        }

        public static double CountItemsInventory(IMyInventory inventory, MyDefinitionId itemDefinition)
        {
            var itemsAmount = inventory.GetItemAmount(itemDefinition);

            return itemsAmount.RawValue == 0 ? 0 : itemsAmount.RawValue / _multi;
        }


        /// <summary>
        /// Debug Method to find what is detected in a container
        /// </summary>
        /// <param name="inventory">Cubeblock that has an inventory (if multiple inventories like an assembler, the first inventory is chosen)</param>
        public static void ListItemsInventory(IMyCubeBlock inventory)
        {
            var entity = (inventory as MyEntity);

            var firstInventory = entity.GetInventory(0);

            ListItemsInventory(firstInventory);
        }

        private static void ListItemsInventory(IMyInventory inventory)
        {
            var items = inventory.GetItems();

            foreach (var item in items)
            {
                /*
                MyAPIGateway.Utilities.ShowMessage("skl",
                    "Item " + item.ItemId + ", Amount: " + item.Amount + ", Type:" + item.Content);
                 */
            }
        }

        public static bool AreInventoriesConnected(IMyCubeBlock inventory, IMyCubeBlock otherInventory)
        {
            var blockEntity1 = (inventory as MyEntity);
            var blockEntity2 = (otherInventory as MyEntity);

            var inventory1 = (blockEntity1.GetInventory(0) as VRage.Game.ModAPI.Ingame.IMyInventory);
            var inventory2 = (blockEntity2.GetInventory(0) as VRage.Game.ModAPI.Ingame.IMyInventory);

            if (inventory1 != null && inventory2 != null)
                return inventory1.IsConnectedTo(inventory2);

            return false;
        }
    }
}