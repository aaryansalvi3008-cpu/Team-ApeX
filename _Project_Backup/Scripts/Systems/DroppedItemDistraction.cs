using UnityEngine;
using EscapeTheLab.Interaction;

namespace EscapeTheLab.Systems
{
    /// <summary>
    /// Attached to dropped items in the physical world.
    /// Emits a noise event upon impacting the floor/walls, serving as a tactical distraction.
    /// Also allows the player to pick the item back up.
    /// </summary>
    public class DroppedItemDistraction : MonoBehaviour, IInteractable
    {
        private InventoryItem itemData;
        private bool hasCollided = false;

        public void Setup(InventoryItem item)
        {
            itemData = item;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (hasCollided) return;
            hasCollided = true;

            // Distraction sound radius: 12 meters for throw impact (pulls The Subject away)
            float noiseRadius = (itemData != null && itemData.Type == ItemType.SonicLure) ? 22f : 12f;
            NoiseManager.Instance?.EmitNoise(transform.position, noiseRadius, 1.0f, "DroppedItemImpact");

            // Play collision sound
            Audio.HorrorAudioManager.Instance?.PlayKeycardChime();
        }

        public string GetPromptText()
        {
            return itemData != null ? "[E] Pick up " + itemData.DisplayName : "[E] Pick up Item";
        }

        public bool CanInteract()
        {
            return true;
        }

        public void Interact(PlayerInteraction player)
        {
            if (itemData != null && InventoryManager.Instance != null)
            {
                if (InventoryManager.Instance.AddItem(itemData))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
