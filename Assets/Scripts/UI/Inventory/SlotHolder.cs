using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public enum SlotType
{
    Bag, Weapon, Shield, Accessory, Shop
}

public class SlotHolder : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemUI itemUI;
    public SlotType slotType;



    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount % 2 == 0)
        {
            UseItem();
        }

    }

    public void UseItem()
    {
        if (itemUI.GetItem() != null)
        {
            if (itemUI.GetItem().itemType 
                == ItemType.Useable && itemUI.Bag.inventoryItems[itemUI.index].num > 0)
            {
                GameManager.Instance.playerStats.ApplyHealth(
                    itemUI.GetItem().useableItemData.healthPoint);
                itemUI.Bag.inventoryItems[itemUI.index].num--;

                //更新任务物品进度
               // QuestManager.Instance.UpdateQuestProgress(itemUI.GetItem().name, -1);

            }
        }
        UpdateItem();
    }

    public void UpdateItem()
    {

        switch (slotType)
        {
            case SlotType.Bag:
                itemUI.Bag = InventoryManager.Instance.inventoryData;
                break;

            case SlotType.Weapon:
                itemUI.Bag = InventoryManager.Instance.weaponData;
                if (itemUI.Bag.inventoryItems[itemUI.index].itemData != null)
                {
                    GameManager.Instance.playerStats.ChangeWeapon(itemUI.Bag.inventoryItems[itemUI.index].itemData);
                }
                else
                {
                    //GameManager.Instance.playerStats.UnEquipWeapon();
                    //会由于player开始时未生成而报错
                    if (GameManager.Instance.playerStats != null)
                    {
                        GameManager.Instance.playerStats.UnEquipWeapon();
                    }
                    else
                    {
                        Debug.LogError("playerStats 未初始化！");
                    }

                }
                break;

            case SlotType.Shield:
                itemUI.Bag = InventoryManager.Instance.shieldData;
                if (itemUI.Bag.inventoryItems[itemUI.index].itemData != null)
                {
                    GameManager.Instance.playerStats.ChangeShield(itemUI.Bag.inventoryItems[itemUI.index].itemData);
                    //在manager生成的时候player还未注册，会导致RefreshUI无法成功执行
                }
                else
                {
                    //GameManager.Instance.playerStats.UnEquipShield();
                    if (GameManager.Instance.playerStats != null)
                    {
                        GameManager.Instance.playerStats.UnEquipShield();
                    }
                    else
                    {
                        Debug.LogError("playerStats 未初始化！");
                    }

                }
                break;

            case SlotType.Accessory:
                itemUI.Bag = InventoryManager.Instance.accessoryData;
                if (itemUI.Bag.inventoryItems[itemUI.index].itemData != null)
                {
                    GameManager.Instance.playerStats.ChangeAccessory(
                        itemUI.Bag.inventoryItems[itemUI.index].itemData);
                    //在manager生成的时候player还未注册，会导致RefreshUI无法成功执行
                    //修改：在注册玩家的时候refreshUI
                }
                else
                {
                    if (GameManager.Instance.playerStats != null)
                    {
                        GameManager.Instance.playerStats.UnEquipAccessory();
                    }
                    else
                    {
                        Debug.LogError("playerStats 未初始化！");
                    }

                }

                break;


            case SlotType.Shop:
                itemUI.Bag = InventoryManager.Instance.accessoryShopData;


                break;

        }
        var item = itemUI.Bag.inventoryItems[itemUI.index];
        itemUI.SetUpItemUI(item.itemData, item.num);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemUI.GetItem())
        {
            InventoryManager.Instance.toolTip.SetUpItemToolTip(itemUI.GetItem());
            InventoryManager.Instance.toolTip.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryManager.Instance.toolTip.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        InventoryManager.Instance.toolTip.gameObject.SetActive(false);
    }
}
