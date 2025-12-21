using UnityEngine;

public class HackingPlayer : MonoBehaviour
{
    public enum State { Moving, Hacking, Idle }
    public State currentState = State.Idle;

    [Header("--- Referanslar ---")]
    public HackingManager manager;
    public HackingUIManager uiManager;
    public float moveSpeed = 10f;

    private HackingNode currentNode;
    private HackingNode targetNode;
    private bool isMovingToTarget = false;

    // Hacking Deðiþkenleri
    private int currentHackIndex = 0;

    [Header("--- Sesler ---")]
    public AudioSource audioSource;
    public AudioClip moveSFX;
    public AudioClip keyCorrectSFX;
    public AudioClip keyWrongSFX;
    public AudioClip unlockSFX;
    public AudioClip arriveLockedSFX; // Kilitli yere varýnca çýkan ses

    public void Setup(HackingNode startNode)
    {
        currentNode = startNode;
        transform.position = currentNode.transform.position;
        currentState = State.Idle;
        isMovingToTarget = false;
        uiManager.HideCombo();
    }

    void Update()
    {
        if (manager.isGameOver) return;

        // --- DURUM MAKÝNESÝ ---

        // 1. Eðer bir hedefe doðru gidiyorsak (Animasyon)
        if (isMovingToTarget)
        {
            MoveToTarget();
        }
        // 2. Eðer duruyorsak ve Hacklemiyorsak (Input Bekle)
        else if (currentState == State.Idle)
        {
            HandleInputMovement();
        }
        // 3. Eðer Hackliyorsak (Þifre Bekle)
        else if (currentState == State.Hacking)
        {
            HandleHackingInput();
        }
    }

    // --- HAREKET MANTIÐI ---

    void MoveToTarget()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetNode.transform.position, moveSpeed * Time.deltaTime);

        // HEDEFE VARDIK MI?
        if (Vector2.Distance(transform.position, targetNode.transform.position) < 0.01f)
        {
            transform.position = targetNode.transform.position;
            currentNode = targetNode; // Artýk yeni evimiz burasý
            isMovingToTarget = false;

            // --- VARINCA KONTROL ET ---
            CheckCurrentNodeStatus();
        }
    }

    void CheckCurrentNodeStatus()
    {
        // Vardýðýmýz yer Bitiþ mi?
        if (currentNode.isEndNode)
        {
            manager.WinGame();
            return;
        }

        // Vardýðýmýz yer KÝLÝTLÝ MÝ?
        if (currentNode.isLocked)
        {
            // KÝLÝTLÝ! Hack modunu baþlat
            StartHacking();
        }
        else
        {
            // TEMÝZ. Yeni hareket için bekle
            currentState = State.Idle;
        }
    }

    void HandleInputMovement()
    {
        HackingNode nextNode = null;

        // Hem Ok Tuþlarý Hem WASD kontrolü
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            nextNode = currentNode.upNode;
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            nextNode = currentNode.downNode;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            nextNode = currentNode.leftNode;
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            nextNode = currentNode.rightNode;

        if (nextNode != null)
        {
            targetNode = nextNode;
            isMovingToTarget = true;
            PlaySFX(moveSFX);
        }
    }

    // --- HACKING MANTIÐI ---

    void StartHacking()
    {
        currentState = State.Hacking; // Player'ý dondur
        currentHackIndex = 0;

        PlaySFX(arriveLockedSFX); // "Dýt dýt" sesi

        // UI'ý currentNode'un þifresiyle aç
        uiManager.ShowCombo(currentNode.unlockSequence);
    }

    void HandleHackingInput()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) return;
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) return;

            KeyCode requiredKey = currentNode.unlockSequence[currentHackIndex];

            // 1. DOÐRU TUÞ MU? (CheckInputMatch fonksiyonunu kullanýyoruz)
            if (CheckInputMatch(requiredKey))
            {
                currentHackIndex++;
                PlaySFX(keyCorrectSFX);
                uiManager.RemoveFirstArrow();

                if (currentHackIndex >= currentNode.unlockSequence.Count)
                {
                    SuccessHacking();
                }
            }
            // 2. YANLIÞ TUÞ MU?
            else
            {
                // Herhangi bir YÖN tuþuna (WASD dahil) basýldýysa ve yanlýþsa ceza ver
                if (IsAnyDirectionKeyPressed())
                {
                    PlaySFX(keyWrongSFX);
                    Debug.Log("HATA! Yanlýþ tuþ. Baþtan Baþlýyor...");
                    currentHackIndex = 0;
                    uiManager.ShowCombo(currentNode.unlockSequence);
                }
            }
        }
    }

    bool CheckInputMatch(KeyCode required)
    {
        if (required == KeyCode.UpArrow)
            return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);

        if (required == KeyCode.DownArrow)
            return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);

        if (required == KeyCode.LeftArrow)
            return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);

        if (required == KeyCode.RightArrow)
            return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);

        return false;
    }

    bool IsAnyDirectionKeyPressed()
    {
        return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) ||
               Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) ||
               Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) ||
               Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
    }

    void SuccessHacking()
    {
        PlaySFX(unlockSFX);

        // Node'un kilidini aç (Rengi deðiþsin)
        currentNode.Unlock();

        uiManager.HideCombo();

        // Tekrar hareket edebilir hale gel
        currentState = State.Idle;
    }

    // Yardýmcý: Sadece ok tuþlarýna basýnca hata versin, mouse týklamasý vs. hata saymasýn
    bool IsDirectionKey(KeyCode k)
    {
        return k == KeyCode.UpArrow || k == KeyCode.DownArrow || k == KeyCode.LeftArrow || k == KeyCode.RightArrow;
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip && audioSource) audioSource.PlayOneShot(clip);
    }
}
