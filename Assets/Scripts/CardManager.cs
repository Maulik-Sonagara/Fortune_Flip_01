using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    [Header("References")]
    public CardDatabase cardDatabase;
    public Transform tableArea;
    public Transform playerHandArea;
    public Transform spawnArea;
    public Button playButton;

    [Header("Settings")]
    public float cardMoveSpeed = 1500f;
    public float delayBetweenCards = 0.15f;
    public float delayBeforeTableCards = 1f;

    [Header("Table Layout")]
    public int columns = 8;
    public float cardSpacing = 120f;
    public Vector2 tableStartOffset = new Vector2(60, -60);

    private List<CardData> currentDeck = new List<CardData>();
    private bool isAnimating = false;

    public static CardManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RewardCalculation.Instance.UpdateBalanceUI();
        InitializeDeck();
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
    }

    public void InitializeDeck()
    {
        currentDeck.Clear();
        foreach (CardData card in cardDatabase.cardPrefabs)
            currentDeck.Add(card);
        ShuffleDeck();
    }

    public void ShuffleDeck()
    {
        // -------------------------
        // RTP can be included
        // -------------------------

        for (int i = 0; i < currentDeck.Count; i++)
        {
            int rand = Random.Range(i, currentDeck.Count);
            (currentDeck[i], currentDeck[rand]) = (currentDeck[rand], currentDeck[i]);
        }
        Debug.Log("Deck shuffled with " + currentDeck.Count + " cards.");
    }

    public void StartGame()
    {
        // Prevent running while already animating
        if (isAnimating) return;

        float turboMultiplier = TurboModeController.Instance != null && TurboModeController.Instance.isTurboOn ? 2f : 1f;

        // Apply turbo speed
        cardMoveSpeed = 1500f * turboMultiplier;
        delayBetweenCards = 0.15f / turboMultiplier;
        delayBeforeTableCards = 1f / turboMultiplier;

        if (RewardCalculation.Instance.playerBalance >= RewardCalculation.Instance.baseBetAmount)
        {
            RewardCalculation.Instance.playerBalance -= RewardCalculation.Instance.baseBetAmount;
            RewardCalculation.Instance.UpdateBalanceUI();
        }
        else
        {
            Debug.LogWarning("Not enough balance to play!");
            return; // stop round start
        }

        GameManager.Instance.SetAnimating(true);
        GameManager.Instance.StartNewRound();


        // Disable Play button
        if (playButton != null)
            playButton.interactable = false;

        // Reset deck if needed
        if (currentDeck.Count < 52)
        {
            InitializeDeck();
        }

        // Clear previous cards
        ClearExistingCards();

        GameManager.Instance.ResetFlipChances();

        StartCoroutine(SpawnCardsSequence());
    }

    private IEnumerator SpawnCardsSequence()
    {
        isAnimating = true;

        int playerCardCount = GameManager.Instance.CardSelected;
        int tableCardCount = playerCardCount * 8; 

        yield return GiveCardsToPlayerAnimated(playerCardCount);
        yield return new WaitForSeconds(delayBeforeTableCards);
        yield return SpawnCardsOnTableAnimated(tableCardCount);

        // Collect data after animation finishes
        if (CardDetection.Instance != null)
            CardDetection.Instance.CollectCardData();

        // Finished arranging cards
        isAnimating = false;
    }

    private void ClearExistingCards()
    {
        foreach (Transform child in playerHandArea)
            Destroy(child.gameObject);

        foreach (Transform child in tableArea)
            Destroy(child.gameObject);

    }

    private IEnumerator GiveCardsToPlayerAnimated(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (currentDeck.Count == 0) yield break;

            CardData cardData = null;

            for (int j = 0; j < currentDeck.Count; j++)
            {
                if (!currentDeck[j].isJoker)
                {
                    cardData = currentDeck[j];
                    currentDeck.RemoveAt(j);
                    break;
                }
            }

            // Safety check for joker
            if (cardData == null)
            {
                Debug.LogWarning("No non-joker cards available for player hand!");
                yield break;
            }

            GameObject newCard = Instantiate(cardData.cardPrefab, spawnArea);
            AudioManager.Instance.PlayCardSpread();

            // getting carddata
            CardIdentifier id = newCard.GetComponent<CardIdentifier>() ?? newCard.AddComponent<CardIdentifier>();
            id.cardData = cardData;

            RectTransform rect = newCard.GetComponent<RectTransform>();
            rect.position = spawnArea.position;
            newCard.transform.SetParent(playerHandArea);

            CardFlip flip = newCard.GetComponent<CardFlip>();
            if (flip != null)
                flip.SetFaceUp(true); // show front

            yield return MoveCardToHandPosition(rect, i);
            yield return new WaitForSeconds(delayBetweenCards);
        }
    }

    private IEnumerator SpawnCardsOnTableAnimated(int count)
    {
        List<Button> cardButtons = new List<Button>();

        float turboMultiplier = TurboModeController.Instance != null && TurboModeController.Instance.isTurboOn ? 2f : 1f;

        cardMoveSpeed = 7000f * turboMultiplier;
        delayBetweenCards = (turboMultiplier > 1f) ? 0f : 0.1f;

        for (int i = 0; i < count; i++)
        {
            if (currentDeck.Count == 0) yield break;

            CardData cardData = currentDeck[0];
            currentDeck.RemoveAt(0);

            GameObject newCard = Instantiate(cardData.cardPrefab, spawnArea);
            AudioManager.Instance.PlayCardSpread();

            // getting carddata
            CardIdentifier id = newCard.GetComponent<CardIdentifier>() ?? newCard.AddComponent<CardIdentifier>();
            id.cardData = cardData;

            RectTransform rect = newCard.GetComponent<RectTransform>();
            rect.position = spawnArea.position;
            newCard.transform.SetParent(tableArea);

            CardFlip flip = newCard.GetComponent<CardFlip>();
            if (flip != null)
                flip.SetFaceUp(false); // face down at start

            yield return MoveCardToTablePosition(rect, i);

            // Add flip button but keep disabled until animation ends
            Button cardButton = newCard.GetComponent<Button>() ?? newCard.AddComponent<Button>();
            cardButton.transition = Selectable.Transition.None;
            cardButton.interactable = false;

            cardButtons.Add(cardButton);

            int capturedIndex = i;
            cardButton.onClick.AddListener(() =>
            {
                if (!isAnimating && GameManager.Instance.CanFlipCard())
                {
                    if (!flip.isFlipped) { 
                        flip.FlipCard();

                        // Reward check (after flip)
                        CardIdentifier id = newCard.GetComponent<CardIdentifier>();
                        if (id != null && id.cardData != null)
                        {
                            RewardCalculation rewardCalc = FindObjectOfType<RewardCalculation>();
                            if (rewardCalc != null)
                                rewardCalc.OnCardFlipped(id.cardData, flip);
                        }

                        GameManager.Instance.UseFlipChance();
                        flip.isFlipped = true;
                    }


                    //// Re-enable play button when chances are over
                    //if (!GameManager.Instance.CanFlipCard() && playButton != null)
                    //{
                    //    playButton.interactable = true;
                    //    GameManager.Instance.SetAnimating(false);

                    //}

                }
            });

            yield return new WaitForSeconds(delayBetweenCards);
        }

        // After all cards placed, enable flipping
        foreach (Button btn in cardButtons)
            btn.interactable = true;
        GameManager.Instance.SetAnimating(false);
    }

    private IEnumerator MoveCardToHandPosition(RectTransform card, int index)
    {
        int columns = 3;
        float xSpacing = 130f;
        float ySpacing = 120f;

        int row = index / columns;
        int col = index % columns;

        Vector3 targetPos = playerHandArea.position + new Vector3(col * xSpacing, -row * ySpacing, 0);

        while (Vector3.Distance(card.position, targetPos) > 1f)
        {
            card.position = Vector3.MoveTowards(card.position, targetPos, cardMoveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator MoveCardToTablePosition(RectTransform card, int index)
    {
        int row = index / columns;
        int col = index % columns;
        
        float cardSpacingX = 100f;
        float cardSpacingY = 130f;

        Vector3 targetPos = tableArea.position +
            new Vector3(col * cardSpacingX + tableStartOffset.x, -row * cardSpacingY + tableStartOffset.y, 0);

        while (Vector3.Distance(card.position, targetPos) > 1f)
        {
            card.position = Vector3.MoveTowards(card.position, targetPos, cardMoveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
