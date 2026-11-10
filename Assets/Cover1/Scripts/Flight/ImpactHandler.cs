using UnityEngine;

namespace Cover1
{
    public class ImpactHandler : MonoBehaviour
    {
        // 只取撞向接触面的速度，沿表面滑动不算全部撞击。
        public static float ImpactSpeed(Collision2D collision)
        {
            float speed = 0;
            for (int i = 0; i < collision.contactCount; i++)
                speed = Mathf.Max(speed, Mathf.Abs(Vector2.Dot(collision.relativeVelocity, collision.GetContact(i).normal)));
            return speed;
        }
        // 黑鸟和 TNT 共用爆炸：范围内向外推，离中心越远越弱。
        public void Explode(Vector2 center, float radius)
        {
            var game = SpaceGame.Instance;
            game.Pulse(center, new Color(1, .76f, .32f), radius * 2);
            foreach (var collider in Physics2D.OverlapCircleAll(center, radius))
            {
                // 气泡等触发区域不承受冲量，避免同一刚体重复受力。
                if (!collider || collider.isTrigger) continue;
                Vector2 offset = (Vector2)collider.transform.position - center;
                Vector2 direction = offset.sqrMagnitude < .001f ? Vector2.up : offset.normalized;
                float distanceRatio = Mathf.Clamp01(offset.magnitude / radius);
                var target = collider.GetComponent<SpaceTarget>();
                var block = collider.GetComponent<SpaceDebris>();
                if (target) target.Hit(direction * Mathf.Lerp(10, 3, distanceRatio));
                else if (block) block.Blast(center, radius);
                else if (collider.attachedRigidbody)
                    collider.attachedRigidbody.AddForce(direction * Mathf.Lerp(8, 2, distanceRatio), ForceMode2D.Impulse);
            }
        }
    }
}
