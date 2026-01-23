using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    bool canOpen = false;

    //public bool chosen = false;

    private void Update()
    {
        if (canOpen 
            && Input.GetKeyDown(KeyCode.B) 
            //&& !chosen)  
            &&GameManager.Instance.gameData.CanShop(
                InventoryManager.Instance.accessoryShopData.GetEmptyCount()))
        {
            InventoryManager.Instance.shopPanel.SetActive(canOpen);
            //chosen = true;
            //不应该在这里设置选择与否，而在InventoryManager里设置
        }

        //if (chosen)
        //    //选择了饰品以后关闭商店面板
        //{
        //    InventoryManager.Instance.shopPanel.SetActive(false);
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")
            //&& !chosen) 
            && GameManager.Instance.gameData.CanShop(
                InventoryManager.Instance.accessoryShopData.GetEmptyCount()))
        {
            canOpen = true;
            GameManager.Instance.playerStats.GetComponent<InteractUI>().UpdateHintUI(true, "按B打开商店");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canOpen = false;
            GameManager.Instance.playerStats.GetComponent<InteractUI>().UpdateHintUI(false, "");
            InventoryManager.Instance.shopPanel.SetActive(canOpen);
        }
    }

}
