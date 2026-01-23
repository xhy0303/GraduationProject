using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyContainerController : ChestController
{
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")
            && !isOpen
            && GameManager.Instance.playerStats.currentShape == PlayerStats.PlayerShape.Yokai) 
        {
            canOpen = true;
            GameManager.Instance.playerStats.GetComponent<InteractUI>().UpdateHintUI(true, "°´E´ò¿ª");
        }
    }
}
