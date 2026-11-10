using System.Collections.Generic;
using UnityEngine;

namespace Cover1
{
    public class TrajectoryRenderer : MonoBehaviour
    {
        public LineRenderer preview, elastic;
        CoverLevel level;
        // 两条线已经放在 AimLines Prefab 中，这里只连接关卡并清空。
        public void Initialize(SpaceGame game)
        {
            level = game.level;
            Clear();
        }

        // 画出预测点，并连接左锚点、小鸟和右锚点。
        public void Show(List<Vector2> points, Vector2 birdPosition)
        {
            preview.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++) preview.SetPosition(i, points[i]);
            elastic.positionCount = 3;
            elastic.SetPosition(0, level.leftAnchor.position);
            elastic.SetPosition(1, birdPosition);
            elastic.SetPosition(2, level.rightAnchor.position);
        }
        public void Clear()
        {
            preview.positionCount = 0;
            elastic.positionCount = 0;
        }
    }
}
