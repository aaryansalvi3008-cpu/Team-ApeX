using UnityEngine;
using UnityEngine.SceneManagement;
using EscapeTheLab.Audio;

namespace EscapeTheLab.UI
{
    /// <summary>
    /// Controls the Main Menu:
    /// - Project VANTA atmospheric horror title and presentation.
    /// - Level 1: Upper Labs (Always accessible).
    /// - Level 2: Sub-Basement Cryo (Locked until Level 1 completed).
    /// - Survival Manual modal with complete GDD mechanics guide.
    /// - Quit Game button.
    /// - Ambient horror facility drone audio.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Colors")]
        [SerializeField] private Color titleColor = new Color(0.95f, 0.95f, 0.95f);
        [SerializeField] private Color subtitleColor = new Color(0.85f, 0.25f, 0.25f);
        [SerializeField] private Color buttonNormalColor = new Color(0.18f, 0.20f, 0.22f, 0.85f);
        [SerializeField] private Color buttonHoverColor = new Color(0.28f, 0.32f, 0.36f, 0.95f);

        private bool showInstructions = false;
        private string accessDeniedMessage = "";
        private float accessDeniedTimer = 0f;
        private Texture2D whitePixel;

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle instructionHeaderStyle;
        private GUIStyle instructionBodyStyle;
        private GUIStyle noticeStyle;
        private GUIStyle smallResetStyle;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            whitePixel = new Texture2D(1, 1);
            whitePixel.SetPixel(0, 0, Color.white);
            whitePixel.Apply();

