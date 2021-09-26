using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {
    private Transform cam;
    private World world;

    public bool isGrounded;
    public bool isSprinting;

    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public float mouseHorizontalSensitivity = 2f;
    public float mouseVerticalSensitivity = 2f;
    public float maxLookUp = 80f;
    public float maxLookDown = -80f;

    public float playerWidth = 0.15f;
    public float playerHeight = 1.8f;

    public float jetpackFlightForce = 0.1f;
    public int jetpackFuelMax = 100;
    public int jetpackFuelCost = 10;
    public int jetpackFuelRefillRate = 1;
    private int jetpackFuelCurrent = 100;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum;
    private bool jumpRequest;
    private bool jetpackActive;
    private float xRotation = 0f;

    public Transform highlightBlock;
    public Transform placementHighlight;

    public float checkIncrement = 0.1f;
    public float reach = 8f;

    public Text jetpackFuelIndicator;
    //public Text selectedBlockText;
    public byte selectedBlockIndex = 1;

    private void Start () {
        cam = GameObject.Find ("Main Camera").transform;
        world = GameObject.Find ("World").GetComponent<World>();

        Cursor.lockState = CursorLockMode.Locked;
        //selectedBlockText.text = world.blocktypes [selectedBlockIndex].blockName + " block selected";
        jetpackFuelIndicator.text = "Fuel: " + jetpackFuelCurrent + "/" + jetpackFuelMax;
    }

    private void FixedUpdate () {
        CalculateVelocity ();

        if (jumpRequest)
            Jump ();

        if (jetpackActive)
            JetpackFlight ();

        if (!jetpackActive && jetpackFuelCurrent != jetpackFuelMax)
            jetpackFuelCurrent += jetpackFuelRefillRate;
            

        transform.Rotate (Vector3.up * mouseHorizontal * mouseHorizontalSensitivity);

        xRotation -= mouseVertical * mouseVerticalSensitivity;
        xRotation = Mathf.Clamp (xRotation, maxLookDown, maxLookUp);
        cam.localRotation = Quaternion.Euler (xRotation, 0f, 0f);

        transform.Translate (velocity, Space.World);       
    }
    private void Update () {
        GetPlayerInput ();
        PlaceCursorBlocks ();

        jetpackFuelIndicator.text = "Fuel: " + jetpackFuelCurrent + "/" + jetpackFuelMax;
    }

    private void CalculateVelocity () {
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        
        velocity +=Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)   
            velocity.y = CheckDownSpeed (velocity.y);
        else if (velocity.y > 0)
            velocity.y = CheckUpSpeed (velocity.y);
    }

    void Jump () {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void JetpackFlight () {
        jetpackFuelCurrent -= jetpackFuelCost;
        verticalMomentum += jetpackFlightForce;
        jetpackActive = false;
    }

    private void GetPlayerInput () {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if (isGrounded && Input.GetButton ("Jump"))
            jumpRequest = true;

        if (!isGrounded && jetpackFuelCurrent > jetpackFuelCost && Input.GetButton ("Sprint"))
            jetpackActive = true;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (highlightBlock.gameObject.activeSelf) {
            // left click (destroy)
            if (Input.GetMouseButtonDown (0))
                world.GetChunkFromVector3 (highlightBlock.position).EditVoxel (highlightBlock.position, 0);
            // right click (place)
            if (Input.GetMouseButtonDown (1))
                world.GetChunkFromVector3 (placementHighlight.position).EditVoxel (placementHighlight.position, selectedBlockIndex);
        }
    }

    private void PlaceCursorBlocks () {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3 ();

        while (step < reach) {
            Vector3 pos = cam.position + (cam.forward * step);

            if (world.CheckForVoxel (pos)) {
                highlightBlock.position = new Vector3 (Mathf.FloorToInt (pos.x), Mathf.FloorToInt (pos.y), Mathf.FloorToInt (pos.z));
                placementHighlight.position = lastPos;

                highlightBlock.gameObject.SetActive (true);
                placementHighlight.gameObject.SetActive (true);

                return;
            }

            lastPos = new Vector3 (Mathf.FloorToInt (pos.x), Mathf.FloorToInt (pos.y), Mathf.FloorToInt (pos.z));
            step += checkIncrement; 
        }

        highlightBlock.gameObject.SetActive (false);
        placementHighlight.gameObject.SetActive (false);
    }

    private float CheckDownSpeed (float downSpeed) {
        if (
            world.CheckForVoxel (new Vector3 (transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel (new Vector3 (transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel (new Vector3 (transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel (new Vector3 (transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
        ) {
            isGrounded = true;
            return 0;
        } else {
            isGrounded = false;
            return downSpeed;
        }
    }

    private float CheckUpSpeed (float upSpeed) {
        if (
            world.CheckForVoxel (new Vector3 (transform.position.x - playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel (new Vector3 (transform.position.x + playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel (new Vector3 (transform.position.x + playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel (new Vector3 (transform.position.x - playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth))
        ) {
            return 0;
        } else {
            return upSpeed;
        }
    }

    public bool front {
        get {
            bool frontBlocked = false;

            for (int i = 0; i < Mathf.CeilToInt (playerHeight); i++) {
                if (world.CheckForVoxel (new Vector3 (transform.position.x, transform.position.y + i, transform.position.z + playerWidth)))
                    frontBlocked = true;
            }
            return frontBlocked;
        }
    }

    public bool back {
        get {
            bool backBlocked = false;
            
            for (int i = 0; i < Mathf.CeilToInt (playerHeight); i++) {
                if (world.CheckForVoxel (new Vector3 (transform.position.x, transform.position.y + (float)i, transform.position.z - playerWidth)))
                    backBlocked = true;
            }
            return backBlocked;
        }
    }

    public bool left {
        get {
            bool leftBlocked = false;
            
            for (int i = 0; i < Mathf.CeilToInt (playerHeight); i++) {
                if (world.CheckForVoxel (new Vector3 (transform.position.x - playerWidth, transform.position.y + (float)i, transform.position.z)))
                    leftBlocked = true;
            }
            return leftBlocked;
        }
    }

    public bool right {
        get {
            bool rightBlocked = false;
            
            for (int i = 0; i < Mathf.CeilToInt (playerHeight); i++) {
                if (world.CheckForVoxel (new Vector3 (transform.position.x + playerWidth, transform.position.y + (float)i, transform.position.z + playerWidth)))
                    rightBlocked = true;
            }
            return rightBlocked;
        }
    }
}
