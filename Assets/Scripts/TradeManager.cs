using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json; // Requires package installation via Unity Package Manager
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections; // Added for Coroutine

// Enum defining the possible states of a trade negotiation
public enum TradeState
{
    None,            // No trade active
    OfferMade,       // Player (Human or AI) is making/has made an offer
    CounterReceived, // Player (Human or AI) received a counter from the trader
    Haggling,        // Human player is in the haggling minigame
    Completed,       // Trade finished successfully
    Rejected         // Trade ended without agreement
}


public class TradeManager : MonoBehaviour
{
    public static TradeManager Instance;

    // --- References ---
    [Header("UI References")]
    [Tooltip("The main parent window for all trade UI elements")]
    [SerializeField] private GameObject tradeWindow;
    [Tooltip("UI for the player to create/edit their offer")]
    [SerializeField] private TradeOfferDisplay createOfferDisplay;
    [Tooltip("UI to display the trader's counter offer")]
    [SerializeField] private TradeOfferDisplay counterOfferDisplay;
    [Tooltip("UI for the haggling minigame (human only)")]
    [SerializeField] private HaggleDisplay haggleDisplay;
    [Tooltip("UI to display the final result of the trade")]
    [SerializeField] private TradeOfferDisplay tradeResultDisplay;

    // --- State ---
    private TradeState currentState = TradeState.None;
    private Trader currentTrader;
    private TradeOffer currentOffer; // Represents the offer currently on the table
    private Player player;
    private AIBrain aiBrainInstance;
    private bool isAIPlayer = false;
    private int negotiationRounds = 0;
    private const int MAX_NEGOTIATION_ROUNDS = 6;
    private Coroutine autoCloseCoroutine; // To manage the auto-close timer

    void Awake()
    {
        // Standard Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        player = GameManager.Instance?.GetPlayerReference();
        if (player == null)
        {
            Debug.LogError("[TradeManager] Player reference not found in Start! Trading may fail.");
        }
        CleanTradeUI(); // Ensure UI is hidden initially
    }

    // --- Trade Initiation ---
    public void StartTrade(Trader trader)
    {
        if (player == null) player = GameManager.Instance?.GetPlayerReference();
        if (player == null || trader == null)
        {
            Debug.LogError($"[TradeManager] Cannot start trade. Player or Trader is null. Player: {player}, Trader: {trader}");
            return;
        }

        // Stop any previous auto-close attempts
        StopAutoCloseTimer();

        currentTrader = trader;
        negotiationRounds = 0;
        isAIPlayer = GameManager.Instance.IsAIPlayer();
        aiBrainInstance = isAIPlayer ? GameManager.Instance.GetAIBrainReference() : null;

        CleanTradeUI();

        if (isAIPlayer)
        {
            if (aiBrainInstance != null)
            {
                Debug.Log($"[TradeManager] AI Player trading with {currentTrader.traderType} trader. Requesting initial offer...");
                currentState = TradeState.OfferMade;
                _ = InitiateAITradeAsync(); // Fire-and-forget async task
            }
            else
            {
                Debug.LogError("[TradeManager] AI Player detected, but AIBrain reference not found! Cannot initiate AI trade.");
                currentState = TradeState.None;
            }
        }
        else // Human Player
        {
            Debug.Log($"[TradeManager] Human Player trading with {currentTrader.traderType} trader.");
            currentState = TradeState.OfferMade;
            currentOffer = new TradeOffer(0, 0, 0);

            createOfferDisplay.Initialize(currentOffer, currentTrader);
            createOfferDisplay.gameObject.SetActive(true);
            tradeWindow.SetActive(true);
        }
    }

    // --- AI Initial Offer ---
    private async Task InitiateAITradeAsync()
    {
        if (aiBrainInstance == null || player == null || currentTrader == null)
        {
            Debug.LogError("[TradeManager] InitiateAITradeAsync called with null references. Aborting trade.");
            RejectTrade();
            return;
        }
        try
        {
            string aiAction = await aiBrainInstance.GetTradeDecisionAsync(player, currentTrader, null);
            await ProcessAIActionAsync(aiAction);
        }
        catch (System.Exception e)
        {
             Debug.LogError($"[TradeManager] Exception during AI initial offer fetch: {e.Message}\n{e.StackTrace}");
             RejectTrade();
        }
    }

