using UnityEngine;

namespace Cover1
{
    public class CameraRig : MonoBehaviour
    {
        Camera view;
        Vector3 moveSpeed;
        float zoomSpeed;
        void Awake()
        {
            view = GetComponent<Camera>();
        }
        // 开局直接定位，并清空平滑移动的速度。
        public void Snap(Vector3 center, float size)
        {
            transform.position = center;
            view.orthographicSize = size;
            moveSpeed = Vector3.zero;
            zoomSpeed = 0;
        }
        // 平滑移动和缩放，减少镜头跳动。
        public void Move(Vector3 center, float size)
        {
            transform.position = Vector3.SmoothDamp(transform.position, center, ref moveSpeed, .28f);
            view.orthographicSize = Mathf.SmoothDamp(view.orthographicSize, size, ref zoomSpeed, .3f);
        }
        // 跟随有延迟，靠近边缘时立即扩大视野，避免鸟群出画。
        public void KeepVisible(Bounds bounds)
        {
            float width = Mathf.Max(Mathf.Abs(bounds.min.x - transform.position.x), Mathf.Abs(bounds.max.x - transform.position.x));
            float height = Mathf.Max(Mathf.Abs(bounds.min.y - transform.position.y), Mathf.Abs(bounds.max.y - transform.position.y));
            view.orthographicSize = Mathf.Max(view.orthographicSize, (height + .65f) / .68f, (width + .65f) / (view.aspect * .94f));
        }
    }
}
