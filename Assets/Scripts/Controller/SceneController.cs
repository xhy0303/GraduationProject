using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SceneController : Manager<SceneController>
{
    public GameObject playerPrefab;

    bool fadeFinished;

    private GameObject player;
    private NavMeshAgent playerAgent;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void Transition2Destination(TransitionPoint transitionPoint)
    {
        switch (transitionPoint.transitionType)
        {

            case TransitionPoint.TransitionType.SameScene:
                StartCoroutine(Transition(
                                SceneManager.GetActiveScene().name,
                                transitionPoint.destinationTag));
                break;
            case TransitionPoint.TransitionType.DifferentScene:
                StartCoroutine(Transition(
                                transitionPoint.sceneName, 
                                transitionPoint.destinationTag));
                break;


        }
    }

    IEnumerator Transition(
        string sceneName, 
        TransitionDestination.DestinationTag destinationTag)
    {
        //数据保存
        SaveManager.Instance.SavePlayerData();//玩家数据
        InventoryManager.Instance.SaveInventoryData();//背包数据
        foreach (var objManager in FindObjectsOfType<InteractableObjectManager>())
            //场景中可交互物品数据，不用手动记载，在启动时自动加载
        {
            objManager.SaveObjectStates();
        }

        //QuestManager.Instance.SaveQuestManager();

        if (sceneName != SceneManager.GetActiveScene().name)//跨场景传送
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return Instantiate(playerPrefab, 
                FindDestination(destinationTag).transform.position,
                FindDestination(destinationTag).transform.rotation);

            //传送以后自动加载数据
            SaveManager.Instance.LoadPlayerData();
            SaveManager.Instance.LoadInventoryData();

            InventoryManager.Instance.RefreshUI();

            UpdateBGMAccordingToScene();

            yield break;
        }
        else
        {
            player = GameManager.Instance.playerStats.gameObject;
            playerAgent = player.GetComponent<NavMeshAgent>();
            playerAgent.enabled = false;
            player.transform.SetPositionAndRotation(
                FindDestination(destinationTag).transform.position,
                FindDestination(destinationTag).transform.rotation);
            playerAgent.enabled = true;

            UpdateBGMAccordingToScene();

            yield return null;
        }

    }

    private TransitionDestination FindDestination(
        TransitionDestination.DestinationTag destinationTag)
    {
        TransitionDestination[] destinations = FindObjectsOfType<TransitionDestination>();
        foreach (TransitionDestination destination in destinations)
        {
            if (destination.destinationTag == destinationTag)
            {
                return destination;
            }
        }
        return null;
    }

    public void Transition2FirstLevel()
    //开始菜单选择新游戏
    {
        StartCoroutine(LoadLevel("Main"));
    }

    public void Transition2LoadGame()
    //开始菜单选择继续游戏
    {
        StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));

        SaveManager.Instance.LoadGeneralGameData();
    }

    public void Back2Home()
    {
        StartCoroutine(LoadLevel("Begin"));
    }

    IEnumerator LoadLevel(string sceneName)
        //从开始菜单加载游戏
    {
        yield return SceneManager.LoadSceneAsync(sceneName);

        if (sceneName != "Begin")
        {
            yield return player = Instantiate(playerPrefab,
            GameManager.Instance.GetEntrance(sceneName).position,
            GameManager.Instance.GetEntrance(sceneName).rotation);

            SaveManager.Instance.LoadPlayerData();
            SaveManager.Instance.LoadInventoryData();
            SaveManager.Instance.LoadGeneralGameData();

            //不需要在这里保存数据
            InventoryManager.Instance.RefreshUI();
        }

        


        UpdateBGMAccordingToScene();

        yield break;
    }

    private void UpdateBGMAccordingToScene()
    {
        // 简单判断是否为室内场景（可根据你实际情况修改）
        string sceneName = SceneManager.GetActiveScene().name;
        bool isIndoor = sceneName != "Begin" && sceneName != "Main";
        //除开始菜单和主场景之外都算室内

        var state = GameManager.Instance.playerStats.exposureState;
        BGMManager.Instance.UpdateBGM(isIndoor, state);
    }


}
