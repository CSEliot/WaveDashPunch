using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    #region Public Refs
    public RectTransform jetpackBar;
    public RectTransform healthBar;
    #endregion

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetHPBar(float Percentage)
    {
        healthBar.localScale = new Vector3(Percentage, 1f, 1f);
    }

    public void SetJetBar(float Percentage)
    {
        jetpackBar.localScale = new Vector3(Percentage, 1f, 1f);
    }
}