            // Start menu ambient audio
            AudioSource aSrc = gameObject.AddComponent<AudioSource>();
            aSrc.clip = ProceduralAudioClips.CreateFacilityDrone("MenuDrone", 4f);
            aSrc.loop = true;
            aSrc.volume = 0.5f;
            aSrc.Play();
        }

        private void Update()
        {
            if (accessDeniedTimer > 0f)
            {
                accessDeniedTimer -= Time.deltaTime;
                if (accessDeniedTimer <= 0f)
                {
                    accessDeniedMessage = "";
                }
            }
        }

        private void InitStylesIfNeeded()
        {
            if (titleStyle != null) return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 44,
                fontStyle = FontStyle.Bold
            };

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Italic
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };

            instructionHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };

            instructionBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                fontStyle = FontStyle.Normal
            };

            noticeStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };

            smallResetStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Normal
            };
        }

        private void OnGUI()
        {
            InitStylesIfNeeded();

            float screenW = Screen.width;
            float screenH = Screen.height;

            // Gothic Blood & Darkness Vignette
            GothicUISkin.DrawBloodVignette(screenW, screenH, 0.90f);

            if (showInstructions)
            {
                DrawInstructionsModal(screenW, screenH);
                return;
            }

            // Title Banner Container
            float titleCardW = 780f;
            float titleCardH = 140f;
            float titleCardX = (screenW - titleCardW) * 0.5f;
            float titleCardY = screenH * 0.10f;

            GothicUISkin.DrawGothicPanel(new Rect(titleCardX, titleCardY, titleCardW, titleCardH), new Color(0.20f, 0.16f, 0.14f, 0.95f), "☣ CLASSIFIED ASYLUM ARCHIVE // PROJECT VANTA ☣");

            GUI.color = titleColor;
            GUI.Label(new Rect(titleCardX, titleCardY + 28, titleCardW, 46), "ESCAPE THE ABANDONED LAB", titleStyle);

            GUI.color = subtitleColor;
            GUI.Label(new Rect(titleCardX, titleCardY + 76, titleCardW, 28), "SUBTERRANEAN EXPERIMENTAL SECTOR // CONTAINMENT FAILURE", subtitleStyle);

            GUI.color = new Color(0.85f, 0.75f, 0.55f);
            GUI.Label(new Rect(titleCardX, titleCardY + 104, titleCardW, 24), "5 Lockdown Cycles. Route Circuit Power. Evade The Subject.", subtitleStyle);

            // Access Denied Notification Banner (if player clicks locked Level 2)
            if (accessDeniedTimer > 0f)
            {
                float noticeW = 740f;
                float noticeH = 58f;
                float noticeX = (screenW - noticeW) * 0.5f;
                float noticeY = screenH * 0.35f;

                GothicUISkin.DrawGothicPanel(new Rect(noticeX, noticeY, noticeW, noticeH), new Color(0.40f, 0.10f, 0.08f, 0.98f), "☣ HIGH SECURITY ACCESS DENIAL ☣");
                GUI.color = new Color(1f, 0.40f, 0.35f);
                GUI.Label(new Rect(noticeX + 10, noticeY + 20, noticeW - 20, 32), accessDeniedMessage, noticeStyle);
            }

            // Menu Buttons Container
            float btnW = 440f;
            float btnH = 48f;
            float startY = screenH * 0.44f;
            float spacing = 58f;
            float btnX = (screenW - btnW) * 0.5f;

            bool isLevel2Unlocked = PlayerPrefs.GetInt("ProjectVANTA_Level2Unlocked", 0) == 1;

            // 1. ENTER LEVEL 1: UPPER LABS (Always available)
            GUI.color = Color.white;
            if (GUI.Button(new Rect(btnX, startY, btnW, btnH), "▶ ENTER LEVEL 1: UPPER LABS", buttonStyle))
            {
                StartGame("LaboratoryLevel");
            }

            // 2. ENTER LEVEL 2: CRYO SUB-BASEMENT (Locked until Level 1 passed!)
            if (isLevel2Unlocked)
            {
                GUI.color = new Color(0.40f, 1.0f, 0.70f); // Gothic phosphor green unlocked tint
                if (GUI.Button(new Rect(btnX, startY + spacing, btnW, btnH), "▶ ENTER LEVEL 2: CRYO SUB-BASEMENT [HARD]", buttonStyle))
                {
                    StartGame("LaboratoryLevel2");
                }
            }
            else
            {
                GUI.color = new Color(0.65f, 0.35f, 0.35f, 0.85f); // Dark red locked style
                if (GUI.Button(new Rect(btnX, startY + spacing, btnW, btnH), "🔒 LEVEL 2: LOCKED (PASS LEVEL 1 FIRST)", buttonStyle))
                {
                    accessDeniedMessage = "ACCESS DENIED: Cryo Sub-Basement sector is under lockdown!\nYou must escape Level 1 (Upper Labs) to unlock Level 2.";
                    accessDeniedTimer = 4.5f;
                }
            }

            // 3. HOW TO PLAY / SURVIVAL MANUAL
            GUI.color = Color.white;
            if (GUI.Button(new Rect(btnX, startY + spacing * 2, btnW, btnH), "☠ SURVIVAL MANUAL & DIRECTIVES", buttonStyle))
            {
                showInstructions = true;
            }

            // 4. QUIT TO DESKTOP
            if (GUI.Button(new Rect(btnX, startY + spacing * 3, btnW, btnH), "✖ QUIT TO DESKTOP", buttonStyle))
            {
                QuitGame();
            }

            // Optional: Reset Progress Button (Allows testing lock/unlock state)
            if (isLevel2Unlocked)
            {
                GUI.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                if (GUI.Button(new Rect(20, screenH - 42, 210, 26), "⟲ Reset Level Progress (Lock L2)", smallResetStyle))
                {
                    PlayerPrefs.DeleteKey("ProjectVANTA_Level2Unlocked");
                    PlayerPrefs.Save();
                    accessDeniedMessage = "PROGRESS RESET: Level 2 has been locked again.";
                    accessDeniedTimer = 3.0f;
                }
            }

            // Footer
            GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);
            GUI.Label(new Rect(0, screenH - 35, screenW, 25), "Unity 6 Gothic Survival-Horror Experience // Project VANTA", subtitleStyle);
            GUI.color = Color.white;
        }

        private void DrawInstructionsModal(float screenW, float screenH)
        {
            float modalW = 820f;
            float modalH = 540f;
            float modalX = (screenW - modalW) * 0.5f;
            float modalY = (screenH - modalH) * 0.5f;

            GothicUISkin.DrawGothicPanel(new Rect(modalX, modalY, modalW, modalH), new Color(0.18f, 0.16f, 0.14f, 0.98f), "☣ EMERGENCY PROTOCOL OPERATIONAL DOSSIER ☣");

            // Title
            GUI.color = new Color(1.0f, 0.85f, 0.35f);
            GUI.Label(new Rect(modalX, modalY + 30, modalW, 35), "PROJECT VANTA: SURVIVAL MANUAL", instructionHeaderStyle);

            // Instructions text
            GUI.color = Color.white;
            float lineX = modalX + 35f;
            float lineY = modalY + 68f;
            float lineGap = 27f;

            GUI.Label(new Rect(lineX, lineY, 750, 24), "•  WASD — Walk (6m noise) | CTRL / C — Crouch-walk (Quiet: 2m noise)", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap, 750, 24), "•  LEFT SHIFT — Sprint (14m noise! Metal grates add +3m to footsteps)", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 2, 750, 24), "•  SPACE — Hold breath while hiding/crouched (Silences breathing, drains stamina)", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 3, 750, 24), "•  E — Interact (Open blast doors, collect salvage, enter lockers, toggle breakers)", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 4, 750, 24), "•  1, 2, 3, 4 — Select specimen inventory hotbar | Q — Throw distraction item", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 5, 750, 24), "•  F — Flashlight (Drains battery; recharge with Battery Packs)", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 6, 750, 24), "•  R — Sonar Pulse (Reveals interactables through walls with 3m ping)", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 7, 750, 24), "•  POWER GRID — 3 Circuits (Lights, Doors, Systems). Max 2 active simultaneously!", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 8, 750, 24), "•  LEVEL 1 (Upper Labs) — Normal speed, 5 cycles, airlock escape unlocks Level 2.", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 9, 750, 24), "•  LEVEL 2 (Cryo Sub-Basement) [HARD] — Faster Subject, 4 cycles, acute hearing & fog!", instructionBodyStyle);

            // Close button
            GUI.color = Color.white;
            if (GUI.Button(new Rect(modalX + (modalW - 240f) * 0.5f, modalY + modalH - 58f, 240f, 42f), "RETURN TO DOSSIER", buttonStyle))
            {
                showInstructions = false;
            }
        }

        public void StartGame(string sceneName = "LaboratoryLevel")
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }

        public void QuitGame()
        {
            Debug.Log("[MainMenu] Quitting application...");
            Application.Quit();
        }

        private void OnDestroy()
        {
            if (whitePixel != null) Destroy(whitePixel);
        }
    }
}