    // --- Human Submits Initial Offer (Button Callback) ---
    public void SubmitOffer()
    {
        if (isAIPlayer || currentState != TradeState.OfferMade)
        {
            Debug.LogWarning($"[TradeManager] SubmitOffer called inappropriately. isAI:{isAIPlayer}, State:{currentState}");
            return;
        }
        if (currentOffer == null)
        {
             Debug.LogError("[TradeManager] SubmitOffer called but currentOffer is null.");
             return;
        }

        if (currentOffer.GetPlayerValue() == 0 && currentOffer.GetTraderValue() == 0)
        {
            Debug.LogWarning("[TradeManager] Human submitted empty offer.");
            // TODO: Show UI feedback to human
            return;
        }
        if (currentOffer.goldToTrader > player.gold)
        {
             Debug.LogWarning("[TradeManager] Human submitted offer with insufficient gold.");
             // TODO: Show UI feedback to human
             return;
        }

        Debug.Log($"[TradeManager] Human submitting offer: {currentOffer}");
        _ = SubmitOfferToTraderAsync(currentOffer);
    }


    // --- Core Logic: Submit Offer to Trader & Handle Response ---
    private async Task SubmitOfferToTraderAsync(TradeOffer offerToSubmit)
    {
        if (currentTrader == null || offerToSubmit == null || player == null)
        {
            Debug.LogError($"[TradeManager] SubmitOfferToTraderAsync: Null references. Trader:{currentTrader}, Offer:{offerToSubmit}, Player:{player}");
            RejectTrade(); return;
        }

        negotiationRounds++;
        Debug.Log($"[TradeManager] Submitting Offer - Round: {negotiationRounds}");
        if (negotiationRounds > MAX_NEGOTIATION_ROUNDS)
        {
            Debug.LogWarning("[TradeManager] Max negotiation rounds reached. Rejecting trade.");
            RejectTrade(); return;
        }

        currentOffer = offerToSubmit;
        bool accepted = currentTrader.EvaluateTrade(currentOffer);

        if (accepted)
        {
            Debug.Log($"[TradeManager] Trader ACCEPTS offer: {currentOffer}");
            if (isAIPlayer)
            {
                FinalizeAITrade();
            }
            else
            {
                await StartHagglingAsync();
            }
        }
        else // Trader Rejects / Counters
        {
            Debug.Log($"[TradeManager] Trader REJECTS offer: {currentOffer}. Checking for counter...");
            TradeOffer counterOffer = currentTrader.CreateCounterOffer(currentOffer);

            if (counterOffer != null && !counterOffer.Equals(currentOffer))
            {
                Debug.Log($"[TradeManager] Trader COUNTERS with: {counterOffer}");
                currentOffer = counterOffer;
                currentState = TradeState.CounterReceived;

                if (isAIPlayer)
                {
                    Debug.Log("[TradeManager] Requesting AI response to counter offer...");
                    if (aiBrainInstance != null)
                    {
                        try
                        {
                            string aiAction = await aiBrainInstance.GetTradeDecisionAsync(player, currentTrader, currentOffer);
                            await ProcessAIActionAsync(aiAction);
                        }
                        catch (System.Exception e)
                        {
                             Debug.LogError($"[TradeManager] Exception fetching AI response to counter: {e.Message}\n{e.StackTrace}");
                             RejectTrade();
                        }
                    }
                    else { Debug.LogError("[TradeManager] AI Brain lost during counter! Rejecting."); RejectTrade(); }
                }
                else // Human player receives counter
                {
                    counterOfferDisplay.Initialize(currentOffer, currentTrader);
                    createOfferDisplay.gameObject.SetActive(false);
                    counterOfferDisplay.gameObject.SetActive(true);
                    tradeWindow.SetActive(true);
                }
            }
            else
            {
                Debug.LogWarning("[TradeManager] Trader could not make a valid counter offer (or offer was unchanged). Ending negotiation.");
                RejectTrade();
            }
        }
    }

