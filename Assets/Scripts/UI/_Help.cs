using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Help : MonoBehaviour {


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static void FirstTimePunched()
    {
        GameObject.FindGameObjectWithTag("Help").GetComponent<How2PlayManager>().FirstTimePunch();
    }

    public static void FirstTimeWaved()
    {
        GameObject.FindGameObjectWithTag("Help").GetComponent<How2PlayManager>().FirstTimeWaved();
    }

    public static void ToggleDeathPanel()
    {
        GameObject.FindGameObjectWithTag("Help").GetComponent<How2PlayManager>().ToggleDeathPanel();
    }
}
