using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogueController : DialogueController
{
    public DialogueData_SO newGameIntro;
    public DialogueData_SO continueGameIntro;
    public DialogueData_SO olderGamerIntro;

    private void Awake()
    {
        if (GameManager.Instance.gameData.gameTime == 0)
        {
            currentDialogue = newGameIntro;
        }
        else if (GameManager.Instance.gameData.gameTime == 1) 
        {
            currentDialogue = continueGameIntro;
        }
        else
        {
            currentDialogue = olderGamerIntro;
        }
    }

}
