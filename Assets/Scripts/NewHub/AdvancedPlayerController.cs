using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class AdvancedPlayerController : MonoBehaviour
{

    [Header("--- Audio Settings (YENİ) ---")]
    public AudioClip jumpSFX;
    public AudioClip doubleJumpSFX;
    public AudioClip wallJumpSFX;
    public AudioClip dashSFX;
    public AudioClip landSFX;

    [Range(0.8f, 1.2f)] public float pitchRandomness = 0.1f; // Sesler hep aynı çıkmasın diye
    private AudioSource audioSource;

    // --- NEW INPUT SYSTEM DEĞİŞKENLERİ ---
    private InputSystem_Actions inputActions;
    private Vector2 moveInput; // Yatay ve Dikey girdiyi burada tutacağız

    [Header("--- Temel Hareket ---")]
    public Animator anim;
    public float moveSpeed = 10f;
    public float acceleration = 10f;
    public float decceleration = 10f;
    public float velPower = 0.96f;
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

    public bool unlockDoubleJump = false; // Özelliği aç/kapa
    private bool canDoubleJump; // Hakkımız var mı?
    private bool canStelth;

    [Header("--- Duvar Mekanikleri (Hollow Knight) ---")]
    public float wallSlideSpeed = 2.5f;
    public Vector2 wallJumpPower = new Vector2(15f, 22f);
    public float wallJumpStopControlTime = 0.15f;

    [Header("--- Celeste Style Dash ---")]
    public float dashSpeed = 30f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;
    private Vector2 dashDir;

    [Header("--- Gizlilik (Stealth) ---")]
    public float maxStealthTime = 3f;
    public float stealthRefillRate = 1f;
    [Range(0, 1)] public float stealthAlpha = 0.3f;
    public string defaultLayer = "Player";
    public string stealthLayer = "Stealth";
    public float minStealthToReactivate = 1f;
    private bool isStealthExhausted = false;

    public bool IsHidden { get; private set; }
    public bool IsGrounded => isGrounded;

    [Header("--- Kontroller (Checks) ---")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer;
    public bool unlockDash = false;
    public bool unlockWallJump = false;
    public bool unlockStelth = false;

    // Internal Variables
    private Rigidbody2D rb;
    // horizontalInput ve verticalInput ARTIK YOK (moveInput kullanacağız)
    private bool isFacingRight = true;

    // States
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isDashing;
    private bool canDash = true;
    private bool isControlLocked = false;

    // Timers
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private float currentStealthTime;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // --- NEW INPUT SYSTEM BAŞLATMA ---
    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (spriteRenderer) originalColor = spriteRenderer.color;
        currentStealthTime = maxStealthTime;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        if (playerJuice == null) playerJuice = GetComponentInChildren<PlayerJuice>();

        if (GameManager.instance != null)
        {
            unlockDash = GameManager.instance.hasDash;
            unlockDoubleJump = GameManager.instance.hasDoubleJump;
            unlockWallJump = GameManager.instance.hasWallJump;
            unlockStelth = GameManager.instance.hide;

            if (GameManager.instance.isReturningToHub && SceneManager.GetActiveScene().name == "NewHub")
            {
                transform.position = GameManager.instance.lastHubPosition;
                GameManager.instance.isReturningToHub = false; // İşimiz bitti, kapat
            }
            // 2. Durum: Öldüysek ve Checkpointimiz varsa (Checkpointte doğmalı)
            else if (GameManager.instance.hasActiveCheckpoint && SceneManager.GetActiveScene().name == "NewHub")
            {
                transform.position = GameManager.instance.currentCheckpointPos;
                Debug.Log("Checkpoint Noktasına Işınlandı!");
            }
        }
    }

    void Update()
    {
        // 1. INPUTLARI OKU (Eski Input.GetAxis yerine)
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        HandleSimpleAnimations();

        // Zıplama Buffer (Eski Input.GetButtonDown yerine)
        if (inputActions.Player.Jump.WasPressedThisFrame()) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // Eğer Dash atıyorsak:
        if (isDashing)
        {
            if (jumpBufferCounter > 0 && CheckWallOrGroundForDashJump())
            {
                StopCoroutine(Dash());
                isDashing = false;
                rb.gravityScale = gravityScale;
                canDash = true;
                // Double Jump hakkını da yenileyelim ki Dash Jump sonrası havada kalmasın
                if (unlockDoubleJump) canDoubleJump = true;
            }
            else return;
        }

        // 2. Dash Input (Celeste Style - Yöne Göre)
        // (Eski Input.GetKeyDown ve GetAxis yerine)
        if (inputActions.Player.Dash.WasPressedThisFrame() && canDash && unlockDash)
        {
            // Input varsa o yöne, yoksa baktığı yöne
            if (moveInput == Vector2.zero)
            {
                dashDir = new Vector2(isFacingRight ? 1 : -1, 0);
            }
            else
            {
                dashDir = moveInput.normalized;
            }

            StartCoroutine(Dash());
        }

        // 3. Timer Yönetimi
        ManageTimers();

        // 4. Duvar Kayma Kontrolü
        CheckWallSlide();

        // 5. Zıplama Mantığı
        if (jumpBufferCounter > 0)
        {
            // 1. Öncelik: Duvardan Zıplama
            if (unlockWallJump && (isWallSliding || (isTouchingWall && !isGrounded)))
            {
                PerformWallJump();
            }
            // 2. Öncelik: Normal Zıplama (Yerdeyken)
            else if (coyoteTimeCounter > 0)
            {
                PerformJump();
            }
            // 3. Öncelik: DOUBLE JUMP (Havadaysak ve Hakkımız Varsa)
            else if (unlockDoubleJump && canDoubleJump && !isGrounded && !isTouchingWall)
            {
                PerformDoubleJump();
            }
        }

        if (inputActions.Player.Jump.WasReleasedThisFrame() && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        if (!wasGrounded && isGrounded)
        {
            if (playerJuice) playerJuice.PlayLandEffect(rb.linearVelocity.y);
            canDash = true;
            // Yere inince double jump hakkını yenile
            if (unlockDoubleJump) canDoubleJump = true;
        }
        wasGrounded = isGrounded;

        if (!isControlLocked && !isWallSliding)
        {
            if (moveInput.x > 0 && !isFacingRight) Flip();
            else if (moveInput.x < 0 && isFacingRight) Flip();
        }

        if (unlockStelth)
        {
            HandleStealth();
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        if (!isControlLocked)
        {
            Run();
        }

        ApplyGravityModifiers();
        CheckCollisions();
    }

    private void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || audioSource == null) return;

        // Her seferinde hafif farklı tonda çalsın (Robotik duyulmaz)
        audioSource.PlayOneShot(clip, volume);
    }

    #region Movement Logic
    private void Run()
    {
        // moveInput.x kullanıyoruz
        float targetSpeed = moveInput.x * moveSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;

        if (!isGrounded) accelRate *= airControl;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right);
    }

    private void HandleSimpleAnimations()
    {
        if (anim == null) return;

        // 1. RUN (KOŞMA) KONTROLÜ
        // moveInput.x 0 değilse (sağa veya sola basılıyorsa) VE yerdeysek koşuyoruzdur.
        bool isRunning = Mathf.Abs(moveInput.x) > 0.01f && isGrounded;
        anim.SetBool("IsRunning", isRunning);

        // 2. JUMP (ZIPLAMA) KONTROLÜ
        // isGrounded TRUE ise yerdedir (Idle/Run), FALSE ise havadadır (Jump)
        anim.SetBool("IsGrounded", isGrounded);
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

        isGrounded = false;
        // -----------------------

        if (unlockDoubleJump) canDoubleJump = true;

        if (playerJuice) playerJuice.PlayJumpEffect();

        PlaySFX(jumpSFX);
    }

    private void PerformDoubleJump()
    {
        // Hızı sıfırla ki yerçekimiyle savaşmasın, anında yukarı fırlasın
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

        // İstersen Double Jump gücünü biraz azaltabilirsin (örn: jumpForce * 0.8f)
        rb.AddForce(Vector2.up * jumpForce * 0.8f , ForceMode2D.Impulse);

        jumpBufferCounter = 0;
        canDoubleJump = false; // Hakkı tüket

        // Eğer farklı bir efekt veya ses istiyorsan buraya ekleyebilirsin
        if (playerJuice) playerJuice.PlayJumpEffect();

        PlaySFX(doubleJumpSFX);
    }

    private void CheckWallSlide()
    {
        if (!unlockWallJump)
        {
            isWallSliding = false;
            return;
        }

        // Input duvara doğru mu? (moveInput.x ile kontrol)
        bool pushingWall = false;
        if ((isFacingRight && moveInput.x > 0) || (!isFacingRight && moveInput.x < 0))
        {
            pushingWall = true;
        }

        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0 && pushingWall)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            canDash = true;

            if (unlockDoubleJump) canDoubleJump = true;
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

        if (unlockDoubleJump) canDoubleJump = true;

        Flip();
        StartCoroutine(DisableControlBriefly());

        if (playerJuice) playerJuice.PlayJumpEffect();

        PlaySFX(wallJumpSFX);
    }
    #endregion

    #region Celeste Dash Logic
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = dashDir * dashSpeed;

        if (playerJuice) playerJuice.PlayDashEffect();

        PlaySFX(dashSFX);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.25f);
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    #endregion

    #region Stealth Logic
    private void HandleStealth()
    {
        // Hareket kontrolü için de moveInput kullanıyoruz
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f || Mathf.Abs(rb.linearVelocity.y) > 0.1f || isDashing;

        if (currentStealthTime <= 0)
        {
            isStealthExhausted = true;
            currentStealthTime = 0;
        }
        else if (currentStealthTime >= minStealthToReactivate)
        {
            isStealthExhausted = false;
        }

        // 3. Stealth Aktifleşme Şartları (Eski Input.GetKey yerine IsPressed)
        bool isStealthPressed = inputActions.Player.Stealth.IsPressed();

        if (isStealthPressed && !isMoving && !isStealthExhausted && currentStealthTime > 0)
        {
            IsHidden = true;
            currentStealthTime -= Time.deltaTime;

            if (spriteRenderer)
            {
                Color tempColor = originalColor;
                tempColor.a = stealthAlpha;
                spriteRenderer.color = tempColor;
            }

            gameObject.layer = LayerMask.NameToLayer(stealthLayer);
        }
        else
        {
            IsHidden = false;

            if (currentStealthTime < maxStealthTime)
            {
                currentStealthTime += Time.deltaTime * stealthRefillRate;
            }

            if (spriteRenderer) spriteRenderer.color = originalColor;
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

        // Zıplama buffer yönetimi Update'te yapılıyor (WasPressedThisFrame kullandığımız için)
    }

    private bool CheckWallOrGroundForDashJump()
    {
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
