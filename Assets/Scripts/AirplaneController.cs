using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    public bool isActive = false;
    [SerializeField] public float throttleIncrement = 0.1f;
    [SerializeField] public float maxThrust = 200f;
    [SerializeField] public float responsiveness = 10f;
    [SerializeField] public float lift = 135f;
    [SerializeField] public Transform propeller;
    [SerializeField] public float dragCoefficient = 0.1f;
    [SerializeField] public float turbulenceStrength = 5f;
    [SerializeField] public float turbulenceSpeedFactor = 50f;

    [Header("Respawn")]
    [SerializeField] Transform spawnPoint;

    Rigidbody rb;
    private float throttle;
    private float roll;
    private float pitch;
    private float yaw;

    private bool isGrounded = false;
    private bool isCrashed = false;

    [Header("Ground Collision Settings")]
    [SerializeField] private float groundSlowdownFactor = 0.1f;
    [SerializeField] private float crashFallSpeed = 10f;
    [SerializeField] private float gravityMultiplier = 2f;
    private Vector3 originalGravity;

    private float responseModifier
    {
        get
        {
            return rb.mass / 10f * responsiveness;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalGravity = Physics.gravity;
    }

    private void Update()
    {
        HandleInput();
        HandleSteering();
    }

    private void FixedUpdate()
    {
        if (!isActive || isCrashed) return;

        ApplyThrustAndControlForces();
        ApplyLiftForce();
        ApplyDragForce();
        ApplyTurbulenceForce();
    }

    private void HandleInput()
    {
        roll = Input.GetAxis("Horizontal");
        pitch = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.R)) ResetToSpawn();

        if (isCrashed)
        {
            if (Input.GetKeyDown(KeyCode.R))
                ResetToSpawn();
            return;
        }

        if (Input.GetKey(KeyCode.Space)) throttle += throttleIncrement;
        else if (Input.GetKey(KeyCode.LeftControl)) throttle -= throttleIncrement;

        if (Input.GetKey(KeyCode.Q)) yaw = -1f;
        else if (Input.GetKey(KeyCode.E)) yaw = 1f;
        else yaw = 0f;

        throttle = Mathf.Clamp(throttle, 0f, 100f);
    }

    private void HandleSteering()
    {
        if (isGrounded && throttle > 80f)
        {
            rb.AddTorque(-transform.right * 0.25f * responseModifier);
        }
        if (!isCrashed) propeller.transform.Rotate(0f, 0f, throttle * 100f * Time.deltaTime);     
    }

    private void ApplyThrustAndControlForces()
    {
        rb.AddForce(transform.forward * maxThrust * throttle);
        rb.AddTorque(transform.up * yaw * responseModifier);
        rb.AddTorque(transform.right * pitch * responseModifier);
        rb.AddTorque(-transform.forward * roll * responseModifier);
    }

    private void ApplyLiftForce()
    {
        Vector3 velocityDir = rb.velocity.normalized;
        Vector3 forwardDir = transform.forward;
        float angle = Vector3.Angle(forwardDir, velocityDir);
        float aoaFactor = Mathf.Sin(angle * Mathf.Deg2Rad);
        float liftForce = Mathf.Pow(rb.velocity.magnitude, 2) * lift * aoaFactor;
        liftForce = Mathf.Clamp(liftForce, 0f, 5000f);

        rb.AddForce(transform.up * liftForce);
    }

    private void ApplyDragForce()
    {
        Vector3 dragForce = -rb.velocity.normalized * Mathf.Pow(rb.velocity.magnitude, 2) * dragCoefficient;
        rb.AddForce(dragForce);
    }

    private void ApplyTurbulenceForce()
    {
        float turbulenceFactor = rb.velocity.magnitude / turbulenceSpeedFactor;
        turbulenceFactor = Mathf.Clamp01(turbulenceFactor);

        Vector3 randomForce = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * turbulenceStrength * turbulenceFactor;

        rb.AddForce(randomForce);
    }

    private void ResetToSpawn()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        throttle = 0;
        isCrashed = false;
        Physics.gravity = originalGravity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            rb.velocity = rb.velocity * (1 - groundSlowdownFactor);
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {          
            isCrashed = true;

            rb.useGravity = true;  

            rb.velocity = new Vector3(rb.velocity.x, -crashFallSpeed, rb.velocity.z);

            Physics.gravity = new Vector3(0, -9.81f * gravityMultiplier, 0);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}

