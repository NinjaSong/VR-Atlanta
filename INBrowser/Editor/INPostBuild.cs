using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;

public class INPostBuild
{
    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        var sourcePath = Application.dataPath + "/INBrowser/Plugins/";
        var destinationPath = pathToBuiltProject;

        if (destinationPath.EndsWith(".exe"))
            destinationPath = destinationPath.Replace(".exe", "_Data");
        else if (!destinationPath.EndsWith("_Data"))
            destinationPath += "_Data";

        FileUtil.CopyFileOrDirectory(sourcePath + "ChromiumFX.dll",
                                     destinationPath + "/ChromiumFX.dll");

        FileUtil.CopyFileOrDirectory(sourcePath + "INBrowserApp.exe",
                                     destinationPath + "/INBrowserApp.exe");

        if (target == BuildTarget.StandaloneWindows64)
        {
            FileUtil.CopyFileOrDirectory(sourcePath + "libcfx64.dll",
                             destinationPath + "/libcfx64.dll");

            FileUtil.CopyFileOrDirectory(sourcePath + "cef64",
                             destinationPath + "/cef64");
        }
        else if(target == BuildTarget.StandaloneWindows)
        {
            FileUtil.CopyFileOrDirectory(sourcePath + "libcfx.dll",
                             destinationPath + "/libcfx.dll");

            FileUtil.CopyFileOrDirectory(sourcePath + "cef",
                             destinationPath + "/cef");
        }
    }
}
