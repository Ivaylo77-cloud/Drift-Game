using UnityEngine;

public class CarController : MonoBehaviour
{
    public Rigidbody sphereRB;

    public Vector3 startPosition;

    public float fwdSpeed = 50f;
    public float revSpeed = 30f;
    public float turnSpeed = 100f;

    public LayerMask groundLayer;

    private bool isCarGrounded;

    public float alignToGroundTime = 5f;

    private float normalDrag;
    public float modifiedDrag = 2f;

    private CarControls controls;

    private float throttleInput; // 0 → 1
    private float brakeInput;    // 0 → 1
    private float turnInput;     // -1 → 1



    void Awake()
    {
        controls = new CarControls();
    }

    void Start()
    {
        sphereRB.transform.parent = null;
        normalDrag = sphereRB.linearDamping;
    }

    void OnEnable()
    {
        controls.Enable();

        // Steering
        controls.Driving.Steer.performed += ctx => turnInput = ctx.ReadValue<float>();
        controls.Driving.Steer.canceled += ctx => turnInput = 0f;

        // Throttle (accelerator pedal)
        controls.Driving.Throttle.performed += ctx => throttleInput = ctx.ReadValue<float>();
        controls.Driving.Throttle.canceled += ctx => throttleInput = 0f;

        // Brake pedal
        controls.Driving.Brake.performed += ctx => brakeInput = ctx.ReadValue<float>();
        controls.Driving.Brake.canceled += ctx => brakeInput = 0f;
    }

    void OnDisable()
    {
        controls.Disable();
    }



    void Update()
    {
        // Ground check
        RaycastHit hit;
        isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);

        // Align to ground
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

        // Steering
        if (isCarGrounded)
        {
            float turn = turnInput * turnSpeed * Time.deltaTime * Mathf.Clamp(throttleInput, 0.2f, 1f);
            transform.Rotate(0, turn, 0);
        }

        // Follow sphere
        transform.position = sphereRB.transform.position;

        // Drag
        sphereRB.linearDamping = isCarGrounded ? normalDrag : modifiedDrag;
    }

    void FixedUpdate()
    {
        // FINAL movement logic (IMPORTANT PART)

        float acceleration = throttleInput * fwdSpeed;
        float braking = brakeInput * revSpeed;

        float finalForce = acceleration - braking;

        sphereRB.AddForce(transform.forward * finalForce, ForceMode.Acceleration);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("death box"))
        {
            sphereRB.transform.position = startPosition;
        }
    }
}