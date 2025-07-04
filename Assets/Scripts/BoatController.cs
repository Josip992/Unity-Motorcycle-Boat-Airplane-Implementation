using UnityEngine;

public class BoatController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxForwardForce = 50f;
    public float maxReverseForce = 30f;
    public float throttleResponse = 5f;

    [Header("Steering Settings")]
    public Transform rudder;
    public float rudderAngle = 30f;
    public float turnForce = 10f;
    public float forwardSpeedThreshold = 0.1f;

    public bool isActive = false;
    private bool isCrashed = false;

    private Rigidbody rb;
    private float throttle = 0f;
    private float currentThrust = 0f;
    private float steeringInput;

    [SerializeField] private float maxEmissionRate = 50f;
    [SerializeField] private ParticleSystem[] exhaustSmokeParticles = new ParticleSystem[2];

    [SerializeField] Transform spawnPoint;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        if (isCrashed) return;

        ApplyThrust();

        handleSteering();

        UpdateSmokeEmission(Mathf.Abs(throttle));
    }

    private void HandleInput() 
    {
        throttle = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        if (Input.GetKeyDown(KeyCode.R)) ResetToSpawn();
    }

    private void ApplyThrust()
    {
        float targetThrust = 0f;

        if (throttle > 0f)
        {
            targetThrust = throttle * maxForwardForce;
        }
        else if (throttle < 0f)
        {
            targetThrust = throttle * maxReverseForce;
        }

        currentThrust = Mathf.MoveTowards(currentThrust, targetThrust, throttleResponse * Time.fixedDeltaTime);
        rb.AddForce(transform.forward * currentThrust);
    }

    private void handleSteering()
    {
        float forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);

        if (Mathf.Abs(forwardSpeed) <= forwardSpeedThreshold) return;

        if (rudder != null)
        {
            rudder.localRotation = Quaternion.Euler(0f, steeringInput * -rudderAngle, 0f);
        }

        float steeringDirection = Mathf.Sign(forwardSpeed);
        float steerTorque = steeringInput * turnForce * Mathf.Abs(forwardSpeed);
        rb.AddTorque(Vector3.up * steerTorque * steeringDirection);
    }

    private void ResetToSpawn()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        currentThrust = 0;
        rb.useGravity = false;
        isCrashed = false;

        foreach (var ps in exhaustSmokeParticles)
        {
            if (ps != null)
            { 
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var emission = ps.emission;
                emission.rateOverTime = 0f;
                ps.Play();
            }
        }
    }

    private void UpdateSmokeEmission(float throttleAmount)
    {
        foreach (var ps in exhaustSmokeParticles)
        {
            if (ps == null) continue;

            var emission = ps.emission;
            emission.rateOverTime = maxEmissionRate * throttleAmount;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            isCrashed = true;
            currentThrust = 0;
            rb.useGravity = true;
            Physics.gravity = new Vector3(0, -9.81f * 10, 0);

            foreach (var ps in exhaustSmokeParticles)
            {
                if (ps != null)
                    ps.Stop();
            }
        }
    }
}
