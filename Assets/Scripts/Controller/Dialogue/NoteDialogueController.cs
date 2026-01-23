using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteDialogueController : DialogueController
{
    [Header("NoteDialogueData")]
    public DialogueData_SO noteContent;

    private void Awake()
    {
        currentDialogue = Instantiate(noteContent);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")
            && currentDialogue != null
            && GameManager.Instance.playerStats.exposureState == PlayerStats.ExposureState.Peace)
        {
            canTalk = true;
            GameManager.Instance.playerStats.gameObject.GetComponent<InteractUI>().UpdateHintUI(canTalk, "°´E²é¿´");
        }
    }

}
