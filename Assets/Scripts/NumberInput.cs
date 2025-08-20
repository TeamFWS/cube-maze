using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberInput : MonoBehaviour
{
    [SerializeField] private int minValue = 1;
    [SerializeField] private int maxValue = 10;
    [SerializeField] private int defaultValue = 3;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;

    private void Start()
    {
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        inputField.characterValidation = TMP_InputField.CharacterValidation.Integer;

        inputField.onValidateInput += ValidateInput;
        plusButton.onClick.AddListener(IncrementValue);
        minusButton.onClick.AddListener(DecrementValue);

        defaultValue = Mathf.Clamp(defaultValue, minValue, maxValue);
        inputField.text = defaultValue.ToString();
    }

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        if (char.IsDigit(addedChar) ||
            (addedChar == '-' && charIndex == 0 && !inputField.text.Contains("-")))
            return addedChar;
        return '\0'; // Reject invalid input
    }

    public void IncrementValue()
    {
        var currentValue = GetIntValue();
        var newValue = Mathf.Clamp(currentValue + 1, minValue, maxValue);
        inputField.text = newValue.ToString();
    }

    public void DecrementValue()
    {
        var currentValue = GetIntValue();
        var newValue = Mathf.Clamp(currentValue - 1, minValue, maxValue);
        inputField.text = newValue.ToString();
    }

    public int GetIntValue()
    {
        if (int.TryParse(inputField.text, out var value)) return Mathf.Clamp(value, minValue, maxValue);
        return defaultValue;
    }
}