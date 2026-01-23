using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    public Text optionText;
    public Button thisBut;

    DialoguePiece currentPiece;
    string nextPieceID;

    private void Awake()
    {
        thisBut = GetComponent<Button>();
        thisBut.onClick.AddListener(OnOptionClicked);
    }

    public void UpdateOption(DialoguePiece piece, DialogueOption option)
    {
        currentPiece = piece;
        optionText.text = option.text;
        nextPieceID = option.targetID;

        //takeQuest = option.takeQuest;
    }

    public void OnOptionClicked()
    {
        //if (currentPiece.quest != null)
        //{
        //    var newTask = new QuestManager.QuestTask
        //    {
        //        questData = Instantiate(currentPiece.quest)
        //    };

        //    if (takeQuest)
        //    {
        //        //添加到任务列表
        //        //判断是否已经有此任务（一名字判断
        //        if (QuestManager.Instance.HasQuest(newTask.questData))
        //        {
        //            //判断是否完成并发放奖励
        //            if (QuestManager.Instance.GetTask(newTask.questData).IsCompleted)
        //            {
        //                newTask.questData.GiveRewards();
        //                QuestManager.Instance.GetTask(newTask.questData).IsFinished = true;
        //            }

        //        }
        //        else
        //        {
        //            //没有任务,添加到任务列表中
        //            QuestManager.Instance.tasks.Add(newTask);
        //            QuestManager.Instance.GetTask(newTask.questData).IsStarted = true;

        //            foreach (var requireItem in newTask.questData.RequireTargetNames())
        //            {
        //                InventoryManager.Instance.CheckQuestItemInBag(requireItem);
        //            }
        //        }
        //    }
        //}

        if (nextPieceID == "")
        {
            DialogueUI.Instance.dialoguePanel.SetActive(false);
            return;
        }
        else
        {
            DialogueUI.Instance.UpdateMainDialogue(
                DialogueUI.Instance.currentData.dialogueIndex[nextPieceID]);
            //for (int i = 0; i < DialogueUI.Instance.currentData.dialoguePieces.Count; i++) 
            //{
            //    if(DialogueUI.Instance.currentData.dialogueIndex[nextPieceID] 
            //        == DialogueUI.Instance.currentData.dialoguePieces[i])
            //    {
            //        DialogueUI.Instance.currentIndex = i;
            //    }
            //}
        }
    }

}
