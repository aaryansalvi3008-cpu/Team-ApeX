#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using EscapeTheLab.Player;
using EscapeTheLab.AI;
using EscapeTheLab.Interaction;
using EscapeTheLab.Systems;
using EscapeTheLab.UI;
using EscapeTheLab.Core;
using EscapeTheLab.Environment;

namespace EscapeTheLab.Editor
{
    /// <summary>
    /// Implements the complete Game Design Document specification for "Escape the Abandoned Lab" (Project VANTA):
    /// - 5 Lockdown Cycles manager & Medical Bay spawn/respawn.
    /// - Sound propagation NoiseManager with wall occlusion.
    /// - 3-Circuit Power Grid (Lights, Doors, Systems) with interactive Reactor Breakers.
    /// - 4-Slot Inventory & Droppable items with distraction noise physics.
    /// - Hiding Lockers with 0.5s transitions and breath-hold mechanic.
    /// - The Subject AI (weak vision, acute hearing, patrol/investigate/hunt/search states).
    /// - Metal grating floor sections (+3m noise penalty).
    /// - Exit Airlock requiring Systems Circuit power + Security Keycard.
    /// - Complete NavMesh baking and standalone executable build.
    /// </summary>
    public static class Stage8GDDLevelBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/LaboratoryLevel.unity";
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string MatDir = "Assets/_Project/Materials";

        [MenuItem("Tools/Escape The Lab/Apply Full GDD Design and Rebuild")]
        public static void ApplyGDDMenuItem()
        {
            ApplyGDDDesign();
            EditorUtility.DisplayDialog(
                "GDD Design Applied",
                "The complete Project VANTA game design has been applied:\n" +
                "• 5 Lockdown Cycles & Respawn\n" +
                "• Dynamic Sound Propagation & Noise Meter\n" +
                "• 3-Circuit Power Grid & Breakers\n" +
                "• 4-Slot Inventory & Drop Distractions\n" +
                "• The Subject AI (Patrol, Investigate, Hunt, Search)\n" +
                "• Hiding Lockers & Breath Hold\n\nRebuilding Standalone Game Executable...",
                "OK"
            );
            RebuildStandaloneGame();
        }

        public static void SetupGDDAndRebuildBatchmode()
        {
            Debug.Log("[Stage8GDDLevelBuilder] Applying Full GDD Design in batchmode...");
            ApplyGDDDesign();
            Debug.Log("[Stage8GDDLevelBuilder] Scene updated. Rebuilding Standalone Windows Player...");
            RebuildStandaloneGame();
        }

        public static void ApplyGDDDesign()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Ensure Materials
            Material metalGratingMat = GetOrCreateMaterial("M_Lab_Metal_Grating", new Color(0.18f, 0.20f, 0.22f), 0.7f);
            Material lockerMat = GetOrCreateMaterial("M_Lab_Locker_Metal", new Color(0.12f, 0.15f, 0.18f), 0.5f);
            Material batteryMat = GetOrCreateEmissiveMaterial("M_Lab_Battery_Glow", new Color(0.2f, 0.2f, 0f), new Color(1f, 0.85f, 0.1f) * 2f);
            Material lureMat = GetOrCreateEmissiveMaterial("M_Lab_Lure_Glow", new Color(0f, 0.2f, 0.2f), new Color(0.1f, 0.9f, 0.9f) * 2f);

            // 1. Setup Core System Managers in Scene
            SetupSystemManagers();

            // 2. Setup Metal Grating Floor Hazards (Corridor)
            SetupMetalGratingFloors(metalGratingMat);

            // 3. Setup Hiding Lockers
            SetupHidingLockers(lockerMat);

            // 4. Setup Power Grid Breaker Console (Reactor Room)
            SetupPowerGridBreakers();

            // 5. Setup Collectible Inventory Items (Batteries & Sonic Lures)
            SetupCollectibleItems(batteryMat, lureMat);

            // 6. Connect Overhead Lights to PowerGridManager
            RegisterAllOverheadLights();

            // 7. Update The Subject AI and Spawn/Containment
            SetupTheSubject();

            // 8. Update Exit Airlock
            SetupExitAirlock();

