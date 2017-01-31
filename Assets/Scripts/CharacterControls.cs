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
    public Transform armsTransform;
    public Rigidbody rigidbody;
    public CapsuleCollider collider;
    public BoxCollider hitTrigger;
    public Animator animator;

    private AudioSource jetpackStartSound, jetpackLoopSound;
    private AudioSource punchSound, punchGroundSound;
    private AudioSource dieSound;

    public PhotonView myPhotonView;

    public List<CharacterControls> punchTargets;

    public int RespawnYLimit;


    public float MinPeaceTime;
    private float previousPunchTime;
    private bool direSituation;

    private bool isDead;

    public GameObject ArmLeft, ArmRight, Body, ArmCamLeft, ArmCamRight, Head;

    public float punchCooldown;
    private float lastPunchTime;

    public GameObject MusicBox1;
    public GameObject MusicBox2;

    public List<PhotonView> WaveCandidates;
    public List<int> WaveRecieved;
    public List<int> WaveSent;

    private List<int> enemyIDs;
    public List<int> FriendIDs;

    public GameObject FriendIcon;
    public GameObject EnemyIcon;

    private float previousYrot;

    private bool onGround;

    // Use this for initialization
    void Start()
    {
        //TODO: Fix sync issues. Removed arm, head, and body rigidbodies as a test. 
        //It was in fact all the rigidodies -____-
        onGround = false;

        punchTargets = new List<CharacterControls>();
        WaveCandidates = new List<PhotonView>();
        WaveRecieved = new List<int>();

        survivalTime = 0;
        respawnTime = 0;
        isDead = false;
        

        if (!myPhotonView.isMine)
        {
            cameraTransform.GetComponent<Camera>().enabled = false;//gameObject.SetActive(false);
            cameraTransform.GetComponent<AudioListener>().enabled = false;
            ArmCamLeft.SetActive(false);
            ArmCamRight.SetActive(false);
            gameObject.tag = "iPC";
        }
        else
        {
            GameObject.FindGameObjectWithTag("MUSIC").GetComponent<MusicManager>().MusicBox1 = MusicBox1.GetComponent<AudioSource>();
            GameObject.FindGameObjectWithTag("MUSIC").GetComponent<MusicManager>().MusicBox2 = MusicBox2.GetComponent<AudioSource>();
            MusicManager._SwitchTo(0);
            Debug.Log("My photon view, I am: " + myPhotonView.viewID);
            ArmLeft.SetActive(false);
            ArmRight.SetActive(false);
            Body.SetActive(false);
            Head.SetActive(false);
            gameObject.tag = "PC";
        }

        
        Cursor.lockState = CursorLockMode.Locked;
        jetpackFuel = MaxJetpackFuel;
        canJetpack = true;

        AudioSource[] sources = armsTransform.GetComponents<AudioSource>();
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


    private void FixedUpdate()
    {
        previousYrot = transform.rotation.eulerAngles.y;
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
                safeSituationActivate();
            }
        }

        // Wave/punch
        if (Input.GetButtonDown("Punch") && Time.time - previousPunchTime > punchCooldown)
        {
            myPhotonView.RPC("DoPunch", PhotonTargets.All);
        }
        else if (Input.GetButtonDown("Wave"))
        {
            //_Help.FirstTimeWaved();
            // Use energy, give boost
            if (jetpackFuel - PunchEnergy >= 0f)
            {
                if(WaveCandidates.Count > 0)
                {
                    for(int x = 0; x < WaveCandidates.Count; x++)
                    {
                        //WaveCandidates[x].RPC()
                    }
                }
                myPhotonView.RPC("DoWave", PhotonTargets.All);
            }
        }

        #region Mouse stuff
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

        newCameraRotX = armsTransform.rotation.eulerAngles.x - mouseChange.y * MouseSensitivity * Time.deltaTime;
        if (newCameraRotX > 90.0f && newCameraRotX < 180.0f)
            newCameraRotX = 90.0f;
        if (newCameraRotX < 270.0f && newCameraRotX > 180.0f)
            newCameraRotX = 270.0f;
        newCameraRotY = armsTransform.rotation.eulerAngles.y + mouseChange.x * MouseSensitivity * Time.deltaTime;
        armsTransform.rotation = Quaternion.Euler(newCameraRotX, newCameraRotY, armsTransform.rotation.eulerAngles.z);
        #endregion

        // Check if on ground
        if (!onGround && Physics.Raycast(transform.position + Vector3.down * 0.9f, Vector3.down, 0.5f))
        {
            fixNetGuessWork();
            onGround = true;
        }
        else
        {
            onGround = Physics.Raycast(transform.position + Vector3.down * 0.9f, Vector3.down, 0.5f);
        }
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
        ArmLeft.transform.SetParent(null);
        ArmLeft.SetActive(true);
        ArmLeft.GetComponent<Rigidbody>().isKinematic = false;
        ArmLeft.GetComponent<BoxCollider>().isTrigger = false;
        ArmRight.transform.SetParent(null);
        ArmRight.GetComponent<BoxCollider>().isTrigger = false;
        ArmRight.GetComponent<Rigidbody>().isKinematic = false;
        ArmRight.SetActive(true);
        Body.transform.SetParent(null);
        Body.GetComponent<Rigidbody>().isKinematic = false;
        Body.SetActive(true);
        Head.transform.SetParent(null);
        Head.GetComponent<Rigidbody>().isKinematic = false;
        Head.SetActive(true);
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
        Body.transform.SetParent(null);
        Body.GetComponent<Rigidbody>().isKinematic = false;
        Head.transform.SetParent(null);
        Head.GetComponent<Rigidbody>().isKinematic = false;

    }

    private void die()
    {
        myPhotonView.RPC("netDie", PhotonTargets.All);
        localDie();
    }

    /// <summary>
    /// Other players iPCs on your game.
    /// </summary>
    [PunRPC]
    public void DoPunch()
    {
        animator.SetTrigger("DoPunch");
        previousPunchTime = Time.time;
        if (punchTargets.Count > 0)
        {
            for(int x = 0; x < punchTargets.Count; x++)
            {
                punchGroundSound.Play();
                punchTargets[x].ModHealth(-PunchDamage);
                //punchTarget.rigidbody.AddExplosionForce(PunchBoostForce, punchTarget.rigidbody.position + punchTarget.cameraTransform.forward * 5, 100f);
                if (punchTargets[x].myPhotonView.isMine)
                {
                    jetpackFuel = Mathf.Max(jetpackFuel - PunchEnergy, 0f);
                    rigidbody.AddExplosionForce(PunchBoostForce, transform.position + cameraTransform.forward * 5, 100f);
                    rigidbody.velocity += punchTargets[x].rigidbody.velocity;
                    fixNetGuessWork();
                    CBUG.Do("I GOT PUNCHED!");
                }
                
            }
        }else if (Physics.Raycast(transform.position, cameraTransform.forward, 3f))
        {
            rigidbody.AddExplosionForce(PunchBoostForce, transform.position + cameraTransform.forward * 5, 100f);
        }
    }

    [PunRPC]
    public void DoWave()
    {
        //PC -> 1. Waves to WaveCandidates -> 2a. If(WaveCan in WaveReci) marksCalls WaveCan's ReceiveWave RPC ->

        //iPC  -> 3. ReceiveWave records Waver's ID -> iPC Waves to WaveCandidates 
        animator.SetTrigger("DoWave");
        if (WaveCandidates.Count > 0)
        {
            for(int x = 0; x < WaveCandidates.Count; x++)
            {
                //If they already waved at us
                if (WaveRecieved.Contains(WaveCandidates[x].viewID))
                {
                    //MAKE FRIENDS
                    FriendIDs.Add(WaveCandidates[x].viewID);
                    WaveRecieved.Remove(WaveCandidates[x].viewID);
                    //On local machine, if the friend is me
                    if (WaveCandidates[x].isMine)
                    {
                        //Make their icon appear
                        FriendIcon.SetActive(true);
                        EnemyIcon.SetActive(false);
                        //Me
                        WaveCandidates[x].transform.GetComponent<CharacterControls>().FriendIDs.Add(WaveCandidates[x].viewID);
                    }
                }
                else
                {
                    WaveCandidates[x].RPC("ReceiveWave", PhotonTargets.All, myPhotonView.viewID);
                    //if(!FriendIDs.Contains(WaveCandidates[x].viewID))
                    //    WaveRecieved.Add(WaveCandidates[x].viewID);
                }

            }
            //waveTarget.Health = 100f;

        }
    }

    [PunRPC]
    public void ReceiveWave(int viewID_received)
    {
        WaveRecieved.Add(viewID_received);
    }

    //[PunRPC]
    //public void ConfirmWave(int viewID_confirmed)
    //{
    //    FriendIcon.SetActive(true);
    //}

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
        if(HPChange < 0 && myPhotonView.isMine)
        {
            direSituationActivate();
        }
    }

    /// <summary>
    /// Activated whenever PC gets punched by iPC.
    /// </summary>
    private void direSituationActivate()
    {
        MusicManager._SwitchTo(1);
        EnemyIcon.SetActive(true);
        FriendIcon.SetActive(false);
        direSituation = true;
        previousPunchTime = Time.time;
    }

    private void safeSituationActivate()
    {
        MusicManager._SwitchTo(1);
        EnemyIcon.SetActive(false);
        direSituation = false;
    }

    private void fixNetGuessWork()
    {
        GetComponent<PhotonTransformView>().SetSynchronizedValues(rigidbody.velocity,
                (transform.rotation.eulerAngles.y - previousYrot) / Time.fixedDeltaTime);
        CBUG.Do("FIXING SYNC VALUES");
    }
}
