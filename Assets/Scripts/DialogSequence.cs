using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class DialogSequence : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueRoot;         // root panel (inactive by default)
    public TMP_Text speakerNameLeft;
    public TMP_Text speakerNameRight;
    public Image portraitLeft;
    public Image portraitRight;
    public TMP_Text contentText;

    [Header("Options")]
    public bool pauseGameWhileDialog = false;
    public UnityEvent OnDialogueComplete;

    List<DialogueLine> currentLines = null;
    int index = 0;
    bool active = false;
    int endDialog = 0;

    void Start()
    {
        if (dialogueRoot != null) dialogueRoot.SetActive(false);
    }

    void Update()
    {
        if (!active) return;
        // Next on left mouse button (you can replace with UI button)
        if (Input.GetMouseButtonDown(0))
            Next();

        if (endDialog >= 3)
        {
            SceneManager.LoadScene("TownScene");
            Time.timeScale = 0f;
        }
    }

    public void Play(DialogueData data)
    {
        if (data == null || data.lines == null || data.lines.Count == 0)
        {
            Debug.LogWarning("DialogueSequence.Play: DialogueData boþ.");
            return;
        }

        currentLines = data.lines;
        index = 0;
        ShowLine(index);

        if (dialogueRoot != null) dialogueRoot.SetActive(true);
        active = true;

        if (pauseGameWhileDialog)
            Time.timeScale = 0f;
    }

    public void Next()
    {
        if (!active) return;
        index++;
        if (index >= currentLines.Count)
        {
            End();
            return;
        }
        ShowLine(index);
    }

    void ShowLine(int i)
    {
        if (currentLines == null || i < 0 || i >= currentLines.Count) return;
        var line = currentLines[i];

        if (contentText != null) contentText.text = line.text;

        if (line.leftSide)
        {
            if (speakerNameLeft != null) speakerNameLeft.text = line.speaker;
            if (speakerNameRight != null) speakerNameRight.text = "";
            if (portraitLeft != null) portraitLeft.sprite = line.portrait;
            if (portraitRight != null) portraitRight.sprite = null;
            portraitRight.enabled = false;
            portraitLeft.enabled = true;
            // optionally highlight left panel
        }
        else
        {
            if (speakerNameRight != null) speakerNameRight.text = line.speaker;
            if (speakerNameLeft != null) speakerNameLeft.text = "";
            if (portraitRight != null) portraitRight.sprite = line.portrait;
            if (portraitLeft != null) portraitLeft.sprite = null;
            portraitLeft.enabled = false;
            portraitRight.enabled = true;
        }
    }

    void End()
    {
        active = false;
        if (dialogueRoot != null) dialogueRoot.SetActive(false);

        if (pauseGameWhileDialog)
            Time.timeScale = 1f;

        endDialog++;
        CameraFocusController.Instance.ResetCamera();
        OnDialogueComplete?.Invoke();
    }


    public void ApplyDialogRootTransform(Transform overrideTransform, bool matchWorldPosition = true, bool setParent = false)
    {
        if (dialogueRoot == null || overrideTransform == null) return;

        if (setParent)
        {
            dialogueRoot.transform.SetParent(overrideTransform, worldPositionStays: true);
        }

        if (matchWorldPosition)
        {
            dialogueRoot.transform.position = overrideTransform.position;
            dialogueRoot.transform.rotation = overrideTransform.rotation;
            dialogueRoot.transform.localScale = overrideTransform.localScale;
        }
        else
        {
            // sadece local transform olarak eþitle
            dialogueRoot.transform.localPosition = overrideTransform.localPosition;
            dialogueRoot.transform.localRotation = overrideTransform.localRotation;
            dialogueRoot.transform.localScale = overrideTransform.localScale;
        }
    }
}
