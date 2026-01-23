using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Net.Http.Headers;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.Universal;
using Unity.VisualScripting;

public class GameManager : Manager<GameManager>
{
    public PlayerStats playerStats;

    private CinemachineFreeLook followCam;
    private CinemachineVirtualCamera virtualCamera;

    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();

    public GeneralGameData_SO gameDataTemplate;
    public GeneralGameData_SO gameData { get; private set; }


    // 游戏暂停状态（其他脚本可读不可改）
    public bool IsPaused { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (gameDataTemplate != null)
        {
            gameData = Instantiate(gameDataTemplate);
        }
        DontDestroyOnLoad(this);

    }

    // 一键切换暂停/恢复
    public void TogglePause(bool pause)
    {
        IsPaused = pause;

        // 核心暂停逻辑
        Time.timeScale = pause ? 0 : 1;
        //Cursor.lockState = pause ? CursorLockMode.None : CursorLockMode.Locked;
        //Cursor.visible = pause;

        // 可选：暂停时静音所有音频（按需取消注释）
        // AudioListener.pause = pause;
    }

    public void RigisterPlayer(PlayerStats player)
    {
        playerStats = player;

        followCam = GameObject.FindObjectOfType<CinemachineFreeLook>();
        virtualCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();

        if (followCam != null)
        {
            followCam.Follow = playerStats.transform;
            followCam.LookAt = playerStats.transform;
        }
        if (virtualCamera != null)
        {
            virtualCamera.Follow = playerStats.transform;
            if (!virtualCamera.GetComponent<VirtualCamController>()) 
            {
                virtualCamera.gameObject.AddComponent<VirtualCamController>();
            }
        }
        //if( followCam == null)
        //{
        //    followCam = new CinemachineFreeLook();
        //}

        //followCam.Follow = playerStats.transform.GetChild(2);
        //followCam.LookAt = playerStats.transform.GetChild(2);

        //每次切换场景后，在注册玩家的同时加载背包数据并更新UI
        //InventoryManager.Instance.LoadInventoryData();

        //取消在此处加载背包数据，会影响装备配饰相关功能，改在场景切换时加载
        InventoryManager.Instance.RefreshUI();
    }

    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }

    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }

    public void NotifyObservers()
    {
        foreach (var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }

    public Transform GetEntrance(string sceneName)
    {
        switch(sceneName)
        {
            case "Main":
                foreach (var item in FindObjectsOfType<TransitionDestination>())
                {
                    if (item.destinationTag == TransitionDestination.DestinationTag.BirthPos)
                    {
                        return item.transform;
                    }
                }
                break;

            case "FirstFloor":
                foreach (var item in FindObjectsOfType<TransitionDestination>())
                {
                    if (item.destinationTag == TransitionDestination.DestinationTag.FrontGateIndoor)
                    {
                        return item.transform;
                    }
                }
                break;

            case "SecondFloor":
                foreach (var item in FindObjectsOfType<TransitionDestination>())
                {
                    if (item.destinationTag == TransitionDestination.DestinationTag.SecondFloorDOWN)
                    {
                        return item.transform;
                    }
                }
                break;

            case "ThirdFloor":
                foreach (var item in FindObjectsOfType<TransitionDestination>())
                {
                    if (item.destinationTag == TransitionDestination.DestinationTag.ThirdFloor)
                    {
                        return item.transform;
                    }
                }
                break;

            case "Dungon":
                foreach (var item in FindObjectsOfType<TransitionDestination>())
                {
                    if (item.destinationTag == TransitionDestination.DestinationTag.DungonIndoor)
                    {
                        return item.transform;
                    }
                }
                break;

        }
        return null;
    }


}
