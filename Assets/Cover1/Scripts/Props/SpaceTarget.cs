using UnityEngine;

namespace Cover1
{
    public class SpaceTarget : MonoBehaviour
    {
        [Tooltip("勾选后是爆炸物；不勾选是需要消灭的小猪。")]
        public bool tnt;
        public bool spent { get; private set; }
        [Tooltip("撞击速度超过这个值时，小猪被击败。")]
        public float impactThreshold = 4f;
        Rigidbody2D body;
        SpriteRenderer[] visuals;
        float timeSinceHit;

        // 小猪使用动态刚体和星球引力，失去支撑后会下落。
        void Awake()
        {
            visuals = GetComponentsInChildren<SpriteRenderer>();
            if (tnt) return;
            body = GetComponent<Rigidbody2D>();
        }
        // 每个目标只结算一次；小猪击退，TNT 调用共用爆炸。
        public void Hit(Vector2 knockback)
        {
            if (spent) return;
            spent = true;
            if (tnt)
            {
                gameObject.SetActive(false);
                SpaceGame.Instance.impacts.Explode(transform.position, 3.1f);
                Destroy(gameObject);
                return;
            }
            body.linearVelocity = Vector2.ClampMagnitude(knockback, 12);
            body.angularVelocity = knockback.x >= 0 ? -240 : 240;
            SpaceGame.Instance.TargetDown();
        }
        // 足够强的撞击才击败目标，击退速度限制在合理范围内。
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (spent) return;
            float speed = ImpactHandler.ImpactSpeed(collision);
            if (speed <= (tnt ? 1.4f : impactThreshold)) return;
            Vector2 direction = (Vector2)transform.position - (Vector2)collision.collider.bounds.center;
            if (direction.sqrMagnitude < .001f) direction = Vector2.up;
            Hit(direction.normalized * Mathf.Clamp(speed * .85f, 3, 12));
        }
        // 保留一小段受击运动，再淡出小猪。
        void Update()
        {
            if (!spent || tnt) return;
            timeSinceHit += Time.deltaTime;
            float opacity = 1 - Mathf.InverseLerp(.7f, 1.3f, timeSinceHit);
            foreach (SpriteRenderer visual in visuals)
            {
                if (!visual) continue;
                Color color = visual.color;
                color.a = opacity;
                visual.color = color;
            }
            if (timeSinceHit >= 1.3f) Destroy(gameObject);
        }
    }
}
