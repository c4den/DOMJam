using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Text effectsText; // assign the text in the inspector

    private void Awake()
    {
        // singleton pattern ensures a single instance
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void DisplayEffects(List<PlayerEffect> effects)
    {
        string message = "Effects Applied: \n";
        foreach (PlayerEffect effect in effects)
        {
            message += "- " + GetEffectName(effect) + "\n";
        }

        effectsText.text = message;
    }

    private string GetEffectName(PlayerEffect effect)
    {
        switch (effect)
        {
            case PlayerEffect.Balance:
                return "Balance - all as it should be";
            case PlayerEffect.Jester:
                return "The Jester! - SILLY!";
            case PlayerEffect.Knight:
                return "Knight - Melee for all!";
            case PlayerEffect.Rogue:
                return "Rogue - Everyone's mad";
            case PlayerEffect.Sun:
                return "The Sun - 2x HP";
            case PlayerEffect.Flames:
                return "Flames - It burns, burns, burns";
            case PlayerEffect.Fool:
                return "The Fool - Please don't run.";
            case PlayerEffect.Fates:
                return "The Fates - 1HP!";
            case PlayerEffect.Donjon:
                return "Donjon - No moving!";
            case PlayerEffect.Comet:
                return "The Comet - To orbit!";
            default:
                return effect.ToString();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