    // --- Process AI's Decision String ---
    private async Task ProcessAIActionAsync(string aiAction)
    {
        if (player == null || currentTrader == null) {
             Debug.LogError("[TradeManager] ProcessAIActionAsync: Null player or trader. Rejecting.");
             RejectTrade(); return;
        }

        Debug.Log($"[TradeManager] Processing AI Action: '{aiAction}'");
        if (string.IsNullOrEmpty(aiAction)) { Debug.LogError("[TradeManager] Received null/empty AI action. Rejecting."); RejectTrade(); return; }

        string upperAction = aiAction.ToUpperInvariant().Trim();

        if (upperAction == "ACCEPT")
        {
            if (currentOffer == null) { Debug.LogError("[TradeManager] AI ACCEPT failed: currentOffer is null. Rejecting."); RejectTrade(); return; }
            if (currentOffer.goldToTrader <= player.gold)
            {
                Debug.Log($"[TradeManager] AI accepts offer: {currentOffer}. Finalizing.");
                FinalizeAITrade();
            }
            else { Debug.LogWarning($"[TradeManager] AI ACCEPT failed: Insufficient gold ({player.gold}/{currentOffer.goldToTrader}). Rejecting."); RejectTrade(); }
        }
        else if (upperAction == "REJECT")
        {
            Debug.Log("[TradeManager] AI rejects offer. Ending negotiation.");
            RejectTrade();
        }
        else if (upperAction.StartsWith("COUNTER OFFER"))
        {
            Debug.Log("[TradeManager] AI proposes counter offer.");
            try
            {
                Match match = Regex.Match(aiAction, @"{.*}", RegexOptions.Singleline);
                if (!match.Success) throw new JsonException("No JSON object found in COUNTER OFFER string.");

                string jsonPart = match.Value;
                TradeOffer aiCounterOffer = JsonConvert.DeserializeObject<TradeOffer>(jsonPart);

                if (aiCounterOffer == null) throw new JsonException("Deserialized counter offer is null.");

                aiCounterOffer.goldToPlayer = Mathf.Max(0, aiCounterOffer.goldToPlayer);
                aiCounterOffer.foodToTrader = Mathf.Max(0, aiCounterOffer.foodToTrader);
                aiCounterOffer.waterToTrader = Mathf.Max(0, aiCounterOffer.waterToTrader);
                aiCounterOffer.foodToPlayer = Mathf.Max(0, aiCounterOffer.foodToPlayer);
                aiCounterOffer.waterToPlayer = Mathf.Max(0, aiCounterOffer.waterToPlayer);
                aiCounterOffer.goldToTrader = Mathf.Max(0, aiCounterOffer.goldToTrader);

                Debug.Log($"[TradeManager] AI deserialized counter: {aiCounterOffer}");

                if (ValidateAICounter(aiCounterOffer))
                {
                    Debug.Log($"[TradeManager] AI counter validated. Submitting to trader...");
                    await SubmitOfferToTraderAsync(aiCounterOffer);
                }
                else
                {
                    Debug.LogWarning("[TradeManager] AI proposed invalid counter offer (failed validation). Rejecting.");
                    RejectTrade();
                }
            }
            catch (JsonReaderException e) { Debug.LogError($"[TradeManager] JSON Error processing AI counter: {e.Message}\nString: {aiAction}\nRejecting."); RejectTrade(); }
            catch (System.Exception e) { Debug.LogError($"[TradeManager] Error processing AI counter: {e.GetType().Name} - {e.Message}\nRejecting."); RejectTrade(); }
        }
        else
        {
            Debug.LogWarning($"[TradeManager] Unknown action string from AI: '{aiAction}'. Rejecting.");
            RejectTrade();
        }
    }


