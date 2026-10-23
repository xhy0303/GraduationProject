using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public sealed class ItemCatalogBuilder : IPreprocessBuildWithReport
{
    static ItemCatalogBuilder()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingEditMode) Rebuild();
        };
    }
    public int callbackOrder => -1000;
    public void OnPreprocessBuild(BuildReport report) { Rebuild(); }

    [MenuItem("Tools/One More Time/Rebuild Item Catalog")]
    public static void Rebuild()
    {
        const string path = "Assets/Resources/ItemCatalog.asset";
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        var catalog = AssetDatabase.LoadAssetAtPath<ItemCatalog>(path);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<ItemCatalog>();
            AssetDatabase.CreateAsset(catalog, path);
        }
        catalog.items.Clear();
        foreach (var guid in AssetDatabase.FindAssets("t:ItemData_SO", new[] { "Assets" }).OrderBy(g => g))
        {
            var item = AssetDatabase.LoadAssetAtPath<ItemData_SO>(AssetDatabase.GUIDToAssetPath(guid));
            var serialized = new SerializedObject(item);
            serialized.FindProperty("persistentId").stringValue = guid;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            // AssetRipper retained source object names with suffix-free filenames.
            // Align only the internal asset name; the player's itemName is unchanged.
            item.name = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(item));
            EditorUtility.SetDirty(item);
            catalog.items.Add(item);
        }
        catalog.CreateLookup();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
    }
}
