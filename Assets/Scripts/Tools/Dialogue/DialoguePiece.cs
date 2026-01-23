using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialoguePiece
{
    public string ID;

    public Sprite image;
    [TextArea]
    public string text;

    public bool playTineLine;

    public List<DialogueOption> options = new List<DialogueOption>();
}
