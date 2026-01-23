using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image icon = null;
    public Text count = null;

    public ItemData_SO currentItemData;

    public InventoryData_SO Bag { get; set; }
    public int index { get; set; } = -1;

    public void SetUpItemUI(ItemData_SO item, int itemCount)
    {
        if (itemCount == 0)
        {
            Bag.inventoryItems[index].itemData = null;
            icon.gameObject.SetActive(false);
            return;
        }
        if (itemCount < 0)
        {
            item = null;
        }

        if (item != null)
        {
            currentItemData = item;
            icon.sprite = item.itemIcon;
            count.text = itemCount.ToString("00");

            icon.gameObject.SetActive(true);
        }
        else
        {
            icon.gameObject.SetActive(false);
        }
    }

    public ItemData_SO GetItem()
    {
        return Bag.inventoryItems[index].itemData;
    }
}
