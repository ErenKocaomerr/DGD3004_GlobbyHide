using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NPCInteractable : MonoBehaviour
{
    [Header("--- Kimlik (ÇOK ÖNEMLÝ) ---")]
    public string npcID;

    [Header("--- Sahne Ayarlarý ---")]
    public string sceneToLoad;
    public string npcName = "Gizemli Yolcu";

    [Header("--- Diyaloglar ---")]
    [TextArea(3, 10)] public string[] firstDialogueLines;
    [TextArea(3, 10)] public string[] secondDialogueLines;

    [Header("--- Ödüller (Yetenek Ver) ---")]
    public bool giveDash;
    public bool giveDoubleJump;
    public bool giveHide;
    public bool giveWallJump;

    [Header("--- Referanslar ---")]
    public GameObject interactPromptUI;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Button nextLineButton;

    public CinemachineCamera zoomCamera;
    public AdvancedPlayerController player;

    private InputSystem_Actions inputActions;
    private bool isPlayerInRange;
    private bool isDialogueActive;
    private int currentLineIndex;
    private string[] currentLinesToDisplay;
    private bool hasTalkedBefore;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        if (nextLineButton != null)
        {
            nextLineButton.onClick.RemoveAllListeners();
            nextLineButton.onClick.AddListener(AdvanceDialogue);
        }
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (GameManager.instance != null)
        {
            hasTalkedBefore = GameManager.instance.HasTalkedTo(npcID);
        }
    }

    // --- BURASI GÜNCELLENDÝ ---
    void Update()
    {
        // Interact tuþuna (E) basýldý mý?
        if (inputActions.Player.Interact.WasPressedThisFrame())
        {
            // 1. Menzildeyiz ve konuþmuyoruz -> Diyaloðu Baþlat
            if (isPlayerInRange && !isDialogueActive)
            {
                StartDialogue();
            }
            // 2. Zaten konuþuyoruz -> Diyaloðu Ýlerlet
            else if (isDialogueActive)
            {
                AdvanceDialogue();
            }
        }
    }
    // ---------------------------

    void StartDialogue()
    {
        if (hasTalkedBefore)
        {
            if (secondDialogueLines == null || secondDialogueLines.Length == 0) return;
            currentLinesToDisplay = secondDialogueLines;
        }
        else
        {
            if (firstDialogueLines == null || firstDialogueLines.Length == 0)
            {
                Debug.LogWarning("Diyalog boþ, geçici metin atanýyor.");
                currentLinesToDisplay = new string[] { "..." };
            }
            else
            {
                currentLinesToDisplay = firstDialogueLines;
            }
        }

        if (currentLinesToDisplay == null || currentLinesToDisplay.Length == 0) return;

        isDialogueActive = true;
        currentLineIndex = 0;

        if (interactPromptUI) interactPromptUI.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(true);
        if (nameText) nameText.text = npcName;

        if (dialogueText) dialogueText.text = currentLinesToDisplay[0];

        if (zoomCamera != null) zoomCamera.Priority = 20;

        if (player != null)
        {
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            player.enabled = false;
        }
    }

    public void AdvanceDialogue()
    {
        // Diyalog aktif deðilse veya metin yoksa iþlem yapma
        if (!isDialogueActive || currentLinesToDisplay == null) return;

        currentLineIndex++;

        // Dizinin sýnýrlarý içinde miyiz?
        if (currentLineIndex < currentLinesToDisplay.Length)
        {
            if (dialogueText) dialogueText.text = currentLinesToDisplay[currentLineIndex];
        }
        else
        {
            // Dizi bittiyse kapat
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        if (GameManager.instance != null)
        {
            if (currentLinesToDisplay == firstDialogueLines)
            {
                if (giveDash) GameManager.instance.UnlockAbility("Dash");
                if (giveDoubleJump) GameManager.instance.UnlockAbility("DoubleJump");
                if (giveHide) GameManager.instance.UnlockAbility("Hide");
                //if (giveWallJump) GameManager.instance.UnlockAbility("WallJump");
            }

            if (!string.IsNullOrEmpty(npcID))
            {
                GameManager.instance.MarkNpcAsTalked(npcID);
                hasTalkedBefore = true;
            }
            GameManager.instance.lastHubPosition = transform.position;
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (currentLinesToDisplay == firstDialogueLines)
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                CloseDialogueBox();
            }
        }
        else
        {
            CloseDialogueBox();
        }
    }

    void CloseDialogueBox()
    {
        isDialogueActive = false;
        if (dialoguePanel) dialoguePanel.SetActive(false);

        if (zoomCamera != null) zoomCamera.Priority = 0;
        if (player != null) player.enabled = true;

        if (hasTalkedBefore && (secondDialogueLines == null || secondDialogueLines.Length == 0))
        {
            if (interactPromptUI) interactPromptUI.SetActive(false);
        }
        else if (isPlayerInRange)
        {
            if (interactPromptUI) interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (player == null) player = collision.GetComponent<AdvancedPlayerController>();

            if (hasTalkedBefore && (secondDialogueLines == null || secondDialogueLines.Length == 0))
            {
                if (interactPromptUI) interactPromptUI.SetActive(false);
            }
            else if (!isDialogueActive)
            {
                if (interactPromptUI) interactPromptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactPromptUI) interactPromptUI.SetActive(false);
            if (isDialogueActive) CloseDialogueBox();
        }
    }

    [ContextMenu("Reset This NPC Status")]
    public void ResetNPCData()
    {
        hasTalkedBefore = false;
        Debug.Log(npcName + " sýfýrlandý.");
    }
}
