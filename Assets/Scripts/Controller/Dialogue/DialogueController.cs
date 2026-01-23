using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public DialogueData_SO currentDialogue;
    protected bool canTalk = false;



    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")
            && currentDialogue != null
            && GameManager.Instance.playerStats.exposureState == PlayerStats.ExposureState.Peace) 
        {
            canTalk = true;
            GameManager.Instance.playerStats.gameObject.GetComponent<InteractUI>().
                UpdateHintUI(canTalk, "按E交谈");
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            DialogueUI.Instance.dialoguePanel.SetActive(false);
            canTalk = false;
            GameManager.Instance.playerStats.gameObject.GetComponent<InteractUI>().UpdateHintUI(canTalk, "");
        }
    }

    protected virtual void Update()
    {
        if (canTalk && Input.GetKeyDown(KeyCode.E) && currentDialogue != null) 
        {
            OpenDialogue();
        }

    }

    public void OpenDialogue()
    {
        //打开UI面板
        //传输对话内容

        DialogueUI.Instance.UpdateDialogueData(currentDialogue);
        DialogueUI.Instance.UpdateMainDialogue(currentDialogue.dialoguePieces[0]);
    }

}
