using System.Collections.Generic;
using UnityEngine;

namespace Cover1
{
    public class SpaceGame : MonoBehaviour
    {
        public static SpaceGame Instance { get; private set; }
        public CoverLevel level;
        [Tooltip("依次放入红鸟、黑鸟、蓝鸟 Prefab，与 BirdKind 的顺序对应。")]
        public BirdController[] birdPrefabs;
        public SpacePulse pulsePrefab;
        [Tooltip("本关鸟的发射顺序；在每个 Scene 分别设置。")]
        public BirdKind[] queue = { BirdKind.Red, BirdKind.Red, BirdKind.Red };
        public readonly List<BirdController> Birds = new List<BirdController>();
        public int LatestShot { get; private set; }
        public int RemainingTargets { get; private set; }
        public int Ammo => queue.Length - usedBirds;
        public bool Aiming => slingshot != null && slingshot.loaded != null;
        public bool ended, won;
        public SlingshotController slingshot { get; private set; }
        public ImpactHandler impacts { get; private set; }
        public SceneCampaign campaign { get; private set; }
        public CameraController follow { get; private set; }
        public Camera cam { get; private set; }
        public Bounds overviewBounds { get; private set; }
        public Vector3 viewPosition { get; private set; }
        int usedBirds;
        float timeWithoutBirds;

        void Awake()
        {
            Instance = this;
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            Time.fixedDeltaTime = .02f;
        }

        // 连接关卡、弹弓和镜头，再装填第一只鸟。
        void Start()
        {
            cam = Camera.main;
            campaign = GetComponent<SceneCampaign>();
            impacts = GetComponent<ImpactHandler>();
            slingshot = GetComponent<SlingshotController>();
            follow = cam.GetComponent<CameraController>();
            overviewBounds = level.GetOverviewBounds(slingshot.maxPull);
            viewPosition = new Vector3(overviewBounds.center.x, overviewBounds.center.y, -10);
            if (level.birdPreview) level.birdPreview.gameObject.SetActive(false);
            foreach (var target in level.GetComponentsInChildren<SpaceTarget>())
                if (!target.tnt) RemainingTargets++;
            follow.Initialize(this);
            slingshot.Initialize(this);
            LoadBird();
        }

        // 同时满足关卡高度和宽度，防止宽屏或窄屏裁掉关卡。
        public float OverviewZoom(float aspect)
        {
            float verticalSize = overviewBounds.extents.y + level.cameraPadding;
            float horizontalSize = (overviewBounds.extents.x + level.cameraPadding) / aspect;
            return Mathf.Max(level.viewSize, verticalSize, horizontalSize);
        }

        // 复制已配置好的鸟 Prefab，再记录它属于哪次发射。
        public BirdController CreateBird(Vector2 position, BirdKind kind, int shot)
        {
            BirdController prefab = birdPrefabs[(int)kind];
            BirdController bird = Instantiate(prefab, position, Quaternion.identity, level.transform);
            bird.name = prefab.name;
            bird.shotId = shot;
            Birds.Add(bird);
            return bird;
        }

        // 按队列装填，更新弹弓旁的备用鸟。
        void LoadBird()
        {
            slingshot.loaded = CreateBird(level.launchPoint.position, queue[usedBirds], LatestShot + 1);
            if (level.waitingBirds != null)
                for (int i = 0; i < level.waitingBirds.Length; i++)
                    if (level.waitingBirds[i]) level.waitingBirds[i].gameObject.SetActive(i < Ammo - 1);
            follow.overview = false;
        }

        // 每次发射分配编号，供技能和镜头识别同一组鸟。
        public void Launch(Vector2 velocity)
        {
            if (ended || !slingshot.loaded) return;
            LatestShot++;
            slingshot.loaded.shotId = LatestShot;
            slingshot.loaded.Launch(velocity);
            slingshot.loaded = null;
            usedBirds++;
            timeWithoutBirds = 0;
            follow.overview = false;
        }

        public void NextBird()
        {
            if (!ended && !Aiming && Ammo > 0) LoadBird();
        }

        // 分裂会添加新鸟，所以遍历列表副本。
        public void ActivateAbility()
        {
            if (ended || Aiming) return;

            foreach (var bird in Birds.ToArray())
                if (bird && bird.shotId == LatestShot) bird.Ability();
        }

        // 所有小猪被击败后判定胜利。
        public void TargetDown()
        {
            RemainingTargets--;
            if (RemainingTargets == 0)
            {
                ended = true;
                won = true;
            }
        }

        // 创建反馈圆环，大小和淡出由 SpacePulse 控制。
        public void Pulse(Vector2 position, Color color, float size)
        {
            SpacePulse effect = Instantiate(pulsePrefab, position, Quaternion.identity, level.transform);
            effect.size = size;
            effect.GetComponent<SpriteRenderer>().color = color;
        }

        // 处理快捷键，再检查是否需要装填或判负。
        void Update()
        {
            if (!slingshot) return;
            if (Input.GetKeyDown(KeyCode.R))
            {
                campaign.Retry();
                return;
            }
            for (int i = 0; i < 4; i++)
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
                {
                    campaign.LoadStage(i);
                    return;
                }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                campaign.Continue();
                return;
            }
            if (Input.GetKeyDown(KeyCode.V)) follow.overview = !follow.overview;
            if (ended) return;
            if (Input.GetKeyDown(KeyCode.N)) NextBird();

            // 没有飞行或待发射的鸟时等一秒，再装填或结束。
            Birds.RemoveAll(bird => !bird);
            bool hasFlyingBird = Birds.Exists(bird => bird.launched);
            if (hasFlyingBird || Aiming)
            {
                timeWithoutBirds = 0;
                return;
            }
            timeWithoutBirds += Time.deltaTime;
            if (timeWithoutBirds < 1) return;
            if (Ammo > 0) NextBird();
            else
            {
                ended = true;
                won = false;
            }
        }
    }
}
