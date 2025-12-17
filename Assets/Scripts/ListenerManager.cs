using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ListenerManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject uiRoot;
    public Slider slider;
    public TMP_Text resultText;
    public TMP_Text instructionText;

    [Header("Optional visual")]
    public RectTransform stopMarker;

    [Header("Growth settings")]
    public float baseGrowthSpeed = 0.15f;
    public float growthAcceleration = 0.35f;
    public float maxGrowthSpeed = 3.0f;
    [Range(0f, 1f)] public float startNormalized = 0f;
    public bool randomStartEnabled = false;
    [Range(0f, 1f)] public float randomRangeMin = 0f;
    [Range(0f, 1f)] public float randomRangeMax = 0.3f;

    [Header("Target / Success")]
    [Range(0f, 1f)] public float targetNormalized = 0.5f;
    [Range(0f, 0.5f)] public float toleranceNormalized = 0.05f;

    [Header("Flow")]
    public UnityEvent<bool> OnMinigameEnd; // true = success

    [Header("Position-based speed")]
    public AnimationCurve positionSpeedCurve = null;
    [Range(0f, 0.5f)]
    public float minEffectiveSpeed = 0.02f;

    [Header("Audio")]
    [Tooltip("Basılı tutarken çalacak looping ses. AudioSource ekleyip Inspector'dan atayın.")]
    public AudioSource holdAudio;

    // internal
    bool running = false;
    bool stopped = false;
    GameObject playerRef;

    // hold logic
    bool isHolding = false;
    float heldTime = 0f;
    float currentSpeed = 0f; // base speed before pos multiplier
    float holdDirection = 1f; // 1 => increasing, -1 => decreasing (ping-pong)

    public bool IsRunning => running;

    void Start()
    {
        if (uiRoot != null) uiRoot.SetActive(false);
        if (instructionText != null) instructionText.text = "Hold Space to grow. Release to stop. R = retry, Esc = cancel.";
        if (resultText != null) resultText.text = "";
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.interactable = false;
        }
        if (stopMarker != null) stopMarker.gameObject.SetActive(false);

        // ensure holdAudio not playing initially
        if (holdAudio != null)
        {
            holdAudio.loop = true;
            holdAudio.Stop();
        }
    }

    void Update()
    {
        if (!running) return;

        if (!stopped)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (!isHolding)
                {
                    isHolding = true;
                    heldTime = 0f;
                    currentSpeed = baseGrowthSpeed;
                    holdDirection = 1f;

                    // start audio
                    StartHoldSound();
                }

                heldTime += Time.deltaTime;
                currentSpeed = baseGrowthSpeed + growthAcceleration * heldTime;
                if (maxGrowthSpeed > 0f) currentSpeed = Mathf.Min(currentSpeed, maxGrowthSpeed);

                float pos = (slider != null) ? slider.value : 0f;
                float posMultiplier = 1f;
                if (positionSpeedCurve != null) posMultiplier = positionSpeedCurve.Evaluate(pos);
                else posMultiplier = Mathf.Sin(Mathf.PI * pos);
                posMultiplier = Mathf.Max(posMultiplier, minEffectiveSpeed);

                float effectiveSpeed = currentSpeed * posMultiplier;

                if (slider != null)
                {
                    float newVal = slider.value + holdDirection * effectiveSpeed * Time.deltaTime;

                    if (newVal >= 1f)
                    {
                        slider.value = 1f;
                        holdDirection = -1f;
                    }
                    else if (newVal <= 0f)
                    {
                        slider.value = 0f;
                        holdDirection = 1f;
                    }
                    else
                    {
                        slider.value = newVal;
                    }

                    UpdateStopMarkerPosition();
                }
            }
            else
            {
                if (isHolding)
                {
                    isHolding = false;
                    stopped = true;

                    // stop audio on release
                    StopHoldSound();

                    EvaluateStop();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                // ensure audio stopped when cancelling
                StopHoldSound();
                EndMinigame(false);
            }
        }
    }

    // Start from ListenerTrigger (overload passes player and target)
    public void StartMinigame(GameObject player, float targetNorm)
    {
        targetNormalized = Mathf.Clamp01(targetNorm);
        StartMinigame(player);
    }

    public void StartMinigame(GameObject player)
    {
        if (running) return;
        running = true;
        stopped = false;
        playerRef = player;

        var ctrl = playerRef.GetComponent<BasicTopdownController>();
        if (ctrl != null) ctrl.enabled = false;

        if (uiRoot != null) uiRoot.SetActive(true);
        if (resultText != null) resultText.text = "";

        float chosenStart = startNormalized;
        if (randomStartEnabled)
        {
            float mn = Mathf.Min(randomRangeMin, randomRangeMax);
            float mx = Mathf.Max(randomRangeMin, randomRangeMax);
            chosenStart = Random.Range(mn, mx);
        }
        chosenStart = Mathf.Clamp01(chosenStart);

        if (slider != null)
        {
            slider.value = chosenStart;
            slider.interactable = false;
        }

        UpdateStopMarkerPosition();
        if (stopMarker != null) stopMarker.gameObject.SetActive(true);

        isHolding = false;
        heldTime = 0f;
        currentSpeed = baseGrowthSpeed;
        holdDirection = 1f;

        // ensure audio stopped at start
        StopHoldSound();
    }

    void Restart()
    {
        stopped = false;
        if (resultText != null) resultText.text = "";

        float chosenStart = startNormalized;
        if (randomStartEnabled)
        {
            float mn = Mathf.Min(randomRangeMin, randomRangeMax);
            float mx = Mathf.Max(randomRangeMin, randomRangeMax);
            chosenStart = Random.Range(mn, mx);
        }
        chosenStart = Mathf.Clamp01(chosenStart);
        if (slider != null) slider.value = chosenStart;

        UpdateStopMarkerPosition();

        isHolding = false;
        heldTime = 0f;
        currentSpeed = baseGrowthSpeed;
        holdDirection = 1f;

        StopHoldSound();
    }

    void EvaluateStop()
    {
        if (slider == null)
        {
            Debug.LogWarning("ListenerManager: slider eksik!");
            return;
        }

        float val = slider.value;
        float delta = Mathf.Abs(val - targetNormalized);
        bool success = delta <= toleranceNormalized;

        if (resultText != null)
        {
            if (success) resultText.text = $"Success! Δ={delta:F2}";
            else resultText.text = $"Miss. Δ={delta:F2}";
        }

        StartCoroutine(EndAfterDelay(0.6f, success));
    }

    IEnumerator EndAfterDelay(float d, bool success)
    {
        yield return new WaitForSeconds(d);
        EndMinigame(success);
    }

    void EndMinigame(bool success)
    {
        if (!running) return;
        running = false;
        stopped = false;

        // ensure audio stopped
        StopHoldSound();

        if (uiRoot != null) uiRoot.SetActive(false);

        if (playerRef != null)
        {
            var ctrl = playerRef.GetComponent<BasicTopdownController>();
            if (ctrl != null) ctrl.enabled = true;
        }

        if (stopMarker != null) stopMarker.gameObject.SetActive(false);

        OnMinigameEnd?.Invoke(success);
        playerRef = null;
    }

    void UpdateStopMarkerPosition()
    {
        if (slider == null || stopMarker == null) return;

        RectTransform sRect = slider.GetComponent<RectTransform>();
        float width = sRect.rect.width;
        float xLocal = (targetNormalized - 0.5f) * width;
        stopMarker.anchoredPosition = new Vector2(xLocal, stopMarker.anchoredPosition.y);
    }

    // --- Audio helpers ---
    void StartHoldSound()
    {
        if (holdAudio == null) return;
        if (!holdAudio.isPlaying)
        {
            holdAudio.loop = true;
            holdAudio.Play();
        }
    }

    void StopHoldSound()
    {
        if (holdAudio == null) return;
        if (holdAudio.isPlaying)
        {
            holdAudio.Stop();
        }
    }
}

