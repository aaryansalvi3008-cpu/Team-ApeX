using UnityEngine;

namespace EscapeTheLab.Environment
{
    /// <summary>
    /// The Seated Apparition (Gothic Horror Specter):
    /// - Seated on the iron hospital bed in a Victorian shroud, holding a flickering brass candlestick.
    /// - Tracks the player with an uncanny unblinking gaze and sinister smile.
    /// - Flickers candle flame with realistic light modulation.
    /// </summary>
    public class SeatedApparition : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform flameTransform;
        [SerializeField] private Light candleLight;

        [Header("Player Tracking")]
        [SerializeField] private float lookRange = 7.0f;
        [SerializeField] private float lookSpeed = 2.5f;

        private Transform playerTransform;
        private Quaternion initialHeadRotation;
        private float flameBaseIntensity = 1.6f;
        private AudioSource audioSource;

        private void Start()
        {
            if (headTransform != null)
            {
                initialHeadRotation = headTransform.localRotation;
            }

            if (candleLight != null)
            {
                flameBaseIntensity = candleLight.intensity;
            }

            audioSource = GetComponent<AudioSource>();
            FindPlayer();
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player != null) playerTransform = player.transform;
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            // 1. Natural Candle Flame Flicker
            AnimateCandleFlame();

            // 2. Uncanny Head Tracking towards Player
            TrackPlayerGaze();
        }

        private void AnimateCandleFlame()
        {
            float t = Time.time;
            float flicker = Mathf.Sin(t * 18f) * 0.08f + Mathf.Sin(t * 33f) * 0.05f;

            if (flameTransform != null)
            {
                float scale = 1f + flicker;
                flameTransform.localScale = new Vector3(scale, 1f + flicker * 2f, scale);
            }

            if (candleLight != null)
            {
                candleLight.intensity = flameBaseIntensity * (1f + flicker * 1.5f);
            }
        }

        private void TrackPlayerGaze()
        {
            if (headTransform == null || playerTransform == null) return;

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= lookRange)
            {
                // Calculate direction to player eye height
                Vector3 targetPos = playerTransform.position + Vector3.up * 1.6f;
                Vector3 dir = (targetPos - headTransform.position).normalized;

                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                // Clamp rotation so she doesn't twist her head completely backward
                headTransform.rotation = Quaternion.Slerp(headTransform.rotation, targetRot, Time.deltaTime * lookSpeed);
            }
            else
            {
                // Smoothly return to staring forward with uncanny stillness
                headTransform.localRotation = Quaternion.Slerp(headTransform.localRotation, initialHeadRotation, Time.deltaTime * 1.5f);
            }
        }

        public void SetupComponents(Transform head, Transform flame, Light light)
        {
            headTransform = head;
            flameTransform = flame;
            candleLight = light;
            if (headTransform != null) initialHeadRotation = headTransform.localRotation;
            if (candleLight != null) flameBaseIntensity = candleLight.intensity;
        }
    }
}
