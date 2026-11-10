using System.Collections.Generic;
using UnityEngine;

namespace Cover1
{
    public class TrajectoryPredictor : MonoBehaviour
    {
        public const float BirdRadius = .34f;
        readonly List<Vector2> points = new List<Vector2>();
        readonly List<RaycastHit2D> hits = new List<RaycastHit2D>();

        // 从发射位置开始计算短路径，与真实飞行共用引力规则。
        public List<Vector2> Predict(Vector2 position, Vector2 velocity)
        {
            Physics2D.SyncTransforms();
            points.Clear();
            points.Add(position);
            var planets = FindObjectsByType<GravityField>(FindObjectsSortMode.None);
            float timeStep = Time.fixedDeltaTime;
            float distanceLeft = 15f;

            for (int i = 0; i < 85; i++)
            {
                Vector2 acceleration = GravityField.Sample(position, planets);

                // 每步先更新速度，再用新速度计算移动距离。
                velocity.x += acceleration.x * timeStep;
                velocity.y += acceleration.y * timeStep;

                Vector2 movement = new Vector2(velocity.x * timeStep, velocity.y * timeStep);
                float stepDistance = movement.magnitude;
                if (stepDistance < .000001f) break;
                Vector2 direction = movement / stepDistance;
                float travel = Mathf.Min(stepDistance, distanceLeft);

                // 用小鸟大小的圆检查前方，碰到实体就截断预瞄。
                bool blocked = FindObstacle(position, direction, ref travel);
                if (blocked || travel < stepDistance) position += direction * travel;
                else position += movement;
                points.Add(position);
                distanceLeft -= travel;
                if (blocked || distanceLeft <= 0) break;
            }
            return points;
        }

        // 忽略触发器和小鸟，只保留最近的实体碰撞。
        bool FindObstacle(Vector2 position, Vector2 direction, ref float distance)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            Physics2D.CircleCast(position, BirdRadius, direction, filter, hits, distance);
            bool found = false;
            foreach (var hit in hits)
            {
                if (!hit.collider || hit.collider.GetComponentInParent<BirdController>()) continue;
                if (hit.distance > distance) continue;
                distance = hit.distance;
                found = true;
            }
            return found;
        }
    }
}
