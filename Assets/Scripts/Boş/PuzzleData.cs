using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleData", menuName = "Scriptable Objects/PuzzleData")]
public class PuzzleData : ScriptableObject
{
    [Serializable]
    public class LeverEntry
    {
        public string name = "Lever";
        public bool startOn = true;           // baþlangýç ON state
        public Sprite onSprite;
        public Sprite offSprite;
    }

    [Serializable]
    public enum LeverAction { SetOn, SetOff, Toggle }

    [Serializable]
    public class LeverEffect
    {
        [Tooltip("index of target lever inside this puzzle (0..N-1)")]
        public int targetIndex;
        public LeverAction action = LeverAction.SetOff;
    }

    [Serializable]
    public class LeverRule
    {
        [Tooltip("kaynaðýn index'i (hangi lever tetikliyor)")]
        public int sourceIndex;
        [Tooltip("Bu rule yalnýzca kaynak lever ON olduðunda mý tetiklensin? false ise OFF tetiklesin.")]
        public bool triggerOnState = true;
        public List<LeverEffect> effects = new List<LeverEffect>();
    }

    [Header("Levers (order matters)")]
    public List<LeverEntry> levers = new List<LeverEntry>();

    [Header("Rules")]
    public List<LeverRule> rules = new List<LeverRule>();
}

