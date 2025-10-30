using UnityEngine;

public enum CardSuit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades,
    Joker
}

public enum CardRank
{
    Ace = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Joker = 14
}

// Add new enum for payout tiers
public enum PayoutGroup
{
    Low,
    Medium,
    High,
    Special, // for Joker or unique cards
    Joker
}

[CreateAssetMenu(fileName = "NewCardData", menuName = "Card System/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    public string cardName;            // Example: "Ace of Spades"
    public CardSuit suit;
    public CardRank rank;

    [Header("Gameplay Settings")]
    public int cardValue;              // base numeric value for logic
    public PayoutGroup payoutGroup;    // Low / Medium / High / Special
    public GameObject cardPrefab;      // prefab reference

    [Header("Card Type")]
    public bool isJoker = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Automatically assign payout group based on rank
        switch (rank)
        {
            case CardRank.Two:
            case CardRank.Three:
            case CardRank.Four:
            case CardRank.Five:
            case CardRank.Six:
                payoutGroup = PayoutGroup.Low;
                break;

            case CardRank.Seven:
            case CardRank.Eight:
            case CardRank.Nine:
            case CardRank.Ten:
                payoutGroup = PayoutGroup.Medium;
                break;

            case CardRank.Jack:
            case CardRank.Queen:
            case CardRank.King:
            case CardRank.Ace:
                payoutGroup = PayoutGroup.High;
                break;

            case CardRank.Joker:
                payoutGroup = PayoutGroup.Joker;
                isJoker = true;
                break;
        }
    }
#endif
}
