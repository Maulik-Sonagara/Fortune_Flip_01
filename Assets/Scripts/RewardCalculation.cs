using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardCalculation : MonoBehaviour
{
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

    // 🔹 New control flag
    private bool isCycleActive = false;

    // -------------------- CARD FLIP --------------------
    public void OnCardFlipped(CardData flippedCard)
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

        // Match by rank only
        bool isMatch = false;
        foreach (CardData playerCard in CardDetection.Instance.playerCards)
        {
            if (playerCard != null && playerCard.rank == flippedCard.rank)
            {
                isMatch = true;
                break;
            }
        }

        if (isMatch)
        {
            AudioManager.Instance.PlayWin();
            float multiplier = GetMultiplier(flippedCard.payoutGroup);
            float reward = baseBetAmount * multiplier;
            totalReward += reward;

            string msg = $"HIT! {flippedCard.rank}";
            string amt = $"$ {reward:F2}";

            Debug.Log($"{msg} ({flippedCard.payoutGroup}) → {amt}");

            WinUIManager.Instance.rewardAmountText.text = amt;
            WinUIManager.Instance.rewardText.text = msg;

            hitMessages.Add(msg);
            hitAmounts.Add(amt);
        }
        else
        {
            // Missed flip
            AudioManager.Instance.PlayLose();
            Debug.Log($"MISS: {flippedCard.rank} ({flippedCard.payoutGroup}) → +$0.00");
            WinUIManager.Instance.rewardAmountText.text = "MISS";
            WinUIManager.Instance.rewardText.text = "No Match!";
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

        StopCycle();
        isCycleActive = true; // Allow cycle to start
        cycleCoroutine = StartCoroutine(DelayedShowTotalAndStartCycle());
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
}
