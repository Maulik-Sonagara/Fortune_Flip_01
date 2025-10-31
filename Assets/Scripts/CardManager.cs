using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    private float turboMultiplier = 1f;

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

    [Header("RTP Settings")]
    [Range(50f, 100f)]
    public float RTPPercentage = 95f;  // Adjustable in Inspector

    [Tooltip("Chance weight scaling for high payout cards")]
    public float highCardWeight = 0.2f;

    [Tooltip("Chance weight scaling for medium payout cards")]
    public float mediumCardWeight = 0.6f;

    [Tooltip("Chance weight scaling for low payout cards")]
    public float lowCardWeight = 1.0f;


    [Header("Table Layout")]
    public int columns = 7;
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

        if (TurboModeController.Instance != null)
            TurboModeController.Instance.OnTurboChanged += UpdateTurboSpeed;
    }

    private void UpdateTurboSpeed(bool isTurbo)
    {
        turboMultiplier = isTurbo ? 2f : 1f;
        Debug.Log("Turbo multiplier changed to " + turboMultiplier);
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

        for (int i = 0; i < currentDeck.Count; i++)
        {
            int rand = Random.Range(i, currentDeck.Count);
            (currentDeck[i], currentDeck[rand]) = (currentDeck[rand], currentDeck[i]);
        }
        Debug.Log("Deck shuffled with " + currentDeck.Count + " cards.");
    }

    public void BiasedShuffle()
    {
        System.Random rand = new System.Random();
        List<CardData> tempDeck = new List<CardData>(currentDeck);

        // Fisher–Yates with probability biasing on each swap
        for (int i = tempDeck.Count - 1; i > 0; i--)
        {
            // Bias affects how far ahead a card can move
            int biasRange = Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Lerp(1, i, RTPPercentage / 100f)),
                1, i
            );

            // Select a random index within a bias-weighted window
            int j = rand.Next(i - biasRange, i + 1);

            // Occasionally break bias to increase realism
            if (UnityEngine.Random.value < 0.25f)
                j = rand.Next(0, i + 1);

            // Swap cards
            (tempDeck[i], tempDeck[j]) = (tempDeck[j], tempDeck[i]);
        }

        currentDeck = tempDeck;
        Debug.Log($"Deck shuffled with realistic RTP bias ({RTPPercentage}%)");
    }



    private float GetCardBias(CardData card)
    {
        float rtpFactor = RTPPercentage / 100f;

        // Interpolate weights: low RTP → favor low payout cards
        switch (card.payoutGroup)
        {
            case PayoutGroup.High:
                return Mathf.Lerp(0.1f, highCardWeight, rtpFactor);
            case PayoutGroup.Medium:
                return Mathf.Lerp(0.5f, mediumCardWeight, rtpFactor);
            case PayoutGroup.Low:
            default:
                return Mathf.Lerp(1.0f, lowCardWeight, 1f - (rtpFactor - 0.5f));
        }
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
            RTPController.Instance.RegisterBet(RewardCalculation.Instance.baseBetAmount);

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
        CardDetection.Instance.CollectCardData();

        // Initialize RTP system after player hand known
        if (RTPController.Instance != null)
            RTPController.Instance.InitializeRTP(currentDeck);

        StartCoroutine(SpawnCardsSequence());
    }

    private IEnumerator SpawnCardsSequence()
    {
        isAnimating = true;

        int playerCardCount = GameManager.Instance.CardSelected;
        int tableCardCount = playerCardCount * 7; 

        yield return GiveCardsToPlayerAnimated(playerCardCount);
        yield return new WaitForSeconds(delayBeforeTableCards);
        //BiasedShuffle();

        // Collect player hand for RTP adjustment
        CardDetection.Instance.CollectCardData();

        // Initialize RTP probabilities
        if (RTPController.Instance != null)
            RTPController.Instance.InitializeRTP(currentDeck);


        BiasedShuffle(); // legacy fallback
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
            yield return new WaitForSeconds(delayBetweenCards / turboMultiplier);

        }
    }

    private IEnumerator SpawnCardsOnTableAnimated(int count)
    {
        List<Button> cardButtons = new List<Button>();

        float turboMultiplier = (TurboModeController.Instance != null && TurboModeController.Instance.isTurboOn) ? 2f : 1f;

        cardMoveSpeed = 7000f * turboMultiplier;
        delayBetweenCards = (turboMultiplier > 1f) ? 0f : 0.1f;

        for (int i = 0; i < count; i++)
        {
            if (currentDeck.Count == 0)
            {
                Debug.LogWarning("[CardManager] Deck empty during spawn.");
                yield break;
            }

            // --- Pick card using RTP system ---
            CardData cardData = null;
            if (RTPController.Instance != null)
                cardData = RTPController.Instance.GetNextCard();

            // Fallback (if RTP returns null)
            if (cardData == null)
                cardData = currentDeck[0];

            // Remove the card safely from deck (by reference or fallback)
            bool removed = currentDeck.Remove(cardData);
            if (!removed && currentDeck.Count > 0)
            {
                Debug.LogWarning($"[CardManager] Card not found in deck: {cardData.cardName}, using fallback index 0.");
                cardData = currentDeck[0];
                currentDeck.RemoveAt(0);
            }

            // --- Instantiate card on table ---
            GameObject newCard = Instantiate(cardData.cardPrefab, spawnArea);
            AudioManager.Instance.PlayCardSpread();

            // Attach CardIdentifier for reward logic
            CardIdentifier id = newCard.GetComponent<CardIdentifier>() ?? newCard.AddComponent<CardIdentifier>();
            id.cardData = cardData;

            RectTransform rect = newCard.GetComponent<RectTransform>();
            rect.position = spawnArea.position;
            newCard.transform.SetParent(tableArea);

            // Start with face down
            CardFlip flip = newCard.GetComponent<CardFlip>();
            if (flip != null)
                flip.SetFaceUp(false);

            yield return MoveCardToTablePosition(rect, i);

            // Add button (for flipping)
            Button cardButton = newCard.GetComponent<Button>() ?? newCard.AddComponent<Button>();
            cardButton.transition = Selectable.Transition.None;
            cardButton.interactable = false;
            cardButtons.Add(cardButton);

            // --- Flip logic ---
            cardButton.onClick.AddListener(() =>
            {
                if (!isAnimating && GameManager.Instance.CanFlipCard())
                {
                    if (!flip.isFlipped)
                    {
                        flip.FlipCard();

                        // Reward calculation
                        if (id != null && id.cardData != null)
                        {
                            RewardCalculation rewardCalc = FindObjectOfType<RewardCalculation>();
                            if (rewardCalc != null)
                                rewardCalc.OnCardFlipped(id.cardData, flip);
                        }

                        GameManager.Instance.UseFlipChance();
                        flip.isFlipped = true;
                    }
                }
            });

            // Delay between card spawns (faster if turbo)
            yield return new WaitForSeconds(delayBetweenCards / turboMultiplier);
        }

        // --- After all cards are spawned ---
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
            float speed = cardMoveSpeed * turboMultiplier;
            card.position = Vector3.MoveTowards(card.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (TurboModeController.Instance != null)
            TurboModeController.Instance.OnTurboChanged -= UpdateTurboSpeed;
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
            float speed = cardMoveSpeed * turboMultiplier;
            card.position = Vector3.MoveTowards(card.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
    }
}
