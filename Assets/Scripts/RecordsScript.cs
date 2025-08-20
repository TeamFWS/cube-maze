using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RecordsScript : MonoBehaviour
{
    [SerializeField] private GameObject MainViewPanel;
    [SerializeField] private GameObject SinglePanel;
    [SerializeField] private GameObject MultiPanel;
    [SerializeField] private GameObject NoDataPanel;

    [SerializeField] private List<GameObject> SingleValues;
    [SerializeField] private List<GameObject> MultiValues;
    [SerializeField] private GameObject MultiMainValues;
    private int currentPanel;

    private int PageNumber;

    public void PageUp()
    {
        PageNumber++;
        LoadPanelPage();
    }

    public void PageDown()
    {
        PageNumber--;
        LoadPanelPage();
    }

    public void SwapPanel(int panel)
    {
        //0 Main, 1 Single, 2 Multi, 3 NoData  
        if (panel == 1 && !PlayerPrefs.HasKey("SingleSaved")) panel = 3;
        if (panel == 2 && !PlayerPrefs.HasKey("MultiSaved")) panel = 3;
        currentPanel = panel;
        switch (panel)
        {
            case 0:
                MainViewPanel.SetActive(true);
                SinglePanel.SetActive(false);
                MultiPanel.SetActive(false);
                NoDataPanel.SetActive(false);
                break;
            case 1:
                PageNumber = 1;
                LoadPanelPage();
                MainViewPanel.SetActive(false);
                SinglePanel.SetActive(true);
                MultiPanel.SetActive(false);
                NoDataPanel.SetActive(false);
                break;
            case 2:
                PageNumber = 1;
                LoadPanelPage();
                MainViewPanel.SetActive(false);
                SinglePanel.SetActive(false);
                MultiPanel.SetActive(true);
                NoDataPanel.SetActive(false);
                break;
            case 3:
                MainViewPanel.SetActive(false);
                SinglePanel.SetActive(false);
                MultiPanel.SetActive(false);
                NoDataPanel.SetActive(true);
                break;
        }
    }

    private void LoadPanelPage()
    {
        if (PageNumber < 0) PageNumber = 0;
        if (currentPanel == 1)
        {
            var nrOfRecords = PlayerPrefs.GetInt("SingleSaved");
            if (PageNumber * 10 + 1 > nrOfRecords) PageNumber--;
            for (var i = 0; i < 10; i++)
            {
                var recordNumber = PageNumber * 10 + i + 1;
                if (recordNumber <= nrOfRecords)
                {
                    SingleValues[i].SetActive(true);
                    SingleValues[i].transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text =
                        PlayerPrefs.GetInt("SingleGame" + recordNumber + "height").ToString();
                    SingleValues[i].transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text =
                        PlayerPrefs.GetInt("SingleGame" + recordNumber + "width").ToString();
                    SingleValues[i].transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text =
                        PlayerPrefs.GetInt("SingleGame" + recordNumber + "depth").ToString();
                    SingleValues[i].transform.GetChild(3).gameObject.GetComponent<TMP_Text>().text =
                        PlayerPrefs.GetInt("SingleGame" + recordNumber + "seed").ToString();
                    SingleValues[i].transform.GetChild(4).gameObject.GetComponent<TMP_Text>().text =
                        PlayerPrefs.GetString("SingleGame" + recordNumber + "Time");
                    SingleValues[i].transform.GetChild(5).gameObject.GetComponent<TMP_Text>().text =
                        PlayerPrefs.GetString("SingleGame" + recordNumber + "Nickname");
                }
                else
                {
                    SingleValues[i].SetActive(false);
                }
            }
        }

        if (currentPanel == 2)
        {
            if (PageNumber >= PlayerPrefs.GetInt("MultiSaved")) PageNumber--;
            var recordNumber = PageNumber + 1;
            MultiMainValues.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text =
                PlayerPrefs.GetInt("MultiGame" + recordNumber + "height").ToString();
            MultiMainValues.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text =
                PlayerPrefs.GetInt("MultiGame" + recordNumber + "width").ToString();
            MultiMainValues.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text =
                PlayerPrefs.GetInt("MultiGame" + recordNumber + "depth").ToString();
            MultiMainValues.transform.GetChild(3).gameObject.GetComponent<TMP_Text>().text =
                PlayerPrefs.GetInt("MultiGame" + recordNumber + "seed").ToString();
            for (var i = 0; i < 10; i++)
                if (i < PlayerPrefs.GetInt("MultiGame" + recordNumber + "NrOfPlayers"))
                {
                    MultiValues[i].SetActive(true);
                    MultiValues[i].transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text =
                        PlayerPrefs.GetString("MultiGame" + recordNumber + "Player" + i + "Time");
                    MultiValues[i].transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text =
                        PlayerPrefs.GetString("MultiGame" + recordNumber + "Player" + i + "Nickname");
                }
                else
                {
                    MultiValues[i].SetActive(false);
                }
        }
    }
}