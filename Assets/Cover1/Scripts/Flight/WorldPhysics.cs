using UnityEngine;

namespace Cover1
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class WorldPhysics : MonoBehaviour
    {
        Rigidbody2D body;
        GravityField[] planets;
        // 关闭默认向下的重力，改用星球引力。
        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0;
            // 星球固定在场景中，只查找一次；停用的引力场由 Sample 跳过。
            planets = FindObjectsByType<GravityField>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }
        // 每个物理时间步按 F = m × a 施力。
        void FixedUpdate()
        {
            if (!body.simulated || body.bodyType != RigidbodyType2D.Dynamic) return;
            Vector2 acceleration = GravityField.Sample(body.position, planets);
            float forceX = body.mass * acceleration.x;
            float forceY = body.mass * acceleration.y;
            body.AddForce(new Vector2(forceX, forceY), ForceMode2D.Force);
        }
    }
}
