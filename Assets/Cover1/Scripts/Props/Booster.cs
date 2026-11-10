using UnityEngine;
using UnityEngine.Serialization;

namespace Cover1
{
    public class Booster : MonoBehaviour
    {
        [FormerlySerializedAs("impulse")]
        [Tooltip("进入时增加的水平、竖直速度，单位为世界单位/秒。")]
        public Vector2 velocityBoost = new Vector2(6f, 3f);

        // 进入时加一次速度，不在区域内持续施力。
        void OnTriggerEnter2D(Collider2D other)
        {
            BirdController bird = other.GetComponent<BirdController>();
            if (bird == null || !bird.launched)
            {
                return;
            }

            bird.Body.linearVelocity += velocityBoost;

            SpaceGame game = SpaceGame.Instance;
            game.Pulse(transform.position, Color.cyan, 1);
        }
    }
}
