using UnityEngine;

namespace EscapeTheLab.Environment
{
    /// <summary>
    /// Simulates erratic fluorescent tube stutter and flickering for broken horror fixtures.
    /// </summary>
    public class FlickeringLight : MonoBehaviour
    {
        [SerializeField] private float minIntensity = 0.4f;
        [SerializeField] private float maxIntensity = 1.5f;
        [SerializeField] private float flickerSpeed = 0.08f;

        private Light targetLight;
        private float baseIntensity = 1.4f;
        private float timer;

        private void Awake()
        {
            targetLight = GetComponentInChildren<Light>();
            if (targetLight != null) baseIntensity = targetLight.intensity;
        }

        private void Update()
        {
            if (targetLight == null) return;
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                // Occasional dramatic micro-stutter
                if (Random.value < 0.18f)
                {
                    targetLight.intensity = Random.Range(0.05f, 0.25f);
                    timer = Random.Range(0.04f, 0.12f);
                }
                else
                {
                    targetLight.intensity = baseIntensity * Random.Range(minIntensity, maxIntensity);
                    timer = Random.Range(0.02f, flickerSpeed);
                }
            }
        }
    }
}
