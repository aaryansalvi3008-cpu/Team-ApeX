using UnityEngine;
using UnityEngine.SceneManagement;
using EscapeTheLab.Audio;

namespace EscapeTheLab.UI
{
    /// <summary>
    /// Controls the Main Menu:
    /// - Project VANTA atmospheric horror title and presentation.
    /// - Start Game button (loads LaboratoryLevel).
    /// - How to Play modal with complete GDD mechanics guide.
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
        private Texture2D whitePixel;

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle instructionHeaderStyle;
        private GUIStyle instructionBodyStyle;

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
                fontSize = 19,
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
            float titleCardW = 760f;
            float titleCardH = 140f;
            float titleCardX = (screenW - titleCardW) * 0.5f;
            float titleCardY = screenH * 0.12f;

            GothicUISkin.DrawGothicPanel(new Rect(titleCardX, titleCardY, titleCardW, titleCardH), new Color(0.20f, 0.16f, 0.14f, 0.95f), "☣ CLASSIFIED ASYLUM ARCHIVE // PROJECT VANTA ☣");

            GUI.color = titleColor;
            GUI.Label(new Rect(titleCardX, titleCardY + 28, titleCardW, 46), "ESCAPE THE ABANDONED LAB", titleStyle);

            GUI.color = subtitleColor;
            GUI.Label(new Rect(titleCardX, titleCardY + 76, titleCardW, 28), "SUBTERRANEAN EXPERIMENTAL SECTOR // CONTAINMENT FAILURE", subtitleStyle);

            GUI.color = new Color(0.85f, 0.75f, 0.55f);
            GUI.Label(new Rect(titleCardX, titleCardY + 104, titleCardW, 24), "5 Lockdown Cycles. Route Circuit Power. Evade The Subject.", subtitleStyle);

            // Menu Buttons Container
            float btnW = 380f;
            float btnH = 50f;
            float startY = screenH * 0.46f;
            float spacing = 65f;
            float btnX = (screenW - btnW) * 0.5f;

            GUI.color = Color.white;

            // 1. START GAME
            if (GUI.Button(new Rect(btnX, startY, btnW, btnH), "▶ ENTER FACILITY (START GAME)", buttonStyle))
            {
                StartGame();
            }

            // 2. HOW TO PLAY
            if (GUI.Button(new Rect(btnX, startY + spacing, btnW, btnH), "☠ SURVIVAL MANUAL & DIRECTIVES", buttonStyle))
            {
                showInstructions = true;
            }

            // 3. QUIT
            if (GUI.Button(new Rect(btnX, startY + spacing * 2, btnW, btnH), "✖ QUIT TO DESKTOP", buttonStyle))
            {
                QuitGame();
            }

            // Footer
            GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);
            GUI.Label(new Rect(0, screenH - 35, screenW, 25), "Unity 6 Gothic Survival-Horror Experience", subtitleStyle);
            GUI.color = Color.white;
        }

        private void DrawInstructionsModal(float screenW, float screenH)
        {
            float modalW = 780f;
            float modalH = 520f;
            float modalX = (screenW - modalW) * 0.5f;
            float modalY = (screenH - modalH) * 0.5f;

            GothicUISkin.DrawGothicPanel(new Rect(modalX, modalY, modalW, modalH), new Color(0.18f, 0.16f, 0.14f, 0.98f), "☣ EMERGENCY PROTOCOL OPERATIONAL DOSSIER ☣");

            // Title
            GUI.color = new Color(1.0f, 0.85f, 0.35f);
            GUI.Label(new Rect(modalX, modalY + 30, modalW, 35), "PROJECT VANTA: SURVIVAL MANUAL", instructionHeaderStyle);

            // Instructions text
            GUI.color = Color.white;
            float lineX = modalX + 35f;
            float lineY = modalY + 70f;
            float lineGap = 28f;

            GUI.Label(new Rect(lineX, lineY, 710, 26), "•  WASD — Walk (6m noise) | CTRL / C — Crouch-walk (Quiet: 2m noise)", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap, 710, 26), "•  LEFT SHIFT — Sprint (14m noise! Metal grates add +3m to footsteps)", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 2, 710, 26), "•  SPACE — Hold breath while hiding/crouched (Silences breathing, drains stamina)", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 3, 710, 26), "•  E — Interact (Open blast doors, collect salvage, enter lockers, toggle breakers)", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 4, 710, 26), "•  1, 2, 3, 4 — Select specimen inventory hotbar | Q — Throw distraction item", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 5, 710, 26), "•  F — Flashlight (Drains battery; recharge with Battery Packs)", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 6, 710, 26), "•  R — Sonar Pulse (Reveals interactables through walls with 3m ping)", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 7, 710, 26), "•  POWER GRID — 3 Circuits (Lights, Doors, Systems). Max 2 active simultaneously!", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 8, 710, 26), "•  LEVEL 1: Upper Labs -> Find Keycard, route Power, escape through Airlock.", instructionBodyStyle);
            GUI.Label(new Rect(lineX, lineY + lineGap * 9, 710, 26), "•  LEVEL 2: Sub-Basement Cryo -> Restore Auxiliary Reactor, unlock Helipad Elevator!", instructionBodyStyle);

            // Close button
            GUI.color = Color.white;
            if (GUI.Button(new Rect(modalX + (modalW - 240f) * 0.5f, modalY + modalH - 58f, 240f, 42f), "RETURN TO DOSSIER", buttonStyle))
            {
                showInstructions = false;
            }
        }

        public void StartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("LaboratoryLevel");
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
