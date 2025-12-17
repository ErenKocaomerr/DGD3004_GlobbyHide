using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Targets")]
    public List<FleeTarget> targets = new List<FleeTarget>();

    [Header("Timer")]
    public float levelTime = 60f;
    float timeLeft;
    public TMP_Text timerText;
    public TMP_Text statusText;
    public AudioSource AudioSource;

    int caughtCount = 0;
    bool levelEnded = false;

    public GameObject succesPanel;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    void Start()
    {
        timeLeft = levelTime;
        // Eðer targets listesi boþsa sahnedekileri otomatik topla
        if (targets.Count == 0)
        {
            FleeTarget[] found = GameObject.FindObjectsOfType<FleeTarget>();
            targets.AddRange(found);
        }
        UpdateUI();
    }

    void Update()
    {
        if (levelEnded) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            Lose("Time up!");
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();
        if (statusText != null)
            statusText.text = $"Caught {caughtCount}/{targets.Count}";
    }

    public void OnTargetFinalReached(FleeTarget target)
    {
        // hedef artýk yakalanabilir, istenirse iþaret/FX koy
        Debug.Log("Target is now catchable: " + target.name);
    }

    public void OnTargetCaught(FleeTarget target)
    {
        caughtCount++;
        Debug.Log("Target caught: " + target.name + " total: " + caughtCount);
        UpdateUI();
        CheckWin();
    }

    void CheckWin()
    {
        if (caughtCount >= targets.Count)
        {
            Win("All caught!");
        }
    }

    void Win(string reason)
    {
        levelEnded = true;
        Debug.Log("YOU WIN! " + reason);
        if (statusText != null) statusText.text = "WIN!";

        AudioSource.enabled = false;

        succesPanel.SetActive(true);
    }

    public void ReturnTown()
    {
        EndManager.Instance.value++;
        SceneManager.LoadScene("TownScene");
    }


    void Lose(string reason)
    {
        levelEnded = true;
        Debug.Log("YOU LOSE! " + reason);
        if (statusText != null) statusText.text = "LOSE!";
    }
}
