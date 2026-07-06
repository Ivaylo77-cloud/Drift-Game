using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarController : MonoBehaviour
{
    public Rigidbody sphereRB;

    public Vector3 startPosition;
    
    public float fwdSpeed;
    public float revSpeed;
    public float turnSpeed;
    public LayerMask groundLayer;
    public float moveInput = 0.0f;
    public float turnInput = 0.0f;
    private bool isCarGrounded;
    public bool isCarFlipped;
    public bool isFlipping;
    float timeFlipped;
    [SerializeField] float timeBeforeFlip = 2f;
    [SerializeField] float flipSpeed = 2f;
    
    private float normalDrag;
    public float modifiedDrag;
    
    public float alignToGroundTime;

    void Start()
    {
        // Detach Sphere from car
        sphereRB.transform.parent = null;
        normalDrag = sphereRB.linearDamping;
        timeFlipped = 0f;
    }
    
    void Update()
    {
        if (FindObjectOfType<GameManager>().inputType == InputType.keyboard)
        {
            // Get Input
            moveInput = Input.GetAxisRaw("Vertical");
            turnInput = Input.GetAxisRaw("Horizontal");
        }
        moveInput = Mathf.Clamp(moveInput, -1.0f, 1.0f);

        // Calculate Turning Rotation
        float newRot = turnInput * turnSpeed * Time.deltaTime * moveInput;

        if (isCarGrounded)
            transform.Rotate(0, newRot, 0, Space.World);

        // Set Cars Position to Our Sphere
        transform.position = sphereRB.transform.position;
        // Raycast to the ground and get normal to align car with it.
        RaycastHit hit;
        RaycastHit upHit;
        isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);

        if(!isCarFlipped)
        {
            isCarFlipped = Physics.Raycast(transform.position, transform.up, out upHit, 1f, groundLayer);
        }
        
        
        // Rotate Car to align with ground
        Quaternion toRotateTo = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotateTo, alignToGroundTime * Time.deltaTime);
        
        // Calculate Movement Direction
        moveInput *= moveInput > 0 ? fwdSpeed : revSpeed;
        
        // Calculate Drag
        sphereRB.linearDamping = isCarGrounded ? normalDrag : modifiedDrag;

        if(isCarFlipped)
        {
            timeFlipped += Time.deltaTime;
            Debug.Log("Upside Down!");
        }
        else
        {
            timeFlipped = 0f;
        }

        if(timeFlipped > timeBeforeFlip)
        {
            isFlipping = true;
        }
        if(isFlipping)
        {
            SelfRight();
        }
    }
    private void FixedUpdate()
    {
        if (isCarGrounded)
            sphereRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration); // Add Movement
        else
            sphereRB.AddForce(Vector3.up * -400f * Time.deltaTime); // Add Gravity
    }

    void SelfRight()
    {
        float lerpFactor = (timeFlipped - timeBeforeFlip)/flipSpeed;
        
        if(timeFlipped - timeBeforeFlip < flipSpeed)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, 0f), 1.0f * Time.deltaTime);
            Debug.Log("self righting");
        }
        else
        {
            isFlipping = false;
            isCarFlipped = false;
            Debug.Log("Finished Flip!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "death box")
        {
            sphereRB.transform.position = startPosition;
        }
    }
}
