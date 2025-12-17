using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public PuzzleData puzzleData;
    public PuzzleUI puzzleUI;
    public bool disablePlayerOnOpen = true;
    public string playerTag = "Player";
    public bool closeOnSolved = true;

    bool[] leverStates;
    public bool IsOpen { get; private set; } = false;

    public Lever[] sceneLevers;

    void Awake()
    {
        if (puzzleData == null)
        {
            Debug.LogError($"{name}: puzzleData atanmamýþ! Lütfen inspector'dan ata.");
            leverStates = new bool[0];
            return;
        }

        int n = Mathf.Max(1, puzzleData.levers.Count);
        leverStates = new bool[n];

        for (int i = 0; i < puzzleData.levers.Count; i++)
            leverStates[i] = puzzleData.levers[i].startOn;

        Debug.Log($"[PuzzleManager] {name} Awake: puzzleData lever count = {puzzleData.levers.Count}, sceneLevers length = {sceneLevers?.Length ?? 0}");

        for (int i = 0; i < sceneLevers.Length; i++)
        {
            var lv = sceneLevers[i];
            if (lv == null)
            {
                Debug.LogWarning($"[PuzzleManager] sceneLevers[{i}] null in {name}");
                continue;
            }
            lv.index = i;
            lv.manager = this;

            if (i < puzzleData.levers.Count)
            {
                lv.ForceSetSprites(puzzleData.levers[i].offSprite, puzzleData.levers[i].onSprite);
                lv.isOn = leverStates[i];
                lv.UpdateVisual();
                lv.SetInteractable(false);
            }
            else
            {
                lv.SetInteractable(false);
                Debug.LogWarning($"[PuzzleManager] scene lever {lv.name} (index {i}) has no corresponding PuzzleData entry.");
            }
        }
    }

    public void OpenPuzzle()
    {
        if (IsOpen) return;
        IsOpen = true;
        Debug.Log($"[PuzzleManager] OpenPuzzle called on {name}");

        if (puzzleUI != null) puzzleUI.Show(this, puzzleData, leverStates);

        if (disablePlayerOnOpen)
        {
            var player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                var ctrl = player.GetComponent<BasicTopdownController>();
                if (ctrl != null) ctrl.enabled = false;
            }
        }

        for (int i = 0; i < sceneLevers.Length; i++)
        {
            var lv = sceneLevers[i];
            if (lv == null) continue;
            if (i < puzzleData.levers.Count)
            {
                lv.ForceSetSprites(puzzleData.levers[i].offSprite, puzzleData.levers[i].onSprite);
                lv.SetState(leverStates[i]);
            }
            lv.SetInteractable(true);
        }
    }

    public void ClosePuzzle()
    {
        if (!IsOpen) return;
        IsOpen = false;
        Debug.Log($"[PuzzleManager] ClosePuzzle called on {name}");
        if (puzzleUI != null) puzzleUI.Hide();

        if (disablePlayerOnOpen)
        {
            var player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                var ctrl = player.GetComponent<BasicTopdownController>();
                if (ctrl != null) ctrl.enabled = true;
            }
        }

        for (int i = 0; i < sceneLevers.Length; i++)
            if (sceneLevers[i] != null) sceneLevers[i].SetInteractable(false);
    }

    // NEW: called by UI buttons
    public void OnPlayerPressedLever(int index)
    {
        Debug.Log($"[PuzzleManager] UI pressed lever {index}");
        if (index < 0 || index >= leverStates.Length)
        {
            Debug.LogWarning($"[PuzzleManager] OnPlayerPressedLever index out of range: {index}");
            return;
        }

        // toggle the logical state
        leverStates[index] = !leverStates[index];

        // apply rules
        ApplyRulesFor(index, leverStates[index]);

        // update visuals
        SyncVisuals();

        if (CheckSolved()) OnSolved();
    }

    public void OnLeverToggledFromScene(int index)
    {
        Debug.Log($"[PuzzleManager] OnLeverToggledFromScene index={index} (IsOpen={IsOpen})");
        if (index < 0 || index >= leverStates.Length)
        {
            Debug.LogWarning($"[PuzzleManager] index out of range: {index} (leverStates length={leverStates.Length})");
            return;
        }

        if (index < sceneLevers.Length && sceneLevers[index] != null)
            leverStates[index] = sceneLevers[index].isOn;
        else
            leverStates[index] = !leverStates[index];

        ApplyRulesFor(index, leverStates[index]);
        SyncVisuals();

        if (CheckSolved()) OnSolved();
    }

    void ApplyRulesFor(int sourceIndex, bool newState)
    {
        foreach (var rule in puzzleData.rules)
        {
            if (rule.sourceIndex != sourceIndex) continue;
            if (rule.triggerOnState != newState) continue;

            foreach (var eff in rule.effects)
            {
                int t = eff.targetIndex;
                if (t < 0 || t >= leverStates.Length) { Debug.LogWarning($"[PuzzleManager] effect targetIndex out of range: {t}"); continue; }

                switch (eff.action)
                {
                    case PuzzleData.LeverAction.SetOn: leverStates[t] = true; break;
                    case PuzzleData.LeverAction.SetOff: leverStates[t] = false; break;
                    case PuzzleData.LeverAction.Toggle: leverStates[t] = !leverStates[t]; break;
                }
            }
        }
    }

    void SyncVisuals()
    {
        for (int i = 0; i < sceneLevers.Length && i < leverStates.Length; i++)
        {
            var lv = sceneLevers[i];
            if (lv == null) continue;
            lv.SetState(leverStates[i]);
        }

        if (puzzleUI != null) puzzleUI.UpdateAllButtonVisuals(leverStates);
    }

    bool CheckSolved()
    {
        for (int i = 0; i < leverStates.Length; i++) if (leverStates[i]) return false;
        return true;
    }

    void OnSolved()
    {
        Debug.Log($"[PuzzleManager] {name} solved!");
        if (closeOnSolved) ClosePuzzle();
        var interact = GetComponent<PuzzleInteractable>();
        if (interact != null) interact.enabled = false;
    }
}
