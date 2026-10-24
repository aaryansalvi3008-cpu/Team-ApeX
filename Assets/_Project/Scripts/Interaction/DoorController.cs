using System.Collections;
using UnityEngine;
using EscapeTheLab.UI;
using EscapeTheLab.Systems;

namespace EscapeTheLab.Interaction
{
    /// <summary>
    /// Interactive sliding or rotating door connected to the Power Grid.
    /// - Normal doors require Doors Circuit to be active.
    /// - Exit Airlock requires Systems Circuit active + Security Keycard.
    /// </summary>
    public class DoorController : MonoBehaviour, IInteractable
    {
        public enum DoorType { SlidingUp, SlidingSide, Rotating }

        [Header("Door Configuration")]
        [SerializeField] private DoorType doorType = DoorType.SlidingUp;
        [SerializeField] private Transform movingDoorTransform;
        [SerializeField] private float openDistance = 3.2f;
        [SerializeField] private float animationDuration = 1.4f;

        [Header("Lock & Power Settings")]
        [Tooltip("Whether this door requires the security keycard to unlock.")]
        [SerializeField] private bool requiresKeycard = false;

        [Tooltip("Whether the door is currently locked.")]
        [SerializeField] private bool isLocked = false;

        [Tooltip("Whether this door relies on the Doors power circuit to function.")]
        [SerializeField] private bool requiresDoorsPower = true;

        [Tooltip("Whether this is the main exit airlock (requires Systems power circuit).")]
        [SerializeField] private bool isExitAirlock = false;

        [Header("Scanner Visual Feedback")]
        [SerializeField] private Renderer scannerRenderer;
        [SerializeField] private Material lockedMaterial;
        [SerializeField] private Material unlockedMaterial;

        [Header("Audio Feedback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip accessDeniedClip;
        [SerializeField] private AudioClip accessGrantedClip;

        // Runtime states
        private Vector3 closedPosition;
        private Vector3 openPosition;
        private Quaternion closedRotation;
        private Quaternion openRotation;
        private bool isOpen = false;
        private bool isAnimating = false;

        private void Awake()
        {
            InitPositions();
        }

        private void Start()
        {
            InitPositions();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            UpdateScannerVisual();
        }

        public void SetMovingTransform(Transform t)
        {
            movingDoorTransform = t;
            InitPositions();
        }

        public void InitPositions()
        {
            if (movingDoorTransform == null) movingDoorTransform = transform;

            closedPosition = movingDoorTransform.position;
            closedRotation = movingDoorTransform.rotation;

            switch (doorType)
            {
                case DoorType.SlidingUp:
                    openPosition = closedPosition + Vector3.up * openDistance;
                    break;
                case DoorType.SlidingSide:
                    openPosition = closedPosition + movingDoorTransform.right * openDistance;
                    break;
                case DoorType.Rotating:
                    openRotation = closedRotation * Quaternion.Euler(0, 90f, 0);
                    break;
            }
        }

        public string GetPromptText()
        {
            if (isAnimating) return "";

            // Check power states
            if (isExitAirlock)
            {
                if (PowerGridManager.Instance != null && !PowerGridManager.Instance.SystemsActive)
                {
                    return "[E] Airlock Unpowered (Route Power to Systems Breaker)";
                }
                if (isLocked)
                {
                    return "[E] Airlock Locked (Security Keycard Required)";
                }
            }
            else if (requiresDoorsPower && PowerGridManager.Instance != null && !PowerGridManager.Instance.DoorsActive)
            {
                return "[E] Door Offline (Doors Circuit Unpowered)";
            }

            if (isLocked)
            {
                return requiresKeycard ? "[E] Locked (Keycard Required)" : "[E] Locked";
            }

            return isOpen ? "[E] Close Door" : "[E] Open Door";
        }

        public bool CanInteract()
        {
            return !isAnimating;
        }

        public void Interact(PlayerInteraction player)
        {
            if (isAnimating) return;

            // 1. Check Exit Airlock power & keycard conditions
            if (isExitAirlock)
            {
                if (PowerGridManager.Instance != null && !PowerGridManager.Instance.SystemsActive)
                {
                    HUDController.Instance?.ShowNotification("Airlock terminal offline! Route power to Systems in Reactor.", Color.red, 3.0f);
                    PlayDeniedAudio();
                    return;
                }

                bool hasCard = player.HasKeycard || (InventoryManager.Instance != null && (InventoryManager.Instance.HasItem(ItemType.KeycardLevel1) || InventoryManager.Instance.HasItem(ItemType.KeycardLevel2)));
                if (!hasCard)
                {
                    HUDController.Instance?.ShowNotification("Access Denied! Security Keycard required.", Color.red, 2.5f);
                    PlayDeniedAudio();
                    return;
                }

                // Both power and keycard present: unlock airlock!
                isLocked = false;
                UpdateScannerVisual();
                HUDController.Instance?.ShowNotification("Access Granted! Main Exit Airlock Unsealed.", Color.green, 3.0f);
                PlayGrantedAudio();
                StartCoroutine(AnimateDoor(true));
                return;
            }

            // 2. Normal Doors power check
            if (requiresDoorsPower && PowerGridManager.Instance != null && !PowerGridManager.Instance.DoorsActive)
            {
                HUDController.Instance?.ShowNotification("Blast Door offline! Doors power circuit is cut.", Color.yellow, 2.5f);
                PlayDeniedAudio();
                return;
            }

            // 3. Normal locked check
            if (isLocked)
            {
                bool hasCard = player.HasKeycard || (InventoryManager.Instance != null && (InventoryManager.Instance.HasItem(ItemType.KeycardLevel1) || InventoryManager.Instance.HasItem(ItemType.KeycardLevel2)));
                if (requiresKeycard && hasCard)
                {
                    isLocked = false;
                    UpdateScannerVisual();
                    HUDController.Instance?.ShowNotification("Door Unlocked.", Color.green, 2.0f);
                    PlayGrantedAudio();
                    StartCoroutine(AnimateDoor(true));
                }
                else
                {
                    HUDController.Instance?.ShowNotification("Door is locked.", Color.red, 2.0f);
                    PlayDeniedAudio();
                }
                return;
            }

            // 4. Normal toggle
            StartCoroutine(AnimateDoor(!isOpen));
        }

        private IEnumerator AnimateDoor(bool targetOpen)
        {
            isAnimating = true;
            if (movingDoorTransform == null) movingDoorTransform = transform;
            if (openPosition == Vector3.zero && closedPosition == Vector3.zero) InitPositions();

            if (audioSource != null && openClip != null)
            {
                audioSource.PlayOneShot(openClip);
            }
            else
            {
                Audio.HorrorAudioManager.Instance?.PlayDoorSlide();
            }

            // Fast opening noise event (GDD: opening doors emits noise)
            NoiseManager.Instance?.EmitNoise(transform.position, 6.0f, 0.5f, "DoorSlide");

            float elapsed = 0f;

            if (doorType == DoorType.Rotating)
            {
                Quaternion startRot = movingDoorTransform.rotation;
                Quaternion endRot = targetOpen ? openRotation : closedRotation;

                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);
                    movingDoorTransform.rotation = Quaternion.Slerp(startRot, endRot, t);
                    yield return null;
                }
                movingDoorTransform.rotation = endRot;
            }
            else
            {
                Vector3 startPos = movingDoorTransform.position;
                Vector3 endPos = targetOpen ? openPosition : closedPosition;

                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);
                    movingDoorTransform.position = Vector3.Lerp(startPos, endPos, t);
                    yield return null;
                }
                movingDoorTransform.position = endPos;
            }

