using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BountyBoardUI : MonoBehaviour
{
    [Header("Contract Buttons")]
    public Button openContractBtn;
    public Button veteranContractBtn;
    public Button eliteContractBtn;

    [Header("Current Habitat Target")]
    // In a full game, you might change this via a dropdown or map click.
    // For now, we expose it to the Inspector.
    public Habitat selectedHabitat = Habitat.Normal;

    [Header("Audio ")]
    public AudioSource audioSource;
    public AudioClip stapleSound;
    public AudioClip paperRustleSound;
    public AudioClip stampSound;
    public AudioClip splashSound;
    public AudioClip crateOpenSound;

    [Header("In Progress UI")]
    public GameObject inProgressOverlay; // A UI panel saying "Scavenger Dispatched..."
    public TextMeshProUGUI countdownText; // Optional: To show the countdown
    public Button collectBountyBtn;

    [Header("Error Feedback")]
    public TextMeshProUGUI errorText; // The "Not Enough Lumins" text
    public AudioClip errorSound;      // Optional: A dull thud or jammed stapler sound
    private Coroutine _errorCoroutine;

    [Header("Lootbox UI - Single Panel")]
    public GameObject lootboxPanel;          // The main background for the sequence
    public Animator chestAnimator;           // The Animator component on your 2D Chest Prefab
    public Button chestClickButton;          // An invisible button placed over the chest
    public float animationDelay = 1.0f;      // How long to wait for the chest to open before showing stats

    [Header("Lootbox UI - Reveal Content")]
    public GameObject revealContentContainer; // Holds everything below, starts HIDDEN
    public Image resultFishIcon;
    public TextMeshProUGUI resultFishNameText;
    public Image resultRarityStamp;
    public TextMeshProUGUI warningText;

    [Header("Fish Data Stats")]
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI habitatText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI baseLuminText; // Shows the BoonBonus

    [Header("Player Choice Buttons")]
    public Button keepButton;
    public Button releaseButton;
    public TextMeshProUGUI releaseRefundText; // E.g., "Release (+400 Lumins)"

    private void OnEnable()
    {
        if (BountyBoardManager.Instance != null)
        {
            BountyBoardManager.Instance.OnBountyStarted += ShowInProgressState;
            BountyBoardManager.Instance.OnBountyReadyToReveal += ShowCollectState;
            BountyBoardManager.Instance.OnRewardProcessed += HandleRewardProcessed;
        }

        // Hook up the chest click to start the Coroutine
        collectBountyBtn.onClick.AddListener(SetupMysteryChest);
        chestClickButton.onClick.AddListener(StartRevealSequence);
        keepButton.onClick.AddListener(() => SubmitChoice(true));
        releaseButton.onClick.AddListener(() => SubmitChoice(false));

        // Hook up the buttons
        openContractBtn.onClick.AddListener(BuyOpenContract);
        veteranContractBtn.onClick.AddListener(BuyVeteranContract);
        eliteContractBtn.onClick.AddListener(BuyEliteContract);
    }

    private void OnDisable()
    {
        if (BountyBoardManager.Instance != null)
        {
            BountyBoardManager.Instance.OnBountyStarted -= ShowInProgressState;
            //BountyBoardManager.Instance.OnBountyFulfilled -= ShowBountyResult;
            BountyBoardManager.Instance.OnBountyReadyToReveal -= ShowCollectState;
            BountyBoardManager.Instance.OnRewardProcessed -= HandleRewardProcessed;
        }

        chestClickButton.onClick.RemoveAllListeners();
        keepButton.onClick.RemoveAllListeners();
        releaseButton.onClick.RemoveAllListeners();

        openContractBtn.onClick.RemoveAllListeners();
        veteranContractBtn.onClick.RemoveAllListeners();
        eliteContractBtn.onClick.RemoveAllListeners();
    }

    //ANGLER RETURNS
    private void ShowCollectState()
    {
        // 1. Change the text so it no longer looks like a timer
        countdownText.text = "Angler has returned!\nReady to collect.";

        // 2. Turn on the shiny new collect button
        if (collectBountyBtn != null) collectBountyBtn.gameObject.SetActive(true);
    }

    // (SETUP THE CHEST) 
    private void SetupMysteryChest()
    {
        if (inProgressOverlay != null) inProgressOverlay.SetActive(false);

        //Show the main panel
        lootboxPanel.SetActive(true);

        //Hide the stats and buttons
        revealContentContainer.SetActive(false);

        //Reset the chest to its closed state (if your animator has a trigger for it, or just let it idle)
        chestClickButton.interactable = true;

        // Optional: If your animator needs a reset trigger when closing/reopening the menu
        // if (chestAnimator != null) chestAnimator.SetTrigger("Reset"); 
    }

    // PLAYER CLICKS THE CHEST 
    private void StartRevealSequence()
    {
        // Prevent double-clicking
        chestClickButton.interactable = false;

        // Trigger the animation and sound
        PlaySound(crateOpenSound);
        if (chestAnimator != null) chestAnimator.SetTrigger("Open"); 

        // Start the wait
        StartCoroutine(RevealDelayRoutine());
    }

    private IEnumerator RevealDelayRoutine()
    {
        // Wait for the chest animation to physically open
        yield return new WaitForSeconds(animationDelay);

        // Fetch the data we saved in the Manager's "Purgatory"
        PendingReward reward = BountyBoardManager.Instance.GetPendingReward();
        if (reward == null) yield break;

        FishData newFishData = FishDatabase.Instance.GetById(reward.FishId);
        if (newFishData == null)
        {
            Debug.LogError($"No fish found for ID: {reward.FishId}");
            yield break;
        }
        Debug.Log($"[DEBUG] Pulled Fish ID: {reward.FishId}. Database Name says: '{newFishData.Name}'");

        // Populate the UI (same as your previous logic)
        resultFishNameText.text = newFishData.Name;
        UpdateRarityStampVisuals(reward.Rarity);
        LoadDynamicIcon(newFishData.Name, reward.Rarity);

        descriptionText.text = newFishData.Description;
        //habitatText.text = $"Habitat: {newFishData.Habitat.ToString()}";

        float actualWeight = newFishData.Weight * reward.WeightModifier;
        weightText.text = $"Weight: {actualWeight:F1} lbs";
        baseLuminText.text = $"Base Output: {newFishData.BoonBonus} Lumins/min";

        float refundAmount = reward.CostPaid * 0.4f;
        releaseRefundText.text = $"Release (+{refundAmount} Lumins)";

        //APACITY CHECK 
        bool hasSpace = IdleTankManager.Instance.HasSpaceInTank();
        keepButton.interactable = hasSpace;

        if (!hasSpace)
        {
            warningText.enabled = true;
            warningText.text = "Tank is Full! You must release this catch.";
        }
        else
        {
            warningText.enabled = false;
        }

            // Show the UI elements
            revealContentContainer.SetActive(true);
        PlaySound(paperRustleSound);
        PlaySound(stampSound);
    }

    //THE CHOICE 
    private void SubmitChoice(bool keep)
    {
        if (!keep) PlaySound(splashSound);
        BountyBoardManager.Instance.ProcessPlayerChoice(keep);
    }

    private void HandleRewardProcessed(bool kept)
    {
        // Hide the single panel and re-enable the store
        lootboxPanel.SetActive(false);
        openContractBtn.interactable = true;
        veteranContractBtn.interactable = true;
        eliteContractBtn.interactable = true;
    }

    /*
    private void ShowMysteryBox()
    {
        if (inProgressOverlay != null) inProgressOverlay.SetActive(false);

        mysteryPanel.SetActive(true);
        revealPanel.SetActive(false);
    }

    private void ExecuteReveal()
    {
        PlaySound(crateOpenSound);
        mysteryPanel.SetActive(false);

        PendingReward reward = BountyBoardManager.Instance.GetPendingReward();
        if (reward == null) return;

        FishData newFishData = FishDatabase.Instance.GetById(reward.FishId);
        if (newFishData == null) return;

        // 1. Populate the basic visuals
        resultFishNameText.text = newFishData.Name;
        UpdateRarityStampVisuals(reward.Rarity);
        LoadDynamicIcon(newFishData.Name, reward.Rarity);

        // 2. Populate the deep lore/stats from FishData
        descriptionText.text = newFishData.Description;
        habitatText.text = $"Habitat: {newFishData.Habitat.ToString()}";

        // Calculate the actual weight based on the modifier the manager rolled
        float actualWeight = newFishData.Weight * reward.WeightModifier;
        weightText.text = $"Weight: {actualWeight:F1} lbs";

        baseLuminText.text = $"Base Output: {newFishData.BoonBonus} Lumins/min";

        // 3. Setup the Release Button text math
        float refundAmount = reward.CostPaid * 0.4f;
        releaseRefundText.text = $"Release (+{refundAmount} Lumins)";

        // 4. Show the panel
        revealPanel.SetActive(true);
        PlaySound(paperRustleSound);
        PlaySound(stampSound);
    }
    */
    private void LoadDynamicIcon(string fishName, RarityLevel rarity)
    {
        string iconPath = $"Icons/{fishName}{rarity.ToString()}icon";
        Sprite loadedSprite = Resources.Load<Sprite>(iconPath);

        if (loadedSprite != null) resultFishIcon.sprite = loadedSprite;
        else
        {
            resultFishIcon.sprite = Resources.Load<Sprite>("Icons/UnknownFish");
            Debug.LogError($"Missing icon: {iconPath}");
        }
    }
    

    private void ShowInProgressState(float waitTime)
    {
        // 1. Disable the buttons so they can't buy another one yet
        openContractBtn.interactable = false;
        veteranContractBtn.interactable = false;
        eliteContractBtn.interactable = false;

        // 2. Show the "Scavenger is hunting" text overlay
        if (inProgressOverlay != null) inProgressOverlay.SetActive(true);

        // 3. Optional: Start a UI timer here if you want to count down the 'waitTime'
    }

    private void Update()
    {
        // If the overlay is active, update the countdown text every frame
        if (inProgressOverlay.activeSelf)
        {
            ActiveBountyRecord activeBounty = BountyBoardManager.Instance.GetActiveBounty();
            if (activeBounty != null)
            {
                TimeSpan timeRemaining = activeBounty.CompletionTime - DateTime.UtcNow;

                // Prevent negative numbers if it finishes
                if (timeRemaining.TotalSeconds < 0) timeRemaining = TimeSpan.Zero;

                // Format it nicely: "HH:MM:SS"
                countdownText.text = $"Angler returns in:\n{timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
            }
        }
    }

    private void ShowInProgressState(ActiveBountyRecord bounty)
    {
        openContractBtn.interactable = false;
        veteranContractBtn.interactable = false;
        eliteContractBtn.interactable = false;

        // Ensure the collect button starts hidden while the timer is running
        if (collectBountyBtn != null) collectBountyBtn.gameObject.SetActive(false);

        if (inProgressOverlay != null) inProgressOverlay.SetActive(true);
    }
    /*
    private void ShowBountyResult(int fishId, RarityLevel rarity)
    {
        // 1. Hide the progress overlay and re-enable the purchase buttons
        if (inProgressOverlay != null) inProgressOverlay.SetActive(false);
        openContractBtn.interactable = true;
        veteranContractBtn.interactable = true;
        eliteWarrantBtn.interactable = true;

        // 2. Get the visual data for this fish
        FishData newFishData = FishDatabase.Instance.GetById(fishId);
        if (newFishData == null) return;

        // 3. Update the UI text
        resultFishNameText.text = newFishData.Name;

        // 4. --- LOAD THE DYNAMIC ICON ---
        // Remember: No "Resources/" prefix and no file extensions like ".png"!
        string iconPath = $"Icons/{newFishData.Name}{rarity.ToString()}icon";

        // Attempt to load the sprite from the Resources/Icons folder
        Sprite loadedSprite = Resources.Load<Sprite>(iconPath);

        if (loadedSprite != null)
        {
            resultFishIcon.sprite = loadedSprite;
        }
        else
        {
            // FALLBACK LOGIC
            string fallbackIconPath = "Icons/UnknownFish";

            Sprite fallbackSprite = Resources.Load<Sprite>(fallbackIconPath);
            if (fallbackSprite != null)
            {
                resultFishIcon.sprite = fallbackSprite;
            }

            Debug.LogError($"Could not find an icon at path: {iconPath}. Did you name it correctly?");
        }

        // 5. Set the Rarity Stamp color/image
        UpdateRarityStampVisuals(newFishData.Rarity);

        // 6. Show the result panel and play the tactile sounds
        //resultPanel.SetActive(true);
        PlaySound(paperRustleSound);
        PlaySound(stampSound);
    }
    // --- BUTTON CLICK HANDLERS ---
    */
    private void BuyOpenContract()
    {
        bool success = BountyBoardManager.Instance.PostBounty(BountyTier.OpenContract, selectedHabitat);

        if (success) { PlaySound(stapleSound); }
        else { ShowError("Not Enough Lumins!"); }
    }

    private void BuyVeteranContract()
    {
        bool success = BountyBoardManager.Instance.PostBounty(BountyTier.VeteranContract, selectedHabitat);

        if (success) { PlaySound(stapleSound); }
        else { ShowError("Not Enough Lumins!"); }
    }

    private void BuyEliteContract()
    {
        bool success = BountyBoardManager.Instance.PostBounty(BountyTier.EliteContract, selectedHabitat);

        if (success) { PlaySound(stapleSound); }
        else { ShowError("Not Enough Lumins!"); }
    }

    // --- RESULT PRESENTATION ---
    /*
    private void ShowBountyResult(int fishId, Rarity rarity)
    {
        //Get the visual data for this fish
        FishData newFishData = FishDatabase.Instance.GetById(fishId);
        if (newFishData == null)
        {
            Debug.LogError($"Could not find a fish by id: {fishId}");
            return;
        }

        //Update the UI text and icons
        resultFishNameText.text = newFishData.Name;
        string iconPath = $"Icons/{newFishData.Name}{rarity.ToString()}icon";
        // Attempt to load the sprite from the Resources folder
        Sprite loadedSprite = Resources.Load<Sprite>(iconPath);

        if (loadedSprite != null)
        {
            resultFishIcon.sprite = loadedSprite;
        }
        else
        {
            // FALLBACK
            string fallbackIconPath = "Icons/UnknownFish";

            Sprite fallbackSprite = Resources.Load<Sprite>(fallbackIconPath);
            if (fallbackSprite != null)
            {
                resultFishIcon.sprite = fallbackSprite;
            }

            Debug.LogError($"Could not find an icon at path: {iconPath}");
        }

        //Set the Rarity Stamp color/image
        UpdateRarityStampVisuals(rarity);

        //Show the panel and play the stamp sound
        resultPanel.SetActive(true);
        PlaySound(paperRustleSound);
        PlaySound(stampSound);
    }
    */
    private void UpdateRarityStampVisuals(RarityLevel rarity)
    {
        // For prototyping, we can just change the color of a generic stamp image.
        // Later, you could swap out the actual Sprite for different wax seals/stamps.
        switch (rarity)
        {
            case RarityLevel.Common:
                resultRarityStamp.color = Color.gray;
                break;
            case RarityLevel.Uncommon:
                resultRarityStamp.color = Color.green;
                break;
            case RarityLevel.Rare:
                resultRarityStamp.color = Color.cyan;
                break;
            case RarityLevel.Epic:
                resultRarityStamp.color = new Color(0.5f, 0f, 0.5f); // Purple
                break;
            case RarityLevel.Legendary:
                resultRarityStamp.color = new Color(1f, 0.8f, 0f); // Gold
                break;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // --- ERROR FEEDBACK ---

    private void ShowError(string message)
    {
        PlaySound(errorSound);

        if (errorText == null) return;

        // If an error is already flashing, stop it so we can restart the animation cleanly
        if (_errorCoroutine != null) StopCoroutine(_errorCoroutine);

        _errorCoroutine = StartCoroutine(FlashErrorText(message));
    }

    private System.Collections.IEnumerator FlashErrorText(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);

        // Ensure it starts fully opaque red
        errorText.color = new Color(1f, 0.2f, 0.2f, 1f);

        // Wait for half a second so the player can read it
        yield return new WaitForSeconds(0.5f);

        // Fade it out over 1 second
        float fadeDuration = 1.0f;
        float elapsed = 0f;
        Color startColor = errorText.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            errorText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null; // Wait for the next frame
        }

        errorText.gameObject.SetActive(false);
    }
}
