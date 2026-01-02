using System;
using System.Diagnostics;
using UnityEngine;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Camera Speed Effects")]
    public float minFOV = 60f;
    public float maxFOV = 80f;
    public float maxSpeedForFOV = 60f; // m/s (~216 km/h)
    public float maxCameraBackOffset = 1.5f;
    public Camera playerCamera;
    public GameObject brakeLight;
    public Material braking, nonBraking;

    [Header("Car Settings")]
    public float driftFactor = 0.95f; // Lower = more drift

    [Header("Speedometer")]
    public bool useKMH = true;
    private float speed;
    
    [Header("Engine")]
    public float engineHighpitch = .2f;
    public float engineLowpitch = 4.0f;
    public float maxMotorTorque = 1500f;
    public float maxSteeringAngle = 30f;
    public float reverseTorque = 1500f;
    public float minRPM = 800f;
    public float maxRPM = 7000f;
    public float redlineRPM = 6500f;
    public AnimationCurve torqueCurve; // RPM (0–1) -> Torque (0–1)

    private float engineRPM;
    private float deltaTime;
    [Header("Audio")]
    public AudioSource engineSource;
    public AudioSource tireSlipSource;


    [Header("Wheel References")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    [Header("Brake")]
    public float brakeTorque = 3000f; 
    [Tooltip("0 = Rear brakes only, 1 = Front brakes only")]
    [Range(0f, 1f)]
    public float brakeBias = 0.5f;
    public float handbrakeTorque = 6000f;
    private bool handbrake;

    [Header("Suspension Settings")]
    public float suspensionDistance = 0.2f;
    public float suspensionSpring = 30000f;
    public float suspensionDamper = 4500f;

    [Header("Gearing")]
    public int maxGears = 5;
    public float[] gearRatios = { 3.2f, 2.4f, 1.8f, 1.3f, 1.0f };
    public float finalDriveRatio = 3.0f;
    public float shiftCooldown = 0.4f;

    [Header("Nitrous")]
    public float nitroMultiplier = 1.5f;
    public float nitroDuration = 5f;
    public float nitroCooldown = 8f;

    private float nitroTimer;
    private bool nitroActive;

    private int currentGear = 1; // 1-based (1 = first gear)
    private bool isReverse = false;
    private float lastShiftTime;


    private Rigidbody rb;

    private float currentSteerAngle = 0f;
    private float currentMotorTorque = 0f;
    private int throttle = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0); // Lower center of mass for better stability
        SetupWheelColliders(); 
        nitroTimer = nitroDuration;
    }

    void Update()
    {
        HandleInput();
        UpdateWheelVisuals();
        ApplySteering(); 
        HandleEngineSound();
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void FixedUpdate()
    {
        CalculateEngineRPM();
        CalculateSpeed();
        ApplyMotor();
        ApplyHandbrake(); 
        HandleTireSounds();
        //ApplyDrift();
    }

    void CalculateSpeed()
    {
        speed = rb.linearVelocity.magnitude;
    }

    void HandleEngineSound()
    {
        float rpm01 = engineRPM / maxRPM;

        engineSource.pitch = Mathf.Lerp(engineLowpitch, engineHighpitch, rpm01);
        engineSource.volume = Mathf.Lerp(0.4f, 1f, rpm01);
    }

    void HandleTireSounds()
    {
        WheelHit hit;
        bool slipping = false;

        if (rearLeftCollider.GetGroundHit(out hit))
            slipping |= Mathf.Abs(hit.sidewaysSlip) > 0.3f || Mathf.Abs(hit.forwardSlip) > 0.4f;

        if (rearRightCollider.GetGroundHit(out hit))
            slipping |= Mathf.Abs(hit.sidewaysSlip) > 0.3f || Mathf.Abs(hit.forwardSlip) > 0.4f;

        tireSlipSource.volume = slipping ? Mathf.Clamp01(rb.linearVelocity.magnitude / 200f) : 0f;
    }


    void CalculateEngineRPM()
    {
        float wheelRPM = (rearLeftCollider.rpm + rearRightCollider.rpm) * 0.5f;
        float gearRatio = isReverse ? 1f : GetCurrentGearRatio();

        float targetRPM = Mathf.Abs(wheelRPM * gearRatio);

        // Fake clutch slip
        engineRPM = Mathf.Lerp(engineRPM, targetRPM, Time.fixedDeltaTime * 5f);
        engineRPM = Mathf.Clamp(engineRPM, minRPM, maxRPM);
    }

    void ApplyHandbrake()
    {
        tireSlipSource.volume = handbrake ? 1f : 0f;
        if (handbrake)
        {
            rearLeftCollider.brakeTorque = handbrakeTorque;
            rearRightCollider.brakeTorque = handbrakeTorque;
        }
        else
        {
            rearLeftCollider.brakeTorque = 0f;
            rearRightCollider.brakeTorque = 0f;
        }
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
        throttle = 0;

        brakeLight.GetComponent<MeshRenderer>().material = nonBraking;
        if (Input.GetKey(KeyCode.W))
            throttle = 1;
        else if (Input.GetKey(KeyCode.S)) 
        {
            brakeLight.GetComponent<MeshRenderer>().material = braking;
            throttle = -1;
        }

        // Reverse override
        if (isReverse)
            currentMotorTorque = reverseTorque;
        else
            currentMotorTorque = maxMotorTorque * GetCurrentGearRatio();

        // HandBrake
        handbrake = Input.GetKey(KeyCode.Space);

        // Nitro
        if (Input.GetKey(KeyCode.LeftShift) && nitroTimer > 0f)
        {
            nitroActive = true;
            nitroTimer -= Time.deltaTime;
        }
        else
        {
            nitroActive = false;
        }

        if (nitroTimer <= 0f)
        {
            nitroTimer += Time.deltaTime / nitroCooldown;
        }

        nitroTimer = Mathf.Clamp(nitroTimer, 0f, nitroDuration);
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
        frontLeftCollider.brakeTorque = 0f;
        frontRightCollider.brakeTorque = 0f;
        rearLeftCollider.brakeTorque = 0f;
        rearRightCollider.brakeTorque = 0f;
        //tireSlipSource.volume = throttle == -1 ? 1f : 0f;
        float rpmNormalized = engineRPM / maxRPM;
        float torqueFromCurve = torqueCurve.Evaluate(rpmNormalized);

        // Cut power at redline
        if (engineRPM >= redlineRPM)
            torqueFromCurve = 0f;

        float finalTorque = currentMotorTorque * torqueFromCurve;

        if (nitroActive)
            finalTorque *= nitroMultiplier;

        if(throttle == -1)
        {
            float frontBrake = brakeTorque * brakeBias;
            float rearBrake = brakeTorque * (1f - brakeBias) * currentMotorTorque;

            frontLeftCollider.brakeTorque = frontBrake;
            frontRightCollider.brakeTorque = frontBrake;

            rearLeftCollider.brakeTorque = rearBrake;
            rearRightCollider.brakeTorque = rearBrake;
        }
        else if(throttle == 1)
        {
            if (isReverse)
            {
                rearLeftCollider.motorTorque = -finalTorque;
                rearRightCollider.motorTorque = -finalTorque;
            }
            else
            {
                rearLeftCollider.motorTorque = finalTorque;
                rearRightCollider.motorTorque = finalTorque;
            }
        }
        else
        {
            rearLeftCollider.motorTorque = 0;
            rearRightCollider.motorTorque = 0;
        }
    }
    void OnGUI()
    {
        float displaySpeed = useKMH ? speed * 3.6f : speed * 2.23694f;
        string unit = useKMH ? "km/h" : "mph";

        GUI.Label(new Rect(10, 10, 200, 20),
            isReverse ? "Gear: R" : $"Gear: {currentGear}");

        GUI.Label(new Rect(10, 30, 300, 20),
            $"RPM: {(int)engineRPM}");

        GUI.Label(new Rect(10, 50, 300, 20),
            $"Speed: {(int)displaySpeed} {unit}");

        GUI.Label(new Rect(10, 70, 300, 20),
            $"Nitro: {(int)(nitroTimer / nitroDuration * 100f)}%");
        
        float fps = 1f / deltaTime;

        GUI.Label(new Rect(Screen.width - 100, 10, 100, 20),
            $"FPS: {(int)fps}");
    }



    void ApplySteering()
    {
        frontLeftCollider.steerAngle = currentSteerAngle;
        frontRightCollider.steerAngle = currentSteerAngle;

        float speed01 = Mathf.Clamp01(speed / maxSpeedForFOV);

        // FOV effect
        playerCamera.fieldOfView = Mathf.Lerp(minFOV, maxFOV, speed01);

        // Camera pull-back effect
        Vector3 baseOffset =
            Vector3.up * 2.5f +
            Vector3.back * 5f;

        Vector3 speedOffset = Vector3.back * (speed01 * maxCameraBackOffset);

        // Steering sway
        Vector3 steerOffset = -0.03f * currentSteerAngle * Vector3.right;

        playerCamera.transform.localPosition =
            baseOffset + speedOffset;

        // Small yaw rotation based on steering
        playerCamera.transform.rotation = Quaternion.Euler(
            20f,
            transform.eulerAngles.y + currentSteerAngle * 0.05f,
            0f
        );
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
