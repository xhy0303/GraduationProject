using UnityEditor;
using UnityEditor.SceneManagement;

// Gameplay scenes depend on the persistent managers created by the main menu.
// Keep the scene being edited, but start Play Mode through the normal boot scene.
[InitializeOnLoad]
internal static class PlayModeEntry
{
    static PlayModeEntry()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var entry = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Begin.unity");
            if (entry != null) EditorSceneManager.playModeStartScene = entry;
        };
    }
}
