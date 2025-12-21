using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HackingUIManager : MonoBehaviour
{
    [Header("--- UI Referanslarý ---")]
    public GameObject comboPanel;
    public GameObject arrowPrefab;
    public Transform arrowContainer;

    [Header("--- Ok Sprite'larý ---")]
    public Sprite[] arrowSprites; // 0: Up, 1: Down, 2: Left, 3: Right

    // Ekranda duran oklarýn listesi
    private List<GameObject> spawnedArrows = new List<GameObject>();

    public void ShowCombo(List<KeyCode> combination)
    {
        comboPanel.SetActive(true);

        // Önce temizlik yap (Eski veya hatalý denemeden kalanlar varsa sil)
        ClearArrows();

        // Yeni kombinasyonu diz
        foreach (KeyCode key in combination)
        {
            GameObject newArrow = Instantiate(arrowPrefab, arrowContainer);
            Image img = newArrow.GetComponent<Image>();

            if (key == KeyCode.UpArrow) img.sprite = arrowSprites[0];
            else if (key == KeyCode.DownArrow) img.sprite = arrowSprites[1];
            else if (key == KeyCode.LeftArrow) img.sprite = arrowSprites[2];
            else if (key == KeyCode.RightArrow) img.sprite = arrowSprites[3];

            spawnedArrows.Add(newArrow);
        }
    }

    // --- YENÝ: Doðru bilineni yok et ---
    public void RemoveFirstArrow()
    {
        if (spawnedArrows.Count > 0)
        {
            // Listeden ve sahneden kaldýr
            GameObject arrowToRemove = spawnedArrows[0];
            spawnedArrows.RemoveAt(0);
            Destroy(arrowToRemove);
        }
    }

    // Hata yapýlýnca efekt ver (Ýstersen ekraný salla veya kýrmýzý yap)
    public void FailEffect()
    {
        // Burada basit bir titreme veya ses efekti yönetilebilir
        // Þimdilik Player scripti direkt ShowCombo çaðýrarak resetleyecek.
    }

    public void HideCombo()
    {
        comboPanel.SetActive(false);
        ClearArrows();
    }

    private void ClearArrows()
    {
        foreach (GameObject obj in spawnedArrows)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedArrows.Clear();
    }
}
