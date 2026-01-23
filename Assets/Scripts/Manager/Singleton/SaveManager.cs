using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : Manager<SaveManager>
{
    string sceneName = "level";

    public string SceneName
    {
        get
        {
            return PlayerPrefs.GetString(sceneName);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void Save(object data, string key)
    {
        var jsonData = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.SetString(sceneName, SceneManager.GetActiveScene().name);

        PlayerPrefs.Save();
    }

    public void Load(object data, string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            var jsonData = PlayerPrefs.GetString(key);
            JsonUtility.FromJsonOverwrite(jsonData, data);
        }
    }

    #region 数据保存
    public void SavePlayerData()
    {
        Save(GameManager.Instance.playerStats.characterData,
            GameManager.Instance.playerStats.characterData.name);
    }

    public void LoadPlayerData()
    {
        Load(GameManager.Instance.playerStats.characterData,
            GameManager.Instance.playerStats.characterData.name);
    }

    public void SaveInventoryData()
        //统一整理到SaveManager里调用
    {
        InventoryManager.Instance.SaveInventoryData();
    }
    public void LoadInventoryData()
    {
        InventoryManager.Instance.LoadInventoryData();
    }

    public void SaveGeneralGameData()
    {
        Save(GameManager.Instance.gameData, GameManager.Instance.gameData.name);
    }
    public void LoadGeneralGameData()
    //只有在开始游戏时（不管是新游戏还是读档）才会调用
    {
        Load(GameManager.Instance.gameData, GameManager.Instance.gameData.name);
    }

    #endregion

    public void SaveAllData()
    {
        SavePlayerData();
        SaveInventoryData();

        foreach (var objManager in FindObjectsOfType<InteractableObjectManager>())
        //场景中可交互物品数据，不用手动记载，在启动时自动加载
        {
            objManager.SaveObjectStates();
        }

        SaveGeneralGameData();

    }

    public void SetDataAfterWin()
    {
        //游戏胜利后，重置数据
        Save(GameManager.Instance.playerStats.templateData,
            GameManager.Instance.playerStats.characterData.name);//玩家生命值

        Save(GameManager.Instance.gameData, GameManager.Instance.gameData.name);//游戏时间

        Save(InventoryManager.Instance.inventoryTemplate,
            InventoryManager.Instance.inventoryData.name);//背包数据
        Save(InventoryManager.Instance.weaponTemplate,
            InventoryManager.Instance.weaponData.name);//武器栏数据
        Save(InventoryManager.Instance.shieldTemplate,
            InventoryManager.Instance.shieldData.name);//盾牌栏数据
        Save(InventoryManager.Instance.accessoryTemplate,
            InventoryManager.Instance.accessoryData.name);//饰品栏数据
        Save(InventoryManager.Instance.accessoryShopTemplate,
            InventoryManager.Instance.accessoryShopData.name);//饰品商店数据

        PlayerPrefs.SetString(sceneName, "Main");


    }

    public void ResetGameData()
    {
        //重置数据
        Save(GameManager.Instance.gameDataTemplate, 
            GameManager.Instance.gameData.name);//重置游戏时间
    }

}
