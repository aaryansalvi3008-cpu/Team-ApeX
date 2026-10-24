#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EscapeTheLab.Editor
{
    /// <summary>
    /// Generates high-quality procedural PBR textures for "Escape the Abandoned Lab":
    /// - Ceramic hospital tiles with grout & water stains (Albedo + Normal Map)
    /// - Industrial steel metal panels with seams & rivets (Albedo + Normal Map)
    /// - Weathered bunker concrete
    /// - Metal diamond grating for floor hazards
    /// - Yellow/black industrial hazard caution stripes
    /// - Retro medical phosphor CRT monitor screen
    /// - The Subject pale mottled skin & tattered institutional gown
    /// - Biohazard warning signage
    /// </summary>
    public static class ProceduralTextureGenerator
    {
        private const string TextureDir = "Assets/_Project/Textures";
        private const string MatDir = "Assets/_Project/Materials";

        [MenuItem("Tools/Escape The Lab/Generate PBR Horror Textures")]
        public static void GenerateAllTextures()
        {
            EnsureDirectories();

            Debug.Log("[ProceduralTextureGenerator] Generating PBR texture suite...");

            // 1. Ceramic Lab Tiles (Albedo + Normal)
            Texture2D tilesAlbedo = GenerateTilesAlbedo(512, 512, 8);
            SaveTexture(tilesAlbedo, $"{TextureDir}/T_Lab_Tiles_Dirty_Albedo.png", false);

            Texture2D tilesNormal = GenerateTilesNormal(512, 512, 8, 3.0f);
            SaveTexture(tilesNormal, $"{TextureDir}/T_Lab_Tiles_Dirty_Normal.png", true);

            // 2. Industrial Steel Panels with Rivets (Albedo + Normal)
            Texture2D metalAlbedo = GenerateMetalPanelAlbedo(512, 512);
            SaveTexture(metalAlbedo, $"{TextureDir}/T_Lab_Metal_Panel_Albedo.png", false);

            Texture2D metalNormal = GenerateMetalPanelNormal(512, 512, 2.5f);
            SaveTexture(metalNormal, $"{TextureDir}/T_Lab_Metal_Panel_Normal.png", true);

            // 3. Weathered Bunker Concrete
            Texture2D concreteAlbedo = GenerateConcreteAlbedo(512, 512);
            SaveTexture(concreteAlbedo, $"{TextureDir}/T_Lab_Concrete_Albedo.png", false);

            // 4. Metal Diamond Grating (Noisy Floor)
            Texture2D grateAlbedo = GenerateGratingAlbedo(256, 256);
            SaveTexture(grateAlbedo, $"{TextureDir}/T_Lab_Metal_Grating_Albedo.png", false);

            // 5. Hazard Caution Stripes
            Texture2D hazardAlbedo = GenerateHazardStripes(256, 256);
            SaveTexture(hazardAlbedo, $"{TextureDir}/T_Lab_Hazard_Stripes.png", false);

            // 6. Retro Medical Monitor CRT Screen (Emissive)
            Texture2D monitorScreen = GenerateMonitorScreen(512, 256);
            SaveTexture(monitorScreen, $"{TextureDir}/T_Lab_Monitor_Screen.png", false);

            // 7. The Subject: Mottled Pale Skin & Shroud
            Texture2D monsterSkin = GenerateMonsterSkin(256, 256);
            SaveTexture(monsterSkin, $"{TextureDir}/T_Monster_Skin_Albedo.png", false);

            Texture2D monsterGown = GenerateMonsterGown(256, 256);
            SaveTexture(monsterGown, $"{TextureDir}/T_Monster_Gown_Albedo.png", false);

            // 8. Decayed Bone & Gothic Laboratory Elements
            Texture2D monsterBone = GenerateBoneAlbedo(256, 256);
            SaveTexture(monsterBone, $"{TextureDir}/T_Monster_Bone_Albedo.png", false);

            Texture2D autopsySteel = GenerateAutopsySteelAlbedo(512, 512);
            SaveTexture(autopsySteel, $"{TextureDir}/T_Lab_Autopsy_Steel_Albedo.png", false);

            Texture2D bloodDecal = GenerateBloodSplatter(256, 256);
            SaveTexture(bloodDecal, $"{TextureDir}/T_Lab_Blood_Decal.png", false);

            // 9. Gothic Asylum Ward & Corridor Textures (Image 1 & Image 2 Matching)
            Texture2D asylumGreenTilesAlbedo = GenerateAsylumGreenTilesAlbedo(512, 512, 16);
            SaveTexture(asylumGreenTilesAlbedo, $"{TextureDir}/T_Asylum_Green_Tiles_Albedo.png", false);

            Texture2D asylumGreenTilesNormal = GenerateAsylumGreenTilesNormal(512, 512, 16, 3.5f);
            SaveTexture(asylumGreenTilesNormal, $"{TextureDir}/T_Asylum_Green_Tiles_Normal.png", true);

            Texture2D asylumWardWallAlbedo = GenerateAsylumWardWallAlbedo(512, 512);
            SaveTexture(asylumWardWallAlbedo, $"{TextureDir}/T_Asylum_Ward_Wall_Albedo.png", false);

            Texture2D asylumCorridorFloorAlbedo = GenerateAsylumCorridorFloorAlbedo(512, 512);
            SaveTexture(asylumCorridorFloorAlbedo, $"{TextureDir}/T_Asylum_Corridor_Floor_Albedo.png", false);

            Texture2D asylumCorridorWallAlbedo = GenerateAsylumCorridorWallAlbedo(512, 512);
            SaveTexture(asylumCorridorWallAlbedo, $"{TextureDir}/T_Asylum_Corridor_Wall_Albedo.png", false);

            Texture2D asylumPatientPaper = GenerateAsylumPatientPaper(256, 256);
            SaveTexture(asylumPatientPaper, $"{TextureDir}/T_Asylum_Patient_Paper_Albedo.png", false);

            Texture2D asylumCeilingTiles = GenerateAsylumCeilingTiles(512, 512, 8);
            SaveTexture(asylumCeilingTiles, $"{TextureDir}/T_Asylum_Ceiling_Tiles_Albedo.png", false);

            Texture2D asylumDoorAlbedo = GenerateAsylumDoorAlbedo(256, 512);
            SaveTexture(asylumDoorAlbedo, $"{TextureDir}/T_Asylum_Door_Albedo.png", false);

            Texture2D asylumWindowAlbedo = GenerateAsylumWindowAlbedo(256, 512);
            SaveTexture(asylumWindowAlbedo, $"{TextureDir}/T_Asylum_Window_Bars_Albedo.png", false);

            AssetDatabase.Refresh();

            // Apply generated textures to existing Materials
            ApplyTexturesToMaterials();

            Debug.Log("[ProceduralTextureGenerator] All PBR textures generated and applied successfully!");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(TextureDir)) Directory.CreateDirectory(TextureDir);
            if (!Directory.Exists(MatDir)) Directory.CreateDirectory(MatDir);
        }

        #region Texture Generators

        private static Texture2D GenerateTilesAlbedo(int width, int height, int tileCount)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            int tileSize = width / tileCount;

            Color baseTileColor = new Color(0.72f, 0.77f, 0.73f); // Faded hospital greenish-white
            Color groutColor = new Color(0.18f, 0.18f, 0.20f);    // Dark aged grout

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int tx = x % tileSize;
                    int ty = y % tileSize;
                    bool isGrout = (tx <= 2 || tx >= tileSize - 2 || ty <= 2 || ty >= tileSize - 2);

                    if (isGrout)
                    {
                        tex.SetPixel(x, y, groutColor);
                    }
                    else
                    {
                        // Subtile variations, grunge, noise, and dirt stains
                        float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.12f;
                        float macroGrime = Mathf.PerlinNoise(x * 0.015f, y * 0.015f);
                        float grimeFactor = macroGrime > 0.65f ? (macroGrime - 0.65f) * 1.5f : 0f;

                        Color c = baseTileColor * (1.0f - noise);
                        c = Color.Lerp(c, new Color(0.35f, 0.32f, 0.26f), grimeFactor); // Grime discoloration
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateTilesNormal(int width, int height, int tileCount, float bumpStrength)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            int tileSize = width / tileCount;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int tx = x % tileSize;
                    int ty = y % tileSize;

                    float dx = 0f;
                    float dy = 0f;

                    // Beveled edge slope towards grout
                    if (tx < 5) dx = (5 - tx) * 0.18f;
                    else if (tx > tileSize - 5) dx = -(tx - (tileSize - 5)) * 0.18f;

                    if (ty < 5) dy = (5 - ty) * 0.18f;
                    else if (ty > tileSize - 5) dy = -(ty - (tileSize - 5)) * 0.18f;

                    Vector3 normal = new Vector3(dx * bumpStrength, dy * bumpStrength, 1.0f).normalized;
                    Color nColor = new Color(normal.x * 0.5f + 0.5f, normal.y * 0.5f + 0.5f, normal.z * 0.5f + 0.5f);
                    tex.SetPixel(x, y, nColor);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateMetalPanelAlbedo(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color steelBase = new Color(0.24f, 0.26f, 0.29f); // Cold industrial steel
            Color rustColor = new Color(0.42f, 0.22f, 0.12f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.15f;
                    float scratch = (Mathf.Sin(x * 2.2f + y * 0.4f) > 0.98f) ? -0.1f : 0f;
                    float rustPatch = Mathf.PerlinNoise(x * 0.02f + 50f, y * 0.02f + 50f);

                    Color c = steelBase * (1.0f + noise + scratch);
                    if (rustPatch > 0.72f)
                    {
                        c = Color.Lerp(c, rustColor, (rustPatch - 0.72f) * 3f);
                    }

                    // Seam line every 128 pixels
                    if (x % 128 == 0 || y % 128 == 0)
                    {
                        c *= 0.4f; // Dark seam groove
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateMetalPanelNormal(int width, int height, float strength)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = 0f;
                    float dy = 0f;

                    int seamX = x % 128;
                    int seamY = y % 128;

                    if (seamX < 3) dx = -0.5f;
                    else if (seamX > 125) dx = 0.5f;

                    if (seamY < 3) dy = -0.5f;
                    else if (seamY > 125) dy = 0.5f;

                    Vector3 n = new Vector3(dx * strength, dy * strength, 1.0f).normalized;
                    tex.SetPixel(x, y, new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f));
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateConcreteAlbedo(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color concrete = new Color(0.38f, 0.40f, 0.42f); // Brutalist damp grey

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float fineGrain = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.08f;
                    float coarseGrain = Mathf.PerlinNoise(x * 0.04f, y * 0.04f) * 0.14f;
                    float waterStain = Mathf.PerlinNoise(x * 0.01f, y * 0.03f);

                    float factor = 1.0f - fineGrain - coarseGrain;
                    if (waterStain > 0.6f) factor *= (1.0f - (waterStain - 0.6f) * 0.45f); // Dark water trickle

                    tex.SetPixel(x, y, concrete * factor);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateGratingAlbedo(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color metal = new Color(0.28f, 0.30f, 0.33f);
            Color hole = new Color(0.04f, 0.04f, 0.05f);

            int cell = 32;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int cx = x % cell - cell / 2;
                    int cy = y % cell - cell / 2;
                    // Diamond pattern: |cx| + |cy|
                    bool isHole = (Mathf.Abs(cx) + Mathf.Abs(cy)) < 9;
                    tex.SetPixel(x, y, isHole ? hole : metal);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateHazardStripes(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color yellow = new Color(0.92f, 0.78f, 0.12f);
            Color black = new Color(0.12f, 0.12f, 0.14f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool stripe = ((x + y) / 24) % 2 == 0;
                    float wear = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.15f;
                    Color c = (stripe ? yellow : black) * (1.0f - wear);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateMonitorScreen(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color darkGreen = new Color(0.01f, 0.05f, 0.02f);
            Color brightGreen = new Color(0.15f, 0.95f, 0.35f);
            Color alertRed = new Color(0.95f, 0.15f, 0.15f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Scanline pattern
                    float scanline = (y % 4 == 0) ? 0.7f : 1.0f;
                    Color c = darkGreen * scanline;

                    // ECG Vital wave line in middle
                    float midY = height * 0.45f;
                    float wave = Mathf.Sin(x * 0.06f) * 12f;
                    if (x % 160 > 130 && x % 160 < 145)
                    {
                        // QRS spike
                        wave = (x % 160 == 138) ? 45f : -20f;
                    }

                    if (Mathf.Abs(y - (midY + wave)) < 2.5f)
                    {
                        c = brightGreen;
                    }

                    // Header warning bar
                    if (y > height - 28 && y < height - 6 && (x % 40 < 30))
                    {
                        c = alertRed;
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateMonsterSkin(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color paleSkin = new Color(0.68f, 0.72f, 0.69f); // Sickly pale cadaverous green-grey
            Color veinColor = new Color(0.40f, 0.48f, 0.52f); // Dark venous discoloration

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.06f, y * 0.06f);
                    float veins = Mathf.PerlinNoise(x * 0.18f + 10f, y * 0.18f + 10f);

                    Color c = Color.Lerp(paleSkin, paleSkin * 0.75f, noise);
                    if (veins > 0.68f)
                    {
                        c = Color.Lerp(c, veinColor, (veins - 0.68f) * 2.5f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateMonsterGown(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color cloth = new Color(0.45f, 0.50f, 0.48f); // Torn dirty medical gown
            Color grime = new Color(0.22f, 0.20f, 0.18f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float weave = ((x + y) % 2 == 0) ? 0.95f : 1.05f;
                    float stain = Mathf.PerlinNoise(x * 0.03f, y * 0.03f);

                    Color c = cloth * weave;
                    if (stain > 0.55f)
                    {
                        c = Color.Lerp(c, grime, (stain - 0.55f) * 2.0f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateBoneAlbedo(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color boneBase = new Color(0.82f, 0.78f, 0.68f);
            Color driedBlood = new Color(0.38f, 0.08f, 0.05f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                    float stain = Mathf.PerlinNoise(x * 0.03f + 5f, y * 0.03f + 5f);

                    Color c = Color.Lerp(boneBase, boneBase * 0.8f, noise);
                    if (stain > 0.62f)
                    {
                        c = Color.Lerp(c, driedBlood, (stain - 0.62f) * 2.2f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateAutopsySteelAlbedo(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color steel = new Color(0.35f, 0.38f, 0.40f);
            Color rust = new Color(0.25f, 0.12f, 0.08f);
            Color blood = new Color(0.28f, 0.04f, 0.04f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float brush = Mathf.PerlinNoise(x * 0.02f, y * 0.5f) * 0.1f;
                    float rustStain = Mathf.PerlinNoise(x * 0.04f, y * 0.04f);
                    float bloodDrip = Mathf.PerlinNoise(x * 0.03f + 8f, y * 0.15f);

                    Color c = steel + new Color(brush, brush, brush);
                    if (rustStain > 0.65f)
                    {
                        c = Color.Lerp(c, rust, (rustStain - 0.65f) * 2.5f);
                    }
                    if (bloodDrip > 0.70f)
                    {
                        c = Color.Lerp(c, blood, (bloodDrip - 0.70f) * 3.0f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateBloodSplatter(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color transparent = new Color(0, 0, 0, 0);
            Color arterialBlood = new Color(0.42f, 0.02f, 0.02f, 0.92f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float n1 = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                    float n2 = Mathf.PerlinNoise(x * 0.25f + 3f, y * 0.25f + 3f);
                    float combined = n1 * n2;

                    if (combined > 0.32f)
                    {
                        float alpha = Mathf.Clamp01((combined - 0.32f) * 4f);
                        tex.SetPixel(x, y, new Color(arterialBlood.r, arterialBlood.g, arterialBlood.b, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateAsylumGreenTilesAlbedo(int width, int height, int tileCount)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            int tileSize = width / tileCount;
            Color baseTileColor = new Color(0.18f, 0.32f, 0.28f); // Dirty deep teal green
            Color baseTileColor2 = new Color(0.24f, 0.40f, 0.34f); // Brighter variation
            Color groutColor = new Color(0.06f, 0.08f, 0.07f);    // Aged dark grout
            Color puddleColor = new Color(0.08f, 0.16f, 0.14f);   // Wet slimy puddle

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int tx = x % tileSize;
                    int ty = y % tileSize;
                    bool isGrout = (tx <= 1 || tx >= tileSize - 2 || ty <= 1 || ty >= tileSize - 2);

                    if (isGrout)
                    {
                        tex.SetPixel(x, y, groutColor);
                    }
                    else
                    {
                        float perlin = Mathf.PerlinNoise(x * 0.04f, y * 0.04f);
                        float grime = Mathf.PerlinNoise(x * 0.12f, y * 0.12f) * 0.2f;
                        float puddleNoise = Mathf.PerlinNoise(x * 0.02f, y * 0.02f);

                        Color tile = Color.Lerp(baseTileColor, baseTileColor2, perlin);
                        tile = Color.Lerp(tile, Color.black, grime);

                        if (puddleNoise > 0.62f)
                        {
                            tile = Color.Lerp(tile, puddleColor, (puddleNoise - 0.62f) * 2.5f);
                        }

                        tex.SetPixel(x, y, tile);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateAsylumGreenTilesNormal(int width, int height, int tileCount, float bumpStrength)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            int tileSize = width / tileCount;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int tx = x % tileSize;
                    int ty = y % tileSize;

                    float dx = 0f;
                    float dy = 0f;

                    if (tx < 4) dx = (4 - tx) * 0.22f;
                    else if (tx > tileSize - 4) dx = -(tx - (tileSize - 4)) * 0.22f;

                    if (ty < 4) dy = (4 - ty) * 0.22f;
                    else if (ty > tileSize - 4) dy = -(ty - (tileSize - 4)) * 0.22f;

                    Vector3 normal = new Vector3(dx * bumpStrength, dy * bumpStrength, 1.0f).normalized;
                    Color nColor = new Color(normal.x * 0.5f + 0.5f, normal.y * 0.5f + 0.5f, normal.z * 0.5f + 0.5f);
                    tex.SetPixel(x, y, nColor);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateAsylumWardWallAlbedo(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            int tileSize = width / 12; // Wall tile wainscot
            Color tileDark = new Color(0.14f, 0.22f, 0.20f);
            Color tileLight = new Color(0.20f, 0.30f, 0.27f);
            Color grout = new Color(0.07f, 0.08f, 0.07f);
            Color railWood = new Color(0.12f, 0.09f, 0.07f);
            Color plasterBase = new Color(0.66f, 0.68f, 0.60f); // Peeling hospital wall
            Color waterStain = new Color(0.32f, 0.34f, 0.26f);

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    if (v < 0.44f)
                    {
                        // Lower 44%: Ceramic square tile wainscot
                        int tx = x % tileSize;
                        int ty = y % tileSize;
                        bool isGrout = (tx <= 1 || tx >= tileSize - 2 || ty <= 1 || ty >= tileSize - 2);
                        if (isGrout)
                        {
                            tex.SetPixel(x, y, grout);
                        }
                        else
                        {
                            float n = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                            tex.SetPixel(x, y, Color.Lerp(tileDark, tileLight, n));
                        }
                    }
                    else if (v <= 0.47f)
                    {
                        // Wainscot chair rail trim
                        tex.SetPixel(x, y, railWood);
                    }
                    else
                    {
                        // Upper plaster wall with water stains & vertical peeling paint streaks
                        float noise = Mathf.PerlinNoise(x * 0.03f, y * 0.03f);
                        float verticalStreak = Mathf.PerlinNoise(x * 0.08f, y * 0.005f);
                        Color c = Color.Lerp(plasterBase, waterStain, verticalStreak * 0.7f);
                        c *= (0.85f + noise * 0.25f);
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateAsylumCorridorFloorAlbedo(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            // Diamond harlequin lattice pattern matching Image 2
            int diamondSize = width / 8;
            Color darkDiamond = new Color(0.20f, 0.12f, 0.10f); // Aged dark mahogany
            Color lightDiamond = new Color(0.38f, 0.32f, 0.22f); // Faded ochre / tan
            Color edgeGrout = new Color(0.06f, 0.05f, 0.05f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 45 degree diamond formula
                    int u = (x + y) / diamondSize;
                    int v = (x - y + 10000 * diamondSize) / diamondSize;
                    bool isEven = ((u + v) % 2 == 0);

                    // Diamond edge lines
                    int du = (x + y) % diamondSize;
                    int dv = (x - y + 10000 * diamondSize) % diamondSize;
                    bool isBorder = (du <= 1 || du >= diamondSize - 2 || dv <= 1 || dv >= diamondSize - 2);

                    if (isBorder)
                    {
                        tex.SetPixel(x, y, edgeGrout);
                    }
                    else
                    {
                        float grunge = Mathf.PerlinNoise(x * 0.04f, y * 0.04f) * 0.3f;
                        Color c = isEven ? lightDiamond : darkDiamond;
                        c *= (1.0f - grunge);
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateAsylumCorridorWallAlbedo(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color beadboard = new Color(0.14f, 0.10f, 0.07f);
            Color beadboardGroove = new Color(0.06f, 0.04f, 0.03f);
            Color mouldRail = new Color(0.10f, 0.07f, 0.05f);
            Color wallpaperStripe1 = new Color(0.35f, 0.32f, 0.20f); // Olive gold
            Color wallpaperStripe2 = new Color(0.26f, 0.23f, 0.15f); // Deep muted olive
            Color wallpaperTear = new Color(0.15f, 0.12f, 0.10f);

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    if (v < 0.40f)
                    {
                        // Vertical wooden beadboard slats
                        int slat = x % 16;
                        if (slat <= 1)
                        {
                            tex.SetPixel(x, y, beadboardGroove);
                        }
                        else
                        {
                            float woodGrain = Mathf.PerlinNoise(x * 0.2f, y * 0.02f) * 0.15f;
                            tex.SetPixel(x, y, beadboard * (1f - woodGrain));
                        }
                    }
                    else if (v <= 0.43f)
                    {
                        tex.SetPixel(x, y, mouldRail);
                    }
                    else
                    {
                        // Victorian striped wallpaper with peeling damage
                        int stripe = (x / 24) % 2;
                        Color c = stripe == 0 ? wallpaperStripe1 : wallpaperStripe2;
                        float damage = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                        if (damage > 0.70f)
                        {
                            c = Color.Lerp(c, wallpaperTear, (damage - 0.70f) * 3f);
                        }
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateAsylumPatientPaper(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color paperBase = new Color(0.88f, 0.85f, 0.76f);
            Color paperEdge = new Color(0.55f, 0.48f, 0.35f);
            Color inkLine = new Color(0.20f, 0.20f, 0.22f);
            Color redStamp = new Color(0.75f, 0.12f, 0.12f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float edgeDist = Mathf.Min(Mathf.Min(x, width - x), Mathf.Min(y, height - y));
                    float edgeNoise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 8f;

                    if (edgeDist + edgeNoise < 6)
                    {
                        tex.SetPixel(x, y, paperEdge);
                        continue;
                    }

                    Color c = paperBase;
                    // Typewritten horizontal text lines
                    if (y > 30 && y < height - 30 && (y % 14 <= 2) && x > 25 && x < width - 25)
                    {
                        float inkNoise = Mathf.PerlinNoise(x * 0.2f, y * 0.5f);
                        if (inkNoise > 0.35f) c = Color.Lerp(c, inkLine, 0.75f);
                    }

                    // Red stamp in center: "CONTAINMENT"
                    if (x > width * 0.35f && x < width * 0.75f && y > height * 0.45f && y < height * 0.65f)
                    {
                        bool isBorder = (x <= width * 0.35f + 3 || x >= width * 0.75f - 3 || y <= height * 0.45f + 3 || y >= height * 0.65f - 3);
                        if (isBorder || Mathf.PerlinNoise(x * 0.3f, y * 0.3f) > 0.4f)
                        {
                            c = Color.Lerp(c, redStamp, 0.65f);
                        }
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateAsylumCeilingTiles(int width, int height, int grid)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            int cellSize = width / grid;
            Color tileFace = new Color(0.19f, 0.20f, 0.19f);
            Color gridFrame = new Color(0.08f, 0.09f, 0.08f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int tx = x % cellSize;
                    int ty = y % cellSize;
                    bool isFrame = (tx <= 3 || tx >= cellSize - 4 || ty <= 3 || ty >= cellSize - 4);
                    if (isFrame)
                    {
                        tex.SetPixel(x, y, gridFrame);
                    }
                    else
                    {
                        float n = Mathf.PerlinNoise(x * 0.06f, y * 0.06f) * 0.15f;
                        tex.SetPixel(x, y, tileFace * (1f - n));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateAsylumDoorAlbedo(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color doorWood = new Color(0.16f, 0.13f, 0.10f);
            Color brassPlate = new Color(0.65f, 0.52f, 0.22f);
            Color glassDark = new Color(0.12f, 0.20f, 0.18f);

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    // Lower kickplate
                    if (v < 0.18f && u > 0.08f && u < 0.92f)
                    {
                        tex.SetPixel(x, y, brassPlate);
                    }
                    // Observation glass window
                    else if (v > 0.58f && v < 0.88f && u > 0.22f && u < 0.78f)
                    {
                        bool isWire = (x % 16 <= 1 || y % 16 <= 1);
                        tex.SetPixel(x, y, isWire ? Color.black : glassDark);
                    }
                    else
                    {
                        float wood = Mathf.PerlinNoise(x * 0.1f, y * 0.01f) * 0.15f;
                        tex.SetPixel(x, y, doorWood * (1f - wood));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateAsylumWindowAlbedo(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color ironBar = new Color(0.08f, 0.09f, 0.10f);
            Color darkGlass = new Color(0.05f, 0.09f, 0.12f); // Night fog blue-grey glass

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int cellX = x % (width / 4);
                    int cellY = y % (height / 6);
                    bool isBar = (cellX <= 4 || cellX >= (width / 4) - 5 || cellY <= 4 || cellY >= (height / 6) - 5);
                    if (isBar)
                    {
                        tex.SetPixel(x, y, ironBar);
                    }
                    else
                    {
                        float grime = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.2f;
                        tex.SetPixel(x, y, darkGlass * (1f - grime));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        #endregion

        #region Saving & Material Setup

        private static void SaveTexture(Texture2D tex, string path, bool isNormal)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                if (isNormal)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                }
                else
                {
                    importer.textureType = TextureImporterType.Default;
                }
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        public static void ApplyTexturesToMaterials()
        {
            Texture2D tilesAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Lab_Tiles_Dirty_Albedo.png");
            Texture2D tilesNormal = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Lab_Tiles_Dirty_Normal.png");

            Texture2D metalAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Lab_Metal_Panel_Albedo.png");
            Texture2D metalNormal = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Lab_Metal_Panel_Normal.png");

            Texture2D concreteAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Lab_Concrete_Albedo.png");
            Texture2D gratingAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Lab_Metal_Grating_Albedo.png");
            Texture2D hazardAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Lab_Hazard_Stripes.png");
            Texture2D monitorScreen = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Lab_Monitor_Screen.png");

            Texture2D monsterSkin = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Monster_Skin_Albedo.png");
            Texture2D monsterGown = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Monster_Gown_Albedo.png");

            // 1. Floor Tiles
            Material floorTilesMat = GetMaterial("M_Lab_Floor_Tiles");
            if (floorTilesMat != null)
            {
                if (tilesAlbedo != null) floorTilesMat.mainTexture = tilesAlbedo;
                if (tilesNormal != null)
                {
                    floorTilesMat.EnableKeyword("_NORMALMAP");
                    floorTilesMat.SetTexture("_BumpMap", tilesNormal);
                }
                floorTilesMat.SetFloat("_Glossiness", 0.45f);
                floorTilesMat.mainTextureScale = new Vector2(4f, 4f);
            }

            // 2. Clean/Faded Lab Wall
            Material wallCleanMat = GetMaterial("M_Lab_Wall_Clean");
            if (wallCleanMat != null)
            {
                if (tilesAlbedo != null) wallCleanMat.mainTexture = tilesAlbedo;
                if (tilesNormal != null)
                {
                    wallCleanMat.EnableKeyword("_NORMALMAP");
                    wallCleanMat.SetTexture("_BumpMap", tilesNormal);
                }
                wallCleanMat.SetFloat("_Glossiness", 0.25f);
                wallCleanMat.mainTextureScale = new Vector2(3f, 2f);
            }

            // 3. Concrete Walls & Pillars
            Material wallMat = GetMaterial("M_Lab_Wall");
            if (wallMat != null)
            {
                if (concreteAlbedo != null) wallMat.mainTexture = concreteAlbedo;
                wallMat.SetFloat("_Glossiness", 0.15f);
                wallMat.mainTextureScale = new Vector2(2f, 2f);
            }

            Material pillarMat = GetMaterial("M_Lab_Pillar");
            if (pillarMat != null)
            {
                if (concreteAlbedo != null) pillarMat.mainTexture = concreteAlbedo;
                pillarMat.SetFloat("_Glossiness", 0.15f);
            }

            // 4. Metal Panels & Doors
            Material metalMat = GetMaterial("M_Lab_Metal");
            if (metalMat != null)
            {
                if (metalAlbedo != null) metalMat.mainTexture = metalAlbedo;
                if (metalNormal != null)
                {
                    metalMat.EnableKeyword("_NORMALMAP");
                    metalMat.SetTexture("_BumpMap", metalNormal);
                }
                metalMat.SetFloat("_Metallic", 0.65f);
                metalMat.SetFloat("_Glossiness", 0.55f);
                metalMat.mainTextureScale = new Vector2(2f, 2f);
            }

            // 5. Metal Floor Grating
            Material gratingMat = GetMaterial("M_Lab_Metal_Grating");
            if (gratingMat != null)
            {
                if (gratingAlbedo != null) gratingMat.mainTexture = gratingAlbedo;
                gratingMat.SetFloat("_Metallic", 0.85f);
                gratingMat.SetFloat("_Glossiness", 0.60f);
                gratingMat.mainTextureScale = new Vector2(4f, 8f);
            }

            // 6. Hazard Stripes
            Material hazardMat = GetMaterial("M_Lab_Hazard");
            if (hazardMat != null)
            {
                if (hazardAlbedo != null) hazardMat.mainTexture = hazardAlbedo;
                hazardMat.mainTextureScale = new Vector2(6f, 1f);
            }

            // 7. Monitor Screen Material
            Material screenMat = GetOrCreateMaterial("M_Lab_Monitor_Screen", Color.black, 0.8f);
            if (screenMat != null && monitorScreen != null)
            {
                screenMat.mainTexture = monitorScreen;
                screenMat.EnableKeyword("_EMISSION");
                screenMat.SetColor("_EmissionColor", Color.white);
                screenMat.SetTexture("_EmissionMap", monitorScreen);
            }

            // 8. The Subject Materials
            Material skinMat = GetMaterial("M_Monster_Skin");
            if (skinMat != null && monsterSkin != null)
            {
                skinMat.mainTexture = monsterSkin;
                skinMat.SetFloat("_Glossiness", 0.35f);
            }

            Material gownMat = GetOrCreateMaterial("M_Monster_Gown", new Color(0.4f, 0.45f, 0.42f), 0.1f);
            if (gownMat != null && monsterGown != null)
            {
                gownMat.mainTexture = monsterGown;
                gownMat.SetFloat("_Glossiness", 0.08f);
            }

            // 9. Bone & Autopsy Steel Materials
            Texture2D monsterBone = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Monster_Bone_Albedo.png");
            Material boneMat = GetOrCreateMaterial("M_Monster_Bone", new Color(0.82f, 0.78f, 0.68f), 0.25f);
            if (boneMat != null && monsterBone != null)
            {
                boneMat.mainTexture = monsterBone;
            }

            Texture2D autopsySteel = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Lab_Autopsy_Steel_Albedo.png");
            Material autopsyMat = GetOrCreateMaterial("M_Lab_Autopsy_Steel", new Color(0.35f, 0.38f, 0.40f), 0.65f);
            if (autopsyMat != null && autopsySteel != null)
            {
                autopsyMat.mainTexture = autopsySteel;
                autopsyMat.SetFloat("_Metallic", 0.85f);
            }

            Texture2D bloodDecal = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Lab_Blood_Decal.png");
            Material bloodMat = GetOrCreateMaterial("M_Lab_Blood_Decal", new Color(0.42f, 0.02f, 0.02f), 0.45f);
            if (bloodMat != null && bloodDecal != null)
            {
                bloodMat.mainTexture = bloodDecal;
            }

            // 10. Gothic Asylum Ward & Corridor Materials (Image 1 & Image 2)
            Texture2D greenTilesAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Asylum_Green_Tiles_Albedo.png");
            Texture2D greenTilesNormal = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Asylum_Green_Tiles_Normal.png");
            Material greenTilesMat = GetOrCreateMaterial("M_Asylum_Green_Tiles", new Color(0.20f, 0.35f, 0.30f), 0.58f);
            if (greenTilesMat != null)
            {
                if (greenTilesAlbedo != null) greenTilesMat.mainTexture = greenTilesAlbedo;
                if (greenTilesNormal != null)
                {
                    greenTilesMat.EnableKeyword("_NORMALMAP");
                    greenTilesMat.SetTexture("_BumpMap", greenTilesNormal);
                }
                greenTilesMat.SetFloat("_Glossiness", 0.62f);
                greenTilesMat.mainTextureScale = new Vector2(4f, 4f);
            }

            Texture2D wardWallAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Asylum_Ward_Wall_Albedo.png");
            Material wardWallMat = GetOrCreateMaterial("M_Asylum_Ward_Wall", new Color(0.55f, 0.58f, 0.52f), 0.20f);
            if (wardWallMat != null && wardWallAlbedo != null)
            {
                wardWallMat.mainTexture = wardWallAlbedo;
                wardWallMat.SetFloat("_Glossiness", 0.22f);
                wardWallMat.mainTextureScale = new Vector2(3f, 1f);
            }

            Texture2D corridorFloorAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Asylum_Corridor_Floor_Albedo.png");
            Material corridorFloorMat = GetOrCreateMaterial("M_Asylum_Corridor_Floor", new Color(0.28f, 0.20f, 0.16f), 0.32f);
            if (corridorFloorMat != null && corridorFloorAlbedo != null)
            {
                corridorFloorMat.mainTexture = corridorFloorAlbedo;
                corridorFloorMat.SetFloat("_Glossiness", 0.35f);
                corridorFloorMat.mainTextureScale = new Vector2(2f, 8f);
            }

            Texture2D corridorWallAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Asylum_Corridor_Wall_Albedo.png");
            Material corridorWallMat = GetOrCreateMaterial("M_Asylum_Corridor_Wall", new Color(0.24f, 0.22f, 0.18f), 0.15f);
            if (corridorWallMat != null && corridorWallAlbedo != null)
            {
                corridorWallMat.mainTexture = corridorWallAlbedo;
                corridorWallMat.SetFloat("_Glossiness", 0.18f);
                corridorWallMat.mainTextureScale = new Vector2(3f, 1f);
            }

            Texture2D patientPaperTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Asylum_Patient_Paper_Albedo.png");
            Material paperMat = GetOrCreateMaterial("M_Asylum_Patient_Paper", Color.white, 0.10f);
            if (paperMat != null && patientPaperTex != null)
            {
                paperMat.mainTexture = patientPaperTex;
                paperMat.SetFloat("_Glossiness", 0.08f);
            }

            Texture2D ceilingTilesTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Asylum_Ceiling_Tiles_Albedo.png");
            Material ceilingMat = GetOrCreateMaterial("M_Asylum_Ceiling_Tiles", new Color(0.18f, 0.19f, 0.18f), 0.12f);
            if (ceilingMat != null && ceilingTilesTex != null)
            {
                ceilingMat.mainTexture = ceilingTilesTex;
                ceilingMat.SetFloat("_Glossiness", 0.12f);
                ceilingMat.mainTextureScale = new Vector2(4f, 4f);
            }

            Texture2D doorTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Asylum_Door_Albedo.png");
            Material doorMat = GetOrCreateMaterial("M_Asylum_Door", new Color(0.22f, 0.18f, 0.15f), 0.40f);
            if (doorMat != null && doorTex != null)
            {
                doorMat.mainTexture = doorTex;
                doorMat.SetFloat("_Glossiness", 0.40f);
            }

            Texture2D windowTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureDir}/T_Asylum_Window_Bars_Albedo.png");
            Material windowMat = GetOrCreateMaterial("M_Asylum_Window_Bars", new Color(0.10f, 0.12f, 0.14f), 0.65f);
            if (windowMat != null && windowTex != null)
            {
                windowMat.mainTexture = windowTex;
                windowMat.SetFloat("_Glossiness", 0.65f);
                windowMat.SetFloat("_Metallic", 0.50f);
            }

            AssetDatabase.SaveAssets();
        }

        private static Material GetMaterial(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>($"{MatDir}/{name}.mat");
        }

        private static Material GetOrCreateMaterial(string name, Color color, float smoothness)
        {
            string path = $"{MatDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"))
                {
                    color = color
                };
                mat.SetFloat("_Glossiness", smoothness);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        #endregion
    }
}
#endif
