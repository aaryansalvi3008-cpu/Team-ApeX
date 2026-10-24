using UnityEngine;
using EscapeTheLab.Systems;
using EscapeTheLab.Player;
using EscapeTheLab.Audio;

namespace EscapeTheLab.Environment
{
    /// <summary>
    /// Broken Glass / Hazard Grate Floor Section:
    /// - Walking or sprinting over it triggers a loud glass crunch / metal clang.
    /// - Emits an 11m acoustic noise event to NoiseManager, alerting The Subject!
    /// - Crouching (Left Ctrl / C) allows the player to step gingerly and pass completely silently.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FloorHazard : MonoBehaviour
    {
        [Header("Hazard Settings")]
        [SerializeField] private float stepNoiseRadius = 11.0f;
        [SerializeField] private float noiseCooldown = 0.8f;

        private AudioSource audioSource;
        private AudioClip crunchClip;
        private float lastTriggerTime = -999f;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.minDistance = 2f;
            audioSource.maxDistance = 22f;

            crunchClip = ProceduralAudioClips.CreateGlassCrunch("GlassCrunch");
        }

        private void OnTriggerStay(Collider other)
        {
            if (Time.time - lastTriggerTime < noiseCooldown) return;

            FirstPersonMovement movement = other.GetComponent<FirstPersonMovement>();
            if (movement == null) movement = other.GetComponentInParent<FirstPersonMovement>();

            if (movement != null && movement.IsMoving)
            {
                // If player is crouching, they pass silently!
                if (movement.IsCrouching) return;

                lastTriggerTime = Time.time;

                if (audioSource != null && crunchClip != null)
                {
                    audioSource.pitch = Random.Range(0.85f, 1.15f);
                    audioSource.PlayOneShot(crunchClip, 0.9f);
                }

                if (NoiseManager.Instance != null)
                {
                    NoiseManager.Instance.EmitNoise(transform.position, stepNoiseRadius, 1.0f, "Broken_Glass_Floor");
                }
            }
        }
    }
}
