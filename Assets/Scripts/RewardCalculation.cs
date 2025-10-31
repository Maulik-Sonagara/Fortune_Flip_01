using Bilions.Foundation.Bet;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardCalculation : MonoBehaviour
{
    [Header("Player Balance")]
    [System.NonSerialized] public float playerBalance;
    public TextMeshProUGUI balanceText;

    [Header("Settings")]
    public float baseBetAmount = 1f;

    [Header("Cycle Settings")]
    public float delayBeforeTotal = 2f;          // Wait before showing total
    public float cycleSpeed = 0.5f;              // Duration per entry
    public float delayAfterTotalBeforeCycle = 1f;// Wait after showing total before starting cycle

    private float totalReward = 0f;

    private List<string> hitMessages = new List<string>();
    private List<string> hitAmounts = new List<string>();
    private Coroutine cycleCoroutine;

    public static RewardCalculation Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        playerBalance = PlayerPrefs.GetFloat("balance", 5000f);
        Debug.Log($"Loaded balance from prefs: {playerBalance}");
    }

    private void Start()
    {
        UpdateBalanceUI();
    }

    // New control flag
    private bool isCycleActive = false;

    private void OnEnable() // auto calls before creation
    {
        BetServiceBehaviour.Instance.OnBetChanged += setbetvalue;
    }

    private void setbetvalue(float obj)
    {
        baseBetAmount = obj;
    }

    private void OnDisable() // auto calls before destroy
    {
        BetServiceBehaviour.Instance.OnBetChanged -= setbetvalue;
    }


    // -------------------- GAME BALANCE --------------------

    public void UpdateBalanceUI()
    {
        if (balanceText != null)
        {
            balanceText.text = $"{playerBalance:0.00}";
        }

        // Save immediately when updated
        PlayerPrefs.SetFloat("balance", playerBalance);
        PlayerPrefs.Save();

        Debug.Log($"Balance saved: {playerBalance}");
    }


    // -------------------- CARD FLIP --------------------
    public void OnCardFlipped(CardData flippedCard, CardFlip flip)
    {
        if (flippedCard == null) return;

        if (CardDetection.Instance == null || !CardDetection.Instance.IsDataReady())
        {
            Debug.LogWarning("CardDetection data not ready yet!");
            return;
        }

        // Handle Joker first
        if (flippedCard.isJoker)
        {
            AudioManager.Instance.PlayJoker();
            Debug.Log("JOKER flipped! +2 extra flips");

            flip.isHitCard = true;
            GameManager.Instance.AddExtraFlips(2);
            totalReward += baseBetAmount;

            string msg = "JOKER! +2 Flips";
            string amt = $"$ {baseBetAmount:F2}";

            WinUIManager.Instance.rewardAmountText.text = amt;
            WinUIManager.Instance.rewardText.text = msg;

            hitMessages.Add(msg);
            hitAmounts.Add(amt);
            return;
        }

        // Count matches by rank in player hand
        int matchCount = 0;
        foreach (CardData playerCard in CardDetection.Instance.playerCards)
        {
            if (playerCard != null && playerCard.rank == flippedCard.rank)
            {
                matchCount++;
            }
        }

        if (matchCount > 0)
        {
            flip.isHitCard = true;
            AudioManager.Instance.PlayWin();

            // Reward multiplied by how many same rank cards in hand
            float multiplier = GetMultiplier(flippedCard.payoutGroup);
            float rewardPerMatch = baseBetAmount * multiplier;
            float totalCardReward = rewardPerMatch * matchCount;
            totalReward += totalCardReward;

            string suitName = flippedCard.suit.ToString();
            string msg;

            if (matchCount > 1)
                msg = $"HIT! {flippedCard.rank} of {suitName} ×{matchCount} Matches";
            else
                msg = $"HIT! {flippedCard.rank} of {suitName} ×{matchCount}";

            string amt = $"${rewardPerMatch:F2} × {matchCount} = ${totalCardReward:F2}";


            Debug.Log($"{msg} → {matchCount} match(es) → {amt}");

            WinUIManager.Instance.rewardText.text = msg;
            WinUIManager.Instance.rewardAmountText.text = amt;

            hitMessages.Add(msg);
            hitAmounts.Add(amt);
        }
        else
        {
            // Missed flip
            AudioManager.Instance.PlayLose();
            Debug.Log($"MISS: {flippedCard.rank} ({flippedCard.payoutGroup}) → +$0.00");

            WinUIManager.Instance.rewardText.text = "No Match!";
            WinUIManager.Instance.rewardAmountText.text = "MISS";
        }
    }


    private float GetMultiplier(PayoutGroup group)
    {
        switch (group)
        {
            case PayoutGroup.Low: return 0.5f;
            case PayoutGroup.Medium: return 0.75f;
            case PayoutGroup.High: return 1.25f;
            default: return 0f;
        }
    }

    // -------------------- CALCULATION --------------------
    public void CalculateRewards()
    {
        Debug.Log($"All flips done → Total Reward: ${totalReward:F2}");
        AddRewardToBalance(totalReward);
        StopCycle();
        isCycleActive = true; // Allow cycle to start
        cycleCoroutine = StartCoroutine(DelayedShowTotalAndStartCycle());
    }

    public void AddRewardToBalance(float reward)
    {
        playerBalance += reward;
        PlayerPrefs.SetFloat("balance", playerBalance);
        UpdateBalanceUI();

        // Show reward in Win UI
        if (WinUIManager.Instance != null)
        {
            WinUIManager.Instance.rewardText.text = "Round Reward";
            WinUIManager.Instance.rewardAmountText.text = $"+${reward:0.00}";
        }

        Debug.Log($"Reward added: ${reward:0.00}. New balance: ${playerBalance:0.00}");
    }


    public void StopCycle()
    {
        if (cycleCoroutine != null)
        {
            StopCoroutine(cycleCoroutine);
            cycleCoroutine = null;
        }

        isCycleActive = false; // Stop all future cycles
    }

    public void ResetRewards()
    {
        StopCycle(); // stops coroutine + disables cycle
        totalReward = 0f;
        hitMessages.Clear();
        hitAmounts.Clear();

        WinUIManager.Instance.rewardText.text = "";
        WinUIManager.Instance.rewardAmountText.text = "";

        Debug.Log("Rewards reset and cycle disabled.");
    }

    // -------------------- COROUTINE --------------------
    private IEnumerator DelayedShowTotalAndStartCycle()
    {
        // Step 1: Wait before showing total
        yield return new WaitForSeconds(delayBeforeTotal);

        if (!isCycleActive) yield break; // stop if disabled before showing total

        WinUIManager.Instance.rewardAmountText.text = $"$ {totalReward:F2}";
        WinUIManager.Instance.rewardText.text = "Total Reward!";

        // Step 2: Wait before starting the UI cycle
        yield return new WaitForSeconds(delayAfterTotalBeforeCycle);

        if (!isCycleActive) yield break;

        // Step 3: Build combined list
        List<string> combinedTexts = new List<string>(hitMessages);
        List<string> combinedAmounts = new List<string>(hitAmounts);

        combinedTexts.Add("Total Reward!");
        combinedAmounts.Add($"$ {totalReward:F2}");

        // Step 4: Start infinite cycle only if active
        int index = 0;
        while (isCycleActive)
        {
            if (combinedTexts.Count == 0) yield break;

            WinUIManager.Instance.rewardText.text = combinedTexts[index];
            WinUIManager.Instance.rewardAmountText.text = combinedAmounts[index];

            //Debug.Log($"Cycle display: {combinedTexts[index]} -> {combinedAmounts[index]}");

            index = (index + 1) % combinedTexts.Count;
            yield return new WaitForSeconds(cycleSpeed);
        }

        Debug.Log("Cycle stopped gracefully.");
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetFloat("balance", playerBalance);
        PlayerPrefs.Save();
    }

}