using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetGameVersion : MonoBehaviour
{

    public Text VersionText;

    // Use this for initialization
    void Start()
    {
        VersionText.text = "v" + GameVersion.Get();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
