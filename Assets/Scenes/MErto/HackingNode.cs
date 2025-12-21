using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HackingNode : MonoBehaviour
{
    [Header("--- Ayarlar ---")]
    public bool isStartNode = false;
    public bool isEndNode = false;
    public bool isLocked = false;
    public List<KeyCode> unlockSequence = new List<KeyCode>();

    // YENÝ: Baþlangýçtaki kilit durumu neydi?
    private bool initialLockedState;

    public HackingNode upNode, downNode, leftNode, rightNode;

    private Image img;
    public Color normalColor = Color.white;
    public Color lockedColor = Color.red;
    public Color endColor = Color.cyan;

    void Awake()
    {
        img = GetComponent<Image>();

        // YENÝ: Oyunun en baþýnda kilitli miydim? Kaydet.
        initialLockedState = isLocked;
    }

    void Start()
    {
        UpdateVisuals();
    }

    // YENÝ: Manager bu fonksiyonu çaðýrýp node'u sýfýrlayacak
    public void ResetNode()
    {
        isLocked = initialLockedState; // Eski haline dön
        UpdateVisuals(); // Rengi düzelt
    }

    public void Unlock()
    {
        isLocked = false;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (img == null) return;

        if (isLocked) img.color = lockedColor;
        else if (isEndNode) img.color = endColor;
        else img.color = normalColor;
    }

    // Editörde çizgileri görmek için
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        if (upNode) Gizmos.DrawLine(transform.position, upNode.transform.position);
        if (downNode) Gizmos.DrawLine(transform.position, downNode.transform.position);
        if (leftNode) Gizmos.DrawLine(transform.position, leftNode.transform.position);
        if (rightNode) Gizmos.DrawLine(transform.position, rightNode.transform.position);
    }
}
