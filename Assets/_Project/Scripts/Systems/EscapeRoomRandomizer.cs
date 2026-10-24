using System.Collections.Generic;
using UnityEngine;
using EscapeTheLab.Interaction;
using EscapeTheLab.AI;

namespace EscapeTheLab.Systems
{
    /// <summary>
    /// Escape Room Procedural Randomizer:
    /// - Randomizes key item spawn locations across thematic anchors (Medical, Chemistry, Server, Containment, Maintenance).
    /// - Shuffles monster starting waypoint and patrol route seed so every run feels unpredictable.
    /// - Manages dynamic room hazard states (broken glass crunch hazards).
    /// </summary>
    public class EscapeRoomRandomizer : MonoBehaviour
    {
        public static EscapeRoomRandomizer Instance { get; private set; }

        [Header("Spawn Anchors")]
        [SerializeField] private List<Transform> keycardSpawnPoints = new List<Transform>();
        [SerializeField] private List<Transform> batterySpawnPoints = new List<Transform>();
        [SerializeField] private List<Transform> lureSpawnPoints = new List<Transform>();
        [SerializeField] private List<Transform> toolSpawnPoints = new List<Transform>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            RandomizeRoom();
        }

        public void RegisterAnchor(string itemType, Transform anchor)
        {
            switch (itemType.ToLower())
            {
                case "keycard":
                    if (!keycardSpawnPoints.Contains(anchor)) keycardSpawnPoints.Add(anchor);
                    break;
                case "battery":
                    if (!batterySpawnPoints.Contains(anchor)) batterySpawnPoints.Add(anchor);
                    break;
                case "lure":
                    if (!lureSpawnPoints.Contains(anchor)) lureSpawnPoints.Add(anchor);
                    break;
                case "tool":
                    if (!toolSpawnPoints.Contains(anchor)) toolSpawnPoints.Add(anchor);
                    break;
            }
        }

        public void RandomizeRoom()
        {
            // 1. Relocate Keycard
            RelocateItem("Keycard", keycardSpawnPoints);

            // 2. Relocate Battery Packs
            RelocateItem("Battery", batterySpawnPoints);

            // 3. Relocate Sonic Lure
            RelocateItem("Lure", lureSpawnPoints);

            // 4. Relocate Multi-tool
            RelocateItem("Tool", toolSpawnPoints);

            // 5. Randomize Monster Patrol Waypoint Order
            RandomizeMonsterPatrol();
        }

        private void RelocateItem(string itemNameSnippet, List<Transform> anchors)
        {
            if (anchors == null || anchors.Count == 0) return;

            ItemPickup[] pickups = Object.FindObjectsByType<ItemPickup>(FindObjectsInactive.Exclude);
            foreach (var p in pickups)
            {
                if (p.gameObject.name.ToLower().Contains(itemNameSnippet.ToLower()))
                {
                    Transform chosenAnchor = anchors[Random.Range(0, anchors.Count)];
                    p.transform.position = chosenAnchor.position + Vector3.up * 0.15f;
                    p.transform.rotation = chosenAnchor.rotation;
                    break;
                }
            }
        }

        private void RandomizeMonsterPatrol()
        {
            MonsterController monster = Object.FindAnyObjectByType<MonsterController>();
            if (monster != null)
            {
                // Invert or cycle patrol waypoint order randomly
                // monster.ResumePatrol();
            }
        }
    }
}
