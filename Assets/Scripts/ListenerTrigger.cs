using System.ComponentModel;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class ListenerTrigger : MonoBehaviour
{
    [Header("Minigame")]
    public ListenerManager minigameManager;
    public GameObject promptObject;
    [Range(0f, 1f)] public float targetNormalized = 0.5f;

    [Header("Success count (this trigger requires multiple successes)")]
    [Tooltip("Bu trigger için kaç başarılı minigame gerekiyor (1 = default).")]
    public int requiredSuccesses = 1;

    public int currentSuccesses = 0;

    [Header("Dialogue")]
    public DialogueData dialogueData;
    public DialogSequence dialogueSequence;

    [Tooltip("Opsiyonel: dialog root'u buradaki transform ile yerleştir. Genelde UI içindeki bir RectTransform placeholder atayın.")]
    public Transform dialogRootOverride;
    [Tooltip("Override sırasında dialogueRoot'un parent'ını overrideTransform'a set etsin mi? (UI için genelde true)")]
    public bool setParentToOverride = true;
    [Tooltip("Override yaparken world-space pozisyonu eşitlenip eşitlenmeyeceği.")]
    public bool matchWorldPosition = true;

    [Header("Camera per-trigger (opsiyonel)")]
    public CinemachineCamera triggerCamera;
    public float triggerZoom = -1f;

    [Header("Controls & UI")]
    public KeyCode startKey = KeyCode.E;
    public Collider2D Collider2D;
    public TMPro.TMP_Text progressText; // optional: "2 / 3"
    public GameObject unlem;

    public SpriteRenderer playerImage;
    public Sprite NormalSprite;
    public Sprite DarkSprite;

    // internals
    bool playerInside = false;
    GameObject player;
    bool waitingForZoom = false;
    bool startedMinigame = false;

    void Start()
    {
        if (promptObject != null) promptObject.SetActive(false);
        if (minigameManager == null) Debug.LogWarning($"{name}: minigameManager atanmamış.");
        if (dialogueSequence == null && dialogueData != null) Debug.LogWarning($"{name}: dialogueSequence atanmamış (dialogueData var).");
        UpdateProgressUI();
        unlem.gameObject.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        playerInside = true;
        player = col.gameObject;
        if (promptObject != null && !IsManagerRunning()) promptObject.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        playerInside = false;
        player = null;
        if (promptObject != null) promptObject.SetActive(false);

        if (waitingForZoom)
            CancelWaitingForZoom();
    }

    void Update()
    {
        if (!playerInside || minigameManager == null) return;

        if (promptObject != null)
            promptObject.SetActive(!IsManagerRunning() && !waitingForZoom);

        if (Input.GetKeyDown(startKey) && !IsManagerRunning() && !waitingForZoom)
        {
            // Focus camera
            if (CameraFocusController.Instance != null)
            {
                CameraFocusController.Instance.FocusOn(triggerCamera != null ? triggerCamera : CameraFocusController.Instance.interactCam,
                                                       transform,
                                                       triggerZoom > 0f ? triggerZoom : -1f);
            }

            waitingForZoom = true;
            startedMinigame = false;

            if (CameraFocusController.Instance != null)
                CameraFocusController.Instance.OnZoomComplete?.AddListener(OnCameraZoomComplete);

            if (promptObject != null) promptObject.SetActive(false);
        }
    }

    void OnCameraZoomComplete()
    {
        if (CameraFocusController.Instance != null)
            CameraFocusController.Instance.OnZoomComplete?.RemoveListener(OnCameraZoomComplete);

        waitingForZoom = false;

        if (CameraFocusController.Instance != null && CameraFocusController.Instance.zoomEnd)
        {
            if (!startedMinigame)
            {
                startedMinigame = true;

                // Abone ol: minigame bitince HandleMinigameEnd çağrılacak
                minigameManager.OnMinigameEnd?.AddListener(HandleMinigameEnd);
                minigameManager.StartMinigame(player, targetNormalized);
                playerImage.sprite = DarkSprite;
            }
        }
        else
        {
            CancelWaitingForZoom();
        }
    }

    void CancelWaitingForZoom()
    {
        waitingForZoom = false;
        startedMinigame = false;

        if (CameraFocusController.Instance != null)
            CameraFocusController.Instance.OnZoomComplete?.RemoveListener(OnCameraZoomComplete);

        if (CameraFocusController.Instance != null)
            CameraFocusController.Instance.ResetCamera();

        if (playerInside && promptObject != null) promptObject.SetActive(true);
        playerImage.sprite = NormalSprite;
    }

    void HandleMinigameEnd(bool success)
    {
        // remove minigame subscription for this run
        minigameManager.OnMinigameEnd?.RemoveListener(HandleMinigameEnd);

        if (success)
        {
            // increase success counter for this trigger
            currentSuccesses = Mathf.Min(requiredSuccesses, currentSuccesses + 1);
            unlem.gameObject.SetActive(false);
            UpdateProgressUI();

            if (currentSuccesses >= requiredSuccesses)
            {
                // Trigger fully succeeded — play dialog and disable collider
                OnTriggerFullySucceeded();
            }
            else
            {
                // not enough successes yet: show prompt again so player can try another round
                if (playerInside && promptObject != null) promptObject.SetActive(true);
            }
        }
        else
        {
            // fail -> reset camera immediately and show prompt again (no increment)
            if (CameraFocusController.Instance != null)
                CameraFocusController.Instance.ResetCamera();

            if (playerInside && promptObject != null) promptObject.SetActive(true);
            playerImage.sprite = NormalSprite;
        }
    }

    void OnTriggerFullySucceeded()
    {
        // reset camera if needed (dialogueSequence will handle reset after OnDialogueComplete if you prefer)
        if (CameraFocusController.Instance != null)
            CameraFocusController.Instance.ResetCamera();

        // play dialogue (optionally move dialog root)
        if (dialogueData != null && dialogueSequence != null)
        {
            // apply dialog root transform override (if any)
            if (dialogRootOverride != null)
            {
                // set parent & position (use inspector flags)
                dialogueSequence.ApplyDialogRootTransform(dialogRootOverride, matchWorldPosition, setParentToOverride);
            }

            // when dialog completes we want to run OnDialogComplete_ResetCameraOnce
            dialogueSequence.OnDialogueComplete?.AddListener(OnDialogComplete_ResetCameraOnce);

            dialogueSequence.Play(dialogueData);
            if (Collider2D != null) Collider2D.enabled = false;
        }
        else
        {
            Debug.LogWarning($"{name}: dialogueData veya dialogueSequence atanmadı.");
        }
    }

    void OnDialogComplete_ResetCameraOnce()
    {
        if (dialogueSequence != null)
            dialogueSequence.OnDialogueComplete?.RemoveListener(OnDialogComplete_ResetCameraOnce);

        if (CameraFocusController.Instance != null)
            CameraFocusController.Instance.ResetCamera();

        if (playerInside && promptObject != null) promptObject.SetActive(true);

        playerImage.sprite = NormalSprite;
    }

    void UpdateProgressUI()
    {
        if (progressText != null)
            progressText.text = $"{currentSuccesses} / {requiredSuccesses}";
    }

    bool IsManagerRunning() => minigameManager != null && minigameManager.IsRunning;
}
