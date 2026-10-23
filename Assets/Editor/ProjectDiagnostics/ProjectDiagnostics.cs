using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Editor-only, file-based diagnostics. Mutations require explicit request files.
[InitializeOnLoad]
internal static class ProjectDiagnostics
{
    [Serializable]
    private sealed class State
    {
        public string utc;
        public string unityVersion;
        public bool compiling;
        public bool updating;
        public bool playing;
        public int errors;
        public int warnings;
        public int messages;
        public string scene;
        public bool sceneDirty;
    }

    private static double nextUpdate;
    private static readonly string DirectoryPath = Path.GetFullPath("Logs/ProjectDiagnostics");

    static ProjectDiagnostics()
    {
        EditorApplication.update += Update;
    }

    [Serializable] private sealed class ScriptEntry { public string type; public string guid; public long fileId; public string path; }
    [Serializable] private sealed class ScriptMap { public List<ScriptEntry> entries = new List<ScriptEntry>(); }
    public static void ExportScriptMap()
    {
        var map = new ScriptMap();
        foreach (var script in MonoImporter.GetAllRuntimeMonoScripts())
        {
            var type = script.GetClass();
            if (type != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(script, out string guid, out long id))
                map.entries.Add(new ScriptEntry { type = type.FullName, guid = guid, fileId = id, path = AssetDatabase.GetAssetPath(script) });
        }
        Directory.CreateDirectory(DirectoryPath);
        File.WriteAllText(Path.Combine(DirectoryPath, "script-map.json"), JsonUtility.ToJson(map, true));
    }

    // Run only in an isolated validation copy with -executeMethod.
    public static void ValidateLookupCompatibility()
    {
        var created = new GameObject[3];
        try
        {
            for (int i = 0; i < created.Length; i++)
                created[i] = new GameObject("LookupCompatibility-" + i);
            var legacy = typeof(UnityEngine.Object).GetMethod("FindObjectsOfType", new[] { typeof(Type), typeof(bool) });
            var expected = (UnityEngine.Object[])legacy.Invoke(null, new object[] { typeof(GameObject), false });
            var lookup = Type.GetType("LegacyObjectLookup, Assembly-CSharp", true);
            var actual = (GameObject[])lookup.GetMethod("FindAll").MakeGenericMethod(typeof(GameObject)).Invoke(null, null);
            if (expected.Length != actual.Length) throw new InvalidOperationException("Lookup count changed.");
            for (int i = 0; i < actual.Length; i++)
                if (expected[i] != actual[i]) throw new InvalidOperationException("Lookup order changed at " + i);
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(Path.Combine(DirectoryPath, "lookup-validation.txt"),
                "PASS: same objects and ordering as the original Unity lookup. Unity " + Application.unityVersion);
        }
        finally
        {
            foreach (var item in created)
                if (item != null) UnityEngine.Object.DestroyImmediate(item);
        }
    }

    private static void Update()
    {
        if (EditorApplication.timeSinceStartup < nextUpdate) return;
        nextUpdate = EditorApplication.timeSinceStartup + 2;
        Directory.CreateDirectory(DirectoryPath);
        var state = new State
        {
            utc = DateTime.UtcNow.ToString("O"),
            unityVersion = Application.unityVersion,
            compiling = EditorApplication.isCompiling,
            updating = EditorApplication.isUpdating,
            playing = EditorApplication.isPlaying
        };
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        state.scene = scene.path;
        state.sceneDirty = scene.isDirty;
        var logEntries = typeof(EditorApplication).Assembly.GetType("UnityEditor.LogEntries");
        var getCounts = logEntries?.GetMethod("GetCountsByType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (getCounts != null)
        {
            object[] counts = { 0, 0, 0 };
            getCounts.Invoke(null, counts);
            state.errors = (int)counts[0];
            state.warnings = (int)counts[1];
            state.messages = (int)counts[2];
        }
        File.WriteAllText(Path.Combine(DirectoryPath, "state.json"), JsonUtility.ToJson(state, true));
        if (state.playing && SessionState.GetBool("OneMoreTime.MenuCheck", false)
            && EditorApplication.timeSinceStartup >= SessionState.GetFloat("OneMoreTime.MenuCheckEnd", 0))
        {
            File.WriteAllText(Path.Combine(DirectoryPath, "menu-play-state.json"), JsonUtility.ToJson(state, true));
            SessionState.SetBool("OneMoreTime.MenuCheck", false);
            EditorApplication.isPlaying = false;
        }
        var request = Path.Combine(DirectoryPath, "refresh.request");
        if (File.Exists(request) && !state.compiling && !state.updating && !state.playing)
        {
            File.Delete(request);
            AssetDatabase.Refresh();
        }
        var invoke = Path.Combine(DirectoryPath, "invoke.request");
        if (File.Exists(invoke) && !state.compiling && !state.updating)
        {
            var name = File.ReadAllText(invoke).Trim();
            File.Delete(invoke);
            if (name == "ExportScriptMap") ExportScriptMap();
            if (name == "CheckMenuPlay" && !state.playing && !state.sceneDirty
                && (scene.name == "Begin" || UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene?.name == "Begin"))
            {
                SessionState.SetBool("OneMoreTime.MenuCheck", true);
                SessionState.SetFloat("OneMoreTime.MenuCheckEnd", (float)EditorApplication.timeSinceStartup + 20);
                EditorApplication.isPlaying = true;
            }
            if (name == "PrepareResourceSync" && !state.playing && !state.sceneDirty)
            {
                File.WriteAllText(Path.Combine(DirectoryPath, "before-resource-sync.json"), JsonUtility.ToJson(state, true));
                UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
                AssetDatabase.DisallowAutoRefresh();
                typeof(EditorApplication).Assembly.GetType("UnityEditor.LogEntries")?.GetMethod("Clear", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
                File.WriteAllText(Path.Combine(DirectoryPath, "resource-sync-ready.txt"), DateTime.UtcNow.ToString("O"));
            }
            if (name == "CompleteResourceSync" && !state.playing)
            {
                AssetDatabase.AllowAutoRefresh();
                var previousPath = Path.Combine(DirectoryPath, "before-resource-sync.json");
                var previous = File.Exists(previousPath) ? JsonUtility.FromJson<State>(File.ReadAllText(previousPath)) : null;
                File.WriteAllText(Path.Combine(DirectoryPath, "open-begin.request"), previous?.scene ?? "");
                AssetDatabase.Refresh();
            }
        }
        var openBegin = Path.Combine(DirectoryPath, "open-begin.request");
        if (File.Exists(openBegin) && !EditorApplication.isCompiling && !EditorApplication.isUpdating && !EditorApplication.isPlaying)
        {
            var targetScene = File.ReadAllText(openBegin).Trim();
            File.Delete(openBegin);
            if (string.IsNullOrEmpty(targetScene) || !File.Exists(targetScene)) targetScene = "Assets/Scenes/Begin.unity";
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(targetScene);
        }
    }
}
