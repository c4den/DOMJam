using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerEffectEnum.cs

/// <summary>
/// Represents the various effects that can be applied to the player.
/// </summary>
public enum PlayerEffect
{
    Balance, // no negative effects, go all out!
    Jester, // You and enemies emit silly noises when hit
    Knight, // Everyone is Melee!
    Rogue, // Enemies kill each other
    Sun, // Enemies have 2x HP
    Flames, // You take damage every 5 seconds
    Fool, // No weapons!
    Fates, // Everyone has 1HP
    Donjon, // You cannot move
    Comet // Jump Height x5
}

public class EffectEnum : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
