using UnityEngine;
using EscapeTheLab.Systems;
using EscapeTheLab.UI;

namespace EscapeTheLab.Interaction
{
    /// <summary>
    /// Interactive world pickup for inventory items:
    /// - Keycards (Level 1, Level 2, Level 3)
    /// - Battery Packs (recharges flashlight)
    /// - Sonic Lures (distraction noisemakers)
    /// - Multi-tool (breaker/hack bypass)
    /// Gently bobs and rotates with an emissive aura.
    /// </summary>
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        [Header("Item Definition")]
        [SerializeField] private string itemId = "battery_pack";
        [SerializeField] private string displayName = "Flashlight Battery";
        [SerializeField] private ItemType itemType = ItemType.BatteryPack;
        [SerializeField] private string description = "Restores flashlight battery power.";
        [SerializeField] private Color itemColor = Color.yellow;

        [Header("Hover Animation")]
        [SerializeField] private bool enableHover = true;
        [SerializeField] private float rotationSpeed = 45f;
        [SerializeField] private float bobAmplitude = 0.035f;
        [SerializeField] private float bobFrequency = 2.2f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pickupClip;

        private Vector3 startPosition;
        private bool isCollected = false;

        private void Start()
        {
            startPosition = transform.position;
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (isCollected || !enableHover) return;

            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            float newY = startPosition.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }

        public string GetPromptText()
        {
            return $"[E] Take {displayName}";
        }

        public bool CanInteract()
        {
            return !isCollected;
        }

        public void Interact(PlayerInteraction player)
        {
            if (isCollected) return;

            InventoryItem item = new InventoryItem(itemId, displayName, itemType, description, itemColor);

            if (InventoryManager.Instance != null)
            {
                if (InventoryManager.Instance.AddItem(item))
                {
                    Collect();
                    if (itemType == ItemType.KeycardLevel1 || itemType == ItemType.KeycardLevel2)
                    {
                        player.HasKeycard = true;
                    }
                }
            }
            else
            {
                // Fallback direct keycard possession
                player.HasKeycard = true;
                Collect();
            }
        }

        private void Collect()
        {
            isCollected = true;

            // Audio feedback
            if (audioSource != null && pickupClip != null)
            {
                audioSource.PlayOneShot(pickupClip);
            }
            else
            {
                Audio.HorrorAudioManager.Instance?.PlayKeycardChime();
            }

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) r.enabled = false;

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Destroy(gameObject, 0.4f);
        }

        public void SetupItem(string id, string name, ItemType type, Color color)
        {
            itemId = id;
            displayName = name;
            itemType = type;
            itemColor = color;
        }

        public void Configure(ItemType type, string name, Color color)
        {
            SetupItem(name.ToLower().Replace(" ", "_"), name, type, color);
        }
    }
}
