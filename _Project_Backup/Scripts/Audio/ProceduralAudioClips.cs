using UnityEngine;

namespace EscapeTheLab.Audio
{
    /// <summary>
    /// Generates high-quality procedural horror audio clips directly in memory
    /// using Unity's native AudioClip.Create API.
    /// Provides 100% self-contained audio with zero external asset dependencies.
    /// </summary>
    public static class ProceduralAudioClips
    {
        private const int SampleRate = 44100;

        /// <summary>
        /// Generates an ominous low-frequency industrial facility drone loop.
        /// </summary>
        public static AudioClip CreateFacilityDrone(string name = "FacilityDrone", float length = 3.0f)
        {
            int totalSamples = (int)(SampleRate * length);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;

                // 48 Hz deep sub-bass hum + 96 Hz harmonic + 144 Hz electrical hum
                float sub = Mathf.Sin(2f * Mathf.PI * 48f * t) * 0.45f;
                float harm1 = Mathf.Sin(2f * Mathf.PI * 96f * t) * 0.20f;
                float harm2 = Mathf.Sin(2f * Mathf.PI * 144f * t) * 0.10f;

                // Subtle ambient noise / hiss
                float noise = (Random.value * 2f - 1f) * 0.03f;

                // Slow atmospheric LFO modulation (0.2 Hz)
                float lfo = 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);

                samples[i] = (sub + harm1 + harm2 + noise) * lfo * 0.6f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a realistic human heartbeat pulse (lub-dub).
        /// </summary>
        public static AudioClip CreateHeartbeatClip(string name = "Heartbeat", float length = 1.0f)
        {
            int totalSamples = (int)(SampleRate * length);
            float[] samples = new float[totalSamples];

            // Lub (low deep thump at 0.1s)
            AddThump(samples, 0.08f, 55f, 0.12f, 0.85f);

            // Dub (slightly higher thump at 0.35s)
            AddThump(samples, 0.32f, 68f, 0.10f, 0.70f);

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a dull, muffled shoe impact on concrete or metal.
        /// </summary>
        public static AudioClip CreateFootstepClip(string name = "Footstep")
        {
            float length = 0.18f;
            int totalSamples = (int)(SampleRate * length);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 28f);

                // Low thud + filtered noise heel tap
                float thud = Mathf.Sin(2f * Mathf.PI * 90f * t);
                float noise = (Random.value * 2f - 1f) * 0.4f;

                samples[i] = (thud * 0.6f + noise * 0.4f) * env * 0.7f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates an industrial mechanical click for the flashlight toggle.
        /// </summary>
        public static AudioClip CreateFlashlightClickClip(string name = "FlashlightClick")
        {
            float length = 0.06f;
            int totalSamples = (int)(SampleRate * length);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 80f);

                float click1 = Mathf.Sin(2f * Mathf.PI * 1800f * t);
                float click2 = Mathf.Sin(2f * Mathf.PI * 3200f * t);
                float snap = (Random.value * 2f - 1f) * 0.3f;

                samples[i] = (click1 * 0.5f + click2 * 0.3f + snap) * env * 0.9f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a keycard pickup chime.
        /// </summary>
        public static AudioClip CreateKeycardChimeClip(string name = "KeycardChime")
        {
            float length = 0.6f;
            int totalSamples = (int)(SampleRate * length);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 6f);

                float note1 = Mathf.Sin(2f * Mathf.PI * 587.33f * t); // D5
                float note2 = Mathf.Sin(2f * Mathf.PI * 880.00f * t); // A5

                samples[i] = (note1 * 0.5f + note2 * 0.5f) * env * 0.6f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a heavy hydraulic sliding door sound.
        /// </summary>
        public static AudioClip CreateDoorSlideClip(string name = "DoorSlide", float length = 1.3f)
        {
            int totalSamples = (int)(SampleRate * length);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / length));

                float motorHum = Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.35f;
                float frictionNoise = (Random.value * 2f - 1f) * 0.35f;

                samples[i] = (motorHum + frictionNoise) * env * 0.7f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates an electronic buzz error tone for locked doors.
        /// </summary>
        public static AudioClip CreateAccessDeniedClip(string name = "AccessDenied")
        {
            float length = 0.35f;
            int totalSamples = (int)(SampleRate * length);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 8f);

                float buzz = Mathf.Sin(2f * Mathf.PI * 160f * t) > 0f ? 1f : -1f; // Square wave

                samples[i] = buzz * env * 0.4f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates an electronic confirmation tone.
        /// </summary>
        public static AudioClip CreateAccessGrantedClip(string name = "AccessGranted")
        {
            float length = 0.5f;
            int totalSamples = (int)(SampleRate * length);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 5f);

                float freq = t < 0.2f ? 523.25f : 659.25f; // C5 to E5
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t);

                samples[i] = tone * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a guttural horror monster roar / shriek.
        /// </summary>
        public static AudioClip CreateMonsterRoarClip(string name = "MonsterRoar", float length = 1.2f)
        {
            int totalSamples = (int)(SampleRate * length);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / length));

                float mod = Mathf.Sin(2f * Mathf.PI * 30f * t) * 25f;
                float freq = Mathf.Lerp(110f, 60f, t / length) + mod;
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);

                float noise = (Random.value * 2f - 1f) * 0.4f;
                float distorted = Mathf.Clamp((wave + noise) * 1.5f, -1f, 1f);

                samples[i] = distorted * env * 0.75f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a sharp crunchy glass shattering step sound.
        /// </summary>
        public static AudioClip CreateGlassCrunch(string name = "GlassCrunch", float length = 0.35f)
        {
            int totalSamples = (int)(SampleRate * length);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 14f);

                float tone1 = Mathf.Sin(2f * Mathf.PI * 2400f * t);
                float tone2 = Mathf.Sin(2f * Mathf.PI * 3800f * t);
                float noise = (Random.value * 2f - 1f) * 0.7f;

                samples[i] = (tone1 * 0.3f + tone2 * 0.2f + noise) * env * 0.8f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates an electric spark snap sound.
        /// </summary>
        public static AudioClip CreateElectricArc(string name = "ElectricArc", float length = 0.25f)
        {
            int totalSamples = (int)(SampleRate * length);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 22f);
                float buzz = Mathf.Sin(2f * Mathf.PI * 120f * t);
                float crack = (Random.value * 2f - 1f) * 0.85f;

                samples[i] = (buzz * 0.4f + crack * 0.8f) * env * 0.85f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static void AddThump(float[] buffer, float startTime, float freq, float duration, float amp)
        {
            int startSample = (int)(startTime * SampleRate);
            int lengthSamples = (int)(duration * SampleRate);

            for (int i = 0; i < lengthSamples; i++)
            {
                int idx = startSample + i;
                if (idx >= buffer.Length) break;

                float t = (float)i / SampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);

                buffer[idx] += wave * env * amp;
            }
        }
    }
}