    // --- Human UI Interaction Handlers ---
    public async void AcceptCounterOffer()
    {
        if (isAIPlayer || currentState != TradeState.CounterReceived) return;
        if (currentOffer == null) { Debug.LogError("[TradeManager] AcceptCounterOffer: currentOffer is null!"); RejectTrade(); return; }

        if (currentOffer.goldToTrader <= player.gold)
        {
            Debug.Log($"[TradeManager] Human accepts trader's counter: {currentOffer}. Starting haggle.");
            await StartHagglingAsync();
        }
        else { Debug.LogWarning("[TradeManager] Human cannot afford trader's counter. Cannot accept."); /* TODO: Show UI feedback */ }
    }

    public void RejectTrade()
    {
        Debug.Log("[TradeManager] Trade rejected or cancelled.");
        currentState = TradeState.Rejected;
        CloseTradeWindow();
    }

    public void MakeCounterOffer()
    {
        if (isAIPlayer || currentState != TradeState.CounterReceived) return;
        if (currentOffer == null) { Debug.LogError("[TradeManager] MakeCounterOffer: currentOffer is null!"); return; }

        Debug.Log("[TradeManager] Human making counter offer.");
        currentState = TradeState.OfferMade;

        createOfferDisplay.Initialize(currentOffer, currentTrader);
        createOfferDisplay.gameObject.SetActive(true);
        counterOfferDisplay.gameObject.SetActive(false);
        tradeWindow.SetActive(true);
    }

    // --- Haggling (Human Only) ---
    private async Task StartHagglingAsync()
    {
        if (isAIPlayer || haggleDisplay == null || currentTrader == null || currentOffer == null || currentState == TradeState.Haggling) return;

        currentState = TradeState.Haggling;
        createOfferDisplay.gameObject.SetActive(false);
        counterOfferDisplay.gameObject.SetActive(false);
        haggleDisplay.gameObject.SetActive(true);
        tradeWindow.SetActive(true);

        try
        {
            int haggleDiscount = await haggleDisplay.StartHaggling(currentTrader, currentOffer);
            ApplyHaggleResult(haggleDiscount);
        }
        catch (System.Exception e)
        {
             Debug.LogError($"[TradeManager] Haggling task failed or was cancelled: {e.Message}");
             EndTrade(false);
        }
    }

    private void ApplyHaggleResult(int haggleDiscount)
    {
        if (isAIPlayer || currentOffer == null || currentState != TradeState.Haggling) return;

        Debug.Log($"[TradeManager] Applying haggle result: Discount = {haggleDiscount}");
        currentOffer.goldToTrader = Mathf.Max(0, currentOffer.goldToTrader - haggleDiscount);
        StatEffectManager.Instance?.ReflectTradeOffer(currentOffer);

        EndTrade(true);
    }

    // --- Trade Finalization ---
    private void FinalizeAITrade()
    {
         Debug.Log("[TradeManager] Finalizing AI trade.");
         EndTrade(true);
    }

    public void EndTrade(bool success)
    {
        if (currentState == TradeState.Completed || currentState == TradeState.Rejected || currentOffer == null || currentTrader == null || player == null)
        {
             if (currentState == TradeState.Completed || currentState == TradeState.Rejected) CloseTradeWindow();
             return;
        }

        Debug.Log($"[TradeManager] Ending Trade. Success: {success}. Final Offer: {currentOffer}");

        if (success)
        {
            currentTrader.ModifyStock(currentOffer);
            player.ApplyTrade(currentOffer);
            GameManager.Instance?.UpdateDisplay();
            currentState = TradeState.Completed;
        }
        else
        {
             currentState = TradeState.Rejected;
        }

        StatEffectManager.Instance?.ClearEffect("all");

        if (tradeResultDisplay != null)
        {
            tradeResultDisplay.Initialize(currentOffer, currentTrader);
            tradeResultDisplay.gameObject.SetActive(true);
        }
        CleanTradeUIExceptResult();
        tradeWindow?.SetActive(true);

        // Auto-close for AI after a delay
        if (isAIPlayer && success) // Only auto-close successful AI trades
        {
            StartAutoCloseTimer(2.0f); // Start 2-second timer
        }
        // Human player relies on a button in tradeResultDisplay to call CloseTradeWindow()
    }

