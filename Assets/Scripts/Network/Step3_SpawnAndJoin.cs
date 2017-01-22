using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Step3_SpawnAndJoin : MonoBehaviour {

    #region Public Sets
    public string[] SpawnOBJNames;
    public Transform Spawner;
    #endregion

    #region Public Refs
    public GameObject UICharSelectPanel;
    #endregion
    //TODO: DELETE THIS, DONT TRACK THIS WAY
    private List<GameObject> Players;



	// Use this for initialization
	void Start () {
        Players = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    void OnJoinedRoom()
    {
        CBUG.Do("Joined Room!!");
        CreatePlayerObject();
    }

    void CreatePlayerObject()
    {

        Debug.Log("Spawning Player!");

        GameObject newPlayerObject = PhotonNetwork.Instantiate(SpawnOBJNames[Random.Range(0, SpawnOBJNames.Length)], Spawner.position, Spawner.rotation, 0);

        Players.Add(newPlayerObject);

        UICharSelectPanel.SetActive(false);
    }

    public static void _SpawnPlayer()
    {
        GameObject.FindGameObjectWithTag("NETWORK").GetComponent<Step3_SpawnAndJoin>().CreatePlayerObject();
    }


}
