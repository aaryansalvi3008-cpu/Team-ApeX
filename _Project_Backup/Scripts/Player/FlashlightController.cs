using System;
using UnityEngine;
using EscapeTheLab.Systems;
using EscapeTheLab.UI;

namespace EscapeTheLab.Player
{
    /// <summary>
    /// Controls the handheld flashlight and scanner tool specified in GDD Section 3.5 & Appendix A:
    /// - Flashlight (F key): Handheld light with smooth inertial sway and limited battery life.
    /// - Flashlight Battery: Drains when ON; rechargeable with found Battery Packs.
    /// - Scanner (R key): Pulses to reveal interactables and objectives briefly; emits a quiet ping (3m noise).
    /// </summary>
    public class FlashlightController : MonoBehaviour
    {
        [Header("Flashlight Components")]
        [SerializeField] private Light flashlightLight;
        [SerializeField] private Transform followTarget;

        [Header("Controls")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F;
        [SerializeField] private KeyCode scannerKey = KeyCode.R;

        [Header("Battery Life (GDD Appendix A: 4 min)")]
        [Tooltip("Total battery life in seconds.")]
        [SerializeField] private float maxBatteryLife = 240.0f;
        [SerializeField] private float currentBattery = 240.0f;

        [Header("Handheld Sway & Inertia")]
        [SerializeField] private bool enableSway = true;
        [SerializeField] private float swaySpeed = 12f;

        [Header("Scanner Tool (GDD Section 3.5)")]
        [SerializeField] private float scannerCooldown = 6.0f;
        [SerializeField] private float scannerPingNoiseRadius = 3.0f;
        private float scannerTimer = 0f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip toggleSound;

        // Current state
        public bool IsOn { get; private set; }
        public float BatteryRatio => Mathf.Clamp01(currentBattery / maxBatteryLife);
        public bool IsScannerReady => scannerTimer <= 0f;
        public float ScannerCooldownRatio => Mathf.Clamp01(scannerTimer / scannerCooldown);

        private void Awake()
        {
            if (flashlightLight == null) flashlightLight = GetComponent<Light>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (followTarget == null && transform.parent != null) followTarget = transform.parent;

            currentBattery = maxBatteryLife;
        }

        private void Start()
        {
            SetFlashlightState(true);
        }

        private void Update()
        {
            HandleFlashlightToggle();
            HandleBatteryDrain();
            HandleScannerPulse();
            HandleSway();
        }

        private void HandleFlashlightToggle()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (!IsOn && currentBattery <= 0f)
                {
                    HUDController.Instance?.ShowNotification("Flashlight battery is dead! Find a Battery Pack.", Color.red, 2.0f);
                    return;
                }
                SetFlashlightState(!IsOn);
            }
        }

        private void HandleBatteryDrain()
        {
            if (IsOn)
            {
                currentBattery -= Time.deltaTime;
                if (currentBattery <= 0f)
                {
                    currentBattery = 0f;
                    SetFlashlightState(false);
                    HUDController.Instance?.ShowNotification("Flashlight battery depleted!", Color.red, 2.5f);
                }
            }
        }

        private void HandleScannerPulse()
        {
            if (scannerTimer > 0f)
            {
                scannerTimer -= Time.deltaTime;
            }

            if (Input.GetKeyDown(scannerKey) && IsScannerReady)
            {
                TriggerScannerPulse();
            }
        }

        private void TriggerScannerPulse()
        {
            scannerTimer = scannerCooldown;

            // Emit quiet sonar ping noise event (GDD: 3m radius)
            NoiseManager.Instance?.EmitNoise(transform.position, scannerPingNoiseRadius, 0.3f, "ScannerPing", true);

            // Play procedural chime
            Audio.HorrorAudioManager.Instance?.PlayKeycardChime();

            // Trigger HUD Scanner highlight
            HUDController.Instance?.TriggerScannerPulse(3.5f);
            HUDController.Instance?.ShowNotification("Scanner Pulse: Scanning for interactables...", Color.cyan, 2.0f);
        }

        private void HandleSway()
        {
            if (!enableSway || followTarget == null) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, followTarget.rotation, Time.deltaTime * swaySpeed);
        }

        public void SetFlashlightState(bool turnOn)
        {
            IsOn = turnOn && currentBattery > 0f;

            if (flashlightLight != null)
            {
                flashlightLight.enabled = IsOn;
            }

            PlayToggleAudio();
        }

        public void RechargeBattery(float amountPercent = 0.5f)
        {
            currentBattery = Mathf.Min(currentBattery + maxBatteryLife * amountPercent, maxBatteryLife);
            HUDController.Instance?.ShowNotification($"Flashlight Recharged! ({(int)(BatteryRatio * 100)}%)", Color.green, 2.5f);
        }

        private void PlayToggleAudio()
        {
            if (audioSource != null && toggleSound != null)
            {
                audioSource.PlayOneShot(toggleSound);
            }
            else
            {
                Audio.HorrorAudioManager.Instance?.PlayFlashlightClick();
            }
        }

        public void SetupReferences(Light lightComp, Transform target, AudioSource aSource = null)
        {
            flashlightLight = lightComp;
            followTarget = target;
            audioSource = aSource;
        }
    }
}
