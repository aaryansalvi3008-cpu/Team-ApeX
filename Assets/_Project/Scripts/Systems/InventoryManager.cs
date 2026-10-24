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
        [SerializeField] private float throwForce = 13.5f;

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
            if (HUDController.Instance != null && HUDController.Instance.IsInUIMenu)
            {
                return;
            }

            HandleSlotSelectionInput();
            HandleUseInput();
            HandleDropOrThrowInput();
        }

        private void HandleSlotSelectionInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);

            // Mouse wheel scroll to cycle equipment hotbar
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.05f)
            {
                SelectSlot((activeSlotIndex - 1 + SlotCount) % SlotCount);
            }
            else if (scroll < -0.05f)
            {
                SelectSlot((activeSlotIndex + 1) % SlotCount);
            }
        }

        private void HandleUseInput()
        {
            // Left click or 'U' key to USE currently equipped item
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.U))
            {
                // Only use item if cursor is locked (active gameplay)
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    UseActiveItem();
                }
            }
        }

        private void HandleDropOrThrowInput()
        {
            // Right click or 'Q' key to THROW currently equipped item as a distraction projectile
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(1))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    ThrowActiveItem();
                }
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
                    string hint = GetActionHint(slots[activeSlotIndex]);
                    HUDController.Instance?.ShowNotification(
                        $"{slots[activeSlotIndex].DisplayName} {hint}",
                        slots[activeSlotIndex].SlotColor,
                        1.8f
                    );
                }
            }
        }

        private string GetActionHint(InventoryItem item)
        {
            switch (item.Type)
            {
                case ItemType.BatteryPack:
                    return "[Left Click / U: Use to Recharge | Q / Right Click: Throw]";
                case ItemType.SonicLure:
                    return "[Left Click: Deploy Siren | Q / Right Click: Throw Decoy]";
                case ItemType.DistractionBeaker:
                    return "[Q / Left / Right Click: Throw & Shatter Glass]";
                case ItemType.MultiTool:
                    return "[Left Click: Diagnostics | Q / Right Click: Throw]";
                default:
                    return "[Q / Right Click: Throw / Drop]";
            }
        }

        /// <summary>
        /// Contextually uses the currently equipped item.
        /// </summary>
        public void UseActiveItem()
        {
            if (slots[activeSlotIndex] == null) return;
            InventoryItem item = slots[activeSlotIndex];

            switch (item.Type)
            {
                case ItemType.BatteryPack:
                    Player.FlashlightController flashlight = UnityEngine.Object.FindAnyObjectByType<Player.FlashlightController>();
                    if (flashlight != null)
                    {
                        if (flashlight.BatteryRatio >= 0.98f)
                        {
                            HUDController.Instance?.ShowNotification("Flashlight battery is already at 100%!", Color.yellow, 2.0f);
                            return;
                        }

                        flashlight.RechargeBattery(0.60f); // Recharges +60%
                        Audio.HorrorAudioManager.Instance?.PlayFlashlightClick();
                        ConsumeActiveItem();
                    }
                    break;

                case ItemType.SonicLure:
                    DeployActiveSonicLure(item);
                    ConsumeActiveItem();
                    break;

                case ItemType.DistractionBeaker:
                    ThrowActiveItem();
                    break;

                case ItemType.MultiTool:
                    Audio.HorrorAudioManager.Instance?.PlayKeycardChime();
                    HUDController.Instance?.ShowNotification("Multi-Tool: Aim at locked breaker consoles or use to pry doors.", Color.cyan, 2.5f);
                    break;

                case ItemType.KeycardLevel1:
                case ItemType.KeycardLevel2:
                    Audio.HorrorAudioManager.Instance?.PlayKeycardChime();
                    HUDController.Instance?.ShowNotification(item.DisplayName + ": Use at locked security blast doors & terminals.", Color.cyan, 2.0f);
                    break;
            }
        }

        /// <summary>
        /// Deploys an armed acoustic sonic lure directly on the floor.
        /// </summary>
        private void DeployActiveSonicLure(InventoryItem item)
        {
            Camera cam = Camera.main;
            Vector3 spawnPos = cam != null ? cam.transform.position + cam.transform.forward * 1.2f : transform.position + transform.forward * 1.2f;
            spawnPos.y = Mathf.Max(0.1f, spawnPos.y - 0.8f);

            GameObject lureObj = CreateItemVisual(item, spawnPos);
            lureObj.name = "Active_Sonic_Lure";

            Rigidbody rb = lureObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.down * 2f;
            }

            DroppedItemDistraction distraction = lureObj.AddComponent<DroppedItemDistraction>();
            distraction.Setup(item);

            Audio.HorrorAudioManager.Instance?.PlayKeycardChime();
            HUDController.Instance?.ShowNotification("Sonic Lure armed & deployed! Emitting acoustic beacon.", Color.cyan, 2.5f);
        }

        /// <summary>
        /// Drops or throws the item in the active slot into the world with physics arc.
        /// </summary>
        public void DropActiveItem()
        {
            ThrowActiveItem();
        }

        /// <summary>
        /// Throws the active item forward with physics trajectory and spinning angular velocity.
        /// </summary>
        public void ThrowActiveItem()
        {
            if (slots[activeSlotIndex] == null) return;

            InventoryItem itemToThrow = slots[activeSlotIndex];
            ConsumeActiveItem();

            Camera cam = Camera.main;
            Vector3 spawnPos = cam != null ? cam.transform.position + cam.transform.forward * 0.7f : transform.position + transform.forward * 1.0f + Vector3.up * 1.2f;
            Vector3 throwDir = cam != null ? cam.transform.forward : transform.forward;

            GameObject thrownObj = CreateItemVisual(itemToThrow, spawnPos);
            thrownObj.name = "Thrown_" + itemToThrow.ItemId;

            Rigidbody rb = thrownObj.GetComponent<Rigidbody>();
            if (rb == null) rb = thrownObj.AddComponent<Rigidbody>();
            rb.mass = 0.6f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Apply forward propulsion with arc
            Vector3 impulse = (throwDir * throwForce) + (Vector3.up * 2.8f);
            rb.linearVelocity = impulse;
            rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 14.0f;

            // Attach distraction noise impact handler
            DroppedItemDistraction distraction = thrownObj.AddComponent<DroppedItemDistraction>();
            distraction.Setup(itemToThrow);

            Audio.HorrorAudioManager.Instance?.PlayThrowWhoosh();
            HUDController.Instance?.ShowNotification("Thrown: " + itemToThrow.DisplayName, Color.white, 1.5f);
        }

        private GameObject CreateItemVisual(InventoryItem item, Vector3 spawnPos)
        {
            GameObject obj;

            switch (item.Type)
            {
                case ItemType.DistractionBeaker:
                    // Glass laboratory beaker / flask visual
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    obj.transform.position = spawnPos;
                    obj.transform.localScale = new Vector3(0.22f, 0.28f, 0.22f);
                    Renderer rBeaker = obj.GetComponent<Renderer>();
                    if (rBeaker != null)
                    {
                        rBeaker.material.color = new Color(0.6f, 0.9f, 1.0f, 0.85f);
                    }
                    break;

                case ItemType.BatteryPack:
                    // Heavy cylindrical industrial battery cell
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    obj.transform.position = spawnPos;
                    obj.transform.localScale = new Vector3(0.18f, 0.22f, 0.18f);
                    Renderer rBatt = obj.GetComponent<Renderer>();
                    if (rBatt != null) rBatt.material.color = Color.yellow;

                    // Small indicator glow
                    GameObject glow = new GameObject("BattGlow");
                    glow.transform.parent = obj.transform;
                    glow.transform.localPosition = Vector3.zero;
                    Light lt = glow.AddComponent<Light>();
                    lt.type = LightType.Point;
                    lt.color = Color.yellow;
                    lt.range = 2.5f;
                    lt.intensity = 0.8f;
                    break;

                case ItemType.SonicLure:
                    // High-tech sound emitter canister
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    obj.transform.position = spawnPos;
                    obj.transform.localScale = new Vector3(0.25f, 0.18f, 0.25f);
                    Renderer rLure = obj.GetComponent<Renderer>();
                    if (rLure != null) rLure.material.color = Color.cyan;
                    break;

                case ItemType.MultiTool:
                    // Metallic utility tool
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.position = spawnPos;
                    obj.transform.localScale = new Vector3(0.12f, 0.08f, 0.35f);
                    Renderer rTool = obj.GetComponent<Renderer>();
                    if (rTool != null) rTool.material.color = new Color(0.7f, 0.75f, 0.8f);
                    break;

                default:
                    // Security keycards or generic item
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.position = spawnPos;
                    obj.transform.localScale = new Vector3(0.35f, 0.04f, 0.22f);
                    Renderer rGen = obj.GetComponent<Renderer>();
                    if (rGen != null) rGen.material.color = item.SlotColor;
                    break;
            }

            // Ensure collider & physical rigidbody
            Collider col = obj.GetComponent<Collider>();
            if (col != null) col.isTrigger = false;

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null) rb = obj.AddComponent<Rigidbody>();
            rb.mass = 0.5f;

            return obj;
        }

        private void ConsumeActiveItem()
        {
            slots[activeSlotIndex] = null;
            OnInventoryChanged?.Invoke();
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

            HUDController.Instance?.ShowNotification("Inventory Full! (Press Q to drop/throw)", Color.yellow, 2.0f);
            return false;
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
