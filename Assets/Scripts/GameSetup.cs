using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSetup : MonoBehaviour
{
    [SerializeField] private NumberInput singleHeightInput;
    [SerializeField] private NumberInput singleWidthInput;
    [SerializeField] private NumberInput singleDepthInput;
    [SerializeField] private Toggle singleSeedToggle;
    [SerializeField] private GameObject singleSeedInput;
    [SerializeField] private GameObject singleNameInput;
    [SerializeField] private GameObject singleStartButton;

    [SerializeField] private NumberInput multiHeightInput;
    [SerializeField] private NumberInput multiWidthInput;
    [SerializeField] private NumberInput multiDepthInput;
    [SerializeField] private Toggle multiSeedToggle;
    [SerializeField] private GameObject multiSeedInput;
    [SerializeField] private NumberInput multiNumberNamesInput;
    [SerializeField] private GameObject multiStartButton;
    [SerializeField] private GameObject mazeGenerator;
    [SerializeField] private GameObject setupCube;

    [SerializeField] private GameObject frontPanel;
    [SerializeField] private GameObject singlePanel;
    [SerializeField] private GameObject multiPanel;
    [SerializeField] private GameObject namesPanel;
    [SerializeField] private GameObject midGamePanel;

    [SerializeField] private List<GameObject> names;

    private int currentSide = 1;

    private int seed;

    private void Start()
    {
        BlockOtherPanels();
    }

    public void StartSingle()
    {
        var height = singleHeightInput.GetIntValue();
        var width = singleWidthInput.GetIntValue();
        var depth = singleDepthInput.GetIntValue();
        var name = singleNameInput.GetComponent<TMP_InputField>().text;

        if (singleSeedToggle.isOn)
            int.TryParse(singleSeedInput.GetComponent<TMP_InputField>().text, out seed);
        else
            seed = Random.Range(1, 1000000);
        if (height >= 1 && width >= 1 && depth >= 1 && height * width * depth > 6)
        {
            mazeGenerator.GetComponent<MazeDisplay>().UpdateParameters(height, width, depth, seed);
            singleStartButton.GetComponent<Image>().color = Color.green;
            midGamePanel.SetActive(true);
            midGamePanel.GetComponent<MidGameUI>().UpdateParameters(height, width, depth, seed, name);
            setupCube.SetActive(false);
        }
        else
        {
            singleStartButton.GetComponent<Image>().color = Color.red;
        }
    }

    public void StartMulti()
    {
        var height = multiHeightInput.GetIntValue();
        var width = multiWidthInput.GetIntValue();
        var depth = multiDepthInput.GetIntValue();

        if (multiSeedToggle.isOn)
            int.TryParse(multiSeedInput.GetComponent<TMP_InputField>().text, out seed);
        else
            seed = Random.Range(1, 1000000);
        if (height >= 1 && width >= 1 && depth >= 1 && height * width * depth > 6)
        {
            mazeGenerator.GetComponent<MazeDisplay>().UpdateParameters(height, width, depth, seed);
            multiStartButton.GetComponent<Image>().color = Color.green;
            midGamePanel.SetActive(true);
            midGamePanel.GetComponent<MidGameUI>().UpdateParameters(height, width, depth, seed, "name");
            setupCube.SetActive(false);
            midGamePanel.GetComponent<MidGameUI>().SetUpMultiplayer(multiNumberNamesInput.GetIntValue());
        }
        else
        {
            multiStartButton.GetComponent<Image>().color = Color.red;
        }
    }

    public void StartRotating(int side)
    {
        // 0 - single, 1 - mode selection, 2 - multi, 3 - multi names
        if (side == 3) UpdateNumberOfNamesToEnter();
        if (currentSide - side == 1)
            StartCoroutine(RotateMe(Vector3.down * 90, 0.8f));
        else
            StartCoroutine(RotateMe(Vector3.up * 90, 0.8f));
        currentSide = side;
        BlockOtherPanels();
    }

    private void UpdateNumberOfNamesToEnter()
    {
        var nrNames = multiNumberNamesInput.GetIntValue();
        for (var i = 0; i < 10; i++)
            if (i < nrNames)
                names[i].SetActive(true);
            else
                names[i].SetActive(false);
    }

    private void BlockOtherPanels()
    {
        switch (currentSide)
        {
            case 0:
                frontPanel.SetActive(false);
                singlePanel.SetActive(true);
                multiPanel.SetActive(false);
                namesPanel.SetActive(false);
                break;
            case 1:
                frontPanel.SetActive(true);
                singlePanel.SetActive(false);
                multiPanel.SetActive(false);
                namesPanel.SetActive(false);
                break;
            case 2:
                frontPanel.SetActive(true);
                singlePanel.SetActive(false);
                multiPanel.SetActive(true);
                namesPanel.SetActive(false);
                break;
            case 3:
                frontPanel.SetActive(false);
                singlePanel.SetActive(false);
                multiPanel.SetActive(false);
                namesPanel.SetActive(true);
                break;
        }
    }

    private IEnumerator RotateMe(Vector3 byAngles, float inTime)
    {
        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(transform.eulerAngles + byAngles);
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }
    }
}