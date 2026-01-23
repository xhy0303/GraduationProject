using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungonDoor : TransitionPoint
{
    protected override void OnTriggerStay(Collider other)
    {
        if(other.tag== "Player")
        {
            if (GetComponent<LockController>().CanUnlock())
            {
                canTransit = true;
                GameManager.Instance.playerStats.gameObject.GetComponent<InteractUI>().
                    UpdateHintUI(canTransit, "按E打开");
            }
            else
            {
                GameManager.Instance.playerStats.gameObject.GetComponent<InteractUI>().
                    UpdateHintUI(true, "需要钥匙");
            }
        }
    }
}
