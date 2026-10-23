using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OneMoreTimeRuntimeChecks
{
    [Serializable] public class Report { public List<string> steps = new List<string>(); public List<string> diagnostics = new List<string>(); }
    static Report report;
    static double next, deadline;
    static int step;
    public static void Run()
    {
        if (PlayerSettings.companyName != "CodexIsolatedValidation") throw new Exception("Refusing to touch user saves.");
        report = new Report();
        // Finish the editor's first-use search setup before entering Play Mode.
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var search = assembly.GetType("UnityEditor.Search.SearchDatabase");
            if (search != null) search.GetMethod("GetDefaultSearchDatabase").Invoke(null, null);
        }
        Application.logMessageReceived += Log;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene("Assets/Scenes/Begin.unity");
        deadline = EditorApplication.timeSinceStartup + 300;
        next = EditorApplication.timeSinceStartup + 12;
        EditorApplication.update += Tick;
        Directory.CreateDirectory("Logs/Recovery/Screenshots");
        EditorApplication.isPlaying = true;
    }
    static void Log(string message, string trace, LogType type)
    {
        if (type == LogType.Warning || type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            var entry = type + ": " + message + "\n" + trace;
            if (!report.diagnostics.Contains(entry)) report.diagnostics.Add(entry);
        }
    }
    static void Tick()
    {
        if (EditorApplication.timeSinceStartup > deadline) { report.diagnostics.Add("Timeout"); Finish(); return; }
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < next) return;
        next = EditorApplication.timeSinceStartup + 4;
        try
        {
            OneMoreTimeGameplayChecks.Capture(step + "-" + SceneManager.GetActiveScene().name);
            switch(step++)
            {
                case 0:
                    report.steps.Add("Menu loaded");
                    LegacyObjectLookup.FindFirst<MainMenu>().newGameBut.onClick.Invoke();
                    break;
                case 1:
                    if (SceneManager.GetActiveScene().name != "Main" || !GameManager.Instance.playerStats) throw new Exception("New game failed");
                    report.steps.Add("New game spawned player in Main");
                    InventoryManager.Instance.RefreshUI();
                    SaveManager.Instance.SaveAllData();
                    report.steps.Add("Inventory refreshed and game saved");
                    SceneController.Instance.Back2Home();
                    break;
                case 2:
                    if (SceneManager.GetActiveScene().name != "Begin") throw new Exception("Return to menu failed");
                    report.steps.Add("Returned to menu");
                    LegacyObjectLookup.FindFirst<MainMenu>().continueBut.onClick.Invoke();
                    break;
                case 3:
                    if (SceneManager.GetActiveScene().name != "Main" || !GameManager.Instance.playerStats) throw new Exception("Continue failed");
                    report.steps.Add("Continue restored player in Main");
                    GameManager.Instance.playerStats.GetComponent<PlayerController>().ShapeShift("兽人");
                    break;
                case 4:
                    if (GameManager.Instance.playerStats.currentShape != PlayerStats.PlayerShape.Yokai) throw new Exception("Yokai failed");
                    report.steps.Add("Yokai shape and animator restored");
                    GameManager.Instance.playerStats.GetComponent<PlayerController>().ShapeShift("隐形");
                    break;
                case 5:
                    if (GameManager.Instance.playerStats.currentShape != PlayerStats.PlayerShape.Invisiable) throw new Exception("Invisible failed");
                    report.steps.Add("Invisible shape restored");
                    GameManager.Instance.playerStats.GetComponent<PlayerController>().ShapeShift("人形");
                    break;
                case 6: Travel("Dungon", TransitionDestination.DestinationTag.DungonIndoor); break;
                case 7: CheckScene("Dungon"); Travel("FirstFloor", TransitionDestination.DestinationTag.FrontGateIndoor); break;
                case 8: CheckScene("FirstFloor"); Travel("SecondFloor", TransitionDestination.DestinationTag.SecondFloorDOWN); break;
                case 9: CheckScene("SecondFloor"); Travel("ThirdFloor", TransitionDestination.DestinationTag.ThirdFloor); break;
                case 10:
                    CheckScene("ThirdFloor");
                    SaveManager.Instance.SaveAllData();
                    GameOverManager.Instance.ShowGameOver(false);
                    report.steps.Add("Defeat panel opened");
                    SceneController.Instance.Back2Home();
                    break;
                case 11:
                    LegacyObjectLookup.FindFirst<MainMenu>().continueBut.onClick.Invoke();
                    break;
                case 12:
                    CheckScene("ThirdFloor");
                    GameOverManager.Instance.ShowGameOver(true);
                    SaveManager.Instance.SetDataAfterWin();
                    report.steps.Add("Victory panel and next-run save");
                    SceneController.Instance.Back2Home();
                    break;
                case 13:
                    LegacyObjectLookup.FindFirst<MainMenu>().continueBut.onClick.Invoke();
                    break;
                case 14:
                    CheckScene("Main");
                    Finish();
                    break;
            }
        }
        catch(Exception ex) { report.diagnostics.Add(ex.ToString()); Finish(); }
    }
    static void Travel(string scene, TransitionDestination.DestinationTag tag)
    {
        var point = new GameObject("RuntimeCheckTransition").AddComponent<TransitionPoint>();
        point.transitionType = TransitionPoint.TransitionType.DifferentScene;
        point.sceneName = scene;
        point.destinationTag = tag;
        SceneController.Instance.Transition2Destination(point);
    }
    static void CheckScene(string name)
    {
        if (SceneManager.GetActiveScene().name != name || !GameManager.Instance.playerStats) throw new Exception("Transition failed: " + name);
        var agent = GameManager.Instance.playerStats.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (!agent.isOnNavMesh) throw new Exception("Player off navigation mesh: " + name);
        report.steps.Add("Player and navigation ready: " + name);
    }
    static void Finish()
    {
        EditorApplication.update -= Tick;
        Directory.CreateDirectory("Logs/Recovery");
        File.WriteAllText("Logs/Recovery/runtime-checks.json", JsonUtility.ToJson(report, true));
        EditorApplication.Exit(report.diagnostics.Count == 0 ? 0 : 1);
    }
}
