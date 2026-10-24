#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using EscapeTheLab.Player;
using EscapeTheLab.Environment;
using EscapeTheLab.Interaction;
using EscapeTheLab.Systems;

namespace EscapeTheLab.Editor
{
    /// <summary>
    /// Builds the complete abandoned science laboratory level for Stage 3:
    /// - Security Office (Player Start)
    /// - Corridors with blind corners and interconnected routes
    /// - Research & Specimen Lab (Keycard Zone)
    /// - Monster Containment Chamber (Monster Lair)
    /// - Main Exit Airlock (Escape Objective Zone)
    /// - Atmospheric horror lighting, volumetric fog, and flickering tube lights.
    /// </summary>
    public static class Stage3LabLevelBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/LaboratoryLevel.unity";
        private const string MatDir = "Assets/_Project/Materials";

        [MenuItem("Tools/Escape The Lab/Build Standalone Windows Game")]
        public static void BuildStandalonePlayer()
        {
            string buildDir = "Build";
            if (!System.IO.Directory.Exists(buildDir))
            {
                System.IO.Directory.CreateDirectory(buildDir);
            }
            string buildPath = System.IO.Path.Combine(buildDir, "EscapeTheAbandonedLab.exe");
            string[] scenes = new string[] {
                "Assets/_Project/Scenes/LaboratoryLevel.unity",
                "Assets/_Project/Scenes/Stage2_TestScene.unity"
            };
            BuildPlayerOptions opt = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            Debug.Log("[Stage3LabLevelBuilder] Starting Standalone Windows build to: " + buildPath);
            var report = BuildPipeline.BuildPlayer(opt);
            Debug.Log("[Stage3LabLevelBuilder] Build finished with result: " + report.summary.result);
        }

        [MenuItem("Tools/Escape The Lab/Build Laboratory Level")]
        public static void BuildLevelMenuItem()
        {
            BuildLevel();
            EditorUtility.DisplayDialog(
                "Laboratory Level Created",
                "The complete Abandoned Laboratory level has been created at:\n" + ScenePath + "\n\nPress Play to explore the dark facility with your flashlight!",
                "OK"
            );
        }

        public static void BuildLevelBatchmode()
        {
            Debug.Log("[Stage3LabLevelBuilder] Starting batchmode laboratory level build...");
            BuildLevel();
            Debug.Log("[Stage3LabLevelBuilder] Laboratory level built successfully at: " + ScenePath);
        }

        public static void BuildLevel()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            EnsureDirectories();
            var mats = CreateAllMaterials();

