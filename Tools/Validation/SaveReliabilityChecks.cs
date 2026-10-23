using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveReliabilityChecks
{
    [Serializable] private sealed class Expected
    {
        public string bag, weapon, shield, accessory, shop;
        public int health, run;
        public string objectName;
        public int objectIndex;
        public string managerName;
        public int itemCount;
    }
    [Serializable] private sealed class Report
    {
        public string mode;
        public List<string> checks = new List<string>();
        public List<string> diagnostics = new List<string>();
    }
    static Report report;
    static string mode;
    static int step;
    static double next, timeout;
    static string Output => "Logs/SaveReliability";
    public static void Write() { Run("write"); }
    public static void Read() { Run("read"); }
    static void Run(string value)
    {
        if (PlayerSettings.companyName != "CodexIsolatedValidation" || PlayerSettings.productName != "OneMoreTimeSaveReliabilityChecks")
            throw new Exception("Refusing to modify non-test saves");
        ItemCatalogBuilder.Rebuild();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            assembly.GetType("UnityEditor.Search.SearchDatabase")?.GetMethod("GetDefaultSearchDatabase")?.Invoke(null, null);
        mode = value;
        report = new Report { mode = mode };
        Directory.CreateDirectory(Output);
        Application.logMessageReceived += Log;
        EditorSceneManager.OpenScene("Assets/Scenes/Begin.unity");
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        next = EditorApplication.timeSinceStartup + 10;
        timeout = EditorApplication.timeSinceStartup + 180;
        EditorApplication.update += Tick;
        EditorApplication.isPlaying = true;
    }
    static void Log(string message, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Warning || type == LogType.Assert)
        {
            var text = type + ": " + message + "\n" + stack;
            if (!report.diagnostics.Contains(text)) report.diagnostics.Add(text);
        }
    }
    static void Require(bool value, string check)
    {
        if (!value) throw new Exception(check);
        report.checks.Add(check);
    }
    static InventoryData_SO[] Inventories()
    {
        var i = InventoryManager.Instance;
        return new[]{i.inventoryData, i.weaponData, i.shieldData, i.accessoryData, i.accessoryShopData};
    }
    static Expected Snapshot()
    {
        var all = Inventories();
        return new Expected { bag = InventorySaveCodec.Encode(all[0]), weapon = InventorySaveCodec.Encode(all[1]),
            shield = InventorySaveCodec.Encode(all[2]), accessory = InventorySaveCodec.Encode(all[3]), shop = InventorySaveCodec.Encode(all[4]),
            health = GameManager.Instance.playerStats.CurrentHealth, run = GameManager.Instance.gameData.gameTime };
    }
    static void Tick()
    {
        if (EditorApplication.timeSinceStartup > timeout) { report.diagnostics.Add("Timeout"); Finish(); return; }
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < next) return;
        next = EditorApplication.timeSinceStartup + 3;
        try
        {
            if (step++ == 0)
            {
                var menu = LegacyObjectLookup.FindFirst<MainMenu>();
                if (mode == "write") menu.newGameBut.onClick.Invoke();
                else menu.continueBut.onClick.Invoke();
                next += 5;
            }
            else if (step == 2)
            {
                Require(SceneManager.GetActiveScene().name == "Main", "Continue/new game enters saved Main scene");
                if (mode == "write") PrepareSave();
                else VerifySave();
            }
            else if (step == 3)
            {
                if (mode == "write")
                {
                    VerifyEquipmentModels();
                    var expected = Snapshot();
                    var manager = LegacyObjectLookup.FindAll<InteractableObjectManager>().First(m => m.objectStates.Count > 0);
                    var state = manager.objectStates.First(o => o != null);
                    state.isActive = false;
                    expected.objectName = state.objName;
                    expected.objectIndex = state.objIndex;
                    expected.managerName = manager.name;
                    expected.itemCount = Resources.Load<ItemCatalog>("ItemCatalog").items.Count;
                    SaveManager.Instance.SaveAllData();
                    SaveManager.Instance.SaveAllData();
                    File.WriteAllText(Output + "/expected.json", JsonUtility.ToJson(expected, true));
                    Require(!PlayerPrefs.GetString(SaveRecordStore.PrimaryKey("inventory.bag")).Contains("instanceID"), "Serialized inventory has stable IDs and no instanceID");
                    Require(SaveManager.Instance.SceneName == "Main", "Scene record persisted");
                    Finish();
                }
                else { VerifyRecovery(); Finish(); }
            }
        }
        catch (Exception e) { report.diagnostics.Add(e.ToString()); Finish(); }
    }
    static void PrepareSave()
    {
        var catalog = Resources.Load<ItemCatalog>("ItemCatalog");
        var bag = InventoryManager.Instance.inventoryData;
        Require(catalog.CreateLookup().Count == catalog.items.Count && catalog.items.Count >= 25, "Every item has a unique registered ID");
        Require(bag.inventoryItems.Count >= catalog.items.Count, "Bag can hold test set");
        for (int n = 0; n < bag.inventoryItems.Count; n++)
        {
            bag.inventoryItems[n].itemData = n < catalog.items.Count ? catalog.items[n] : null;
            bag.inventoryItems[n].num = n < catalog.items.Count ? (catalog.items[n].countable ? n + 2 : 1) : 0;
        }
        var all = Inventories();
        Set(all[1], catalog.items.First(i => i.itemName == "剑"));
        Set(all[2], catalog.items.First(i => i.itemName == "盾牌"));
        Set(all[3], catalog.items.First(i => i.itemName == "鹰眼宝石"));
        Set(all[4], null);
        var before = Snapshot();
        InventoryManager.Instance.RefreshUI();
        InventoryManager.Instance.RefreshUI();
        var after = Snapshot();
        Require(before.bag == after.bag && before.weapon == after.weapon && before.shield == after.shield && before.accessory == after.accessory,
            "Repeated UI refresh does not move saved equipment into bag");
        GameManager.Instance.playerStats.CurrentHealth = Math.Min(37, GameManager.Instance.playerStats.MaxHealth);
        GameManager.Instance.gameData.gameTime = 3;
        GameManager.Instance.TogglePause(true);
        Time.timeScale = 0;
    }
    static void Set(InventoryData_SO inventory, ItemData_SO item)
    {
        inventory.inventoryItems[0].itemData = item;
        inventory.inventoryItems[0].num = item == null ? 0 : 1;
    }
    static void VerifySave()
    {
        var e = JsonUtility.FromJson<Expected>(File.ReadAllText(Output + "/expected.json"));
        var a = Snapshot();
        Require(a.bag == e.bag, "All bag IDs, counts and empty slots survive a new Unity process");
        Require(a.weapon == e.weapon && a.shield == e.shield && a.accessory == e.accessory, "All equipped slots survive restart and UI initialization");
        Require(a.shop == e.shop, "Shop stock survives restart");
        Require(a.health == e.health && a.run == e.run, "Health and run count survive restart");
        Require(GameManager.Instance.playerStats.EagleVision, "Restored accessory effect is active");
        Require(GameManager.Instance.playerStats.weaponData != null && GameManager.Instance.playerStats.shieldData != null, "Restored weapon/shield stats are active");
        var manager = LegacyObjectLookup.FindAll<InteractableObjectManager>().First(m => m.name == e.managerName);
        var state = manager.objectStates.First(o => o.objIndex == e.objectIndex && o.objName == e.objectName);
        Require(!state.isActive, "World object state restores using saved run count before scene Awake");
        GameManager.Instance.TogglePause(true);
    }
    static void VerifyEquipmentModels()
    {
        var form = GameManager.Instance.playerStats.transform.GetChild(2).GetComponent<ShapeShiftController>();
        Require(form.weaponSlot.childCount > 0 && form.shieldSlot.childCount > 0, "Weapon and shield models survive deferred shape replacement");
    }
    static void VerifyRecovery()
    {
        VerifyEquipmentModels();
        var manager = SaveManager.Instance;
        var bag = InventoryManager.Instance.inventoryData;
        var before = InventorySaveCodec.Encode(bag);
        var primaryKey = SaveRecordStore.PrimaryKey("inventory.bag");
        PlayerPrefs.SetString(primaryKey, "{truncated");
        Set(bag, null);
        manager.Load(bag, "inventory.bag");
        Require(InventorySaveCodec.Encode(bag) == before && manager.LastLoadStatus.Contains("备份"), "Damaged primary restores verified backup");
        var invalid = JsonUtility.FromJson<InventorySaveCodec.Data>(before);
        invalid.slots[0].itemId = "missing-item-id";
        var rejected = JsonUtility.ToJson(invalid);
        Require(!InventorySaveCodec.TryDecode(rejected, bag, out _) && InventorySaveCodec.Encode(bag) == before, "Unknown item ID rejects entire load without partial mutation");
        SaveRecordStore.Write("inventory.bag", "inventory", rejected);
        manager.Load(bag, "inventory.bag");
        Require(InventorySaveCodec.Encode(bag) == before, "Semantically invalid primary uses intact backup");
        var legacy = "{\"inventoryItems\":[{\"itemData\":{\"instanceID\":123456},\"num\":7}]}";
        PlayerPrefs.SetString("legacy-test", legacy);
        PlayerPrefs.DeleteKey(SaveRecordStore.Prefix + "ignoreLegacy");
        manager.Load(bag, "legacy-test-new", "legacy-test");
        Require(PlayerPrefs.GetString("legacy-test") == legacy && InventorySaveCodec.Encode(bag) == before && manager.LastLoadStatus.Contains("旧背包"), "Legacy instance IDs never resolve to unrelated objects and original record is preserved");
        PlayerPrefs.SetString("user.preference.sentinel", "keep");
        PlayerPrefs.SetString(SaveRecordStore.Prefix + "keys", "{corrupted-index");
        manager.ResetGameData();
        Require(PlayerPrefs.GetString("user.preference.sentinel") == "keep" && PlayerPrefs.GetString("legacy-test") == legacy, "New game preserves unrelated preferences and archived legacy records");
        Require(!SaveRecordStore.HasRecord("inventory.bag") && !SaveRecordStore.CanReadLegacy, "New game cannot resurrect old inventory even with a corrupted key index");
        Require(GameManager.Instance.gameData.gameTime == GameManager.Instance.gameDataTemplate.gameTime, "New game resets in-memory run count");
        GameManager.Instance.gameData.gameTime = 9;
        manager.SetDataAfterWin();
        Require(manager.SceneName == "Main", "Victory persists next-run scene before exit");
    }
    static void Finish()
    {
        EditorApplication.update -= Tick;
        PlayerPrefs.Save();
        File.WriteAllText(Output + "/" + mode + ".json", JsonUtility.ToJson(report, true));
        EditorApplication.Exit(report.diagnostics.Count == 0 ? 0 : 1);
    }
}
