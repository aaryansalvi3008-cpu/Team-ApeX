using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using EscapeTheLab.Player;
using EscapeTheLab.Systems;

namespace EscapeTheLab.UI
{
    /// <summary>
    /// Gothic Abandoned Laboratory HUD & Interface Director:
    /// - Gothic asylum / classified dossier aesthetic.
    /// - Dynamic acoustic resonance meter.
    /// - Proximity horror feedback (radial blood vignette & heartbeat tension).
    /// - 4-Slot specimen inventory bar with antique brass slots.
    /// - Multi-level progression pipeline (Level 1 -> Level 2 transition).
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [Header("Gothic Palette")]
        [SerializeField] private Color normalCrosshairColor = new Color(0.85f, 0.85f, 0.80f, 0.75f);
        [SerializeField] private Color focusedCrosshairColor = new Color(0.95f, 0.20f, 0.15f, 1.0f); // Crimson
        [SerializeField] private Color objectiveColor = new Color(0.95f, 0.82f, 0.45f); // Tarnished gold
        [SerializeField] private Color victoryColor = new Color(0.35f, 0.95f, 0.55f); // Gothic phosphor green
        [SerializeField] private Color gameOverColor = new Color(0.95f, 0.12f, 0.12f); // Deep blood red

        // Runtime states
        private string promptMessage = "";
        private string objectiveMessage = "Locate Security Keycard & Route Power to Airlock Systems";
        private string notificationMessage = "";
        private Color currentNotificationColor = Color.cyan;
        private float notificationTimer = 0f;

        private int currentCycle = 1;
        private int maxCycles = 5;

        private bool isCrosshairFocused = false;
        private bool isEscaped = false;
        private bool isGameOver = false;
        private bool isPaused = false;

        // Level Transition System
        private string nextLevelSceneName = "LaboratoryLevel2";
        private bool isUltimateVictory = false;
        private float levelTransitionCountdown = 5f;

        // Proximity horror feedback
        private float currentMonsterDistance = 999f;
        private bool isMonsterHunting = false;
        private float proximityVignetteAlpha = 0f;

        // Static glitch & Scanner pulse
        private float staticGlitchTimer = 0f;
        private float scannerPulseTimer = 0f;

        private Texture2D staticNoiseTex;

        private GUIStyle promptStyle;
        private GUIStyle objectiveStyle;
        private GUIStyle notificationStyle;
        private GUIStyle bannerTitleStyle;
        private GUIStyle bannerSubStyle;
        private GUIStyle menuButtonStyle;
        private GUIStyle hotbarStyle;
        private GUIStyle statsStyle;

        private FirstPersonMovement playerMovement;
        private FlashlightController playerFlashlight;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            GenerateStaticTexture();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (staticNoiseTex != null) Destroy(staticNoiseTex);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            playerMovement = null;
            playerFlashlight = null;
            isEscaped = false;
            isGameOver = false;
            isPaused = false;
            Time.timeScale = 1f;
            FindPlayerReferences();

