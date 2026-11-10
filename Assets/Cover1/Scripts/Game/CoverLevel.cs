using UnityEngine;

namespace Cover1
{
    public class CoverLevel : MonoBehaviour
    {
        [Header("原版参考（仅编辑器显示）")]
        public string referenceLevel;
        public string referenceVideo;
        [TextArea(2, 5)] public string designNotes;
        [Header("弹弓和镜头")]
        public Transform launchPoint, leftAnchor, rightAnchor;
        public SpriteRenderer birdPreview;
        [Tooltip("弹弓旁的备用鸟，显示剩余弹药。")]
        public SpriteRenderer[] waitingBirds;
        public float viewSize = 9f;
        public float cameraPadding = 1f;
        // 合并关卡图形范围，并为拉弓留出空间。
        public Bounds GetOverviewBounds(float maxPull)
        {
            var bounds = new Bounds(launchPoint.position, Vector3.zero);
            foreach (var sprite in GetComponentsInChildren<SpriteRenderer>())
                if (sprite.enabled && sprite.sprite) bounds.Encapsulate(sprite.bounds);
            Vector3 pullSpace = new Vector3(maxPull + .5f, maxPull + .5f, 0);
            bounds.Encapsulate(launchPoint.position + pullSpace);
            bounds.Encapsulate(launchPoint.position - pullSpace);
            return bounds;
        }
    }
}
