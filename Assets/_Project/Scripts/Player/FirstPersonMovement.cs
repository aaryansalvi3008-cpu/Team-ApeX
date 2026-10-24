using UnityEngine;
using EscapeTheLab.Systems;
using EscapeTheLab.UI;

namespace EscapeTheLab.Player
{
    /// <summary>
    /// Handles first-person movement, crouching, sprinting, surface noise modulation,
    /// and the breath-holding stealth mechanic specified in GDD Section 3.1 & 3.3.
    /// Emits real-time NoiseEvents to the NoiseManager based on gait and floor surface.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonMovement : MonoBehaviour
    {
        [Header("Movement Speeds (GDD Tuned)")]
        [Tooltip("Crouch-walking speed: 1.5 m/s")]
        [SerializeField] private float crouchSpeed = 1.5f;

        [Tooltip("Standard walking speed: 3.0 m/s")]
        [SerializeField] private float walkSpeed = 3.0f;

        [Tooltip("Sprinting speed: 5.5 m/s")]
        [SerializeField] private float sprintSpeed = 5.5f;

        [Header("Controls")]
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
        [SerializeField] private KeyCode crouchKeyAlt = KeyCode.C;
        [SerializeField] private KeyCode breathKey = KeyCode.Space;

        [Header("Crouching Settings")]
        [SerializeField] private float standingHeight = 2.0f;
        [SerializeField] private float crouchingHeight = 1.1f;
        [SerializeField] private float standingCameraY = 1.6f;
        [SerializeField] private float crouchingCameraY = 0.85f;
        [SerializeField] private float crouchTransitionSpeed = 9f;

        [Header("Noise Radii (GDD Section 3.1)")]
        [SerializeField] private float crouchNoiseRadius = 2.0f;
        [SerializeField] private float walkNoiseRadius = 6.0f;
        [SerializeField] private float sprintNoiseRadius = 14.0f;
        [SerializeField] private float jumpNoiseRadius = 10.0f;

        [Header("Breath-Hold Mechanic (GDD Section 3.3)")]
        [SerializeField] private float maxBreathStamina = 8.0f; // Seconds
        [SerializeField] private float breathRecoveryRate = 2.0f;

        [Header("Physics & Gravity")]
        [SerializeField] private float gravity = -19.6f;
        [SerializeField] private float groundedStickForce = -2.5f;

        [Header("Camera & Head Bobbing")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private bool enableHeadBob = true;
        [SerializeField] private float bobFrequency = 8f;
        [SerializeField] private float bobAmount = 0.035f;

        // References
        private CharacterController controller;
        private Vector3 verticalVelocity = Vector3.zero;
        private Vector3 defaultCameraLocalPos;
        private float bobTimer = 0f;
        private float footstepTimer = 0f;

        // Breath state
        private float currentBreathStamina;
        private bool isGasping = false;
        private float gaspTimer = 0f;

        // Public State Properties
        public bool IsMoving { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsHoldingBreath { get; private set; }
        public bool IsGrounded => controller != null && controller.isGrounded;
        public float CurrentSpeed { get; private set; }
        public float BreathRatio => Mathf.Clamp01(currentBreathStamina / maxBreathStamina);
        public float BreathStaminaRatio => BreathRatio;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (cameraTransform == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null) cameraTransform = cam.transform;
            }

            if (cameraTransform != null)
            {
                defaultCameraLocalPos = cameraTransform.localPosition;
            }

            currentBreathStamina = maxBreathStamina;
        }

        private void Update()
        {
            if (HUDController.Instance != null && HUDController.Instance.IsInUIMenu)
            {
                IsMoving = false;
                IsSprinting = false;
                return;
            }

            HandleCrouching();
            HandleMovement();
            HandleGravity();
            HandleBreathHold();
            HandleHeadBob();
            HandleFootstepNoise();
        }

        private void HandleCrouching()
        {
            bool wantsCrouch = Input.GetKey(crouchKey) || Input.GetKey(crouchKeyAlt);
            IsCrouching = wantsCrouch;

            // Smoothly adjust CharacterController height and center
            float targetHeight = IsCrouching ? crouchingHeight : standingHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            controller.center = new Vector3(0f, controller.height * 0.5f, 0f);

            // Smoothly adjust camera eye height
            if (cameraTransform != null)
            {
                float targetCamY = IsCrouching ? crouchingCameraY : standingCameraY;
                Vector3 currentCamPos = cameraTransform.localPosition;
                cameraTransform.localPosition = new Vector3(
                    currentCamPos.x,
                    Mathf.Lerp(currentCamPos.y, targetCamY, Time.deltaTime * crouchTransitionSpeed),
                    currentCamPos.z
                );
            }
        }

        private void HandleMovement()
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");

            Vector3 moveDirection = (transform.right * inputX + transform.forward * inputZ).normalized;
            IsMoving = moveDirection.sqrMagnitude > 0.001f;

            // Determine speed
            if (IsCrouching)
            {
                IsSprinting = false;
                CurrentSpeed = crouchSpeed;
            }
            else
            {
                bool sprintInput = Input.GetKey(sprintKey);
                IsSprinting = IsMoving && sprintInput && inputZ > 0.1f && !isGasping;
                CurrentSpeed = IsSprinting ? sprintSpeed : walkSpeed;
            }

            Vector3 horizontalMove = moveDirection * (CurrentSpeed * Time.deltaTime);
            controller.Move(horizontalMove);
        }

