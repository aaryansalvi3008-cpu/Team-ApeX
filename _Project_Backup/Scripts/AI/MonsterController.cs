using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EscapeTheLab.Player;
using EscapeTheLab.UI;
using EscapeTheLab.Systems;
using EscapeTheLab.Interaction;

namespace EscapeTheLab.AI
{
    /// <summary>
    /// The Subject (Primary Threat) - Failed Bio-Engineering Experiment (GDD Section 4):
    /// - Nearly blind (60° FOV, 6m range), but acute hearing via NoiseManager.
    /// - States:
    ///   * PATROL (1.8 m/s): Wanders between facility sectors. Drags claws, wheezes.
    ///   * INVESTIGATE (2.5 m/s): Moves to position of last heard noise event; sniffs and clicks.
    ///   * HUNT (4.5 m/s): Shrieks and sprints directly towards detected technician!
    ///   * SEARCH (2.5 m/s): Checks area/hiding spots for 5-10 seconds before resuming patrol.
    ///   * STUNNED (0 m/s): Frozen briefly if hit by trap/lure.
    /// - 8-second memory of last known position.
    /// - Catches player -> triggers LockdownCycleManager cycle transition.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MonsterController : MonoBehaviour
    {
        public enum AIState { Patrol, Investigate, Hunt, Chase = Hunt, Search, Stunned }

        [Header("Current State")]
        [SerializeField] private AIState currentState = AIState.Patrol;

        [Header("Speeds (Tuned for Granny-Style Suspense)")]
        [Tooltip("Patrol speed: 1.15 m/s (creeping stalking pace)")]
        [SerializeField] private float patrolSpeed = 1.15f;

        [Tooltip("Investigate speed: 1.75 m/s")]
        [SerializeField] private float investigateSpeed = 1.75f;

        [Tooltip("Hunt/Sprint speed: 3.25 m/s (fast enough to pressure, slow enough to allow escape sprint)")]
        [SerializeField] private float huntSpeed = 3.25f;

        [Tooltip("Search speed: 1.5 m/s")]
        [SerializeField] private float searchSpeed = 1.5f;

        [Tooltip("Acceleration for turning and rushing.")]
        [SerializeField] private float huntAcceleration = 8.5f;

        [Header("Sensory & Detection (GDD Section 4.4)")]
        [Tooltip("Weak vision range in standard lighting: 6m.")]
        [SerializeField] private float visionRange = 6.0f;

        [Tooltip("Vision range in pitch blackness (when Lights circuit is cut): 3m.")]
        [SerializeField] private float blackoutVisionRange = 3.0f;

        [Tooltip("Vision range when player shines flashlight directly: 14m.")]
        [SerializeField] private float flashlightVisionRange = 14.0f;

        [Tooltip("Field of view angle in degrees (GDD: 60°).")]
        [SerializeField] private float fieldOfViewAngle = 60.0f;

        [Tooltip("Memory duration of last-known player position (GDD: 8s).")]
        [SerializeField] private float memoryDuration = 8.0f;

        [Tooltip("Time spent searching hiding spots/area before giving up (GDD: 5-10s).")]
        [SerializeField] private float searchDurationMin = 5.0f;
        [SerializeField] private float searchDurationMax = 9.0f;

        [Header("Kill Range")]
        [Tooltip("Distance at which The Subject catches the player.")]
        [SerializeField] private float killDistance = 1.6f;

        [Header("Waypoints")]
        [SerializeField] private List<Transform> patrolWaypoints = new List<Transform>();
        [SerializeField] private float waypointWaitTime = 2.0f;

        [Header("Visual & Audio Feedback")]
        [SerializeField] private Light eyeGlowLight;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip roarClip;
        [SerializeField] private AudioClip catchClip;

        // References
        private NavMeshAgent agent;
        private Transform playerTransform;
        private FirstPersonMovement playerMovement;
        private FlashlightController playerFlashlight;

        // Internal State
        private int currentWaypointIndex = 0;
        private float waitTimer = 0f;
        private float memoryTimer = 0f;
        private float searchTimer = 0f;
        private Vector3 targetInvestigatePos;
        private bool hasCaughtPlayerInCycle = false;
        private HidingSpot suspectedHidingSpot = null;

        public AIState CurrentState => currentState;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            FindPlayerReferences();

            // Ensure monster is snapped to NavMesh
            if (agent != null && !agent.isOnNavMesh)
            {
                if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }

            agent.speed = patrolSpeed;
            agent.stoppingDistance = 0.5f;

            // Subscribe to Noise Propagation Manager
            if (NoiseManager.Instance != null)
            {
                NoiseManager.Instance.OnNoiseEmitted += HandleNoiseEvent;
            }

            if (patrolWaypoints.Count > 0)
            {
                GoToNextWaypoint();
            }
        }

