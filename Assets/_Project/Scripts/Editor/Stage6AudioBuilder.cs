#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EscapeTheLab.Audio;
using EscapeTheLab.Player;

namespace EscapeTheLab.Editor
{
    /// <summary>
    /// Configures Stage 6 Horror Atmosphere & Audio Systems in LaboratoryLevel.unity:
    /// - HorrorAudioManager (procedural facility drone, dynamic heartbeat, footsteps, SFX).
    /// - CameraTension (fear vibration and camera shake when the monster is near/chasing).
    /// - Rebuilds standalone game executable.
    /// </summary>
    public static class Stage6AudioBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/LaboratoryLevel.unity";

        [MenuItem("Tools/Escape The Lab/Setup Stage 6 Audio and Rebuild")]
        public static void SetupStage6MenuItem()
        {
            SetupStage6();
            EditorUtility.DisplayDialog(
                "Stage 6 Audio Configured",
                "Stage 6 Horror Atmosphere and Audio systems have been configured!\n\nRebuilding standalone executable...",
                "OK"
            );
            GameFixerAndBuilder.FixAndRebuild();
        }

        public static void SetupStage6Batchmode()
        {
            Debug.Log("[Stage6AudioBuilder] Configuring Stage 6 Audio and Horror Atmosphere...");
            SetupStage6();
            Debug.Log("[Stage6AudioBuilder] Stage 6 setup complete. Rebuilding standalone player...");
            GameFixerAndBuilder.FixAndRebuild();
        }

        public static void SetupStage6()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // 1. Setup Audio Manager
            GameObject existingAudio = GameObject.Find("Audio_Manager");
            if (existingAudio == null)
            {
                existingAudio = new GameObject("Audio_Manager");
            }
            if (existingAudio.GetComponent<HorrorAudioManager>() == null)
            {
                existingAudio.AddComponent<HorrorAudioManager>();
            }

            // 2. Setup CameraTension on Player Camera
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }
            if (player != null)
            {
                Camera cam = player.GetComponentInChildren<Camera>();
                if (cam != null && cam.GetComponent<CameraTension>() == null)
                {
                    cam.gameObject.AddComponent<CameraTension>();
                }
            }

            // Save Scene
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
