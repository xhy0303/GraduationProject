using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockController : MonoBehaviour
{
    public ItemData_SO keyData;

    public bool CanUnlock()
    {
        if (keyData != null)
            return InventoryManager.Instance.inventoryData.CheckItemInBag(keyData);
        return false;
    }
}
