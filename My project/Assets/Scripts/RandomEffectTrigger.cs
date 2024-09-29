using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEffectTrigger : MonoBehaviour
{

    public int numberOfEffects = 3; // # of random effects to apply
    public bool effectEndFlag = false;

    private void OnTriggerEnter(Collider other)
    {
        // check if the player has entered the trigger
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            // apply the random effects to the player
            List<PlayerEffect> chosenEffects = ApplyRandomEffects(player);

            // Display the chosen effects on the screen
            UIManager.Instance.DisplayEffects(chosenEffects);

            // disable trigger after activation
            gameObject.SetActive(false);
        }
    }

    private List<PlayerEffect> ApplyRandomEffects(PlayerController player)
    {
        // get all possible effects
        List<PlayerEffect> allEffects = new List<PlayerEffect>((PlayerEffect[])System.Enum.GetValues(typeof(PlayerEffect)));
        List<PlayerEffect> chosenEffects = new List<PlayerEffect>();

        // shuffle the list of effects
        ShuffleList(allEffects);

        // Select the desired number of effects
        int effectsToApply = Mathf.Min(numberOfEffects, allEffects.Count);
        for (int i = 0; i < effectsToApply; i++)
        {
            PlayerEffect effect = allEffects[i];
            chosenEffects.Add(effect);

            // Apply the affect to the player
            player.ApplyEffect(effect);
        }
        return chosenEffects;
    }

    // Fisher-Yates shuffling alg
    public void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
