using UnityEngine;
using UnityEngine.UI;

public class InfoPanelManager : MonoBehaviour
{
    [Header("References")]
    public GameObject InfoCanvas;
    public GameObject InfoPanel;
    public GameObject GameInfoPanel;
    public GameObject CardsPanel;
    public GameObject JokerPanel;

    [Header("Buttons")]
    public Button rightBtn;
    public Button leftBtn;
    public Button closeBtn;

    [Header("Variables")]
    private int panelIndex = 1;  // 1 = GameInfo, 2 = Cards, 3 = Joker
    private bool isPanelOpen = false;

    public static InfoPanelManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Attach button listeners
        closeBtn.onClick.AddListener(CloseInfoPanel);
        rightBtn.onClick.AddListener(NextPanel);
        leftBtn.onClick.AddListener(PreviousPanel);

        // Initial state
        InfoCanvas.SetActive(false);
        InfoPanel.SetActive(true);
        
        ShowPanel(1);
    }

    void Update()
    {
        // Manage arrow visibility
        leftBtn.gameObject.SetActive(panelIndex > 1);
        rightBtn.gameObject.SetActive(panelIndex < 3);
    }

    public void OpenInfoPanel()
    {
        isPanelOpen = true;
        InfoCanvas.SetActive(true);
        panelIndex = 1;
        ShowPanel(panelIndex);

        Debug.Log("Info Panel Opened");
    }

    public void CloseInfoPanel()
    {
        isPanelOpen = false;
        InfoCanvas.SetActive(false);
        Debug.Log("Info Panel Closed");
    }

    private void NextPanel()
    {
        panelIndex++;
        if (panelIndex > 3) panelIndex = 3;
        ShowPanel(panelIndex);
    }

    private void PreviousPanel()
    {
        panelIndex--;
        if (panelIndex < 1) panelIndex = 1;
        ShowPanel(panelIndex);
    }

    private void ShowPanel(int index)
    {
        GameInfoPanel.SetActive(index == 1);
        CardsPanel.SetActive(index == 2);
        JokerPanel.SetActive(index == 3);
    }
}
