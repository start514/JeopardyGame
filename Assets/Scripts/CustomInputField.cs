using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
public class CustomInputField : MonoBehaviour
{
    public Sprite enableBG, disableBG;
    public Color enableColor, disableColor;
    public Font enableFont, disableFont;
    public Text text, placeholderText;
    public Image image;
    public InputField inputField;
    public bool isEnabled = true;
    void Update() {
        if(isEnabled) {
            if(image!=null) {
                image.sprite = enableBG;
                if(inputField!=null) inputField.enabled = isEnabled;
            }
            if(text!=null) {
                text.color = enableColor;
                text.font = enableFont;
            }
            if(placeholderText!=null) {
                placeholderText.color = enableColor;
                placeholderText.font = enableFont;
            }
        } else {
            if(image!=null) {
                image.sprite = disableBG;
                if(inputField!=null) inputField.enabled = isEnabled;
            }
            if(text!=null) {
                text.color = disableColor;
                text.font = disableFont;
            }
            if(placeholderText!=null) {
                placeholderText.color = disableColor;
                placeholderText.font = disableFont;
            }
        }
    }

    public void SetEnable(bool value) {
        isEnabled = value;
    }
}