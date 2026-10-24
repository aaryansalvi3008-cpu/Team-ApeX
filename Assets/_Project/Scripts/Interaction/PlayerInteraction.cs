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
        [SerializeField] private float reachDistance = 3.5f;

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
            EnsureCamera();
        }

        private void EnsureCamera()
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
            EnsureCamera();
            if (playerCamera == null) return;

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit[] hits = Physics.RaycastAll(ray, reachDistance, raycastMask, QueryTriggerInteraction.Collide);

            IInteractable foundInteractable = null;

            if (hits != null && hits.Length > 0)
            {
                // Sort by distance from camera
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                for (int i = 0; i < hits.Length; i++)
                {
                    var hit = hits[i];
                    // Skip self or child colliders of player
                    if (hit.collider == null || hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                        continue;

                    // Check if hit collider or its parents implement IInteractable
                    IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                    if (interactable == null)
                    {
                        interactable = hit.collider.GetComponent<IInteractable>();
                    }

                    if (interactable != null && interactable.CanInteract())
                    {
                        foundInteractable = interactable;
                        break;
                    }

                    // If we hit a solid non-trigger obstacle that is not part of any interactable, it blocks line of sight
                    if (!hit.collider.isTrigger)
                    {
                        break;
                    }
                }
            }

            // Fallback: If direct line raycast missed due to slight crosshair offset, try a narrow SphereCast (radius 0.28m)
            if (foundInteractable == null)
            {
                RaycastHit[] sphereHits = Physics.SphereCastAll(ray, 0.28f, reachDistance, raycastMask, QueryTriggerInteraction.Collide);
                if (sphereHits != null && sphereHits.Length > 0)
                {
                    System.Array.Sort(sphereHits, (a, b) => a.distance.CompareTo(b.distance));
                    for (int i = 0; i < sphereHits.Length; i++)
                    {
                        var hit = sphereHits[i];
                        if (hit.collider == null || hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                            continue;

                        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                        if (interactable == null)
                        {
                            interactable = hit.collider.GetComponent<IInteractable>();
                        }

                        if (interactable != null && interactable.CanInteract())
                        {
                            foundInteractable = interactable;
                            break;
                        }
                    }
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
