using UnityEngine;
using UnityEngine.AI;

namespace EscapeTheLab.AI
{
    /// <summary>
    /// Procedurally animates The Subject's posture and limbs:
    /// - Subtle spine breathing and eerie head micro-twitches while patrolling/searching.
    /// - Articulated arm sway and dragging claw motion.
    /// - Aggressive forward hunched surge during Hunt/Chase.
    /// Eliminates static T-pose / capsule feel without requiring external animation packages.
    /// </summary>
    public class SubjectProceduralAnimator : MonoBehaviour
    {
        [Header("Rig Transforms")]
        [SerializeField] private Transform torsoTransform;
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform leftArmTransform;
        [SerializeField] private Transform rightArmTransform;
        [SerializeField] private Transform lowerJawTransform;

        [Header("Animation Tuning")]
        [SerializeField] private float breatheSpeed = 1.8f;
        [SerializeField] private float breatheAmount = 0.04f;
        [SerializeField] private float twitchInterval = 3.5f;

        private MonsterController monsterController;
        private NavMeshAgent agent;

        private Vector3 initialTorsoLocalPos;
        private Quaternion initialHeadLocalRot;
        private Quaternion initialLeftArmRot;
        private Quaternion initialRightArmRot;

        private float twitchTimer = 0f;
        private float currentTwitchYaw = 0f;
        private float currentTwitchPitch = 0f;

        private void Awake()
        {
            monsterController = GetComponent<MonsterController>();
            agent = GetComponent<NavMeshAgent>();

            if (torsoTransform != null) initialTorsoLocalPos = torsoTransform.localPosition;
            if (headTransform != null) initialHeadLocalRot = headTransform.localRotation;
            if (leftArmTransform != null) initialLeftArmRot = leftArmTransform.localRotation;
            if (rightArmTransform != null) initialRightArmRot = rightArmTransform.localRotation;
        }

        private void Update()
        {
            AnimateBreathing();
            AnimateHeadTwitch();
            AnimateArms();
        }

        private void AnimateBreathing()
        {
            if (torsoTransform == null) return;

            bool isHunting = monsterController != null && monsterController.CurrentState == MonsterController.AIState.Hunt;
            float speedMult = isHunting ? 3.0f : 1.0f;

            float breathe = Mathf.Sin(Time.time * breatheSpeed * speedMult) * breatheAmount;
            torsoTransform.localPosition = new Vector3(
                initialTorsoLocalPos.x,
                initialTorsoLocalPos.y + breathe,
                initialTorsoLocalPos.z
            );
        }

        private void AnimateHeadTwitch()
        {
            if (headTransform == null) return;

            twitchTimer += Time.deltaTime;
            if (twitchTimer >= twitchInterval)
            {
                twitchTimer = 0f;
                twitchInterval = Random.Range(2.0f, 5.0f);

                // Sudden sharp unnatural twitch angle
                currentTwitchYaw = Random.Range(-18f, 18f);
                currentTwitchPitch = Random.Range(-12f, 10f);
            }

            // Smoothly return from twitch
            currentTwitchYaw = Mathf.Lerp(currentTwitchYaw, 0f, Time.deltaTime * 5f);
            currentTwitchPitch = Mathf.Lerp(currentTwitchPitch, 0f, Time.deltaTime * 5f);

            headTransform.localRotation = initialHeadLocalRot * Quaternion.Euler(currentTwitchPitch, currentTwitchYaw, 0f);

            // Animate lower jaw chattering/gaping
            if (lowerJawTransform != null)
            {
                bool isHunting = monsterController != null && monsterController.CurrentState == MonsterController.AIState.Hunt;
                float jawAngle = isHunting 
                    ? (22f + Mathf.Sin(Time.time * 20f) * 7f) // Fast ravenous chattering
                    : (8f + Mathf.Sin(Time.time * 2.5f) * 5f); // Creepy slack jaw
                lowerJawTransform.localRotation = Quaternion.Euler(jawAngle, 0f, 0f);
            }
        }

        private void AnimateArms()
        {
            float moveSpeed = agent != null ? agent.velocity.magnitude : 0f;
            float walkCycle = Mathf.Sin(Time.time * (moveSpeed > 3f ? 14f : 6f));

            bool isHunting = monsterController != null && monsterController.CurrentState == MonsterController.AIState.Hunt;

            if (leftArmTransform != null)
            {
                float forwardLean = isHunting ? -28f : -10f; // Reaching forward to claw
                float swing = walkCycle * (moveSpeed > 0.1f ? 15f : 3f);
                leftArmTransform.localRotation = initialLeftArmRot * Quaternion.Euler(forwardLean + swing, 0f, -4f);
            }

            if (rightArmTransform != null)
            {
                float forwardLean = isHunting ? -32f : -8f;
                float swing = -walkCycle * (moveSpeed > 0.1f ? 15f : 3f);
                rightArmTransform.localRotation = initialRightArmRot * Quaternion.Euler(forwardLean + swing, 0f, 4f);
            }
        }

        public void SetupTransforms(Transform torso, Transform head, Transform leftArm, Transform rightArm, Transform jaw = null)
        {
            torsoTransform = torso;
            headTransform = head;
            leftArmTransform = leftArm;
            rightArmTransform = rightArm;
            lowerJawTransform = jaw;

            if (torsoTransform != null) initialTorsoLocalPos = torsoTransform.localPosition;
            if (headTransform != null) initialHeadLocalRot = headTransform.localRotation;
            if (leftArmTransform != null) initialLeftArmRot = leftArmTransform.localRotation;
            if (rightArmTransform != null) initialRightArmRot = rightArmTransform.localRotation;
        }
    }
}
