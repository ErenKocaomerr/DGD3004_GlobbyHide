using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AdvancedPlayerController : MonoBehaviour
{
    [Header("--- Temel Hareket ---")]
    public float moveSpeed = 10f;
    public float acceleration = 10f; // Daha tepkisel olması için artırdım
    public float decceleration = 10f; // Durma yumuşaklığı
    public float velPower = 0.96f; // Hızlanma eğrisi
    [Range(0, 1)] public float airControl = 0.7f;

    public PlayerJuice playerJuice;
    private bool wasGrounded;

    [Header("--- Zıplama (Hollow Knight Physics) ---")]
    public float jumpForce = 22f;
    [Range(0, 1)] public float jumpCutMultiplier = 0.5f;
    public float gravityScale = 4f;
    public float fallGravityMultiplier = 1.6f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("--- Duvar Mekanikleri (Hollow Knight) ---")]
    public float wallSlideSpeed = 2.5f;
    // Duvar zıplaması: X duvardan itme gücü, Y yukarı itme gücü
    public Vector2 wallJumpPower = new Vector2(15f, 22f);
    public float wallJumpStopControlTime = 0.15f; // Oyuncunun kontrolünü kısa süre al

    [Header("--- Celeste Style Dash ---")]
    public float dashSpeed = 30f;
    public float dashDuration = 0.15f; // Celeste dash'i çok kısadır
    public float dashCooldown = 0.5f;
    private Vector2 dashDir; // Dash atılacak yön

    [Header("--- Gizlilik (Stealth) ---")]
    public float maxStealthTime = 3f; // Max görünmezlik süresi
    public float stealthRefillRate = 1f; // Dolma hızı
    [Range(0, 1)]
    public float stealthAlpha = 0.3f;
    public string defaultLayer = "Player";
    public string stealthLayer = "Stealth";
    public float minStealthToReactivate = 1f; // Flicker önleyici limit
    private bool isStealthExhausted = false; // Enerji bitti mi?

    public bool IsHidden { get; private set; }

    [Header("--- Kontroller (Checks) ---")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer;
    public bool unlockDash = false;    
    public bool unlockWallJump = false;

    // Internal Variables
    private Rigidbody2D rb;
    private float horizontalInput;
    private float verticalInput; // Celeste dash için dikey input lazım
    private bool isFacingRight = true;

    // States
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isDashing;
    private bool canDash = true;
    private bool isControlLocked = false; // WallJump sonrası kontrolü kitlemek için

    // Timers
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private float currentStealthTime;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;


    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); // Visuals içindeyse oradan al
        if (spriteRenderer) originalColor = spriteRenderer.color;
        currentStealthTime = maxStealthTime;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        if (playerJuice == null) playerJuice = GetComponentInChildren<PlayerJuice>();
    }

    void Update()
    {
        // Dash atarken inputları okumayı bırakma ama fiziğe müdahale etme
        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // Eğer Dash atıyorsak:
        if (isDashing)
        {
            // Dash atarken zıplamaya basıldıysa Dash'i iptal et ve Zıpla (Celeste Mechanic)
            if (jumpBufferCounter > 0 && CheckWallOrGroundForDashJump())
            {
                StopCoroutine(Dash()); // Dash coroutine'ini durdur
                isDashing = false;
                rb.gravityScale = gravityScale; // Gravity'i geri aç
                canDash = true; // İsteğe bağlı: Hakkını geri ver veya verme

                // Buradan aşağısı normal akışa devam eder ve zıplama gerçekleşir
            }
            else
            {
                return; // Zıplama yoksa return at, Dash devam etsin
            }
        }

        // 1. Inputlar
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // 2. Dash Input (Celeste Style - Yöne Göre)
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && unlockDash)
        {
            // Input varsa o yöne, yoksa baktığı yöne
            if (horizontalInput == 0 && verticalInput == 0)
            {
                dashDir = new Vector2(isFacingRight ? 1 : -1, 0);
            }
            else
            {
                dashDir = new Vector2(horizontalInput, verticalInput).normalized;
            }

            StartCoroutine(Dash());
        }

        // 3. Timer Yönetimi
        ManageTimers();

        // 4. Duvar Kayma Kontrolü (Hollow Knight mantığı)
        CheckWallSlide();

        // 5. Zıplama Mantığı
        if (jumpBufferCounter > 0)
        {
            // Duvardan Zıplama (Wall Jump)
            if (unlockWallJump && (isWallSliding || (isTouchingWall && !isGrounded)))
            {
                PerformWallJump();
            }
            // Normal Zıplama
            else if (coyoteTimeCounter > 0)
            {
                PerformJump();
            }
        }

        // 6. Variable Jump Height (Zıplamayı kesme)
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        // İniş Efekti
        if (!wasGrounded && isGrounded)
        {
            if (playerJuice) playerJuice.PlayLandEffect(rb.linearVelocity.y);
            canDash = true; // Yere inince dash yenile (Celeste mantığı)
        }
        wasGrounded = isGrounded;

        // 7. Yön Çevirme
        // Kontrol kilitliyken (WallJump sonrası) dönme!
        if (!isControlLocked && !isWallSliding)
        {
            if (horizontalInput > 0 && !isFacingRight) Flip();
            else if (horizontalInput < 0 && isFacingRight) Flip();
        }

        HandleStealth();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        // Eğer kontrol kilitli değilse yürü (WallJump sonrası havada süzülme için)
        if (!isControlLocked)
        {
            Run();
        }

        ApplyGravityModifiers();
        CheckCollisions();
    }

    #region Movement Logic
    private void Run()
    {
        float targetSpeed = horizontalInput * moveSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;

        // Hızlanma ve Yavaşlama
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;

        // Havada kontrolü azalt ama tamamen bitirme (Hollow Knight havada kontrol verir)
        if (!isGrounded) accelRate *= airControl;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right);
    }

    private void ApplyGravityModifiers()
    {
        if (isWallSliding)
        {
            rb.gravityScale = 0;
            return;
        }

        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }
    #endregion

    #region Jump & Wall Mechanics
    private void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;

        if (playerJuice) playerJuice.PlayJumpEffect();
    }

    private void CheckWallSlide()
    {
        if (!unlockWallJump)
        {
            isWallSliding = false;
            return;
        }

        bool pushingWall = false;
        if ((isFacingRight && horizontalInput > 0) || (!isFacingRight && horizontalInput < 0))
        {
            pushingWall = true;
        }

        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0 && pushingWall)
        {
            isWallSliding = true;
            // Sabit kayma hızı
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);

            canDash = true;
        }
        else
        {
            isWallSliding = false;
        }


    }

    private void PerformWallJump()
    {
        float wallDir = isFacingRight ? -1 : 1;

        rb.linearVelocity = Vector2.zero;

        Vector2 force = new Vector2(wallJumpPower.x * wallDir, wallJumpPower.y);
        rb.AddForce(force, ForceMode2D.Impulse);

        jumpBufferCounter = 0;

        Flip();

        StartCoroutine(DisableControlBriefly());

        if (playerJuice) playerJuice.PlayJumpEffect();
    }
    #endregion

    #region Celeste Dash Logic
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Dash yönüne fırlat
        rb.linearVelocity = dashDir * dashSpeed;

        if (playerJuice) playerJuice.PlayDashEffect();

        yield return new WaitForSeconds(dashDuration);

        // --- DASH BİTİŞ KISMI ---
        rb.gravityScale = originalGravity;
        isDashing = false;

        // SORUNUN ÇÖZÜMÜ BURADA:
        // Eğer Dash bittiğinde hala yukarı doğru bir hızımız varsa,
        // bu hızı sertçe kesiyoruz (Momentum Cut). 
        // Böylece roket gibi uçmaya devam etmez.
        if (rb.linearVelocity.y > 0)
        {
            // Y eksenindeki hızı %10'a düşür (0.1f), X aynen kalsın
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.25f);
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    #endregion


    #region Stealth Logic
    private void HandleStealth()
    {
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f || Mathf.Abs(rb.linearVelocity.y) > 0.1f || isDashing;

        // 2. Yorgunluk (Exhaustion) Yönetimi - FLICKER ÇÖZÜMÜ
        if (currentStealthTime <= 0)
        {
            // Süre tamamen bitti, sistemi kitle
            isStealthExhausted = true;
            currentStealthTime = 0; // Negatife düşmesin
        }
        else if (currentStealthTime >= minStealthToReactivate)
        {
            // Süre belirlenen limite (örn. 1 saniye) kadar doldu, kilidi aç
            isStealthExhausted = false;
        }

        // 3. Stealth Aktifleşme Şartları
        // S basılı + Hareket YOK + Yorgun DEĞİL + Süre VAR
        if (Input.GetKey(KeyCode.S) && !isMoving && !isStealthExhausted && currentStealthTime > 0)
        {
            IsHidden = true;
            currentStealthTime -= Time.deltaTime;

            // Görsel Şeffaflık
            if (spriteRenderer)
            {
                Color tempColor = originalColor;
                tempColor.a = stealthAlpha;
                spriteRenderer.color = tempColor;
            }

            // Layer Değişimi (Ghost Mode)
            gameObject.layer = LayerMask.NameToLayer(stealthLayer);
        }
        else
        {
            // Stealth Kapalı
            IsHidden = false;

            // Süre dolumu (Max süreye gelene kadar doldur)
            if (currentStealthTime < maxStealthTime)
            {
                currentStealthTime += Time.deltaTime * stealthRefillRate;
            }

            // Görseli Normale Döndür
            if (spriteRenderer) spriteRenderer.color = originalColor;

            // Layer Normale Döndür
            gameObject.layer = LayerMask.NameToLayer(defaultLayer);
        }
    }
    #endregion


    #region Helpers
    private IEnumerator DisableControlBriefly()
    {
        isControlLocked = true;
        yield return new WaitForSeconds(wallJumpStopControlTime);
        isControlLocked = false;
    }

    private void ManageTimers()
    {
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;
    }

    private bool CheckWallOrGroundForDashJump()
    {
        // Yerdeysek veya Duvar Slide durumundaysak zıplamaya izin ver
        return isGrounded || isTouchingWall;
    }

    private void CheckCollisions()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckDistance, groundLayer);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (groundCheck) Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        if (wallCheck) Gizmos.DrawWireSphere(wallCheck.position, wallCheckDistance);
    }
    #endregion
}
