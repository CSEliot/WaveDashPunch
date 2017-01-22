using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class How2PlayManager : MonoBehaviour {


    public GameObject HelpPanel;
    public GameObject PunchInfoPanel;
    public GameObject WaveInfoPanel;
    public GameObject ObjectivePanel;
    public GameObject DeathPanel;

    private bool hasFirstTimePunched;
    private bool hasFirstTimeWaved;

    public float SecondsTillObjReveal;

    private bool ObjectiveRevealed;

    public GameObject DummyCamera;

    private float spawnTime;

    // Use this for initialization
    void Start () {

        hasFirstTimePunched = false;
        HelpPanel.SetActive(true);
        PunchInfoPanel.SetActive(false);
        WaveInfoPanel.SetActive(false);
        ObjectivePanel.SetActive(false);

        ObjectiveRevealed = false;

	}
	
	// Update is called once per frame
	void Update () {

        if (DeathPanel.GetActive() && Input.GetButtonDown("Respawn"))
        {
            Step3_SpawnAndJoin._SpawnPlayer();
            ToggleDeathPanel();
            DummyCamera.SetActive(false);
        }

        if(SecondsTillObjReveal < Time.time && !ObjectiveRevealed)
        {
            ObjectiveRevealed = true;
            //ObjectivePanel.SetActive(false);
        }

        if (Input.GetButtonDown("Help"))
        {
            Debug.Log("Help Toggled");
            HelpPanel.SetActive(!HelpPanel.GetActive());
        }

        if (Input.GetButtonDown("ClosePunchHelp"))
        {
            PunchInfoPanel.SetActive(false);
        }

        if (Input.GetButtonDown("CloseWaveHelp"))
        {
            WaveInfoPanel.SetActive(false);
        }
	}

    public void FirstTimePunch()
    {

        if (HelpPanel.GetActive())
            return;

        if (hasFirstTimePunched)
            return;
        hasFirstTimePunched = true;
        PunchInfoPanel.SetActive(true);
    }

    public void FirstTimeWaved()
    {

        if (HelpPanel.GetActive())
            return;

        if (hasFirstTimeWaved)
            return;
        hasFirstTimeWaved = true;
        WaveInfoPanel.SetActive(true);
    }

    public void ToggleDeathPanel()
    {
        DeathPanel.SetActive(!DeathPanel.GetActive());

        DummyCamera.SetActive(true);
    }
}
