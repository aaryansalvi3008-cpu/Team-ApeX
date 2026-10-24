using System.Collections;
using UnityEngine;
using EscapeTheLab.Interaction;
using EscapeTheLab.Environment;
using EscapeTheLab.UI;

namespace EscapeTheLab.Systems
{
    /// <summary>
    /// Attached to dropped or thrown items in the physical world.
    /// Emits sound events upon impacting surfaces, serving as a tactical distraction.
    /// Supports glass shatter destruction, active sonic beacon pulses, and picking items back up.
    /// </summary>
    public class DroppedItemDistraction : MonoBehaviour, IInteractable
    {
        private InventoryItem itemData;
        private bool hasCollided = false;
        private bool isLureActive = false;
        private Light lureLight;

        public bool IsLureActive => isLureActive;

        public void Setup(InventoryItem item)
        {
            itemData = item;

            if (item != null && item.Type == ItemType.SonicLure)
            {
                // Create pulsating beacon light for sonic lure
                GameObject ltObj = new GameObject("LureBeaconLight");
                ltObj.transform.parent = transform;
                ltObj.transform.localPosition = Vector3.up * 0.15f;
                lureLight = ltObj.AddComponent<Light>();
                lureLight.type = LightType.Point;
                lureLight.color = Color.cyan;
                lureLight.range = 6.0f;
                lureLight.intensity = 1.2f;

                StartCoroutine(ActiveSonicLureRoutine());
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (hasCollided) return;
            hasCollided = true;

            if (itemData != null && itemData.Type == ItemType.DistractionBeaker)
            {
                // 1. Chemical Flask / Beaker Glass Shatter
                Audio.HorrorAudioManager.Instance?.PlayGlassShatter();
                NoiseManager.Instance?.EmitNoise(transform.position, 18.0f, 1.0f, "GlassShatter");
                HUDController.Instance?.ShowNotification("Glass flask shattered! Loud noise emitted.", Color.yellow, 2.0f);

                // Spawn a crunch floor hazard at impact point
                SpawnImpactGlassHazard(transform.position);

                // Destroy flask on shatter
                Destroy(gameObject, 0.05f);
                return;
            }
            else if (itemData != null && itemData.Type == ItemType.SonicLure)
            {
                // Sonic lure already pulsing
                Audio.HorrorAudioManager.Instance?.PlayItemImpact();
                NoiseManager.Instance?.EmitNoise(transform.position, 22.0f, 1.0f, "SonicLureImpact");
            }
            else
            {
                // 2. Metallic or General Equipment Impact
                Audio.HorrorAudioManager.Instance?.PlayItemImpact();
                NoiseManager.Instance?.EmitNoise(transform.position, 12.0f, 0.75f, "DroppedItemImpact");
            }
        }

        private IEnumerator ActiveSonicLureRoutine()
        {
            isLureActive = true;
            float duration = 12.0f;
            float timer = 0f;

            while (timer < duration)
            {
                // Pulse audio & noise event
                Audio.HorrorAudioManager.Instance?.PlaySonicLurePulse();
                NoiseManager.Instance?.EmitNoise(transform.position, 24.0f, 1.0f, "SonicLurePulse");

                // Flash LED light
                if (lureLight != null)
                {
                    lureLight.color = Color.red;
                    lureLight.intensity = 2.2f;
                }

                yield return new WaitForSeconds(0.2f);

                if (lureLight != null)
                {
                    lureLight.color = Color.cyan;
                    lureLight.intensity = 0.8f;
                }

                yield return new WaitForSeconds(1.1f);
                timer += 1.3f;
            }

            if (lureLight != null) lureLight.enabled = false;
            isLureActive = false;
        }

        private void SpawnImpactGlassHazard(Vector3 pos)
        {
            GameObject shards = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shards.name = "Shattered_Glass_Hazard";
            shards.transform.position = new Vector3(pos.x, pos.y + 0.02f, pos.z);
            shards.transform.localScale = new Vector3(1.8f, 0.04f, 1.8f);

            // Shards are trigger hazard
            Collider col = shards.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            FloorHazard hazard = shards.AddComponent<FloorHazard>();
            // Broken glass hazard triggers noise on step
        }

        public string GetPromptText()
        {
            if (itemData == null) return "[E] Pick up Item";
            return "[E] Pick up " + itemData.DisplayName;
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
