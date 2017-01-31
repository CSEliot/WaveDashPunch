using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveTrigger : MonoBehaviour {
    public CharacterControls parent;


    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("iPC") && collider.gameObject.GetComponent<CharacterControls>() != parent)
            parent.WaveCandidates.Add(collider.GetComponent<PhotonView>());
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("iPC") && collider.gameObject.GetComponent<CharacterControls>() != parent)
        {
            parent.WaveCandidates.Remove(collider.GetComponent<PhotonView>());
            if(parent.WaveRecieved.Contains(collider.GetComponent<PhotonView>().viewID))
                parent.WaveRecieved.Remove(collider.GetComponent<PhotonView>().viewID);
        }
    }
}
