using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MidGameUI : MonoBehaviour
{
    [SerializeField] private GameObject heightText;
    [SerializeField] private GameObject widthText;
    [SerializeField] private GameObject depthText;
    [SerializeField] private GameObject seedText;
    [SerializeField] private GameObject nameText;
    [SerializeField] private GameObject mazeGenerator;
    [SerializeField] private GameObject timer;

    [SerializeField] private GameObject setupCube;
    [SerializeField] private GameObject midGamePanel;

    [SerializeField] private List<GameObject> nameInputs;
    private int currentplayer;

    private bool multiplayer;
    private List<string> names;
    private int nrOfPlayers;


    private double time;
    private List<double> times;
    private int UI_depth;
    private int UI_height;
    private string UI_name;
    private int UI_seed;
    private int UI_width;
    private bool updatingTimer;

    private void Start()
    {
        updatingTimer = false;
    }


    private void Update()
    {
        if (updatingTimer)
        {
            time = mazeGenerator.GetComponent<MazeDisplay>().GetTime();
            timer.GetComponent<TMP_Text>().text = time.ToString("#.00");
        }
    }

    public string GetNextPlayerName()
    {
        return UI_name;
    }

    public void UpdateParameters(int height, int width, int depth, int seed, string name)
    {
        UI_height = height;
        UI_width = width;
        UI_depth = depth;
        UI_seed = seed;
        UI_name = name;

        heightText.GetComponent<TMP_Text>().text = UI_height.ToString();
        widthText.GetComponent<TMP_Text>().text = UI_width.ToString();
        depthText.GetComponent<TMP_Text>().text = UI_depth.ToString();
        seedText.GetComponent<TMP_Text>().text = UI_seed.ToString();
        nameText.GetComponent<TMP_Text>().text = UI_name;

        multiplayer = false;
        currentplayer = 0;
        updatingTimer = true;
    }

    public void SetUpMultiplayer(int numberOfPlayers)
    {
        multiplayer = true;
        nrOfPlayers = numberOfPlayers;
        for (var i = 0; i < 10; i++) names[i] = nameInputs[i].GetComponent<TMP_InputField>().text;
        UI_name = names[0];
        nameText.GetComponent<TMP_Text>().text = UI_name;
    }

    public bool NextPlayerSetUp()
    {
        if (!multiplayer)
        {
            updatingTimer = false;
            return false;
        }

        times[currentplayer] = time;
        currentplayer++;
        if (currentplayer == nrOfPlayers)
        {
            return false;
        }

        UI_name = names[currentplayer];
        return true;
    }

    public void AbortPuzzle()
    {
        updatingTimer = false;
        mazeGenerator.GetComponent<MazeDisplay>().DestroyPuzzle();
        setupCube.SetActive(true);
        midGamePanel.SetActive(false);
    }

    public void SaveAndEnd()
    {
        int gameID;
        if (multiplayer)
        {
            if (PlayerPrefs.HasKey("MultiSaved"))
            {
                gameID = PlayerPrefs.GetInt("MultiSaved");
                gameID++;
            }
            else
            {
                gameID = 1;
            }

            PlayerPrefs.SetInt("MultiGame" + gameID + "height", UI_height);
            PlayerPrefs.SetInt("MultiGame" + gameID + "width", UI_width);
            PlayerPrefs.SetInt("MultiGame" + gameID + "depth", UI_depth);
            PlayerPrefs.SetInt("MultiGame" + gameID + "seed", UI_seed);
            PlayerPrefs.SetInt("MultiGame" + gameID + "NrOfPlayers", nrOfPlayers);
            for (var i = 0; i < nrOfPlayers; i++)
            {
                PlayerPrefs.SetString("MultiGame" + gameID + "Player" + i + "Time", times[i].ToString("#.00"));
                PlayerPrefs.SetString("MultiGame" + gameID + "Player" + i + "Nickname", names[i]);
            }

            PlayerPrefs.SetInt("MultiSaved", gameID);
        }
        else
        {
            if (PlayerPrefs.HasKey("SingleSaved"))
            {
                gameID = PlayerPrefs.GetInt("SingleSaved");
                gameID++;
            }
            else
            {
                gameID = 1;
            }

            PlayerPrefs.SetInt("SingleGame" + gameID + "height", UI_height);
            PlayerPrefs.SetInt("SingleGame" + gameID + "width", UI_width);
            PlayerPrefs.SetInt("SingleGame" + gameID + "depth", UI_depth);
            PlayerPrefs.SetInt("SingleGame" + gameID + "seed", UI_seed);
            PlayerPrefs.SetString("SingleGame" + gameID + "Time", time.ToString("#.00"));
            PlayerPrefs.SetString("SingleGame" + gameID + "Nickname", UI_name);
            PlayerPrefs.SetInt("SingleSaved", gameID);
        }

        setupCube.SetActive(true);
        midGamePanel.SetActive(false);
    }
}