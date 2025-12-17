using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PuzzleUI : MonoBehaviour
{
    [Header("UI refs")]
    public GameObject uiRoot;              // panel root (inactive by default)
    public Transform buttonContainer;      // where to instantiate buttons
    public Button buttonPrefab;            // button prefab (should have child "Icon" Image and TMP text)
    public TMP_Text titleText;
    public Button closeButton;             // optional
    public Sprite open;
    public Sprite close;

    // runtime
    PuzzleManager currentManager;
    PuzzleData currentData;
    List<Button> createdButtons = new List<Button>();

    void Start()
    {
        if (uiRoot != null) uiRoot.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(() => { currentManager?.ClosePuzzle(); });
    }

    // show UI and create N buttons according to puzzleData
    public void Show(PuzzleManager manager, PuzzleData data, bool[] initialStates)
    {
        Hide();
        currentManager = manager;
        currentData = data;

        if (uiRoot != null) uiRoot.SetActive(true);
        if (titleText != null) titleText.text = manager != null ? manager.name : "Puzzle";

        if (buttonContainer == null || buttonPrefab == null || data == null)
        {
            Debug.LogWarning("[PuzzleUI] buttonContainer or buttonPrefab or data is null!");
            return;
        }

        for (int i = 0; i < data.levers.Count; i++)
        {
            var entry = data.levers[i];
            var btn = Instantiate(buttonPrefab, buttonContainer);
            createdButtons.Add(btn);

            int idx = i;
            var txt = btn.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = entry.name;

            var icon = btn.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                bool state = initialStates != null && initialStates.Length > idx && initialStates[idx];
                icon.sprite = state ? entry.onSprite : entry.offSprite;
                icon.SetNativeSize();
            }

            btn.onClick.AddListener(() =>
            {
                // call manager method to handle toggle + rules + visuals
                currentManager?.OnPlayerPressedLever(idx);
            });
        }
    }

    // call to refresh all visuals (manager will call after state changes applied)
    public void UpdateAllButtonVisuals(bool[] states)
    {
        if (currentData == null) return;

        for (int i = 0; i < createdButtons.Count && i < currentData.levers.Count; i++)
        {
            var entry = currentData.levers[i];
            var icon = createdButtons[i].transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                bool state = states != null && states.Length > i && states[i];
                icon.sprite = state ? entry.onSprite : entry.offSprite;
                Debug.Log("AYARLANDII");
            }

            var txt = createdButtons[i].GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                bool state = states != null && states.Length > i && states[i];
                txt.text = $"{entry.name}\n{(state ? "ON" : "OFF")}";
            }
        }
    }

    public void Hide()
    {
        foreach (var b in createdButtons)
        {
            if (b != null) Destroy(b.gameObject);
        }
        createdButtons.Clear();

        currentManager = null;
        currentData = null;

        if (uiRoot != null) uiRoot.SetActive(false);
    }
}
