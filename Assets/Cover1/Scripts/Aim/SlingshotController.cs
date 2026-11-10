using UnityEngine;

namespace Cover1
{
    public class SlingshotController : MonoBehaviour
    {
        [Tooltip("最大拉弓距离，单位为 Unity 世界单位。")]
        public float maxPull = 3f;
        [Tooltip("拉弓距离乘这个系数，得到发射速度。")]
        public float launchScale = 4.5f;
        public BirdController loaded;
        public TrajectoryPredictor predictor { get; private set; }
        SpaceGame game;
        public TrajectoryRenderer lines;
        bool dragging;
        Vector2 pull;
        Vector2 SlingPosition => game.level.launchPoint.position;

        public void Initialize(SpaceGame owner)
        {
            game = owner;
            predictor = GetComponent<TrajectoryPredictor>();
            lines.Initialize(game);
        }

        // 拉弓偏移取反，再乘发射系数，得到初速度。
        public Vector2 LaunchVelocity(Vector2 dragOffset)
        {
            float velocityX = -dragOffset.x * launchScale;
            float velocityY = -dragOffset.y * launchScale;
            return new Vector2(velocityX, velocityY);
        }

        // 有待发射的鸟就开始拖拽，否则触发飞行中的技能。
        public void Press(Vector2 mouse, bool insideWindow)
        {
            if (game.ended || !insideWindow) return;
            if (!loaded)
            {
                game.ActivateAbility();
                return;
            }
            if (Vector2.Distance(mouse, loaded.transform.position) < 1.1f) dragging = true;
        }

        // 限制拉距，同时更新小鸟位置和预瞄线。
        public void Drag(Vector2 mouse)
        {
            if (!dragging || !loaded || game.ended) return;
            pull = Vector2.ClampMagnitude(mouse - SlingPosition, maxPull);
            Vector2 start = SlingPosition + pull;
            loaded.Body.position = start;
            loaded.transform.position = start;
            lines.Show(predictor.Predict(start, LaunchVelocity(pull)), start);
        }

        // 松手时发射；拖动太短则放回弹弓。
        public void Release(Vector2 mouse)
        {
            if (!dragging || !loaded || game.ended) return;
            Drag(mouse);
            dragging = false;
            lines.Clear();
            if (pull.magnitude > .15f) game.Launch(LaunchVelocity(pull));
            else
            {
                loaded.Body.position = SlingPosition;
                loaded.transform.position = SlingPosition;
            }
        }

        // 切出窗口时取消拖拽，避免回来后误发射。
        void CancelDrag()
        {
            if (!dragging) return;
            dragging = false;
            pull = Vector2.zero;
            if (lines) lines.Clear();
            if (loaded && game && game.level && game.level.launchPoint)
            {
                loaded.Body.position = SlingPosition;
                loaded.transform.position = SlingPosition;
            }
        }

        void OnApplicationFocus(bool focused)
        {
            if (!focused) CancelDrag();
        }
        void OnApplicationPause(bool paused)
        {
            if (paused) CancelDrag();
        }
        void OnDisable()
        {
            CancelDrag();
        }

        // 把屏幕鼠标坐标转成世界坐标，处理左键按下和松开。
        void Update()
        {
            if (!game) return;
            if (game.ended)
            {
                CancelDrag();
                return;
            }
            Vector2 mouse = game.cam.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0)) Press(mouse, game.cam.pixelRect.Contains(Input.mousePosition));
            Drag(mouse);
            if (Input.GetMouseButtonUp(0)) Release(mouse);
        }
    }
}
