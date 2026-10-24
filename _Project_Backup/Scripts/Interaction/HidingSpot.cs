using System.Collections;
using UnityEngine;
using EscapeTheLab.Player;
using EscapeTheLab.UI;
using EscapeTheLab.AI;

namespace EscapeTheLab.Interaction
{
    /// <summary>
    /// Interactive hiding spot (Locker, Cabinet, Supply Alcove) as specified in GDD Section 3.3.
    /// Entering/exiting takes 0.5 seconds.
    /// While hiding, the player is concealed from The Subject's vision unless spotted entering.
    /// </summary>
    public class HidingSpot : MonoBehaviour, IInteractable
    {
        [Header("Hiding Interior")]
        [Tooltip("Transform position inside the locker where the player camera sits.")]
        [SerializeField] private Transform interiorCameraPoint;

        [Tooltip("Transform position outside the locker where the player emerges.")]
        [SerializeField] private Transform exitPoint;

        [Header("Locker Door")]
        [Tooltip("Optional door pivot transform to open/close.")]
        [SerializeField] private Transform lockerDoor;
        [SerializeField] private float doorOpenAngle = -85f;

        private bool isPlayerInside = false;
        private bool isTransitioning = false;

        private FirstPersonMovement cachedPlayerMovement;
        private MouseLook cachedMouseLook;
        private Transform playerCameraTransform;
        private Vector3 originalCameraLocalPos;

        public bool IsOccupied => isPlayerInside;

        private void Start()
        {
            if (interiorCameraPoint == null)
            {
                interiorCameraPoint = transform;
            }
            if (exitPoint == null)
            {
                exitPoint = transform;
            }
        }

        public string GetPromptText()
        {
            if (isTransitioning) return "";
            return isPlayerInside ? "[E] Exit Hiding Spot" : "[E] Hide in Locker";
        }

        public bool CanInteract()
        {
            return !isTransitioning;
        }

        public void Interact(PlayerInteraction player)
        {
            if (isTransitioning) return;

            if (!isPlayerInside)
            {
                StartCoroutine(EnterHidingRoutine(player));
            }
            else
            {
                StartCoroutine(ExitHidingRoutine(player));
            }
        }

        private IEnumerator EnterHidingRoutine(PlayerInteraction player)
        {
            isTransitioning = true;
            isPlayerInside = true;

            cachedPlayerMovement = player.GetComponent<FirstPersonMovement>();
            cachedMouseLook = player.GetComponentInChildren<MouseLook>();
            Camera cam = player.GetComponentInChildren<Camera>();

            if (cam != null)
            {
                playerCameraTransform = cam.transform;
                originalCameraLocalPos = playerCameraTransform.localPosition;
            }

            // Disable player ground movement
            if (cachedPlayerMovement != null)
            {
                cachedPlayerMovement.enabled = false;
            }

            // Animate door open, move camera inside, animate door close (0.5s total)
            if (lockerDoor != null) lockerDoor.localRotation = Quaternion.Euler(0, doorOpenAngle, 0);

            float elapsed = 0f;
            Vector3 startPos = player.transform.position;
            Vector3 targetPos = interiorCameraPoint.position;

            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.5f);
                player.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            if (lockerDoor != null) lockerDoor.localRotation = Quaternion.identity;
            player.transform.position = targetPos;
            isTransitioning = false;

            HUDController.Instance?.ShowNotification("Hiding (Hold SPACE to hold breath)", Color.cyan, 2.5f);

            // Inform The Subject if nearby
            MonsterController subject = UnityEngine.Object.FindAnyObjectByType<MonsterController>();
            if (subject != null)
            {
                subject.NotifyPlayerEnteredHidingSpot(this);
            }
        }

        private IEnumerator ExitHidingRoutine(PlayerInteraction player)
        {
            isTransitioning = true;

            float elapsed = 0f;
            Vector3 startPos = player.transform.position;
            Vector3 targetPos = exitPoint.position;

            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.5f);
                player.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            player.transform.position = targetPos;

            // Re-enable player movement
            if (cachedPlayerMovement != null)
            {
                cachedPlayerMovement.enabled = true;
            }

            isPlayerInside = false;
            isTransitioning = false;

            HUDController.Instance?.ShowNotification("Exited hiding spot", Color.white, 1.5f);
        }
    }
}
