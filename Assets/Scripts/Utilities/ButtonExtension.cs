using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class ButtonExtension
{
    public static void SetButtonText(this Button button, string text)
    {
        TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();

        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }
}