            if (scene.name == "LaboratoryLevel2")
            {
                objectiveMessage = "Sub-Basement: Locate Master Override Key & Restore Auxiliary Reactor";
            }
            else if (scene.name == "LaboratoryLevel")
            {
                objectiveMessage = "Locate Security Keycard & Route Power to Airlock Systems";
            }
        }

        private void GenerateStaticTexture()
        {
            staticNoiseTex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float grey = Random.value;
                    staticNoiseTex.SetPixel(x, y, new Color(grey, grey, grey, 0.40f));
                }
            }
            staticNoiseTex.Apply();
        }

        private void Update()
        {
            FindPlayerReferences();

            // Smooth proximity vignette
            float targetVignette = 0f;
            if (currentMonsterDistance < 14f)
            {
                float prox = Mathf.Clamp01(1f - (currentMonsterDistance / 14f));
                targetVignette = prox * (isMonsterHunting ? 0.95f : 0.65f);
                // Pulse heartbeat
                targetVignette += Mathf.Sin(Time.time * (isMonsterHunting ? 9f : 4f)) * 0.12f;
            }
            proximityVignetteAlpha = Mathf.MoveTowards(proximityVignetteAlpha, Mathf.Clamp01(targetVignette), Time.deltaTime * 3f);

            // Handle Pause Toggle
            if (!isGameOver && !isEscaped)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                {
                    TogglePause();
                }
            }

            // Handle Level 1 Victory Countdown & Input
            if (isEscaped && !isUltimateVictory && !string.IsNullOrEmpty(nextLevelSceneName))
            {
                levelTransitionCountdown -= Time.unscaledDeltaTime;
                if (levelTransitionCountdown <= 0f || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
                {
                    ProceedToNextLevel();
                }
            }

            if ((isGameOver || isUltimateVictory) && Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }

            if (notificationTimer > 0f)
            {
                notificationTimer -= Time.deltaTime;
                if (notificationTimer <= 0f) notificationMessage = "";
            }

            if (staticGlitchTimer > 0f) staticGlitchTimer -= Time.deltaTime;
            if (scannerPulseTimer > 0f) scannerPulseTimer -= Time.deltaTime;
        }

        private void FindPlayerReferences()
        {
            if (playerMovement == null)
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj == null) playerObj = GameObject.Find("Player");
                if (playerObj != null)
                {
                    playerMovement = playerObj.GetComponent<FirstPersonMovement>();
                    playerFlashlight = playerObj.GetComponentInChildren<FlashlightController>();
                }
            }
        }

        public void ReportMonsterDistance(float distance, bool isHunting)
        {
            currentMonsterDistance = distance;
            isMonsterHunting = isHunting;
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;

            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPaused;
        }

        public void ProceedToNextLevel()
        {
            Time.timeScale = 1f;
            isPaused = false;
            isGameOver = false;
            isEscaped = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (!string.IsNullOrEmpty(nextLevelSceneName))
            {
                SceneManager.LoadScene(nextLevelSceneName);
            }
            else
            {
                SceneManager.LoadScene("LaboratoryLevel2");
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            isPaused = false;
            isGameOver = false;
            isEscaped = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SceneManager.LoadScene("LaboratoryLevel");
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            isPaused = false;
            isGameOver = false;
            isEscaped = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("MainMenu");
        }

        private void InitStylesIfNeeded()
        {
            if (promptStyle != null) return;

            promptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Bold
            };

            objectiveStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            notificationStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };

            bannerTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 38,
                fontStyle = FontStyle.Bold
            };

            bannerSubStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Normal
            };

            menuButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Bold
            };

            hotbarStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            statsStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
        }

        private void OnGUI()
        {
            InitStylesIfNeeded();

            float screenW = Screen.width;
            float screenH = Screen.height;

            // 1. Proximity Horror Vignette (blood splatter & darkness when monster stalks nearby)
            if (proximityVignetteAlpha > 0.02f)
            {
                GothicUISkin.DrawBloodVignette(screenW, screenH, proximityVignetteAlpha);
            }

            // 2. Static Glitch Overlay (during catch / jumpscare)
            if (staticGlitchTimer > 0f)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.75f);
                GUI.DrawTextureWithTexCoords(new Rect(0, 0, screenW, screenH), staticNoiseTex, new Rect(Random.value, Random.value, 4, 4));
            }

            // 3. Pause Menu Overlay
            if (isPaused)
            {
                DrawPauseMenu(screenW, screenH);
                return;
            }

            // 4. Game Over Screen
            if (isGameOver)
            {
                DrawGameOverScreen(screenW, screenH);
                return;
            }

            // 5. Victory Screen (handles Level 1 -> Level 2 transition)
            if (isEscaped)
            {
                DrawVictoryScreen(screenW, screenH);
                return;
            }

            // 6. Gothic Crosshair: Weathered Iron Reticle with Crimson Bracket Expansion
            DrawGothicCrosshair(screenW, screenH);

            // 7. Interaction Prompt: Weathered Laboratory Placard
            if (!string.IsNullOrEmpty(promptMessage))
            {
                DrawGothicPrompt(screenW, screenH);
            }

            // 8. Top-Left: Incident Dossier / Objective Tracker
            if (!string.IsNullOrEmpty(objectiveMessage))
            {
                DrawGothicObjective();
            }

            // 9. Top-Right: Containment Protocol & Circuit Power
            DrawGothicTopRightStatus(screenW);

            // 10. Bottom-Left: Gothic Acoustic Resonance Meter
            DrawGothicNoiseMeter(screenH);

            // 11. Bottom-Center: 4-Slot Specimen Inventory Bar
            DrawGothicInventoryBar(screenW, screenH);

            // 12. Bottom-Right: Voltmeter & Scanner Status
            DrawGothicToolStatus(screenW, screenH);

            // 13. Center-Bottom: Temporary Gothic Notification
            if (!string.IsNullOrEmpty(notificationMessage))
            {
                DrawGothicNotification(screenW, screenH);
            }

            // 14. Breath-Hold Stamina Bar
            if (playerMovement != null && playerMovement.IsHoldingBreath)
            {
                DrawGothicBreathBar(screenW, screenH);
            }

            GUI.color = Color.white;
        }

        private void DrawGothicCrosshair(float screenW, float screenH)
        {
            float cx = screenW * 0.5f;
            float cy = screenH * 0.5f;

            if (isCrosshairFocused)
            {
                // Expanding glowing crimson brackets: ⟦ + ⟧
                GUI.color = focusedCrosshairColor;
                // Center pip
                GUI.DrawTexture(new Rect(cx - 2, cy - 2, 4, 4), GothicUISkin.WhitePixel);
                // Surrounding target notches
                float dist = 8f;
                GUI.DrawTexture(new Rect(cx - dist - 6, cy - 1, 6, 2), GothicUISkin.WhitePixel); // left
                GUI.DrawTexture(new Rect(cx + dist, cy - 1, 6, 2), GothicUISkin.WhitePixel);     // right
                GUI.DrawTexture(new Rect(cx - 1, cy - dist - 6, 2, 6), GothicUISkin.WhitePixel); // top
                GUI.DrawTexture(new Rect(cx - 1, cy + dist, 2, 6), GothicUISkin.WhitePixel);     // bottom
            }
            else
            {
                // Subtle weathered dot
                GUI.color = normalCrosshairColor;
                GUI.DrawTexture(new Rect(cx - 2, cy - 2, 4, 4), GothicUISkin.WhitePixel);
            }
        }

        private void DrawGothicPrompt(float screenW, float screenH)
        {
            float promptW = 560f;
            float promptH = 38f;
            float promptX = (screenW - promptW) * 0.5f;
            float promptY = (screenH * 0.5f) + 38f;

            GothicUISkin.DrawGothicPanel(new Rect(promptX, promptY, promptW, promptH), new Color(0.18f, 0.16f, 0.14f, 0.95f));

            GUI.color = isCrosshairFocused ? new Color(1f, 0.85f, 0.35f) : Color.white;
            GUI.Label(new Rect(promptX, promptY + 2, promptW, promptH - 4), promptMessage, promptStyle);
        }

        private void DrawGothicObjective()
        {
            float w = 540f;
            float h = 50f;
            Rect rect = new Rect(20, 20, w, h);

            GothicUISkin.DrawGothicPanel(rect, new Color(0.20f, 0.18f, 0.15f, 0.94f), "☠ BIO-HAZARD DOSSIER // MANDATORY DIRECTIVE ☠");

            GUI.color = objectiveColor;
            GUI.Label(new Rect(30, 42, w - 20, 24), "▶ " + objectiveMessage, objectiveStyle);
        }

        private void DrawGothicTopRightStatus(float screenW)
        {
            float w = 330f;
            float h = 64f;
            float x = screenW - w - 20f;
            float y = 20f;
            Rect rect = new Rect(x, y, w, h);

            GothicUISkin.DrawGothicPanel(rect, new Color(0.22f, 0.18f, 0.14f, 0.95f), "☣ FACILITY STATUS MONITOR ☣");

            // Cycle text
            GUI.color = currentCycle >= maxCycles ? new Color(1f, 0.2f, 0.2f) : new Color(0.95f, 0.85f, 0.4f);
            GUI.Label(new Rect(x + 10, y + 26, w - 20, 20), $"LOCKDOWN CYCLE: [ {currentCycle} / {maxCycles} ]", statsStyle);

            // Power circuit status
            string powerInfo = "POWER GRID: ";
            if (PowerGridManager.Instance != null)
            {
                powerInfo += PowerGridManager.Instance.LightsActive ? "[LT:ON] " : "[LT:OFF] ";
                powerInfo += PowerGridManager.Instance.DoorsActive ? "[DR:ON] " : "[DR:OFF] ";
                powerInfo += PowerGridManager.Instance.SystemsActive ? "[SYS:ON]" : "[SYS:OFF]";
            }
            GUI.color = new Color(0.4f, 0.85f, 0.95f);
            GUI.Label(new Rect(x + 10, y + 44, w - 20, 18), powerInfo, statsStyle);
        }

        private void DrawGothicNoiseMeter(float screenH)
        {
            float noiseRadius = NoiseManager.Instance != null ? NoiseManager.Instance.CurrentPlayerNoiseRadius : 0f;

            float x = 20f;
            float y = screenH - 78f;
            float w = 210f;
            float h = 56f;
            Rect rect = new Rect(x, y, w, h);

            GothicUISkin.DrawGothicPanel(rect, new Color(0.20f, 0.18f, 0.16f, 0.94f), "ACOUSTIC GALVANOMETER");

            string noiseLabel = "SILENCE (SAFE)";
            Color noiseColor = new Color(0.35f, 0.85f, 0.45f);

            if (noiseRadius > 8.0f)
            {
                noiseLabel = "☠ DANGER: THE SUBJECT HEARS YOU! ☠";
                noiseColor = new Color(1.0f, 0.15f, 0.15f);
            }
            else if (noiseRadius > 3.0f)
            {
                noiseLabel = "⚠️ RESONANCE DETECTED ⚠️";
                noiseColor = new Color(1.0f, 0.80f, 0.20f);
            }

            GUI.color = noiseColor;
            GUI.Label(new Rect(x + 6, y + 22, w - 12, 16), noiseLabel, hotbarStyle);

            // Analog gauge bar
            float barW = (w - 20f);
            float barFill = Mathf.Clamp01(noiseRadius / 14.0f) * barW;

            GUI.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);
            GUI.DrawTexture(new Rect(x + 10, y + 38, barW, 10), GothicUISkin.WhitePixel);

            GUI.color = noiseColor;
            GUI.DrawTexture(new Rect(x + 10, y + 38, barFill, 10), GothicUISkin.WhitePixel);
        }

        private void DrawGothicInventoryBar(float screenW, float screenH)
        {
            if (InventoryManager.Instance == null) return;

            float slotW = 92f;
            float slotH = 58f;
            float gap = 8f;
            float totalW = (slotW * InventoryManager.SlotCount) + (gap * (InventoryManager.SlotCount - 1));
            float startX = (screenW - totalW) * 0.5f;
            float startY = screenH - slotH - 18f;

            int activeIdx = InventoryManager.Instance.ActiveSlotIndex;
            string[] roman = new string[] { "I", "II", "III", "IV" };

            for (int i = 0; i < InventoryManager.SlotCount; i++)
            {
                float slotX = startX + i * (slotW + gap);
                bool isActive = (i == activeIdx);
                Rect slotRect = new Rect(slotX, startY, slotW, slotH);

                // Slot background
                Color tint = isActive ? new Color(0.40f, 0.32f, 0.20f, 0.95f) : new Color(0.14f, 0.14f, 0.16f, 0.90f);
                GUI.color = tint;
                GUI.DrawTexture(slotRect, GothicUISkin.SlotTexture);

                // Active border highlight
                if (isActive)
                {
                    GUI.color = new Color(0.95f, 0.75f, 0.25f, 1f); // Tarnished gold border
                    GUI.DrawTexture(new Rect(slotX, startY, slotW, 2), GothicUISkin.WhitePixel);
                    GUI.DrawTexture(new Rect(slotX, startY + slotH - 2, slotW, 2), GothicUISkin.WhitePixel);
                    GUI.DrawTexture(new Rect(slotX, startY, 2, slotH), GothicUISkin.WhitePixel);
                    GUI.DrawTexture(new Rect(slotX + slotW - 2, startY, 2, slotH), GothicUISkin.WhitePixel);
                }

                // Roman numeral slot tag
                GUI.color = isActive ? new Color(1f, 0.9f, 0.6f) : new Color(0.6f, 0.6f, 0.6f);
                GUI.Label(new Rect(slotX + 4, startY + 4, 24, 16), roman[i], hotbarStyle);

                InventoryItem item = InventoryManager.Instance.GetSlot(i);
                if (item != null)
                {
                    GUI.color = item.SlotColor;
                    GUI.Label(new Rect(slotX + 4, startY + 18, slotW - 8, 34), item.DisplayName, hotbarStyle);
                }
                else
                {
                    GUI.color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
                    GUI.Label(new Rect(slotX, startY + 24, slotW, 20), "Empty", hotbarStyle);
                }
            }
        }

        private void DrawGothicToolStatus(float screenW, float screenH)
        {
            if (playerFlashlight == null) return;

            float w = 230f;
            float h = 64f;
            float x = screenW - w - 20f;
            float y = screenH - h - 18f;
            Rect rect = new Rect(x, y, w, h);

            GothicUISkin.DrawGothicPanel(rect, new Color(0.20f, 0.18f, 0.16f, 0.94f), "EQUIPMENT TELEMETRY");

            // Flashlight battery
            float battRatio = playerFlashlight.BatteryRatio;
            string battText = $"[F] FLASHLIGHT: {(playerFlashlight.IsOn ? "ACTIVE" : "OFF")} ({(int)(battRatio * 100)}%)";
            GUI.color = battRatio < 0.2f ? new Color(1f, 0.2f, 0.2f) : (playerFlashlight.IsOn ? new Color(1f, 0.85f, 0.3f) : new Color(0.8f, 0.8f, 0.8f));
            GUI.Label(new Rect(x + 10, y + 26, w - 20, 18), battText, hotbarStyle);

            // Scanner status
            string scanText = playerFlashlight.IsScannerReady ? "[R] SONAR SCANNER: READY" : "[R] SONAR: RECHARGING";
            GUI.color = playerFlashlight.IsScannerReady ? new Color(0.25f, 0.85f, 0.95f) : new Color(0.5f, 0.5f, 0.5f);
            GUI.Label(new Rect(x + 10, y + 44, w - 20, 18), scanText, hotbarStyle);
        }

        private void DrawGothicBreathBar(float screenW, float screenH)
        {
            float barW = 260f;
            float barH = 28f;
            float barX = (screenW - barW) * 0.5f;
            float barY = screenH - 120f;

            GothicUISkin.DrawGothicPanel(new Rect(barX, barY, barW, barH), new Color(0.18f, 0.16f, 0.14f, 0.95f));

            float stamina = playerMovement.BreathStaminaRatio;
            Color breathCol = stamina > 0.35f ? new Color(0.3f, 0.85f, 0.95f) : new Color(1.0f, 0.2f, 0.2f);

            GUI.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);
            GUI.DrawTexture(new Rect(barX + 8, barY + 16, barW - 16, 8), GothicUISkin.WhitePixel);

            GUI.color = breathCol;
            GUI.DrawTexture(new Rect(barX + 8, barY + 16, (barW - 16) * stamina, 8), GothicUISkin.WhitePixel);

            GUI.color = Color.white;
            GUI.Label(new Rect(barX, barY + 2, barW, 16), "HOLDING BREATH [SPACE]", hotbarStyle);
        }

        private void DrawGothicNotification(float screenW, float screenH)
        {
            float notifW = 640f;
            float notifH = 44f;
            float notifX = (screenW - notifW) * 0.5f;
            float notifY = screenH - 145f;

            GothicUISkin.DrawGothicPanel(new Rect(notifX, notifY, notifW, notifH), new Color(0.22f, 0.18f, 0.14f, 0.96f));

            GUI.color = currentNotificationColor;
            GUI.Label(new Rect(notifX, notifY + 6, notifW, notifH - 12), notificationMessage, notificationStyle);
        }

        private void DrawPauseMenu(float screenW, float screenH)
        {
            // Dark vignette backdrop
            GothicUISkin.DrawBloodVignette(screenW, screenH, 0.95f);

            float menuW = 420f;
            float menuH = 340f;
            float menuX = (screenW - menuW) * 0.5f;
            float menuY = (screenH - menuH) * 0.5f;

            GothicUISkin.DrawGothicPanel(new Rect(menuX, menuY, menuW, menuH), new Color(0.20f, 0.18f, 0.15f, 0.98f), "☣ OPERATIONS SUSPENDED ☣");

            GUI.color = new Color(0.95f, 0.85f, 0.65f);
            GUI.Label(new Rect(menuX, menuY + 36, menuW, 40), "GAME PAUSED", bannerTitleStyle);

            float btnW = 300f;
            float btnH = 44f;
            float btnX = menuX + (menuW - btnW) * 0.5f;
            float startY = menuY + 100f;
            float gap = 55f;

            GUI.color = Color.white;
            if (GUI.Button(new Rect(btnX, startY, btnW, btnH), "RESUME (ESC)", menuButtonStyle)) TogglePause();
            if (GUI.Button(new Rect(btnX, startY + gap, btnW, btnH), "RESTART SECTOR 1", menuButtonStyle)) RestartGame();
            if (GUI.Button(new Rect(btnX, startY + gap * 2, btnW, btnH), "MAIN MENU", menuButtonStyle)) ReturnToMainMenu();
        }

        private void DrawGameOverScreen(float screenW, float screenH)
        {
            GothicUISkin.DrawBloodVignette(screenW, screenH, 1.0f);

            float cardW = 580f;
            float cardH = 360f;
            float cardX = (screenW - cardW) * 0.5f;
            float cardY = (screenH - cardH) * 0.5f;

            GothicUISkin.DrawGothicPanel(new Rect(cardX, cardY, cardW, cardH), new Color(0.35f, 0.10f, 0.08f, 0.98f), "☠ MAXIMUM BIO-QUARANTINE ENFORCED ☠");

            GUI.color = gameOverColor;
            GUI.Label(new Rect(cardX, cardY + 38, cardW, 55), "FACILITY PERMANENTLY SEALED", bannerTitleStyle);

            GUI.color = new Color(0.85f, 0.85f, 0.85f);
            GUI.Label(new Rect(cardX + 20, cardY + 95, cardW - 40, 45), "All 5 lockdown cycles expired. Project VANTA remains quarantined.\nThe Subject has reclaimed the facility.", bannerSubStyle);

            float btnW = 320f;
            float btnH = 46f;
            float btnX = cardX + (cardW - btnW) * 0.5f;
            float startY = cardY + 175f;
            float gap = 60f;

            GUI.color = Color.white;
            if (GUI.Button(new Rect(btnX, startY, btnW, btnH), "TRY AGAIN (PRESS R)", menuButtonStyle)) RestartGame();
            if (GUI.Button(new Rect(btnX, startY + gap, btnW, btnH), "MAIN MENU", menuButtonStyle)) ReturnToMainMenu();
        }

        private void DrawVictoryScreen(float screenW, float screenH)
        {
            GothicUISkin.DrawBloodVignette(screenW, screenH, 0.75f);

            float cardW = 660f;
            float cardH = 400f;
            float cardX = (screenW - cardW) * 0.5f;
            float cardY = (screenH - cardH) * 0.5f;

            if (!isUltimateVictory && !string.IsNullOrEmpty(nextLevelSceneName))
            {
                // Level 1 -> Level 2 Transition Screen!
                GothicUISkin.DrawGothicPanel(new Rect(cardX, cardY, cardW, cardH), new Color(0.15f, 0.25f, 0.18f, 0.98f), "☣ DECONTAMINATION CYCLE ACTIVE — AIRLOCK SEALED ☣");

                GUI.color = victoryColor;
                GUI.Label(new Rect(cardX, cardY + 35, cardW, 55), "SECTOR 1 ESCAPED!", bannerTitleStyle);

                GUI.color = new Color(0.92f, 0.92f, 0.92f);
                GUI.Label(new Rect(cardX + 25, cardY + 95, cardW - 50, 60), 
                    "You breached Upper Labs & Medical Bay.\nDecontamination locks are clearing... Preparing descent into Sector 2: Sub-Basement Cryo Labs.", bannerSubStyle);

                // Auto countdown
                GUI.color = new Color(1.0f, 0.85f, 0.35f);
                GUI.Label(new Rect(cardX, cardY + 165, cardW, 25), $"AUTOMATIC DESCENT IN: {Mathf.CeilToInt(Mathf.Max(0f, levelTransitionCountdown))}s", bannerSubStyle);

                float btnW = 380f;
                float btnH = 48f;
                float btnX = cardX + (cardW - btnW) * 0.5f;
                float startY = cardY + 210f;
                float gap = 60f;

                GUI.color = Color.white;
                if (GUI.Button(new Rect(btnX, startY, btnW, btnH), "ENTER LEVEL 2: SUB-BASEMENT (SPACE)", menuButtonStyle))
                {
                    ProceedToNextLevel();
                }
                if (GUI.Button(new Rect(btnX, startY + gap, btnW, btnH), "MAIN MENU", menuButtonStyle))
                {
                    ReturnToMainMenu();
                }
            }
            else
            {
                // Final Extraction Victory Screen
                GothicUISkin.DrawGothicPanel(new Rect(cardX, cardY, cardW, cardH), new Color(0.14f, 0.28f, 0.18f, 0.98f), "★ MAXIMUM SURVIVAL RATING ACHIEVED ★");

                GUI.color = victoryColor;
                GUI.Label(new Rect(cardX, cardY + 35, cardW, 55), "FACILITY FULLY ESCAPED!", bannerTitleStyle);

                GUI.color = Color.white;
                GUI.Label(new Rect(cardX + 25, cardY + 95, cardW - 50, 60), 
                    "You survived both sectors of Project VANTA and reached the surface extraction helipad.\nThe containment catastrophe has been documented.", bannerSubStyle);

                float btnW = 340f;
                float btnH = 48f;
                float btnX = cardX + (cardW - btnW) * 0.5f;
                float startY = cardY + 195f;
                float gap = 60f;

                GUI.color = Color.white;
                if (GUI.Button(new Rect(btnX, startY, btnW, btnH), "PLAY AGAIN (SECTOR 1)", menuButtonStyle)) RestartGame();
                if (GUI.Button(new Rect(btnX, startY + gap, btnW, btnH), "MAIN MENU", menuButtonStyle)) ReturnToMainMenu();
            }
        }

        public void SetPrompt(string prompt)
        {
            promptMessage = prompt ?? "";
            isCrosshairFocused = !string.IsNullOrEmpty(promptMessage);
        }

        public void SetObjective(string objective) => objectiveMessage = objective ?? "";

        public void SetCycle(int cycle, int max)
        {
            currentCycle = cycle;
            maxCycles = max;
        }

        public void TriggerStaticGlitch(float duration = 1.5f) => staticGlitchTimer = duration;

        public void TriggerScannerPulse(float duration = 3.5f) => scannerPulseTimer = duration;

        public void ShowNotification(string message, Color color, float duration = 2.5f)
        {
            notificationMessage = message;
            currentNotificationColor = color;
            notificationTimer = duration;
        }

        public void ShowVictory(string nextScene = "LaboratoryLevel2", bool isFinal = false)
        {
            isEscaped = true;
            nextLevelSceneName = nextScene;
            isUltimateVictory = isFinal;
            levelTransitionCountdown = 5f;
            promptMessage = "";
            objectiveMessage = isFinal ? "Facility Escaped!" : "Sector 1 Escaped! Advancing to Sector 2...";
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowGameOver()
        {
            isGameOver = true;
            promptMessage = "";
            objectiveMessage = "Quarantined";
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
