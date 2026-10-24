#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EscapeTheLab.Interaction;
using EscapeTheLab.UI;
using EscapeTheLab.Core;
using EscapeTheLab.Player;

namespace EscapeTheLab.Editor
{
    /// <summary>
    /// Configures Stage 4 interactive elements in LaboratoryLevel.unity:
    /// - Collectible Security Keycard in Research Lab with floating animation.
    /// - Locked Airlock Blast Door requiring the Keycard to open.
    /// - Scanner terminal with Red -> Green LED transition upon unlock.
    /// - Interactive standard door between Security Office and Corridor.
    /// - Escape victory trigger behind the Airlock.
    /// - HUD Manager with Crosshair dot, Interaction prompt, Objective tracker, and Victory screen.
    /// </summary>
    public static class Stage4ObjectiveBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/LaboratoryLevel.unity";

        [MenuItem("Tools/Escape The Lab/Setup Stage 4 Objectives and Rebuild")]
        public static void SetupStage4MenuItem()
        {
            SetupStage4();
            EditorUtility.DisplayDialog(
                "Stage 4 Objectives Configured",
                "Stage 4 interaction system and objectives have been set up!\n\nRebuilding standalone executable...",
                "OK"
            );
            GameFixerAndBuilder.FixAndRebuild();
        }

        public static void SetupStage4Batchmode()
        {
            Debug.Log("[Stage4ObjectiveBuilder] Configuring Stage 4 interaction elements...");
            SetupStage4();
            Debug.Log("[Stage4ObjectiveBuilder] Stage 4 setup complete. Rebuilding standalone player...");
            GameFixerAndBuilder.FixAndRebuild();
        }

        public static void SetupStage4()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Load Materials
            Material cyanEmissive = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_Lab_Emissive_Cyan.mat");
            Material redEmissive = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_Lab_Emissive_Red.mat");
            Material greenEmissive = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_Lab_Emissive_Green.mat");
            Material metalMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_Lab_Metal.mat");

            // 1. Setup HUD Manager
            SetupHUD();

            // 2. Setup Player Interaction Component
            SetupPlayer();

            // 3. Create Keycard in Research Lab
            SetupKeycard(cyanEmissive);

            // 4. Setup Interactive Security Office Door
            SetupOfficeDoor(metalMat);

            // 5. Setup Locked Airlock Blast Door
            SetupAirlockDoor(redEmissive, greenEmissive);

            // 6. Setup Exit Escape Trigger
            SetupExitTrigger();

            // Save Scene
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void SetupHUD()
        {
            GameObject existingHUD = GameObject.Find("HUD_Manager");
            if (existingHUD == null)
            {
                existingHUD = new GameObject("HUD_Manager");
            }

            if (existingHUD.GetComponent<HUDController>() == null)
            {
                existingHUD.AddComponent<HUDController>();
            }
        }

        private static void SetupPlayer()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }
            if (player != null)
            {
                Camera cam = player.GetComponentInChildren<Camera>();
                PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
                if (interaction == null)
                {
                    interaction = player.AddComponent<PlayerInteraction>();
                }
                if (cam != null)
                {
                    interaction.SetCamera(cam);
                }
            }
        }

        private static void SetupKeycard(Material cyanMat)
        {
            GameObject existing = GameObject.Find("Security_Keycard");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            GameObject keycard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keycard.name = "Security_Keycard";
            // Placed atop the pedestal desk in Research Lab
            keycard.transform.position = new Vector3(5f, 1.15f, 32f);
            keycard.transform.localScale = new Vector3(0.4f, 0.04f, 0.25f);
            keycard.GetComponent<Renderer>().sharedMaterial = cyanMat;

            AudioSource aSrc = keycard.AddComponent<AudioSource>();
            aSrc.playOnAwake = false;
            aSrc.spatialBlend = 0f;

            keycard.AddComponent<KeycardPickup>();
        }

        private static void SetupOfficeDoor(Material metalMat)
        {
            GameObject existing = GameObject.Find("Door_SecurityOffice");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            // Door positioned in the doorway between Security Office and West Corridor
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door_SecurityOffice";
            door.transform.position = new Vector3(-7.5f, 1.6f, -5f);
            door.transform.localScale = new Vector3(0.2f, 3.2f, 1.8f);
            door.GetComponent<Renderer>().sharedMaterial = metalMat;

            DoorController doorCtrl = door.AddComponent<DoorController>();
            doorCtrl.SetupDoor(false, false);
        }

        private static void SetupAirlockDoor(Material redMat, Material greenMat)
        {
            GameObject blastDoor = GameObject.Find("Exit_BlastDoor_Locked");
            if (blastDoor != null)
            {
                DoorController doorCtrl = blastDoor.GetComponent<DoorController>();
                if (doorCtrl == null)
                {
                    doorCtrl = blastDoor.AddComponent<DoorController>();
                }

                GameObject scanner = GameObject.Find("Keycard_Terminal_Scanner");
                Renderer scannerRend = scanner != null ? scanner.GetComponent<Renderer>() : null;

                doorCtrl.SetupDoor(true, true, scannerRend, redMat, greenMat);
            }
        }

        private static void SetupExitTrigger()
        {
            GameObject existing = GameObject.Find("Exit_Escape_Trigger");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            GameObject trigger = new GameObject("Exit_Escape_Trigger");
            trigger.transform.position = new Vector3(15f, 1.5f, -25.5f);
            BoxCollider col = trigger.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(5f, 3.5f, 3f);

            trigger.AddComponent<ExitTrigger>();
        }
    }
}
#endif
