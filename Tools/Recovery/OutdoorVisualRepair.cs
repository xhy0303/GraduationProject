using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class OutdoorVisualRepair
{
    public static void Apply()
    {
        Directory.CreateDirectory("Logs/OutdoorVisual");
        var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Recovered/Shaders/PolybrushLitVertexColor.shadergraph");
        if (!shader || ShaderUtil.ShaderHasError(shader)) throw new Exception("Vertex color shader failed to import");
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Recovered/Material/Polybrush_Universal Render Pipeline_Lit Vertex Color URP.mat");
        material.shader = shader;
        material.shaderKeywords = Array.Empty<string>();
        EditorUtility.SetDirty(material);
        RestorePalette("Gray_Gradient", "55d393a33a18bc34bbaf2ae95cabfc7b", Color.white);
        RestorePalette("Color", "40f7256e7099cc6469ed0cb075983154", new Color(1, 0.7849823f, 0.5896226f, 1));
        var builderMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Recovered/Material/ProBuilder Default URP.mat");
        builderMaterial.shader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.unity.probuilder/Content/Shader/Standard Vertex Color.shadergraph");
        builderMaterial.shaderKeywords = Array.Empty<string>();
        EditorUtility.SetDirty(builderMaterial);
        AssetDatabase.SaveAssets();
        var report = new List<string>();
        foreach (var name in new[]{"Begin","Main"})
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>())
            {
                if (!renderer.sharedMaterials.Contains(material)) continue;
                var mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                var colors = mesh.colors;
                var mean = colors.Length > 0 ? colors.Aggregate(Color.clear, (a,b)=>a+b) / colors.Length : Color.clear;
                report.Add(name + ": " + renderer.name + ", vertices="+mesh.vertexCount+", colors="+colors.Length+", mean="+mean+", bounds="+renderer.bounds);
            }
            // The recovered sky probe predates the replacement sky shader.
            // Use an explicit neutral daylight gradient for these two outdoor scenes.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.58f, 0.60f);
            RenderSettings.ambientEquatorColor = new Color(0.40f, 0.42f, 0.38f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.25f, 0.18f);
            foreach (var light in UnityEngine.Object.FindObjectsByType<Light>())
                if (light.type == LightType.Directional) RenderSettings.sun = light;
            EditorSceneManager.SaveScene(scene);
        }
        File.WriteAllLines("Logs/OutdoorVisual/grass-audit.txt", report);
        EditorSceneManager.OpenScene("Assets/Scenes/Begin.unity");
    }
    static void RestorePalette(string name, string textureGuid, Color tint)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Recovered/Material/" + name + ".mat");
        var texture = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(textureGuid));
        if (!texture) throw new Exception("Missing original palette: " + name);
        material.SetTexture("_BaseMap", texture);
        material.SetColor("_BaseColor", tint);
        EditorUtility.SetDirty(material);
    }
    static double next;
    static int step;
    static readonly List<string> issues = new List<string>();
    public static void Capture()
    {
        if (PlayerSettings.companyName != "CodexIsolatedValidation") throw new Exception("Test save namespace required");
        EditorSceneManager.OpenScene("Assets/Scenes/Begin.unity");
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        Application.logMessageReceived += (m,s,t) => { if(t==LogType.Warning || t==LogType.Error || t==LogType.Exception) issues.Add(t+": "+m+"\n"+s); };
        next = EditorApplication.timeSinceStartup + 12;
        EditorApplication.update += Tick;
        EditorApplication.isPlaying = true;
    }
    static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < next) return;
        next = EditorApplication.timeSinceStartup + 5;
        try
        {
            if (step++ == 0) { OneMoreTimeGameplayChecks.Capture("outdoor-fixed-menu"); LegacyObjectLookup.FindFirst<MainMenu>().newGameBut.onClick.Invoke(); }
            else if (step == 2)
            {
                var entrance = UnityEngine.Object.FindObjectsByType<TransitionDestination>().First(d => d.destinationTag == TransitionDestination.DestinationTag.FrontGateOurdoor);
                var agent = GameManager.Instance.playerStats.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (!agent.Warp(entrance.transform.position)) throw new Exception("Castle entrance is off navigation mesh");
            }
            else if (step == 3) { OneMoreTimeGameplayChecks.Capture("outdoor-fixed-castle"); }
            else
            {
                Directory.CreateDirectory("Logs/OutdoorVisual");
                File.WriteAllLines("Logs/OutdoorVisual/diagnostics.txt", issues);
                EditorApplication.update -= Tick;
                EditorApplication.Exit(issues.Count == 0 ? 0 : 1);
            }
        }
        catch(Exception e) { File.WriteAllText("Logs/OutdoorVisual/capture-error.txt", e.ToString()); EditorApplication.Exit(1); }
    }
}