        private void OnDestroy()
        {
            if (NoiseManager.Instance != null)
            {
                NoiseManager.Instance.OnNoiseEmitted -= HandleNoiseEvent;
            }
        }

        private void FindPlayerReferences()
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");

            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerMovement = playerObj.GetComponent<FirstPersonMovement>();
                playerFlashlight = playerObj.GetComponentInChildren<FlashlightController>();
            }
        }

        private void Update()
        {
            if (hasCaughtPlayerInCycle) return;

            if (agent == null) return;
            if (!agent.isOnNavMesh)
            {
                if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
                else
                {
                    return;
                }
            }

            if (playerTransform == null)
            {
                FindPlayerReferences();
                return;
            }

            // Check Weak Vision Detection
            bool canSeePlayer = CheckWeakVision();

            if (canSeePlayer)
            {
                targetInvestigatePos = playerTransform.position;
                memoryTimer = memoryDuration;

                if (currentState != AIState.Hunt)
                {
                    StartHunt();
                }
            }

            // State Machine Execution
            switch (currentState)
            {
                case AIState.Patrol:
                    UpdatePatrol();
                    break;
                case AIState.Investigate:
                    UpdateInvestigate();
                    break;
                case AIState.Hunt:
                    UpdateHunt(canSeePlayer);
                    break;
                case AIState.Search:
                    UpdateSearch();
                    break;
                case AIState.Stunned:
                    // Frozen
                    break;
            }

            // Proximity feedback to HUD
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (HUDController.Instance != null)
            {
                HUDController.Instance.ReportMonsterDistance(distToPlayer, currentState == AIState.Hunt);
            }

            if (distToPlayer <= killDistance && canSeePlayer && currentState == AIState.Hunt)
            {
                CatchPlayer();
            }
        }

        #region Sensory Detection (GDD Section 4.4)

        private bool CheckWeakVision()
        {
            if (playerTransform == null) return false;

            // Determine effective range based on lights circuit & player flashlight
            bool lightsOn = PowerGridManager.Instance == null || PowerGridManager.Instance.LightsActive;
            bool flashlightOn = playerFlashlight != null && playerFlashlight.IsOn;

            float effectiveRange = visionRange;
            if (flashlightOn) effectiveRange = flashlightVisionRange;
            else if (!lightsOn) effectiveRange = blackoutVisionRange;

            Vector3 eyePos = transform.position + Vector3.up * 1.8f;
            Vector3 targetPos = playerTransform.position + Vector3.up * 1.4f;
            Vector3 dir = targetPos - eyePos;
            float dist = dir.magnitude;

            if (dist > effectiveRange) return false;

            // Check 60° FOV cone (GDD Appendix A)
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle > (fieldOfViewAngle * 0.5f) && !flashlightOn)
            {
                return false;
            }

            // If player is crouching still in darkness without flashlight, The Subject overlooks them
            if (playerMovement != null && playerMovement.IsCrouching && !playerMovement.IsMoving && !flashlightOn && dist > 2.5f)
            {
                return false;
            }

            // Raycast for solid walls/obstacles
            if (Physics.Raycast(eyePos, dir.normalized, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.CompareTag("Player") || hit.transform.root == playerTransform)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Acoustic listener: reacts to noise events propagated through NoiseManager.
        /// Accounts for 50% wall attenuation.
        /// </summary>
        private void HandleNoiseEvent(NoiseEvent evt)
        {
            if (hasCaughtPlayerInCycle || currentState == AIState.Hunt) return;

            if (NoiseManager.Instance.CanHearNoise(evt, transform.position, out float effectiveRadius, out float perceivedDist))
            {
                targetInvestigatePos = evt.Position;

                // If sound was very close (within 4m), jump immediately to Hunt!
                if (perceivedDist <= 4.0f)
                {
                    StartHunt();
                }
                else
                {
                    StartInvestigate(targetInvestigatePos);
                }
            }
        }

        #endregion

        #region State Transitions & Updates

        private void StartInvestigate(Vector3 soundPos)
        {
            currentState = AIState.Investigate;
            agent.speed = investigateSpeed;
            if (agent.isOnNavMesh) agent.SetDestination(soundPos);
            searchTimer = Random.Range(searchDurationMin, searchDurationMax);

            if (eyeGlowLight != null)
            {
                eyeGlowLight.color = new Color(1f, 0.6f, 0.1f); // Amber
                eyeGlowLight.intensity = 1.3f;
            }
        }

        private void UpdateInvestigate()
        {
            agent.speed = investigateSpeed;

            if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= 1.2f)
            {
                // Reached noise position: search area
                searchTimer -= Time.deltaTime;
                if (searchTimer <= 0f)
                {
                    ResumePatrol();
                }
            }
        }

        private void StartHunt()
        {
            currentState = AIState.Hunt;
            agent.speed = huntSpeed;
            agent.acceleration = huntAcceleration;

            if (audioSource != null && roarClip != null)
            {
                audioSource.PlayOneShot(roarClip);
            }
            else
            {
                Audio.HorrorAudioManager.Instance?.PlayMonsterRoar();
            }

            if (eyeGlowLight != null)
            {
                eyeGlowLight.color = new Color(1f, 0.05f, 0.05f); // Crimson
                eyeGlowLight.intensity = 2.4f;
            }

            HUDController.Instance?.ShowNotification("THE SUBJECT HAS SPOTTED YOU! BREAK LINE OF SIGHT!", new Color(1f, 0.15f, 0.15f), 2.5f);
        }

        private void UpdateHunt(bool canSeePlayer)
        {
            if (canSeePlayer)
            {
                if (agent.isOnNavMesh) agent.SetDestination(playerTransform.position);
                memoryTimer = memoryDuration;
            }
            else
            {
                // Lost direct sight: move towards last known position using 8-second memory
                memoryTimer -= Time.deltaTime;
                if (agent.isOnNavMesh) agent.SetDestination(targetInvestigatePos);

                if (memoryTimer <= 0f || (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= 1.2f))
                {
                    StartSearch();
                }
            }
        }

        private void StartSearch()
        {
            currentState = AIState.Search;
            agent.speed = searchSpeed;
            searchTimer = Random.Range(searchDurationMin, searchDurationMax);

            if (eyeGlowLight != null)
            {
                eyeGlowLight.color = new Color(0.9f, 0.4f, 0.1f);
                eyeGlowLight.intensity = 1.2f;
            }
        }

        private void UpdateSearch()
        {
            searchTimer -= Time.deltaTime;

            // If a hiding spot was suspected nearby, inspect it
            if (suspectedHidingSpot != null)
            {
                if (agent.isOnNavMesh) agent.SetDestination(suspectedHidingSpot.transform.position);
                if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= 1.8f)
                {
                    // Check if player is inside
                    if (suspectedHidingSpot.IsOccupied)
                    {
                        CatchPlayer();
                        suspectedHidingSpot = null;
                        return;
                    }
                }
            }

            if (searchTimer <= 0f)
            {
                suspectedHidingSpot = null;
                ResumePatrol();
            }
        }

        private void UpdatePatrol()
        {
            agent.speed = patrolSpeed;
            if (patrolWaypoints.Count == 0) return;

            if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waypointWaitTime)
                {
                    waitTimer = 0f;
                    GoToNextWaypoint();
                }
            }
        }

        private void GoToNextWaypoint()
        {
            if (patrolWaypoints.Count == 0) return;
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Count;
            if (patrolWaypoints[currentWaypointIndex] != null && agent.isOnNavMesh)
            {
                agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
            }
        }

        private void ResumePatrol()
        {
            currentState = AIState.Patrol;
            agent.speed = patrolSpeed;

            if (eyeGlowLight != null)
            {
                eyeGlowLight.color = new Color(0.9f, 0.8f, 0.3f); // Pale amber
                eyeGlowLight.intensity = 0.8f;
            }

            GoToNextWaypoint();
        }

        #endregion

        #region Catch & Cycle Transition

        public void NotifyPlayerEnteredHidingSpot(HidingSpot spot)
        {
            // If The Subject was already hunting or within 8m, it saw/heard player hide!
            float dist = Vector3.Distance(transform.position, spot.transform.position);
            if (currentState == AIState.Hunt || dist <= 8.0f)
            {
                suspectedHidingSpot = spot;
                StartInvestigate(spot.transform.position);
            }
        }

        private void CatchPlayer()
        {
            hasCaughtPlayerInCycle = true;
            agent.isStopped = true;

            StartCoroutine(CatchSequenceRoutine());
        }

        private IEnumerator CatchSequenceRoutine()
        {
            // Disable player movement
            if (playerMovement != null) playerMovement.enabled = false;

            // Turn player camera towards The Subject
            if (playerTransform != null)
            {
                Camera cam = playerTransform.GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    Vector3 lookDir = (transform.position + Vector3.up * 1.8f) - cam.transform.position;
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);

                    float elapsed = 0f;
                    while (elapsed < 0.4f)
                    {
                        elapsed += Time.deltaTime;
                        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRot, elapsed / 0.4f);
                        yield return null;
                    }
                }
            }

            if (audioSource != null && catchClip != null) audioSource.PlayOneShot(catchClip);

            yield return new WaitForSeconds(0.4f);

            // Delegate to LockdownCycleManager
            if (LockdownCycleManager.Instance != null)
            {
                LockdownCycleManager.Instance.HandlePlayerCaught();
            }
            else
            {
                HUDController.Instance?.ShowGameOver();
            }
        }

        public void ResetToContainment(Vector3 containmentPos)
        {
            hasCaughtPlayerInCycle = false;
            currentState = AIState.Patrol;
            suspectedHidingSpot = null;

            agent.Warp(containmentPos);
            agent.isStopped = false;
            agent.speed = patrolSpeed;

            ResumePatrol();
        }

        public void Stun(float duration = 6.0f)
        {
            StartCoroutine(StunRoutine(duration));
        }

        private IEnumerator StunRoutine(float duration)
        {
            AIState prev = currentState;
            currentState = AIState.Stunned;
            agent.isStopped = true;

            yield return new WaitForSeconds(duration);

            agent.isStopped = false;
            currentState = prev;
        }

        public void SetupWaypoints(List<Transform> waypoints)
        {
            patrolWaypoints = waypoints;
        }

        public void AddWaypoint(Transform wp)
        {
            if (wp != null && !patrolWaypoints.Contains(wp))
            {
                patrolWaypoints.Add(wp);
            }
        }

        public void ClearWaypoints()
        {
            patrolWaypoints.Clear();
        }

        public void SetupEyeLight(Light lt)
        {
            eyeGlowLight = lt;
        }

        public void SetSpeeds(float patrol, float investigate, float hunt, float search)
        {
            patrolSpeed = patrol;
            investigateSpeed = investigate;
            huntSpeed = hunt;
            searchSpeed = search;
            if (agent != null) agent.speed = patrolSpeed;
        }

        #endregion
    }
}
