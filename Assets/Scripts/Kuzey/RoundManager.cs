using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("Spawners")]
    [Tooltip("Optional: add stone spawners here; otherwise spawners can auto-register on Start.")]
    public List<StoneSpawner> spawners = new List<StoneSpawner>();

    [Header("Round")]
    public float roundDuration = 30f;
    public TMP_Text timerText;
    public TMP_Text resultText;
    public GameObject succesPanel;
    public Button startButton;
    public Button returnButton;
    public Button retryButton;
    public GameObject uiRoot; // root for minigame UI
    public AudioSource audioSource;
    public AudioClip canSesi;
    public AudioClip rockSesi;


    bool roundRunning = false;
    bool failed = false;
    Coroutine roundRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        Instance = this;
    }

    void Start()
    {
        if (resultText != null) resultText.text = "";
        if (timerText != null) timerText.text = "";
        if (uiRoot != null) uiRoot.SetActive(false);
    }

    // Called by StoneSpawner on Start if autoRegister true
    public void RegisterSpawner(StoneSpawner s)
    {
        if (s == null) return;
        if (!spawners.Contains(s)) spawners.Add(s);
    }

    public void UnregisterSpawner(StoneSpawner s)
    {
        if (s == null) return;
        if (spawners.Contains(s)) spawners.Remove(s);
    }

    public void StartRound()
    {
        if (roundRunning) return;

        failed = false;
        if (uiRoot != null) uiRoot.SetActive(true);

        // reset UI
        if (resultText != null) resultText.text = "";
        if (timerText != null) timerText.text = $"{roundDuration:F1}s";

        // start all registered spawners
        foreach (var s in spawners)
        {
            if (s != null) s.roundManagerRef = this;
            if (s != null) s.StartSpawning();
        }

        StartCoroutine(SesRoutine());

        roundRoutine = StartCoroutine(RoundTimer());
        roundRunning = true;

        startButton.gameObject.SetActive(false);
    }

    IEnumerator SesRoutine() 
    {
        audioSource.PlayOneShot(canSesi);
        yield return new WaitForSeconds(2);
        audioSource.PlayOneShot(rockSesi);
        yield return new WaitForSeconds(5);
        audioSource.PlayOneShot(rockSesi);
        audioSource.PlayOneShot(canSesi);
        yield return new WaitForSeconds(5);
        audioSource.PlayOneShot(rockSesi);
        audioSource.PlayOneShot(canSesi);
        yield return new WaitForSeconds(5);
        audioSource.PlayOneShot(rockSesi);
        audioSource.PlayOneShot(canSesi);
        yield return new WaitForSeconds(5);
        audioSource.PlayOneShot(canSesi);
        audioSource.PlayOneShot(rockSesi);
        yield return new WaitForSeconds(5);
        audioSource.PlayOneShot(canSesi);
        audioSource.PlayOneShot(rockSesi);
    }

    IEnumerator RoundTimer()
    {
        float t = 0f;
        while (t < roundDuration)
        {
            t += Time.deltaTime;
            if (timerText != null) timerText.text = $"{(roundDuration - t):F1}s";
            if (failed) yield break;
            yield return null;
        }

        OnRoundSuccess();
    }

    public void OnGirlHit(Stone stone)
    {
        if (!roundRunning) return;
        failed = true;
        OnRoundFail();
    }

    void OnRoundFail()
    {
        roundRunning = false;
        foreach (var s in spawners) if (s != null) s.StopSpawning();
        foreach (var s in spawners) if (s != null) s.ClearAll();
        if (resultText != null) resultText.text = "Fail!";
        retryButton.gameObject.SetActive(true);
        returnButton.gameObject.SetActive(false);
        succesPanel.SetActive(true);
        // optionally cleanup remaining stones
    }

    void OnRoundSuccess()
    {
        roundRunning = false;
        foreach (var s in spawners) if (s != null) s.StopSpawning();
        if (resultText != null) resultText.text = "Success!";
        retryButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(true);
        succesPanel.SetActive(true);

        audioSource.enabled = false;
    }

    public void ReturnTown() 
    {
        GameManager.instance.isReturningToHub = true;
        SceneManager.LoadScene("NewHub");
    }

    public void TryAgain() 
    {
        succesPanel.SetActive(false);
        StartRound();
    }

    public void CancelRound()
    {
        if (!roundRunning) return;
        roundRunning = false;
        failed = true;
        foreach (var s in spawners) if (s != null) s.ClearAll();
        if (resultText != null) resultText.text = "Canceled";
    }

    public bool IsRunning() => roundRunning;
}

