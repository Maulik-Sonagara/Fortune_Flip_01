using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Card System/Card Database")]
public class CardDatabase : ScriptableObject
{
    [Header("All Available Card Prefabs")]
    public List<CardData> cardPrefabs;  // each element is a prefab of a card (front+back)
}
