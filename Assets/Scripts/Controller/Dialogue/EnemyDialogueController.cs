using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDialogueController : DialogueController
{
    [Header("EnemyDialogueData")]
    public DialogueData_SO regularDialogue;
    public DialogueData_SO alertedDialogue;

    private void Awake()
    {
        SetRegularDialogue();
    }

    public void SetAlertedDialogue()
    {
        currentDialogue = alertedDialogue;
    }
    public void SetRegularDialogue()
    {
        currentDialogue = regularDialogue;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")
            && currentDialogue != null
            && GameManager.Instance.playerStats.currentShape == PlayerStats.PlayerShape.Yokai) 
        {
            canTalk = true;
            GameManager.Instance.playerStats.gameObject.GetComponent<InteractUI>().
                UpdateHintUI(canTalk, "°´E½»Ì¸");
        }
    }


    protected override void Update()
    {
        base.Update();
        gameObject.GetComponent<NavMeshAgent>().isStopped = true;

        if (!DialogueUI.Instance.dialoguePanel.activeSelf)
        {
            gameObject.GetComponent<NavMeshAgent>().isStopped = false;
        }
    } 

}
