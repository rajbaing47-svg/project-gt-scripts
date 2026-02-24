using UnityEngine;

namespace TeamGT.Vehicle
{
    /// <summary>
    /// Simple follow camera for racing - follows player car.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 5, -12);
        [SerializeField] private float smoothSpeed = 5f;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + target.rotation * offset;
            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }

        public void SetTarget(Transform t)
        {
            target = t;
        }
    }
}
