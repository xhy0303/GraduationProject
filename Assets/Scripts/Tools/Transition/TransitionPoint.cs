using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionPoint : MonoBehaviour
{
    public enum TransitionType
    {
        DifferentScene, SameScene
    }

    [Header("Transition Info")]

    public string sceneName;
    public TransitionType transitionType;

    public TransitionDestination.DestinationTag destinationTag;

    public bool canTransit;

    protected virtual void Update()
    {
        if (canTransit && Input.GetKeyDown(KeyCode.E))
        {
            //TODO:传送
            SceneController.Instance.Transition2Destination(this);
        }
    }


    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            canTransit = true;
            GameManager.Instance.playerStats.gameObject.GetComponent<InteractUI>().
                UpdateHintUI(canTransit, "按E传送");
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            canTransit = false;
            GameManager.Instance.playerStats.gameObject.GetComponent<InteractUI>().UpdateHintUI(canTransit, "按E传送");
        }
    }
}
