using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterControls : MonoBehaviour
{

    private float survivalTime;
    private float respawnTime;

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
    public float PunchDamage = 25f;
    public float PunchBoostForce = 100f;

    private float jetpackFuel;
    private bool canJetpack;

    public Transform cameraTransform;
    public Rigidbody rigidbody;
    public CapsuleCollider collider;
    public BoxCollider hitTrigger;
    public Animator animator;

    private AudioSource jetpackStartSound, jetpackLoopSound;
    private AudioSource punchSound, punchGroundSound;
    private AudioSource dieSound;

    public PhotonView myPhotonView;

    public CharacterControls punchTarget;
    public CharacterControls waveTarget;

    public int RespawnYLimit;


    public float MinPeaceTime;
    private float previousPunchTime;
    private bool direSituation;

    private bool isDead;

    public GameObject ArmLeft, ArmRight, FakeBody;

    public float punchCooldown;
    private float lastPunchTime;
    
    // Use this for initialization
    void Start()
    {
        survivalTime = 0;
        respawnTime = 0;
        isDead = false;

        if (!myPhotonView.isMine)
        {
            cameraTransform.GetComponent<Camera>().enabled = false;//gameObject.SetActive(false);
            cameraTransform.GetComponent<AudioListener>().enabled = false;
        }
        else
        {
            GameObject.FindGameObjectWithTag("MUSIC").GetComponent<MusicManager>().MusicBox1 = transform.GetChild(0).GetChild(4).GetComponent<AudioSource>();
            GameObject.FindGameObjectWithTag("MUSIC").GetComponent<MusicManager>().MusicBox2 = transform.GetChild(0).GetChild(5).GetComponent<AudioSource>();
            MusicManager._SwitchTo(0);
            Debug.Log("My photon view, I am: " + myPhotonView.viewID);
        }

        
        Cursor.lockState = CursorLockMode.Locked;
        jetpackFuel = MaxJetpackFuel;
        canJetpack = true;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!myPhotonView.isMine)
            return;

        if (direSituation)
        {
            if (Time.time - previousPunchTime > MinPeaceTime)
            {
                direSituation = false;
                MusicManager._SwitchTo(0);
            }
        }

        // Wave/punch
        if (Input.GetButtonDown("Punch") && Time.time - previousPunchTime > punchCooldown)
        {
            previousPunchTime = Time.time;
            //_Help.FirstTimePunched();
            myPhotonView.RPC("DoPunch", PhotonTargets.Others);
            DoPunchLocal();
        }
        else if (Input.GetButtonDown("Wave"))
        {
            //_Help.FirstTimeWaved();
            // Use energy, give boost
            if (jetpackFuel - PunchEnergy >= 0f)
            {
                myPhotonView.RPC("DoWave", PhotonTargets.Others);
                DoWaveLocal();
            }
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
        // Check if underground
        if(transform.position.y < RespawnYLimit && !isDead)
        {
            die();
        }

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
        bool skiing = Input.GetButton("Ski");
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
        if (Input.GetButtonDown("Fire2"))
        {
            // Start playing jetpack sounds
            jetpackLoopSound.volume = 0f;
            jetpackStartSound.volume = 1f;
            jetpackStartSound.Play();
            jetpackLoopSound.loop = true;
            jetpackLoopSound.Play();

            canJetpack = true;
        }

        jetpackStartSound.volume = Mathf.Max(0f, jetpackStartSound.volume - 2f * Time.deltaTime);
        if (Input.GetButton("Fire2"))
        {
            // Transition into more seemless loop from initial burst
            jetpackLoopSound.volume = Mathf.Min(1f, jetpackLoopSound.volume + 0.5f * Time.deltaTime);

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
            jetpackLoopSound.volume = Mathf.Max(0f, jetpackLoopSound.volume - 2f * Time.deltaTime);
            jetpackFuel += JetpackRechargeRate * Time.deltaTime;
        }
        if (jetpackFuel < 0.0f)
            jetpackFuel = 0.0f;
        else if (jetpackFuel > MaxJetpackFuel)
            jetpackFuel = MaxJetpackFuel;

        // Die if health is gone
        //Dead, Death
        if (!isDead && (Health <= 0f || Input.GetButtonDown("Suicide")))
        {
            die();
        }

        // Update jetpack UI
        _UI.SetJetBar(jetpackFuel / MaxJetpackFuel);
        _UI.SetHPBar(Health / 100f);
    }

    private void localDie()
    {
        _Help.ToggleDeathPanel();
        PhotonNetwork.Destroy(myPhotonView);
    }

    [PunRPC]
    private void netDie()
    {
        CBUG.Do("DIED!");
        dieSound.Play();
        isDead = true;
        ArmLeft.transform.SetParent(null);
        ArmLeft.GetComponent<Rigidbody>().isKinematic = false;
        ArmLeft.GetComponent<BoxCollider>().isTrigger = false;
        ArmRight.transform.SetParent(null);
        ArmRight.GetComponent<BoxCollider>().isTrigger = false;
        ArmRight.GetComponent<Rigidbody>().isKinematic = false;
        FakeBody.transform.SetParent(null);
        FakeBody.SetActive(true);
    }

    private void die()
    {
        myPhotonView.RPC("netDie", PhotonTargets.All);
        localDie();
    }

    [PunRPC]
    public void DoPunch()
    {
        animator.SetTrigger("DoPunch");
        if (Physics.Raycast(transform.position, cameraTransform.forward, 3f))
        {
            punchGroundSound.Play();
            if (punchTarget != null)
            {
                punchTarget.rigidbody.AddExplosionForce(PunchBoostForce, punchTarget.rigidbody.position + punchTarget.cameraTransform.forward * 5, 100f);
                punchTarget.ModHealth(-PunchDamage);
                MusicManager._SwitchTo(1);
            }
        }
    }

    public void DoPunchLocal()
    {
        animator.SetTrigger("DoPunch");
        jetpackFuel = Mathf.Max(jetpackFuel - PunchEnergy, 0f);
        if (jetpackFuel != 0f && Physics.Raycast(transform.position, cameraTransform.forward, 3f))
        {
            if (Physics.Raycast(transform.position, cameraTransform.forward, 3f))
            {
                punchGroundSound.Play();
                rigidbody.AddExplosionForce(PunchBoostForce, transform.position + cameraTransform.forward * 5, 100f);
            }
        }
    }

    [PunRPC]
    public void DoWave()
    {
        animator.SetTrigger("DoWave");
        if (waveTarget != null)
        {
            waveTarget.Health = 100f;
        }
    }

    public void DoWaveLocal()
    {
        animator.SetTrigger("DoWave");
        if (waveTarget != null)
        {
            waveTarget.Health = 100f;
        }
    }

    //private void respawn()
    //{
    //    Step3_SpawnAndJoin._SpawnPlayer();

    //    //transform.position = GameObject.FindGameObjectWithTag("RESPAWN").transform.position;
    //    //transform.rotation = GameObject.FindGameObjectWithTag("RESPAWN").transform.rotation;

    //    //rigidbody.isKinematic = true;
    //    //StartCoroutine(_unKinematic());
    //}

    /// <summary>
    /// Single frame delay on unKinematicing
    /// </summary>
    /// <returns></returns>
    private IEnumerator _unKinematic()
    {
        yield return null;
        rigidbody.isKinematic = false;
    }

    public void ModHealth(float HPChange)
    {
        Health += HPChange;
        if(HPChange < 0)
        {
            previousPunchTime = Time.time;
            direSituation = true;
        }
    }
}
