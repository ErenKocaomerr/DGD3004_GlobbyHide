using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea(2, 5)] public string text;
    public bool leftSide = true;
    public Sprite portrait;
}

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();

}
