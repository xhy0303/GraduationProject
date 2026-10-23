using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class SaveTestBuild
{
    public static void Build()
    {
        PlayerSettings.companyName = "CodexIsolatedValidation";
        PlayerSettings.productName = "OneMoreTimeSavePlayerChecks";
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            target = BuildTarget.StandaloneOSX,
            locationPathName = Path.GetFullPath("../SaveValidation/OneMoreTimeSaveCheck.app"),
            options = BuildOptions.None
        });
        Directory.CreateDirectory("Logs/SaveReliability");
        File.WriteAllText("Logs/SaveReliability/player-build.txt", result.summary.result+"\nErrors: "+result.summary.totalErrors+"\nWarnings: "+result.summary.totalWarnings);
        if (result.summary.result != BuildResult.Succeeded || result.summary.totalWarnings != 0) throw new Exception("Test build did not pass cleanly");
    }
}
