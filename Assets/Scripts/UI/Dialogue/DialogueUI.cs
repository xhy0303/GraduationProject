using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class DialogueUI : Manager<DialogueUI>
{
    [Header("BasicElements")]

    public Image icon;
    public Text mainText;
    public Button nextBut;

    public GameObject dialoguePanel;

    [Header("Options")]
    public RectTransform optionPanel;
    public OptionUI optionPrefab;

    [Header("Data")]
    public DialogueData_SO currentData;
    int currentIndex = 0;

    PlayableDirector director;

    protected override void Awake()
    {
        base.Awake();
        nextBut.onClick.AddListener(ContinueDialogue);

        director = FindObjectOfType<PlayableDirector>();
    }
    void ContinueDialogue()
    {
        if (currentIndex < currentData.dialoguePieces.Count)
        {
            var piece = currentData.dialoguePieces[currentIndex];

            if (currentData.dialoguePieces[currentIndex - 1].playTineLine)
            //currentIndex - 1 是因为在 UpdateMainDialogue 中已经 ++ 了
            {
                PlayTimeLine();
                // 不马上调用 UpdateMainDialogue，等 Timeline 播放完再调用
            }
            else
            {
                UpdateMainDialogue(piece);
            }
        }
        else
        {
            dialoguePanel.SetActive(false);
        }


    }


    public void UpdateDialogueData(DialogueData_SO data)
    {
        currentData = data;
        currentIndex = 0;
    }

    public void UpdateMainDialogue(DialoguePiece piece)
    {
        dialoguePanel.SetActive(true);
        currentIndex++;


        if (piece.image != null)
        {
            icon.enabled = true;
            icon.sprite = piece.image;
        }
        else
        {
            icon.enabled = false;
        }

        mainText.text = "";
        mainText.text = piece.text;


        if (piece.options.Count == 0 && currentData.dialoguePieces.Count > 0)
        //如果当前对话的当前句子没有分支选项且有下一句时，显示下一句按钮并更新index
        {
            nextBut.interactable = true;
            nextBut.transform.GetChild(0).gameObject.SetActive(true);
            nextBut.gameObject.SetActive(true);

        }
        else
        {
            //nextBut.gameObject.SetActive(false);
            //如果当前对话的当前句子有分支选项，隐藏下一句按钮
            //不直接删除button，因为会影响layout的显示效果
            nextBut.transform.GetChild(0).gameObject.SetActive(false);
            nextBut.interactable = false;
        }

        //创建option
        CreateOption(piece);

    }

    void CreateOption(DialoguePiece piece)
    {
        if (optionPanel.childCount > 0)
        {
            for (int i = 0; i < optionPanel.childCount; i++)
            {
                Destroy(optionPanel.GetChild(i).gameObject);
            }
        }

        for (int i = 0; i < piece.options.Count; i++)
        {
            var option = Instantiate(optionPrefab, optionPanel);
            option.UpdateOption(piece, piece.options[i]);

        }
    }

    void PlayTimeLine()
    {
        // 隐藏 UI 与鼠标管理器
        dialoguePanel.SetActive(false);
        MouseManager.Instance.gameObject.SetActive(false);

        // 重置 Timeline 时间到起点
        director.time = 0;
        director.Evaluate();

        // 播放 Timeline
        director.Play();
        director.stopped += OnTimelineStopped;
    }


    void OnTimelineStopped(PlayableDirector obj)
    {
        // 显示 UI
        dialoguePanel.SetActive(true);
        MouseManager.Instance.gameObject.SetActive(true);

        // 播放结束后推进对话内容
        var piece = currentData.dialoguePieces[currentIndex];
        UpdateMainDialogue(piece);

        // 移除事件监听
        director.stopped -= OnTimelineStopped;
    }


}
