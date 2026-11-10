using UnityEngine;

namespace Cover1
{
    public class SpaceBubble : MonoBehaviour
    {
        // 鸟或砖块碰到后移除气泡，不直接消灭里面的小猪。
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponentInParent<BirdController>() && !other.GetComponent<SpaceDebris>()) return;
            SpaceGame.Instance.Pulse(transform.position, new Color(.65f, .9f, 1f), transform.lossyScale.x);
            Destroy(gameObject);
        }
    }
}
