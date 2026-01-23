using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedChestController : ChestController
{
    public ItemData_SO keyData;

    protected override void OnTriggerEnter(Collider other)
    {
        if(keyData!=null)
        {
            if (other.CompareTag("Player")
            && !isOpen)
            {
                if(InventoryManager.Instance.inventoryData.CheckItemInBag(keyData))
                {
                    canOpen = true;
                    GameManager.Instance.playerStats.GetComponent<InteractUI>().UpdateHintUI(true, "按E打开");
                }
                else
                {
                    GameManager.Instance.playerStats.GetComponent<InteractUI>().UpdateHintUI(true, "需要钥匙");
                }
                
            }
        }
    }


}
