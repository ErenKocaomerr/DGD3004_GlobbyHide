using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Lever : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public int index;
    [HideInInspector] public PuzzleManager manager;

    public Sprite offSprite;
    public Sprite onSprite;

    public bool isOn = true;

    public UnityEvent<int, bool> OnStateChanged = new UnityEvent<int, bool>();

    SpriteRenderer sr;
    Collider2D col;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        UpdateVisual();
    }

    void OnMouseDown()
    {
        Debug.Log($"[Lever] OnMouseDown index={index} manager={(manager == null ? "NULL" : "OK")} manager.IsOpen={(manager != null ? manager.IsOpen.ToString() : "N/A")} colliderEnabled={(col != null ? col.enabled.ToString() : "NO Collider")}");
        // Eðer UI elemanlarýnýn üzerine týklanýyorsa bunu engelle
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[Lever] Pointer is over UI, ignoring click.");
            return;
        }

        if (manager == null)
        {
            Debug.LogWarning($"[Lever] clicked but manager is null on {name}");
            return;
        }
        if (!manager.IsOpen)
        {
            Debug.Log("[Lever] clicked but puzzle UI not open (manager.IsOpen=false) -> ignoring");
            return;
        }

        Toggle();
    }

    public void Toggle()
    {
        SetState(!isOn);
        manager?.OnLeverToggledFromScene(index);
    }

    public void SetState(bool on)
    {
        if (isOn == on) return;
        isOn = on;
        UpdateVisual();
        OnStateChanged?.Invoke(index, isOn);
    }

    public void ForceSetSprites(Sprite offS, Sprite onS)
    {
        offSprite = offS;
        onSprite = onS;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        sr.sprite = isOn ? (onSprite != null ? onSprite : sr.sprite) : (offSprite != null ? offSprite : sr.sprite);
    }

    public void SetInteractable(bool ok)
    {
        if (col != null) col.enabled = ok;
        Debug.Log($"[Lever] SetInteractable {name} => {ok}");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (this == null) return;

        // ignore if pointer over UI element
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(eventData.pointerId))
            return;

        // check manager open
        if (this.manager == null || !this.manager.IsOpen) return;

        this.Toggle();
    }
}
