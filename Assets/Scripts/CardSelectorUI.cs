using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardSelectorUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text selectedCardText;
    public Button plusButton;
    public Button minusButton;
    public Button playButton;

    [Header("Settings")]
    public int minCards = 2;
    public int maxCards = 5;

    [HideInInspector] public int selectedCards = 2;

    public static CardSelectorUI instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UpdateDisplay();

        plusButton.onClick.AddListener(OnPlus);
        minusButton.onClick.AddListener(OnMinus);
        playButton.onClick.AddListener(() => FindObjectOfType<CardManager>().StartGame());

    }

    private void OnPlus()
    {
        if (selectedCards < maxCards)
        {
            selectedCards++;
            UpdateDisplay();
        }
    }

    private void OnMinus()
    {
        if (selectedCards > minCards)
        {
            selectedCards--;
            UpdateDisplay();
        }
    }

    private void OnPlay()
    {
        GameManager.Instance.CardSelected = selectedCards;
        
    }

    private void UpdateDisplay()
    {
        selectedCardText.text = selectedCards.ToString();
        if (GameManager.Instance != null)
            GameManager.Instance.CardSelected = selectedCards;
    }
}
