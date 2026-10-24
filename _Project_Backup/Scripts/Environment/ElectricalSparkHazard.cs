using System.Collections;
using UnityEngine;
using EscapeTheLab.Systems;
using EscapeTheLab.Audio;

namespace EscapeTheLab.Environment
{
    /// <summary>
    /// Dangling Live Wire Electrical Spark Hazard:
    /// Periodically emits brilliant blue/white arcs, crackling sound, and a localized acoustic ping.
    /// Creates tension and visual depth in corridors and sub-basement corridors.
    /// </summary>
    public class ElectricalSparkHazard : MonoBehaviour
    {
        [Header("Spark Timing")]
        [SerializeField] private float minInterval = 2.5f;
        [SerializeField] private float maxInterval = 6.0f;
        [SerializeField] private float arcNoiseRadius = 8.0f;

        private Light sparkLight;
        private AudioSource audioSource;
        private AudioClip arcClip;

        private void Awake()
        {
            sparkLight = GetComponentInChildren<Light>();
            if (sparkLight == null)
            {
                GameObject lightObj = new GameObject("SparkLight");
                lightObj.transform.parent = transform;
                lightObj.transform.localPosition = Vector3.zero;
                sparkLight = lightObj.AddComponent<Light>();
                sparkLight.type = LightType.Point;
                sparkLight.color = new Color(0.4f, 0.75f, 1.0f);
                sparkLight.range = 7f;
                sparkLight.intensity = 0f;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.minDistance = 2f;
            audioSource.maxDistance = 18f;

            arcClip = ProceduralAudioClips.CreateElectricArc("SparkSound");
        }

        private void Start()
        {
            StartCoroutine(SparkRoutine());
        }

        private IEnumerator SparkRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

                // 2-3 rapid arc pulses
                int pulses = Random.Range(2, 5);
                for (int i = 0; i < pulses; i++)
                {
                    if (sparkLight != null) sparkLight.intensity = Random.Range(2.5f, 5.0f);
                    if (audioSource != null && arcClip != null)
                    {
                        audioSource.pitch = Random.Range(0.9f, 1.3f);
                        audioSource.PlayOneShot(arcClip, 0.7f);
                    }

                    if (NoiseManager.Instance != null && i == 0)
                    {
                        NoiseManager.Instance.EmitNoise(transform.position, arcNoiseRadius, 1.0f, "Electrical_Spark");
                    }

                    yield return new WaitForSeconds(Random.Range(0.04f, 0.08f));
                    if (sparkLight != null) sparkLight.intensity = 0f;
                    yield return new WaitForSeconds(Random.Range(0.03f, 0.07f));
                }
            }
        }
    }
}
