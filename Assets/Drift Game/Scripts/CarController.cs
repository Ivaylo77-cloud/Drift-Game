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
    public bool isFlipping;

    float timeFlipped;
    [SerializeField] float timeBeforeFlip = 2f;
    [SerializeField] float flipSpeed = 2f;

    private float normalDrag;
    public float modifiedDrag;

    public float alignToGroundTime;

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

        controls.Driving.Throttle.performed += ctx => moveInput = ctx.ReadValue<float>();
        controls.Driving.Throttle.canceled += ctx => moveInput = 0f;

        controls.Driving.Brake.performed += ctx =>
        {
            float brake = ctx.ReadValue<float>();

            // Ignore tiny values at rest.
            if (brake < 0.1f)
            {
                moveInput = 0f;
                return;
            }

            moveInput = -brake;
        };

        controls.Driving.Brake.canceled += ctx =>
        {
            moveInput = 0f;
        };
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        moveInput = Mathf.Clamp(moveInput, -1f, 1f);

        float newRot = turnInput * turnSpeed * Time.deltaTime * moveInput;

        if (isCarGrounded)
            transform.Rotate(0, newRot, 0, Space.World);

        transform.position = sphereRB.transform.position;

        RaycastHit hit;
        RaycastHit upHit;

        isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);

        if (!isCarFlipped)
        {
            isCarFlipped = Physics.Raycast(transform.position, transform.up, out upHit, 1f, groundLayer);
        }

        // Align to ground (safe check added)
        if (isCarGrounded)
        {
            Quaternion toRotateTo =
                Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                toRotateTo,
                alignToGroundTime * Time.deltaTime
            );
        }

        // Speed calculation FIX (don’t overwrite input)
        float speed = moveInput > 0 ? fwdSpeed : revSpeed;
        float finalMove = moveInput * speed;

        sphereRB.linearDamping = isCarGrounded ? normalDrag : modifiedDrag;

        // Flip detection
        if (isCarFlipped)
        {
            timeFlipped += Time.deltaTime;
        }
        else
        {
            timeFlipped = 0f;
        }

        if (timeFlipped > timeBeforeFlip)
        {
            isFlipping = true;
        }

        if (isFlipping)
        {
            SelfRight();
        }
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

    void SelfRight()
    {
        if (timeFlipped - timeBeforeFlip < flipSpeed)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, 0f, 0f),
                Time.deltaTime * 2f
            );
        }
        else
        {
            isFlipping = false;
            isCarFlipped = false;
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