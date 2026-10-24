using System;
using UnityEngine;
using EscapeTheLab.UI;

namespace EscapeTheLab.Systems
{
    public enum ItemType
    {
        None,
        KeycardLevel1,
        KeycardLevel2,
        BatteryPack,
        MultiTool,
        SonicLure,
        DistractionBeaker
    }

    [System.Serializable]
    public class InventoryItem
    {
        public string ItemId;
        public string DisplayName;
        public ItemType Type;
        public string Description;
        public Color SlotColor;

        public InventoryItem(string id, string name, ItemType type, string description, Color color)
        {
            ItemId = id;
            DisplayName = name;
            Type = type;
            Description = description;
            SlotColor = color;
        }
    }

    /// <summary>
    /// Manages the player's 4-slot inventory system as specified in the GDD.
    /// Handles item selection (Keys 1-4), item dropping/throwing ('Q'),
    /// and contextual item consumption (Battery pack recharge, lure distraction).
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        public const int SlotCount = 4;

        [Header("Inventory State")]
        [SerializeField] private InventoryItem[] slots = new InventoryItem[SlotCount];
        [SerializeField] private int activeSlotIndex = 0;

        [Header("Drop & Throw Settings")]
        [Tooltip("Force applied when throwing a dropped distraction item forward.")]
        [SerializeField] private float throwForce = 7.5f;

        [Tooltip("Prefab for generic dropped distraction items (optional).")]
        [SerializeField] private GameObject genericDropPrefab;

        // Events
        public event Action OnInventoryChanged;
        public event Action<int> OnActiveSlotChanged;

        public int ActiveSlotIndex => activeSlotIndex;
        public InventoryItem ActiveItem => slots[activeSlotIndex];

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            HandleSlotSelectionInput();
            HandleDropInput();
        }

        private void HandleSlotSelectionInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        }

        private void HandleDropInput()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                DropActiveItem();
            }
        }

        public void SelectSlot(int index)
        {
            if (index >= 0 && index < SlotCount)
            {
                activeSlotIndex = index;
                OnActiveSlotChanged?.Invoke(activeSlotIndex);

                if (slots[activeSlotIndex] != null)
                {
                    HUDController.Instance?.ShowNotification(
                        "Selected: " + slots[activeSlotIndex].DisplayName,
                        slots[activeSlotIndex].SlotColor,
                        1.5f
                    );
                }
            }
        }

        /// <summary>
        /// Attempts to add an item to the first empty slot.
        /// </summary>
        public bool AddItem(InventoryItem item)
        {
            if (item == null) return false;

            for (int i = 0; i < SlotCount; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = item;
                    OnInventoryChanged?.Invoke();
                    HUDController.Instance?.ShowNotification("Picked up: " + item.DisplayName, item.SlotColor, 2.0f);
                    return true;
                }
            }

            HUDController.Instance?.ShowNotification("Inventory Full! (Press Q to drop)", Color.yellow, 2.0f);
            return false;
        }

        /// <summary>
        /// Drops or throws the item in the active slot into the world.
        /// </summary>
        public void DropActiveItem()
        {
            if (slots[activeSlotIndex] == null) return;

            InventoryItem itemToDrop = slots[activeSlotIndex];
            slots[activeSlotIndex] = null;
            OnInventoryChanged?.Invoke();

            Camera cam = Camera.main;
            Vector3 spawnPos = cam != null ? cam.transform.position + cam.transform.forward * 0.8f : transform.position + transform.forward * 1.0f;
            Vector3 throwDir = cam != null ? cam.transform.forward : transform.forward;

            // Spawn physical dropped item primitive with Rigidbody
            GameObject droppedObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            droppedObj.name = "Dropped_" + itemToDrop.ItemId;
            droppedObj.transform.position = spawnPos;
            droppedObj.transform.localScale = new Vector3(0.2f, 0.15f, 0.2f);

            // Add material color
            Renderer r = droppedObj.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = itemToDrop.SlotColor;
            }

            Rigidbody rb = droppedObj.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.AddForce((throwDir + Vector3.up * 0.2f) * throwForce, ForceMode.Impulse);

            // Attach distraction noise emitter on impact
            DroppedItemDistraction distraction = droppedObj.AddComponent<DroppedItemDistraction>();
            distraction.Setup(itemToDrop);

            HUDController.Instance?.ShowNotification("Dropped: " + itemToDrop.DisplayName, Color.white, 1.5f);
        }

        /// <summary>
        /// Checks if player currently possesses an item of the specified type.
        /// </summary>
        public bool HasItem(ItemType type)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (slots[i] != null && slots[i].Type == type)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the first instance of an item of the specified type from inventory.
        /// </summary>
        public bool ConsumeItem(ItemType type)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (slots[i] != null && slots[i].Type == type)
                {
                    slots[i] = null;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public InventoryItem GetSlot(int index)
        {
            if (index >= 0 && index < SlotCount)
            {
                return slots[index];
            }
            return null;
        }
    }
}
