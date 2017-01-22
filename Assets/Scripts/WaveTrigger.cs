using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveTrigger : MonoBehaviour {
    public CharacterControls parent;

    void OnTriggerEnter(Collider collider)
    {
        if (!collider.CompareTag("PlayerTrigger") && collider.gameObject.CompareTag("Player") && collider.gameObject.GetComponent<CharacterControls>() != parent)
            parent.waveTarget = collider.gameObject.GetComponent<CharacterControls>();
    }

    void OnTriggerLeave(Collider collider)
    {
        if (!collider.CompareTag("PlayerTrigger") && collider.gameObject.CompareTag("Player") && collider.gameObject.GetComponent<CharacterControls>() != parent)
            parent.waveTarget = null;
    }
}
