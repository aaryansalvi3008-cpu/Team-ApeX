using UnityEngine;
using EscapeTheLab.UI;

namespace EscapeTheLab.Interaction
{
    /// <summary>
    /// Attached to the Player or Player Camera.
    /// Casts a raycast forward through the crosshair to detect IInteractable objects.
    /// Manages interaction input ('E') and player inventory state (e.g. keycard possession).
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [Tooltip("The camera used as the origin and forward direction of the interaction ray.")]
        [SerializeField] private Camera playerCamera;

        [Tooltip("Maximum reach distance in meters to interact with objects.")]
        [SerializeField] private float reachDistance = 2.8f;

        [Tooltip("Layers that can block interaction rays.")]
        [SerializeField] private LayerMask raycastMask = ~0; // Everything by default

        [Header("Controls")]
        [Tooltip("Key used to interact with world objects.")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("Inventory State")]
        [Tooltip("Whether the player currently possesses the facility security keycard.")]
        public bool HasKeycard = false;

        private IInteractable currentInteractable = null;

        private void Awake()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null)
                {
                    playerCamera = Camera.main;
                }
            }
        }

        private void Update()
        {
            PerformRaycast();
            HandleInteractionInput();
        }

        private void PerformRaycast()
        {
            if (playerCamera == null) return;

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            IInteractable foundInteractable = null;

            if (Physics.Raycast(ray, out hit, reachDistance, raycastMask))
            {
                // Look for IInteractable on the hit object or in its parents
                foundInteractable = hit.collider.GetComponentInParent<IInteractable>();
                if (foundInteractable == null)
                {
                    foundInteractable = hit.collider.GetComponent<IInteractable>();
                }
            }

            // Update interactable reference & HUD prompt
            if (foundInteractable != null && foundInteractable.CanInteract())
            {
                currentInteractable = foundInteractable;
                if (HUDController.Instance != null)
                {
                    HUDController.Instance.SetPrompt(currentInteractable.GetPromptText());
                }
            }
            else
            {
                currentInteractable = null;
                if (HUDController.Instance != null)
                {
                    HUDController.Instance.SetPrompt(null);
                }
            }
        }

        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(interactKey))
            {
                if (currentInteractable != null)
                {
                    currentInteractable.Interact(this);
                }
                else
                {
                    // Contextual item use from active inventory slot (e.g. Battery Pack)
                    TryUseActiveInventoryItem();
                }
            }
        }

        private void TryUseActiveInventoryItem()
        {
            if (EscapeTheLab.Systems.InventoryManager.Instance == null) return;

            var activeItem = EscapeTheLab.Systems.InventoryManager.Instance.ActiveItem;
            if (activeItem == null) return;

            if (activeItem.Type == EscapeTheLab.Systems.ItemType.BatteryPack)
            {
                var flashlight = GetComponentInChildren<EscapeTheLab.Player.FlashlightController>();
                if (flashlight != null)
                {
                    flashlight.RechargeBattery(0.6f);
                    EscapeTheLab.Systems.InventoryManager.Instance.ConsumeItem(EscapeTheLab.Systems.ItemType.BatteryPack);
                }
            }
        }

        public void SetCamera(Camera cam)
        {
            playerCamera = cam;
        }
    }
}
