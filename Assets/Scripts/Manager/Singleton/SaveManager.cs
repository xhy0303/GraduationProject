using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : Manager<SaveManager>
{
    [System.Serializable] private sealed class SceneRecord { public string name; }
    public string LastLoadStatus { get; private set; } = "";
    private static bool IsPlayableScene(string name)
    {
        return name == "Main" || name == "Dungon" || name == "FirstFloor" || name == "SecondFloor" || name == "ThirdFloor";
    }
    public string SceneName
    {
        get
        {
            for (int i = 0; i < 2; i++)
            {
                if (!SaveRecordStore.TryRead("scene", "scene", i == 1, out var payload)) continue;
                var value = JsonUtility.FromJson<SceneRecord>(payload);
                if (value != null && IsPlayableScene(value.name)) return value.name;
            }
            if (!SaveRecordStore.HasRecord("scene") && SaveRecordStore.CanReadLegacy)
            {
                var legacy = PlayerPrefs.GetString("level", "");
                if (IsPlayableScene(legacy)) return legacy;
            }
            return "";
        }
    }
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
        DontDestroyOnLoad(this);
    }
    public void Save(object data, string key)
    {
        var inventory = data as InventoryData_SO;
        var json = inventory != null ? InventorySaveCodec.Encode(inventory) : JsonUtility.ToJson(data);
        SaveRecordStore.Write(key, inventory != null ? "inventory" : data.GetType().FullName, json);
        RecordScene(SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }
    public void RecordScene(string name)
    {
        if (!IsPlayableScene(name)) return;
        SaveRecordStore.Write("scene", "scene", JsonUtility.ToJson(new SceneRecord { name = name }));
        PlayerPrefs.Save();
    }
    public void Load(object data, string key, string legacyKey = null)
    {
        LastLoadStatus = "";
        var inventory = data as InventoryData_SO;
        string kind = inventory != null ? "inventory" : data.GetType().FullName;
        for (int i = 0; i < 2; i++)
        {
            if (!SaveRecordStore.TryRead(key, kind, i == 1, out var json)) continue;
            bool applied;
            if (inventory != null) applied = InventorySaveCodec.TryDecode(json, inventory, out _);
            else applied = TryApply(json, data);
            if (!applied) continue;
            if (i == 1) Report("存档 " + key + " 校验失败，已恢复上一份有效备份。");
            return;
        }
        if (SaveRecordStore.HasRecord(key))
        {
            Report("存档 " + key + " 无法读取，保留原记录并使用初始数据。");
            return;
        }
        var oldKey = legacyKey ?? key;
        if (!SaveRecordStore.CanReadLegacy || !PlayerPrefs.HasKey(oldKey)) return;
        var oldJson = PlayerPrefs.GetString(oldKey);
        if (inventory != null)
        {
            if (!InventorySaveCodec.TryDecodeLegacy(oldJson, inventory))
                Report("旧背包 " + oldKey + " 仅包含临时实例引用，无法可靠恢复物品；旧档原样保留，当前使用初始背包。");
        }
        else if (!TryApply(oldJson, data)) Report("旧存档 " + oldKey + " 无法读取，原记录已保留。");
    }
    private static bool TryApply(string json, object data)
    {
        var before = JsonUtility.ToJson(data);
        try
        {
            if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{")) return false;
            JsonUtility.FromJsonOverwrite(json, data);
            if (data is CharacterData_SO character && (character.maxHealth <= 0 || character.currentHealth < 0 || character.currentHealth > character.maxHealth))
                throw new System.ArgumentException("Invalid character data");
            if (data is GeneralGameData_SO progress && progress.gameTime < 0)
                throw new System.ArgumentException("Invalid progress");
            return true;
        }
        catch (System.ArgumentException) { JsonUtility.FromJsonOverwrite(before, data); return false; }
    }
    private void Report(string message)
    {
        LastLoadStatus = message;
        Debug.Log(message);
    }

    #region 数据保存
    public void SavePlayerData()
    {
        Save(GameManager.Instance.playerStats.characterData, "player");
    }

    public void LoadPlayerData()
    {
        Load(GameManager.Instance.playerStats.characterData, "player", GameManager.Instance.playerStats.characterData.name);
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
        Save(GameManager.Instance.gameData, "progress");
    }
    public void LoadGeneralGameData()
    //只有在开始游戏时（不管是新游戏还是读档）才会调用
    {
        Load(GameManager.Instance.gameData, "progress", GameManager.Instance.gameData.name);
    }

    #endregion

    public void SaveAllData()
    {
        SavePlayerData();
        SaveInventoryData();

        foreach (var objManager in LegacyObjectLookup.FindAll<InteractableObjectManager>())
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
            "player");//玩家生命值

        Save(GameManager.Instance.gameData, "progress");//游戏时间

        Save(InventoryManager.Instance.inventoryTemplate,
            "inventory.bag");//背包数据
        Save(InventoryManager.Instance.weaponTemplate,
            "inventory.weapon");//武器栏数据
        Save(InventoryManager.Instance.shieldTemplate,
            "inventory.shield");//盾牌栏数据
        Save(InventoryManager.Instance.accessoryTemplate,
            "inventory.accessory");//饰品栏数据
        Save(InventoryManager.Instance.accessoryShopTemplate,
            "inventory.shop");//饰品商店数据

        RecordScene("Main");


    }

    public void ResetGameData()
    {
        SaveRecordStore.StartNewGame();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(GameManager.Instance.gameDataTemplate), GameManager.Instance.gameData);
        Save(GameManager.Instance.gameDataTemplate, "progress");//重置游戏时间
    }

}
