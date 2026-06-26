using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CameraController : MonoBehaviour
    {
        public Transform FollowTarget;
        public float panSpeed = 16f;
        public float zoomSpeed = 4f;
        public float minZoom = 5f;
        public float maxZoom = 58f;
        private Camera cam;
        private Vector3 lastPanMouse;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                if (Mathf.Abs(transform.position.z) < 0.5f) transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
            }
        }

        public void SnapTo(Transform target)
        {
            FollowTarget = target;
            if (target == null) return;
            float z = Mathf.Abs(transform.position.z) < 0.5f ? -10f : transform.position.z;
            transform.position = new Vector3(target.position.x, target.position.y, z);
        }

        private void LateUpdate()
        {
            if (cam == null) cam = GetComponent<Camera>();
            Vector3 input = KeyboardPanInput();
            if (input.sqrMagnitude > 0.01f)
            {
                FollowTarget = null;
                transform.position += input.normalized * panSpeed * Time.deltaTime;
            }
            else if (Input.GetMouseButtonDown(2))
            {
                FollowTarget = null;
                lastPanMouse = Input.mousePosition;
            }
            else if (Input.GetMouseButton(2))
            {
                FollowTarget = null;
                Vector3 delta = Input.mousePosition - lastPanMouse;
                transform.position -= new Vector3(delta.x, delta.y, 0f) * (cam != null ? cam.orthographicSize * 2f / Mathf.Max(1f, Screen.height) : 0.02f);
                lastPanMouse = Input.mousePosition;
            }
            else if (FollowTarget != null)
            {
                Vector3 target = FollowTarget.position;
                target.z = transform.position.z;
                transform.position = Vector3.Lerp(transform.position, target, 4f * Time.deltaTime);
            }
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f && cam != null)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
            }
        }

        private Vector3 KeyboardPanInput()
        {
            float x = 0f;
            float y = 0f;
            if (Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.UpArrow)) y += 1f;
            return new Vector3(x, y, 0f);
        }
    }
}
