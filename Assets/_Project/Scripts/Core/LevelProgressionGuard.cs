using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeTheLab.Core
{
    /// <summary>
    /// Enforces level progression security:
    /// Ensures LaboratoryLevel2 can only be accessed after completing LaboratoryLevel (Level 1).
    /// If Level 2 is locked, safely redirects the player back to MainMenu with an access violation log.
    /// </summary>
    public class LevelProgressionGuard : MonoBehaviour
    {
        private void Awake()
        {
            int unlocked = PlayerPrefs.GetInt("ProjectVANTA_Level2Unlocked", 0);
            if (unlocked == 0)
            {
                Debug.LogWarning("[LevelProgressionGuard] ACCESS DENIED: Sector 2 (Sub-Basement Cryo) is locked! Escape Level 1 (Upper Labs) first. Redirecting to MainMenu...");
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                Debug.Log("[LevelProgressionGuard] Clearance verified: Sub-Basement Cryo Labs unlocked.");
            }
        }
    }
}
