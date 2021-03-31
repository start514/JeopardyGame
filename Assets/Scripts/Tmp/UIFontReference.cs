using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIFontReference : MonoBehaviour
{
    public TMPFontAttribute TMPAttribute;

    [Header("Override color set in attribute")]
    public bool OverrideColor = false;
    [SerializeField]
    private Color m_Color;

    private TextMeshProUGUI m_TextMeshProUGUI;

    void Start()
    {
        m_TextMeshProUGUI = GetComponent<TextMeshProUGUI>();

        SetFontAsset();
        SetFontSize();
        SetFontColor();
        SetStyle();
    }

    private void SetFontAsset()
    {
        m_TextMeshProUGUI.font = TMPAttribute.Font;
    }

    private void SetFontSize()
    {
        m_TextMeshProUGUI.fontSize = TMPAttribute.FontSize;
    }

    private void SetFontColor()
    {
        if (!OverrideColor)
        {
            m_TextMeshProUGUI.color = TMPAttribute.FontColor;
        }
        else
        {
            SetColor(m_Color);
        }
    }

    private void SetStyle()
    {
        m_TextMeshProUGUI.fontStyle = TMPAttribute.FontStyle | TMPAttribute.FontCapitalization;
    }

    public void SetColor(Color color)
    {
        m_TextMeshProUGUI.color = color;
    }
}
