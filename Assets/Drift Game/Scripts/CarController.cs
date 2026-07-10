using UnityEngine;

public class CarController : MonoBehaviour
{
    public Rigidbody sphereRB;

    public Vector3 startPosition;

    public float fwdSpeed;
    public float revSpeed;
    public float turnSpeed;
    public LayerMask groundLayer;

    public float moveInput;
    public float turnInput;

    private bool isCarGrounded;
    public bool isCarFlipped;


    float timeFlipped;
    [SerializeField] float timeBeforeFlip = 2f;
    [SerializeField] float flipSpeed = 2f;

    private float normalDrag;
    public float modifiedDrag;

    public float alignToGroundTime;

    public ParticleSystem leftSmoke;
    public ParticleSystem rightSmoke;

    private CarControls controls;

    void Awake()
    {
        controls = new CarControls();
    }

    void Start()
    {
        sphereRB.transform.parent = null;
        normalDrag = sphereRB.linearDamping; // FIX: use drag (not linearDamping)
        timeFlipped = 0f;
    }

    void OnEnable()
    {
        controls.Enable();

        controls.Driving.Steer.performed += ctx => turnInput = ctx.ReadValue<float>();
        controls.Driving.Steer.canceled += ctx => turnInput = 0f;

        
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        float throttle = controls.Driving.Throttle.ReadValue<float>();
        float brake = controls.Driving.Brake.ReadValue<float>();

        moveInput = Mathf.Clamp(throttle - brake, -1f, 1f);

        turnInput = controls.Driving.Steer.ReadValue<float>();


        moveInput = Mathf.Clamp(moveInput, -1f, 1f);

        float newRot = turnInput * turnSpeed * Time.deltaTime * moveInput;

        if (isCarGrounded)
            transform.Rotate(0, newRot, 0, Space.World);

        transform.position = sphereRB.transform.position;

        RaycastHit hit;
        RaycastHit upHit;

        isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);

        



        // Speed calculation FIX (don’t overwrite input)
        float speed = moveInput > 0 ? fwdSpeed : revSpeed;
        float finalMove = moveInput * speed;

        sphereRB.linearDamping = isCarGrounded ? normalDrag : modifiedDrag;

        float carSpeed = sphereRB.linearVelocity.magnitude;

        bool drifting = Mathf.Abs(turnInput) > 0.5f && carSpeed > 12f;

        if (drifting)
        {
            if (!leftSmoke.isPlaying)
                leftSmoke.Play();

            if (!rightSmoke.isPlaying)
                rightSmoke.Play();
        }
        else
        {
            if (leftSmoke.isPlaying)
                leftSmoke.Stop();

            if (rightSmoke.isPlaying)
                rightSmoke.Stop();
        }

        // Flip detection



    }

    void FixedUpdate()
    {
        float speed = moveInput > 0 ? fwdSpeed : revSpeed;
        float finalMove = moveInput * speed;

        if (isCarGrounded)
        {
            sphereRB.AddForce(transform.forward * finalMove, ForceMode.Acceleration);
        }
        else
        {
            sphereRB.AddForce(Vector3.down * 400f * Time.deltaTime);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("death box"))
        {
            sphereRB.transform.position = startPosition;
        }
    }
}