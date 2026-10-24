using UnityEngine;
using EscapeTheLab.UI;
using EscapeTheLab.Player;

namespace EscapeTheLab.Core
{
    /// <summary>
    /// Placed beyond the unlocked Exit Airlock door.
    /// When the player steps into the trigger area, initiates the escape victory sequence.
    /// Includes PhysX trigger detection, kinematic rigidbody attachment, and distance proximity failsafes.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ExitTrigger : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip victorySound;

        [Header("Level Progression")]
        [SerializeField] private string nextSceneName = "LaboratoryLevel2";
        [SerializeField] private bool isFinalLevel = false;
        [SerializeField] private float triggerRadius = 3.5f;

        private bool hasTriggered = false;
        private Transform playerTransform;

        public void Configure(string nextScene, bool isFinal)
        {
            nextSceneName = nextScene;
            isFinalLevel = isFinal;
        }

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            // In Unity PhysX, a trigger MUST have a Rigidbody to reliably detect CharacterControllers
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        private void Start()
        {
            FindPlayer();
        }

        private void Update()
        {
            if (hasTriggered) return;

            if (playerTransform == null)
            {
                FindPlayer();
            }

            if (playerTransform != null)
            {
                Collider col = GetComponent<Collider>();
                bool inside = col != null && col.bounds.Contains(playerTransform.position);
                float dist = Vector3.Distance(transform.position, playerTransform.position);

                if (inside || dist <= triggerRadius)
                {
                    TriggerVictory();
                }
            }
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered) return;

            if (other.CompareTag("Player") || other.GetComponent<FirstPersonMovement>() != null || other.GetComponent<CharacterController>() != null)
            {
                TriggerVictory();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (hasTriggered) return;

            if (other.CompareTag("Player") || other.GetComponent<FirstPersonMovement>() != null || other.GetComponent<CharacterController>() != null)
            {
                TriggerVictory();
            }
        }

        public void TriggerVictory()
        {
            if (hasTriggered) return;
            hasTriggered = true;

            Debug.Log($"[ExitTrigger] Player reached escape trigger! isFinalLevel={isFinalLevel}, nextScene={nextSceneName}");

            // Unlock Level 2 when completing Level 1
            if (!isFinalLevel || nextSceneName == "LaboratoryLevel2")
            {
                PlayerPrefs.SetInt("ProjectVANTA_Level2Unlocked", 1);
                PlayerPrefs.Save();
                Debug.Log("[ExitTrigger] Clearance Level 2 UNLOCKED in persistent PlayerPrefs!");
            }
            else
            {
                PlayerPrefs.SetInt("ProjectVANTA_GameCompleted", 1);
                PlayerPrefs.Save();
                Debug.Log("[ExitTrigger] Ultimate Victory achieved: Project VANTA fully survived!");
            }

            if (HUDController.Instance != null)
            {
                HUDController.Instance.ShowVictory(nextSceneName, isFinalLevel);
            }

            if (audioSource != null && victorySound != null)
            {
                audioSource.PlayOneShot(victorySound);
            }
            else
            {
                Audio.HorrorAudioManager.Instance?.PlayKeycardChime();
            }
        }
    }
}
