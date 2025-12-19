using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("--- Bakýþ Ayarlarý ---")]
    public float lookOffsetAmount = 4f;
    public float panSpeed = 5f;
    public float resetSpeedMultiplier = 3f;
    public float lookTimeThreshold = 0.4f;

    [Header("--- Stabilizasyon ---")]
    public float stationaryDelay = 0.15f;

    [Header("--- Düþüþ Ayarlarý ---")]
    public float fallYPanAmount = 3f;
    public float fallSpeedThreshold = -12f;

    [Header("--- Referanslar ---")]
    public CinemachineCamera vcam;
    public AdvancedPlayerController player;

    private CinemachinePositionComposer positionComposer;
    private InputSystem_Actions inputActions;

    private Vector3 defaultOffset;
    private float currentPanY;
    private float lookTimer;
    private float defaultDeadZoneHeight;
    private float stationaryTimer = 0f;

    // YENÝ: Bu offset deðiþimi Manuel Bakýþtan mý kaynaklanýyor?
    private bool isManualLookActive = false;

    void Awake() => inputActions = new InputSystem_Actions();
    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Start()
    {
        if (vcam == null) vcam = GetComponent<CinemachineCamera>();
        positionComposer = vcam.GetComponent<CinemachinePositionComposer>();

        if (positionComposer != null)
        {
            defaultOffset = positionComposer.TargetOffset;
            currentPanY = defaultOffset.y;
            defaultDeadZoneHeight = positionComposer.Composition.DeadZone.Size.y;
        }
    }

    void Update()
    {
        if (positionComposer == null || player == null) return;

        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        float targetY = defaultOffset.y;
        float currentSpeed = panSpeed;

        // 1. Durma Kontrolü (Gecikmeli)
        bool physicallyStopped = player.IsGrounded && Mathf.Abs(rb.linearVelocity.x) < 0.1f;

        if (physicallyStopped) stationaryTimer += Time.deltaTime;
        else stationaryTimer = 0f;

        bool isTrulyStable = stationaryTimer > stationaryDelay;


        // 2. Hedef Belirleme & Mantýk Ayrýmý

        // --- SENARYO A: Düþüþ (Fall) ---
        if (!player.IsGrounded && rb.linearVelocity.y < fallSpeedThreshold)
        {
            targetY = defaultOffset.y - fallYPanAmount;
            lookTimer = 0;
            currentSpeed = panSpeed * 2f;

            // Düþerken "Manuel Bakýþ" modunda olamayýz, bunu iptal et.
            // Böylece Dead Zone orijinal haline döner.
            isManualLookActive = false;
        }
        // --- SENARYO B: Manuel Bakýþ (Look) ---
        else
        {
            // Bakýþ yapmak için stabil olmalýyýz
            if (isTrulyStable && Mathf.Abs(moveInput.y) > 0.5f)
            {
                lookTimer += Time.deltaTime;

                if (lookTimer >= lookTimeThreshold)
                {
                    // Tuþa yeterince bastýk, artýk Manuel Bakýþ Modundayýz
                    isManualLookActive = true;

                    if (moveInput.y > 0) targetY = defaultOffset.y + lookOffsetAmount;
                    else targetY = defaultOffset.y - lookOffsetAmount;
                }
            }
            else
            {
                // Tuþu býraktýk veya hareket ettik
                lookTimer = 0;
                currentSpeed = panSpeed * resetSpeedMultiplier;

                // Manuel Bakýþ Modundan ne zaman çýkacaðýz?
                // 1. Eðer kamera yerine tam oturduysa
                // 2. Veya karakter hareket etmeye baþladýysa (Stabilite bozulduysa)
                if (Mathf.Abs(currentPanY - defaultOffset.y) < 0.01f || !isTrulyStable)
                {
                    isManualLookActive = false;
                }
            }
        }


        // 3. DEAD ZONE YÖNETÝMÝ (FIX BURADA)

        // Sadece ve Sadece "Manuel Bakýþ Modu" aktifse Dead Zone'u sýfýrla.
        // Düþüþten dönerken isManualLookActive "false" olacaðý için 
        // Dead Zone açýk kalýr (0.2) ve iniþ yumuþak olur.
        if (isManualLookActive)
        {
            SetDeadZone(0f);
        }
        else
        {
            SetDeadZone(defaultDeadZoneHeight);
        }


        // 4. Hareketi Uygula
        if (Mathf.Abs(currentPanY - targetY) < 0.01f) currentPanY = targetY;
        else currentPanY = Mathf.Lerp(currentPanY, targetY, currentSpeed * Time.deltaTime);

        positionComposer.TargetOffset = new Vector3(defaultOffset.x, currentPanY, defaultOffset.z);
    }

    private void SetDeadZone(float value)
    {
        // Gereksiz atama yapmamak için kontrol (Performans)
        if (Mathf.Abs(positionComposer.Composition.DeadZone.Size.y - value) > 0.001f)
        {
            var composition = positionComposer.Composition;
            composition.DeadZone.Size.y = value;
            positionComposer.Composition = composition;
        }
    }
}