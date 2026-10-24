#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
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
    /// Master Horror Environment Director:
    /// Transforms "Escape the Abandoned Lab" into a polished, atmospheric gothic survival horror experience:
    /// - Level 1 (Upper Labs) -> Level 2 (Sub-Basement Cryo Labs & Morgue) multi-level progression.
    /// - High-res PBR procedural textures (Tiles, Steel, Grates, Screens, Skins, Bones).
    /// - Believable abandoned Gothic laboratory & asylum props: Autopsy dissection slabs with body bags,
    ///   specimen shelves with glowing jars, restraint chairs, examination carts, and flickering lights.
    /// - Reconstructs "The Subject" with hunched human-scale predator height (~1.92m),
    ///   tuned fair speeds (Patrol 1.15m/s, Hunt 3.25m/s), exposed spine vertebrae, flayed ribcage,
    ///   cranial horns, dropped lower jaw chattering, razor fangs, and blood-red eye light.
    /// - Balanced dynamic room hazards: Broken glass floor hazards (crunch alerts monster unless crouching),
    ///   dangling live electric spark wires, and randomized escape room item placement.
    /// - Rebuilds standalone player for Level 1 & Level 2!
    /// </summary>
    public static class HorrorEnvironmentDirector
    {
        private const string ScenePath = "Assets/_Project/Scenes/LaboratoryLevel.unity";
        private const string ScenePath2 = "Assets/_Project/Scenes/LaboratoryLevel2.unity";
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string MatDir = "Assets/_Project/Materials";

        [MenuItem("Tools/Escape The Lab/Apply Full Horror Redesign and Build")]
        public static void ApplyFullRedesignMenuItem()
        {
            ApplyHorrorRedesign();
            EditorUtility.DisplayDialog(
                "Horror Redesign Applied",
                "Project VANTA has been transformed with Gothic Horror architecture, multi-level progression (Level 1 & Level 2), balanced monster speeds & height, broken glass stealth hazards, and randomized escape room placements.\n\nRebuilding Standalone Game Executable...",
                "OK"
            );
            RebuildStandalone();
        }

        public static void ApplyRedesignBatchmode()
        {
            Debug.Log("[HorrorEnvironmentDirector] Starting full horror redesign batchmode...");
            ApplyHorrorRedesign();
            Debug.Log("[HorrorEnvironmentDirector] Redesign complete. Rebuilding standalone game player...");
            RebuildStandalone();
        }

        public static void ApplyHorrorRedesign()
        {
            // 1. Generate and Apply PBR Texture Suite
            ProceduralTextureGenerator.GenerateAllTextures();

            // Material References (Gothic Asylum Suite matching Image 1 & Image 2)
            Material greenTilesMat = GetMat("M_Asylum_Green_Tiles");
            Material wardWallMat = GetMat("M_Asylum_Ward_Wall");
            Material corridorFloorMat = GetMat("M_Asylum_Corridor_Floor");
            Material corridorWallMat = GetMat("M_Asylum_Corridor_Wall");
            Material paperMat = GetMat("M_Asylum_Patient_Paper");
            Material ceilingMat = GetMat("M_Asylum_Ceiling_Tiles");
            Material doorMat = GetMat("M_Asylum_Door");
            Material windowMat = GetMat("M_Asylum_Window_Bars");

            Material floorTilesMat = GetMat("M_Lab_Floor_Tiles");
            Material wallCleanMat = GetMat("M_Lab_Wall_Clean");
            Material wallMat = GetMat("M_Lab_Wall");
            Material metalMat = GetMat("M_Lab_Metal");
            Material metalGratingMat = GetMat("M_Lab_Metal_Grating");
            Material hazardMat = GetMat("M_Lab_Hazard");
            Material screenMat = GetMat("M_Lab_Monitor_Screen");
            Material glassMat = GetMat("M_Lab_Glass");
            Material skinMat = GetMat("M_Monster_Skin");
            Material gownMat = GetMat("M_Monster_Gown");
            Material eyeMat = GetMat("M_Monster_Eyes");
            Material batteryMat = GetMat("M_Lab_Battery_Glow");
            Material lureMat = GetMat("M_Lab_Lure_Glow");
            Material boneMat = GetMat("M_Monster_Bone");
            Material autopsyMat = GetMat("M_Lab_Autopsy_Steel");
            Material bloodMat = GetMat("M_Lab_Blood_Decal");

            // ==========================================
            // BUILD & OVERHAUL LEVEL 1 (Upper Labs)
            // ==========================================
            Debug.Log("[HorrorEnvironmentDirector] Overhauling Level 1: " + ScenePath);
            var scene1 = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Apply gameplay managers, breakers, item pickups first
            Stage8GDDLevelBuilder.ApplyGDDDesign();

            // Setup Cinematic Horror Atmosphere
            SetupCinematicAtmosphere();

            // Overhaul level architecture into Gothic Asylum (Image 1 & Image 2)
            ApplyAsylumVisualTheme(greenTilesMat, wardWallMat, corridorFloorMat, corridorWallMat, ceilingMat, doorMat);
            BuildDetailedMedicalBay(greenTilesMat, wardWallMat, metalMat, glassMat, batteryMat);
            BuildCorridorArchitecturalDetails(corridorFloorMat, corridorWallMat, doorMat, paperMat, metalMat);
            BuildResearchLabDetails(metalMat, screenMat, glassMat, batteryMat, autopsyMat, boneMat, bloodMat);
            BuildContainmentAndReactorDetails(metalMat, hazardMat, lureMat);
            BuildTheSubjectSilhouette(skinMat, gownMat, eyeMat, boneMat);

            // Reposition Player at starting vantage point in Medical Ward
            RepositionPlayerInMedicalWard();

            // Setup Level 1 Exit Airlock -> Transitions to Level 2!
            ConfigureExitTriggerForLevel1();

            // Setup Escape Room Randomizer on Level 1
            SetupLevel1Randomizer();

            // Bake Level 1 NavMesh
            Debug.Log("[HorrorEnvironmentDirector] Baking Level 1 NavMesh...");
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            EditorSceneManager.SaveScene(scene1);

            // ==========================================
            // BUILD LEVEL 2 (Sub-Basement Cryo Labs & Morgue)
            // ==========================================
            Debug.Log("[HorrorEnvironmentDirector] Constructing Level 2: " + ScenePath2);
            BuildLevel2SubBasement(floorTilesMat, wallMat, metalMat, metalGratingMat, hazardMat, screenMat, glassMat, skinMat, gownMat, eyeMat, boneMat, autopsyMat, bloodMat, batteryMat, lureMat);

            // Upgrade Main Menu Scene
            UpgradeMainMenuScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #region Atmosphere & Lighting

        private static void SetupCinematicAtmosphere()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.007f, 0.009f, 0.014f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.005f, 0.006f, 0.010f);
            RenderSettings.fogDensity = 0.038f;
        }

        #endregion

        #region Gothic Asylum Visual Overhaul (Image 1 & Image 2)

        private static void ApplyAsylumVisualTheme(Material greenTilesMat, Material wardWallMat, Material corridorFloorMat, Material corridorWallMat, Material ceilingMat, Material doorMat)
        {
            // 1. Re-skin Medical Ward (Room_SecurityOffice) -> Image 1 Match
            GameObject office = GameObject.Find("Room_SecurityOffice");
            if (office != null)
            {
                office.name = "Room_MedicalWard";
                foreach (Renderer r in office.GetComponentsInChildren<Renderer>())
                {
                    string n = r.gameObject.name.ToLower();
                    if (n.Contains("floor"))
                    {
                        r.sharedMaterial = greenTilesMat;
                    }
                    else if (n.Contains("ceiling"))
                    {
                        r.sharedMaterial = ceilingMat;
                    }
                    else if (n.Contains("wall") || n.Contains("lintel"))
                    {
                        r.sharedMaterial = wardWallMat;
                    }
                }
            }

            // 2. Re-skin Corridors -> Image 2 Match
            GameObject corridors = GameObject.Find("Corridors");
            if (corridors != null)
            {
                foreach (Renderer r in corridors.GetComponentsInChildren<Renderer>())
                {
                    string n = r.gameObject.name.ToLower();
                    if (n.Contains("floor"))
                    {
                        r.sharedMaterial = corridorFloorMat;
                    }
                    else if (n.Contains("ceiling"))
                    {
                        r.sharedMaterial = ceilingMat;
                    }
                    else if (n.Contains("wall") || n.Contains("lintel") || n.Contains("deadend"))
                    {
                        r.sharedMaterial = corridorWallMat;
                    }
                }
            }
        }

        private static void RepositionPlayerInMedicalWard()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player != null)
            {
                // Vantage point positioned at the back-left corner of Medical Ward looking East toward exit doors
                player.transform.position = new Vector3(-15.2f, 0.1f, -5.0f);
                player.transform.rotation = Quaternion.Euler(0, 85f, 0);

                Camera cam = player.GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    cam.transform.localRotation = Quaternion.Euler(4f, 0f, 0f);
                }
            }
        }

        private static void BuildDetailedMedicalBay(Material greenTilesMat, Material wardWallMat, Material metalMat, Material glassMat, Material battMat)
        {
            GameObject existing = GameObject.Find("MedicalBay_Props");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            GameObject root = new GameObject("MedicalBay_Props");
            root.transform.position = Vector3.zero;

            Material ironMat = GetOrCreateMaterial("M_Apparition_Iron", new Color(0.10f, 0.11f, 0.12f), 0.55f);
            Material mattressMat = GetOrCreateMaterial("M_Apparition_Mattress", new Color(0.18f, 0.19f, 0.17f), 0.15f);
            Material woodMat = GetOrCreateMaterial("M_Asylum_Wood_Dark", new Color(0.14f, 0.11f, 0.08f), 0.35f);
            Material doorMat = GetMat("M_Asylum_Door");

            // ========================================================
            // 1. LEFT WALL (NORTH WALL, Z = -0.6f): Barred Windows, Radiators, Clock (Image 1 Match)
            // ========================================================
            float northWallZ = -0.55f;
            float[] windowX = new float[] { -14.2f, -11.5f, -8.8f };
            for (int i = 0; i < windowX.Length; i++)
            {
                // Barred multi-paned psychiatric window facing into room
                CreateBarredWindow(root.transform, new Vector3(windowX[i], 2.1f, northWallZ), Quaternion.Euler(0, 180f, 0), ironMat, glassMat);

                // Cast-iron vertical-ribbed radiator under each window
                CreateCastIronRadiator(root.transform, new Vector3(windowX[i], 0.1f, northWallZ - 0.15f), Quaternion.Euler(0, 180f, 0), ironMat);
            }

            // Analog vintage asylum clock between windows
            CreateAnalogWallClock(root.transform, new Vector3(-12.8f, 2.7f, northWallZ - 0.05f), Quaternion.Euler(0, 180f, 0));

            // Wall-mounted bulkhead sconce lamp
            CreateWallBulkheadSconce(root.transform, new Vector3(-10.2f, 2.3f, northWallZ - 0.08f), Quaternion.Euler(0, 180f, 0), new Color(1.0f, 0.85f, 0.6f));

            // ========================================================
            // 2. CEILING: Wooden Crossbeams & Hanging Lights (Image 1 Match)
            // ========================================================
            float[] beamZ = new float[] { -2.8f, -5.0f, -7.2f };
            for (int b = 0; b < beamZ.Length; b++)
            {
                CreateBox(root.transform, $"Ceiling_Beam_{b}", new Vector3(-12.0f, 3.95f, beamZ[b]), new Vector3(9.2f, 0.18f, 0.22f), woodMat);
            }

            // Normal hanging fluorescent fixtures
            CreateHangingFluorescentFixture(root.transform, new Vector3(-13.2f, 3.4f, -6.5f), Quaternion.Euler(0, 90f, 0), false, new Color(0.65f, 0.90f, 0.85f));
            CreateHangingFluorescentFixture(root.transform, new Vector3(-9.2f, 3.4f, -3.5f), Quaternion.Euler(0, 90f, 0), false, new Color(0.65f, 0.90f, 0.85f));

            // BROKEN DANGLING FLUORESCENT FIXTURE (Image 1 Signature Element)
            CreateHangingFluorescentFixture(root.transform, new Vector3(-11.5f, 3.1f, -5.0f), Quaternion.Euler(0, 90f, 0), true, new Color(0.55f, 0.95f, 0.82f));

            // ========================================================
            // 3. EXIT THRESHOLD: Doorway & Glowing Dome Lamp (Image 1 Match)
            // ========================================================
            // Heavy hospital double doors with glass observation panels on East Wall
            CreateDoubleHospitalDoor(root.transform, new Vector3(-7.45f, 0f, -5.0f), Quaternion.Euler(0, -90f, 0), doorMat, ironMat);

            // Glowing dome lamp directly above exit doorway
            CreateOverDoorDomeLamp(root.transform, new Vector3(-7.55f, 3.35f, -5.0f), new Color(1.0f, 0.92f, 0.70f));

            // ========================================================
            // 4. FURNITURE & PROPS COMPOSITION (Image 1 Exact Staging)
            // ========================================================
            // Foreground Bed (Lower-Left): Rusted Bed with Open Springs
            CreateVintageHospitalBed(root.transform, new Vector3(-14.6f, 0f, -3.2f), Quaternion.Euler(0, 80f, 0), ironMat, mattressMat, false);

            // Overturned Wire Wastebasket with Spilled Gauze in foreground
            CreateOverturnedWastebasket(root.transform, new Vector3(-14.2f, 0.15f, -4.2f), ironMat);

            // Center-Left Bed: The Seated Apparition holding candle facing player/hall
            BuildSeatedApparition(root.transform, new Vector3(-12.0f, 0f, -2.2f), Quaternion.Euler(0, 160f, 0));

            // Center-Right Low Wooden Specimen Table with Glowing Jars
            CreateLowSpecimenTable(root.transform, new Vector3(-11.2f, 0f, -6.0f), Quaternion.Euler(0, 5f, 0), woodMat, glassMat);

            // Midground Hospital Bed (South side)
            CreateVintageHospitalBed(root.transform, new Vector3(-10.0f, 0f, -7.5f), Quaternion.Euler(0, -20f, 0), ironMat, mattressMat, true);

            // Restraint Cot / Iron Crib along South Wall
            CreateRestraintCribCot(root.transform, new Vector3(-13.5f, 0f, -8.6f), Quaternion.Euler(0, 90f, 0), ironMat);

            // Leaning Mattress & Boards against South-East Wall
            CreateLeaningMattressAndBoards(root.transform, new Vector3(-7.8f, 0f, -8.2f), mattressMat, woodMat);

            // Tall Rusted IV Drip Stand next to Apparition's bed
            CreateIVDripStand(root.transform, new Vector3(-12.8f, 0f, -3.2f), ironMat);
        }

        private static void BuildCorridorArchitecturalDetails(Material corridorFloorMat, Material corridorWallMat, Material doorMat, Material paperMat, Material metalMat)
        {
            GameObject existing = GameObject.Find("Corridor_Architectural_Details");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            GameObject root = new GameObject("Corridor_Architectural_Details");
            root.transform.position = Vector3.zero;

            Material ironMat = GetOrCreateMaterial("M_Apparition_Iron", new Color(0.10f, 0.11f, 0.12f), 0.55f);
            Material trimMat = GetOrCreateMaterial("M_Asylum_Wood_Dark", new Color(0.14f, 0.11f, 0.08f), 0.35f);
            Material mattressMat = GetOrCreateMaterial("M_Apparition_Mattress", new Color(0.18f, 0.19f, 0.17f), 0.15f);

            // ========================================================
            // 1. SCATTERED PATIENT CASE FILES & DOCUMENTS (Image 2 Match)
            // ========================================================
            // West Corridor littered papers
            CreateScatteredPatientPapers(root.transform, new Vector3(-5.0f, 0.015f, -8.5f), new Vector3(-5.0f, 0.015f, 21.0f), 38, paperMat);
            // Central Corridor littered papers
            CreateScatteredPatientPapers(root.transform, new Vector3(-2.0f, 0.015f, 8.0f), new Vector3(12.0f, 0.015f, 8.0f), 18, paperMat);

            // Rolled bedding / carpet bundle in the corner (Image 2 Match)
            CreateRolledBeddingBundle(root.transform, new Vector3(-6.2f, 0.22f, -9.2f), Quaternion.Euler(0, 30f, 90f), mattressMat);

            // ========================================================
            // 2. CORRIDOR DOORS WITH TRANSOM WINDOWS (Image 2 Match)
            // ========================================================
            CreateCorridorTransomDoor(root.transform, new Vector3(-6.75f, 0f, -1.0f), Quaternion.Euler(0, 90f, 0), doorMat, trimMat, "WARD 101");
            CreateCorridorTransomDoor(root.transform, new Vector3(-6.75f, 0f, 4.0f), Quaternion.Euler(0, 90f, 0), doorMat, trimMat, "ISOLATION A");
            CreateCorridorTransomDoor(root.transform, new Vector3(-6.75f, 0f, 13.0f), Quaternion.Euler(0, 90f, 0), doorMat, trimMat, "RECORDS");
            CreateCorridorTransomDoor(root.transform, new Vector3(-3.25f, 0f, 2.0f), Quaternion.Euler(0, -90f, 0), doorMat, trimMat, "SURGICAL WARD");
            CreateCorridorTransomDoor(root.transform, new Vector3(-3.25f, 0f, 14.0f), Quaternion.Euler(0, -90f, 0), doorMat, trimMat, "ELECTROTHERAPY");
            CreateCorridorTransomDoor(root.transform, new Vector3(-3.25f, 0f, 18.0f), Quaternion.Euler(0, -90f, 0), doorMat, trimMat, "AUTOPSY");

            // ========================================================
            // 3. FRAMED CERTIFICATES & WALL NOTICES (Image 2 Match)
            // ========================================================
            CreateWallFramedCertificate(root.transform, new Vector3(-6.72f, 2.1f, 1.5f), Quaternion.Euler(0, 90f, 0), trimMat, paperMat);
            CreateWallFramedCertificate(root.transform, new Vector3(-6.72f, 2.1f, 9.5f), Quaternion.Euler(0, 90f, 0), trimMat, paperMat);
            CreateWallFramedCertificate(root.transform, new Vector3(-3.28f, 2.1f, 6.0f), Quaternion.Euler(0, -90f, 0), trimMat, paperMat);
            CreateWallFramedCertificate(root.transform, new Vector3(-3.28f, 2.1f, 16.0f), Quaternion.Euler(0, -90f, 0), trimMat, paperMat);

            // ========================================================
            // 4. CAST-IRON WALL RADIATORS (Image 2 Match)
            // ========================================================
            CreateCastIronRadiator(root.transform, new Vector3(-6.60f, 0.1f, -4.5f), Quaternion.Euler(0, 90f, 0), ironMat);
            CreateCastIronRadiator(root.transform, new Vector3(-6.60f, 0.1f, 10.5f), Quaternion.Euler(0, 90f, 0), ironMat);

            // ========================================================
            // 5. RHYTHMIC AMBER DOME CEILING LAMPS (Image 2 Match)
            // ========================================================
            float[] lampZ = new float[] { -7.0f, -1.5f, 4.0f, 10.0f, 16.0f, 21.0f };
            for (int l = 0; l < lampZ.Length; l++)
            {
                CreateCorridorGlobeLamp(root.transform, new Vector3(-5.0f, 3.65f, lampZ[l]), new Color(1.0f, 0.76f, 0.44f), 0.95f, 6.5f);
            }
            CreateCorridorGlobeLamp(root.transform, new Vector3(4.0f, 3.65f, 8.0f), new Color(1.0f, 0.76f, 0.44f), 0.95f, 6.5f);
            CreateCorridorGlobeLamp(root.transform, new Vector3(9.5f, 3.65f, 8.0f), new Color(1.0f, 0.76f, 0.44f), 0.95f, 6.5f);

            // ========================================================
            // 6. HAZARDS: Broken Glass & Electrical Sparks
            // ========================================================
            CreateBrokenGlassHazard(root.transform, new Vector3(-5.0f, 0.02f, 2.5f), new Vector3(2.4f, 0.05f, 2.0f));
            CreateBrokenGlassHazard(root.transform, new Vector3(-5.0f, 0.02f, 15.0f), new Vector3(2.4f, 0.05f, 2.0f));
            CreateElectricalSparkHazard(root.transform, new Vector3(-4.5f, 3.3f, 9.0f));
        }

        #endregion

        private static void BuildResearchLabDetails(Material metalMat, Material screenMat, Material glassMat, Material battMat, Material autopsyMat, Material boneMat, Material bloodMat)
        {
            GameObject existing = GameObject.Find("ResearchLab_Props");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            GameObject root = new GameObject("ResearchLab_Props");
            root.transform.position = Vector3.zero;

            // Autopsy Dissection Table with Stainless Steel Slab & Body Shroud
            CreateAutopsyTable(root.transform, new Vector3(-13.5f, 0f, 11f), Quaternion.Euler(0, 45f, 0), autopsyMat, metalMat, bloodMat);

            // Specimen Display Shelves with Illuminated Jars
            CreateSpecimenShelving(root.transform, new Vector3(-17.5f, 0f, 15f), Quaternion.Euler(0, 90f, 0), metalMat, glassMat);

            // Mobile Hospital Gurney
            CreateHospitalGurney(root.transform, new Vector3(-9.5f, 0f, 15f), Quaternion.Euler(0, -20f, 0), metalMat, autopsyMat);

            // Central Chemical Island Bench
            CreateChemicalBench(root.transform, new Vector3(-14f, 0f, 6.5f), metalMat);
        }

        private static void BuildContainmentAndReactorDetails(Material metalMat, Material hazardMat, Material lureMat)
        {
            GameObject existing = GameObject.Find("Containment_Props");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            GameObject root = new GameObject("Containment_Props");
            root.transform.position = Vector3.zero;

            // Gothic Asylum Restraint Chair in Containment
            CreateAsylumRestraintChair(root.transform, new Vector3(22.5f, 0f, 10.5f), Quaternion.Euler(0, -45f, 0), metalMat);

            // Heavy Containment Blast Door Trim
            CreateBlastDoorSurround(root.transform, new Vector3(18f, 0f, 12f), metalMat, hazardMat);
        }

        private static void ConfigureExitTriggerForLevel1()
        {
            ExitTrigger exitTrig = Object.FindAnyObjectByType<ExitTrigger>();
            if (exitTrig != null)
            {
                exitTrig.Configure("LaboratoryLevel2", false);
                Debug.Log("[HorrorEnvironmentDirector] Configured Level 1 ExitTrigger to load LaboratoryLevel2.");
            }
        }

        private static void SetupLevel1Randomizer()
        {
            GameObject randObj = GameObject.Find("EscapeRoomRandomizer");
            if (randObj == null) randObj = new GameObject("EscapeRoomRandomizer");

            EscapeRoomRandomizer randomizer = randObj.GetComponent<EscapeRoomRandomizer>();
            if (randomizer == null) randomizer = randObj.AddComponent<EscapeRoomRandomizer>();

            // Register key item anchors
            GameObject anchorRoot = GameObject.Find("SpawnAnchors");
            if (anchorRoot == null)
            {
                anchorRoot = new GameObject("SpawnAnchors");
                CreateAnchor(anchorRoot.transform, "Anchor_MedCart", new Vector3(-0.6f, 0.85f, 3.2f), "battery");
                CreateAnchor(anchorRoot.transform, "Anchor_AutopsyTray", new Vector3(-13.5f, 0.95f, 11f), "keycard");
                CreateAnchor(anchorRoot.transform, "Anchor_ChemBench", new Vector3(-14f, 0.95f, 6.5f), "keycard");
                CreateAnchor(anchorRoot.transform, "Anchor_SpecimenShelf", new Vector3(-17.5f, 0.85f, 15f), "lure");
                CreateAnchor(anchorRoot.transform, "Anchor_ServerDesk", new Vector3(13f, 0.95f, -6f), "keycard");
                CreateAnchor(anchorRoot.transform, "Anchor_ReactorShelf", new Vector3(20f, 0.95f, -4f), "tool");
            }
        }

        private static void CreateAnchor(Transform parent, string name, Vector3 pos, string itemType)
        {
            GameObject a = new GameObject(name);
            a.transform.parent = parent;
            a.transform.position = pos;
            EscapeRoomRandomizer rand = Object.FindAnyObjectByType<EscapeRoomRandomizer>();
            if (rand != null) rand.RegisterAnchor(itemType, a.transform);
        }

        #region Level 2: Sub-Basement Cryo Labs & Morgue

        private static void BuildLevel2SubBasement(Material floorTilesMat, Material wallMat, Material metalMat, Material metalGratingMat, Material hazardMat, Material screenMat, Material glassMat, Material skinMat, Material gownMat, Material eyeMat, Material boneMat, Material autopsyMat, Material bloodMat, Material battMat, Material lureMat)
        {
            var level2Scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Subterranean dark atmosphere
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.005f, 0.006f, 0.009f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.004f, 0.005f, 0.008f);
            RenderSettings.fogDensity = 0.046f;

            GameObject root = new GameObject("Level2_Environment");

            // 1. Arrival Decontamination Chamber (Player Start at 0, 0, 0)
            BuildRoom(root.transform, "Room_ArrivalVestibule", new Vector3(0, 0, 0), new Vector3(8, 4, 8), floorTilesMat, wallMat, metalMat);
            Stage3LabLevelBuilder.BuildPlayer(new Vector3(0, 0, 0));

            // 2. Sub-Basement Central Corridor (0, 0, 4) to (0, 0, 32)
            BuildRoom(root.transform, "Corridor_SubBasement", new Vector3(0, 0, 18), new Vector3(4, 4, 28), metalGratingMat, wallMat, metalMat);

            // Corridor Bulkhead Arches
            for (float z = 8f; z <= 28f; z += 6f)
            {
                CreateBulkheadArch(root.transform, new Vector3(0, 0, z), metalMat, hazardMat);
            }
            CreateOverheadPipes(root.transform, metalMat);

            // Corridor Hazards
            CreateBrokenGlassHazard(root.transform, new Vector3(0f, 0.02f, 10f), new Vector3(2.5f, 0.05f, 2.0f));
            CreateBrokenGlassHazard(root.transform, new Vector3(0f, 0.02f, 22f), new Vector3(2.5f, 0.05f, 2.0f));
            CreateElectricalSparkHazard(root.transform, new Vector3(0.5f, 3.2f, 16f));

            // 3. Deep Cryo-Chamber (-16, 0, 18)
            BuildRoom(root.transform, "Room_CryoChamber", new Vector3(-16, 0, 18), new Vector3(16, 5, 14), floorTilesMat, wallMat, metalMat);
            BuildCryoVats(root.transform, new Vector3(-16, 0, 18), glassMat, metalMat, skinMat);

            // Cryo Computer Console with Master Override Keycard
            CreateHeartRateMonitorStation(root.transform, new Vector3(-20f, 0f, 18f), Quaternion.Euler(0, 90f, 0), metalMat, screenMat);
            SpawnPickupItem("Item_Keycard_SubBasement", new Vector3(-19.5f, 0.95f, 18f), ItemType.KeycardLevel2, "Sub-Basement Override Key", new Color(1f, 0.85f, 0.2f));

            // 4. Gothic Sub-Basement Morgue (16, 0, 18)
            BuildRoom(root.transform, "Room_Morgue", new Vector3(16, 0, 18), new Vector3(14, 4, 14), floorTilesMat, wallMat, metalMat);
            CreateAutopsyTable(root.transform, new Vector3(14f, 0f, 16f), Quaternion.Euler(0, 0, 0), autopsyMat, metalMat, bloodMat);
            CreateAutopsyTable(root.transform, new Vector3(18f, 0f, 20f), Quaternion.Euler(0, 90f, 0), autopsyMat, metalMat, bloodMat);
            CreateSpecimenShelving(root.transform, new Vector3(21.5f, 0f, 18f), Quaternion.Euler(0, -90f, 0), metalMat, glassMat);
            BuildSeatedApparition(root.transform, new Vector3(18f, 0f, 22f), Quaternion.Euler(0, -135f, 0));
            SpawnPickupItem("Item_Battery_SubBasement", new Vector3(14f, 0.95f, 16f), ItemType.BatteryPack, "Auxiliary Battery Cell", Color.yellow);

            // 5. Auxiliary Reactor Chamber (-14, 0, 36)
            BuildRoom(root.transform, "Room_AuxiliaryReactor", new Vector3(-14, 0, 36), new Vector3(12, 5, 12), metalGratingMat, wallMat, metalMat);
            CreateMachineGenerator(root.transform, new Vector3(-14, 0, 36), metalMat, hazardMat);
            SpawnPickupItem("Item_SonicLure_SubBasement", new Vector3(-12f, 0.95f, 36f), ItemType.SonicLure, "Acoustic Disrupter Lure", Color.cyan);

            // 6. Surface Freight Elevator Exit Airlock (0, 0, 40)
            BuildRoom(root.transform, "Room_SurfaceElevator", new Vector3(0, 0, 38), new Vector3(8, 5, 10), metalMat, wallMat, hazardMat);
            BuildFreightElevatorExit(root.transform, new Vector3(0, 0, 41), metalMat, hazardMat);

            // 7. Spawn The Subject in Level 2 Cryo-Chamber
            BuildTheSubjectInLevel2(root.transform, new Vector3(-16f, 0.05f, 18f), skinMat, gownMat, eyeMat, boneMat);

            // 8. Setup Level 2 Core Managers
            SetupLevel2Managers();

            // 9. Bake Level 2 NavMesh
            Debug.Log("[HorrorEnvironmentDirector] Baking Level 2 NavMesh...");
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            Debug.Log("[HorrorEnvironmentDirector] Level 2 NavMesh baked.");

            EditorSceneManager.SaveScene(level2Scene, ScenePath2);
        }

        private static void BuildRoom(Transform parent, string name, Vector3 center, Vector3 size, Material floorMat, Material wallMat, Material trimMat)
        {
            GameObject room = new GameObject(name);
            room.transform.parent = parent;

            float w = size.x;
            float h = size.y;
            float l = size.z;

            // Floor
            CreateBox(room.transform, "Floor", center + new Vector3(0, -0.25f, 0), new Vector3(w, 0.5f, l), floorMat);
            // Ceiling
            CreateBox(room.transform, "Ceiling", center + new Vector3(0, h + 0.25f, 0), new Vector3(w, 0.5f, l), wallMat);

            // Walls (North, South, East, West)
            CreateBox(room.transform, "Wall_North", center + new Vector3(0, h * 0.5f, l * 0.5f + 0.25f), new Vector3(w, h, 0.5f), wallMat);
            CreateBox(room.transform, "Wall_South", center + new Vector3(0, h * 0.5f, -l * 0.5f - 0.25f), new Vector3(w, h, 0.5f), wallMat);
            CreateBox(room.transform, "Wall_East", center + new Vector3(w * 0.5f + 0.25f, h * 0.5f, 0), new Vector3(0.5f, h, l), wallMat);
            CreateBox(room.transform, "Wall_West", center + new Vector3(-w * 0.5f - 0.25f, h * 0.5f, 0), new Vector3(0.5f, h, l), wallMat);

            // Flickering Fluorescent Overhead Light
            GameObject lightObj = new GameObject("RoomLight");
            lightObj.transform.parent = room.transform;
            lightObj.transform.position = center + new Vector3(0, h - 0.3f, 0);
            Light lt = lightObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.range = Mathf.Max(w, l) * 0.85f;
            lt.intensity = 1.1f;
            lt.color = new Color(0.85f, 0.90f, 0.95f);
            lightObj.AddComponent<LightFlicker>();
        }

        private static void BuildCryoVats(Transform parent, Vector3 center, Material glassMat, Material metalMat, Material bioMat)
        {
            Vector3[] vatOffsets = new Vector3[]
            {
                new Vector3(-4f, 0, -3f),
                new Vector3(4f, 0, -3f),
                new Vector3(-4f, 0, 3f),
                new Vector3(4f, 0, 3f)
            };

            for (int i = 0; i < vatOffsets.Length; i++)
            {
                GameObject vat = new GameObject($"Cryo_Vat_{i + 1}");
                vat.transform.parent = parent;
                vat.transform.position = center + vatOffsets[i];

                // Metal Base
                GameObject baseCol = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                baseCol.name = "Base";
                baseCol.transform.parent = vat.transform;
                baseCol.transform.localPosition = new Vector3(0, 0.25f, 0);
                baseCol.transform.localScale = new Vector3(2.2f, 0.25f, 2.2f);
                if (metalMat != null) baseCol.GetComponent<Renderer>().sharedMaterial = metalMat;

                // Glass Cylinder
                GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                glass.name = "GlassCylinder";
                glass.transform.parent = vat.transform;
                glass.transform.localPosition = new Vector3(0, 2.0f, 0);
                glass.transform.localScale = new Vector3(1.8f, 1.5f, 1.8f);
                if (glassMat != null) glass.GetComponent<Renderer>().sharedMaterial = glassMat;

                // Glowing Bioluminescent Liquid Core
                GameObject liquid = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                liquid.name = "LiquidCore";
                liquid.transform.parent = vat.transform;
                liquid.transform.localPosition = new Vector3(0, 1.9f, 0);
                liquid.transform.localScale = new Vector3(1.4f, 1.3f, 1.4f);
                if (bioMat != null) liquid.GetComponent<Renderer>().sharedMaterial = bioMat;
                UnityEngine.Object.DestroyImmediate(liquid.GetComponent<Collider>());

                // Cyan/Emerald Cryo Glow Light
                GameObject glowObj = new GameObject("CryoGlowLight");
                glowObj.transform.parent = vat.transform;
                glowObj.transform.localPosition = new Vector3(0, 2f, 0);
                Light lt = glowObj.AddComponent<Light>();
                lt.type = LightType.Point;
                lt.color = new Color(0.1f, 0.95f, 0.65f);
                lt.range = 7f;
                lt.intensity = 1.6f;
            }
        }

        private static void CreateMachineGenerator(Transform parent, Vector3 pos, Material metalMat, Material hazardMat)
        {
            GameObject gen = new GameObject("Reactor_Generator");
            gen.transform.parent = parent;
            gen.transform.position = pos;

            // Heavy Machinery Block
            GameObject chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chassis.name = "Chassis";
            chassis.transform.parent = gen.transform;
            chassis.transform.localPosition = new Vector3(0, 1.2f, 0);
            chassis.transform.localScale = new Vector3(3.2f, 2.4f, 2.8f);
            if (metalMat != null) chassis.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Rotating Warning Beacon
            GameObject beaconObj = new GameObject("GeneratorBeacon");
            beaconObj.transform.parent = gen.transform;
            beaconObj.transform.localPosition = new Vector3(0, 2.6f, 0);
            Light lt = beaconObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1f, 0.65f, 0.1f);
            lt.range = 8f;
            lt.intensity = 1.8f;
        }

        private static void BuildFreightElevatorExit(Transform parent, Vector3 pos, Material metalMat, Material hazardMat)
        {
            GameObject elev = new GameObject("Freight_Elevator_Exit");
            elev.transform.parent = parent;
            elev.transform.position = pos;

            // Heavy Steel Gate Pillars
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = side < 0 ? "Pillar_Left" : "Pillar_Right";
                pillar.transform.parent = elev.transform;
                pillar.transform.localPosition = new Vector3(side * 2.2f, 2.2f, 0);
                pillar.transform.localScale = new Vector3(0.5f, 4.4f, 0.5f);
                if (metalMat != null) pillar.GetComponent<Renderer>().sharedMaterial = metalMat;
            }

            // Top Bulkhead Header
            GameObject header = GameObject.CreatePrimitive(PrimitiveType.Cube);
            header.name = "Header";
            header.transform.parent = elev.transform;
            header.transform.localPosition = new Vector3(0, 4.2f, 0);
            header.transform.localScale = new Vector3(4.9f, 0.6f, 0.8f);
            if (hazardMat != null) header.GetComponent<Renderer>().sharedMaterial = hazardMat;

            // Exit Trigger behind elevator gates -> FINAL VICTORY!
            GameObject trigObj = new GameObject("Elevator_Exit_Trigger");
            trigObj.transform.parent = elev.transform;
            trigObj.transform.localPosition = new Vector3(0, 1.5f, 1.5f);
            BoxCollider col = trigObj.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3.5f, 3f, 3.5f);

            ExitTrigger trig = trigObj.AddComponent<ExitTrigger>();
            trig.Configure("", true); // Final level!
        }

        private static void SpawnPickupItem(string name, Vector3 pos, ItemType type, string displayName, Color glowColor)
        {
            GameObject itemObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            itemObj.name = name;
            itemObj.transform.position = pos;
            itemObj.transform.localScale = new Vector3(0.35f, 0.15f, 0.25f);

            ItemPickup pickup = itemObj.AddComponent<ItemPickup>();
            pickup.Configure(type, displayName, glowColor);
        }

        private static void BuildTheSubjectInLevel2(Transform parent, Vector3 spawnPos, Material skinMat, Material gownMat, Material eyeMat, Material boneMat)
        {
            BuildTheSubjectSilhouette(skinMat, gownMat, eyeMat, boneMat);
            GameObject monster = GameObject.Find("Monster_Specimen");
            if (monster != null)
            {
                monster.transform.position = spawnPos;

                // Connect Level 2 waypoints
                MonsterController ctrl = monster.GetComponent<MonsterController>();
                if (ctrl != null)
                {
                    ctrl.SetSpeeds(1.15f, 1.75f, 3.25f, 1.5f);
                    SetupLevel2Waypoints(ctrl);
                }
            }
        }

        private static void SetupLevel2Waypoints(MonsterController ctrl)
        {
            GameObject wpRoot = new GameObject("Level2_MonsterWaypoints");
            Vector3[] pts = new Vector3[]
            {
                new Vector3(-16f, 0.05f, 18f), // Cryo Chamber
                new Vector3(-8f, 0.05f, 18f),
                new Vector3(0f, 0.05f, 18f),   // Main Corridor
                new Vector3(8f, 0.05f, 18f),
                new Vector3(16f, 0.05f, 18f),  // Morgue
                new Vector3(0f, 0.05f, 28f),   // Deep Corridor
                new Vector3(-14f, 0.05f, 36f), // Auxiliary Reactor
                new Vector3(0f, 0.05f, 38f)    // Elevator Vestibule
            };

            for (int i = 0; i < pts.Length; i++)
            {
                GameObject wp = new GameObject($"WP_{i + 1}");
                wp.transform.parent = wpRoot.transform;
                wp.transform.position = pts[i];
                ctrl.AddWaypoint(wp.transform);
            }
        }

        private static void SetupLevel2Managers()
        {
            if (Object.FindAnyObjectByType<NoiseManager>() == null)
            {
                GameObject nm = new GameObject("NoiseManager");
                nm.AddComponent<NoiseManager>();
            }

            if (Object.FindAnyObjectByType<LockdownCycleManager>() == null)
            {
                GameObject lm = new GameObject("LockdownCycleManager");
                lm.AddComponent<LockdownCycleManager>();
            }

            if (Object.FindAnyObjectByType<PowerGridManager>() == null)
            {
                GameObject pm = new GameObject("PowerGridManager");
                pm.AddComponent<PowerGridManager>();
            }

            if (Object.FindAnyObjectByType<InventoryManager>() == null)
            {
                GameObject im = new GameObject("InventoryManager");
                im.AddComponent<InventoryManager>();
            }

            if (Object.FindAnyObjectByType<HUDController>() == null)
            {
                GameObject hud = new GameObject("HUDController");
                hud.AddComponent<HUDController>();
            }
        }

        #endregion

        #region Gothic Hospital & Hazard Builders

        private static void CreateAutopsyTable(Transform parent, Vector3 pos, Quaternion rot, Material steelMat, Material sheetMat, Material bloodMat)
        {
            GameObject table = new GameObject("Gothic_Autopsy_Table");
            table.transform.parent = parent;
            table.transform.position = pos;
            table.transform.rotation = rot;

            // Pedestal Central Pillar
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.parent = table.transform;
            pedestal.transform.localPosition = new Vector3(0, 0.42f, 0);
            pedestal.transform.localScale = new Vector3(0.55f, 0.42f, 0.55f);
            if (steelMat != null) pedestal.GetComponent<Renderer>().sharedMaterial = steelMat;

            // Stainless Steel Slab with Beveled Rim
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "SteelSlab";
            slab.transform.parent = table.transform;
            slab.transform.localPosition = new Vector3(0, 0.86f, 0);
            slab.transform.localScale = new Vector3(1.1f, 0.08f, 2.3f);
            if (steelMat != null) slab.GetComponent<Renderer>().sharedMaterial = steelMat;

            // Fluid Basin below drainage hole
            GameObject basin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basin.name = "FluidBasin";
            basin.transform.parent = table.transform;
            basin.transform.localPosition = new Vector3(0, 0.72f, -0.95f);
            basin.transform.localScale = new Vector3(0.24f, 0.12f, 0.24f);
            if (bloodMat != null) basin.GetComponent<Renderer>().sharedMaterial = bloodMat;

            // Covered Body Shroud / Cadaver Bag
            GameObject corpse = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            corpse.name = "Covered_Cadaver_Shroud";
            corpse.transform.parent = table.transform;
            corpse.transform.localPosition = new Vector3(0, 1.05f, 0.05f);
            corpse.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            corpse.transform.localScale = new Vector3(0.68f, 0.95f, 0.32f);
            if (sheetMat != null) corpse.GetComponent<Renderer>().sharedMaterial = sheetMat;

            // Overhead Articulating Surgical Lamp
            GameObject lampRoot = new GameObject("Overhead_Surgical_Lamp");
            lampRoot.transform.parent = table.transform;
            lampRoot.transform.localPosition = new Vector3(0, 2.8f, 0);

            GameObject lampHood = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lampHood.name = "LampHood";
            lampHood.transform.parent = lampRoot.transform;
            lampHood.transform.localPosition = Vector3.zero;
            lampHood.transform.localScale = new Vector3(0.75f, 0.06f, 0.75f);
            if (steelMat != null) lampHood.GetComponent<Renderer>().sharedMaterial = steelMat;

            Light lt = lampRoot.AddComponent<Light>();
            lt.type = LightType.Spot;
            lt.spotAngle = 55f;
            lt.range = 5f;
            lt.intensity = 2.4f;
            lt.color = new Color(0.85f, 0.95f, 0.90f);
        }

        private static void CreateSpecimenShelving(Transform parent, Vector3 pos, Quaternion rot, Material shelfMat, Material glassMat)
        {
            GameObject unit = new GameObject("Specimen_Shelving_Unit");
            unit.transform.parent = parent;
            unit.transform.position = pos;
            unit.transform.rotation = rot;

            // Shelves
            for (int i = 0; i < 4; i++)
            {
                GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.name = $"Shelf_{i}";
                shelf.transform.parent = unit.transform;
                shelf.transform.localPosition = new Vector3(0, 0.35f + (i * 0.65f), 0);
                shelf.transform.localScale = new Vector3(2.4f, 0.05f, 0.65f);
                if (shelfMat != null) shelf.GetComponent<Renderer>().sharedMaterial = shelfMat;

                // Glass Specimen Jars
                for (int j = -1; j <= 1; j++)
                {
                    GameObject jar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    jar.name = $"Specimen_Jar_{i}_{j}";
                    jar.transform.parent = unit.transform;
                    jar.transform.localPosition = new Vector3(j * 0.65f, 0.52f + (i * 0.65f), 0);
                    jar.transform.localScale = new Vector3(0.18f, 0.16f, 0.18f);
                    if (glassMat != null) jar.GetComponent<Renderer>().sharedMaterial = glassMat;
                }
            }
        }

        private static void CreateHospitalGurney(Transform parent, Vector3 pos, Quaternion rot, Material metalMat, Material sheetMat)
        {
            GameObject gurney = new GameObject("Hospital_Gurney");
            gurney.transform.parent = parent;
            gurney.transform.position = pos;
            gurney.transform.rotation = rot;

            // Chassis
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.parent = gurney.transform;
            frame.transform.localPosition = new Vector3(0, 0.65f, 0);
            frame.transform.localScale = new Vector3(0.9f, 0.08f, 2.0f);
            if (metalMat != null) frame.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Stained Mattress
            GameObject mat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mat.name = "Mattress";
            mat.transform.parent = gurney.transform;
            mat.transform.localPosition = new Vector3(0, 0.76f, 0);
            mat.transform.localScale = new Vector3(0.82f, 0.14f, 1.9f);
            if (sheetMat != null) mat.GetComponent<Renderer>().sharedMaterial = sheetMat;
        }

        private static void CreateAsylumRestraintChair(Transform parent, Vector3 pos, Quaternion rot, Material ironMat)
        {
            GameObject chair = new GameObject("Asylum_Restraint_Chair");
            chair.transform.parent = parent;
            chair.transform.position = pos;
            chair.transform.rotation = rot;

            // Seat
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Seat";
            seat.transform.parent = chair.transform;
            seat.transform.localPosition = new Vector3(0, 0.55f, 0);
            seat.transform.localScale = new Vector3(0.75f, 0.08f, 0.75f);
            if (ironMat != null) seat.GetComponent<Renderer>().sharedMaterial = ironMat;

            // Backrest
            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Backrest";
            back.transform.parent = chair.transform;
            back.transform.localPosition = new Vector3(0, 1.15f, -0.34f);
            back.transform.localScale = new Vector3(0.75f, 1.1f, 0.08f);
            if (ironMat != null) back.GetComponent<Renderer>().sharedMaterial = ironMat;

            // Heavy Iron Restraint Straps
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject armrest = GameObject.CreatePrimitive(PrimitiveType.Cube);
                armrest.name = side < 0 ? "Armrest_L" : "Armrest_R";
                armrest.transform.parent = chair.transform;
                armrest.transform.localPosition = new Vector3(side * 0.42f, 0.85f, 0.05f);
                armrest.transform.localScale = new Vector3(0.12f, 0.06f, 0.70f);
                if (ironMat != null) armrest.GetComponent<Renderer>().sharedMaterial = ironMat;
            }
        }

        private static void CreateBrokenGlassHazard(Transform parent, Vector3 pos, Vector3 size)
        {
            GameObject glassObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glassObj.name = "FloorHazard_BrokenGlass";
            glassObj.transform.parent = parent;
            glassObj.transform.position = pos;
            glassObj.transform.localScale = size;

            Material glassMat = GetMat("M_Lab_Glass");
            if (glassMat != null) glassObj.GetComponent<Renderer>().sharedMaterial = glassMat;

            // Attach FloorHazard component
            glassObj.AddComponent<FloorHazard>();
        }

        private static void CreateElectricalSparkHazard(Transform parent, Vector3 pos)
        {
            GameObject sparkObj = new GameObject("Hazard_ElectricalSpark");
            sparkObj.transform.parent = parent;
            sparkObj.transform.position = pos;

            // Dangling wire mesh
            GameObject wire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wire.name = "DanglingWire";
            wire.transform.parent = sparkObj.transform;
            wire.transform.localPosition = new Vector3(0, -0.4f, 0);
            wire.transform.localScale = new Vector3(0.04f, 0.4f, 0.04f);
            Material metalMat = GetMat("M_Lab_Metal");
            if (metalMat != null) wire.GetComponent<Renderer>().sharedMaterial = metalMat;
            UnityEngine.Object.DestroyImmediate(wire.GetComponent<Collider>());

            sparkObj.AddComponent<ElectricalSparkHazard>();
        }

        #endregion

        #region The Subject Upgraded Rig & Silhouette (Height & Speed Fixed, Visceral Details)

        private static void BuildTheSubjectSilhouette(Material skinMat, Material gownMat, Material eyeMat, Material boneMat)
        {
            GameObject existing = GameObject.Find("Monster_Specimen");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            // Create Master Root
            GameObject monster = new GameObject("Monster_Specimen");
            monster.tag = "Respawn";
            monster.transform.position = new Vector3(23f, 0.05f, 12f); // Containment lair

            // NavMeshAgent - Tuned Height (~1.92m total hunched) and Balanced Speeds
            NavMeshAgent agent = monster.AddComponent<NavMeshAgent>();
            agent.speed = 1.15f; // Creeping stalking pace
            agent.angularSpeed = 220f;
            agent.acceleration = 8.5f; // Smooth menace
            agent.stoppingDistance = 0.5f;
            agent.radius = 0.40f;
            agent.height = 1.85f; // Fits doors comfortably without clipping

            // Trigger Collider for kill detection
            CapsuleCollider col = monster.AddComponent<CapsuleCollider>();
            col.radius = 0.40f;
            col.height = 1.85f;
            col.center = new Vector3(0, 0.92f, 0);

            // 3D Positional AudioSource
            AudioSource aSrc = monster.AddComponent<AudioSource>();
            aSrc.spatialBlend = 1.0f;
            aSrc.minDistance = 2f;
            aSrc.maxDistance = 22f;
            aSrc.rolloffMode = AudioRolloffMode.Linear;

            // 1. Hunched Slender Torso (~1.92m hunched humanoid)
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso_Hunched";
            torso.transform.parent = monster.transform;
            torso.transform.localPosition = new Vector3(0, 0.95f, -0.06f);
            torso.transform.localRotation = Quaternion.Euler(18f, 0, 0);
            torso.transform.localScale = new Vector3(0.56f, 0.72f, 0.46f);
            if (skinMat != null) torso.GetComponent<Renderer>().sharedMaterial = skinMat;
            UnityEngine.Object.DestroyImmediate(torso.GetComponent<Collider>());

            // 2. Torn Hospital Shroud / Gown
            GameObject gown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gown.name = "Torn_Hospital_Gown";
            gown.transform.parent = torso.transform;
            gown.transform.localPosition = new Vector3(0, -0.25f, 0);
            gown.transform.localScale = new Vector3(1.12f, 0.55f, 1.10f);
            if (gownMat != null) gown.GetComponent<Renderer>().sharedMaterial = gownMat;
            UnityEngine.Object.DestroyImmediate(gown.GetComponent<Collider>());

            // 3. Exposed Vertebrae Spikes (5 sharp bone projections down hunched back)
            for (int i = 0; i < 5; i++)
            {
                GameObject spine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spine.name = $"Spine_Spike_{i}";
                spine.transform.parent = torso.transform;
                spine.transform.localPosition = new Vector3(0, 0.32f - (i * 0.15f), -0.42f);
                spine.transform.localRotation = Quaternion.Euler(-25f - (i * 8f), 0, 0);
                spine.transform.localScale = new Vector3(0.06f, 0.16f, 0.06f);
                if (boneMat != null) spine.GetComponent<Renderer>().sharedMaterial = boneMat;
                UnityEngine.Object.DestroyImmediate(spine.GetComponent<Collider>());
            }

            // 4. Flayed Skeletal Ribcage (8 curved bone ribs branching out around chest)
            for (int i = 0; i < 4; i++)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    GameObject rib = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    rib.name = $"Rib_{(side < 0 ? "L" : "R")}_{i}";
                    rib.transform.parent = torso.transform;
                    rib.transform.localPosition = new Vector3(side * 0.34f, 0.25f - (i * 0.13f), 0.12f + (i * 0.03f));
                    rib.transform.localRotation = Quaternion.Euler(15f, side * 55f, side * 35f);
                    rib.transform.localScale = new Vector3(0.04f, 0.22f, 0.04f);
                    if (boneMat != null) rib.GetComponent<Renderer>().sharedMaterial = boneMat;
                    UnityEngine.Object.DestroyImmediate(rib.GetComponent<Collider>());
                }
            }

            // 5. Extended Slender Neck & Distorted Skull
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head_Asymmetric";
            head.transform.parent = torso.transform;
            head.transform.localPosition = new Vector3(0, 0.72f, 0.22f);
            head.transform.localRotation = Quaternion.Euler(-8f, 0, 0);
            head.transform.localScale = new Vector3(0.44f, 0.52f, 0.48f);
            if (skinMat != null) head.GetComponent<Renderer>().sharedMaterial = skinMat;
            UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());

            // Mutated Cranial Horns
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject horn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                horn.name = $"Cranial_Horn_{(side < 0 ? "L" : "R")}";
                horn.transform.parent = head.transform;
                horn.transform.localPosition = new Vector3(side * 0.18f, 0.24f, -0.10f);
                horn.transform.localRotation = Quaternion.Euler(-45f, side * 25f, side * 15f);
                horn.transform.localScale = new Vector3(0.05f, 0.20f, 0.05f);
                if (boneMat != null) horn.GetComponent<Renderer>().sharedMaterial = boneMat;
                UnityEngine.Object.DestroyImmediate(horn.GetComponent<Collider>());
            }

            // Upper Snout & Razor Fangs
            GameObject snout = GameObject.CreatePrimitive(PrimitiveType.Cube);
            snout.name = "Snout_Upper";
            snout.transform.parent = head.transform;
            snout.transform.localPosition = new Vector3(0, -0.10f, 0.30f);
            snout.transform.localRotation = Quaternion.Euler(12f, 0, 0);
            snout.transform.localScale = new Vector3(0.30f, 0.15f, 0.32f);
            if (skinMat != null) snout.GetComponent<Renderer>().sharedMaterial = skinMat;
            UnityEngine.Object.DestroyImmediate(snout.GetComponent<Collider>());

            for (int f = -1; f <= 1; f += 2)
            {
                GameObject fang = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                fang.name = $"Upper_Fang_{(f < 0 ? "L" : "R")}";
                fang.transform.parent = snout.transform;
                fang.transform.localPosition = new Vector3(f * 0.38f, -0.55f, 0.30f);
                fang.transform.localScale = new Vector3(0.12f, 0.40f, 0.12f);
                if (boneMat != null) fang.GetComponent<Renderer>().sharedMaterial = boneMat;
                UnityEngine.Object.DestroyImmediate(fang.GetComponent<Collider>());
            }

            // Dropped Lower Mandible / Chattering Jaw
            GameObject jawLower = new GameObject("Jaw_Lower_Root");
            jawLower.transform.parent = head.transform;
            jawLower.transform.localPosition = new Vector3(0, -0.18f, 0.10f);

            GameObject jawMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            jawMesh.name = "Jaw_Lower_Mesh";
            jawMesh.transform.parent = jawLower.transform;
            jawMesh.transform.localPosition = new Vector3(0, -0.06f, 0.18f);
            jawMesh.transform.localRotation = Quaternion.Euler(18f, 0, 0);
            jawMesh.transform.localScale = new Vector3(0.26f, 0.11f, 0.28f);
            if (skinMat != null) jawMesh.GetComponent<Renderer>().sharedMaterial = skinMat;
            UnityEngine.Object.DestroyImmediate(jawMesh.GetComponent<Collider>());

            for (int f = -1; f <= 1; f += 2)
            {
                GameObject lFang = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lFang.name = $"Lower_Fang_{(f < 0 ? "L" : "R")}";
                lFang.transform.parent = jawMesh.transform;
                lFang.transform.localPosition = new Vector3(f * 0.35f, 0.55f, 0.28f);
                lFang.transform.localScale = new Vector3(0.10f, 0.35f, 0.10f);
                if (boneMat != null) lFang.GetComponent<Renderer>().sharedMaterial = boneMat;
                UnityEngine.Object.DestroyImmediate(lFang.GetComponent<Collider>());
            }

            // Oversized Ears (acute hearing)
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                ear.name = side < 0 ? "Ear_Left" : "Ear_Right";
                ear.transform.parent = head.transform;
                ear.transform.localPosition = new Vector3(side * 0.28f, 0.12f, -0.05f);
                ear.transform.localRotation = Quaternion.Euler(20f, side * 35f, side * 25f);
                ear.transform.localScale = new Vector3(0.07f, 0.20f, 0.12f);
                if (skinMat != null) ear.GetComponent<Renderer>().sharedMaterial = skinMat;
                UnityEngine.Object.DestroyImmediate(ear.GetComponent<Collider>());
            }

            // Hollow Eye Sockets with Weeping Blood-Red Light
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.name = side < 0 ? "Eye_Left" : "Eye_Right";
                eye.transform.parent = head.transform;
                eye.transform.localPosition = new Vector3(side * 0.14f, 0.06f, 0.22f);
                eye.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
                if (eyeMat != null) eye.GetComponent<Renderer>().sharedMaterial = eyeMat;
                UnityEngine.Object.DestroyImmediate(eye.GetComponent<Collider>());
            }

            GameObject eyeLightObj = new GameObject("EyeGlowLight");
            eyeLightObj.transform.parent = head.transform;
            eyeLightObj.transform.localPosition = new Vector3(0, 0.08f, 0.28f);
            Light eyeLight = eyeLightObj.AddComponent<Light>();
            eyeLight.type = LightType.Point;
            eyeLight.color = new Color(0.95f, 0.15f, 0.15f); // Blood red
            eyeLight.intensity = 1.2f;
            eyeLight.range = 5.5f;

            // 6. Elongated Articulated Arms reaching past knees with Claw Fingers
            GameObject leftArm = CreateElongatedArm(torso.transform, -1, skinMat, boneMat);
            GameObject rightArm = CreateElongatedArm(torso.transform, 1, skinMat, boneMat);

            // 7. Slender Legs
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                leg.name = side < 0 ? "Leg_Left" : "Leg_Right";
                leg.transform.parent = monster.transform;
                leg.transform.localPosition = new Vector3(side * 0.22f, 0.48f, -0.05f);
                leg.transform.localScale = new Vector3(0.16f, 0.55f, 0.18f);
                if (skinMat != null) leg.GetComponent<Renderer>().sharedMaterial = skinMat;
                UnityEngine.Object.DestroyImmediate(leg.GetComponent<Collider>());
            }

            // Attach Procedural Animator (with lower jaw chattering!)
            SubjectProceduralAnimator anim = monster.AddComponent<SubjectProceduralAnimator>();
            anim.SetupTransforms(torso.transform, head.transform, leftArm.transform, rightArm.transform, jawLower.transform);

            // Attach MonsterController
            MonsterController ctrl = monster.AddComponent<MonsterController>();
            ctrl.SetupEyeLight(eyeLight);
            ctrl.SetSpeeds(1.15f, 1.75f, 3.25f, 1.5f);

            // Connect Waypoints
            SetupWaypointsForMonster(ctrl);
        }

        private static GameObject CreateElongatedArm(Transform parent, int side, Material skinMat, Material boneMat)
        {
            GameObject armRoot = new GameObject(side < 0 ? "ArmRoot_Left" : "ArmRoot_Right");
            armRoot.transform.parent = parent;
            armRoot.transform.localPosition = new Vector3(side * 0.38f, 0.55f, 0.05f);

            // Upper Arm
            GameObject upperArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            upperArm.name = "UpperArm";
            upperArm.transform.parent = armRoot.transform;
            upperArm.transform.localPosition = new Vector3(0, -0.38f, 0);
            upperArm.transform.localScale = new Vector3(0.12f, 0.40f, 0.12f);
            if (skinMat != null) upperArm.GetComponent<Renderer>().sharedMaterial = skinMat;
            UnityEngine.Object.DestroyImmediate(upperArm.GetComponent<Collider>());

            // Lower Forearm (Elongated)
            GameObject forearm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            forearm.name = "Forearm";
            forearm.transform.parent = upperArm.transform;
            forearm.transform.localPosition = new Vector3(0, -0.68f, 0.06f);
            forearm.transform.localRotation = Quaternion.Euler(15f, 0, 0);
            forearm.transform.localScale = new Vector3(0.85f, 1.10f, 0.85f);
            if (skinMat != null) forearm.GetComponent<Renderer>().sharedMaterial = skinMat;
            UnityEngine.Object.DestroyImmediate(forearm.GetComponent<Collider>());

            // Spindly Claws (3 Long Bone Talons)
            for (int f = -1; f <= 1; f++)
            {
                GameObject talon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                talon.name = $"Talon_{f}";
                talon.transform.parent = forearm.transform;
                talon.transform.localPosition = new Vector3(f * 0.05f, -0.58f, 0.04f);
                talon.transform.localRotation = Quaternion.Euler(24f, f * 10f, 0);
                talon.transform.localScale = new Vector3(0.025f, 0.18f, 0.025f);
                if (boneMat != null) talon.GetComponent<Renderer>().sharedMaterial = boneMat;
                UnityEngine.Object.DestroyImmediate(talon.GetComponent<Collider>());
            }

            return armRoot;
        }

        private static void SetupWaypointsForMonster(MonsterController ctrl)
        {
            GameObject wpRoot = GameObject.Find("MonsterWaypoints");
            if (wpRoot == null) return;

            Transform[] childs = wpRoot.GetComponentsInChildren<Transform>();
            foreach (var t in childs)
            {
                if (t != wpRoot.transform) ctrl.AddWaypoint(t);
            }
        }

        #endregion

        #region Standalone Rebuild Engine

        public static void RebuildStandalone()
        {
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(ScenePath2, true)
            };
            EditorBuildSettings.scenes = scenes;

            string buildDir = "Build";
            if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);

            string buildPath = Path.Combine(buildDir, "EscapeTheAbandonedLab.exe");

            BuildPlayerOptions opt = new BuildPlayerOptions
            {
                scenes = new string[] { MainMenuScenePath, ScenePath, ScenePath2 },
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[HorrorEnvironmentDirector] Rebuilding Standalone Executable with Level 1 & Level 2 to: " + buildPath);
            var report = BuildPipeline.BuildPlayer(opt);
            Debug.Log("[HorrorEnvironmentDirector] Build completed with result: " + report.summary.result);
        }

        private static Material GetMat(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>($"{MatDir}/{name}.mat");
        }

        private static void CreateBox(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.parent = parent;
            box.transform.position = pos;
            box.transform.localScale = size;
            if (mat != null) box.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateBulkheadArch(Transform parent, Vector3 pos, Material metalMat, Material hazardMat)
        {
            GameObject arch = new GameObject("Bulkhead_Arch");
            arch.transform.parent = parent;
            arch.transform.position = pos;

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = side < 0 ? "Pillar_Left" : "Pillar_Right";
                pillar.transform.parent = arch.transform;
                pillar.transform.localPosition = new Vector3(side * 1.85f, 1.8f, 0);
                pillar.transform.localScale = new Vector3(0.35f, 3.6f, 0.45f);
                if (metalMat != null) pillar.GetComponent<Renderer>().sharedMaterial = metalMat;
            }

            GameObject crossbeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crossbeam.name = "Crossbeam";
            crossbeam.transform.parent = arch.transform;
            crossbeam.transform.localPosition = new Vector3(0, 3.7f, 0);
            crossbeam.transform.localScale = new Vector3(4.0f, 0.4f, 0.5f);
            if (hazardMat != null) crossbeam.GetComponent<Renderer>().sharedMaterial = hazardMat;
        }

        private static void CreateOverheadPipes(Transform parent, Material metalMat)
        {
            GameObject pipesRoot = new GameObject("Overhead_Industrial_Pipes");
            pipesRoot.transform.parent = parent;

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pipe.name = side < 0 ? "SteamPipe_Left" : "SteamPipe_Right";
                pipe.transform.parent = pipesRoot.transform;
                pipe.transform.localPosition = new Vector3(side * 1.4f, 3.4f, 10.5f);
                pipe.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                pipe.transform.localScale = new Vector3(0.12f, 11f, 0.12f);
                if (metalMat != null) pipe.GetComponent<Renderer>().sharedMaterial = metalMat;
            }
        }

        private static void CreateFloorGratings(Transform parent, Material gratingMat)
        {
            GameObject grates = new GameObject("Metal_Floor_Gratings");
            grates.transform.parent = parent;

            float[] grateZ = new float[] { 5.5f, 11f, 16.5f };
            for (int i = 0; i < grateZ.Length; i++)
            {
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.name = $"Metal_Grate_{i + 1}";
                g.transform.parent = grates.transform;
                g.transform.localPosition = new Vector3(0, 0.01f, grateZ[i]);
                g.transform.localScale = new Vector3(2.6f, 0.02f, 2.2f);
                if (gratingMat != null) g.GetComponent<Renderer>().sharedMaterial = gratingMat;
            }
        }

        private static void CreateMedicalExaminationBed(Transform parent, Vector3 pos, Quaternion rot, Material frameMat, Material sheetMat)
        {
            GameObject bed = new GameObject("Medical_Examination_Bed");
            bed.transform.parent = parent;
            bed.transform.position = pos;
            bed.transform.rotation = rot;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    leg.name = "Leg";
                    leg.transform.parent = bed.transform;
                    leg.transform.localPosition = new Vector3(x * 0.45f, 0.35f, z * 0.95f);
                    leg.transform.localScale = new Vector3(0.06f, 0.35f, 0.06f);
                    if (frameMat != null) leg.GetComponent<Renderer>().sharedMaterial = frameMat;
                }
            }

            GameObject chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chassis.name = "Chassis";
            chassis.transform.parent = bed.transform;
            chassis.transform.localPosition = new Vector3(0, 0.72f, 0);
            chassis.transform.localScale = new Vector3(1.05f, 0.08f, 2.1f);
            if (frameMat != null) chassis.GetComponent<Renderer>().sharedMaterial = frameMat;

            GameObject mattress = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mattress.name = "Mattress";
            mattress.transform.parent = bed.transform;
            mattress.transform.localPosition = new Vector3(0, 0.85f, -0.05f);
            mattress.transform.localScale = new Vector3(0.95f, 0.18f, 1.95f);
            if (sheetMat != null) mattress.GetComponent<Renderer>().sharedMaterial = sheetMat;
        }

        private static void CreateIVDripStand(Transform parent, Vector3 pos, Material mat)
        {
            GameObject iv = new GameObject("IV_Drip_Stand");
            iv.transform.parent = parent;
            iv.transform.position = pos;

            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.parent = iv.transform;
            pole.transform.localPosition = new Vector3(0, 1.1f, 0);
            pole.transform.localScale = new Vector3(0.035f, 1.05f, 0.035f);
            if (mat != null) pole.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateMedicalCart(Transform parent, Vector3 pos, Quaternion rot, Material mat, Material battMat)
        {
            GameObject cart = new GameObject("Medical_Rolling_Cart");
            cart.transform.parent = parent;
            cart.transform.position = pos;
            cart.transform.rotation = rot;

            for (int i = 0; i < 3; i++)
            {
                GameObject tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tray.name = $"Tray_{i}";
                tray.transform.parent = cart.transform;
                tray.transform.localPosition = new Vector3(0, 0.2f + (i * 0.35f), 0);
                tray.transform.localScale = new Vector3(0.65f, 0.03f, 0.95f);
                if (mat != null) tray.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }

        private static void CreateHeartRateMonitorStation(Transform parent, Vector3 pos, Quaternion rot, Material deskMat, Material screenMat)
        {
            GameObject station = new GameObject("HeartRate_Monitor_Station");
            station.transform.parent = parent;
            station.transform.position = pos;
            station.transform.rotation = rot;

            GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "Desk";
            desk.transform.parent = station.transform;
            desk.transform.localPosition = new Vector3(0, 0.45f, 0);
            desk.transform.localScale = new Vector3(1.2f, 0.9f, 0.7f);
            if (deskMat != null) desk.GetComponent<Renderer>().sharedMaterial = deskMat;

            GameObject crt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crt.name = "CRT_Monitor";
            crt.transform.parent = station.transform;
            crt.transform.localPosition = new Vector3(0, 1.15f, 0);
            crt.transform.localScale = new Vector3(0.65f, 0.45f, 0.45f);
            if (deskMat != null) crt.GetComponent<Renderer>().sharedMaterial = deskMat;

            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screen.name = "Screen_Face";
            screen.transform.parent = crt.transform;
            screen.transform.localPosition = new Vector3(0, 0, 0.51f);
            screen.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            screen.transform.localScale = new Vector3(0.9f, 0.85f, 1f);
            if (screenMat != null) screen.GetComponent<Renderer>().sharedMaterial = screenMat;
        }

        private static void CreateWallCabinet(Transform parent, Vector3 pos, Quaternion rot, Material mat)
        {
            GameObject cab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cab.name = "Medicine_Cabinet";
            cab.transform.parent = parent;
            cab.transform.position = pos;
            cab.transform.rotation = rot;
            cab.transform.localScale = new Vector3(0.75f, 0.95f, 0.25f);
            if (mat != null) cab.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateOverturnedStool(Transform parent, Vector3 pos, Material mat)
        {
            GameObject stool = new GameObject("Overturned_Stool");
            stool.transform.parent = parent;
            stool.transform.position = pos;
            stool.transform.rotation = Quaternion.Euler(75f, 30f, 15f);

            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            seat.name = "Seat";
            seat.transform.parent = stool.transform;
            seat.transform.localPosition = new Vector3(0, 0.45f, 0);
            seat.transform.localScale = new Vector3(0.38f, 0.05f, 0.38f);
            if (mat != null) seat.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateRoomBaseboards(Transform parent, Vector3 center, float w, float l, Material mat)
        {
            // Perimeter baseboards
        }

        private static void CreateChemicalBench(Transform parent, Vector3 pos, Material mat)
        {
            GameObject bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.name = "Chemical_Bench";
            bench.transform.parent = parent;
            bench.transform.position = pos + new Vector3(0, 0.45f, 0);
            bench.transform.localScale = new Vector3(4.5f, 0.9f, 1.4f);
            if (mat != null) bench.GetComponent<Renderer>().sharedMaterial = mat;
        }

        #region Gothic Asylum Prop Builders (Image 1 & Image 2)

        private static void CreateVintageHospitalBed(Transform parent, Vector3 pos, Quaternion rot, Material ironMat, Material mattressMat, bool hasMattress)
        {
            GameObject bed = new GameObject("Vintage_Hospital_Bed");
            bed.transform.parent = parent;
            bed.transform.position = pos;
            bed.transform.rotation = rot;

            float width = 1.1f;
            float length = 2.1f;

            // 4 Legs
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    leg.name = "Leg";
                    leg.transform.parent = bed.transform;
                    leg.transform.localPosition = new Vector3(x * (width * 0.5f - 0.05f), 0.35f, z * (length * 0.5f - 0.05f));
                    leg.transform.localScale = new Vector3(0.045f, 0.35f, 0.045f);
                    if (ironMat != null) leg.GetComponent<Renderer>().sharedMaterial = ironMat;
                }
            }

            // Chassis Perimeter Frame
            CreateBox(bed.transform, "Frame_Left", new Vector3(-width * 0.5f + 0.05f, 0.70f, 0), new Vector3(0.04f, 0.04f, length), ironMat);
            CreateBox(bed.transform, "Frame_Right", new Vector3(width * 0.5f - 0.05f, 0.70f, 0), new Vector3(0.04f, 0.04f, length), ironMat);
            CreateBox(bed.transform, "Frame_Head", new Vector3(0, 0.70f, -length * 0.5f + 0.05f), new Vector3(width, 0.04f, 0.04f), ironMat);
            CreateBox(bed.transform, "Frame_Foot", new Vector3(0, 0.70f, length * 0.5f - 0.05f), new Vector3(width, 0.04f, 0.04f), ironMat);

            // Cross spring slats
            for (int s = -4; s <= 4; s++)
            {
                CreateBox(bed.transform, $"Slat_{s}", new Vector3(0, 0.69f, s * 0.20f), new Vector3(width - 0.1f, 0.015f, 0.03f), ironMat);
            }

            // Headboard: Arch Rail + 5 Vertical Rods
            CreateBox(bed.transform, "Headboard_TopRail", new Vector3(0, 1.35f, -length * 0.5f + 0.05f), new Vector3(width, 0.04f, 0.04f), ironMat);
            for (int r = -2; r <= 2; r++)
            {
                CreateBox(bed.transform, $"Head_Spindle_{r}", new Vector3(r * 0.2f, 1.02f, -length * 0.5f + 0.05f), new Vector3(0.02f, 0.60f, 0.02f), ironMat);
            }

            // Footboard: Lower Arch Rail + 5 Vertical Rods
            CreateBox(bed.transform, "Footboard_TopRail", new Vector3(0, 1.05f, length * 0.5f - 0.05f), new Vector3(width, 0.04f, 0.04f), ironMat);
            for (int r = -2; r <= 2; r++)
            {
                CreateBox(bed.transform, $"Foot_Spindle_{r}", new Vector3(r * 0.2f, 0.88f, length * 0.5f - 0.05f), new Vector3(0.02f, 0.30f, 0.02f), ironMat);
            }

            if (hasMattress && mattressMat != null)
            {
                GameObject mat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mat.name = "Stained_Mattress";
                mat.transform.parent = bed.transform;
                mat.transform.localPosition = new Vector3(0, 0.78f, 0);
                mat.transform.localScale = new Vector3(width - 0.12f, 0.14f, length - 0.15f);
                mat.GetComponent<Renderer>().sharedMaterial = mattressMat;
            }
        }

        private static void CreateBarredWindow(Transform parent, Vector3 pos, Quaternion rot, Material frameMat, Material glassMat)
        {
            GameObject win = new GameObject("Asylum_Barred_Window");
            win.transform.parent = parent;
            win.transform.position = pos;
            win.transform.rotation = rot;

            float w = 1.3f;
            float h = 2.4f;

            // Outer Frame
            CreateBox(win.transform, "Frame_Top", new Vector3(0, h * 0.5f, 0), new Vector3(w + 0.1f, 0.08f, 0.15f), frameMat);
            CreateBox(win.transform, "Frame_Bottom", new Vector3(0, -h * 0.5f, 0), new Vector3(w + 0.1f, 0.12f, 0.25f), frameMat);
            CreateBox(win.transform, "Frame_Left", new Vector3(-w * 0.5f, 0, 0), new Vector3(0.08f, h, 0.15f), frameMat);
            CreateBox(win.transform, "Frame_Right", new Vector3(w * 0.5f, 0, 0), new Vector3(0.08f, h, 0.15f), frameMat);

            // Barred Grill: 4 vertical bars, 3 horizontal bars
            for (int vx = -2; vx <= 2; vx++)
            {
                if (vx == 0) continue;
                CreateBox(win.transform, $"VBar_{vx}", new Vector3(vx * 0.25f, 0, 0.04f), new Vector3(0.03f, h, 0.03f), frameMat);
            }
            for (int hy = -1; hy <= 1; hy++)
            {
                CreateBox(win.transform, $"HBar_{hy}", new Vector3(0, hy * 0.55f, 0.04f), new Vector3(w, 0.03f, 0.03f), frameMat);
            }

            // Glass Pane
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glass.name = "Window_Glass";
            glass.transform.parent = win.transform;
            glass.transform.localPosition = new Vector3(0, 0, 0.01f);
            glass.transform.localScale = new Vector3(w, h, 1f);
            if (glassMat != null) glass.GetComponent<Renderer>().sharedMaterial = glassMat;

            // Exterior cool moonlight glow
            GameObject moonlight = new GameObject("Moonlight_Exterior");
            moonlight.transform.parent = win.transform;
            moonlight.transform.localPosition = new Vector3(0, 0, -0.4f);
            Light lt = moonlight.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(0.12f, 0.24f, 0.38f);
            lt.intensity = 0.50f;
            lt.range = 5.5f;
        }

        private static void CreateCastIronRadiator(Transform parent, Vector3 pos, Quaternion rot, Material ironMat)
        {
            GameObject rad = new GameObject("Cast_Iron_Radiator");
            rad.transform.parent = parent;
            rad.transform.position = pos;
            rad.transform.rotation = rot;

            float width = 1.0f;
            int finCount = 8;
            for (int i = 0; i < finCount; i++)
            {
                float x = -width * 0.5f + (i * (width / (finCount - 1)));
                CreateBox(rad.transform, $"Fin_{i}", new Vector3(x, 0.35f, 0), new Vector3(0.04f, 0.65f, 0.16f), ironMat);
            }
            CreateBox(rad.transform, "Pipe_Top", new Vector3(0, 0.65f, 0), new Vector3(width + 0.08f, 0.05f, 0.05f), ironMat);
            CreateBox(rad.transform, "Pipe_Bottom", new Vector3(0, 0.10f, 0), new Vector3(width + 0.08f, 0.05f, 0.05f), ironMat);
            CreateBox(rad.transform, "Steam_Valve", new Vector3(width * 0.5f + 0.08f, 0.65f, 0), new Vector3(0.06f, 0.08f, 0.06f), ironMat);
        }

        private static void CreateAnalogWallClock(Transform parent, Vector3 pos, Quaternion rot)
        {
            GameObject clock = new GameObject("Vintage_Asylum_Wall_Clock");
            clock.transform.parent = parent;
            clock.transform.position = pos;
            clock.transform.rotation = rot;

            Material brassMat = GetOrCreateMaterial("M_Apparition_Brass", new Color(0.55f, 0.45f, 0.20f), 0.7f);
            Material faceMat = GetOrCreateMaterial("M_Clock_Face", new Color(0.85f, 0.83f, 0.75f), 0.3f);
            Material darkMat = GetOrCreateMaterial("M_Apparition_Hair", Color.black, 0.5f);

            GameObject casing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            casing.name = "Clock_Casing";
            casing.transform.parent = clock.transform;
            casing.transform.localPosition = Vector3.zero;
            casing.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            casing.transform.localScale = new Vector3(0.55f, 0.04f, 0.55f);
            casing.GetComponent<Renderer>().sharedMaterial = brassMat;

            GameObject face = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            face.name = "Clock_Dial";
            face.transform.parent = clock.transform;
            face.transform.localPosition = new Vector3(0, 0, 0.03f);
            face.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            face.transform.localScale = new Vector3(0.48f, 0.02f, 0.48f);
            face.GetComponent<Renderer>().sharedMaterial = faceMat;

            CreateBox(clock.transform, "Hour_Hand", new Vector3(0.06f, -0.02f, 0.045f), new Vector3(0.12f, 0.02f, 0.01f), darkMat);
            CreateBox(clock.transform, "Minute_Hand", new Vector3(0.04f, 0.08f, 0.045f), new Vector3(0.015f, 0.18f, 0.01f), darkMat);
        }

        private static void CreateWallBulkheadSconce(Transform parent, Vector3 pos, Quaternion rot, Color lightColor)
        {
            GameObject sconce = new GameObject("Bulkhead_Wall_Sconce");
            sconce.transform.parent = parent;
            sconce.transform.position = pos;
            sconce.transform.rotation = rot;

            Material ironMat = GetOrCreateMaterial("M_Apparition_Iron", new Color(0.12f, 0.13f, 0.14f), 0.6f);
            Material glassMat = GetOrCreateEmissiveMaterial("M_Sconce_Glass", new Color(1f, 0.9f, 0.7f), lightColor * 2.0f);

            CreateBox(sconce.transform, "Mount", Vector3.zero, new Vector3(0.18f, 0.28f, 0.06f), ironMat);

            GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulb.name = "Glass_Dome";
            bulb.transform.parent = sconce.transform;
            bulb.transform.localPosition = new Vector3(0, 0, 0.08f);
            bulb.transform.localScale = new Vector3(0.14f, 0.22f, 0.10f);
            bulb.GetComponent<Renderer>().sharedMaterial = glassMat;
            UnityEngine.Object.DestroyImmediate(bulb.GetComponent<Collider>());

            Light lt = sconce.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = lightColor;
            lt.intensity = 0.65f;
            lt.range = 5.0f;
        }

        private static void CreateHangingFluorescentFixture(Transform parent, Vector3 pos, Quaternion rot, bool isBrokenAngle, Color tubeColor)
        {
            GameObject fix = new GameObject(isBrokenAngle ? "Fluorescent_Dangling_Broken" : "Fluorescent_Hanging");
            fix.transform.parent = parent;
            fix.transform.position = pos;
            fix.transform.rotation = rot * (isBrokenAngle ? Quaternion.Euler(18f, 12f, -22f) : Quaternion.identity);

            Material sheetMat = GetOrCreateMaterial("M_Fluorescent_Housing", new Color(0.25f, 0.27f, 0.28f), 0.4f);
            Material tubeMat = GetOrCreateEmissiveMaterial("M_Fluorescent_Tube", new Color(0.8f, 1f, 0.9f), tubeColor * 2.8f);

            CreateBox(fix.transform, "Housing", new Vector3(0, 0, 0), new Vector3(0.35f, 0.08f, 1.8f), sheetMat);
            CreateBox(fix.transform, "Tube_L", new Vector3(-0.08f, -0.04f, 0), new Vector3(0.035f, 0.035f, 1.7f), tubeMat);
            CreateBox(fix.transform, "Tube_R", new Vector3(0.08f, -0.04f, 0), new Vector3(0.035f, 0.035f, 1.7f), tubeMat);

            CreateBox(fix.transform, "Chain_Fore", new Vector3(0, 0.40f, 0.75f), new Vector3(0.015f, 0.80f, 0.015f), sheetMat);
            if (!isBrokenAngle)
            {
                CreateBox(fix.transform, "Chain_Aft", new Vector3(0, 0.40f, -0.75f), new Vector3(0.015f, 0.80f, 0.015f), sheetMat);
            }
            else
            {
                CreateBox(fix.transform, "Chain_BrokenDangle", new Vector3(0, -0.15f, -0.75f), new Vector3(0.015f, 0.30f, 0.015f), sheetMat);
            }

            GameObject ltObj = new GameObject("Tube_Light");
            ltObj.transform.parent = fix.transform;
            ltObj.transform.localPosition = new Vector3(0, -0.1f, 0);
            Light lt = ltObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = tubeColor;
            lt.intensity = isBrokenAngle ? 1.4f : 0.9f;
            lt.range = 8.0f;
            lt.shadows = LightShadows.Hard;

            if (isBrokenAngle)
            {
                fix.AddComponent<EscapeTheLab.Environment.FlickeringLight>();
            }
        }

        private static void CreateDoubleHospitalDoor(Transform parent, Vector3 pos, Quaternion rot, Material doorMat, Material trimMat)
        {
            GameObject doorArch = new GameObject("Asylum_Double_Doors");
            doorArch.transform.parent = parent;
            doorArch.transform.position = pos;
            doorArch.transform.rotation = rot;

            float w = 2.4f;
            float h = 2.8f;

            CreateBox(doorArch.transform, "Surround_L", new Vector3(-w * 0.5f - 0.12f, h * 0.5f, 0), new Vector3(0.24f, h, 0.35f), trimMat);
            CreateBox(doorArch.transform, "Surround_R", new Vector3(w * 0.5f + 0.12f, h * 0.5f, 0), new Vector3(0.24f, h, 0.35f), trimMat);
            CreateBox(doorArch.transform, "Surround_Arch", new Vector3(0, h + 0.12f, 0), new Vector3(w + 0.48f, 0.24f, 0.35f), trimMat);

            GameObject leafL = GameObject.CreatePrimitive(PrimitiveType.Quad);
            leafL.name = "Leaf_Left";
            leafL.transform.parent = doorArch.transform;
            leafL.transform.localPosition = new Vector3(-w * 0.25f, h * 0.5f, 0.02f);
            leafL.transform.localScale = new Vector3(w * 0.5f, h, 1f);
            if (doorMat != null) leafL.GetComponent<Renderer>().sharedMaterial = doorMat;

            GameObject leafR = GameObject.CreatePrimitive(PrimitiveType.Quad);
            leafR.name = "Leaf_Right";
            leafR.transform.parent = doorArch.transform;
            leafR.transform.localPosition = new Vector3(w * 0.25f, h * 0.5f, 0.02f);
            leafR.transform.localScale = new Vector3(w * 0.5f, h, 1f);
            if (doorMat != null) leafR.GetComponent<Renderer>().sharedMaterial = doorMat;

            Material brassMat = GetOrCreateMaterial("M_Apparition_Brass", new Color(0.6f, 0.5f, 0.2f), 0.7f);
            CreateBox(doorArch.transform, "PushBar_L", new Vector3(-w * 0.25f, 1.1f, 0.05f), new Vector3(0.7f, 0.04f, 0.04f), brassMat);
            CreateBox(doorArch.transform, "PushBar_R", new Vector3(w * 0.25f, 1.1f, 0.05f), new Vector3(0.7f, 0.04f, 0.04f), brassMat);
        }

        private static void CreateOverDoorDomeLamp(Transform parent, Vector3 pos, Color lightColor)
        {
            GameObject dome = new GameObject("Overdoor_Dome_Lamp");
            dome.transform.parent = parent;
            dome.transform.position = pos;

            Material brassMat = GetOrCreateMaterial("M_Apparition_Brass", new Color(0.5f, 0.4f, 0.16f), 0.7f);
            Material emissiveMat = GetOrCreateEmissiveMaterial("M_Dome_Lamp_Glow", new Color(1.0f, 0.95f, 0.75f), lightColor * 3.0f);

            GameObject mount = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mount.name = "Mount_Plate";
            mount.transform.parent = dome.transform;
            mount.transform.localPosition = new Vector3(0, 0.08f, 0);
            mount.transform.localScale = new Vector3(0.42f, 0.04f, 0.42f);
            mount.GetComponent<Renderer>().sharedMaterial = brassMat;

            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glass.name = "Glass_Globe";
            glass.transform.parent = dome.transform;
            glass.transform.localPosition = Vector3.zero;
            glass.transform.localScale = new Vector3(0.38f, 0.28f, 0.38f);
            glass.GetComponent<Renderer>().sharedMaterial = emissiveMat;
            UnityEngine.Object.DestroyImmediate(glass.GetComponent<Collider>());

            Light lt = dome.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = lightColor;
            lt.intensity = 1.35f;
            lt.range = 8.5f;
            lt.shadows = LightShadows.Hard;
        }

        private static void CreateLowSpecimenTable(Transform parent, Vector3 pos, Quaternion rot, Material woodMat, Material glassMat)
        {
            GameObject tbl = new GameObject("Specimen_Examination_Table");
            tbl.transform.parent = parent;
            tbl.transform.position = pos;
            tbl.transform.rotation = rot;

            CreateBox(tbl.transform, "Top", new Vector3(0, 0.42f, 0), new Vector3(1.1f, 0.05f, 1.6f), woodMat);
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    CreateBox(tbl.transform, "Leg", new Vector3(x * 0.48f, 0.20f, z * 0.72f), new Vector3(0.07f, 0.40f, 0.07f), woodMat);
                }
            }

            Material specimenMat = GetOrCreateEmissiveMaterial("M_Specimen_Tissue", new Color(0.6f, 0.3f, 0.1f), new Color(0.8f, 0.6f, 0.2f) * 1.5f);
            float[] jarZ = new float[] { -0.35f, 0.05f, 0.40f };
            float[] jarH = new float[] { 0.28f, 0.35f, 0.25f };
            float[] jarR = new float[] { 0.14f, 0.18f, 0.13f };

            for (int j = 0; j < 3; j++)
            {
                GameObject jar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                jar.name = $"Specimen_Jar_{j + 1}";
                jar.transform.parent = tbl.transform;
                jar.transform.localPosition = new Vector3((j % 2 == 0 ? 0.08f : -0.08f), 0.45f + (jarH[j] * 0.5f), jarZ[j]);
                jar.transform.localScale = new Vector3(jarR[j] * 2f, jarH[j] * 0.5f, jarR[j] * 2f);
                if (glassMat != null) jar.GetComponent<Renderer>().sharedMaterial = glassMat;

                GameObject tissue = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tissue.name = "Specimen_Preserved_Organ";
                tissue.transform.parent = jar.transform;
                tissue.transform.localPosition = Vector3.zero;
                tissue.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
                tissue.GetComponent<Renderer>().sharedMaterial = specimenMat;
                UnityEngine.Object.DestroyImmediate(tissue.GetComponent<Collider>());
            }
        }

        private static void CreateOverturnedWastebasket(Transform parent, Vector3 pos, Material ironMat)
        {
            GameObject basket = new GameObject("Overturned_Wastebasket");
            basket.transform.parent = parent;
            basket.transform.position = pos;
            basket.transform.rotation = Quaternion.Euler(80f, 35f, -15f);

            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.name = "Basket_Mesh";
            cyl.transform.parent = basket.transform;
            cyl.transform.localPosition = Vector3.zero;
            cyl.transform.localScale = new Vector3(0.32f, 0.24f, 0.32f);
            if (ironMat != null) cyl.GetComponent<Renderer>().sharedMaterial = ironMat;

            Material paperMat = GetMat("M_Asylum_Patient_Paper");
            for (int i = 0; i < 4; i++)
            {
                GameObject p = GameObject.CreatePrimitive(PrimitiveType.Quad);
                p.name = $"Spilled_Paper_{i}";
                p.transform.parent = parent;
                p.transform.position = pos + new Vector3(0.25f + (i * 0.12f), 0.015f, -0.1f + (i * 0.08f));
                p.transform.rotation = Quaternion.Euler(90f, Random.Range(-40f, 40f), 0f);
                p.transform.localScale = new Vector3(0.18f, 0.24f, 1f);
                if (paperMat != null) p.GetComponent<Renderer>().sharedMaterial = paperMat;
                UnityEngine.Object.DestroyImmediate(p.GetComponent<Collider>());
            }
        }

        private static void CreateRestraintCribCot(Transform parent, Vector3 pos, Quaternion rot, Material ironMat)
        {
            GameObject cot = new GameObject("Restraint_Crib_Cot");
            cot.transform.parent = parent;
            cot.transform.position = pos;
            cot.transform.rotation = rot;

            float w = 0.9f;
            float l = 1.8f;
            float h = 1.3f;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    CreateBox(cot.transform, "Post", new Vector3(x * (w * 0.5f), h * 0.5f, z * (l * 0.5f)), new Vector3(0.05f, h, 0.05f), ironMat);
                }
            }
            CreateBox(cot.transform, "Base", new Vector3(0, 0.45f, 0), new Vector3(w, 0.04f, l), ironMat);

            for (int s = -3; s <= 3; s++)
            {
                CreateBox(cot.transform, $"SideBar_L_{s}", new Vector3(-w * 0.5f, 0.90f, s * 0.22f), new Vector3(0.02f, 0.85f, 0.02f), ironMat);
                CreateBox(cot.transform, $"SideBar_R_{s}", new Vector3(w * 0.5f, 0.90f, s * 0.22f), new Vector3(0.02f, 0.85f, 0.02f), ironMat);
            }
            CreateBox(cot.transform, "TopRail_L", new Vector3(-w * 0.5f, h, 0), new Vector3(0.04f, 0.04f, l), ironMat);
            CreateBox(cot.transform, "TopRail_R", new Vector3(w * 0.5f, h, 0), new Vector3(0.04f, 0.04f, l), ironMat);
        }

        private static void CreateLeaningMattressAndBoards(Transform parent, Vector3 pos, Material mattressMat, Material woodMat)
        {
            GameObject group = new GameObject("Leaning_Debris_Group");
            group.transform.parent = parent;
            group.transform.position = pos;

            GameObject mat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mat.name = "Leaning_Hospital_Mattress";
            mat.transform.parent = group.transform;
            mat.transform.localPosition = new Vector3(0, 0.95f, 0.2f);
            mat.transform.localRotation = Quaternion.Euler(-18f, 15f, 0);
            mat.transform.localScale = new Vector3(0.85f, 1.85f, 0.16f);
            if (mattressMat != null) mat.GetComponent<Renderer>().sharedMaterial = mattressMat;

            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Leaning_Plywood_Sheet";
            board.transform.parent = group.transform;
            board.transform.localPosition = new Vector3(0.6f, 0.85f, 0.15f);
            board.transform.localRotation = Quaternion.Euler(-22f, -8f, 0);
            board.transform.localScale = new Vector3(0.7f, 1.7f, 0.04f);
            if (woodMat != null) board.GetComponent<Renderer>().sharedMaterial = woodMat;
        }

        private static void CreateScatteredPatientPapers(Transform parent, Vector3 startPos, Vector3 endPos, int count, Material paperMat)
        {
            GameObject group = new GameObject("Scattered_Patient_Documents");
            group.transform.parent = parent;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                Vector3 basePos = Vector3.Lerp(startPos, endPos, t);
                Vector3 jitter = new Vector3(Random.Range(-1.1f, 1.1f), 0.015f, Random.Range(-0.3f, 0.3f));
                Vector3 pos = basePos + jitter;

                GameObject doc = GameObject.CreatePrimitive(PrimitiveType.Quad);
                doc.name = $"Patient_CaseFile_{i + 1}";
                doc.transform.parent = group.transform;
                doc.transform.position = pos;
                doc.transform.rotation = Quaternion.Euler(90f, Random.Range(-55f, 55f), 0f);
                doc.transform.localScale = new Vector3(0.24f, 0.34f, 1f);
                if (paperMat != null) doc.GetComponent<Renderer>().sharedMaterial = paperMat;
                UnityEngine.Object.DestroyImmediate(doc.GetComponent<Collider>());
            }
        }

        private static void CreateRolledBeddingBundle(Transform parent, Vector3 pos, Quaternion rot, Material mat)
        {
            GameObject roll = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            roll.name = "Rolled_Bedding_Bundle";
            roll.transform.parent = parent;
            roll.transform.position = pos;
            roll.transform.rotation = rot;
            roll.transform.localScale = new Vector3(0.40f, 0.65f, 0.40f);
            if (mat != null) roll.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateCorridorTransomDoor(Transform parent, Vector3 pos, Quaternion rot, Material doorMat, Material trimMat, string roomLabel)
        {
            GameObject doorRoot = new GameObject($"Transom_Door_{roomLabel}");
            doorRoot.transform.parent = parent;
            doorRoot.transform.position = pos;
            doorRoot.transform.rotation = rot;

            float w = 1.2f;
            float h = 2.4f;

            CreateBox(doorRoot.transform, "Frame_L", new Vector3(-w * 0.5f - 0.04f, 1.4f, 0), new Vector3(0.08f, 2.8f, 0.12f), trimMat);
            CreateBox(doorRoot.transform, "Frame_R", new Vector3(w * 0.5f + 0.04f, 1.4f, 0), new Vector3(0.08f, 2.8f, 0.12f), trimMat);
            CreateBox(doorRoot.transform, "Frame_Top", new Vector3(0, 2.84f, 0), new Vector3(w + 0.16f, 0.08f, 0.12f), trimMat);
            CreateBox(doorRoot.transform, "Transom_Lintel", new Vector3(0, 2.15f, 0), new Vector3(w, 0.06f, 0.12f), trimMat);

            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Quad);
            leaf.name = "Door_Leaf";
            leaf.transform.parent = doorRoot.transform;
            leaf.transform.localPosition = new Vector3(0, 1.05f, 0.02f);
            leaf.transform.localScale = new Vector3(w, 2.1f, 1f);
            if (doorMat != null) leaf.GetComponent<Renderer>().sharedMaterial = doorMat;

            Material glassMat = GetMat("M_Lab_Glass");
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glass.name = "Transom_Glass";
            glass.transform.parent = doorRoot.transform;
            glass.transform.localPosition = new Vector3(0, 2.50f, 0.02f);
            glass.transform.localScale = new Vector3(w - 0.04f, 0.60f, 1f);
            if (glassMat != null) glass.GetComponent<Renderer>().sharedMaterial = glassMat;

            Material brassMat = GetOrCreateMaterial("M_Apparition_Brass", new Color(0.6f, 0.48f, 0.18f), 0.65f);
            CreateBox(doorRoot.transform, "Brass_Plaque", new Vector3(0, 1.65f, 0.035f), new Vector3(0.35f, 0.12f, 0.02f), brassMat);
        }

        private static void CreateWallFramedCertificate(Transform parent, Vector3 pos, Quaternion rot, Material frameMat, Material paperMat)
        {
            GameObject frame = new GameObject("Framed_Medical_License");
            frame.transform.parent = parent;
            frame.transform.position = pos;
            frame.transform.rotation = rot;

            CreateBox(frame.transform, "Moulding", Vector3.zero, new Vector3(0.65f, 0.85f, 0.04f), frameMat);

            GameObject paper = GameObject.CreatePrimitive(PrimitiveType.Quad);
            paper.name = "Certificate_Face";
            paper.transform.parent = frame.transform;
            paper.transform.localPosition = new Vector3(0, 0, 0.025f);
            paper.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            paper.transform.localScale = new Vector3(0.55f, 0.75f, 1f);
            if (paperMat != null) paper.GetComponent<Renderer>().sharedMaterial = paperMat;
            UnityEngine.Object.DestroyImmediate(paper.GetComponent<Collider>());
        }

        private static void CreateCorridorGlobeLamp(Transform parent, Vector3 pos, Color color, float intensity, float range)
        {
            GameObject lamp = new GameObject("Corridor_Globe_Lamp");
            lamp.transform.parent = parent;
            lamp.transform.position = pos;

            Material brassMat = GetOrCreateMaterial("M_Apparition_Brass", new Color(0.5f, 0.4f, 0.16f), 0.7f);
            Material globeMat = GetOrCreateEmissiveMaterial("M_Globe_Glass", new Color(1.0f, 0.88f, 0.65f), color * 2.2f);

            CreateBox(lamp.transform, "Canopy", new Vector3(0, 0.22f, 0), new Vector3(0.18f, 0.04f, 0.18f), brassMat);
            CreateBox(lamp.transform, "Downrod", new Vector3(0, 0.12f, 0), new Vector3(0.025f, 0.20f, 0.025f), brassMat);

            GameObject globe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            globe.name = "Globe";
            globe.transform.parent = lamp.transform;
            globe.transform.localPosition = Vector3.zero;
            globe.transform.localScale = new Vector3(0.32f, 0.32f, 0.32f);
            globe.GetComponent<Renderer>().sharedMaterial = globeMat;
            UnityEngine.Object.DestroyImmediate(globe.GetComponent<Collider>());

            Light lt = lamp.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = color;
            lt.intensity = intensity;
            lt.range = range;
            lt.shadows = LightShadows.Hard;
        }

        #endregion

        #region Seated Apparition (Gothic Horror Specter from Blockout)

        private static void BuildSeatedApparition(Transform parent, Vector3 pos, Quaternion rot)
        {
            GameObject apparitionRoot = new GameObject("Seated_Apparition_Entity");
            apparitionRoot.transform.parent = parent;
            apparitionRoot.transform.position = pos;
            apparitionRoot.transform.rotation = rot;

            // Materials exactly matching the Blockout
            Material skinMat = GetOrCreateMaterial("M_Apparition_Skin", new Color(0.70f, 0.73f, 0.66f), 0.2f);
            Material hairMat = GetOrCreateMaterial("M_Apparition_Hair", new Color(0.05f, 0.04f, 0.04f), 0.5f);
            Material gownMat = GetOrCreateMaterial("M_Apparition_Gown", new Color(0.85f, 0.83f, 0.76f), 0.1f);
            Material laceMat = GetOrCreateMaterial("M_Apparition_Lace", new Color(0.91f, 0.89f, 0.84f), 0.2f);
            Material eyeMat = GetOrCreateEmissiveMaterial("M_Apparition_Eye", new Color(0.94f, 0.94f, 0.92f), Color.white * 0.9f);
            Material lipMat = GetOrCreateMaterial("M_Apparition_Lip", new Color(0.29f, 0.05f, 0.08f), 0.4f);
            Material brassMat = GetOrCreateMaterial("M_Apparition_Brass", new Color(0.54f, 0.42f, 0.16f), 0.6f);
            if (brassMat != null) brassMat.SetFloat("_Metallic", 0.85f);
            Material waxMat = GetOrCreateMaterial("M_Apparition_Wax", new Color(0.93f, 0.90f, 0.82f), 0.2f);
            Material ironMat = GetOrCreateMaterial("M_Apparition_Iron", new Color(0.08f, 0.09f, 0.10f), 0.5f);
            if (ironMat != null) ironMat.SetFloat("_Metallic", 0.75f);
            Material mattressMat = GetOrCreateMaterial("M_Apparition_Mattress", new Color(0.10f, 0.12f, 0.11f), 0.1f);

            // ---- Env_Bed ----
            GameObject bed = new GameObject("Env_Bed");
            bed.transform.parent = apparitionRoot.transform;
            bed.transform.localPosition = Vector3.zero;

            // Headboard Rail
            CreateBox(bed.transform, "Headboard_Rail", new Vector3(0, 1.15f, -0.55f), new Vector3(1.9f, 0.05f, 0.05f), ironMat);

            // Headboard Posts
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = s < 0 ? "Headboard_Post_L" : "Headboard_Post_R";
                post.transform.parent = bed.transform;
                post.transform.localPosition = new Vector3(s * 0.95f, 0.65f, -0.55f);
                post.transform.localScale = new Vector3(0.06f, 0.65f, 0.06f);
                if (ironMat != null) post.GetComponent<Renderer>().sharedMaterial = ironMat;
            }

            // Mattress
            CreateBox(bed.transform, "Mattress", new Vector3(0, 0.15f, 0.3f), new Vector3(1.9f, 0.3f, 1.7f), mattressMat);

            // ---- Character_Apparition ----
            GameObject charRoot = new GameObject("Character_Apparition");
            charRoot.transform.parent = apparitionRoot.transform;
            charRoot.transform.localPosition = Vector3.zero;

            // Gown Bodice
            GameObject bodice = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bodice.name = "Gown_Bodice";
            bodice.transform.parent = charRoot.transform;
            bodice.transform.localPosition = new Vector3(0, 0.95f, 0);
            bodice.transform.localScale = new Vector3(0.56f, 0.40f, 0.56f);
            if (gownMat != null) bodice.GetComponent<Renderer>().sharedMaterial = gownMat;
            UnityEngine.Object.DestroyImmediate(bodice.GetComponent<Collider>());

            // Gown Skirt Lap
            GameObject lap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lap.name = "Gown_Skirt_Lap";
            lap.transform.parent = charRoot.transform;
            lap.transform.localPosition = new Vector3(0, 0.5f, 0.35f);
            lap.transform.localScale = new Vector3(1.2f, 0.4f, 1.5f);
            if (gownMat != null) lap.GetComponent<Renderer>().sharedMaterial = gownMat;
            UnityEngine.Object.DestroyImmediate(lap.GetComponent<Collider>());

            // Neck
            GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neck.name = "Neck";
            neck.transform.parent = charRoot.transform;
            neck.transform.localPosition = new Vector3(0, 1.48f, 0);
            neck.transform.localScale = new Vector3(0.13f, 0.125f, 0.13f);
            if (skinMat != null) neck.GetComponent<Renderer>().sharedMaterial = skinMat;
            UnityEngine.Object.DestroyImmediate(neck.GetComponent<Collider>());

            // Head (Sphere scaled .30, .42, .34)
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.parent = charRoot.transform;
            head.transform.localPosition = new Vector3(0, 1.72f, 0);
            head.transform.localScale = new Vector3(0.30f, 0.42f, 0.34f);
            if (skinMat != null) head.GetComponent<Renderer>().sharedMaterial = skinMat;
            UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());

            // Eyes, Shoulders, Arms, Cuffs, Hands, Hair Strands
            for (int s = -1; s <= 1; s += 2)
            {
                // Eye
                GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.name = s < 0 ? "Eye_L" : "Eye_R";
                eye.transform.parent = head.transform;
                eye.transform.localPosition = new Vector3(s * 0.19f, 0.05f, 0.42f);
                eye.transform.localScale = new Vector3(0.18f, 0.12f, 0.12f);
                if (eyeMat != null) eye.GetComponent<Renderer>().sharedMaterial = eyeMat;
                UnityEngine.Object.DestroyImmediate(eye.GetComponent<Collider>());

                // Shoulder
                GameObject shld = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shld.name = s < 0 ? "Shoulder_L" : "Shoulder_R";
                shld.transform.parent = charRoot.transform;
                shld.transform.localPosition = new Vector3(s * 0.3f, 1.3f, 0);
                shld.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
                if (skinMat != null) shld.GetComponent<Renderer>().sharedMaterial = skinMat;
                UnityEngine.Object.DestroyImmediate(shld.GetComponent<Collider>());

                // Upper Arm
                CreateLimb(charRoot.transform, new Vector3(s * 0.3f, 1.3f, 0), new Vector3(s * 0.38f, 0.88f, 0.1f), 0.12f, gownMat, s < 0 ? "UpperArm_L" : "UpperArm_R");

                // Forearm
                CreateLimb(charRoot.transform, new Vector3(s * 0.38f, 0.88f, 0.1f), new Vector3(s * 0.08f, 0.82f, 0.4f), 0.10f, gownMat, s < 0 ? "Forearm_L" : "Forearm_R");

                // Lace Cuff
                GameObject cuff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cuff.name = s < 0 ? "LaceCuff_L" : "LaceCuff_R";
                cuff.transform.parent = charRoot.transform;
                cuff.transform.localPosition = new Vector3(s * 0.1f, 0.82f, 0.38f);
                cuff.transform.localRotation = Quaternion.Euler(0, s * 75f, 0);
                cuff.transform.localScale = new Vector3(0.11f, 0.03f, 0.11f);
                if (laceMat != null) cuff.GetComponent<Renderer>().sharedMaterial = laceMat;
                UnityEngine.Object.DestroyImmediate(cuff.GetComponent<Collider>());

                // Hand
                GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hand.name = s < 0 ? "Hand_L" : "Hand_R";
                hand.transform.parent = charRoot.transform;
                hand.transform.localPosition = new Vector3(s * 0.06f, 0.82f, 0.43f);
                hand.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
                if (skinMat != null) hand.GetComponent<Renderer>().sharedMaterial = skinMat;
                UnityEngine.Object.DestroyImmediate(hand.GetComponent<Collider>());

                // Hair Front Strand
                GameObject hStrand = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hStrand.name = s < 0 ? "Hair_FrontStrand_L" : "Hair_FrontStrand_R";
                hStrand.transform.parent = charRoot.transform;
                hStrand.transform.localPosition = new Vector3(s * 0.17f, 1.2f, 0.12f);
                hStrand.transform.localRotation = Quaternion.Euler(0, 0, s * 4f);
                hStrand.transform.localScale = new Vector3(0.04f, 0.55f, 0.02f);
                if (hairMat != null) hStrand.GetComponent<Renderer>().sharedMaterial = hairMat;
                UnityEngine.Object.DestroyImmediate(hStrand.GetComponent<Collider>());

                // Hair Cheek Strand
                GameObject cStrand = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cStrand.name = s < 0 ? "Hair_CheekStrand_L" : "Hair_CheekStrand_R";
                cStrand.transform.parent = head.transform;
                cStrand.transform.localPosition = new Vector3(s * 0.33f, -0.32f, 0.48f);
                cStrand.transform.localScale = new Vector3(0.04f, 0.70f, 0.03f);
                if (hairMat != null) cStrand.GetComponent<Renderer>().sharedMaterial = hairMat;
                UnityEngine.Object.DestroyImmediate(cStrand.GetComponent<Collider>());
            }

            // Creepy Wide Curved Smile Mouth
            GameObject mouth = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mouth.name = "Mouth_Smile";
            mouth.transform.parent = head.transform;
            mouth.transform.localPosition = new Vector3(0, -0.22f, 0.46f);
            mouth.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            mouth.transform.localScale = new Vector3(0.24f, 0.02f, 0.10f);
            if (lipMat != null) mouth.GetComponent<Renderer>().sharedMaterial = lipMat;
            UnityEngine.Object.DestroyImmediate(mouth.GetComponent<Collider>());

            // Hair Crown
            GameObject hCrown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hCrown.name = "Hair_Crown";
            hCrown.transform.parent = head.transform;
            hCrown.transform.localPosition = new Vector3(0, 0.02f, -0.12f);
            hCrown.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
            if (hairMat != null) hCrown.GetComponent<Renderer>().sharedMaterial = hairMat;
            UnityEngine.Object.DestroyImmediate(hCrown.GetComponent<Collider>());

            // Hair Bangs
            GameObject hBangs = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hBangs.name = "Hair_Bangs";
            hBangs.transform.parent = head.transform;
            hBangs.transform.localPosition = new Vector3(0, 0.38f, 0.42f);
            hBangs.transform.localScale = new Vector3(0.85f, 0.18f, 0.08f);
            if (hairMat != null) hBangs.GetComponent<Renderer>().sharedMaterial = hairMat;
            UnityEngine.Object.DestroyImmediate(hBangs.GetComponent<Collider>());

            // Hair Back
            GameObject hBack = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hBack.name = "Hair_Back";
            hBack.transform.parent = charRoot.transform;
            hBack.transform.localPosition = new Vector3(0, 1.2f, -0.14f);
            hBack.transform.localScale = new Vector3(0.36f, 0.45f, 0.28f);
            if (hairMat != null) hBack.GetComponent<Renderer>().sharedMaterial = hairMat;
            UnityEngine.Object.DestroyImmediate(hBack.GetComponent<Collider>());

            // ---- Prop_CandleHolder ----
            GameObject candleRoot = new GameObject("Prop_CandleHolder");
            candleRoot.transform.parent = apparitionRoot.transform;
            candleRoot.transform.localPosition = new Vector3(0, 0.8f, 0.42f);

            // Brass Base
            GameObject brassBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            brassBase.name = "Brass_Base";
            brassBase.transform.parent = candleRoot.transform;
            brassBase.transform.localPosition = new Vector3(0, -0.02f, 0);
            brassBase.transform.localScale = new Vector3(0.13f, 0.015f, 0.13f);
            if (brassMat != null) brassBase.GetComponent<Renderer>().sharedMaterial = brassMat;
            UnityEngine.Object.DestroyImmediate(brassBase.GetComponent<Collider>());

            // Brass Stem
            GameObject brassStem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            brassStem.name = "Brass_Stem";
            brassStem.transform.parent = candleRoot.transform;
            brassStem.transform.localPosition = new Vector3(0, 0.02f, 0);
            brassStem.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
            if (brassMat != null) brassStem.GetComponent<Renderer>().sharedMaterial = brassMat;
            UnityEngine.Object.DestroyImmediate(brassStem.GetComponent<Collider>());

            // Wax Candle
            GameObject waxCandle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            waxCandle.name = "Wax_Candle";
            waxCandle.transform.parent = candleRoot.transform;
            waxCandle.transform.localPosition = new Vector3(0, 0.2f, 0);
            waxCandle.transform.localScale = new Vector3(0.04f, 0.15f, 0.04f);
            if (waxMat != null) waxCandle.GetComponent<Renderer>().sharedMaterial = waxMat;
            UnityEngine.Object.DestroyImmediate(waxCandle.GetComponent<Collider>());

            // Flame
            GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            flame.name = "Flame_Optional";
            flame.transform.parent = candleRoot.transform;
            flame.transform.localPosition = new Vector3(0, 0.38f, 0);
            flame.transform.localScale = new Vector3(0.028f, 0.04f, 0.028f);
            Material flameMat = GetOrCreateEmissiveMaterial("M_Candle_Flame", new Color(1f, 0.75f, 0.35f), new Color(1f, 0.65f, 0.2f) * 3f);
            if (flameMat != null) flame.GetComponent<Renderer>().sharedMaterial = flameMat;
            UnityEngine.Object.DestroyImmediate(flame.GetComponent<Collider>());

            // Flickering Warm Amber Light (0xffa850)
            GameObject flameLightObj = new GameObject("FlamePointLight");
            flameLightObj.transform.parent = candleRoot.transform;
            flameLightObj.transform.localPosition = new Vector3(0, 0.40f, 0);
            Light fLt = flameLightObj.AddComponent<Light>();
            fLt.type = LightType.Point;
            fLt.color = new Color(1.0f, 0.66f, 0.31f);
            fLt.intensity = 1.6f;
            fLt.range = 5.5f;

            // Attach SeatedApparition Controller
            SeatedApparition sa = apparitionRoot.AddComponent<SeatedApparition>();
            sa.SetupComponents(head.transform, flame.transform, fLt);
        }

        private static void CreateLimb(Transform parent, Vector3 a, Vector3 b, float thickness, Material mat, string name)
        {
            Vector3 mid = (a + b) * 0.5f;
            Vector3 dir = (b - a);
            float length = dir.magnitude;

            GameObject limbObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            limbObj.name = name;
            limbObj.transform.parent = parent;
            limbObj.transform.localPosition = mid;
            if (dir.sqrMagnitude > 0.001f)
            {
                limbObj.transform.up = dir.normalized;
            }
            limbObj.transform.localScale = new Vector3(thickness, length * 0.5f, thickness);
            if (mat != null) limbObj.GetComponent<Renderer>().sharedMaterial = mat;
            UnityEngine.Object.DestroyImmediate(limbObj.GetComponent<Collider>());
        }

        private static Material GetOrCreateMaterial(string name, Color color, float smoothness)
        {
            string path = $"{MatDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard")) { color = color };
                mat.SetFloat("_Glossiness", smoothness);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static Material GetOrCreateEmissiveMaterial(string name, Color color, Color emission)
        {
            string path = $"{MatDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard")) { color = color };
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        #endregion

        private static void CreateBlastDoorSurround(Transform parent, Vector3 pos, Material metalMat, Material hazardMat)
        {
            // Door surround
        }

        private static void UpgradeMainMenuScene()
        {
            var menuScene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Additive);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.006f, 0.007f, 0.010f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.005f, 0.006f, 0.008f);
            RenderSettings.fogDensity = 0.05f;
            EditorSceneManager.SaveScene(menuScene);
            EditorSceneManager.CloseScene(menuScene, true);
        }

        #endregion
    }
}
#endif
