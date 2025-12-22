using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BossBrain : MonoBehaviour
{
    [Header("--- Durumlar ---")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;

    // YENÝ: Hýz Çarpaný (1 = Normal, 0.5 = Ýki kat hýzlý)
    private float rageMultiplier = 1f;

    [Header("--- Referanslar ---")]
    public BossHand leftHand;
    public BossHand rightHand;
    public GameObject bossHead;
    public Transform player;
    public BossWeakPoint[] balloons;

    public SpriteRenderer bossHeadSpriteRenderer; // Kafa objesinin Sprite Renderer'ý
    public Sprite[] bossFaceSprites;

    [Header("--- Ayarlar ---")]
    public float groundYLevel = -3.5f;
    public float hoverHeight = 4f;
    public float xBound = 8f;

    [Header("--- Genel Zamanlama (Base Deðerler) ---")]
    public int attacksPerRound = 3;
    public float startDelay = 1f;
    public float preparationTime = 1.5f;
    public float tiredDuration = 4f;
    public float recoveryTime = 1.5f;

    // ... (Saldýrý Ayarlarý Ayný - Sadece kodda çarpacaðýz) ...
    // Saldýrý deðiþkenlerini aynen býrak, aþaðýda kullanacaðýz.
    [Header("--- Saldýrý 1: Double Smash ---")]
    public float doubleSmashTrackingTime = 2f;
    public float doubleSmashWarningTime = 0.5f;
    public float doubleSmashRecovery = 1f;

    [Header("--- Saldýrý 2: Alternating ---")]
    public float singleSmashTrackingTime = 1.2f;
    public float singleSmashWarningTime = 0.4f;
    public float singleHandInterval = 0.4f;

    [Header("--- Saldýrý 3: Sweep ---")]
    public float sweepPrepareTime = 1f;
    public float sweepWarningTime = 0.6f;
    public float sweepDuration = 2.5f;

    [Header("--- Saldýrý 4: Alkýþ (Clap) ---")]
    public float clapPrepareDuration = 1f; // Kenarlara gitme süresi
    public float clapWaitDuration = 0.5f;  // Kenarda bekleme/titreme süresi
    public float clapSpeed = 0.2f;         // Çarpýþma hýzý (Düþük = Çok Hýzlý)
    public float clapRecovery = 1f;

    [Header("--- Ses Efektleri (YENÝ) ---")]
    public AudioClip bossRoarSFX;   // Faz deðiþimi / Kükreme
    public AudioClip bossTiredSFX;  // Yorulma sesi
    public AudioClip clapImpactSFX; // Alkýþ çarpýþma sesi
    public AudioClip bossDeathSFX;  // Ölüm sesi
    public AudioClip winSesi;
    public AudioSource audioSource;

    public GameObject successPanel;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateBossFace();
        //bossHead.SetActive(false);
        foreach (var b in balloons) b.gameObject.SetActive(false);
        if (successPanel != null) successPanel.SetActive(false);
        StartCoroutine(CombatLoop());
    }

    IEnumerator CombatLoop()
    {
        yield return new WaitForSeconds(startDelay);

        while (!isDead)
        {
            for (int i = 0; i < attacksPerRound; i++)
            {
                leftHand.SetAttackMode(true);
                rightHand.SetAttackMode(true);

                // Idle'a dönerken RAGE çarpanýný kullanmýyoruz (Güvenli olsun)
                leftHand.ReturnToIdle(0.5f);
                rightHand.ReturnToIdle(0.5f);

                // BEKLEME SÜRESÝ AZALIYOR! (Rage Multiplier devrede)
                yield return new WaitForSeconds(preparationTime * rageMultiplier);

                if (isDead) break;

                leftHand.SetAttackMode(true);
                rightHand.SetAttackMode(true);

                int randomAttack = Random.Range(0, 4);
                if (randomAttack == 0) yield return StartCoroutine(Attack_DoubleSmash());
                else if (randomAttack == 1) yield return StartCoroutine(Attack_AlternatingSmash());
                else if (randomAttack == 2) yield return StartCoroutine(Attack_Sweep());
                else yield return StartCoroutine(Attack_Clap());

                if (isDead) break;
            }

            if (isDead) break;

            Debug.Log("BOSS YORULDU!");
            bossHead.SetActive(true);

            UpdateBossFace();

            foreach (var b in balloons)
            {
                b.ResetBalloon(); // Hepsini görünür yap ve collider aç
            }

            leftHand.StopEverything();
            rightHand.StopEverything();

            leftHand.SetAttackMode(false);
            rightHand.SetAttackMode(false);

            // Yorgunluk anýnda elleri yavaþça kenara çek
            leftHand.StopFloating();
            rightHand.StopFloating();
            leftHand.transform.DOMove(new Vector3(-5, groundYLevel + 2, 0), 1f);
            rightHand.transform.DOMove(new Vector3(5, groundYLevel + 2, 0), 1f);

            // Dayak yeme süresi azalmýyor (Oyuncuya haksýzlýk olmasýn diye sabit 4sn)
            yield return new WaitForSeconds(tiredDuration);

            //bossHead.SetActive(false);
            foreach (var b in balloons) b.gameObject.SetActive(false);
        }
    }

    IEnumerator Attack_DoubleSmash()
    {
        float timer = 0;
        // Takip süresi hýzlanýyor
        float dynamicTracking = doubleSmashTrackingTime;

        while (timer < dynamicTracking)
        {
            float targetX = Mathf.Clamp(player.position.x, -xBound, xBound);
            Vector3 centerPos = new Vector3(targetX, player.position.y + hoverHeight, 0);
            leftHand.TrackPlayer(centerPos + Vector3.left * 2f);
            rightHand.TrackPlayer(centerPos + Vector3.right * 2f);
            timer += Time.deltaTime;
            yield return null;
        }

        // Uyarý süresi kýsalýyor (Daha ani vuruyor)
        float dynamicWarning = doubleSmashWarningTime;

        leftHand.ShakeWarning(dynamicWarning);
        yield return rightHand.ShakeWarning(dynamicWarning).WaitForCompletion();

        // Vuruþ hýzý sabit kalabilir veya çok az hýzlanabilir (Çok hýzlanýrsa animasyon bozulur)
        leftHand.SmashDown(groundYLevel, 0.2f);
        rightHand.SmashDown(groundYLevel, 0.2f);

        yield return new WaitForSeconds(doubleSmashRecovery);


        leftHand.ReturnToIdle(0.5f);
        rightHand.ReturnToIdle(0.5f);
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator Attack_AlternatingSmash()
    {
        yield return StartCoroutine(SingleHandSmash(leftHand));
        yield return StartCoroutine(SingleHandSmash(rightHand));
    }

    IEnumerator SingleHandSmash(BossHand hand)
    {
        float timer = 0;
        // Hýzlanmýþ takip süresi
        while (timer < (singleSmashTrackingTime))
        {
            float targetX = Mathf.Clamp(player.position.x, -xBound, xBound);
            hand.TrackPlayer(new Vector3(targetX, player.position.y + hoverHeight, 0));
            timer += Time.deltaTime;
            yield return null;
        }

        yield return hand.ShakeWarning(singleSmashWarningTime).WaitForCompletion();
        hand.SmashDown(groundYLevel, 0.2f);

        // Ýki el arasýndaki bekleme süresi azalýyor
        yield return new WaitForSeconds(singleHandInterval);

        hand.SetAttackMode(false);
        hand.ReturnToIdle(0.5f);
    }

    IEnumerator Attack_Sweep()
    {
        Vector3 startPos = new Vector3(-xBound - 2, groundYLevel + 1.5f, 0);
        leftHand.TrackPlayer(startPos);
        yield return new WaitForSeconds(sweepPrepareTime);
        yield return leftHand.ShakeWarning(sweepWarningTime).WaitForCompletion();

        // Süpürme hýzý artýyor (Süre kýsalýyor)
        leftHand.SweepMove(xBound + 2, sweepDuration);
        yield return new WaitForSeconds(sweepDuration);

        leftHand.ReturnToIdle(1f);
        yield return new WaitForSeconds(1f);
    }

    IEnumerator Attack_Clap()
    {
        // 1. HAZIRLIK: Elleri ekranýn en saðýna ve en soluna gönder
        // xBound'un biraz daha dýþýna taþsýnlar ki gerilme hissi olsun
        float farLeft = -xBound - 1f;
        float farRight = xBound + 1f;

        leftHand.transform.DORotate(new Vector3(0, 0, -90), 0.5f);
        rightHand.transform.DORotate(new Vector3(0, 0, 90), 0.5f);

        // Yükseklik olarak Player'ýn hizasýna (veya biraz altýna) insinler
        float clapHeight = player.position.y + 0.7f;
        // Ancak zeminin altýna inmesinler
        if (clapHeight < groundYLevel + 1) clapHeight = groundYLevel + 1;

        leftHand.TrackPlayer(new Vector3(farLeft, clapHeight, 0));
        rightHand.TrackPlayer(new Vector3(farRight, clapHeight, 0));

        yield return new WaitForSeconds(clapPrepareDuration * rageMultiplier);

        // 2. UYARI: Titreme
        leftHand.ShakeWarning(clapWaitDuration * rageMultiplier);
        yield return rightHand.ShakeWarning(clapWaitDuration * rageMultiplier).WaitForCompletion();

        // 3. SALDIRI: Triggerlarý Aç
        leftHand.SetAttackMode(true);
        rightHand.SetAttackMode(true);

        leftHand.isDamaging = true;
        rightHand.isDamaging = true;

        // Ortada buluþsunlar (X=0 noktasýnda)
        // SetEase(Ease.InBack) kullanarak "gerilip vurma" efekti verebiliriz
        leftHand.transform.DOMoveX(-1f, clapSpeed).SetEase(Ease.InBack); // Hafif boþluk kalsýn (-1)
        rightHand.transform.DOMoveX(1f, clapSpeed).SetEase(Ease.InBack); // Hafif boþluk kalsýn (1)

        yield return new WaitForSeconds(clapSpeed); // Vuruþun tamamlanmasýný bekle

        // 4. EFEKT: Kamera salla
        if (CameraShaker.instance) CameraShaker.instance.Shake(0.7f);
        if (clapImpactSFX) audioSource.PlayOneShot(clapImpactSFX);

        leftHand.isDamaging = false;
        rightHand.isDamaging = false;

        // Burada bir "Clap Sound" çalabilirsin

        yield return new WaitForSeconds(clapRecovery * rageMultiplier);

        // 5. BÝTÝÞ


        leftHand.ReturnToIdle(1f);
        rightHand.ReturnToIdle(1f);
        yield return new WaitForSeconds(0.5f);
    }

    public void OnBalloonPopped()
    {
        if (isDead) return;

        // 1. Diðer balonlarýn colliderlarýný kapat ki oyuncu sekip hepsini patlatmasýn
        foreach (var b in balloons)
        {
            b.GetComponent<Collider2D>().enabled = false;
        }

        // 2. Hasar al
        TakeDamage();
    }

    public void TakeDamage()
    {
        if (isDead) return;
        currentHealth--;

        UpdateBossFace();

        if (currentHealth == 2)
        {
            // FAZ 2
            rageMultiplier = 0.75f; // %20 Hýzlan
            attacksPerRound++;     // Saldýrý sayýsý +1 (Örn: 3 -> 4)
            Debug.Log("BOSS FAZ 2: Hýzlandý ve +1 Saldýrý!");
        }
        else if (currentHealth == 1)
        {
            // FAZ 3 (SON FAZ)
            rageMultiplier = 0.5f; // %40 Hýzlan
            attacksPerRound++;     // Saldýrý sayýsý +1 (Örn: 4 -> 5)
            Debug.Log("BOSS FAZ 3: ÇILDIRDI! Çok Hýzlý ve +1 Saldýrý!");
        }

        StopAllCoroutines();

        if (currentHealth <= 0) Die();
        else StartCoroutine(RecoverAndRestartLoop());
    }

    IEnumerator RecoverAndRestartLoop()
    {
        yield return new WaitForSeconds(1.0f);

        // ÞÝMDÝ balonlarý gizle
        foreach (var b in balloons) b.gameObject.SetActive(false);

        leftHand.SetAttackMode(false);
        rightHand.SetAttackMode(false);

        yield return new WaitForSeconds(recoveryTime);
        StartCoroutine(CombatLoop());
    }

    IEnumerator ShowWinPanel()
    {
        yield return new WaitForSeconds(1.5f); // 1.5 saniye bekle

        if (successPanel != null)
        {
            successPanel.SetActive(true);
            // Ýstersen burada oyunu durdurabilirsin:
            // Time.timeScale = 0f; 

            audioSource.PlayOneShot(winSesi);
        }
        else
        {
            Debug.LogError("Success Panel atanmadý!");
        }
    }

    void Die()
    {
        isDead = true;
        bossHead.SetActive(false);
        leftHand.gameObject.SetActive(false);
        rightHand.gameObject.SetActive(false);
        foreach (var b in balloons) b.gameObject.SetActive(false);
        // Boss ölünce son bir kez büyük salla
        if (CameraShaker.instance) CameraShaker.instance.Shake(0.5f);

        StartCoroutine(ShowWinPanel());
    }

    void UpdateBossFace()
    {
        if (bossHeadSpriteRenderer == null || bossFaceSprites.Length < 3) return;

        // Can 3 ise -> Index 0
        // Can 2 ise -> Index 1
        // Can 1 ise -> Index 2
        int spriteIndex = maxHealth - currentHealth;

        // Hata önlemek için sýnýrla
        spriteIndex = Mathf.Clamp(spriteIndex, 0, bossFaceSprites.Length - 1);

        bossHeadSpriteRenderer.sprite = bossFaceSprites[spriteIndex];
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawLine(new Vector3(-xBound, -10, 0), new Vector3(-xBound, 10, 0));
        Gizmos.DrawLine(new Vector3(xBound, -10, 0), new Vector3(xBound, 10, 0));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-xBound - 5, groundYLevel, 0), new Vector3(xBound + 5, groundYLevel, 0));
        Gizmos.color = Color.yellow;
        float estimatedHandY = groundYLevel + hoverHeight;
        Gizmos.DrawLine(new Vector3(-xBound, estimatedHandY, 0), new Vector3(xBound, estimatedHandY, 0));
    }
}
