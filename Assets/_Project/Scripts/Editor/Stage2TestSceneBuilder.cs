#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using EscapeTheLab.Player;

namespace EscapeTheLab.Editor
{
    /// <summary>
    /// Automates the creation and setup of the Stage 2 Test Scene:
    /// Constructs a dark enclosed test laboratory chamber, materials, and a configured
    /// First-Person Player character controller with mouse look and flashlight.
    /// </summary>
    public static class Stage2TestSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Stage2_TestScene.unity";
        private const string MaterialsDir = "Assets/_Project/Materials";

        [MenuItem("Tools/Escape The Lab/Build Stage 2 Test Scene")]
        public static void BuildSceneMenuItem()
        {
            BuildScene();
            EditorUtility.DisplayDialog(
                "Stage 2 Test Scene Created",
                "The Stage 2 test scene has been created and saved at:\n" + ScenePath + "\n\nPress Play in Unity to test WASD movement, mouse look, and flashlight toggle (F)!",
                "OK"
            );
        }

        public static void BuildSceneBatchmode()
        {
            Debug.Log("[Stage2TestSceneBuilder] Starting batchmode test scene build...");
            BuildScene();
            Debug.Log("[Stage2TestSceneBuilder] Scene built and saved successfully at: " + ScenePath);
        }

        public static void BuildScene()
        {
            // Create a brand new empty scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }
            if (!AssetDatabase.IsValidFolder(MaterialsDir))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Materials");
            }

            // Create placeholder materials
            Material floorMat = GetOrCreateMaterial("M_Lab_Floor", new Color(0.12f, 0.13f, 0.14f), 0.3f);
            Material wallMat = GetOrCreateMaterial("M_Lab_Wall", new Color(0.20f, 0.22f, 0.24f), 0.1f);
            Material pillarMat = GetOrCreateMaterial("M_Lab_Pillar", new Color(0.16f, 0.18f, 0.20f), 0.2f);
            Material crateMat = GetOrCreateMaterial("M_Lab_Crate", new Color(0.35f, 0.25f, 0.15f), 0.05f);

