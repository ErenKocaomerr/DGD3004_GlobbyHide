using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public Button School;
    public Button Monster;
    public Button Kopru;

    public GameObject successPanel;


    void Update()
    {
        if (EndManager.Instance != null && EndManager.Instance.value == 3)
        {
            successPanel.SetActive(true);
        }
        else
        {
            successPanel.SetActive(false);
        }

        Debug.Log(Time.timeScale);
    }

    public void LoadSchool()
    {
        School.gameObject.SetActive(false);
        SceneManager.LoadScene("Gas"); 
        Time.timeScale = 1.0f;
    }

    public void LoadMonsterHunt()
    {
        Monster.gameObject.SetActive(false);
        SceneManager.LoadScene("Ritim");
        Time.timeScale = 1.0f;
    }

    public void LoadBroken()
    {
        Kopru.gameObject.SetActive(false);
        SceneManager.LoadScene("Broken");
        Time.timeScale = 1.0f;
    }
}
