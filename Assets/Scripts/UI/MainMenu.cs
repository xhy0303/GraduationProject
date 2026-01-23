using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button newGameBut;
    public Button continueBut;
    public Button quitBut;

    private void Awake()
    {
        newGameBut.onClick.AddListener(NewGame);
        continueBut.onClick.AddListener(ContinueGame);
        quitBut.onClick.AddListener(ExitGame);
    }

    void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit Game");
    }

    void ContinueGame()
    {
        Debug.Log("Continue Game");
        //切换场景
        //读取进度
        //如果第一次游戏就死了接着继续游戏
        //或者没有进度的时候就继续游戏会有报错
        if(SaveManager.Instance.SceneName == "")
        {
            Debug.Log("没有存档");
            //直接开始新游戏
            NewGame();
            return;
        }
        SceneController.Instance.Transition2LoadGame();

    }

    void NewGame()
    {
        PlayerPrefs.DeleteAll();
        SaveManager.Instance.ResetGameData();

        //切换场景
        SceneController.Instance.Transition2FirstLevel();

        Debug.Log("New Game");
    }


}