            // Configure Environment Lighting (Spooky horror atmosphere)
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.015f, 0.015f, 0.025f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.008f, 0.008f, 0.012f);
            RenderSettings.fogDensity = 0.04f;

            // Root Environment Hierarchy
            GameObject envRoot = new GameObject("Environment");

            // Chamber Dimensions
            float roomWidth = 24f;
            float roomLength = 24f;
            float roomHeight = 4.5f;

            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.parent = envRoot.transform;
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(roomWidth, 1f, roomLength);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // Ceiling (prevents external sky illumination)
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.parent = envRoot.transform;
            ceiling.transform.position = new Vector3(0, roomHeight + 0.5f, 0);
            ceiling.transform.localScale = new Vector3(roomWidth, 1f, roomLength);
            ceiling.GetComponent<Renderer>().sharedMaterial = wallMat;

            // 4 Perimeter Walls
            CreateWall(envRoot.transform, "Wall_North", new Vector3(0, roomHeight * 0.5f, roomLength * 0.5f + 0.5f), new Vector3(roomWidth, roomHeight, 1f), wallMat);
            CreateWall(envRoot.transform, "Wall_South", new Vector3(0, roomHeight * 0.5f, -roomLength * 0.5f - 0.5f), new Vector3(roomWidth, roomHeight, 1f), wallMat);
            CreateWall(envRoot.transform, "Wall_East", new Vector3(roomWidth * 0.5f + 0.5f, roomHeight * 0.5f, 0), new Vector3(1f, roomHeight, roomLength), wallMat);
            CreateWall(envRoot.transform, "Wall_West", new Vector3(-roomWidth * 0.5f - 0.5f, roomHeight * 0.5f, 0), new Vector3(1f, roomHeight, roomLength), wallMat);

            // Obstacle Pillars (to test collision, navigation, and flashlight shadows)
            CreatePillar(envRoot.transform, "Pillar_NE", new Vector3(5f, roomHeight * 0.5f, 5f), new Vector3(1.6f, roomHeight, 1.6f), pillarMat);
            CreatePillar(envRoot.transform, "Pillar_NW", new Vector3(-5f, roomHeight * 0.5f, 5f), new Vector3(1.6f, roomHeight, 1.6f), pillarMat);
            CreatePillar(envRoot.transform, "Pillar_SE", new Vector3(5f, roomHeight * 0.5f, -5f), new Vector3(1.6f, roomHeight, 1.6f), pillarMat);
            CreatePillar(envRoot.transform, "Pillar_SW", new Vector3(-5f, roomHeight * 0.5f, -5f), new Vector3(1.6f, roomHeight, 1.6f), pillarMat);

            // Lab Crates
            CreateCrate(envRoot.transform, "Crate_01", new Vector3(0, 0.6f, 6f), new Vector3(1.2f, 1.2f, 1.2f), crateMat);
            CreateCrate(envRoot.transform, "Crate_02", new Vector3(1.3f, 0.5f, 6.2f), new Vector3(1f, 1f, 1f), crateMat);
            CreateCrate(envRoot.transform, "Crate_03", new Vector3(-6f, 0.5f, -1f), new Vector3(1f, 1f, 1f), crateMat);

            // Dim Emergency Ceiling Light (cold greenish tint)
            GameObject emergencyLightObj = new GameObject("CeilingEmergencyLight");
            emergencyLightObj.transform.parent = envRoot.transform;
            emergencyLightObj.transform.position = new Vector3(0, roomHeight - 0.2f, 0);
            Light emergencyLight = emergencyLightObj.AddComponent<Light>();
            emergencyLight.type = LightType.Point;
            emergencyLight.range = 14f;
            emergencyLight.intensity = 0.6f;
            emergencyLight.color = new Color(0.4f, 0.7f, 0.6f);
            emergencyLight.shadows = LightShadows.Soft;

            // Player Setup
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0, 0f, -7f);

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
            cam.farClipPlane = 80f;
            cam.fieldOfView = 65f;

            cameraObj.AddComponent<AudioListener>();

            MouseLook mouseLook = cameraObj.AddComponent<MouseLook>();
            mouseLook.SetPlayerBody(player.transform);
            movement.SetCameraTransform(cameraObj.transform);

            // Flashlight
            GameObject flashlightObj = new GameObject("Flashlight");
            flashlightObj.transform.parent = cameraObj.transform;
            // Positioned slightly down and right from player's eyes
            flashlightObj.transform.localPosition = new Vector3(0.25f, -0.22f, 0.35f);
            flashlightObj.transform.localRotation = Quaternion.identity;

            Light flashLight = flashlightObj.AddComponent<Light>();
            flashLight.type = LightType.Spot;
            flashLight.range = 24f;
            flashLight.spotAngle = 48f;
            flashLight.innerSpotAngle = 32f;
            flashLight.intensity = 2.8f;
            flashLight.color = new Color(1.0f, 0.95f, 0.85f); // Realistic warm halogen/incandescent tint
            flashLight.shadows = LightShadows.Soft;

            AudioSource flashAudio = flashlightObj.AddComponent<AudioSource>();
            flashAudio.playOnAwake = false;
            flashAudio.spatialBlend = 0f; // 2D click sound

            FlashlightController flashController = flashlightObj.AddComponent<FlashlightController>();
            flashController.SetupReferences(flashLight, cameraObj.transform, flashAudio);

            // Save Scene
            EditorSceneManager.SaveScene(newScene, ScenePath);

            // Register in Build Settings
            AddSceneToBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Material GetOrCreateMaterial(string matName, Color color, float smoothness)
        {
            string path = $"{MaterialsDir}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader standardShader = Shader.Find("Standard");
                if (standardShader == null)
                {
                    standardShader = Shader.Find("Universal Render Pipeline/Lit");
                }
                if (standardShader == null)
                {
                    standardShader = Shader.Find("Diffuse");
                }

                mat = new Material(standardShader);
                mat.color = color;
                if (mat.HasProperty("_Glossiness"))
                {
                    mat.SetFloat("_Glossiness", smoothness);
                }
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static void CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.parent = parent;
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreatePillar(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = name;
            pillar.transform.parent = parent;
            pillar.transform.position = pos;
            pillar.transform.localScale = scale;
            pillar.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateCrate(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = name;
            crate.transform.parent = parent;
            crate.transform.position = pos;
            crate.transform.localScale = scale;
            crate.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] originalScenes = EditorBuildSettings.scenes;
            foreach (var s in originalScenes)
            {
                if (s.path == scenePath) return;
            }

            EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[originalScenes.Length + 1];
            System.Array.Copy(originalScenes, newScenes, originalScenes.Length);
            newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
#endif
