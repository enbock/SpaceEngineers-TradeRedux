using Sandbox.Game.Entities;
using VRage.Game;
using VRage.ObjectBuilders;

namespace TradeEngineers.Inventory
{
    /// <summary>
    /// This is a Wrapper Class around the Space Engineers Inventory API - Just call the public static Methods on container blocks
    /// </summary>
    public static class InventoryApi
    {
        static double multi = 1000000; //1000000
        /// <summary>
        /// Add an item to an inventory
        /// </summary>
        /// <param name="inventory">Cubeblock that has an inventory (if multiple inventories like an assembler, the first inventory is chosen)</param>
        /// <param name="itemDefinition">Item Definition</param>
        /// <param name="amount">how many of item should be added</param>
        /// <returns>Amount of pieces actually added</returns>
        public static double AddToInventory(VRage.Game.ModAPI.Ingame.IMyCubeBlock inventory, MyDefinitionId itemDefinition, double amount)
        {
            var entity = (inventory as VRage.Game.Entity.MyEntity);

            var firstInventory = entity.GetInventory(0);

            return AddToInventory(firstInventory, itemDefinition, amount);
        }
               
        private static double AddToInventory(VRage.Game.ModAPI.IMyInventory inventory, MyDefinitionId itemDefinition, double amount)
        {
            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(itemDefinition);
            MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = new VRage.MyFixedPoint() { RawValue = (long)(amount * multi) }, PhysicalContent = content };

            if (inventory.CanItemsBeAdded(inventoryItem.Amount, itemDefinition))
            {
                inventory.AddItems(inventoryItem.Amount, inventoryItem.PhysicalContent, -1);
                return amount;
            }

            return 0;
        }

        /// <summary>
        /// Remove an item from an inventory
        /// </summary>
        /// <param name="inventory">Cubeblock that has an inventory (if multiple inventories like an assembler, the first inventory is chosen)</param>
        /// <param name="itemDefinition">Item Definition</param>
        /// <param name="amount">how many of item should be removed at maximum</param>
        /// <returns>Amount of pieces actually removed</returns>
        public static double RemoveFromInventory(VRage.Game.ModAPI.Ingame.IMyCubeBlock inventory, MyDefinitionId itemDefinition, double amount)
        {
            var entity = (inventory as VRage.Game.Entity.MyEntity);

            var firstInventory = entity.GetInventory(0);

            return RemoveFromInventory(firstInventory, itemDefinition, amount);
        }

        public static double RemoveFromInventory(VRage.Game.ModAPI.IMyInventory inventory, MyDefinitionId itemDefinition, double amount)
        {
            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(itemDefinition);
            MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = new VRage.MyFixedPoint()  { RawValue = (long)(amount * multi) }, PhysicalContent = content };
                        
            if (inventory.GetItemAmount(itemDefinition) >= inventoryItem.Amount)
            {
                inventory.RemoveItemsOfType(inventoryItem.Amount, inventoryItem.PhysicalContent);

                return amount;
            }
            else if (inventory.GetItemAmount(itemDefinition) > 0)
            {
                var itemsAmount = inventory.GetItemAmount(itemDefinition);
                inventory.RemoveItemsOfType(itemsAmount, inventoryItem.PhysicalContent);                

                return itemsAmount.RawValue == 0 ? 0 : (double)itemsAmount.RawValue / multi;
            }            
            return 0;
        }

        /// <summary>
        /// Count items of type in inventory
        /// </summary>
        /// <param name="inventory">Cubeblock that has an inventory (if multiple inventories like an assembler, the first inventory is chosen)</param>
        /// <param name="itemDefinition">Item Definition</param>
        /// <returns>Amount of items of given type in target inventory</returns>
        public static double CountItemsInventory(VRage.Game.ModAPI.Ingame.IMyCubeBlock inventory, MyDefinitionId itemDefinition)
        {
            var entity = (inventory as VRage.Game.Entity.MyEntity);

            var firstInventory = entity.GetInventory(0);

            return CountItemsInventory(firstInventory, itemDefinition);
        }
        public static double CountItemsInventory(VRage.Game.ModAPI.IMyInventory inventory, MyDefinitionId itemDefinition)
        {
            var itemsAmount = inventory.GetItemAmount(itemDefinition);

            return itemsAmount.RawValue == 0 ? 0 : (double)itemsAmount.RawValue / multi;
        }
    

        /// <summary>
        /// Debug Method to find what is detected in a container
        /// </summary>
        /// <param name="inventory">Cubeblock that has an inventory (if multiple inventories like an assembler, the first inventory is chosen)</param>
        public static void ListItemsInventory(VRage.Game.ModAPI.Ingame.IMyCubeBlock inventory)
        {
            var entity = (inventory as VRage.Game.Entity.MyEntity);

            var firstInventory = entity.GetInventory(0);

            ListItemsInventory(firstInventory);
        }
        private static void ListItemsInventory(VRage.Game.ModAPI.IMyInventory inventory)
        {
            var items = inventory.GetItems();

            foreach(var item in items)
            {
                Sandbox.ModAPI.MyAPIGateway.Utilities.ShowMessage("skl", "Item "+item.ItemId + ", Amount: "+item.Amount + ", Type:"+item.Content);
            }            
        }

        public static bool AreInventoriesConnected(VRage.Game.ModAPI.Ingame.IMyCubeBlock inventory, VRage.Game.ModAPI.Ingame.IMyCubeBlock otherInventory)
        {
            var blockEntity1 = (inventory as VRage.Game.Entity.MyEntity);
            var blockEntity2 = (otherInventory as VRage.Game.Entity.MyEntity);

            var inventory1 = (blockEntity1.GetInventory(0) as VRage.Game.ModAPI.Ingame.IMyInventory);
            var inventory2 = (blockEntity2.GetInventory(0) as VRage.Game.ModAPI.Ingame.IMyInventory);

            if(inventory1 != null && inventory2 != null)
                return inventory1.IsConnectedTo(inventory2);

            return false;
        }
    }
}
