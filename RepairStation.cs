using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = System.Object;

namespace RepairStation;

public class RepairStation : MonoBehaviour, Hoverable, Interactable
{
    internal static string? ItemName;
    public List<ItemDrop.ItemData> m_tempWornItemsHover = new();
    internal static int CostAmount;

    public string GetHoverText()
    {
        GetWornItemsHover(m_tempWornItemsHover);
        CostAmount = RepairStationPlugin.UseItemMultiplier.Value == RepairStationPlugin.Toggle.On &&
                     RepairStationPlugin.RepairAllItems.Value == RepairStationPlugin.Toggle.On
            ? RepairStationPlugin.Cost.Value * m_tempWornItemsHover.Count
            : RepairStationPlugin.Cost.Value;
        StringBuilder stringBuilder = new();
        stringBuilder.Append(Localization.instance.Localize(
            $"{GetHoverName()}{Environment.NewLine}Press [<color=yellow><b>$KEY_Use</b></color>] to repair everything in your inventory" +
            (RepairStationPlugin.ShouldCost.Value == RepairStationPlugin.Toggle.On
                ? $" (Uses {CostAmount} {RepairStationPlugin.RepairItem.Value})"
                : "")
        ));

        return stringBuilder.ToString();
    }

    public string GetHoverName() => Localization.instance.Localize("$piece_repairstation");


    public bool Interact(Humanoid character, bool hold, bool alt)
    {
        if (hold)
            return false;
        if (!CheckRepairCost(costValue: CostAmount))
        {
            ItemDrop? item = ZNetScene.instance.GetPrefab(RepairStationPlugin.RepairItem.Value)
                .GetComponent<ItemDrop>();
            if (!item) return false;
            var amountPlayerHas = Player.m_localPlayer.GetInventory().CountItems(item.m_itemData.m_shared.m_name);
            var costAmountHeld = CostAmount;
            Player.m_localPlayer.Message(MessageHud.MessageType.Center,
                $"Required Items Needed : {costAmountHeld - amountPlayerHas} {ItemName}");
            return false;
        }

        if (!HaveRepairableItems())
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "No items to repair");
        }
        else
        {
            int repairCount = 0;
            if (RepairStationPlugin.RepairAllItems.Value == RepairStationPlugin.Toggle.On)
            {
                while (HaveRepairableItems())
                {
                    RepairItems();
                    ++repairCount;
                }
            }
            else
            {
                RepairItems();
                ++repairCount;
            }

            if (repairCount <= 0) return true;
            var costValue = RepairStationPlugin.UseItemMultiplier.Value == RepairStationPlugin.Toggle.On
                ? RepairStationPlugin.Cost.Value * repairCount
                : RepairStationPlugin.Cost.Value;
            CheckRepairCost(true, costValue);
            RepairStationPlugin.craftingStationClone.m_repairItemDoneEffects.Create(transform.position,
                Quaternion.identity);
        }


        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;

    public bool HaveRepairableItems()
    {
        if (Player.m_localPlayer == null)
            return false;
        InventoryGui.instance.m_tempWornItems.Clear();
        Player.m_localPlayer.GetInventory().GetWornItems(InventoryGui.instance.m_tempWornItems);
        foreach (ItemDrop.ItemData tempWornItem in InventoryGui.instance.m_tempWornItems)
        {
            if (CanRepairItems(tempWornItem))
                return true;
        }

        return false;
    }

    public bool CanRepairItems(ItemDrop.ItemData item)
    {
        if (Player.m_localPlayer == null || !item.m_shared.m_canBeReparied)
            return false;
        if (Player.m_localPlayer.NoCostCheat())
            return true;
        // TODO: Add a gating system for this
        //Recipe recipe = ObjectDB.instance.GetRecipe(item);
        // return !(recipe == null) && (!(recipe.m_craftingStation == null) || !(recipe.m_repairStation == null)) && (recipe.m_repairStation != null && recipe.m_repairStation.m_name == currentCraftingStation.m_name || (Object) recipe.m_craftingStation != null && recipe.m_craftingStation.m_name == currentCraftingStation.m_name) && currentCraftingStation.GetLevel() >= recipe.m_minStationLevel;
        return true;
    }

    public void RepairItems()
    {
        if (Player.m_localPlayer == null)
            return;
        InventoryGui.instance.m_tempWornItems.Clear();
        m_tempWornItemsHover.Clear();
        Player.m_localPlayer.GetInventory().GetWornItems(InventoryGui.instance.m_tempWornItems);
        //CraftingStation craftingStationClone = null;

        foreach (ItemDrop.ItemData tempWornItem in InventoryGui.instance.m_tempWornItems)
        {
            if (CanRepairItems(tempWornItem))
            {
                tempWornItem.m_durability = tempWornItem.GetMaxDurability();
                if (RepairStationPlugin.craftingStationClone != null)
                    RepairStationPlugin.craftingStationClone.m_repairItemDoneEffects.Create(transform.position, Quaternion.identity);
                Player.m_localPlayer.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize("$msg_repaired", tempWornItem.m_shared.m_name));
                return;
            }
        }

        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "No more items to repair");
    }

    internal static bool CheckRepairCost(bool shouldRemove = false, int costValue = 1)
    {
        if (RepairStationPlugin.ShouldCost.Value == RepairStationPlugin.Toggle.Off) return true;
        ItemDrop? item = ZNetScene.instance.GetPrefab(RepairStationPlugin.RepairItem.Value)
            .GetComponent<ItemDrop>();
        if (!item) return false;
        ItemName = Localization.instance.Localize(item.m_itemData.m_shared.m_name);
        if (Player.m_localPlayer.GetInventory().CountItems(item.m_itemData.m_shared.m_name) >=
            costValue)
        {
            if (shouldRemove)
            {
                RepairStationPlugin.RepairStationLogger.LogError($"Removing Items {costValue}");
                Player.m_localPlayer.GetInventory().RemoveItem(item.m_itemData.m_shared.m_name,
                    costValue);
                Player.m_localPlayer.ShowRemovedMessage(item.m_itemData,
                    costValue);
            }

            return true;
        }

        return false;
    }

    public void GetWornItemsHover(List<ItemDrop.ItemData> worn)
    {
        foreach (ItemDrop.ItemData itemData in Player.m_localPlayer.GetInventory().GetAllItems())
        {
            if (!itemData.m_shared.m_useDurability || !(itemData.m_durability < (double)itemData.GetMaxDurability())) continue;
            if (worn.Contains(itemData)) continue;
            if(CanRepairItems(itemData))
                worn.Add(itemData);
        }
    }
}