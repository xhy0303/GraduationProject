using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    public bool isOpen = false;
    public Animator chestAnimator;
    public float epicChance = 0.2f; // 20% chance to be epic

    protected bool isEpic = false;
    protected bool canOpen = false;

    [System.Serializable]
    public class ChestItem
    {
        public ItemData_SO itemData;
        public int num;
    }

    [Header("Chest Items")]
    public ChestItem[] normalItems;
    public ChestItem[] epicItems;


    private void Awake()
    {
        chestAnimator = GetComponent<Animator>();

        isEpic = UnityEngine.Random.Range(0f, 1f) < epicChance;
        //isOpen = !GetComponent<ObjectState>().isActive;
    }

    protected virtual void Update()
    {
        if (canOpen && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = true; 
            
            AddItems2Inventory(isEpic);
            GameManager.Instance.playerStats.GetComponent<InteractUI>().UpdateHintUI(false, "");

            GetComponent<ObjectState>().isActive = false;
        }

        if (isOpen && chestAnimator != null)
        {
            chestAnimator.SetTrigger("OpenChest");
            //只有史诗物品栏有物品时才会播放史诗动画
            chestAnimator.SetBool("Epic", isEpic && epicItems.Length != 0);
        }

    }

    void AddItems2Inventory(bool isEpic)
    {
        float weight = UnityEngine.Random.Range(0f, 1f);

        if (normalItems.Length != 0)
        {
            foreach (var item in normalItems)
            {
                InventoryManager.Instance.inventoryData.AddItem(item.itemData, item.num);

            }
        }
        if (isEpic && epicItems.Length != 0) 
        {
            foreach (var item in epicItems)
            {
                InventoryManager.Instance.inventoryData.AddItem(item.itemData, item.num);
            }
        }

        InventoryManager.Instance.inventoryUI.RefreshUI();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen) 
        {
            canOpen = true;
            GameManager.Instance.playerStats.GetComponent<InteractUI>().UpdateHintUI(true, "按E打开");
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canOpen = false;
            GameManager.Instance.playerStats.GetComponent<InteractUI>().UpdateHintUI(false, "");
        }
    }

}