        private void HandleGravity()
        {
            if (controller.isGrounded)
            {
                if (verticalVelocity.y < 0f)
                {
                    if (verticalVelocity.y < -5f)
                    {
                        // Dropped or jumped: emit landing noise event (GDD: 10m)
                        NoiseManager.Instance?.EmitNoise(transform.position, jumpNoiseRadius, 0.9f, "PlayerLand", true);
                    }
                    verticalVelocity.y = groundedStickForce;
                }
            }
            else
            {
                verticalVelocity.y += gravity * Time.deltaTime;
            }

            controller.Move(verticalVelocity * Time.deltaTime);
        }

        private void HandleBreathHold()
        {
            bool wantsHoldBreath = Input.GetKey(breathKey) && (!IsMoving || IsCrouching) && !isGasping;

            if (wantsHoldBreath)
            {
                IsHoldingBreath = true;
                currentBreathStamina -= Time.deltaTime;

                if (currentBreathStamina <= 0f)
                {
                    // Ran out of breath: gasp for air!
                    currentBreathStamina = 0f;
                    IsHoldingBreath = false;
                    isGasping = true;
                    gaspTimer = 2.0f;

                    // Emit loud gasp noise event (7m radius)
                    NoiseManager.Instance?.EmitNoise(transform.position, 7.0f, 0.8f, "PlayerGasp", true);
                    HUDController.Instance?.ShowNotification("Gasping for breath! (Noise emitted)", Color.red, 2.0f);
                }
            }
            else
            {
                IsHoldingBreath = false;
                if (isGasping)
                {
                    gaspTimer -= Time.deltaTime;
                    if (gaspTimer <= 0f)
                    {
                        isGasping = false;
                    }
                }
                else
                {
                    currentBreathStamina = Mathf.Min(currentBreathStamina + Time.deltaTime * breathRecoveryRate, maxBreathStamina);
                }
            }
        }

        private void HandleFootstepNoise()
        {
            if (IsMoving && controller.isGrounded)
            {
                float stepInterval = IsCrouching ? 0.65f : (IsSprinting ? 0.32f : 0.50f);
                footstepTimer += Time.deltaTime;

                if (footstepTimer >= stepInterval)
                {
                    footstepTimer = 0f;

                    // Play audio
                    if (!IsCrouching || !IsHoldingBreath)
                    {
                        Audio.HorrorAudioManager.Instance?.PlayFootstep(IsSprinting);
                    }

                    // Calculate noise radius based on gait and floor surface
                    float baseRadius = IsCrouching ? crouchNoiseRadius : (IsSprinting ? sprintNoiseRadius : walkNoiseRadius);

                    // Floor surface modifier check
                    float surfaceModifier = GetFloorSurfaceNoiseModifier();
                    float finalRadius = Mathf.Max(0.5f, baseRadius + surfaceModifier);

                    if (IsHoldingBreath && IsCrouching)
                    {
                        finalRadius *= 0.5f; // Extra suppression while holding breath
                    }

                    // Emit noise event to AI
                    NoiseManager.Instance?.EmitNoise(transform.position, finalRadius, IsSprinting ? 1.0f : 0.6f, "Footstep", true);
                }
            }
            else
            {
                footstepTimer = 0.15f;
            }
        }

        private float GetFloorSurfaceNoiseModifier()
        {
            // Raycast downward to check floor surface material or tag
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hit, 0.6f))
            {
                string colName = hit.collider.name.ToLower();
                if (colName.Contains("grate") || colName.Contains("metal") || colName.Contains("iron"))
                {
                    return 3.0f; // Metal grating bonus (+3m)
                }
                if (colName.Contains("carpet") || colName.Contains("office"))
                {
                    return -2.0f; // Carpet bonus (-2m)
                }
            }
            return 0f;
        }

        private void HandleHeadBob()
        {
            if (!enableHeadBob || cameraTransform == null) return;

            if (IsMoving && controller.isGrounded)
            {
                float speedMultiplier = IsCrouching ? 0.6f : (IsSprinting ? 1.4f : 1.0f);
                bobTimer += Time.deltaTime * bobFrequency * speedMultiplier;

                float baseY = IsCrouching ? crouchingCameraY : standingCameraY;
                float bobOffsetY = Mathf.Sin(bobTimer) * bobAmount;
                float bobOffsetX = Mathf.Cos(bobTimer * 0.5f) * (bobAmount * 0.5f);

                cameraTransform.localPosition = new Vector3(
                    defaultCameraLocalPos.x + bobOffsetX,
                    baseY + bobOffsetY,
                    defaultCameraLocalPos.z
                );
            }
            else
            {
                bobTimer = 0f;
            }
        }

        public void SetCameraTransform(Transform cam)
        {
            cameraTransform = cam;
            if (cameraTransform != null)
            {
                defaultCameraLocalPos = cameraTransform.localPosition;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject != null)
            {
                var exit = hit.gameObject.GetComponent<EscapeTheLab.Core.ExitTrigger>();
                if (exit == null && hit.collider != null)
                {
                    exit = hit.collider.GetComponent<EscapeTheLab.Core.ExitTrigger>();
                }
                if (exit != null)
                {
                    exit.TriggerVictory();
                }
            }
        }
    }
}
