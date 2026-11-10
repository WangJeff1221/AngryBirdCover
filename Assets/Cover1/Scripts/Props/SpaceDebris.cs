using UnityEngine;

namespace Cover1
{
    public enum BlockMaterial
    {
        [InspectorName("木头")] Wood,
        [InspectorName("冰")] Ice,
        [InspectorName("石头")] Stone
    }

    public class SpaceDebris : MonoBehaviour
    {
        [Tooltip("木头、冰、石头；蓝鸟撞冰时伤害乘 3。")]
        public BlockMaterial material;
        public SpaceDebris fragmentPrefab;
        [Tooltip("耐久；越大越难打碎。")]
        public float durability = 10f;
        [Tooltip("低于这个撞击速度不扣耐久，避免静止的结构自己损坏。")]
        public float minimumImpactSpeed = 2.5f;
        [Tooltip("超过最低速度的部分，每单位速度造成多少伤害。")]
        public float impactDamage = 2.5f;
        public bool Broken { get; private set; }
        float remainingDurability, fragmentTime;
        [SerializeField] bool isFragment;
        Rigidbody2D body;
        SpriteRenderer visual;
        Color originalColor;

        // 初始化耐久和引力，记住原色用于受伤变色。
        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            visual = GetComponent<SpriteRenderer>();
            originalColor = visual.color;
            remainingDurability = durability;
        }
        // 超过最低撞击速度才扣耐久；蓝鸟撞冰伤害乘三。
        void OnCollisionEnter2D(Collision2D collision)
        {
            float speed = ImpactHandler.ImpactSpeed(collision);
            float damage = Mathf.Max(0, speed - minimumImpactSpeed) * impactDamage;
            var bird = collision.collider.GetComponentInParent<BirdController>();
            if (material == BlockMaterial.Ice && bird && bird.kind == BirdKind.Split) damage *= 3;
            Damage(damage);
        }
        // 爆炸同时施加一次冲量和距离衰减伤害。
        public void Blast(Vector2 center, float radius)
        {
            if (Broken) return;
            Vector2 offset = (Vector2)transform.position - center;
            float distanceRatio = Mathf.Clamp01(offset.magnitude / radius);
            Vector2 direction = offset.sqrMagnitude < .001f ? Vector2.up : offset.normalized;
            body.AddForce(direction * Mathf.Lerp(8, 2, distanceRatio), ForceMode2D.Impulse);
            Damage(Mathf.Lerp(20, 4, distanceRatio));
        }
        // 耐久耗尽就打碎，否则用变暗表示受伤。
        public void Damage(float amount)
        {
            if (Broken || isFragment || amount <= 0) return;
            remainingDurability = Mathf.Max(0, remainingDurability - amount);
            if (remainingDurability == 0) Break();
            else visual.color = Color.Lerp(originalColor * .55f, originalColor, remainingDurability / durability);
        }
        // 移除原砖块的支撑，生成三块可运动的碎片。
        void Break()
        {
            Broken = true;

            GetComponent<Collider2D>().enabled = false;
            for (int i = -1; i <= 1; i++) CreateFragment(i);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        // 沿长边切成三块，保留原速度并加少量散开速度。
        void CreateFragment(int index)
        {
            Vector3 scale = transform.lossyScale;
            bool horizontal = Mathf.Abs(scale.x) >= Mathf.Abs(scale.y);
            Vector3 localOffset = horizontal ? new Vector3(index / 3f, 0) : new Vector3(0, index / 3f);
            SpaceDebris fragment = Instantiate(fragmentPrefab, transform.TransformPoint(localOffset), transform.rotation, transform.parent);
            GameObject chip = fragment.gameObject;
            chip.transform.localScale = Vector3.one;
            Vector3 parentScale = chip.transform.lossyScale;
            float width = Mathf.Abs(scale.x) * (horizontal ? .27f : .8f);
            float height = Mathf.Abs(scale.y) * (horizontal ? .8f : .27f);
            chip.transform.localScale = new Vector3(width / Mathf.Max(.001f, Mathf.Abs(parentScale.x)), height / Mathf.Max(.001f, Mathf.Abs(parentScale.y)), 1);
            var sprite = chip.GetComponent<SpriteRenderer>();
            sprite.sprite = visual.sprite;
            sprite.sharedMaterial = visual.sharedMaterial;
            sprite.color = originalColor;
            sprite.sortingOrder = visual.sortingOrder;
            fragment.originalColor = originalColor;
            var chipBody = chip.GetComponent<Rigidbody2D>();
            chipBody.mass = body.mass / 3;
            chipBody.linearVelocity = body.linearVelocity + (Vector2)(transform.right * index * 1.5f + transform.up * (index == 0 ? 1.2f : -.7f));
            chipBody.angularVelocity = index * 180 + 70;
            fragment.material = material;
        }
        // 碎片在 1.2 秒后开始淡出，1.8 秒后清理。
        void Update()
        {
            if (!isFragment) return;
            fragmentTime += Time.deltaTime;
            var color = originalColor;
            color.a *= 1 - Mathf.InverseLerp(1.2f, 1.8f, fragmentTime);
            visual.color = color;
            if (fragmentTime >= 1.8f) Destroy(gameObject);
        }
    }
}