            // Setup Horror Atmosphere & Global Lighting
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.012f, 0.013f, 0.018f); // Very dim, near pitch black
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.006f, 0.007f, 0.010f);
            RenderSettings.fogDensity = 0.045f;

            GameObject labRoot = new GameObject("Laboratory_Environment");

            // Build Room 1: Security Office (Player Start Zone)
            BuildSecurityOffice(labRoot.transform, mats);

            // Build Corridor System (West, East, Central Connecting Hallway)
            BuildCorridors(labRoot.transform, mats);

            // Build Room 2: Research & Specimen Lab (Keycard Location)
            BuildResearchLab(labRoot.transform, mats);

            // Build Room 3: Monster Containment Chamber (Monster Spawn)
            BuildContainmentChamber(labRoot.transform, mats);

            // Build Room 4: Exit Airlock (Escape Objective)
            BuildExitAirlock(labRoot.transform, mats);

            // Instantiate Player Character at Start Room
            BuildPlayer(new Vector3(-13.5f, 0f, -5f));

            // Save and register scene
            EditorSceneManager.SaveScene(newScene, ScenePath);
            AddSceneToBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            if (!AssetDatabase.IsValidFolder(MatDir))
                AssetDatabase.CreateFolder("Assets/_Project", "Materials");
        }

        private struct LabMaterials
        {
            public Material floorTiles;
            public Material wallClean;
            public Material wallDirty;
            public Material metal;
            public Material hazard;
            public Material glass;
            public Material emissiveRed;
            public Material emissiveGreen;
            public Material emissiveCyan;
        }

        private static LabMaterials CreateAllMaterials()
        {
            LabMaterials m = new LabMaterials();
            m.floorTiles = GetOrCreateMaterial("M_Lab_Floor_Tiles", new Color(0.13f, 0.14f, 0.16f), 0.35f);
            m.wallClean = GetOrCreateMaterial("M_Lab_Wall_Clean", new Color(0.24f, 0.26f, 0.28f), 0.1f);
            m.wallDirty = GetOrCreateMaterial("M_Lab_Wall_Dirty", new Color(0.18f, 0.20f, 0.19f), 0.05f);
            m.metal = GetOrCreateMaterial("M_Lab_Metal", new Color(0.16f, 0.18f, 0.20f), 0.55f);
            m.hazard = GetOrCreateMaterial("M_Lab_Hazard", new Color(0.85f, 0.65f, 0.08f), 0.2f);
            m.glass = GetOrCreateGlassMaterial("M_Lab_Glass", new Color(0.15f, 0.45f, 0.40f, 0.45f));
            m.emissiveRed = GetOrCreateEmissiveMaterial("M_Lab_Emissive_Red", new Color(0.1f, 0f, 0f), new Color(1f, 0.1f, 0.1f) * 2.5f);
            m.emissiveGreen = GetOrCreateEmissiveMaterial("M_Lab_Emissive_Green", new Color(0f, 0.1f, 0f), new Color(0.1f, 1f, 0.3f) * 2.5f);
            m.emissiveCyan = GetOrCreateEmissiveMaterial("M_Lab_Emissive_Cyan", new Color(0f, 0.05f, 0.1f), new Color(0.1f, 0.8f, 1f) * 2.0f);
            return m;
        }

        #region Room Builders

        private static void BuildSecurityOffice(Transform root, LabMaterials mats)
        {
            GameObject room = new GameObject("Room_SecurityOffice");
            room.transform.parent = root;

            Vector3 center = new Vector3(-12f, 0f, -5f);
            float width = 9f, length = 9f, height = 4f;

            // Floor & Ceiling
            CreateBox(room.transform, "Floor", center + new Vector3(0, -0.5f, 0), new Vector3(width, 1f, length), mats.floorTiles);
            CreateBox(room.transform, "Ceiling", center + new Vector3(0, height + 0.5f, 0), new Vector3(width, 1f, length), mats.wallDirty);

            // Walls (Door opening on East wall at X = -7.5)
            CreateBox(room.transform, "Wall_West", center + new Vector3(-width * 0.5f - 0.5f, height * 0.5f, 0), new Vector3(1f, height, length), mats.wallClean);
            CreateBox(room.transform, "Wall_North", center + new Vector3(0, height * 0.5f, length * 0.5f + 0.5f), new Vector3(width + 2f, height, 1f), mats.wallClean);
            CreateBox(room.transform, "Wall_South", center + new Vector3(0, height * 0.5f, -length * 0.5f - 0.5f), new Vector3(width + 2f, height, 1f), mats.wallClean);

            // East Wall with 2.2m x 1.4m Doorway
            float doorZ = -5f;
            CreateBox(room.transform, "Wall_East_North", new Vector3(center.x + width * 0.5f + 0.5f, height * 0.5f, -2f), new Vector3(1f, height, 3.5f), mats.wallClean);
            CreateBox(room.transform, "Wall_East_South", new Vector3(center.x + width * 0.5f + 0.5f, height * 0.5f, -8f), new Vector3(1f, height, 3.5f), mats.wallClean);
            CreateBox(room.transform, "Wall_East_Lintel", new Vector3(center.x + width * 0.5f + 0.5f, 3.1f, doorZ), new Vector3(1f, 1.8f, 2f), mats.wallClean);

            // Furniture / Props
            CreateBox(room.transform, "Desk", new Vector3(-15.5f, 0.45f, -3f), new Vector3(1.2f, 0.9f, 2.4f), mats.metal);
            CreateBox(room.transform, "Monitor", new Vector3(-15.4f, 1.15f, -3f), new Vector3(0.2f, 0.5f, 0.8f), mats.emissiveCyan);
            CreateBox(room.transform, "Locker_01", new Vector3(-15.5f, 1.1f, -7.5f), new Vector3(0.8f, 2.2f, 1.0f), mats.metal);
            CreateBox(room.transform, "Locker_02", new Vector3(-14.5f, 1.1f, -7.5f), new Vector3(0.8f, 2.2f, 1.0f), mats.metal);

            // Lighting: Amber flickering security light
            CreateFlickeringLight(room.transform, "OfficeLight", center + new Vector3(0, height - 0.3f, 0), new Color(1.0f, 0.75f, 0.45f), 1.2f, 10f);
        }

        private static void BuildCorridors(Transform root, LabMaterials mats)
        {
            GameObject corridors = new GameObject("Corridors");
            corridors.transform.parent = root;
            float corridorW = 3.5f;
            float height = 4f;

            // 1. West Corridor: X = -5, Z from -10 to 22
            float westZCenter = 6f;
            float westLength = 32f;
            CreateBox(corridors.transform, "West_Floor", new Vector3(-5f, -0.5f, westZCenter), new Vector3(corridorW, 1f, westLength), mats.floorTiles);
            CreateBox(corridors.transform, "West_Ceiling", new Vector3(-5f, height + 0.5f, westZCenter), new Vector3(corridorW, 1f, westLength), mats.wallDirty);
            // West Wall of West Corridor (with doorway to Security Office at Z = -5 and Central link at Z = 8)
            CreateBox(corridors.transform, "West_Wall_S", new Vector3(-5f - corridorW * 0.5f - 0.5f, height * 0.5f, -8f), new Vector3(1f, height, 4f), mats.wallDirty);
            CreateBox(corridors.transform, "West_Wall_M", new Vector3(-5f - corridorW * 0.5f - 0.5f, height * 0.5f, 2.5f), new Vector3(1f, height, 13f), mats.wallDirty);
            CreateBox(corridors.transform, "West_Wall_N", new Vector3(-5f - corridorW * 0.5f - 0.5f, height * 0.5f, 17f), new Vector3(1f, height, 10f), mats.wallDirty);
            // East Wall of West Corridor (with doorway to Central Corridor at Z = 8)
            CreateBox(corridors.transform, "West_EastWall_S", new Vector3(-5f + corridorW * 0.5f + 0.5f, height * 0.5f, -2f), new Vector3(1f, height, 16f), mats.wallDirty);
            CreateBox(corridors.transform, "West_EastWall_N", new Vector3(-5f + corridorW * 0.5f + 0.5f, height * 0.5f, 16f), new Vector3(1f, height, 12f), mats.wallDirty);
            CreateBox(corridors.transform, "West_EastWall_Lintel", new Vector3(-5f + corridorW * 0.5f + 0.5f, 3.1f, 8f), new Vector3(1f, 1.8f, 4f), mats.wallDirty);

            // South dead-end of West Corridor
            CreateBox(corridors.transform, "West_DeadEnd", new Vector3(-5f, height * 0.5f, -10f - 0.5f), new Vector3(corridorW, height, 1f), mats.wallDirty);

            // 2. Central Connecting Corridor: Z = 8, X from -3 to 13
            float centralXCenter = 5f;
            float centralLength = 16f;
            CreateBox(corridors.transform, "Central_Floor", new Vector3(centralXCenter, -0.5f, 8f), new Vector3(centralLength, 1f, corridorW), mats.floorTiles);
            CreateBox(corridors.transform, "Central_Ceiling", new Vector3(centralXCenter, height + 0.5f, 8f), new Vector3(centralLength, 1f, corridorW), mats.wallDirty);
            CreateBox(corridors.transform, "Central_NorthWall", new Vector3(centralXCenter, height * 0.5f, 8f + corridorW * 0.5f + 0.5f), new Vector3(centralLength, height, 1f), mats.wallDirty);
            CreateBox(corridors.transform, "Central_SouthWall", new Vector3(centralXCenter, height * 0.5f, 8f - corridorW * 0.5f - 0.5f), new Vector3(centralLength, height, 1f), mats.wallDirty);

            // Obstacle in Central Corridor: Collapsed ventilation duct & crates
            CreateBox(corridors.transform, "Central_Obstacle_Duct", new Vector3(4f, 0.6f, 7.5f), new Vector3(1.5f, 1.2f, 1.5f), mats.metal);
            CreateBox(corridors.transform, "Central_Obstacle_Crate", new Vector3(5.5f, 0.45f, 8.5f), new Vector3(0.9f, 0.9f, 0.9f), mats.metal);

            // 3. East Corridor: X = 15, Z from -13 to 22
            float eastZCenter = 4.5f;
            float eastLength = 35f;
            CreateBox(corridors.transform, "East_Floor", new Vector3(15f, -0.5f, eastZCenter), new Vector3(corridorW, 1f, eastLength), mats.floorTiles);
            CreateBox(corridors.transform, "East_Ceiling", new Vector3(15f, height + 0.5f, eastZCenter), new Vector3(corridorW, 1f, eastLength), mats.wallDirty);
            // West Wall of East Corridor (with entrance from Central Corridor at Z = 8)
            CreateBox(corridors.transform, "East_WestWall_S", new Vector3(15f - corridorW * 0.5f - 0.5f, height * 0.5f, -3f), new Vector3(1f, height, 20f), mats.wallDirty);
            CreateBox(corridors.transform, "East_WestWall_N", new Vector3(15f - corridorW * 0.5f - 0.5f, height * 0.5f, 16f), new Vector3(1f, height, 12f), mats.wallDirty);
            CreateBox(corridors.transform, "East_WestWall_Lintel", new Vector3(15f - corridorW * 0.5f - 0.5f, 3.1f, 8f), new Vector3(1f, 1.8f, 4f), mats.wallDirty);
            // East Wall of East Corridor (with entrance to Containment Chamber at Z = 12)
            CreateBox(corridors.transform, "East_EastWall_S", new Vector3(15f + corridorW * 0.5f + 0.5f, height * 0.5f, 3f), new Vector3(1f, height, 14f), mats.wallDirty);
            CreateBox(corridors.transform, "East_EastWall_N", new Vector3(15f + corridorW * 0.5f + 0.5f, height * 0.5f, 18f), new Vector3(1f, height, 8f), mats.wallDirty);
            CreateBox(corridors.transform, "East_EastWall_Lintel", new Vector3(15f + corridorW * 0.5f + 0.5f, 3.1f, 12f), new Vector3(1f, 1.8f, 4f), mats.wallDirty);

            // Corridor Lights
            CreateFlickeringLight(corridors.transform, "Light_WestCorr_1", new Vector3(-5f, height - 0.3f, 0f), new Color(0.7f, 0.8f, 0.9f), 0.9f, 8f);
            CreateFlickeringLight(corridors.transform, "Light_WestCorr_2", new Vector3(-5f, height - 0.3f, 14f), new Color(0.7f, 0.8f, 0.9f), 0.9f, 8f);
            CreateFlickeringLight(corridors.transform, "Light_Central", new Vector3(5f, height - 0.3f, 8f), new Color(0.5f, 0.6f, 0.7f), 0.7f, 9f);
            CreateFlickeringLight(corridors.transform, "Light_EastCorr_1", new Vector3(15f, height - 0.3f, 2f), new Color(0.7f, 0.8f, 0.9f), 0.9f, 8f);
            CreateFlickeringLight(corridors.transform, "Light_EastCorr_2", new Vector3(15f, height - 0.3f, 18f), new Color(0.8f, 0.3f, 0.3f), 0.7f, 8f); // Warning red tint
        }

        private static void BuildResearchLab(Transform root, LabMaterials mats)
        {
            GameObject room = new GameObject("Room_ResearchLab");
            room.transform.parent = root;

            Vector3 center = new Vector3(5f, 0f, 28f);
            float width = 24f, length = 12f, height = 4.5f;

            // Floor & Ceiling
            CreateBox(room.transform, "Floor", center + new Vector3(0, -0.5f, 0), new Vector3(width, 1f, length), mats.floorTiles);
            CreateBox(room.transform, "Ceiling", center + new Vector3(0, height + 0.5f, 0), new Vector3(width, 1f, length), mats.wallClean);

            // North Wall (Back)
            CreateBox(room.transform, "Wall_North", center + new Vector3(0, height * 0.5f, length * 0.5f + 0.5f), new Vector3(width, height, 1f), mats.wallClean);
            // East Wall
            CreateBox(room.transform, "Wall_East", center + new Vector3(width * 0.5f + 0.5f, height * 0.5f, 0), new Vector3(1f, height, length), mats.wallClean);
            // West Wall
            CreateBox(room.transform, "Wall_West", center + new Vector3(-width * 0.5f - 0.5f, height * 0.5f, 0), new Vector3(1f, height, length), mats.wallClean);

            // South Wall with 2 entrances connecting to West Corridor and East Corridor
            CreateBox(room.transform, "South_WestPillar", new Vector3(-6f, height * 0.5f, 22f - 0.5f), new Vector3(2f, height, 1f), mats.wallClean);
            CreateBox(room.transform, "South_MiddleWall", new Vector3(5f, height * 0.5f, 22f - 0.5f), new Vector3(14f, height, 1f), mats.wallClean);
            CreateBox(room.transform, "South_EastPillar", new Vector3(16f, height * 0.5f, 22f - 0.5f), new Vector3(2f, height, 1f), mats.wallClean);
            CreateBox(room.transform, "South_Lintel_West", new Vector3(-5f, 3.3f, 22f - 0.5f), new Vector3(2f, 1.4f, 1f), mats.wallClean);
            CreateBox(room.transform, "South_Lintel_East", new Vector3(15f, 3.3f, 22f - 0.5f), new Vector3(2f, 1.4f, 1f), mats.wallClean);

            // 3 Cylindrical Specimen Tanks (Bio-tubes)
            CreateCylinder(room.transform, "SpecimenTank_01", new Vector3(1f, 1.8f, 30f), new Vector3(1.6f, 1.6f, 1.6f), mats.glass);
            CreateCylinder(room.transform, "SpecimenTank_02", new Vector3(5f, 1.8f, 30f), new Vector3(1.6f, 1.6f, 1.6f), mats.glass);
            CreateCylinder(room.transform, "SpecimenTank_03", new Vector3(9f, 1.8f, 30f), new Vector3(1.6f, 1.6f, 1.6f), mats.glass);

            // Lab Tables (Overturned / Abandoned)
            CreateBox(room.transform, "LabTable_Center", new Vector3(5f, 0.45f, 25.5f), new Vector3(4f, 0.9f, 1.6f), mats.metal);
            CreateBox(room.transform, "LabTable_Overturned", new Vector3(-1f, 0.35f, 26f), new Vector3(1.5f, 0.7f, 2.8f), mats.metal).transform.rotation = Quaternion.Euler(0, 25f, 15f);

            // Keycard Pedestal Desk (STAGE 4 OBJECTIVE LOCATION)
            GameObject keycardPedestal = CreateBox(room.transform, "Keycard_Pedestal_Desk", new Vector3(5f, 0.5f, 32f), new Vector3(2.5f, 1.0f, 1.2f), mats.metal);
            keycardPedestal.tag = "Untagged";

            // Small spot glow directly illuminating the keycard desk
            GameObject keycardSpotObj = new GameObject("KeycardSpotlight");
            keycardSpotObj.transform.parent = room.transform;
            keycardSpotObj.transform.position = new Vector3(5f, height - 0.2f, 32f);
            Light keycardSpot = keycardSpotObj.AddComponent<Light>();
            keycardSpot.type = LightType.Spot;
            keycardSpot.range = 8f;
            keycardSpot.spotAngle = 45f;
            keycardSpot.color = new Color(0.2f, 0.9f, 1.0f); // Bright blue sci-fi glow
            keycardSpot.intensity = 2.0f;
            keycardSpot.shadows = LightShadows.Hard;

            // Lab Overhead Lighting (Cold clinical cyan with flicker)
            CreateFlickeringLight(room.transform, "Lab_Light_01", new Vector3(0f, height - 0.3f, 26f), new Color(0.5f, 0.85f, 0.9f), 1.1f, 12f);
            CreateFlickeringLight(room.transform, "Lab_Light_02", new Vector3(10f, height - 0.3f, 26f), new Color(0.5f, 0.85f, 0.9f), 1.1f, 12f);
        }

        private static void BuildContainmentChamber(Transform root, LabMaterials mats)
        {
            GameObject room = new GameObject("Room_MonsterContainment");
            room.transform.parent = root;

            Vector3 center = new Vector3(23f, 0f, 12f);
            float width = 12f, length = 12f, height = 4.5f;

            // Floor & Ceiling
            CreateBox(room.transform, "Floor", center + new Vector3(0, -0.5f, 0), new Vector3(width, 1f, length), mats.hazard);
            CreateBox(room.transform, "Ceiling", center + new Vector3(0, height + 0.5f, 0), new Vector3(width, 1f, length), mats.wallDirty);

            // Walls (Doorway from East Corridor at X = 17)
            CreateBox(room.transform, "Wall_East", center + new Vector3(width * 0.5f + 0.5f, height * 0.5f, 0), new Vector3(1f, height, length), mats.metal);
            CreateBox(room.transform, "Wall_North", center + new Vector3(0, height * 0.5f, length * 0.5f + 0.5f), new Vector3(width + 2f, height, 1f), mats.metal);
            CreateBox(room.transform, "Wall_South", center + new Vector3(0, height * 0.5f, -length * 0.5f - 0.5f), new Vector3(width + 2f, height, 1f), mats.metal);

            // West Wall with breached doorway
            CreateBox(room.transform, "WestWall_North", new Vector3(17f - 0.5f, height * 0.5f, 15f), new Vector3(1f, height, 4f), mats.metal);
            CreateBox(room.transform, "WestWall_South", new Vector3(17f - 0.5f, height * 0.5f, 9f), new Vector3(1f, height, 4f), mats.metal);
            CreateBox(room.transform, "WestWall_Lintel", new Vector3(17f - 0.5f, 3.3f, 12f), new Vector3(1f, 1.4f, 4f), mats.hazard);

            // Shattered Caging / Steel Cells
            CreateBox(room.transform, "Cell_BackWall", new Vector3(27f, 1.5f, 12f), new Vector3(0.5f, 3f, 6f), mats.metal);
            CreateBox(room.transform, "Cell_NorthWall", new Vector3(24.5f, 1.5f, 15f), new Vector3(5f, 3f, 0.5f), mats.metal);
            CreateBox(room.transform, "Cell_SouthWall", new Vector3(24.5f, 1.5f, 9f), new Vector3(5f, 3f, 0.5f), mats.metal);
            // Smashed cell bars / debris
            GameObject debris = CreateBox(room.transform, "Cell_Debris", new Vector3(22f, 0.3f, 12f), new Vector3(2.5f, 0.4f, 1.8f), mats.metal);
            debris.transform.rotation = Quaternion.Euler(15f, 40f, -10f);

            // Red Emergency Strobe Light (Flickering Warning Alarm)
            CreateFlickeringLight(room.transform, "Containment_AlarmLight", center + new Vector3(0, height - 0.3f, 0), new Color(1.0f, 0.15f, 0.15f), 1.5f, 14f);
        }

        private static void BuildExitAirlock(Transform root, LabMaterials mats)
        {
            GameObject room = new GameObject("Room_ExitAirlock");
            room.transform.parent = root;

            Vector3 center = new Vector3(15f, 0f, -18f);
            float width = 8f, length = 10f, height = 4f;

            // Floor & Ceiling
            CreateBox(room.transform, "Floor", center + new Vector3(0, -0.5f, 0), new Vector3(width, 1f, length), mats.floorTiles);
            CreateBox(room.transform, "Ceiling", center + new Vector3(0, height + 0.5f, 0), new Vector3(width, 1f, length), mats.metal);

            // Walls (North entrance from East corridor at Z = -13)
            CreateBox(room.transform, "Wall_West", center + new Vector3(-width * 0.5f - 0.5f, height * 0.5f, 0), new Vector3(1f, height, length), mats.metal);
            CreateBox(room.transform, "Wall_East", center + new Vector3(width * 0.5f + 0.5f, height * 0.5f, 0), new Vector3(1f, height, length), mats.metal);

            // North Wall with doorway
            CreateBox(room.transform, "NorthWall_West", new Vector3(12.5f, height * 0.5f, -13f + 0.5f), new Vector3(2f, height, 1f), mats.metal);
            CreateBox(room.transform, "NorthWall_East", new Vector3(17.5f, height * 0.5f, -13f + 0.5f), new Vector3(2f, height, 1f), mats.metal);
            CreateBox(room.transform, "NorthWall_Lintel", new Vector3(15f, 3.1f, -13f + 0.5f), new Vector3(3f, 1.8f, 1f), mats.hazard);

            // Massive Blast Door at the Far End (STAGE 4 OBJECTIVE DOOR)
            GameObject exitBlastDoor = CreateBox(room.transform, "Exit_BlastDoor_Locked", new Vector3(15f, 1.8f, -23f), new Vector3(4f, 3.6f, 0.6f), mats.hazard);
            exitBlastDoor.tag = "Untagged";

            // Frame for Blast Door
            CreateBox(room.transform, "BlastDoor_Frame_Left", new Vector3(12.5f, height * 0.5f, -23f), new Vector3(1f, height, 1f), mats.metal);
            CreateBox(room.transform, "BlastDoor_Frame_Right", new Vector3(17.5f, height * 0.5f, -23f), new Vector3(1f, height, 1f), mats.metal);
            CreateBox(room.transform, "BlastDoor_Frame_Top", new Vector3(15f, 3.8f, -23f), new Vector3(6f, 0.8f, 1f), mats.metal);

            // Keycard Terminal Pillar
            GameObject terminal = CreateBox(room.transform, "Keycard_Terminal_Scanner", new Vector3(17.3f, 1.2f, -22.3f), new Vector3(0.3f, 0.5f, 0.2f), mats.emissiveRed);

            // Green Exit Sign Light
            GameObject exitLightObj = new GameObject("ExitSignGlow");
            exitLightObj.transform.parent = room.transform;
            exitLightObj.transform.position = new Vector3(15f, 3.5f, -22.5f);
            Light exitLight = exitLightObj.AddComponent<Light>();
            exitLight.type = LightType.Point;
            exitLight.color = new Color(0.2f, 1.0f, 0.4f);
            exitLight.intensity = 1.0f;
            exitLight.range = 7f;

            // Dim white airlock overhead light
            CreateFlickeringLight(room.transform, "Airlock_Overhead", center + new Vector3(0, height - 0.3f, 0), new Color(0.8f, 0.85f, 0.9f), 0.8f, 9f);
        }

        #endregion

        #region Player Builder

        public static void BuildPlayer(Vector3 spawnPos)
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = spawnPos;

            CharacterController charController = player.AddComponent<CharacterController>();
            charController.height = 2.0f;
            charController.radius = 0.45f;
            charController.center = new Vector3(0, 1.0f, 0);
            charController.stepOffset = 0.3f;
            charController.slopeLimit = 45f;

            FirstPersonMovement movement = player.AddComponent<FirstPersonMovement>();

            // Player Camera
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.transform.parent = player.transform;
            cameraObj.transform.localPosition = new Vector3(0, 1.7f, 0); // Eye height

            Camera cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 90f;
            cam.fieldOfView = 65f;

            cameraObj.AddComponent<AudioListener>();

            MouseLook mouseLook = cameraObj.AddComponent<MouseLook>();
            mouseLook.SetPlayerBody(player.transform);
            movement.SetCameraTransform(cameraObj.transform);

            // Flashlight
            GameObject flashlightObj = new GameObject("Flashlight");
            flashlightObj.transform.parent = cameraObj.transform;
            flashlightObj.transform.localPosition = new Vector3(0.25f, -0.22f, 0.35f);
            flashlightObj.transform.localRotation = Quaternion.identity;

            Light flashLight = flashlightObj.AddComponent<Light>();
            flashLight.type = LightType.Spot;
            flashLight.range = 25f;
            flashLight.spotAngle = 48f;
            flashLight.innerSpotAngle = 32f;
            flashLight.intensity = 3.0f;
            flashLight.color = new Color(1.0f, 0.95f, 0.85f);
            flashLight.shadows = LightShadows.Soft;

            AudioSource flashAudio = flashlightObj.AddComponent<AudioSource>();
            flashAudio.playOnAwake = false;
            flashAudio.spatialBlend = 0f;

            FlashlightController flashController = flashlightObj.AddComponent<FlashlightController>();
            flashController.SetupReferences(flashLight, cameraObj.transform, flashAudio);

            // Interaction System (Allows player to raycast and open doors, flip switches, pick up items)
            PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
            if (interaction == null) interaction = player.AddComponent<PlayerInteraction>();
            if (cam != null) interaction.SetCamera(cam);

            // Inventory Manager (4-slot inventory for holding, throwing and using equipment)
            InventoryManager im = player.GetComponent<InventoryManager>();
            if (im == null) im = player.AddComponent<InventoryManager>();
        }

        #endregion

        #region Helpers

        private static GameObject CreateBox(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.parent = parent;
            box.transform.position = pos;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = mat;

            // Mark as static for NavMesh and GI
            GameObjectUtility.SetStaticEditorFlags(box, StaticEditorFlags.ContributeGI | StaticEditorFlags.NavigationStatic);
            return box;
        }

        private static GameObject CreateCylinder(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.name = name;
            cyl.transform.parent = parent;
            cyl.transform.position = pos;
            cyl.transform.localScale = scale;
            cyl.GetComponent<Renderer>().sharedMaterial = mat;

            GameObjectUtility.SetStaticEditorFlags(cyl, StaticEditorFlags.ContributeGI | StaticEditorFlags.NavigationStatic);
            return cyl;
        }

        private static GameObject CreateFlickeringLight(Transform parent, string name, Vector3 pos, Color color, float intensity, float range)
        {
            GameObject lightObj = new GameObject(name);
            lightObj.transform.parent = parent;
            lightObj.transform.position = pos;

            Light lt = lightObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = color;
            lt.intensity = intensity;
            lt.range = range;
            lt.shadows = LightShadows.Soft;

            LightFlicker flicker = lightObj.AddComponent<LightFlicker>();
            flicker.SetupReferences(lt, intensity);

            return lightObj;
        }

        private static Material GetOrCreateMaterial(string matName, Color color, float smoothness)
        {
            string path = $"{MatDir}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Diffuse");
                mat = new Material(shader) { color = color };
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static Material GetOrCreateEmissiveMaterial(string matName, Color baseColor, Color emissiveColor)
        {
            string path = $"{MatDir}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Diffuse");
                mat = new Material(shader) { color = baseColor };
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissiveColor);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static Material GetOrCreateGlassMaterial(string matName, Color color)
        {
            string path = $"{MatDir}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Transparent/Diffuse");
                mat = new Material(shader) { color = color };

                // Set Standard Shader to Transparent mode
                if (mat.HasProperty("_Mode"))
                {
                    mat.SetFloat("_Mode", 3f); // 3 = Transparent
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = 3000;
                }
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.85f);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] original = EditorBuildSettings.scenes;
            foreach (var s in original)
            {
                if (s.path == scenePath) return;
            }

            EditorBuildSettingsScene[] updated = new EditorBuildSettingsScene[original.Length + 1];
            System.Array.Copy(original, updated, original.Length);
            updated[updated.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
        }

        #endregion
    }
}
#endif
