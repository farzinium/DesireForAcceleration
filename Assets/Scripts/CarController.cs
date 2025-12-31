using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float maxMotorTorque = 1500f;
    public float maxSteeringAngle = 30f;
    public float brakeTorque = 3000f;
    public float driftFactor = 0.95f; // Lower = more drift
    public float reverseSpeed = 1000f;

    [Header("Wheel References")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    [Header("Suspension Settings")]
    public float suspensionDistance = 0.2f;
    public float suspensionSpring = 30000f;
    public float suspensionDamper = 4500f;

    [Header("Gearing")]
    public int maxGears = 5;
    public float[] gearRatios = { 3.2f, 2.4f, 1.8f, 1.3f, 1.0f };
    public float finalDriveRatio = 3.0f;
    public float shiftCooldown = 0.4f;

    private int currentGear = 1; // 1-based (1 = first gear)
    private bool isReverse = false;
    private float lastShiftTime;


    private Rigidbody rb;

    private float currentSteerAngle = 0f;
    private float currentMotorTorque = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0); // Lower center of mass for better stability
        SetupWheelColliders();
    }

    void Update()
    {
        HandleInput();
        UpdateWheelVisuals();
    }

    private void FixedUpdate()
    {
        ApplyMotor();
        ApplySteering();
        //ApplyDrift();
    }

    void HandleInput()
    {
        // Steering
        currentSteerAngle = maxSteeringAngle * Input.GetAxis("Horizontal");

        // Gear shifting
        if (Time.time - lastShiftTime > shiftCooldown)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ShiftUp();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                ShiftDown();
            }
        }

        // Throttle / Brake
        float throttle = 0f;

        if (Input.GetKey(KeyCode.W))
            throttle = 1f;
        else if (Input.GetKey(KeyCode.S))
            throttle = -1f;

        // Reverse override
        if (isReverse)
            currentMotorTorque = -reverseSpeed * throttle;
        else
            currentMotorTorque = maxMotorTorque * throttle * GetCurrentGearRatio();
    }

    void ShiftUp()
    {
        if (isReverse)
        {
            isReverse = false;
            currentGear = 1;
        }
        else if (currentGear < maxGears)
        {
            currentGear++;
        }

        lastShiftTime = Time.time;
    }

    void ShiftDown()
    {
        if (!isReverse && currentGear > 1)
        {
            currentGear--;
        }
        else if (!isReverse && currentGear == 1)
        {
            isReverse = true;
        }

        lastShiftTime = Time.time;
    }

    float GetCurrentGearRatio()
    {
        int index = Mathf.Clamp(currentGear - 1, 0, gearRatios.Length - 1);
        return gearRatios[index] * finalDriveRatio;
    }


    void ApplyMotor()
    {
        rearLeftCollider.motorTorque = currentMotorTorque;
        rearRightCollider.motorTorque = currentMotorTorque;
    }
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20),
            isReverse ? "Gear: R" : $"Gear: {currentGear}");
    }

    void ApplySteering()
    {
        frontLeftCollider.steerAngle = currentSteerAngle;
        frontRightCollider.steerAngle = currentSteerAngle;
    }

    void ApplyDrift()
    {
        Vector3 forwardVel = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
        Vector3 rightVel = driftFactor * Vector3.Dot(rb.linearVelocity, transform.right) * transform.right;
        rb.linearVelocity = forwardVel + rightVel;
    }

    void UpdateWheelVisuals()
    {
        UpdateWheel(frontLeftCollider, frontLeftWheel);
        UpdateWheel(frontRightCollider, frontRightWheel);
        UpdateWheel(rearLeftCollider, rearLeftWheel);
        UpdateWheel(rearRightCollider, rearRightWheel);
    }

    void UpdateWheel(WheelCollider collider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    void SetupWheelColliders()
    {
        WheelCollider[] wheels = { frontLeftCollider, frontRightCollider, rearLeftCollider, rearRightCollider };
        foreach (WheelCollider wc in wheels)
        {
            JointSpring spring = wc.suspensionSpring;
            spring.spring = suspensionSpring;
            spring.damper = suspensionDamper;
            wc.suspensionSpring = spring;
            wc.suspensionDistance = suspensionDistance;
        }
    }
}
