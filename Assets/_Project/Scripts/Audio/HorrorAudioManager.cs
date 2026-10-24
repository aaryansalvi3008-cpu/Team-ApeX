using UnityEngine;
using EscapeTheLab.AI;
using EscapeTheLab.Player;

namespace EscapeTheLab.Audio
{
    /// <summary>
    /// Master horror audio controller:
    /// - Continuous atmospheric industrial drone ambience.
    /// - Dynamic proximity heartbeat that intensifies and accelerates as the monster nears.
    /// - Footsteps, flashlight switches, door mechanisms, and keycard chime playback.
    /// - Electromagnetic flashlight interference flicker when near the creature.
    /// </summary>
    public class HorrorAudioManager : MonoBehaviour
    {
        public static HorrorAudioManager Instance { get; private set; }

        [Header("Volume Balances")]
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 1.0f;
        [Range(0f, 1f)] [SerializeField] private float ambienceVolume = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float heartbeatMaxVolume = 0.75f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.85f;

        [Header("Heartbeat Proximity")]
        [SerializeField] private float heartbeatStartDistance = 18f;
        [SerializeField] private float heartbeatMaxDistance = 4f;

        // Audio Sources
        private AudioSource ambienceSource;
        private AudioSource heartbeatSource;
        private AudioSource sfxSource;
        private AudioSource footstepSource;

        // Generated Procedural Audio Clips
        private AudioClip droneClip;
        private AudioClip heartbeatClip;
        private AudioClip footstepClip;
        private AudioClip flashlightClip;
        private AudioClip keycardClip;
        private AudioClip doorClip;
        private AudioClip accessDeniedClip;
        private AudioClip accessGrantedClip;
        private AudioClip roarClip;
        private AudioClip glassShatterClip;
        private AudioClip itemImpactClip;
        private AudioClip lurePulseClip;
        private AudioClip throwWhooshClip;

        // Tracking References
        private Transform playerTransform;
        private MonsterController monster;
        private FlashlightController playerFlashlight;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SetupAudioSources();
            GenerateClips();
        }

        private void Start()
        {
            FindReferences();

            // Start ambient drone
            if (ambienceSource != null && droneClip != null)
            {
                ambienceSource.clip = droneClip;
                ambienceSource.loop = true;
                ambienceSource.volume = ambienceVolume * masterVolume;
                ambienceSource.Play();
            }

            // Start heartbeat in loop but muted initially
            if (heartbeatSource != null && heartbeatClip != null)
            {
                heartbeatSource.clip = heartbeatClip;
                heartbeatSource.loop = true;
                heartbeatSource.volume = 0f;
                heartbeatSource.Play();
            }
        }

        private void SetupAudioSources()
        {
            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.playOnAwake = false;
            ambienceSource.spatialBlend = 0f; // 2D

            heartbeatSource = gameObject.AddComponent<AudioSource>();
            heartbeatSource.playOnAwake = false;
            heartbeatSource.spatialBlend = 0f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;

            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.playOnAwake = false;
            footstepSource.spatialBlend = 0f;
        }

        private void GenerateClips()
        {
            droneClip = ProceduralAudioClips.CreateFacilityDrone();
            heartbeatClip = ProceduralAudioClips.CreateHeartbeatClip();
            footstepClip = ProceduralAudioClips.CreateFootstepClip();
            flashlightClip = ProceduralAudioClips.CreateFlashlightClickClip();
            keycardClip = ProceduralAudioClips.CreateKeycardChimeClip();
            doorClip = ProceduralAudioClips.CreateDoorSlideClip();
            accessDeniedClip = ProceduralAudioClips.CreateAccessDeniedClip();
            accessGrantedClip = ProceduralAudioClips.CreateAccessGrantedClip();
            roarClip = ProceduralAudioClips.CreateMonsterRoarClip();
            glassShatterClip = ProceduralAudioClips.CreateGlassShatter();
            itemImpactClip = ProceduralAudioClips.CreateItemImpact();
            lurePulseClip = ProceduralAudioClips.CreateSonicLurePulse();
            throwWhooshClip = ProceduralAudioClips.CreateItemThrowWhoosh();
        }

        private void FindReferences()
        {
            if (playerTransform == null)
            {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null)
                {
                    playerTransform = p.transform;
                    playerFlashlight = p.GetComponentInChildren<FlashlightController>();
                }
            }

