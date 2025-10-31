using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class RTPController : MonoBehaviour
{
    public static RTPController Instance;

    [Header("RTP Settings")]
    [Range(0f, 200f)]
    public float targetRTP = 70f;  

    [Header(" Runtime Data")]
    public float totalFlips;
    public float totalHits;
    public float actualHitRate;

    [Space(10)]
    [Header(" RTP Tracking ")]
    public float totalBetAmount;
    public float totalEarnedAmount;
    public float currentRTP; // currentRTP = (earned / bet) * 100

    public List<CardData> allCards = new List<CardData>();
    public List<CardRank> playerRanks = new List<CardRank>();
    public System.Random rng = new System.Random();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void InitializeRTP(List<CardData> deck)
    {
        allCards = new List<CardData>(deck);
        playerRanks.Clear();

        if (CardDetection.Instance != null && CardDetection.Instance.IsDataReady())
        {
            playerRanks = CardDetection.Instance.playerCards
                .Where(c => c != null)
                .Select(c => c.rank)
                .Distinct()
                .ToList();
        }

        totalFlips = 0;
        totalHits = 0;
        actualHitRate = 0f;
    }

    public CardData GetNextCard()
    {
        if (allCards.Count == 0)
            return null;

        float roll = Random.Range(0f, 100f);
        bool shouldGiveMatch = roll < targetRTP;
        CardData selectedCard = null;

        // --- Favor matching ranks if RTP roll allows ---
        if (shouldGiveMatch && playerRanks.Count > 0)
        {
            var matchingCards = allCards.Where(c => playerRanks.Contains(c.rank)).ToList();
            if (matchingCards.Count > 0)
                selectedCard = matchingCards[rng.Next(matchingCards.Count)];
        }

        // --- Otherwise pick a non-matching card ---
        if (selectedCard == null)
        {
            var nonMatchingCards = allCards.Where(c => !playerRanks.Contains(c.rank)).ToList();
            if (nonMatchingCards.Count > 0)
                selectedCard = nonMatchingCards[rng.Next(nonMatchingCards.Count)];
            else
                selectedCard = allCards[rng.Next(allCards.Count)];
        }

        allCards.Remove(selectedCard);

        totalFlips++;
        if (playerRanks.Contains(selectedCard.rank))
            totalHits++;

        actualHitRate = totalFlips > 0 ? (totalHits / totalFlips) * 100f : 0f;

        return selectedCard;
    }

    // Add when player places a bet
    public void RegisterBet(float amount)
    {
        totalBetAmount += amount;
        UpdateRTP();
    }

    // Add when player wins or earns amount
    public void RegisterWin(float amount)
    {
        totalEarnedAmount += amount;
        UpdateRTP();
    }

    private void UpdateRTP()
    {
        if (totalBetAmount > 0)
            currentRTP = (totalEarnedAmount / totalBetAmount) * 100f;
        else
            currentRTP = 0f;
    }

    public float GetActualHitRate() => actualHitRate;
}
