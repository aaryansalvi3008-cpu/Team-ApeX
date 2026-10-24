using UnityEngine;
using EscapeTheLab.UI;

namespace EscapeTheLab.Interaction
{
    /// <summary>
    /// Interactive collectible security keycard.
    /// Emits a subtle cyan glow, gently bobs and rotates on its pedestal,
    /// and grants the player the keycard when interacted with.
    /// </summary>
    public class KeycardPickup : MonoBehaviour, IInteractable
    {
        [Header("Keycard Data")]
        [SerializeField] private string keycardName = "Security Keycard";

        [Header("Hover Animation")]
        [SerializeField] private bool enableHover = true;
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private float bobAmplitude = 0.04f;
        [SerializeField] private float bobFrequency = 2.5f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pickupClip;

        private Vector3 startPosition;
        private bool isCollected = false;

        private void Start()
        {
            startPosition = transform.position;
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void Update()
        {
            if (isCollected || !enableHover) return;

            // Gentle rotation and vertical hover
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            float newY = startPosition.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }

        public string GetPromptText()
        {
            return $"[E] Pick up {keycardName}";
        }

        public bool CanInteract()
        {
            return !isCollected;
        }

        public void Interact(PlayerInteraction player)
        {
            if (isCollected) return;

            isCollected = true;
            player.HasKeycard = true;

            // Notify HUD
            if (HUDController.Instance != null)
            {
                HUDController.Instance.ShowNotification($"{keycardName} Acquired! Airlock access granted.", new Color(0.2f, 0.9f, 1f));
                HUDController.Instance.SetObjective("Head to the Main Exit Airlock and Escape");
                HUDController.Instance.SetPrompt(null);
            }

            // Audio feedback
            if (audioSource != null && pickupClip != null)
            {
                audioSource.PlayOneShot(pickupClip);
            }
            else
            {
                EscapeTheLab.Audio.HorrorAudioManager.Instance?.PlayKeycardChime();
            }

            // Hide mesh and collider immediately
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) r.enabled = false;

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Destroy object after sound finishes or immediately
            Destroy(gameObject, 0.5f);
        }
    }
}
