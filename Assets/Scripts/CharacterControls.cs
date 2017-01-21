using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterControls : MonoBehaviour {

    public float MouseSensitivity = 50.0f;
    public float MaxRunSpeed = 10.0f;
    public float RunAcceleration = 50.0f;
    public float AirAcceleration = 10.0f;

    public float JumpPower = 30.0f;
    public float JetpackPower = 25.0f;
    public float MaxJetpackFuel = 100.0f;
    public float JetpackUseRate = 30.0f;
    public float MinJetpackFuel = 10.0f;
    public float JetpackRechargeRate = 15.0f;

    public float Health = 100f;

    public float PunchEnergy = 20f;
    public float PunchBoostForce = 100f;

    private float jetpackFuel;
    private bool canJetpack;

    public Transform cameraTransform;
    private new Rigidbody rigidbody;
    private new CapsuleCollider collider;
    public BoxCollider hitTrigger;
    private Animator animator;

    private AudioSource jetpackStartSound, jetpackLoopSound;
    private AudioSource punchSound, punchGroundSound;
    private AudioSource dieSound;

    public PhotonView myPhotonView;

    // Use this for initialization
    void Start ()
    {
        if (!myPhotonView.isMine)
        {
            cameraTransform.GetComponent<Camera>().enabled = false;//gameObject.SetActive(false);
            return;
        }
        else
        {
            Debug.Log("My photon view, I am: " + myPhotonView.viewID);
        }

        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        AudioSource[] sources = cameraTransform.GetComponents<AudioSource>();
        foreach (AudioSource a in sources)
        {
            Debug.Log(a.clip.name);
            if (a.clip.name == "jetpack_start") jetpackStartSound = a;
            else if (a.clip.name == "jetpack_loop") jetpackLoopSound = a;
            else if (a.clip.name == "punch") punchSound = a;
            else if (a.clip.name == "punch_ground") punchGroundSound = a;
            else if (a.clip.name == "die") dieSound = a;
        }

        jetpackFuel = MaxJetpackFuel;
        canJetpack = true;
        
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
        hitTrigger = transform.GetChild(0).GetChild(2).GetComponent<BoxCollider>();
        animator = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!myPhotonView.isMine)
            return;
        else
        {
            Debug.Log("My photon view, I am: " + myPhotonView.viewID);
        }

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

        // Check movement controls
        Vector3 inputDir = Vector3.zero;
        bool keyPressed = false;
        if (Input.GetKey(KeyCode.W))
        {
            keyPressed = true;
            inputDir += forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            keyPressed = true;
            inputDir -= forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            keyPressed = true;
            inputDir -= right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            keyPressed = true;
            inputDir += right;
        }
        inputDir.Normalize();

        // Movement
        bool skiing = Input.GetKey(KeyCode.LeftShift);
        collider.sharedMaterial.dynamicFriction = 0f;
        collider.sharedMaterial.staticFriction = 0f;
        if (!onGround || skiing)
        {
            Vector3 velocityDir = rigidbody.velocity;
            velocityDir.y = 0;
            velocityDir.Normalize();
            Vector3 perpForward = forward - Vector3.Dot(velocityDir, forward) * forward.normalized;
            Vector3 perpRight = right - Vector3.Dot(velocityDir, right) * right.normalized;

            if (Input.GetKey(KeyCode.W))
            {
                if (forwardSpeed < MaxRunSpeed)
                    rigidbody.AddForce(((skiing && onGround) ? perpForward : forward) * AirAcceleration);
            }
            if (Input.GetKey(KeyCode.S))
            {
                if (forwardSpeed > -MaxRunSpeed)
                    rigidbody.AddForce(((skiing && onGround) ? perpForward : forward) * -AirAcceleration);
            }
            if (Input.GetKey(KeyCode.A))
            {
                if (rightSpeed > -MaxRunSpeed)
                    rigidbody.AddForce(((skiing && onGround) ? perpRight : right) * -AirAcceleration);
            }
            if (Input.GetKey(KeyCode.D))
            {
                if (rightSpeed < MaxRunSpeed)
                    rigidbody.AddForce(((skiing && onGround) ? perpRight : right) * AirAcceleration);
            }
        }
        else  // not skiing
        {
            collider.sharedMaterial.dynamicFriction = keyPressed ? 1f : 5f;
            collider.sharedMaterial.staticFriction = keyPressed ? 1f : 5f;

            rigidbody.AddForce(inputDir * (onGround ? RunAcceleration : AirAcceleration));
            if (rigidbody.velocity.sqrMagnitude > MaxRunSpeed * MaxRunSpeed)
            {
                if (onGround)
                    rigidbody.velocity = rigidbody.velocity.normalized * MaxRunSpeed;
            }
        }

        // Jumping/jetpacking
        if (Input.GetMouseButtonDown(1))
        {
            // Start playing jetpack sounds
            jetpackLoopSound.volume = 0f;
            jetpackStartSound.volume = 1f;
            jetpackStartSound.Play();
            jetpackLoopSound.loop = true;
            jetpackLoopSound.Play();

            canJetpack = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            jetpackLoopSound.volume = 0f;
            jetpackStartSound.Stop();
        }
        if (Input.GetMouseButton(1))
        {
            // Transition into more seemless loop from initial burst
            jetpackLoopSound.volume = Mathf.Min(1f, jetpackLoopSound.volume + 0.5f * Time.deltaTime);
            jetpackStartSound.volume = Mathf.Max(0f, jetpackStartSound.volume - 0.5f * Time.deltaTime);

            if (onGround)
                rigidbody.AddExplosionForce(JumpPower, transform.position + Vector3.down * 3.0f, 10.0f);
            if (jetpackFuel > MinJetpackFuel)
            {
                canJetpack = true;
            }
            else if (jetpackFuel <= 0f)
            {
                jetpackFuel = 0.0f;
                canJetpack = false;
            }

            if (canJetpack)
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

        // Update jetpack UI
        _UI.SetJetBar(jetpackFuel / MaxJetpackFuel);

        // Wave/punch
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Use energy, give boost
            if (jetpackFuel - PunchEnergy >= 0f)
            {
                animator.SetBool("DoPunch", true);
                animator.SetBool("DoWave", false);
                jetpackFuel = Mathf.Max(jetpackFuel - PunchEnergy, 0f);

                if (Physics.Raycast(transform.position, cameraTransform.forward, 3f))
                {
                    punchGroundSound.Play();
                    rigidbody.AddExplosionForce(PunchBoostForce, transform.position + cameraTransform.forward * 5, 100f);
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetBool("DoWave", true);
            animator.SetBool("DoPunch", false);
        }
        else
        {
            animator.SetBool("DoWave", false);
            animator.SetBool("DoPunch", false);
        }
    }
}
