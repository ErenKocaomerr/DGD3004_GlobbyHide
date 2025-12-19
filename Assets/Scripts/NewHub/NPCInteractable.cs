using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NPCInteractable : MonoBehaviour
{
    [Header("--- Kimlik (ÇOK ÖNEMLÝ) ---")]
    public string npcID; // GameManager bunu tanýyacak (Örn: "Level1_Guard")

    [Header("--- Sahne Ayarlarý ---")]
    public string sceneToLoad;
    public string npcName = "Gizemli Yolcu";

    [Header("--- Diyaloglar ---")]
    [TextArea(3, 10)] public string[] firstDialogueLines;  // Ýlk konuþma
    [TextArea(3, 10)] public string[] secondDialogueLines; // Sonraki konuþmalar

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

    // Hangi diyaloðu kullanacaðýz?
    private string[] currentLinesToDisplay;
    private bool hasTalkedBefore; // Daha önce konuþtuk mu?

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        nextLineButton.onClick.AddListener(AdvanceDialogue);
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Start()
    {
        // Oyun baþýnda GameManager'a sor: Bu NPC ile konuþtum mu?
        if (GameManager.instance != null)
        {
            hasTalkedBefore = GameManager.instance.HasTalkedTo(npcID);
        }
    }

    void Update()
    {
        // Eðer menzildeysek, diyalog açýk deðilse ve E'ye basýldýysa
        if (isPlayerInRange && !isDialogueActive && inputActions.Player.Interact.WasPressedThisFrame())
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        // KONTROL 1: Daha önce konuþtuk mu?
        if (hasTalkedBefore)
        {
            // Eðer ikinci bir diyalog yoksa, HÝÇBÝR ÞEY YAPMA (Konuþmayý açma)
            if (secondDialogueLines.Length == 0) return;

            // Ýkinci diyaloðu yükle
            currentLinesToDisplay = secondDialogueLines;
        }
        else
        {
            // Ýlk kez konuþuyoruz
            currentLinesToDisplay = firstDialogueLines;
        }

        isDialogueActive = true;
        currentLineIndex = 0;

        interactPromptUI.SetActive(false);
        dialoguePanel.SetActive(true);
        nameText.text = npcName;
        dialogueText.text = currentLinesToDisplay[0]; // Seçili diziden oku

        if (zoomCamera != null) zoomCamera.Priority = 20;

        if (player != null)
        {
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            player.enabled = false;
        }
    }

    public void AdvanceDialogue()
    {
        currentLineIndex++;

        if (currentLineIndex < currentLinesToDisplay.Length)
        {
            dialogueText.text = currentLinesToDisplay[currentLineIndex];
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        // Konuþma bittiðinde GameManager'a kaydet
        if (GameManager.instance != null && !string.IsNullOrEmpty(npcID))
        {
            GameManager.instance.MarkNpcAsTalked(npcID);
            hasTalkedBefore = true; // Anlýk durumu güncelle
        }

        // Pozisyonu Kaydet (Sadece sahne deðiþecekse gerekli ama dursun)
        if (GameManager.instance != null) GameManager.instance.lastHubPosition = transform.position;

        // Sahne Geçiþi Var mý?
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            // Sadece ÝLK konuþmada mý sahne deðiþsin istiyorsun?
            // Genelde "Görev NPC'si" ilk konuþmada gönderir, sonra "Bol þans" der.
            // Eðer her seferinde göndersin istiyorsan bu if'i kaldýr.
            if (currentLinesToDisplay == firstDialogueLines)
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                // Ýkinci konuþmaysa sadece kutuyu kapat
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
        dialoguePanel.SetActive(false);

        if (zoomCamera != null) zoomCamera.Priority = 0;
        if (player != null) player.enabled = true;

        // KUTU KAPANDIKTAN SONRA TEKRAR E ÇIKSIN MI?
        // Kural: Eðer ikinci diyalog YOKSA ve zaten konuþtuysak, "E" yazýsý çýkmasýn.
        if (hasTalkedBefore && secondDialogueLines.Length == 0)
        {
            interactPromptUI.SetActive(false);
        }
        else if (isPlayerInRange)
        {
            interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (player == null) player = collision.GetComponent<AdvancedPlayerController>();

            // Ekrana "E" yazýsýný çýkarma kontrolü:
            // Eðer daha önce konuþtuysak VE ikinci bir diyeceði yoksa -> Yazý Çýkmasýn.
            if (hasTalkedBefore && secondDialogueLines.Length == 0)
            {
                interactPromptUI.SetActive(false);
            }
            else if (!isDialogueActive)
            {
                interactPromptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            interactPromptUI.SetActive(false);
            if (isDialogueActive) CloseDialogueBox();
        }
    }
}