            if (monster == null)
            {
                monster = FindAnyObjectByType<MonsterController>();
            }
        }

        private void Update()
        {
            if (playerTransform == null || monster == null)
            {
                FindReferences();
                return;
            }

            UpdateHeartbeat();
        }

        private void UpdateHeartbeat()
        {
            if (heartbeatSource == null) return;

            float distance = Vector3.Distance(playerTransform.position, monster.transform.position);

            float targetVol = 0f;
            float targetPitch = 1.0f;

            if (monster.CurrentState == MonsterController.AIState.Chase)
            {
                // In chase: rapid loud pounding
                targetVol = heartbeatMaxVolume * masterVolume;
                targetPitch = 1.45f;
            }
            else if (distance < heartbeatStartDistance)
            {
                // Proximity escalation
                float proximity = Mathf.InverseLerp(heartbeatStartDistance, heartbeatMaxDistance, distance);
                targetVol = proximity * heartbeatMaxVolume * masterVolume;
                targetPitch = Mathf.Lerp(1.0f, 1.35f, proximity);
            }

            heartbeatSource.volume = Mathf.Lerp(heartbeatSource.volume, targetVol, Time.deltaTime * 3.5f);
            heartbeatSource.pitch = Mathf.Lerp(heartbeatSource.pitch, targetPitch, Time.deltaTime * 3.0f);
        }

        #region Public Sound Playback Methods

        public void PlayFootstep(bool isSprinting)
        {
            if (footstepSource != null && footstepClip != null)
            {
                footstepSource.pitch = Random.Range(0.92f, 1.08f) * (isSprinting ? 1.15f : 1.0f);
                footstepSource.PlayOneShot(footstepClip, (isSprinting ? 0.75f : 0.45f) * sfxVolume * masterVolume);
            }
        }

        public void PlayFlashlightClick()
        {
            if (sfxSource != null && flashlightClip != null)
            {
                sfxSource.pitch = Random.Range(0.95f, 1.05f);
                sfxSource.PlayOneShot(flashlightClip, sfxVolume * masterVolume);
            }
        }

        public void PlayKeycardChime()
        {
            if (sfxSource != null && keycardClip != null)
            {
                sfxSource.pitch = 1.0f;
                sfxSource.PlayOneShot(keycardClip, sfxVolume * masterVolume);
            }
        }

        public void PlayDoorSlide()
        {
            if (sfxSource != null && doorClip != null)
            {
                sfxSource.pitch = Random.Range(0.95f, 1.05f);
                sfxSource.PlayOneShot(doorClip, sfxVolume * masterVolume);
            }
        }

        public void PlayAccessDenied()
        {
            if (sfxSource != null && accessDeniedClip != null)
            {
                sfxSource.pitch = 1.0f;
                sfxSource.PlayOneShot(accessDeniedClip, sfxVolume * masterVolume);
            }
        }

        public void PlayAccessGranted()
        {
            if (sfxSource != null && accessGrantedClip != null)
            {
                sfxSource.pitch = 1.0f;
                sfxSource.PlayOneShot(accessGrantedClip, sfxVolume * masterVolume);
            }
        }

        public void PlayMonsterRoar()
        {
            if (sfxSource != null && roarClip != null)
            {
                sfxSource.pitch = Random.Range(0.9f, 1.05f);
                sfxSource.PlayOneShot(roarClip, 0.95f * masterVolume);
            }
        }

        public void PlayGlassShatter()
        {
            if (sfxSource != null && glassShatterClip != null)
            {
                sfxSource.pitch = Random.Range(0.95f, 1.08f);
                sfxSource.PlayOneShot(glassShatterClip, 1.0f * sfxVolume * masterVolume);
            }
        }

        public void PlayItemImpact()
        {
            if (sfxSource != null && itemImpactClip != null)
            {
                sfxSource.pitch = Random.Range(0.92f, 1.10f);
                sfxSource.PlayOneShot(itemImpactClip, 0.85f * sfxVolume * masterVolume);
            }
        }

        public void PlaySonicLurePulse()
        {
            if (sfxSource != null && lurePulseClip != null)
            {
                sfxSource.pitch = 1.0f;
                sfxSource.PlayOneShot(lurePulseClip, 0.95f * sfxVolume * masterVolume);
            }
        }

        public void PlayThrowWhoosh()
        {
            if (sfxSource != null && throwWhooshClip != null)
            {
                sfxSource.pitch = Random.Range(0.95f, 1.05f);
                sfxSource.PlayOneShot(throwWhooshClip, 0.70f * sfxVolume * masterVolume);
            }
        }

        #endregion
    }
}
