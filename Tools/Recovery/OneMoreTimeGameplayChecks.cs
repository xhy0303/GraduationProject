using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public static class OneMoreTimeGameplayChecks
{
    [Serializable] public class Report { public List<string> steps = new List<string>(); public List<string> diagnostics = new List<string>(); }
    static Report report;
    static double next, deadline;
    static int step, itemIndex, dialogueIndex;
    static ItemData_SO[] items;
    static DialogueController[] dialogues;
    static Vector3 initialPosition;
    static EnemyController enemy;
    static int enemyHealth;
    public static void Run()
    {
        if (PlayerSettings.companyName != "CodexIsolatedValidation") throw new Exception("Isolated save namespace required");
        report = new Report();
        Application.logMessageReceived += Log;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene("Assets/Scenes/Begin.unity");
        deadline = EditorApplication.timeSinceStartup + 240;
        next = EditorApplication.timeSinceStartup + 12;
        items = AssetDatabase.FindAssets("t:ItemData_SO", new[]{"Assets/Recovered"}).Select(g => AssetDatabase.LoadAssetAtPath<ItemData_SO>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(i => (int)i.itemType).ToArray();
        EditorApplication.update += Tick;
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
        next = EditorApplication.timeSinceStartup + 1;
        try
        {
            switch(step)
            {
                case 0:
                    LegacyObjectLookup.FindFirst<MainMenu>().newGameBut.onClick.Invoke(); step++; next += 6; break;
                case 1:
                    CheckItems(); break;
                case 2:
                    dialogues = LegacyObjectLookup.FindAll<DialogueController>().Where(d => d.currentDialogue != null).ToArray();
                    step++; break;
                case 3:
                    if (dialogueIndex >= dialogues.Length) { DialogueUI.Instance.dialoguePanel.SetActive(false); step++; break; }
                    var dialogue = dialogues[dialogueIndex++];
                    dialogue.OpenDialogue();
                    var piece = dialogue.currentDialogue.dialoguePieces[0];
                    if (DialogueUI.Instance.mainText.text != piece.text) throw new Exception("Dialogue text mismatch");
                    if (piece.options.Count > 0)
                    {
                        var option = DialogueUI.Instance.optionPanel.GetComponentsInChildren<OptionUI>().Last();
                        option.thisBut.onClick.Invoke();
                    }
                    else if (!piece.playTineLine) DialogueUI.Instance.nextBut.onClick.Invoke();
                    report.steps.Add("Dialogue opened and advanced: " + dialogue.name);
                    DialogueUI.Instance.dialoguePanel.SetActive(false);
                    break;
                case 4:
                    var player = GameManager.Instance.playerStats;
                    player.UnEquipAccessory();
                    player.UnEquipWeapon();
                    player.UnEquipShield();
                    initialPosition = player.transform.position;
                    step++; break;
                case 5:
                    var mover = GameManager.Instance.playerStats.GetComponent<NavMeshAgent>();
                    NavMeshHit hit;
                    if (!NavMesh.SamplePosition(initialPosition + Vector3.forward * 4, out hit, 5, NavMesh.AllAreas)) throw new Exception("No navigation target");
                    var click = (Action<Vector3>)typeof(MouseManager).GetField("OnMouseCliked", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(MouseManager.Instance);
                    click(hit.position);
                    step++; next += 3; break;
                case 6:
                    if (Vector3.Distance(initialPosition, GameManager.Instance.playerStats.transform.position) < 0.1f) throw new Exception("Click-to-move did not move player");
                    report.steps.Add("Click-to-move moved player on NavMesh");
                    enemy = LegacyObjectLookup.FindAll<EnemyController>().FirstOrDefault(e => e.GetComponent<CharacterStats>().CurrentHealth > 0);
                    if (!enemy) throw new Exception("No enemy for combat check");
                    var agent = GameManager.Instance.playerStats.GetComponent<NavMeshAgent>();
                    NavMeshHit nearby;
                    if (!NavMesh.SamplePosition(enemy.transform.position + Vector3.right, out nearby, 3, NavMesh.AllAreas)) throw new Exception("Enemy off navmesh");
                    agent.Warp(nearby.position);
                    enemyHealth = enemy.GetComponent<CharacterStats>().CurrentHealth;
                    var attack = (Action<GameObject>)typeof(MouseManager).GetField("OnEnemyClicked", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(MouseManager.Instance);
                    attack(enemy.gameObject);
                    step++; next += 4; break;
                case 7:
                    if (enemy && enemy.GetComponent<CharacterStats>().CurrentHealth >= enemyHealth) throw new Exception("Attack animation did not damage target");
                    report.steps.Add("Click attack and animation event damaged enemy");
                    Finish(); break;
            }
        }
        catch(Exception ex) { report.diagnostics.Add(ex.ToString()); Finish(); }
    }
    static void CheckItems()
    {
        if (itemIndex >= items.Length) { step++; return; }
        var item = items[itemIndex++];
        var inv = InventoryManager.Instance;
        InventoryData_SO data;
        ContainerUI ui;
        switch(item.itemType)
        {
            case ItemType.Weapon: data = inv.weaponData; ui = inv.weaponUI; break;
            case ItemType.Shield: data = inv.shieldData; ui = inv.shieldUI; break;
            case ItemType.Accessory: data = inv.accessoryData; ui = inv.accessoryUI; break;
            default: data = inv.inventoryData; ui = inv.inventoryUI; break;
        }
        data.inventoryItems[0].itemData = item;
        data.inventoryItems[0].num = 1;
        ui.RefreshUI();
        if (item.itemType == ItemType.Useable)
        {
            GameManager.Instance.playerStats.CurrentHealth = 1;
            ui.slotHolders[0].UseItem();
            if (item.useableItemData != null && item.useableItemData.healthPoint > 0 && GameManager.Instance.playerStats.CurrentHealth <= 1) throw new Exception("Healing failed");
        }
        GameManager.Instance.playerStats.CurrentHealth = GameManager.Instance.playerStats.MaxHealth;
        if (item.itemType == ItemType.Useable && item.useableItemData == null && data.inventoryItems[0].num != 1) throw new Exception("Non-consumable item was consumed");
        report.steps.Add("Item UI use/equip: " + item.itemName);
        Capture("item-" + itemIndex);
    }
    public static void Capture(string name)
    {
        var camera = Camera.main;
        if (!camera) return;
        var target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
        var previous = RenderTexture.active;
        var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        var overlays = UnityEngine.Object.FindObjectsByType<Canvas>().Where(c => c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        var cameras = overlays.Select(c => c.worldCamera).ToArray();
        var distances = overlays.Select(c => c.planeDistance).ToArray();
        try
        {
            foreach (var canvas in overlays)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = camera.nearClipPlane + 0.5f;
            }
            Canvas.ForceUpdateCanvases();
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,
                new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination = target });
            RenderTexture.active = target;
            texture.ReadPixels(new Rect(0,0,1280,720),0,0);
            texture.Apply();
            Directory.CreateDirectory("Logs/Recovery/Screenshots");
            File.WriteAllBytes("Logs/Recovery/Screenshots/" + name + ".png", texture.EncodeToPNG());
        }
        finally
        {
            for (int i = 0; i < overlays.Length; i++)
            {
                overlays[i].renderMode = RenderMode.ScreenSpaceOverlay;
                overlays[i].worldCamera = cameras[i];
                overlays[i].planeDistance = distances[i];
            }
            Canvas.ForceUpdateCanvases();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }
    static void Finish()
    {
        EditorApplication.update -= Tick;
        Directory.CreateDirectory("Logs/Recovery");
        File.WriteAllText("Logs/Recovery/gameplay-checks.json", JsonUtility.ToJson(report, true));
        EditorApplication.Exit(report.diagnostics.Count == 0 ? 0 : 1);
    }
}
