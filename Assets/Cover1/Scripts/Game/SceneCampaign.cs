using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cover1
{
    public class SceneCampaign : MonoBehaviour
    {
        [Tooltip("消灭所有小猪后，等待多少秒进入下一关。")]
        public float advanceDelay = 2f;
        public bool autoAdvance = true;
        public int StageIndex => SceneManager.GetActiveScene().buildIndex;
        SpaceGame game;
        float winTime;
        bool loading;
        void Awake()
        {
            game = GetComponent<SpaceGame>();
        }
        // 胜利后等待两秒进入下一关，最后一关停留。
        void Update()
        {
            if (!game.won || !autoAdvance || loading) return;
            if (StageIndex == SceneManager.sceneCountInBuildSettings - 1) return;
            winTime += Time.deltaTime;
            if (winTime >= advanceDelay) LoadStage(StageIndex + 1);
        }
        public void Retry()
        {
            LoadStage(StageIndex);
        }
        // 手动继续时，最后一关循环回第一关。
        public void Continue()
        {
            if (!game.won) return;
            LoadStage((StageIndex + 1) % SceneManager.sceneCountInBuildSettings);
        }
        // 按 Build Settings 的顺序切场景，避免重复加载。
        public void LoadStage(int index)
        {
            if (loading || index < 0 || index >= SceneManager.sceneCountInBuildSettings) return;
            loading = true;
            SceneManager.LoadScene(index);
        }
    }
}
