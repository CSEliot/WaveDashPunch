using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class _UI {

    /// <summary>
    /// Set the HP Bar of the player.
    /// </summary>
    /// <param name="Percentage">from 0 to 1.0f</param>
    public static void SetHPBar(float Percentage)
    {
        GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>().SetHPBar(Percentage);
    }
    
    /// <summary>
     /// Set the Jetpack Fuel Bar of the player.
     /// </summary>
     /// <param name="Percentage">from 0 to 1.0f</param>
    public static void SetJetBar(float Percentage)
    {
        GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>().SetJetBar(Percentage);
    }
}
