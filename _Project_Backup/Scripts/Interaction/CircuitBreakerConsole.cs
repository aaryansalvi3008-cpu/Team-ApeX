using UnityEngine;
using EscapeTheLab.Systems;

namespace EscapeTheLab.Interaction
{
    /// <summary>
    /// Interactive breaker switch terminal in the reactor/power room.
    /// Allows the technician to reroute electricity between Lights, Doors, and Systems.
    /// </summary>
    public class CircuitBreakerConsole : MonoBehaviour, IInteractable
    {
        [Header("Circuit Controlled")]
        [SerializeField] private CircuitType targetCircuit = CircuitType.Lights;

        [Header("Visual Indicator")]
        [SerializeField] private Renderer statusIndicatorRenderer;

        private void Start()
        {
            UpdateIndicatorColor();
            if (PowerGridManager.Instance != null)
            {
                PowerGridManager.Instance.OnCircuitStateChanged += OnCircuitChanged;
            }
        }

        private void OnDestroy()
        {
            if (PowerGridManager.Instance != null)
            {
                PowerGridManager.Instance.OnCircuitStateChanged -= OnCircuitChanged;
            }
        }

        private void OnCircuitChanged(CircuitType circuit, bool state)
        {
            if (circuit == targetCircuit)
            {
                UpdateIndicatorColor();
            }
        }

        private void UpdateIndicatorColor()
        {
            if (statusIndicatorRenderer != null && PowerGridManager.Instance != null)
            {
                bool isActive = PowerGridManager.Instance.GetCircuitState(targetCircuit);
                statusIndicatorRenderer.material.color = isActive ? Color.green : Color.red;
            }
        }

        public string GetPromptText()
        {
            if (PowerGridManager.Instance == null) return "[E] Toggle Circuit";

            bool isActive = PowerGridManager.Instance.GetCircuitState(targetCircuit);
            return $"[E] Switch {targetCircuit} Breaker (Currently: {(isActive ? "ACTIVE" : "OFFLINE")})";
        }

        public bool CanInteract()
        {
            return true;
        }

        public void Interact(PlayerInteraction player)
        {
            if (PowerGridManager.Instance != null)
            {
                PowerGridManager.Instance.ToggleCircuit(targetCircuit);
                UpdateIndicatorColor();
            }
        }
    }
}
