#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;
using EscapeTheLab.AI;

namespace EscapeTheLab.Editor
{
    /// <summary>
    /// Configures Stage 5 Monster AI in LaboratoryLevel.unity:
    /// - Bakes Unity NavMesh across the entire laboratory.
    /// - Places 9 strategic patrol waypoints covering all rooms and corridors.
    /// - Constructs the Monster rig with dark silhouette body and piercing glowing red eyes.
    /// - Configures NavMeshAgent and MonsterController state machine.
    /// - Rebuilds standalone game executable.
    /// </summary>
    public static class Stage5MonsterBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/LaboratoryLevel.unity";
        private const string MatDir = "Assets/_Project/Materials";

        [MenuItem("Tools/Escape The Lab/Setup Stage 5 Monster and Rebuild")]
        public static void SetupStage5MenuItem()
        {
            SetupStage5();
            EditorUtility.DisplayDialog(
                "Stage 5 Monster Configured",
                "Stage 5 Monster AI and NavMesh have been successfully configured!\n\nRebuilding standalone executable...",
                "OK"
            );
            GameFixerAndBuilder.FixAndRebuild();
        }

        public static void SetupStage5Batchmode()
        {
            Debug.Log("[Stage5MonsterBuilder] Configuring Stage 5 Monster AI and NavMesh...");
            SetupStage5();
            Debug.Log("[Stage5MonsterBuilder] Stage 5 setup complete. Rebuilding standalone player...");
            GameFixerAndBuilder.FixAndRebuild();
        }

        public static void SetupStage5()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // 1. Create Monster Materials
            Material skinMat = GetOrCreateMaterial("M_Monster_Skin", new Color(0.08f, 0.08f, 0.10f), 0.2f);
            Material eyesMat = GetOrCreateEmissiveMaterial("M_Monster_Eyes", new Color(0.2f, 0f, 0f), new Color(1f, 0.05f, 0.1f) * 3.5f);

            // 2. Bake NavMesh
            Debug.Log("[Stage5MonsterBuilder] Baking NavMesh for laboratory layout...");
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            Debug.Log("[Stage5MonsterBuilder] NavMesh bake completed.");

            // 3. Setup Patrol Waypoints
            List<Transform> waypoints = SetupWaypoints();

            // 4. Construct Monster GameObject
            SetupMonster(skinMat, eyesMat, waypoints);

            // Save Scene
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static List<Transform> SetupWaypoints()
        {
            GameObject existingWPs = GameObject.Find("Patrol_Waypoints");
            if (existingWPs != null)
            {
                Object.DestroyImmediate(existingWPs);
            }

            GameObject root = new GameObject("Patrol_Waypoints");

            Vector3[] wpPositions = new Vector3[] {
                new Vector3(23f, 0.1f, 12f),  // WP0: Containment Chamber (Spawn)
                new Vector3(15f, 0.1f, 12f),  // WP1: East Corridor - Chamber Entrance
                new Vector3(15f, 0.1f, 2f),   // WP2: East Corridor - Mid
                new Vector3(5f, 0.1f, 8f),    // WP3: Central Hallway - Debris
                new Vector3(-5f, 0.1f, 8f),   // WP4: West Corridor - Junction
                new Vector3(-5f, 0.1f, 20f),  // WP5: West Corridor - Lab Entrance
                new Vector3(5f, 0.1f, 26f),   // WP6: Research Lab - Center
                new Vector3(15f, 0.1f, 20f),  // WP7: East Corridor - Lab Entrance
                new Vector3(15f, 0.1f, -8f)   // WP8: East Corridor - Airlock Approach
            };

            List<Transform> list = new List<Transform>();
            for (int i = 0; i < wpPositions.Length; i++)
            {
                GameObject wp = new GameObject($"Waypoint_{i}");
                wp.transform.parent = root.transform;
                wp.transform.position = wpPositions[i];
                list.Add(wp.transform);
            }

            return list;
        }

