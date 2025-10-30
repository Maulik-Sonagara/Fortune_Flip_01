using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    void Awake() => Instance = this;

    [Header("Game State")]
    public int CardSelected;
    private int remainingFlips;

    [Header("UI References")]
    public TextMeshProUGUI flipCountText;
    public Button infoPanelBtn;

    private RewardCalculation rewardCalculation;
    private bool isAnimating = false;

    void Start()
    {
        CardSelected = CardSelectorUI.instance.selectedCards;
        remainingFlips = CardSelected;
        rewardCalculation = FindObjectOfType<RewardCalculation>();

        if (flipCountText != null)
            flipCountText.gameObject.SetActive(false);

        if (infoPanelBtn != null)
            infoPanelBtn.onClick.AddListener(() => InfoPanelManager.Instance.OpenInfoPanel());
    }

    public void SetAnimating(bool value)
    {
        isAnimating = value;

        if (flipCountText != null)
            flipCountText.gameObject.SetActive(!isAnimating);

        WinUIManager.Instance.setGoodLuck();

        if (!isAnimating)
            UpdateFlipText();
    }

    public bool CanFlipCard()
    {
        return remainingFlips > 0 && !isAnimating;
    }

    public void UseFlipChance()
    {
        if (remainingFlips > 0)
        {
            WinUIManager.Instance.offGoodLuck();
            remainingFlips--;
            UpdateFlipText();

            Debug.Log("Remaining flip chances: " + remainingFlips);

            // All flips done
            if (remainingFlips == 0)
            {
                UpdateFlipText();

                Debug.Log("All flips completed. Calculating rewards...");
                if (rewardCalculation != null)
                {
                    rewardCalculation.CalculateRewards();
                    // ❌ No reset here — wait until next play
                }
                else
                {
                    Debug.LogWarning("RewardCalculation not found!");
                }
            }
        }
    }

    // 🔹 Call this from Play button (at start of new round)
    public void StartNewRound()
    {
        Debug.Log("🔁 Starting new round... Resetting rewards and flips.");

        // ✅ Clear previous cycle and hits
        rewardCalculation.ResetRewards();

        // ✅ Reset flip chances for new selection
        CardSelected = CardSelectorUI.instance.selectedCards;
        remainingFlips = CardSelected;
        UpdateFlipText();

        if (flipCountText != null)
            flipCountText.gameObject.SetActive(false);

        // Optionally clear UI text for clean start
        WinUIManager.Instance.rewardText.text = "";
        WinUIManager.Instance.rewardAmountText.text = "";
    }

    public void AddExtraFlips(int amount)
    {
        remainingFlips += amount;
        Debug.Log($"+{amount} bonus flips! New total flips: {remainingFlips}");

        UpdateFlipText();

        if (flipCountText != null && !isAnimating)
            flipCountText.gameObject.SetActive(true);
    }

    public void ResetFlipChances()
    { 
        remainingFlips = CardSelected; 
        UpdateFlipText(); 
        if (flipCountText != null) flipCountText.gameObject.SetActive(false); 
    }

    private void UpdateFlipText()
    {
        if (flipCountText == null) return;

        if (remainingFlips > 0)
            flipCountText.text = $"Flips Left: {remainingFlips}";
        else
            flipCountText.text = "Flips Over";

        flipCountText.gameObject.SetActive(true);
    }
}
