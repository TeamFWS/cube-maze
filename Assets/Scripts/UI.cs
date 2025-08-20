using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject heightInput;
    [SerializeField] private GameObject widthInput;
    [SerializeField] private GameObject depthInput;
    [SerializeField] private GameObject seedInput;
    [SerializeField] private GameObject inputs;
    [SerializeField] private GameObject mazeGenerator;
    [SerializeField] private GameObject timer;
    private int depth;

    private int height;
    private int seed;
    private bool showTimer;
    private float time;
    private int width;

    private void Start()
    {
        showTimer = false;
        timer.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) inputs.SetActive(true);
        if (Input.GetKeyDown(KeyCode.R)) ButtonPress();
        if (showTimer)
        {
            timer.SetActive(true);
            time = mazeGenerator.GetComponent<MazeDisplay>().GetTime();
            timer.GetComponent<TMP_Text>().text = time.ToString("#.00");
        }
    }

    public void ButtonPress()
    {
        time = 0.0f;
        showTimer = true;
        int.TryParse(heightInput.GetComponent<TMP_InputField>().text, out height);
        int.TryParse(widthInput.GetComponent<TMP_InputField>().text, out width);
        int.TryParse(depthInput.GetComponent<TMP_InputField>().text, out depth);
        int.TryParse(seedInput.GetComponent<TMP_InputField>().text, out seed);
        if (height >= 1 && width >= 1 && depth >= 1 && height * width * depth > 6)
        {
            mazeGenerator.GetComponent<MazeDisplay>().UpdateParameters(height, width, depth, seed);
            inputs.SetActive(false);
        }
    }
}