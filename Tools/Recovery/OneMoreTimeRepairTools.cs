using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class OneMoreTimeRepairTools
{
    public static void BuildMac()
    {
        var company = PlayerSettings.companyName;
        var product = PlayerSettings.productName;
        try
        {
            PlayerSettings.companyName = "xhy Studio";
            PlayerSettings.productName = "再来一次";
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = Path.GetFullPath("../Mac/OneMoreTime.app"),
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            });
            Directory.CreateDirectory("Logs/Recovery");
            File.WriteAllText("Logs/Recovery/mac-build.txt", result.summary.result + "\nErrors: " + result.summary.totalErrors + "\nWarnings: " + result.summary.totalWarnings + "\nBytes: " + result.summary.totalSize);
            if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Mac build failed");
        }
        finally { PlayerSettings.companyName = company; PlayerSettings.productName = product; }
    }
    [Serializable] public sealed class SceneAudit
    {
        public string scene;
        public int objects, missingScripts, missingMeshes, missingMaterials, colliders, navmeshVertices;
        public List<string> issues = new List<string>();
    }
    [Serializable] public sealed class Audit { public List<SceneAudit> scenes = new List<SceneAudit>(); }
    public static void PrepareAndAudit()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/Recovered" }))
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (mat.name.StartsWith("Skybox Cubemap"))
            {
                var cube = mat.GetTexture("_Tex");
                mat.shader = Shader.Find("Skybox/Cubemap");
                mat.SetTexture("_Tex", cube);
                mat.shaderKeywords = Array.Empty<string>();
                EditorUtility.SetDirty(mat);
            }
        }
        AssetDatabase.SaveAssets();
        var report = new Audit();
        foreach (var sceneSetting in EditorBuildSettings.scenes.Where(s => s.enabled))
        {
            var scene = EditorSceneManager.OpenScene(sceneSetting.path, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var mouse in root.GetComponentsInChildren<MouseManager>(true))
                foreach (var texture in new[] { mouse.normal, mouse.attack, mouse.doorWay, mouse.talk, mouse.chest })
                {
                    var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
                    if (importer == null || importer.textureType == TextureImporterType.Cursor) continue;
                    importer.textureType = TextureImporterType.Cursor;
                    importer.isReadable = true;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }
                foreach (var box in root.GetComponentsInChildren<BoxCollider>(true))
                {
                    box.size = new Vector3(Mathf.Abs(box.size.x), Mathf.Abs(box.size.y), Mathf.Abs(box.size.z));
                    var scale = box.transform.lossyScale;
                    if (scale.x >= 0 && scale.y >= 0 && scale.z >= 0) continue;
                    var signs = new Vector3(Mathf.Sign(scale.x), Mathf.Sign(scale.y), Mathf.Sign(scale.z));
                    var child = new GameObject("MirroredBoxCollision");
                    child.layer = box.gameObject.layer;
                    child.tag = box.gameObject.tag;
                    child.transform.SetParent(box.transform, false);
                    child.transform.localScale = signs;
                    var replacement = child.AddComponent<BoxCollider>();
                    EditorUtility.CopySerialized(box, replacement);
                    replacement.center = Vector3.Scale(box.center, signs);
                    replacement.size = new Vector3(Mathf.Abs(box.size.x), Mathf.Abs(box.size.y), Mathf.Abs(box.size.z));
                    UnityEngine.Object.DestroyImmediate(box);
                }
            }
            EditorSceneManager.SaveScene(scene);
            var r = new SceneAudit { scene = scene.name };
            foreach (var root in scene.GetRootGameObjects())
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                var obj = transform.gameObject;
                r.objects++;
                r.missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj);
                foreach (var mesh in obj.GetComponents<MeshFilter>())
                    if (mesh.sharedMesh == null) { r.missingMeshes++; r.issues.Add("Mesh: " + obj.name); }
                foreach (var renderer in obj.GetComponents<Renderer>())
                    foreach (var mat in renderer.sharedMaterials)
                        if (mat == null || mat.shader == null) { r.missingMaterials++; r.issues.Add("Material: " + obj.name); }
                r.colliders += obj.GetComponents<Collider>().Length;
            }
            r.navmeshVertices = NavMesh.CalculateTriangulation().vertices.Length;
            report.scenes.Add(r);
        }
        Directory.CreateDirectory("Logs/Recovery");
        File.WriteAllText("Logs/Recovery/scene-audit.json", JsonUtility.ToJson(report,true));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }
}
