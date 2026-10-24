using System.Collections;
using UnityEngine;

namespace EscapeTheLab.Environment
{
    /// <summary>
    /// Simulates a realistic dying or glitching fluorescent tube light in an abandoned facility.
    /// Provides randomized micro-flickers, irregular buzz intervals, and occasional temporary blackouts.
    /// Optionally modulates an emissive material to synchronize fixture glow with the light beam.
    /// </summary>
    public class LightFlicker : MonoBehaviour
    {
        [Header("Target Components")]
        [Tooltip("The Light component to modulate.")]
        [SerializeField] private Light targetLight;

        [Tooltip("Optional fixture mesh renderer whose emission color will synchronize with flickering.")]
        [SerializeField] private Renderer fixtureRenderer;

        [Tooltip("Material index on fixtureRenderer to modulate emission for.")]
        [SerializeField] private int materialIndex = 0;

        [Header("Flicker Intensity Settings")]
        [Tooltip("Default steady light intensity.")]
        [SerializeField] private float baseIntensity = 1.0f;

        [Tooltip("Minimum intensity during a quick flicker dip.")]
        [SerializeField] private float minFlickerIntensity = 0.05f;

        [Tooltip("Maximum intensity during an electrical surge spike.")]
        [SerializeField] private float maxFlickerIntensity = 1.4f;

        [Header("Timing & Behavior")]
        [Tooltip("Average seconds between flicker burst episodes.")]
        [SerializeField] private float episodeIntervalMin = 1.5f;
        [SerializeField] private float episodeIntervalMax = 5.0f;

        [Tooltip("Number of rapid flashes per flicker episode.")]
        [SerializeField] private int minFlashesPerEpisode = 2;
        [SerializeField] private int maxFlashesPerEpisode = 6;

        [Tooltip("Chance that a flicker episode results in a brief total blackout.")]
        [Range(0f, 1f)]
        [SerializeField] private float blackoutProbability = 0.25f;

        [Tooltip("Duration of a total blackout in seconds.")]
        [SerializeField] private float blackoutDurationMin = 0.3f;
        [SerializeField] private float blackoutDurationMax = 1.2f;

        [Header("Audio (Optional)")]
        [Tooltip("AudioSource to play electrical buzzing/humming sound.")]
        [SerializeField] private AudioSource buzzAudio;

        private Material fixtureMaterial;
        private Color baseEmissionColor = Color.white;
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            if (targetLight == null)
            {
                targetLight = GetComponent<Light>();
            }

            if (buzzAudio == null)
            {
                buzzAudio = GetComponent<AudioSource>();
            }

            if (fixtureRenderer != null && fixtureRenderer.materials.Length > materialIndex)
            {
                fixtureMaterial = fixtureRenderer.materials[materialIndex];
                if (fixtureMaterial.HasProperty(EmissionColorId))
                {
                    baseEmissionColor = fixtureMaterial.GetColor(EmissionColorId);
                }
            }

            if (targetLight != null)
            {
                baseIntensity = targetLight.intensity;
            }
        }

        private void Start()
        {
            if (targetLight != null)
            {
                StartCoroutine(FlickerRoutine());
            }
        }

        private IEnumerator FlickerRoutine()
        {
            while (true)
            {
                // Wait for the next irregular flicker episode
                float waitTime = Random.Range(episodeIntervalMin, episodeIntervalMax);
                yield return new WaitForSeconds(waitTime);

                // Determine whether this episode triggers a blackout
                if (Random.value < blackoutProbability)
                {
                    yield return StartCoroutine(PerformBlackout());
                }
                else
                {
                    yield return StartCoroutine(PerformFlickerBurst());
                }
            }
        }

        private IEnumerator PerformFlickerBurst()
        {
            int flashes = Random.Range(minFlashesPerEpisode, maxFlashesPerEpisode + 1);

            for (int i = 0; i < flashes; i++)
            {
                float targetIntensity = (Random.value > 0.5f) 
                    ? Random.Range(minFlickerIntensity, baseIntensity * 0.4f)
                    : Random.Range(baseIntensity * 1.1f, maxFlickerIntensity);

                SetLightLevel(targetIntensity);

                if (buzzAudio != null && !buzzAudio.isPlaying)
                {
                    buzzAudio.Play();
                }

                yield return new WaitForSeconds(Random.Range(0.03f, 0.09f));

                // Quick momentary restoration
                SetLightLevel(baseIntensity);
                yield return new WaitForSeconds(Random.Range(0.04f, 0.12f));
            }

            SetLightLevel(baseIntensity);
        }

        private IEnumerator PerformBlackout()
        {
            // Rapid cutoff
            SetLightLevel(0f);

            if (buzzAudio != null && buzzAudio.isPlaying)
            {
                buzzAudio.Stop();
            }

            float duration = Random.Range(blackoutDurationMin, blackoutDurationMax);
            yield return new WaitForSeconds(duration);

            // Pop back on with a small surge
            SetLightLevel(maxFlickerIntensity);
            yield return new WaitForSeconds(0.05f);
            SetLightLevel(baseIntensity);
        }

        private void SetLightLevel(float intensity)
        {
            bool gridPowered = EscapeTheLab.Systems.PowerGridManager.Instance == null || EscapeTheLab.Systems.PowerGridManager.Instance.LightsActive;

            if (targetLight != null)
            {
                targetLight.intensity = intensity;
                targetLight.enabled = gridPowered && (intensity > 0.001f);
            }

            if (fixtureMaterial != null && fixtureMaterial.HasProperty(EmissionColorId))
            {
                float factor = (baseIntensity > 0 && gridPowered) ? (intensity / baseIntensity) : 0f;
                Color currentEmission = baseEmissionColor * factor;
                fixtureMaterial.SetColor(EmissionColorId, currentEmission);
            }
        }

        public void SetupReferences(Light lightComp, float intensity)
        {
            targetLight = lightComp;
            baseIntensity = intensity;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