            // 9. Bake Unity NavMesh
            Debug.Log("[Stage8GDDLevelBuilder] Baking NavMesh for updated layout...");
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            Debug.Log("[Stage8GDDLevelBuilder] NavMesh baked successfully.");

            // Save Scene
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void SetupSystemManagers()
        {
            GameObject managersRoot = GameObject.Find("Core_Systems_Managers");
            if (managersRoot == null)
            {
                managersRoot = new GameObject("Core_Systems_Managers");
            }

            // NoiseManager
            if (managersRoot.GetComponent<NoiseManager>() == null)
            {
                managersRoot.AddComponent<NoiseManager>();
            }

            // PowerGridManager
            PowerGridManager pgm = managersRoot.GetComponent<PowerGridManager>();
            if (pgm == null)
            {
                pgm = managersRoot.AddComponent<PowerGridManager>();
            }

            // LockdownCycleManager
            LockdownCycleManager lcm = managersRoot.GetComponent<LockdownCycleManager>();
            if (lcm == null)
            {
                lcm = managersRoot.AddComponent<LockdownCycleManager>();
            }

            // Setup Spawn Points for Lockdown Cycles
            Transform medBaySpawn = GetOrCreateTransform("MedicalBay_SpawnPoint", new Vector3(0f, 0.2f, 0f), Quaternion.identity);
            Transform subjectContainment = GetOrCreateTransform("Subject_Containment_SpawnPoint", new Vector3(23f, 0.2f, 12f), Quaternion.identity);
            lcm.SetupSpawnPoints(medBaySpawn, subjectContainment);

            // Player Inventory Manager
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player != null && player.GetComponent<InventoryManager>() == null)
            {
                player.AddComponent<InventoryManager>();
            }
        }

        private static void SetupMetalGratingFloors(Material gratingMat)
        {
            GameObject existing = GameObject.Find("Metal_Grating_Zones");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            GameObject root = new GameObject("Metal_Grating_Zones");

            // Corridor junction metal grating floor plate (High noise risk zone)
            GameObject grate1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grate1.name = "MetalGrating_Central";
            grate1.transform.parent = root.transform;
            grate1.transform.position = new Vector3(5f, 0.05f, 8f);
            grate1.transform.localScale = new Vector3(3.2f, 0.1f, 6.0f);
            if (gratingMat != null) grate1.GetComponent<Renderer>().sharedMaterial = gratingMat;

            // East Corridor metal grating plate
            GameObject grate2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grate2.name = "MetalGrating_East";
            grate2.transform.parent = root.transform;
            grate2.transform.position = new Vector3(15f, 0.05f, 10f);
            grate2.transform.localScale = new Vector3(2.8f, 0.1f, 5.0f);
            if (gratingMat != null) grate2.GetComponent<Renderer>().sharedMaterial = gratingMat;
        }

        private static void SetupHidingLockers(Material lockerMat)
        {
            GameObject existing = GameObject.Find("Hiding_Lockers");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            GameObject root = new GameObject("Hiding_Lockers");

            // Locker 1: In West Corridor
            CreateLocker("Locker_WestCorridor", root.transform, new Vector3(-6.2f, 0f, 14f), Quaternion.Euler(0, 90f, 0), lockerMat);

            // Locker 2: In Research Lab
            CreateLocker("Locker_ResearchLab", root.transform, new Vector3(1.5f, 0f, 29.2f), Quaternion.Euler(0, 180f, 0), lockerMat);

            // Locker 3: In East Corridor near Containment
            CreateLocker("Locker_EastCorridor", root.transform, new Vector3(16.2f, 0f, 5f), Quaternion.Euler(0, -90f, 0), lockerMat);
        }

        private static void CreateLocker(string name, Transform parent, Vector3 pos, Quaternion rot, Material mat)
        {
            GameObject locker = new GameObject(name);
            locker.transform.parent = parent;
            locker.transform.position = pos;
            locker.transform.rotation = rot;

            // Visual Frame (Outer Box)
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Locker_Frame";
            frame.transform.parent = locker.transform;
            frame.transform.localPosition = new Vector3(0, 1.25f, 0);
            frame.transform.localScale = new Vector3(1.1f, 2.5f, 0.9f);
            if (mat != null) frame.GetComponent<Renderer>().sharedMaterial = mat;

            // Interior Camera Point
            GameObject camPoint = new GameObject("InteriorCameraPoint");
            camPoint.transform.parent = locker.transform;
            camPoint.transform.localPosition = new Vector3(0, 1.5f, -0.1f);

            // Exit Point
            GameObject exitPoint = new GameObject("ExitPoint");
            exitPoint.transform.parent = locker.transform;
            exitPoint.transform.localPosition = new Vector3(0, 0.1f, 1.2f);

            // Add HidingSpot component and trigger collider
            HidingSpot hs = locker.AddComponent<HidingSpot>();

            BoxCollider col = locker.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.25f, 0.5f);
            col.size = new Vector3(1.4f, 2.5f, 1.4f);
            col.isTrigger = false;
        }

        private static void SetupPowerGridBreakers()
        {
            GameObject existing = GameObject.Find("PowerGrid_Console_Station");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            GameObject root = new GameObject("PowerGrid_Console_Station");
            root.transform.position = new Vector3(5f, 0f, 15f); // Near reactor center

            Material panelMat = GetOrCreateMaterial("M_Lab_Metal", new Color(0.2f, 0.22f, 0.25f), 0.6f);

            // Console Stand
            GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stand.name = "BreakerConsole_Stand";
            stand.transform.parent = root.transform;
            stand.transform.localPosition = new Vector3(0, 0.6f, 0);
            stand.transform.localScale = new Vector3(2.4f, 1.2f, 0.6f);
            if (panelMat != null) stand.GetComponent<Renderer>().sharedMaterial = panelMat;

            // 3 Circuit Breakers: Lights, Doors, Systems
            CreateBreakerSwitch("Breaker_Lights", root.transform, new Vector3(-0.7f, 1.35f, -0.1f), CircuitType.Lights);
            CreateBreakerSwitch("Breaker_Doors", root.transform, new Vector3(0f, 1.35f, -0.1f), CircuitType.Doors);
            CreateBreakerSwitch("Breaker_Systems", root.transform, new Vector3(0.7f, 1.35f, -0.1f), CircuitType.Systems);
        }

        private static void CreateBreakerSwitch(string name, Transform parent, Vector3 localPos, CircuitType circuit)
        {
            GameObject switchObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            switchObj.name = name;
            switchObj.transform.parent = parent;
            switchObj.transform.localPosition = localPos;
            switchObj.transform.localScale = new Vector3(0.35f, 0.45f, 0.2f);

            // Indicator LED
            GameObject led = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            led.name = "LED_Indicator";
            led.transform.parent = switchObj.transform;
            led.transform.localPosition = new Vector3(0, 0.35f, -0.55f);
            led.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

            CircuitBreakerConsole breaker = switchObj.AddComponent<CircuitBreakerConsole>();

            // Setup field via reflection/setup
            var fieldCircuit = typeof(CircuitBreakerConsole).GetField("targetCircuit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldCircuit != null) fieldCircuit.SetValue(breaker, circuit);

            var fieldRenderer = typeof(CircuitBreakerConsole).GetField("statusIndicatorRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldRenderer != null) fieldRenderer.SetValue(breaker, led.GetComponent<Renderer>());
        }

        private static void SetupCollectibleItems(Material batteryMat, Material lureMat)
        {
            GameObject existing = GameObject.Find("World_Pickups");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            GameObject root = new GameObject("World_Pickups");

            // Battery 1: On Medical Bay Desk
            CreateItemPickup("Pickup_Battery_MedBay", root.transform, new Vector3(-1.2f, 1.1f, -0.5f), "batt_1", "Flashlight Battery", ItemType.BatteryPack, Color.yellow, batteryMat);

            // Battery 2: In Research Lab
            CreateItemPickup("Pickup_Battery_ResearchLab", root.transform, new Vector3(8.5f, 1.0f, 27f), "batt_2", "Flashlight Battery", ItemType.BatteryPack, Color.yellow, batteryMat);

            // Sonic Lure: In Reactor Room
            CreateItemPickup("Pickup_SonicLure_Reactor", root.transform, new Vector3(6.5f, 1.0f, 14.5f), "lure_1", "Sonic Lure", ItemType.SonicLure, Color.cyan, lureMat);
        }

        private static void CreateItemPickup(string name, Transform parent, Vector3 pos, string id, string displayName, ItemType type, Color color, Material mat)
        {
            GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pickup.name = name;
            pickup.transform.parent = parent;
            pickup.transform.position = pos;
            pickup.transform.localScale = new Vector3(0.2f, 0.12f, 0.2f);
            if (mat != null) pickup.GetComponent<Renderer>().sharedMaterial = mat;

            Collider col = pickup.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            ItemPickup item = pickup.AddComponent<ItemPickup>();
            item.SetupItem(id, displayName, type, color);
        }

        private static void RegisterAllOverheadLights()
        {
            if (PowerGridManager.Instance == null) return;

            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light lt in lights)
            {
                if (lt.type == LightType.Point && lt.gameObject.name.ToLower().Contains("light"))
                {
                    PowerGridManager.Instance.RegisterOverheadLight(lt);
                }
            }
        }

        private static void SetupTheSubject()
        {
            MonsterController ctrl = UnityEngine.Object.FindAnyObjectByType<MonsterController>();
            if (ctrl == null)
            {
                GameObject monster = GameObject.Find("Monster");
                if (monster != null) ctrl = monster.GetComponent<MonsterController>();
            }

            if (ctrl != null)
            {
                NavMeshAgent agent = ctrl.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.speed = 1.8f;
                    agent.acceleration = 16f;
                    agent.stoppingDistance = 0.5f;
                }
            }
        }

        private static void SetupExitAirlock()
        {
            GameObject exitDoorObj = GameObject.Find("AirlockDoor_MovingPanel");
            if (exitDoorObj == null) exitDoorObj = GameObject.Find("ExitAirlockDoor");
            if (exitDoorObj == null)
            {
                DoorController[] doors = UnityEngine.Object.FindObjectsByType<DoorController>(FindObjectsSortMode.None);
                foreach (var d in doors)
                {
                    if (d.gameObject.name.ToLower().Contains("airlock"))
                    {
                        d.SetupDoor(true, true, true);
                    }
                }
            }
            else
            {
                DoorController dc = exitDoorObj.GetComponent<DoorController>();
                if (dc != null)
                {
                    dc.SetupDoor(true, true, true);
                }
            }
        }

        private static void RebuildStandaloneGame()
        {
            // Configure Build Settings scenes
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = scenes;

            PlayerSettings.productName = "Escape the Abandoned Lab";
            PlayerSettings.companyName = "IndieHorror";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            GraphicsDeviceType[] apis = new GraphicsDeviceType[] {
                GraphicsDeviceType.Direct3D11,
                GraphicsDeviceType.Direct3D12
            };
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, apis);

            AssetDatabase.SaveAssets();

            string buildDir = "Build";
            if (!System.IO.Directory.Exists(buildDir)) System.IO.Directory.CreateDirectory(buildDir);

            string buildPath = System.IO.Path.Combine(buildDir, "EscapeTheAbandonedLab.exe");

            BuildPlayerOptions opt = new BuildPlayerOptions
            {
                scenes = new string[] { MainMenuScenePath, ScenePath },
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Stage8GDDLevelBuilder] Compiling Standalone Player to: " + buildPath);
            var report = BuildPipeline.BuildPlayer(opt);
            Debug.Log("[Stage8GDDLevelBuilder] Build result: " + report.summary.result + " (Errors: " + report.summary.totalErrors + ")");
        }

        private static Transform GetOrCreateTransform(string name, Vector3 pos, Quaternion rot)
        {
            GameObject obj = GameObject.Find(name);
            if (obj == null)
            {
                obj = new GameObject(name);
            }
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            return obj.transform;
        }

        private static Material GetOrCreateMaterial(string name, Color albedo, float smoothness)
        {
            string path = $"{MatDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"))
                {
                    color = albedo
                };
                mat.SetFloat("_Glossiness", smoothness);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static Material GetOrCreateEmissiveMaterial(string name, Color albedo, Color emission)
        {
            string path = $"{MatDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"))
                {
                    color = albedo
                };
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }
    }
}
#endif
