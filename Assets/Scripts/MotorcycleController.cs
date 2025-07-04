using UnityEngine;

public class MotorcycleController : MonoBehaviour
{
    public bool isActive = false;
    [SerializeField] public float moveForce = 10f;
    [SerializeField] public float brakeForce = 20f;
    [SerializeField] public float maxSpeed = 10f;
    [SerializeField] public float turnSpeed = 100f;

    [Header("Leaning")]
    [SerializeField] public float leanAngle = 15f;
    [SerializeField] public float leanSmoothing = 5f;
    [SerializeField] private Transform leanPivot;

    [Header("Respawn")]
    [SerializeField] Transform spawnPoint;

    Rigidbody rb;
    private float verticalInput;
    private float steeringInput;

    private bool HandBraking;
    [SerializeField] public Transform rearWheelTransform;
    [SerializeField] public float handbrakeStrength = 500f;
    [SerializeField] public float rearSlipMultiplier = 0.9f;


    private bool isFlipped = false;

    [SerializeField] public float dragCoefficient = 0.1f;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        HandleInput();

        if (!isFlipped)
        {
            UpdateLeaning();
        }
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        if (isFlipped) return;

        HandleMovement();
        handleSteering();
    }

    private void HandleInput()
    {
        verticalInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        HandBraking = Input.GetKey(KeyCode.Space);

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetToSpawn();
        }

        if (isFlipped && Input.GetKeyDown(KeyCode.R))
        {
            ResetToSpawn();
        }
    }

    private void HandleMovement()
    {
        if (HandBraking)
        {
            ApplyHandbrake();
        }

        if (rb.velocity.magnitude < maxSpeed && verticalInput > 0)
        {
            rb.AddForce(transform.forward * verticalInput * moveForce, ForceMode.Acceleration);
        }
        else if (verticalInput < 0)
        {
            rb.AddForce(transform.forward * verticalInput * moveForce, ForceMode.Acceleration);
        }

        if (verticalInput == 0 && rb.velocity.magnitude > 0)
        {
            float dragForce = Mathf.Lerp(0f, brakeForce, dragCoefficient);
            rb.AddForce(-rb.velocity.normalized * dragForce, ForceMode.Acceleration);
        }
    }

    private void handleSteering()
    {
        if (rb.velocity.magnitude > 0.1f)
        {
            float turnAmount = steeringInput * turnSpeed * Time.fixedDeltaTime;
            Quaternion turnOffset = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * turnOffset);
        }
    }

    private void UpdateLeaning()
    {   
        if (leanPivot == null) return;

        float speedFactor = Mathf.Clamp01(rb.velocity.magnitude / maxSpeed);
        float targetZ = -steeringInput * leanAngle * speedFactor;

        Vector3 currentEuler = leanPivot.localEulerAngles;
        float currentZ = (currentEuler.z > 180f) ? currentEuler.z - 360f : currentEuler.z;
        float newZ = Mathf.Lerp(currentZ, targetZ, Time.deltaTime * leanSmoothing);

        leanPivot.localRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, newZ);
    }

    private void FlipOver()
    {
        isFlipped = true;

        rb.constraints = RigidbodyConstraints.None;
        rb.centerOfMass = new Vector3(3f, 0f, 0f); 
        rb.AddTorque(Random.onUnitSphere * 150f); 
    }

    private void ResetToSpawn()
    {
        isFlipped = false;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.centerOfMass = new Vector3(0f, -1f, 0f);
    }

    private void ApplyHandbrake()
    {
        Vector3 rearWheelWorldPos = rearWheelTransform.position;
        Vector3 brakeForce = -transform.forward * handbrakeStrength;
        rb.AddForceAtPosition(brakeForce, rearWheelWorldPos);

        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        localVelocity.x *= rearSlipMultiplier;
        Vector3 modifiedVelocity = transform.TransformDirection(localVelocity);

        rb.velocity = new Vector3(modifiedVelocity.x, rb.velocity.y, modifiedVelocity.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            FlipOver();
        }
    }
}
