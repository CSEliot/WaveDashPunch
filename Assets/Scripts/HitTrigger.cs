using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitTrigger : MonoBehaviour {
    public CharacterControls parent;

    void OnTriggerEnter(Collider collider)
    {
        if (!collider.CompareTag("PlayerTrigger") && collider.tag.Contains("Player") && collider.gameObject.GetComponent<CharacterControls>() != parent)
            parent.punchTargets.Add(collider.gameObject.GetComponent<CharacterControls>());
        CBUG.Do("TriggerEnter: " + collider.name);
    }

    void OnTriggerExit(Collider collider)
    {
        if (!collider.CompareTag("PlayerTrigger") && collider.tag.Contains("Player") && collider.gameObject.GetComponent<CharacterControls>() != parent)
            parent.punchTargets.Remove(collider.gameObject.GetComponent<CharacterControls>());
        CBUG.Do("TriggerExit: " + collider.name);
    }
}
