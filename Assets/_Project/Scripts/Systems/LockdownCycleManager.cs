using System;
using System.Collections;
using UnityEngine;
using EscapeTheLab.Player;
using EscapeTheLab.UI;
using EscapeTheLab.AI;

namespace EscapeTheLab.Systems
{
    /// <summary>
    /// Manages the 5 Lockdown Cycles defined in GDD Section 1.1 & 2.2.
    /// Each time the player is caught by The Subject:
    /// - Screen static cut and blackout alarm occurs.
    /// - Lockdown cycle counter increments (1 to 5).
    /// - Player respawns safely in the Medical Bay.
    /// - The Subject resets to its containment cell.
    /// - If 5 cycles expire, total facility lockdown occurs (Permanent Game Over).
    /// </summary>
    public class LockdownCycleManager : MonoBehaviour
    {
        public static LockdownCycleManager Instance { get; private set; }

        [Header("Cycle Configuration")]
        [Tooltip("Max cycles permitted before total quarantine lockout.")]
        [SerializeField] private int maxCycles = 5;

        [Header("Cycle State")]
        [SerializeField] private int currentCycle = 1;

        [Header("Spawn Locations")]
        [Tooltip("Transform position where player wakes up each cycle.")]
        [SerializeField] private Transform medicalBaySpawnPoint;

        [Tooltip("Transform position where The Subject resets each cycle.")]
        [SerializeField] private Transform subjectContainmentSpawnPoint;

        // Events
        public event Action<int> OnCycleChanged;

        public int CurrentCycle => currentCycle;
        public int MaxCycles => maxCycles;
        public bool IsLastCycle => currentCycle >= maxCycles;

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

        private void Start()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName == "LaboratoryLevel2")
            {
                // Level 2 (Sub-Basement Cryo): High-security protocol allows only 4 cycles!
                maxCycles = 4;
            }

            HUDController.Instance?.SetCycle(currentCycle, maxCycles);
        }

        /// <summary>
        /// Triggered when The Subject catches the technician.
        /// </summary>
        public void HandlePlayerCaught()
        {
            StartCoroutine(CaughtCycleSequence());
        }

        private IEnumerator CaughtCycleSequence()
        {
            HUDController.Instance?.TriggerStaticGlitch(1.8f);

            // Play distant alarm klaxon
            Audio.HorrorAudioManager.Instance?.PlayMonsterRoar();

            yield return new WaitForSeconds(1.8f);

            currentCycle++;
            OnCycleChanged?.Invoke(currentCycle);
            HUDController.Instance?.SetCycle(currentCycle, MaxCycles);

            if (currentCycle > MaxCycles)
            {
                // All 5 cycles depleted: Total Facility Lockdown!
                HUDController.Instance?.ShowGameOver();
            }
            else
            {
                // Respawn player in Medical Bay
                RespawnCycle();
            }
        }

        private void RespawnCycle()
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");

            if (playerObj != null && medicalBaySpawnPoint != null)
            {
                CharacterController cc = playerObj.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                playerObj.transform.position = medicalBaySpawnPoint.position;
                playerObj.transform.rotation = medicalBaySpawnPoint.rotation;

                if (cc != null) cc.enabled = true;

                FirstPersonMovement fpm = playerObj.GetComponent<FirstPersonMovement>();
                if (fpm != null) fpm.enabled = true;
            }

            // Reset The Subject to its containment cell
            MonsterController subject = UnityEngine.Object.FindAnyObjectByType<MonsterController>();
            if (subject != null && subjectContainmentSpawnPoint != null)
            {
                subject.ResetToContainment(subjectContainmentSpawnPoint.position);
            }

            HUDController.Instance?.ShowNotification(
                $"LOCKDOWN CYCLE {currentCycle} / {MaxCycles} INITIATED - BLACKOUT CONTINUES",
                Color.yellow,
                4.0f
            );
        }

        public void SetupSpawnPoints(Transform medBay, Transform containment)
        {
            medicalBaySpawnPoint = medBay;
            subjectContainmentSpawnPoint = containment;
        }

        public void SetMaxCycles(int count)
        {
            maxCycles = Mathf.Max(1, count);
            HUDController.Instance?.SetCycle(currentCycle, maxCycles);
        }
    }
}
