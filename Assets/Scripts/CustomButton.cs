using UnityEngine;
using UnityEngine.UI;
using TMPro;
[ExecuteInEditMode]
public class CustomButton : MonoBehaviour
{
    public Sprite enableButton, disableButton;
    public Color enableColor, disableColor;
    public TMP_FontAsset enableFont, disableFont;
    public TMP_Text text;
    public Image image;
    public bool isEnabled = true;
    void Update() {
        if(isEnabled) {
            if(image!=null) {
                image.sprite = enableButton;
                image.gameObject.GetComponent<Button>().enabled = isEnabled;
            }
            if(text!=null) {
                text.color = enableColor;
                text.font = enableFont;
            }
        } else {
            if(image!=null) {
                image.sprite = disableButton;
                image.gameObject.GetComponent<Button>().enabled = isEnabled;
            }
            if(text!=null) {
                text.color = disableColor;
                text.font = disableFont;
            }
        }
    }

    public void SetEnable(bool value) {
        isEnabled = value;
    }
}