using System.Collections.Generic;
using UnityEngine;

public class CardDetection : MonoBehaviour
{
    public static CardDetection Instance;

    [Header("References")]
    public Transform playerHandArea;
    public Transform tableArea;

    [Header("Card Tracking")]
    public List<CardData> playerCards = new List<CardData>();
    public List<CardData> tableCards = new List<CardData>();

    private bool dataCollected = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void CollectCardData()
    {
        playerCards.Clear();
        tableCards.Clear();

        // Collect cards from Player Hand Area
        foreach (Transform card in playerHandArea)
        {
            CardData data = GetCardDataFromObject(card.gameObject);
            if (data != null)
                playerCards.Add(data);
        }

        // Collect cards from Table Area
        foreach (Transform card in tableArea)
        {
            CardData data = GetCardDataFromObject(card.gameObject);
            if (data != null)
                tableCards.Add(data);
        }

        dataCollected = true;
        Debug.Log($"CardDetection: Data Collected — Player({playerCards.Count}) | Table({tableCards.Count})");
    }

    private CardData GetCardDataFromObject(GameObject cardObject)
    {
        // Try to get the card data from a holder script or prefab reference
        CardIdentifier identifier = cardObject.GetComponent<CardIdentifier>();
        if (identifier != null)
            return identifier.cardData;

        return null;
    }

    public bool IsDataReady() => dataCollected;
}
