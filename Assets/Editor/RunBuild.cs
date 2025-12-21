using UnityEngine;
using Godgame.Headless.Editor;

public static class RunBuild
{
    public static void Build()
    {
        try
        {
            GodgameHeadlessBuilder.BuildLinuxHeadless();
            Debug.Log("Build Succeeded!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Build Failed: {ex.Message}");
        }
    }
}

