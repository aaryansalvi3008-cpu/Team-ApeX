using UnityEngine;

namespace EscapeTheLab.UI
{
    /// <summary>
    /// Procedural Gothic Abandoned Laboratory UI Engine.
    /// Synthesizes antique rusted steel panels, dried blood vignettes, 
    /// weathered hazard stripes, and medical asylum dossier frames.
    /// </summary>
    public static class GothicUISkin
    {
        private static Texture2D s_PanelTex;
        private static Texture2D s_FrameBorderTex;
        private static Texture2D s_SlotTex;
        private static Texture2D s_BloodVignetteTex;
        private static Texture2D s_HazardStripeTex;
        private static Texture2D s_WhitePixel;

        public static Texture2D WhitePixel
        {
            get
            {
                if (s_WhitePixel == null)
                {
                    s_WhitePixel = new Texture2D(1, 1);
                    s_WhitePixel.SetPixel(0, 0, Color.white);
                    s_WhitePixel.Apply();
                }
                return s_WhitePixel;
            }
        }

        public static Texture2D PanelTexture
        {
            get
            {
                if (s_PanelTex == null) GeneratePanelTexture();
                return s_PanelTex;
            }
        }

        public static Texture2D FrameBorderTexture
        {
            get
            {
                if (s_FrameBorderTex == null) GenerateFrameBorderTexture();
                return s_FrameBorderTex;
            }
        }

        public static Texture2D SlotTexture
        {
            get
            {
                if (s_SlotTex == null) GenerateSlotTexture();
                return s_SlotTex;
            }
        }

        public static Texture2D BloodVignetteTexture
        {
            get
            {
                if (s_BloodVignetteTex == null) GenerateBloodVignetteTexture();
                return s_BloodVignetteTex;
            }
        }

        public static Texture2D HazardStripeTexture
        {
            get
            {
                if (s_HazardStripeTex == null) GenerateHazardStripeTexture();
                return s_HazardStripeTex;
            }
        }

        private static void GeneratePanelTexture()
        {
            int size = 64;
            s_PanelTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.15f, y * 0.15f) * 0.12f;
                    float borderDist = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    float edgeShadow = Mathf.Clamp01(borderDist / 6f);
                    
                    // Dark rusted iron / charcoal lab panel
                    Color c = new Color(0.06f + noise, 0.07f + noise, 0.08f + noise, 0.94f);
                    if (borderDist < 2)
                    {
                        c = new Color(0.24f, 0.16f, 0.12f, 0.98f); // Rusted iron rim
                    }
                    else
                    {
                        c *= (0.6f + 0.4f * edgeShadow);
                    }
                    s_PanelTex.SetPixel(x, y, c);
                }
            }
            s_PanelTex.Apply();
        }

        private static void GenerateFrameBorderTexture()
        {
            int size = 64;
            s_FrameBorderTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isCorner = (x < 8 && y < 8) || (x > size - 9 && y < 8) || (x < 8 && y > size - 9) || (x > size - 9 && y > size - 9);
                    bool isRivet = (x == 4 && y == 4) || (x == size - 5 && y == 4) || (x == 4 && y == size - 5) || (x == size - 5 && y == size - 5);

                    Color c = new Color(0.04f, 0.04f, 0.05f, 0.90f);
                    if (isRivet)
                    {
                        c = new Color(0.75f, 0.58f, 0.28f, 1f); // Antique brass rivet
                    }
                    else if (isCorner)
                    {
                        c = new Color(0.32f, 0.24f, 0.18f, 0.98f); // Reinforced rusted steel bracket
                    }
                    else if (x <= 2 || x >= size - 3 || y <= 2 || y >= size - 3)
                    {
                        c = new Color(0.22f, 0.18f, 0.15f, 0.95f); // Border seam
                    }
                    s_FrameBorderTex.SetPixel(x, y, c);
                }
            }
            s_FrameBorderTex.Apply();
        }

        private static void GenerateSlotTexture()
        {
            int size = 64;
            s_SlotTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(size * 0.5f, size * 0.5f));
                    float vignette = Mathf.Clamp01(distFromCenter / (size * 0.55f));

                    bool isBorder = x < 3 || x >= size - 3 || y < 3 || y >= size - 3;
                    Color c = Color.Lerp(new Color(0.10f, 0.11f, 0.13f, 0.92f), new Color(0.04f, 0.04f, 0.05f, 0.95f), vignette);

                    if (isBorder)
                    {
                        c = new Color(0.35f, 0.28f, 0.20f, 0.98f); // Corroded brass rim
                    }
                    s_SlotTex.SetPixel(x, y, c);
                }
            }
            s_SlotTex.Apply();
        }

        private static void GenerateBloodVignetteTexture()
        {
            int size = 128;
            s_BloodVignetteTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float maxDist = size * 0.707f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                    float alpha = Mathf.Pow(Mathf.Clamp01((dist - 0.35f) / 0.65f), 2.2f);

                    // Dried blood and grime splatter simulation
                    float noise = Mathf.PerlinNoise(x * 0.18f, y * 0.18f);
                    float bloodDrip = Mathf.PerlinNoise(x * 0.08f, y * 0.4f);

                    Color blood = new Color(0.35f + noise * 0.15f, 0.02f, 0.03f, alpha * (0.6f + 0.4f * bloodDrip));
                    s_BloodVignetteTex.SetPixel(x, y, blood);
                }
            }
            s_BloodVignetteTex.Apply();
        }

        private static void GenerateHazardStripeTexture()
        {
            int size = 64;
            s_HazardStripeTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int diag = (x + y) % 16;
                    bool isYellow = diag < 8;
                    float noise = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.15f;

                    Color c = isYellow 
                        ? new Color(0.75f - noise, 0.55f - noise, 0.08f, 0.95f) // Weathered caution yellow
                        : new Color(0.12f + noise, 0.12f + noise, 0.12f, 0.95f); // Charcoal black

                    s_HazardStripeTex.SetPixel(x, y, c);
                }
            }
            s_HazardStripeTex.Apply();
        }

        public static void DrawGothicPanel(Rect rect, Color tint, string title = null)
        {
            GUI.color = tint;
            GUI.DrawTexture(rect, FrameBorderTexture);

            // Inner dark panel
            Rect inner = new Rect(rect.x + 4, rect.y + 4, rect.width - 8, rect.height - 8);
            GUI.color = new Color(tint.r * 0.6f, tint.g * 0.6f, tint.b * 0.6f, tint.a);
            GUI.DrawTexture(inner, PanelTexture);

            // Title header bar if provided
            if (!string.IsNullOrEmpty(title))
            {
                Rect headerRect = new Rect(rect.x + 4, rect.y + 4, rect.width - 8, 24);
                GUI.color = new Color(0.2f, 0.12f, 0.10f, 0.85f);
                GUI.DrawTexture(headerRect, WhitePixel);

                GUI.color = new Color(0.95f, 0.85f, 0.65f);
                GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    fontStyle = FontStyle.Bold
                };
                GUI.Label(headerRect, title, titleStyle);
            }
            GUI.color = Color.white;
        }

        public static void DrawBloodVignette(float screenW, float screenH, float intensity)
        {
            if (intensity <= 0.01f) return;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(intensity));
            GUI.DrawTexture(new Rect(0, 0, screenW, screenH), BloodVignetteTexture);
            GUI.color = Color.white;
        }
    }
}
