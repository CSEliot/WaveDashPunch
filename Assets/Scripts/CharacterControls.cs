using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControls : MonoBehaviour {

    public float MouseSensitivity = 50.0f;
    public float JumpPower = 20.0f;
    public float JetpackPower = 15.0f;
    public float MaxRunSpeed = 10.0f;
    public float RunAcceleration = 50.0f;
    public float AirAcceleration = 10.0f;

    public float MaxJetpackFuel = 100.0f;
    public float JetpackUseRate = 30.0f;
    public float MinJetpackFuel = 10.0f;
    public float JetpackRechargeRate = 15.0f;

    private float jetpackFuel;

    private Transform cameraTransform;
    private new Transform transform;
    private new Rigidbody rigidbody;
    private new CapsuleCollider collider;

    // Use this for initialization
    void Start ()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        jetpackFuel = MaxJetpackFuel;

        transform = GetComponent<Transform>();
        cameraTransform = transform.GetChild(0).GetComponent<Transform>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Mouse stuff
        Vector2 mouseChange = Vector2.zero;
        mouseChange.x = Input.GetAxis("Mouse X");
        mouseChange.y = Input.GetAxis("Mouse Y");

        // Mouse stuff
        float newCameraRotX = cameraTransform.rotation.eulerAngles.x - mouseChange.y * MouseSensitivity * Time.deltaTime;
        if (newCameraRotX > 90.0f && newCameraRotX < 180.0f)
            newCameraRotX = 90.0f;
        if (newCameraRotX < 270.0f && newCameraRotX > 180.0f)
            newCameraRotX = 270.0f; 
        float newCameraRotY = cameraTransform.rotation.eulerAngles.y + mouseChange.x * MouseSensitivity * Time.deltaTime;
        cameraTransform.rotation = Quaternion.Euler(newCameraRotX, newCameraRotY, cameraTransform.rotation.eulerAngles.z);

        // Check if on ground
        bool onGround = Physics.Raycast(transform.position + Vector3.down * 0.9f, Vector3.down, 0.5f);

        // Speed calculations
        Vector3 forward = cameraTransform.forward;
        forward.y = 0.0f;
        forward.Normalize();
        Vector3 right = cameraTransform.right;
        right.y = 0.0f;
        right.Normalize();

        float forwardSpeed = Vector3.Dot(forward, rigidbody.velocity);
        float rightSpeed = Vector3.Dot(right, rigidbody.velocity);

        // Movement
        bool skiing = Input.GetKey(KeyCode.LeftShift);
        collider.sharedMaterial.dynamicFriction = 0f;
        collider.sharedMaterial.staticFriction = 0f;
        if (!skiing)
        {
            collider.sharedMaterial.dynamicFriction = 1f;
            collider.sharedMaterial.staticFriction = 1f;

            bool keyPressed = false;
            if (Input.GetKey(KeyCode.W))
            {
                keyPressed = true;
                if (forwardSpeed < MaxRunSpeed)
                    rigidbody.AddForce(forward * (onGround ? RunAcceleration : AirAcceleration));
            }
            if (Input.GetKey(KeyCode.S))
            {
                keyPressed = true;
                if (forwardSpeed > -MaxRunSpeed)
                    rigidbody.AddForce(forward * -(onGround ? RunAcceleration : AirAcceleration));
            }
            if (Input.GetKey(KeyCode.A))
            {
                keyPressed = true;
                if (rightSpeed > -MaxRunSpeed)
                    rigidbody.AddForce(right * -(onGround ? RunAcceleration : AirAcceleration));
            }
            if (Input.GetKey(KeyCode.D))
            {
                keyPressed = true;
                if (rightSpeed < MaxRunSpeed)
                    rigidbody.AddForce(right * (onGround ? RunAcceleration : AirAcceleration));
            }

            if (!keyPressed)
            {
                collider.sharedMaterial.dynamicFriction = 5f;
                collider.sharedMaterial.staticFriction = 5f;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                if (forwardSpeed < MaxRunSpeed)
                    rigidbody.AddForce(forward * AirAcceleration);
            }
            if (Input.GetKey(KeyCode.S))
            {
                if (forwardSpeed > -MaxRunSpeed)
                    rigidbody.AddForce(forward * -AirAcceleration);
            }
            if (Input.GetKey(KeyCode.A))
            {
                if (rightSpeed > -MaxRunSpeed)
                    rigidbody.AddForce(right * -AirAcceleration);
            }
            if (Input.GetKey(KeyCode.D))
            {
                if (rightSpeed < MaxRunSpeed)
                    rigidbody.AddForce(right * AirAcceleration);
            }
        }

        // Jumping/jetpacking
        if (Input.GetMouseButton(1))
        {
            if (onGround)
                rigidbody.AddExplosionForce(JumpPower, transform.position + Vector3.down * 3.0f, 10.0f);
            if (jetpackFuel > MinJetpackFuel)
            {
                rigidbody.AddForce(Vector3.up * JetpackPower);
                jetpackFuel -= JetpackUseRate * Time.deltaTime;
            }
            else
            {
                jetpackFuel += JetpackRechargeRate * Time.deltaTime;
            }
        }
        else
        {
            jetpackFuel += JetpackRechargeRate * Time.deltaTime;
        }

        if (jetpackFuel < 0.0f)
            jetpackFuel = 0.0f;
        else if (jetpackFuel > MaxJetpackFuel)
            jetpackFuel = MaxJetpackFuel;
    }
}
