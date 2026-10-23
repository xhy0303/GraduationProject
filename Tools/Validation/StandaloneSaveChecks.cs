using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Test-only source: copied into the isolated project for its test build, never the game project.
public sealed class StandaloneSaveChecks : MonoBehaviour
{
    [Serializable] private sealed class State
    {
        public string[] inventory;
        public int health, run;
        public string manager;
        public int objectIndex;
    }
    [Serializable] private sealed class Result
    {
        public string mode;
        public List<string> checks = new List<string>();
        public List<string> diagnostics = new List<string>();
    }
    string mode, output;
    Result report;
    double deadline;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (Application.isEditor) return;
        var args = Environment.GetCommandLineArgs();
        var at = Array.IndexOf(args, "-saveCheckMode");
        var path = Array.IndexOf(args, "-saveCheckOutput");
        if (at < 0 || at + 1 >= args.Length || path < 0 || path + 1 >= args.Length) return;
        if (Application.companyName != "CodexIsolatedValidation" || Application.productName != "OneMoreTimeSavePlayerChecks")
            throw new InvalidOperationException("Standalone validation must use isolated saves.");
        var runner = new GameObject("StandaloneSaveChecks").AddComponent<StandaloneSaveChecks>();
        DontDestroyOnLoad(runner);
        runner.mode = args[at + 1];
        runner.output = args[path + 1];
        runner.report = new Result { mode = runner.mode };
        Directory.CreateDirectory(runner.output);
        runner.deadline = Time.realtimeSinceStartupAsDouble + 150;
        Application.logMessageReceived += runner.Log;
        runner.StartCoroutine(runner.Run());
    }
    void Log(string m, string trace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Warning || type == LogType.Assert)
        {
            var value = type + ": " + m + "\n" + trace;
            if (!report.diagnostics.Contains(value)) report.diagnostics.Add(value);
        }
    }
    void Update()
    {
        if (report != null && Time.realtimeSinceStartupAsDouble > deadline) { report.diagnostics.Add("Timeout"); Finish(); }
    }
    void Check(bool value, string text)
    {
        if (!value) throw new InvalidOperationException(text);
        report.checks.Add(text);
    }
    InventoryData_SO[] All()
    {
        var i = InventoryManager.Instance;
        return new[]{i.inventoryData, i.weaponData, i.shieldData, i.accessoryData, i.accessoryShopData};
    }
    State Snapshot()
    {
        return new State { inventory = All().Select(InventorySaveCodec.Encode).ToArray(), health = GameManager.Instance.playerStats.CurrentHealth, run = GameManager.Instance.gameData.gameTime };
    }
    IEnumerator Run()
    {
        yield return new WaitForSecondsRealtime(2);
        var menu = LegacyObjectLookup.FindFirst<MainMenu>();
        if (mode == "write") menu.newGameBut.onClick.Invoke(); else menu.continueBut.onClick.Invoke();
        yield return new WaitForSecondsRealtime(5);
        try
        {
            Check(SceneManager.GetActiveScene().name == "Main", "Actual macOS player resumes Main");
            if (mode == "write") Prepare(); else Verify();
        }
        catch (Exception e) { report.diagnostics.Add(e.ToString()); Finish(); yield break; }
        yield return new WaitForSecondsRealtime(2);
        try
        {
            var form = GameManager.Instance.playerStats.transform.GetChild(2).GetComponent<ShapeShiftController>();
            Check(form.weaponSlot.childCount > 0 && form.shieldSlot.childCount > 0, "Equipment models remain attached after frame-end destruction");
            if (mode == "write")
            {
                var state = Snapshot();
                var manager = LegacyObjectLookup.FindAll<InteractableObjectManager>().First(m => m.objectStates.Count > 0);
                var obj = manager.objectStates.First(o => o != null);
                state.manager = manager.name; state.objectIndex = obj.objIndex; obj.isActive = false;
                SaveManager.Instance.SaveAllData();
                SaveManager.Instance.SaveAllData();
                File.WriteAllText(Path.Combine(output,"expected.json"), JsonUtility.ToJson(state,true));
            }
        }
        catch (Exception e) { report.diagnostics.Add(e.ToString()); }
        Finish();
    }
    void Prepare()
    {
        var items = Resources.Load<ItemCatalog>("ItemCatalog");
        Check(items.CreateLookup().Count == 25, "Player build contains 25 stable item IDs without UnityEditor APIs");
        var all = All();
        for (int i=0;i<all[0].inventoryItems.Count;i++)
        {
            all[0].inventoryItems[i].itemData = i < items.items.Count ? items.items[i] : null;
            all[0].inventoryItems[i].num = i < items.items.Count ? (items.items[i].countable ? i + 2 : 1) : 0;
        }
        Set(all[1], items.items.First(i => i.itemName == "剑"));
        Set(all[2], items.items.First(i => i.itemName == "盾牌"));
        Set(all[3], items.items.First(i => i.itemName == "鹰眼宝石"));
        Set(all[4], null);
        var before = Snapshot();
        InventoryManager.Instance.RefreshUI();
        Check(before.inventory.SequenceEqual(Snapshot().inventory), "Equipment UI restore preserves inventory records");
        GameManager.Instance.playerStats.CurrentHealth = Math.Min(31, GameManager.Instance.playerStats.MaxHealth);
        GameManager.Instance.gameData.gameTime = 4;
    }
    static void Set(InventoryData_SO inventory, ItemData_SO item)
    {
        inventory.inventoryItems[0].itemData = item;
        inventory.inventoryItems[0].num = item == null ? 0 : 1;
    }
    void Verify()
    {
        var expected = JsonUtility.FromJson<State>(File.ReadAllText(Path.Combine(output,"expected.json")));
        var actual = Snapshot();
        Check(expected.inventory.SequenceEqual(actual.inventory), "All five inventories survive closing and relaunching the macOS app");
        Check(expected.health == actual.health && expected.run == actual.run, "Health and run count survive macOS app restart");
        Check(GameManager.Instance.playerStats.EagleVision, "Accessory effect restored in standalone build");
        var manager = LegacyObjectLookup.FindAll<InteractableObjectManager>().First(m => m.name == expected.manager);
        Check(!manager.objectStates.First(o => o.objIndex == expected.objectIndex).isActive, "World object state restored before gameplay");
        Check(GameManager.Instance.playerStats.weaponData && GameManager.Instance.playerStats.shieldData, "Equipment stats restored in standalone build");
    }
    void Finish()
    {
        File.WriteAllText(Path.Combine(output,mode+".json"),JsonUtility.ToJson(report,true));
        Application.Quit(report.diagnostics.Count == 0 ? 0 : 1);
        enabled = false;
    }
}
