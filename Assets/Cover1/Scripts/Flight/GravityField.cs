using UnityEngine;

namespace Cover1
{
    [ExecuteAlways]
    public class GravityField : MonoBehaviour
    {
        [Min(.1f), Tooltip("引力范围半径；灰色外圈之外没有这颗星球的引力。")]
        public float radius = 6.8f;

        [Min(0), Tooltip("重力加速度 g，单位为世界单位/秒²；越大吸引越强，0 为关闭。")]
        public float strength = 14f;

        [Min(.1f), Tooltip("星球实体表面半径，同时改变碰撞范围，不是引力强度。")]
        public float surfaceRadius = 2.5f;

        public Transform surfaceVisual, boundaryVisual;

        // 计算物体到星球中心的距离和方向。
        public Vector2 Acceleration(Vector2 position)
        {
            float horizontalDistance = transform.position.x - position.x;
            float verticalDistance = transform.position.y - position.y;

            float distance = Mathf.Sqrt(
                horizontalDistance * horizontalDistance +
                verticalDistance * verticalDistance);

            float planetScale = Mathf.Max(
                Mathf.Abs(transform.lossyScale.x),
                Mathf.Abs(transform.lossyScale.y));
            float gravityRange = radius * planetScale;

            // 圈外没有引力；中心附近跳过，避免除以零。
            if (distance > gravityRange || distance < .001f)
                return Vector2.zero;

            // 方向比例乘 g，分别得到水平和竖直加速度。
            float horizontalRatio = horizontalDistance / distance;
            float verticalRatio = verticalDistance / distance;
            float accelerationX = strength * horizontalRatio;
            float accelerationY = strength * verticalRatio;

            return new Vector2(accelerationX, accelerationY);
        }

        // 多个引力场的加速度按方向相加。
        public static Vector2 Sample(Vector2 position, GravityField[] planets = null)
        {
            float totalAccelerationX = 0f;
            float totalAccelerationY = 0f;

            if (planets == null) planets = FindObjectsByType<GravityField>(FindObjectsSortMode.None);
            foreach (var planet in planets)
            {
                if (!planet || !planet.isActiveAndEnabled) continue;
                Vector2 acceleration = planet.Acceleration(position);
                totalAccelerationX += acceleration.x;
                totalAccelerationY += acceleration.y;
            }

            return new Vector2(totalAccelerationX, totalAccelerationY);
        }

        void Update()
        {
            SyncVisuals();
        }

        // 让星球表面和引力边界跟随 Inspector 中的半径。
        public void SyncVisuals()
        {
            if (surfaceVisual)
                surfaceVisual.localScale = new Vector3(surfaceRadius * 2, surfaceRadius * 2, 1);
            if (boundaryVisual)
                boundaryVisual.localScale = new Vector3(radius * 2, radius * 2, 1);
        }
    }
}
