using UnityEngine;

namespace TeamGT.Vehicle
{
    /// <summary>
    /// Car controller for Tyler's BMW M4 - supports Mood Lights feature:
    /// Blue = normal/calm, Red = Ego Mode / pushing limits.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        [Header("Wheel References")]
        [SerializeField] private Transform[] wheelMeshes = new Transform[4];
        [SerializeField] private WheelCollider[] wheelColliders = new WheelCollider[4];

        [Header("Engine")]
        [SerializeField] private float motorTorque = 1500f;
        [SerializeField] private float brakeTorque = 3000f;
        [SerializeField] private float maxSteerAngle = 35f;

        [Header("BMW Mood Lights (Tyler's M4 Feature)")]
        [SerializeField] private Light[] moodLights;
        [SerializeField] private Renderer[] moodLightRenderers;
        [SerializeField] private Color calmColor = new Color(0.2f, 0.4f, 1f);
        [SerializeField] private Color angerColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private float moodTransitionSpeed = 2f;

        [Header("Ego Mode Threshold")]
        [SerializeField] private float speedThresholdForRed = 80f;
        [SerializeField] private float driftThresholdForRed = 0.5f;

        private Rigidbody rb;
        private float verticalInput;
        private float horizontalInput;
        private bool isBraking;
        private bool egoModeEngaged;
        private float currentMoodLerp;

        public float CurrentSpeed => rb != null ? rb.velocity.magnitude : 0f;
        public bool IsEgoMode => egoModeEngaged;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = new Vector3(0, -0.3f, 0);
        }

        private void Update()
        {
            verticalInput = Input.GetAxis("Vertical");
            horizontalInput = Input.GetAxis("Horizontal");
            isBraking = Input.GetButton("Jump") || Input.GetKey(KeyCode.Space);

            UpdateMoodLights();
        }

        private void UpdateMoodLights()
        {
            bool pushingLimits = CurrentSpeed > speedThresholdForRed ||
                Mathf.Abs(rb.angularVelocity.y) > driftThresholdForRed ||
                (verticalInput > 0.8f && CurrentSpeed > 50f);

            egoModeEngaged = pushingLimits;

            float targetLerp = pushingLimits ? 1f : 0f;
            currentMoodLerp = Mathf.MoveTowards(currentMoodLerp, targetLerp, moodTransitionSpeed * Time.deltaTime);
            Color currentMoodColor = Color.Lerp(calmColor, angerColor, currentMoodLerp);

            if (moodLights != null)
            {
                foreach (var light in moodLights)
                {
                    if (light != null)
                        light.color = currentMoodColor;
                }
            }

            if (moodLightRenderers != null)
            {
                foreach (var renderer in moodLightRenderers)
                {
                    if (renderer != null && renderer.material != null && renderer.material.HasProperty("_EmissionColor"))
                    {
                        renderer.material.SetColor("_EmissionColor", currentMoodColor * 2f);
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            ApplyMotorTorque();
            ApplySteering();
            UpdateWheelPoses();
        }

        private void ApplyMotorTorque()
        {
            float torque = verticalInput * motorTorque;
            if (isBraking)
            {
                foreach (var wheel in wheelColliders)
                {
                    if (wheel != null)
                        wheel.brakeTorque = brakeTorque;
                }
            }
            else
            {
                for (int i = 0; i < wheelColliders.Length && i < 2; i++)
                {
                    if (wheelColliders[i] != null)
                    {
                        wheelColliders[i].motorTorque = torque;
                        wheelColliders[i].brakeTorque = 0;
                    }
                }
                for (int i = 2; i < wheelColliders.Length; i++)
                {
                    if (wheelColliders[i] != null)
                    {
                        wheelColliders[i].brakeTorque = 0;
                    }
                }
            }
        }

        private void ApplySteering()
        {
            float steer = horizontalInput * maxSteerAngle;
            for (int i = 0; i < wheelColliders.Length && i < 2; i++)
            {
                if (wheelColliders[i] != null)
                    wheelColliders[i].steerAngle = steer;
            }
        }

        private void UpdateWheelPoses()
        {
            if (wheelColliders.Length == 0 || wheelMeshes.Length == 0) return;

            for (int i = 0; i < Mathf.Min(wheelColliders.Length, wheelMeshes.Length); i++)
            {
                if (wheelColliders[i] == null || wheelMeshes[i] == null) continue;

                wheelColliders[i].GetWorldPose(out Vector3 pos, out Quaternion rot);
                wheelMeshes[i].position = pos;
                wheelMeshes[i].rotation = rot;
            }
        }

        public void SetEgoMode(bool enabled)
        {
            egoModeEngaged = enabled;
            currentMoodLerp = enabled ? 1f : 0f;
        }
    }
}
