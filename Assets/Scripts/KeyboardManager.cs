using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardManager : MonoBehaviour
{
    public GameObject virtualKeyboard;
    public OVRVirtualKeyboardTMPInputFieldTextHandler textHandler;
    public string inputTag = "InputField";

    private TMP_InputField[] _tmpInputFields;

    private void Start()
    {
        _tmpInputFields = FindInputFieldsByTag<TMP_InputField>(inputTag);

        foreach (var tmpInputField in _tmpInputFields)
        {
            tmpInputField.onSelect.AddListener(OnTMPInputFieldFocus);
            tmpInputField.onEndEdit.AddListener(OnTMPInputFieldBlur);
        }

        SetKeyboardActive(false);
    }

    private void OnDestroy()
    {
        foreach (var tmpInputField in _tmpInputFields)
        {
            tmpInputField.onSelect.RemoveListener(OnTMPInputFieldFocus);
            tmpInputField.onEndEdit.RemoveListener(OnTMPInputFieldBlur);
        }
    }

    private T[] FindInputFieldsByTag<T>(string tag) where T : Component
    {
        var taggedObjects = GameObject.FindGameObjectsWithTag(tag);
        return Array.ConvertAll(taggedObjects, obj => obj.GetComponent<T>());
    }

    private void OnTMPInputFieldFocus(string text)
    {
        var focusedInputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();

        if (focusedInputField != null)
        {
            textHandler.TMPInputField = focusedInputField;
            textHandler.OnTextChanged?.Invoke(focusedInputField.text);
        }

        SetKeyboardActive(true);
    }

    private void OnTMPInputFieldBlur(string text)
    {
        SetKeyboardActive(false);
    }

    private void SetKeyboardActive(bool isActive)
    {
        if (virtualKeyboard != null) virtualKeyboard.SetActive(isActive);
    }
}