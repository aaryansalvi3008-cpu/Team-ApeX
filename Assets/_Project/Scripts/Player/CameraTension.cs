using UnityEngine;
using EscapeTheLab.AI;

namespace EscapeTheLab.Player
{
    /// <summary>
    /// Applies subtle camera tremors, tension shaking, and fear vibration
    /// to the player's camera when the monster is in close proximity or chasing.
    /// </summary>
    public class CameraTension : MonoBehaviour
    {
        [Header("Tension Settings")]
        [Tooltip("Maximum camera position shake in meters.")]
        [SerializeField] private float maxShakeAmount = 0.04f;

        [Tooltip("Speed of the tension shake frequency.")]
        [SerializeField] private float shakeFrequency = 24f;

        [Tooltip("Distance at which proximity tension begins.")]
        [SerializeField] private float maxTensionDistance = 16f;

        private MonsterController monster;
        private Transform playerRoot;
        private Vector3 defaultLocalPos;
        private float currentShakeStrength = 0f;

        private void Start()
        {
            defaultLocalPos = transform.localPosition;
            playerRoot = transform.root;
            FindMonster();
        }

        private void FindMonster()
        {
            if (monster == null)
            {
                monster = FindAnyObjectByType<MonsterController>();
            }
        }

        private void Update()
        {
            if (monster == null)
            {
                FindMonster();
                return;
            }

            // Calculate monster proximity
            float distance = Vector3.Distance(playerRoot.position, monster.transform.position);

            float targetStrength = 0f;
            if (monster.CurrentState == MonsterController.AIState.Chase)
            {
                targetStrength = 1.0f;
            }
            else if (distance < maxTensionDistance)
            {
                targetStrength = Mathf.InverseLerp(maxTensionDistance, 3f, distance);
            }

            currentShakeStrength = Mathf.Lerp(currentShakeStrength, targetStrength, Time.deltaTime * 3.5f);

            if (currentShakeStrength > 0.01f)
            {
                float shakeX = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) * 2f - 1f) * maxShakeAmount * currentShakeStrength;
                float shakeY = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) * 2f - 1f) * maxShakeAmount * currentShakeStrength;

                transform.localPosition = new Vector3(defaultLocalPos.x + shakeX, defaultLocalPos.y + shakeY, defaultLocalPos.z);
            }
            else
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, defaultLocalPos, Time.deltaTime * 6f);
            }
        }
    }
}
