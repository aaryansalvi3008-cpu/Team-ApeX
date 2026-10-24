#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace EscapeTheLab.Editor
{
    public static class GameFixerAndBuilder
    {
        [MenuItem("Tools/Escape The Lab/Fix Project Settings and Rebuild")]
        public static void FixAndRebuild()
        {
            Debug.Log("[GameFixerAndBuilder] Configuring PlayerSettings for rock-solid stability...");

            // Set clean product metadata
            PlayerSettings.productName = "Escape the Abandoned Lab";
            PlayerSettings.companyName = "IndieHorror";

            // Set windowed display mode to prevent exclusive fullscreen crashes on laptops
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;

            // Force Direct3D 11 as primary graphics API to prevent Intel Iris Xe D3D12 driver filter conflicts
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            GraphicsDeviceType[] apis = new GraphicsDeviceType[] {
                GraphicsDeviceType.Direct3D11,
                GraphicsDeviceType.Direct3D12
            };
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, apis);

            // Re-verify Build Settings scenes
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene("Assets/_Project/Scenes/LaboratoryLevel.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/Stage2_TestScene.unity", true)
            };
            EditorBuildSettings.scenes = scenes;

            AssetDatabase.SaveAssets();

            // Rebuild Standalone Player
            string buildDir = "Build";
            if (!System.IO.Directory.Exists(buildDir))
            {
                System.IO.Directory.CreateDirectory(buildDir);
            }
            string buildPath = System.IO.Path.Combine(buildDir, "EscapeTheAbandonedLab.exe");

            BuildPlayerOptions opt = new BuildPlayerOptions
            {
                scenes = new string[] {
                    "Assets/_Project/Scenes/LaboratoryLevel.unity",
                    "Assets/_Project/Scenes/Stage2_TestScene.unity"
                },
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[GameFixerAndBuilder] Building player to: " + buildPath);
            var report = BuildPipeline.BuildPlayer(opt);
            Debug.Log("[GameFixerAndBuilder] Build completed: " + report.summary.result + " (" + report.summary.totalErrors + " errors)");
        }
    }
}
#endif
