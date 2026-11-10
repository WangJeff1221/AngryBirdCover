using UnityEngine;

namespace Cover1
{
    public class SpacePulse : MonoBehaviour
    {
        public float duration = .6f, size = 2f;
        float elapsedTime;
        SpriteRenderer sprite;
        void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
        }
        // 进度 = 已播放时间 / 总时长；圆环变大，同时淡出。
        void Update()
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            transform.localScale = Vector3.one * Mathf.Lerp(.1f, size, progress);
            var color = sprite.color;
            color.a = 1 - progress;
            sprite.color = color;
            if (elapsedTime >= duration) Destroy(gameObject);
        }
    }
}
