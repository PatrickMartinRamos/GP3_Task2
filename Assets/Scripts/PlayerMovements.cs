using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    //public Var
    [SerializeField] float moveSpeed;
    [SerializeField] float raycastDistance = 10f;
    [SerializeField] Transform headPos;

    //input System
    PlayerInputs playerInputs;

    //movements
    Vector2 runAction;

    //interact
    bool isPickUp = false;
    bool isPutDown = false;

    //system var
    [SerializeField] bool isHoldingFruit = false;
    private GameObject heldFruit;

    //text UI
    public TextMeshProUGUI pickUPTxt;
    public TextMeshProUGUI putDownTxt;

    private Rigidbody rb;

    public void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    private void OnEnable()
    {
        if (playerInputs == null)
        {
            playerInputs = new PlayerInputs();

            playerInputs.movementInputs.Move.performed += i => runAction = i.ReadValue<Vector2>();
            playerInputs.movementInputs.Move.canceled += i => runAction = Vector2.zero;

            playerInputs.Interact.pickUp.performed += i => isPickUp = true;
            playerInputs.Interact.pickUp.canceled += i => isPickUp = false;

            playerInputs.Interact.putDown.performed += i => isPutDown = true;
            playerInputs.Interact.putDown.canceled += i => isPutDown = false;
        }
        playerInputs.Enable();
    }

    private void OnDisable()
    {
        if (playerInputs != null)
        {
            playerInputs.movementInputs.Move.performed -= i => runAction = i.ReadValue<Vector2>();

            playerInputs.Interact.pickUp.performed -= i => isPickUp = true;
            playerInputs.Interact.pickUp.canceled -= i => isPickUp = false;

            playerInputs.Interact.putDown.performed -= i => isPutDown = true;
            playerInputs.Interact.putDown.canceled -= i => isPutDown = false;
        }
        playerInputs.Disable();
    }

    private void Update()
    {
        MovePlayer();
        handlepickupandputdown();

        if (isHoldingFruit)
        {
            // Update the position of the held fruit to match the player's hand position
            heldFruit.transform.position = headPos.position + Camera.main.transform.forward * 3.5f;
            Debug.Log("Currently holding: " + heldFruit.name);
        }
    }

    void MovePlayer()
    {
        Vector3 movement = new Vector3(runAction.x, 0f, runAction.y);

        // Use the camera's forward vector to move in the camera's direction
        Vector3 moveDir = Camera.main.transform.forward * movement.z + Camera.main.transform.right * movement.x;
        moveDir.y = 0; //movement is only in the horizontal plane

        rb.velocity = new Vector3(moveDir.x * moveSpeed, rb.velocity.y, moveDir.z * moveSpeed);
    }

    private void handlepickupandputdown()
    {
        // Cast a ray from the player head position in the direction they are looking
        Ray ray = new Ray(headPos.position, Camera.main.transform.forward);
        RaycastHit hit;

        // Check if the ray hits something
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            // Visualize the ray as a line
            Debug.DrawLine(ray.origin, hit.point, Color.green);

            Debug.Log("Hit object: " + hit.collider.gameObject.name);
            
            if (hit.collider.CompareTag("Fruit") && !isHoldingFruit)
            {
                pickUPTxt.gameObject.SetActive(true);
                putDownTxt.gameObject.SetActive(false);
                // Check if the pick up action is performed
                if (isPickUp)
                {
                    Debug.Log("Fruit picked up: " + hit.collider.gameObject.name);

                    // Set the player as holding a fruit
                    isHoldingFruit = true;
                    heldFruit = hit.collider.gameObject;

                    heldFruit.GetComponent<Rigidbody>().isKinematic = true;

                    pickUPTxt.gameObject.SetActive(false);
                    putDownTxt.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            // If the ray doesn't hit anything, visualize it as a line up to the specified distance
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * raycastDistance, Color.green);
            pickUPTxt.gameObject.SetActive(false);
            putDownTxt.gameObject.SetActive(false);
        }

        // Check if the put down action is performed
        if (isPutDown)
        {
            // If the player is holding a fruit, put it down
            if (isHoldingFruit)
            {
                isHoldingFruit = false;
                heldFruit.GetComponent<Rigidbody>().isKinematic = false;
                Debug.Log("Fruit put down");

                heldFruit = null;

                pickUPTxt.gameObject.SetActive(false);
                putDownTxt.gameObject.SetActive(true);
            }
            else
            {
                putDownTxt.gameObject.SetActive(false);
                Debug.Log("Cannot put down. Player is not holding a fruit.");
            }
        }
    }

}
