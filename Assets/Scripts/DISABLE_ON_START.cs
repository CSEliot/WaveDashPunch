using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DISABLE_ON_START : MonoBehaviour {

    public bool DontDisableThis;

    // Use this for initialization
    void Start () {

        if(!DontDisableThis)
            gameObject.SetActive(false);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
