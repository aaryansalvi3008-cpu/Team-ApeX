using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeTheLab.Systems
{
    /// <summary>
    /// Noise event data emitted by player movements, interactions, and distractions.
    /// Follows the sound propagation rules specified in the GDD:
    /// - Crouch-walk: 2m
    /// - Walk: 6m
    /// - Run (Sprint): 14m
    /// - Jump/Drop: 10m
    /// - Metal grating floor: +3m
    /// - Carpet/Office: -2m
    /// - Walls and closed blast doors attenuate radius by 50%.
    /// </summary>
    public struct NoiseEvent
    {
        public Vector3 Position;
        public float Radius;
        public float Loudness; // 0.0 to 1.0
        public string SourceName;
        public float Timestamp;

        public NoiseEvent(Vector3 position, float radius, float loudness, string sourceName)
        {
            Position = position;
            Radius = radius;
            Loudness = Mathf.Clamp01(loudness);
            SourceName = sourceName;
            Timestamp = Time.time;
        }
    }

    /// <summary>
    /// Central manager for sound propagation and AI acoustic perception.
    /// Handles wall occlusion raycasting and informs listening entities (The Subject).
    /// </summary>
    public class NoiseManager : MonoBehaviour
    {
        public static NoiseManager Instance { get; private set; }

        [Header("Occlusion Settings")]
        [Tooltip("Layer mask representing solid walls and doors that attenuate sound.")]
        [SerializeField] private LayerMask occlusionMask = ~0;

        [Tooltip("Attenuation multiplier when a solid wall or closed door is between the sound and listener (GDD: 50%).")]
        [SerializeField] private float wallAttenuationMultiplier = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool showDebugVisualizers = false;

        // Events
        public event Action<NoiseEvent> OnNoiseEmitted;

        // Current real-time noise radius produced by the player (used by HUD Noise Meter)
        public float CurrentPlayerNoiseRadius { get; private set; }
        private float noiseDecayTimer = 0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // Smoothly decay current player noise radius back towards zero
            if (CurrentPlayerNoiseRadius > 0f)
            {
                noiseDecayTimer += Time.deltaTime;
                if (noiseDecayTimer > 0.15f)
                {
                    CurrentPlayerNoiseRadius = Mathf.Lerp(CurrentPlayerNoiseRadius, 0f, Time.deltaTime * 6f);
                    if (CurrentPlayerNoiseRadius < 0.2f)
                    {
                        CurrentPlayerNoiseRadius = 0f;
                    }
                }
            }
        }

        /// <summary>
        /// Emits a noise event into the world from a given position.
        /// </summary>
        public void EmitNoise(Vector3 position, float baseRadius, float loudness = 1.0f, string sourceName = "Unknown", bool isFromPlayer = false)
        {
            NoiseEvent evt = new NoiseEvent(position, baseRadius, loudness, sourceName);

            if (isFromPlayer)
            {
                CurrentPlayerNoiseRadius = Mathf.Max(CurrentPlayerNoiseRadius, baseRadius);
                noiseDecayTimer = 0f;
            }

            OnNoiseEmitted?.Invoke(evt);
        }

        /// <summary>
        /// Evaluates whether a listener at a specific position can hear a noise event,
        /// accounting for 3D distance and 50% wall attenuation if obstructed.
        /// </summary>
        public bool CanHearNoise(NoiseEvent evt, Vector3 listenerPosition, out float effectiveRadius, out float perceivedDistance)
        {
            perceivedDistance = Vector3.Distance(evt.Position, listenerPosition);
            effectiveRadius = evt.Radius;

            // Check line-of-sight raycast between noise position and listener
            Vector3 dir = listenerPosition - evt.Position;
            if (Physics.Raycast(evt.Position, dir.normalized, perceivedDistance, occlusionMask, QueryTriggerInteraction.Ignore))
            {
                // Wall or solid obstacle detected: attenuate effective radius by 50%
                effectiveRadius *= wallAttenuationMultiplier;
            }

            return perceivedDistance <= effectiveRadius;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugVisualizers) return;
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, CurrentPlayerNoiseRadius);
        }
    }
}
