using UnityEngine;
using UnityEngine.UI;

public class DragThrowController : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public LayerMask stoneLayer; // set the Stone layer in Inspector
    public Image chargeBar;      // optional UI

    [Header("Throw tuning")]
    public float baseForce = 2f;
    public float chargeSpeed = 1.2f;   // unused for velocity method but keep for fallback
    public float maxCharge = 3f;

    [Header("Follow")]
    public float followHeight = 1.5f; // when grabbed, keep stone slightly above cursor

    Rigidbody2D grabbedRb;
    Stone grabbedStone;
    Collider2D grabbedCol;
    bool isHolding = false;

    // velocity calc
    Vector2 lastMouseWorld;
    Vector2 mouseVelocity;
    float heldTime;
    float chargeValue;

    void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        // Press -> try grab stone under cursor
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 world = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D col = Physics2D.OverlapPoint(world, stoneLayer);
            if (col != null)
            {
                var st = col.GetComponentInParent<Stone>();
                if (st != null && !st.isGrabbed)
                {
                    grabbedStone = st;
                    grabbedRb = st.GetComponent<Rigidbody2D>();
                    grabbedCol = st.GetComponent<Collider2D>();
                    BeginGrab(world);
                }
            }
        }

        // While holding -> follow mouse and compute velocity/charge
        if (isHolding && grabbedStone != null)
        {
            Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            // move grabbed stone to mouse X, and fixed Y above ground for easier control
            Vector3 pos = grabbedStone.transform.position;
            pos.x = mouseWorld.x;
            pos.y = mouseWorld.y + followHeight;
            grabbedStone.transform.position = pos;

            // compute mouse velocity (simple)
            mouseVelocity = (mouseWorld - lastMouseWorld) / Mathf.Max(Time.deltaTime, 1e-6f);
            lastMouseWorld = mouseWorld;

            // optional charge (keeps old style)
            heldTime += Time.deltaTime;
            chargeValue = Mathf.Min(maxCharge, chargeValue + chargeSpeed * Time.deltaTime);
            if (chargeBar != null) chargeBar.fillAmount = chargeValue / maxCharge;
        }

        // Release -> compute throw impulse and apply
        if (Input.GetMouseButtonUp(0) && isHolding && grabbedStone != null)
        {
            Vector2 releaseMouse = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            EndGrabAndThrow(releaseMouse);
        }
    }

    void BeginGrab(Vector2 mouseWorld)
    {
        if (grabbedStone == null) return;

        grabbedStone.OnGrab();
        isHolding = true;
        lastMouseWorld = mouseWorld;
        mouseVelocity = Vector2.zero;
        heldTime = 0f;
        chargeValue = 0f;
        if (chargeBar != null) chargeBar.fillAmount = 0f;
    }

    void EndGrabAndThrow(Vector2 mouseWorld)
    {
        // Determine direction and impulse
        Vector2 dir = (mouseWorld - (Vector2)grabbedStone.transform.position);
        // If player didn't move much, use mouseVelocity horizontal
        if (dir.sqrMagnitude < 0.01f)
        {
            dir = new Vector2(Mathf.Sign(mouseVelocity.x == 0f ? 1f : mouseVelocity.x), 0.3f);
        }
        dir = dir.normalized;

        // Use mouseVelocity magnitude as extra force (clamped)
        float velMag = Mathf.Clamp(mouseVelocity.magnitude * 0.02f, 0f, maxCharge); // scaler tweak
        float force = baseForce + velMag + (chargeValue * 0.1f);

        Vector2 impulse = dir * force;

        // release stone (Stone handles kinematic->dynamic)
        grabbedStone.OnRelease(impulse);

        // reset
        grabbedStone = null;
        grabbedRb = null;
        grabbedCol = null;
        isHolding = false;
        if (chargeBar != null) chargeBar.fillAmount = 0f;
    }
}
