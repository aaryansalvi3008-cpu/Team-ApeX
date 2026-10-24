#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EscapeTheLab.UI;
using EscapeTheLab.Environment;

namespace EscapeTheLab.Editor
{
    /// <summary>
    /// Constructs the Stage 7 Main Menu scene and registers full game loop:
    /// - MainMenu.unity with dynamic 3D laboratory backdrop and horror drone audio.
    /// - Start Game, Controls / How to Play guide, and Quit buttons.
    /// - Configures Build Settings with MainMenu at index 0.
    /// - Rebuilds the complete standalone game player.
    /// </summary>
    public static class Stage7MenuBuilder
    {
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string GameScenePath = "Assets/_Project/Scenes/LaboratoryLevel.unity";
        private const string TestScenePath = "Assets/_Project/Scenes/Stage2_TestScene.unity";

        [MenuItem("Tools/Escape The Lab/Setup Stage 7 Menus and Rebuild")]
        public static void SetupStage7MenuItem()
        {
            SetupStage7();
            EditorUtility.DisplayDialog(
                "Stage 7 Menus Built",
                "Main Menu, Pause Menu, Game Over & Victory systems have been configured!\n\nRebuilding standalone executable...",
                "OK"
            );
            RebuildStandalone();
        }

        public static void SetupStage7Batchmode()
        {
            Debug.Log("[Stage7MenuBuilder] Building Stage 7 Main Menu Scene...");
            SetupStage7();
            Debug.Log("[Stage7MenuBuilder] Main Menu built. Rebuilding standalone game player...");
            RebuildStandalone();
        }

        public static void SetupStage7()
        {
            var menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Load Materials
            Material floorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_Lab_Floor_Tiles.mat");
            Material wallMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_Lab_Wall_Clean.mat");
            Material metalMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_Lab_Metal.mat");

            // 1. Camera & AudioListener
            GameObject camObj = new GameObject("Menu_Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
            camObj.transform.position = new Vector3(0f, 1.6f, -3.5f);
            camObj.transform.rotation = Quaternion.Euler(6f, 0f, 0f);
            camObj.AddComponent<AudioListener>();

            // 2. Menu Controller Manager
            GameObject menuMgr = new GameObject("MainMenu_Manager");
            menuMgr.AddComponent<MainMenuController>();

            // 3. Spooky 3D Backdrop
            GameObject backdrop = new GameObject("Menu_Backdrop");

            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.parent = backdrop.transform;
            floor.transform.position = new Vector3(0, -0.5f, 2f);
            floor.transform.localScale = new Vector3(14f, 1f, 14f);
            if (floorMat != null) floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // Back Wall with Airlock Blast Door
            GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.name = "BackWall";
            backWall.transform.parent = backdrop.transform;
            backWall.transform.position = new Vector3(0, 2.5f, 6f);
            backWall.transform.localScale = new Vector3(14f, 6f, 1f);
            if (wallMat != null) backWall.GetComponent<Renderer>().sharedMaterial = wallMat;

            // Blast Door Inset
            GameObject blastDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blastDoor.name = "BlastDoor_Visual";
            blastDoor.transform.parent = backdrop.transform;
            blastDoor.transform.position = new Vector3(0, 1.8f, 5.4f);
            blastDoor.transform.localScale = new Vector3(3.5f, 3.6f, 0.4f);
            if (metalMat != null) blastDoor.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Dim Flickering Light
            GameObject lightObj = new GameObject("MenuFlickerLight");
            lightObj.transform.parent = backdrop.transform;
            lightObj.transform.position = new Vector3(0, 3.8f, 2f);
            Light lt = lightObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(0.85f, 0.35f, 0.35f); // Atmospheric amber-red
            lt.intensity = 0.9f;
            lt.range = 10f;
            lt.shadows = LightShadows.Soft;

            LightFlicker flicker = lightObj.AddComponent<LightFlicker>();
            flicker.SetupReferences(lt, 0.9f);

            // Save Main Menu Scene
            EditorSceneManager.SaveScene(menuScene, MainMenuScenePath);

            // Register in Build Settings (MainMenu at index 0)
            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(GameScenePath, true),
                new EditorBuildSettingsScene(TestScenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void RebuildStandalone()
        {
            string buildPath = "Build/EscapeTheAbandonedLab.exe";
            BuildPlayerOptions opt = new BuildPlayerOptions
            {
                scenes = new string[] {
                    MainMenuScenePath,
                    GameScenePath,
                    TestScenePath
                },
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            Debug.Log("[Stage7MenuBuilder] Building Standalone Player with Main Menu to: " + buildPath);
            var report = BuildPipeline.BuildPlayer(opt);
            Debug.Log("[Stage7MenuBuilder] Build completed: " + report.summary.result);
        }
    }
}
#endif
