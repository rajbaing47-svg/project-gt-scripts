using UnityEngine;

namespace TeamGT.Vehicle
{
    /// <summary>
    /// AI-controlled car for opponent racers (GT friends, DreadCrew).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class AI_CarController : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float waypointReachDistance = 10f;

        [Header("Engine")]
        [SerializeField] private float motorTorque = 1200f;
        [SerializeField] private float brakeTorque = 2500f;
        [SerializeField] private float maxSteerAngle = 30f;
        [SerializeField] private float maxSpeed = 120f;

        [Header("AI Behavior")]
        [SerializeField] private float aggressiveness = 0.5f;

        private Rigidbody rb;
        private int currentWaypointIndex;
        private WheelCollider[] wheelColliders;
        private Transform[] wheelMeshes;

        public bool IsEliminated { get; set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            wheelColliders = GetComponentsInChildren<WheelCollider>();
        }

        private void FixedUpdate()
        {
            if (IsEliminated || waypoints == null || waypoints.Length == 0) return;

            NavigateToWaypoint();
        }

        private void NavigateToWaypoint()
        {
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }

            Transform target = waypoints[currentWaypointIndex];
            if (target == null) return;

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0;
            float distance = toTarget.magnitude;

            if (distance < waypointReachDistance)
            {
                currentWaypointIndex++;
            }

            Vector3 localTarget = transform.InverseTransformPoint(target.position);
            float steer = Mathf.Clamp(localTarget.x * 0.1f, -1f, 1f) * maxSteerAngle;

            float throttle = 1f;
            if (rb.velocity.magnitude > maxSpeed)
                throttle = 0f;
            else if (Mathf.Abs(localTarget.x) > 5f)
                throttle = 0.7f;

            foreach (var wc in wheelColliders)
            {
                if (wc == null) continue;
                if (wc.CompareTag("Steerable"))
                    wc.steerAngle = steer;
                wc.motorTorque = throttle * motorTorque;
            }
        }

        public void SetWaypoints(Transform[] newWaypoints)
        {
            waypoints = newWaypoints;
            currentWaypointIndex = 0;
        }
    }
}
