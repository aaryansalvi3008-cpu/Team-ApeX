using UnityEngine;
using EscapeTheLab.UI;
using EscapeTheLab.Player;

namespace EscapeTheLab.Core
{
    /// <summary>
    /// Placed beyond the unlocked Exit Airlock door.
    /// When the player steps into the trigger area, initiates the escape victory sequence.
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

        private bool hasTriggered = false;

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
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered) return;

            if (other.CompareTag("Player") || other.GetComponent<FirstPersonMovement>() != null)
            {
                hasTriggered = true;
                Debug.Log("[ExitTrigger] Player successfully reached the escape airlock!");

                if (HUDController.Instance != null)
                {
                    HUDController.Instance.ShowVictory(nextSceneName, isFinalLevel);
                }

                if (audioSource != null && victorySound != null)
                {
                    audioSource.PlayOneShot(victorySound);
                }
            }
        }
    }
}
