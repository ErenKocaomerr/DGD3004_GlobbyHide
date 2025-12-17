using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject creditsPanel;

    [Header("Scene Settings")]
    public string gameSceneName = "TowerScene"; // Oyun sahnesinin ismini buraya yaz

    bool creditsOpen = false;

    // PLAY
    public void PlayGame()
    {
        // Eðer sahne ismi boþ deðilse o sahneyi yükle
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogWarning("[MainMenuManager] Game Scene name is empty!");
        }
    }

    // CREDITS
    public void ToggleCredits()
    {
        if (creditsPanel == null)
        {
            Debug.LogWarning("[MainMenuManager] Credits panel not assigned!");
            return;
        }

        creditsOpen = !creditsOpen;
        creditsPanel.SetActive(creditsOpen);
    }

    public void ExitGame() 
    {
        Application.Quit();
    }
}