            isOpen = targetOpen;
            isAnimating = false;

            if (HUDController.Instance != null)
            {
                HUDController.Instance.SetPrompt(GetPromptText());
            }
        }

        private void PlayDeniedAudio()
        {
            if (audioSource != null && accessDeniedClip != null) audioSource.PlayOneShot(accessDeniedClip);
            else Audio.HorrorAudioManager.Instance?.PlayAccessDenied();
        }

        private void PlayGrantedAudio()
        {
            if (audioSource != null && accessGrantedClip != null) audioSource.PlayOneShot(accessGrantedClip);
            else Audio.HorrorAudioManager.Instance?.PlayAccessGranted();
        }

        private void UpdateScannerVisual()
        {
            if (scannerRenderer != null)
            {
                if (isLocked && lockedMaterial != null) scannerRenderer.sharedMaterial = lockedMaterial;
                else if (!isLocked && unlockedMaterial != null) scannerRenderer.sharedMaterial = unlockedMaterial;
            }
        }

        public void SetupDoor(bool lockDoor, bool needKeycard, Renderer scanner = null, Material lockedMat = null, Material unlockedMat = null)
        {
            SetupDoor(lockDoor, needKeycard, false, false, scanner, lockedMat, unlockedMat);
        }

        public void SetupDoor(bool lockDoor, bool needKeycard, bool exitAirlock, bool needsDoorsPower = false, Renderer scanner = null, Material lockedMat = null, Material unlockedMat = null)
        {
            isLocked = lockDoor;
            requiresKeycard = needKeycard;
            isExitAirlock = exitAirlock;
            requiresDoorsPower = needsDoorsPower;
            scannerRenderer = scanner;
            lockedMaterial = lockedMat;
            unlockedMaterial = unlockedMat;
            InitPositions();
        }
    }
}
