using System;
using TMPro;

public class OVRVirtualKeyboardTMPInputFieldTextHandler : OVRVirtualKeyboard.AbstractTextHandler
{
    public TMP_InputField tmpInputField;

    private bool _isSelected;

    /// <summary>
    ///     Set/Get a TMP_InputField to connect to the Virtual Keyboard
    /// </summary>
    public TMP_InputField TMPInputField
    {
        get => tmpInputField;
        set
        {
            if (value == tmpInputField) return;
            if (tmpInputField) tmpInputField.onValueChanged.RemoveListener(ProxyOnValueChanged);
            tmpInputField = value;
            if (tmpInputField) tmpInputField.onValueChanged.AddListener(ProxyOnValueChanged);
            OnTextChanged?.Invoke(Text);
        }
    }

    public override Action<string> OnTextChanged { get; set; }

    public override string Text => tmpInputField ? tmpInputField.text : string.Empty;

    public override bool SubmitOnEnter =>
        tmpInputField && tmpInputField.lineType != TMP_InputField.LineType.MultiLineNewline;

    public override bool IsFocused => tmpInputField && tmpInputField.isFocused;

    protected void Start()
    {
        if (tmpInputField) tmpInputField.onValueChanged.AddListener(ProxyOnValueChanged);
    }

    public override void Submit()
    {
        if (tmpInputField) tmpInputField.onEndEdit.Invoke(tmpInputField.text);
    }

    public override void AppendText(string s)
    {
        if (tmpInputField) tmpInputField.text += s;
    }

    public override void ApplyBackspace()
    {
        if (tmpInputField && !string.IsNullOrEmpty(tmpInputField.text))
            tmpInputField.text = Text.Substring(0, Text.Length - 1);
    }

    public override void MoveTextEnd()
    {
        if (tmpInputField) tmpInputField.MoveTextEnd(false);
    }

    protected void ProxyOnValueChanged(string arg0)
    {
        OnTextChanged?.Invoke(arg0);
    }
}