        private static void SetupMonster(Material skinMat, Material eyesMat, List<Transform> waypoints)
        {
            GameObject existingMonster = GameObject.Find("Monster_Specimen");
            if (existingMonster != null)
            {
                Object.DestroyImmediate(existingMonster);
            }

            // Root Monster (Spawns in containment chamber)
            GameObject monster = new GameObject("Monster_Specimen");
            monster.tag = "Respawn"; // Valid built-in tag
            monster.transform.position = new Vector3(23f, 0.05f, 12f);

            // Body Capsule (Tall, menacing silhouette)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Monster_Body";
            body.transform.parent = monster.transform;
            body.transform.localPosition = new Vector3(0, 1.1f, 0);
            body.transform.localScale = new Vector3(0.9f, 1.1f, 0.9f);
            body.GetComponent<Renderer>().sharedMaterial = skinMat;
            // Disable primitive collider so NavMeshAgent / controller handles collisions cleanly
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // Head Sphere
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Monster_Head";
            head.transform.parent = monster.transform;
            head.transform.localPosition = new Vector3(0, 2.05f, 0.1f);
            head.transform.localScale = new Vector3(0.65f, 0.7f, 0.65f);
            head.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(head.GetComponent<Collider>());

            // Left Glowing Eye
            GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeL.name = "Eye_Left";
            eyeL.transform.parent = head.transform;
            eyeL.transform.localPosition = new Vector3(-0.2f, 0.08f, 0.42f);
            eyeL.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
            eyeL.GetComponent<Renderer>().sharedMaterial = eyesMat;
            Object.DestroyImmediate(eyeL.GetComponent<Collider>());

            // Right Glowing Eye
            GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeR.name = "Eye_Right";
            eyeR.transform.parent = head.transform;
            eyeR.transform.localPosition = new Vector3(0.2f, 0.08f, 0.42f);
            eyeR.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
            eyeR.GetComponent<Renderer>().sharedMaterial = eyesMat;
            Object.DestroyImmediate(eyeR.GetComponent<Collider>());

            // Eerie Red Eye Glow Light
            GameObject eyeLightObj = new GameObject("EyeGlowLight");
            eyeLightObj.transform.parent = head.transform;
            eyeLightObj.transform.localPosition = new Vector3(0, 0.1f, 0.5f);
            Light eyeLight = eyeLightObj.AddComponent<Light>();
            eyeLight.type = LightType.Point;
            eyeLight.color = new Color(1.0f, 0.08f, 0.12f);
            eyeLight.intensity = 1.2f;
            eyeLight.range = 7f;
            eyeLight.shadows = LightShadows.Soft;

            // NavMeshAgent
            NavMeshAgent agent = monster.AddComponent<NavMeshAgent>();
            agent.speed = 2.4f;
            agent.angularSpeed = 240f;
            agent.acceleration = 14f;
            agent.stoppingDistance = 0.5f;
            agent.radius = 0.48f;
            agent.height = 2.2f;
            agent.baseOffset = 0f;

            // CapsuleCollider for player trigger detection
            CapsuleCollider col = monster.AddComponent<CapsuleCollider>();
            col.radius = 0.5f;
            col.height = 2.2f;
            col.center = new Vector3(0, 1.1f, 0);

            // 3D AudioSource
            AudioSource aSrc = monster.AddComponent<AudioSource>();
            aSrc.playOnAwake = false;
            aSrc.spatialBlend = 1.0f; // Full 3D
            aSrc.minDistance = 2f;
            aSrc.maxDistance = 22f;
            aSrc.rolloffMode = AudioRolloffMode.Linear;

            // MonsterController
            MonsterController ctrl = monster.AddComponent<MonsterController>();
            ctrl.SetupWaypoints(waypoints);
            ctrl.SetupEyeLight(eyeLight);
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
    }
}
#endif
