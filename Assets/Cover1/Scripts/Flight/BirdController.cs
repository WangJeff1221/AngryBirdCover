using UnityEngine;

namespace Cover1
{
    public enum BirdKind
    {
        [InspectorName("红鸟：普通撞击")] Red,
        [InspectorName("黑鸟：点击爆炸")] Bomb,
        [InspectorName("蓝鸟：点击分裂")] Split
    }

    public class BirdController : MonoBehaviour
    {
        public Rigidbody2D Body;
        public BirdKind kind;
        public int shotId;
        public bool used, launched;
        float flightTime, stoppedTime;
        float fuseTime;
        bool fuseStarted;

        // 设置初速度，开始刚体模拟和尾迹。
        public void Launch(Vector2 velocity)
        {
            launched = true;
            Body.simulated = true;
            Body.linearVelocity = velocity;
            GetComponent<TrailRenderer>().emitting = true;
        }
        // 更新引信和飞行计时；停住、超时或飞远后退场。
        void FixedUpdate()
        {
            if (!launched) return;
            flightTime += Time.fixedDeltaTime;

            if (fuseStarted)
            {
                fuseTime += Time.fixedDeltaTime;
                if (fuseTime >= 1f)
                {
                    Ability();
                    return;
                }
            }
            if (Body.linearVelocity.sqrMagnitude < .12f) stoppedTime += Time.fixedDeltaTime;
            else stoppedTime = 0;
            if (flightTime > 18 || stoppedTime > 2 || Body.position.magnitude > 65) Retire();
        }
        // 一次发射只允许使用一次技能。
        public void Ability()
        {
            if (!launched || used) return;
            used = true;
            if (kind == BirdKind.Bomb) Explode();
            else if (kind == BirdKind.Split) Split();
        }

        // 先停用黑鸟，再结算爆炸，避免重复参与连锁反应。
        void Explode()
        {
            Vector2 center = Body.position;
            Retire();
            SpaceGame.Instance.impacts.Explode(center, 3.4f);
        }

        // 保留中间鸟，两侧各加一只，方向偏转 ±16°。
        void Split()
        {
            var game = SpaceGame.Instance;
            Vector2 velocity = Body.linearVelocity;
            Vector2 sideDirection = new Vector2(-velocity.y, velocity.x).normalized;
            for (int side = -1; side <= 1; side += 2)
            {
                var child = game.CreateBird(Body.position + sideDirection * side * .65f, BirdKind.Split, shotId);
                child.used = true;
                child.transform.localScale *= .72f;
                child.Launch(Quaternion.Euler(0, 0, side * 16) * velocity);
            }
            transform.localScale *= .72f;
        }

        public void Retire()
        {
            launched = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        // 黑鸟撞到物体后开始一秒引信，刚发射的接触不算。
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (launched && kind == BirdKind.Bomb && flightTime > .15f) fuseStarted = true;
        }
    }
}
