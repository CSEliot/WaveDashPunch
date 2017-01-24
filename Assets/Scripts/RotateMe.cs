using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMe : MonoBehaviour
{

    private Vector3 myRotation;
    public float RotSpeed;

    private float x;
    private float y;

    // Use this for initialization
    void Start()
    {
        x = 0f;
        y = 0f;
    }

    // Update is called once per frame
    void Update()
    {

        x += RotSpeed;
        y += RotSpeed;

        myRotation.Set(x, y, 0f);
        gameObject.transform.rotation = Quaternion.Euler( myRotation);//Rotate(myRotation);
    }
}
