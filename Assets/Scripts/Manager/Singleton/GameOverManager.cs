using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : Manager<GameOverManager>
{
    GameObject gameOverPanel;

    GameObject winText;
    GameObject loseText;

    Button backHomeBut;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        gameOverPanel=transform.GetChild(0).gameObject;

        winText = gameOverPanel.transform.GetChild(0).gameObject;
        loseText = gameOverPanel.transform.GetChild(1).gameObject;
        backHomeBut = gameOverPanel.transform.GetChild(2).GetComponent<Button>();
        backHomeBut.onClick.AddListener(SceneController.Instance.Back2Home);
    }

    public void ShowGameOver(bool won)
    {
        gameOverPanel.SetActive(true);
        if (won)
        {
            winText.SetActive(true);
            loseText.SetActive(false);
        }
        else
        {
            winText.SetActive(false);
            loseText.SetActive(true);
        }
    }

}
