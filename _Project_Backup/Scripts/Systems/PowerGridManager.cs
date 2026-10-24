using System;
using System.Collections.Generic;
using UnityEngine;
using EscapeTheLab.UI;

namespace EscapeTheLab.Systems
{
    public enum CircuitType
    {
        Lights,
        Doors,
        Systems
    }

    /// <summary>
    /// Implements the 3-circuit Power Grid System specified in GDD Section 7.2:
    /// - 3 Circuits: Lights, Doors, Systems.
    /// - Capacity limit: Only 2 circuits can run concurrently under emergency backup power.
    /// - Lights Circuit: Powers overhead lab lights. If cut, plunges facility into pitch blackness and restricts Subject vision.
    /// - Doors Circuit: Powers electronic blast doors. If cut, security doors seal shut.
    /// - Systems Circuit: Powers elevator/airlock overrides and diagnostic terminals.
    /// </summary>
    public class PowerGridManager : MonoBehaviour
    {
        public static PowerGridManager Instance { get; private set; }

        public const int MaxConcurrentCircuits = 2;

        [Header("Circuit States")]
        [SerializeField] private bool lightsActive = true;
        [SerializeField] private bool doorsActive = true;
        [SerializeField] private bool systemsActive = false;

        [Header("Managed World References")]
        [Tooltip("List of ceiling/overhead lights powered by the Lights circuit.")]
        [SerializeField] private List<Light> overheadLights = new List<Light>();

        // Events
        public event Action<CircuitType, bool> OnCircuitStateChanged;

        public bool LightsActive => lightsActive;
        public bool DoorsActive => doorsActive;
        public bool SystemsActive => systemsActive;

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
            ApplyCircuitEffects();
        }

        /// <summary>
        /// Toggles a specific circuit on or off. Enforces the 2-circuit maximum capacity rule.
        /// </summary>
        public bool ToggleCircuit(CircuitType circuit)
        {
            bool currentState = GetCircuitState(circuit);
            bool targetState = !currentState;

            if (targetState)
            {
                // Count how many circuits are currently active
                int activeCount = (lightsActive ? 1 : 0) + (doorsActive ? 1 : 0) + (systemsActive ? 1 : 0);
                if (activeCount >= MaxConcurrentCircuits)
                {
                    HUDController.Instance?.ShowNotification(
                        "GRID OVERLOAD: Max 2 circuits can run simultaneously! Disable one first.",
                        Color.yellow,
                        3.0f
                    );
                    return false;
                }
            }

            SetCircuitState(circuit, targetState);
            ApplyCircuitEffects();

            HUDController.Instance?.ShowNotification(
                $"Power Routed: {circuit} is now {(targetState ? "ONLINE" : "OFFLINE")}",
                targetState ? Color.green : Color.red,
                2.5f
            );

            // Emits an electrical hum noise event
            NoiseManager.Instance?.EmitNoise(transform.position, 8f, 0.6f, "BreakerSwitch");

            return true;
        }

        public bool GetCircuitState(CircuitType circuit)
        {
            switch (circuit)
            {
                case CircuitType.Lights: return lightsActive;
                case CircuitType.Doors: return doorsActive;
                case CircuitType.Systems: return systemsActive;
                default: return false;
            }
        }

        private void SetCircuitState(CircuitType circuit, bool state)
        {
            switch (circuit)
            {
                case CircuitType.Lights: lightsActive = state; break;
                case CircuitType.Doors: doorsActive = state; break;
                case CircuitType.Systems: systemsActive = state; break;
            }

            OnCircuitStateChanged?.Invoke(circuit, state);
        }

        public void RegisterOverheadLight(Light lt)
        {
            if (lt != null && !overheadLights.Contains(lt))
            {
                overheadLights.Add(lt);
                lt.enabled = lightsActive;
            }
        }

        private void ApplyCircuitEffects()
        {
            // Toggle all registered overhead lighting fixtures
            for (int i = overheadLights.Count - 1; i >= 0; i--)
            {
                if (overheadLights[i] != null)
                {
                    overheadLights[i].enabled = lightsActive;
                }
                else
                {
                    overheadLights.RemoveAt(i);
                }
            }
        }
    }
}
