using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Data/Inventory Data")]

public class InventoryData_SO : ScriptableObject
{
    public List<InventoryItem> inventoryItems = new List<InventoryItem>();

    public void AddItem(ItemData_SO newItemData, int num)
    {
        bool found = false;

        if (newItemData.countable)
        {
            foreach (InventoryItem item in inventoryItems)
            {
                if (item.itemData == newItemData)
                {
                    item.num += num;
                    found = true;
                    break;
                }
            }
        }

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].itemData == null && !found)
            {
                inventoryItems[i].itemData = newItemData;
                inventoryItems[i].num += num;
                break;
            }
        }
    }

    public int GetEmptyCount()
    {
        int count = 0;
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].itemData == null)
            {
                count++;
            }
        }
        return count;
    }

    public bool CheckItemInBag(ItemData_SO requireItem)
    {
        foreach (var item in inventoryItems)
        {
            if (item.itemData == requireItem)
            {
                return true;
            }
        }
        return false;
    }


}

[System.Serializable]
public class InventoryItem
{

    public ItemData_SO itemData;
    public int num;
}