using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ItemUI))]

public class DragItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    ItemUI currentItemUI;

    SlotHolder currentHolder;
    SlotHolder targetHolder;


    private void Awake()
    {
        currentItemUI = GetComponent<ItemUI>();
        currentHolder = GetComponentInParent<SlotHolder>();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        InventoryManager.Instance.currentDrag = new InventoryManager.DragData();
        InventoryManager.Instance.currentDrag.originalHolder = GetComponentInParent<SlotHolder>();
        InventoryManager.Instance.currentDrag.originalParent = (RectTransform)transform.parent;


        //记录原始信息
        transform.SetParent(InventoryManager.Instance.dragCanvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //跟随鼠标位置移动
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //放下物品，交换数据
        if (EventSystem.current.IsPointerOverGameObject())//鼠标是否指向UI物品
        {
            if (InventoryManager.Instance.CheckInventoryUI(eventData.position)
                || InventoryManager.Instance.CheckWeaponUI(eventData.position)
                || InventoryManager.Instance.CheckShieldUI(eventData.position)
                || InventoryManager.Instance.CheckAccessoryUI(eventData.position)
                || InventoryManager.Instance.CheckShopUI(eventData.position))  
            {
                if (eventData.pointerEnter.gameObject.GetComponent<SlotHolder>())
                {
                    targetHolder = eventData.pointerEnter.gameObject.GetComponent<SlotHolder>();
                }
                else
                {
                    targetHolder = eventData.pointerEnter.gameObject.GetComponentInParent<SlotHolder>();
                }

                if (targetHolder != null) 
                {

                    if (targetHolder != InventoryManager.Instance.currentDrag.originalHolder)
                    {

                        switch (targetHolder.slotType)
                        {
                            case SlotType.Bag:
                                if (currentHolder.slotType == SlotType.Bag)
                                    //如果时背包之间来回交换
                                    SwapItem();
                                else
                                {
                                    //背包和其他栏位交换
                                    if (targetHolder.itemUI.Bag.inventoryItems[targetHolder.itemUI.index].itemData == null)
                                    //背包目标栏位为空
                                    {
                                        if (currentHolder.slotType == SlotType.Shop
                                            && GameManager.Instance.gameData.CanShop(InventoryManager.Instance.accessoryShopData.GetEmptyCount()))
                                        //商店到背包的话，就要确保从商店中取出的物品没有超过上限（第二次开始游戏只能拿一个饰品)
                                        {
                                            SwapItem();
                                        }
                                        else if (currentHolder.slotType != SlotType.Shop) 
                                        {
                                            SwapItem();
                                        }
                                    }
                                }
                                break;

                            case SlotType.Weapon:
                                if (currentItemUI.Bag.inventoryItems[currentItemUI.index].itemData.itemType
                                    == ItemType.Weapon)
                                    SwapItem();
                                break;

                            case SlotType.Shield:
                                if (currentItemUI.Bag.inventoryItems[currentItemUI.index].itemData.itemType
                                    == ItemType.Shield)
                                    SwapItem();
                                break;

                            case SlotType.Accessory:
                                if (currentItemUI.Bag.inventoryItems[currentItemUI.index].itemData.itemType
                                    == ItemType.Accessory)
                                    SwapItem();
                                break;

                            case SlotType.Shop:
                                //只有背包里的饰品可以放到商店
                                if (currentHolder.slotType == SlotType.Bag
                                    && currentItemUI.Bag.inventoryItems[currentItemUI.index].itemData.itemType
                                    == ItemType.Accessory) 
                                    SwapItem();
                                break;

                            default:
                                break;

                        }
                    }
                    currentHolder.UpdateItem();
                    targetHolder.UpdateItem();
                }
                
                
            }
        }

        transform.SetParent(InventoryManager.Instance.currentDrag.originalParent);

        //重置ItemSlot的位置
        RectTransform t = transform as RectTransform;
        t.offsetMax = -Vector2.one * 5;
        t.offsetMin = Vector2.one * 5;
    }

    public void SwapItem()
    {
        var targetItem = targetHolder.itemUI.Bag.inventoryItems[targetHolder.itemUI.index];
        var tempItem = currentHolder.itemUI.Bag.inventoryItems[currentHolder.itemUI.index];
        //不单是UI的交换，也是背包数据库的交换



        bool isSameItem = tempItem.itemData == targetItem.itemData;


        if (isSameItem && targetItem.itemData.countable)
        {
            targetItem.num += tempItem.num;
            tempItem.itemData = null;
            tempItem.num = 0;
        }
        else
        {
            currentHolder.itemUI.Bag.inventoryItems[currentHolder.itemUI.index] = targetItem;
            currentHolder.itemUI.Bag.inventoryItems[currentHolder.itemUI.index].num = targetItem.num;
            targetHolder.itemUI.Bag.inventoryItems[targetHolder.itemUI.index] = tempItem;
            targetHolder.itemUI.Bag.inventoryItems[targetHolder.itemUI.index].num = tempItem.num;
        }
    }

}
