using UnityEngine;

namespace EscapeTheLab.Player
{
    /// <summary>
    /// Controls the first-person camera rotation via mouse input.
    /// Clamps vertical rotation (pitch) and rotates the player body horizontally (yaw).
    /// Handles cursor locking and unlocking.
    /// </summary>
    public class MouseLook : MonoBehaviour
    {
        [Header("Sensitivity Settings")]
        [Tooltip("Horizontal and vertical mouse look sensitivity.")]
        [SerializeField] private float mouseSensitivity = 180f;

        [Header("References")]
        [Tooltip("The root transform of the player character to rotate horizontally.")]
        [SerializeField] private Transform playerBody;

        [Header("Clamping Limits")]
        [Tooltip("Lowest angle (looking down) in degrees.")]
        [SerializeField] private float minPitch = -85f;

        [Tooltip("Highest angle (looking up) in degrees.")]
        [SerializeField] private float maxPitch = 85f;

        [Header("Cursor Settings")]
        [Tooltip("Whether to lock and hide cursor on start.")]
        [SerializeField] private bool lockCursorOnStart = true;

        private float verticalRotation = 0f;

        private void Awake()
        {
            if (playerBody == null && transform.parent != null)
            {
                playerBody = transform.parent;
            }
        }

        private void Start()
        {
            verticalRotation = transform.localEulerAngles.x;
            if (verticalRotation > 180f) verticalRotation -= 360f;

            if (lockCursorOnStart)
            {
                LockCursor();
            }
        }

        private void Update()
        {
            HandleCursorLockToggle();

            // Only process mouse look if the cursor is locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            // Read raw mouse movement scaled by sensitivity and delta time
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Pitch: Invert mouseY to match standard FPS camera controls
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, minPitch, maxPitch);

            // Apply pitch rotation to the camera transform
            transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

            // Yaw: Rotate the player root body around Y-axis
            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * mouseX);
            }
        }

        /// <summary>
        /// Locks and hides the cursor for gameplay.
        /// </summary>
        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Unlocks and shows the cursor (e.g. for menus or UI).
        /// </summary>
        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandleCursorLockToggle()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnlockCursor();
            }
            else if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }

        public void SetPlayerBody(Transform body)
        {
            playerBody = body;
        }
    }
}