    // --- Utility Methods ---
    public void CloseTradeWindow()
    {
        StopAutoCloseTimer(); // Ensure timer is stopped if closed manually

        Debug.Log("[TradeManager] Closing trade window.");
        currentState = TradeState.None;
        currentTrader = null;
        currentOffer = null;
        isAIPlayer = false;
        aiBrainInstance = null;
        negotiationRounds = 0;

        CleanTradeUI();
        StatEffectManager.Instance?.ClearEffect("all");
    }

    private void CleanTradeUI()
    {
         createOfferDisplay?.gameObject.SetActive(false);
         counterOfferDisplay?.gameObject.SetActive(false);
         haggleDisplay?.gameObject.SetActive(false);
         tradeResultDisplay?.gameObject.SetActive(false);
         tradeWindow?.gameObject.SetActive(false);
    }

    private void CleanTradeUIExceptResult()
    {
         createOfferDisplay?.gameObject.SetActive(false);
         counterOfferDisplay?.gameObject.SetActive(false);
         haggleDisplay?.gameObject.SetActive(false);
    }

    // --- Auto-Close Timer Logic ---
    private void StartAutoCloseTimer(float delay)
    {
         StopAutoCloseTimer(); // Stop any existing timer first
         autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay(delay));
         Debug.Log($"[TradeManager] Started auto-close timer for {delay} seconds.");
    }

    private void StopAutoCloseTimer()
    {
         if (autoCloseCoroutine != null)
         {
             StopCoroutine(autoCloseCoroutine);
             autoCloseCoroutine = null;
             Debug.Log("[TradeManager] Stopped auto-close timer.");
         }
    }

    private IEnumerator AutoCloseAfterDelay(float delay)
    {
         yield return new WaitForSeconds(delay);
         Debug.Log("[TradeManager] Auto-closing trade window after delay.");
         CloseTradeWindow();
         autoCloseCoroutine = null; // Clear the coroutine reference
    }

    // Validation logic for AI counter offers
    private bool ValidateAICounter(TradeOffer offer)
    {
        if (offer == null) { Debug.LogWarning("[ValidateAICounter] Offer is null."); return false; }
        if (player == null) { Debug.LogWarning("[ValidateAICounter] Player is null."); return false; }
        if (currentTrader == null) { Debug.LogWarning("[ValidateAICounter] Trader is null."); return false; }

        if (offer.goldToTrader < 0 || offer.goldToTrader > player.gold) { Debug.LogWarning($"[ValidateAICounter] Invalid Gold Offer: Player has {player.gold}, AI offers {offer.goldToTrader}"); return false; }
        if (offer.foodToPlayer < 0 || player.food + offer.foodToPlayer > player.maxFood) { Debug.LogWarning($"[ValidateAICounter] Invalid Food Request (Capacity): Player cap {player.maxFood}, current {player.food}, AI requests {offer.foodToPlayer}"); return false; }
        if (offer.waterToPlayer < 0 || player.water + offer.waterToPlayer > player.maxWater) { Debug.LogWarning($"[ValidateAICounter] Invalid Water Request (Capacity): Player cap {player.maxWater}, current {player.water}, AI requests {offer.waterToPlayer}"); return false; }
        if (offer.foodToPlayer > currentTrader.foodStock) { Debug.LogWarning($"[ValidateAICounter] Invalid Food Request (Stock): Trader has {currentTrader.foodStock}, AI requests {offer.foodToPlayer}"); return false; }
        if (offer.waterToPlayer > currentTrader.waterStock) { Debug.LogWarning($"[ValidateAICounter] Invalid Water Request (Stock): Trader has {currentTrader.waterStock}, AI requests {offer.waterToPlayer}"); return false; }
        if (offer.foodToTrader != 0 || offer.waterToTrader != 0) { Debug.LogWarning($"[ValidateAICounter] AI offering food/water not supported."); return false; }
        if (offer.goldToPlayer != 0) { Debug.LogWarning($"[ValidateAICounter] AI receiving gold not supported."); return false; }
        if (offer.goldToTrader == 0 && offer.foodToPlayer == 0 && offer.waterToPlayer == 0) { Debug.LogWarning($"[ValidateAICounter] AI proposed empty offer."); return false; }

        return true;
    }
}
