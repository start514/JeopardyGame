using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "NewFontAttribute", menuName = "CustomSO/Attribute/FontAttribute")]
public class TMPFontAttribute : ScriptableObject
{
    public TMP_FontAsset Font;
    public float FontSize;
    public Color FontColor = Color.white;

    [Header("Only to choose style")]
    public FontStyles FontStyle;
    [Header("Only to choose the capitalization type, use same typeas FontStyles for no capitalization")]
    public FontStyles FontCapitalization;
}
