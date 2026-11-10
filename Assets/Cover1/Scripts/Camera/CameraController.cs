using UnityEngine;

namespace Cover1
{
    public class CameraController : MonoBehaviour
    {
        public bool overview;
        SpaceGame game;
        CameraRig rig;
        // 开局直接显示全关，避免从错误位置慢慢移过来。
        public void Initialize(SpaceGame owner)
        {
            game = owner;
            rig = GetComponent<CameraRig>();
            rig.Snap(game.viewPosition, game.OverviewZoom(game.cam.aspect));
        }
        // 飞行时跟随最新鸟群；瞄准和结束时显示全景。
        void LateUpdate()
        {
            if (!game) return;
            if (!overview && !game.Aiming && !game.ended && GroupBounds.TryGet(game.Birds, game.LatestShot, out var bounds))
            {
                float zoom = Mathf.Max(5.4f, bounds.extents.y + 2.6f, (bounds.extents.x + 3.2f) / game.cam.aspect);
                rig.Move(bounds.center + new Vector3(0, 0, -10), zoom);
                rig.KeepVisible(bounds);
            }
            else rig.Move(game.viewPosition, game.OverviewZoom(game.cam.aspect));
        }
    }
}
