using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameVersion : MonoBehaviour
{

    public string Version;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static string Get()
    {
        return GameObject.FindGameObjectWithTag("Version").GetComponent<GameVersion>().Version;
    }
}
