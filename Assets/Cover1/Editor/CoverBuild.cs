using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Cover1;

public static class CoverBuild
{
    public static readonly string[] Scenes = {
        "Assets/Cover1/Scenes/01_PigBang_1-1.unity",
        "Assets/Cover1/Scenes/02_PigBang_1-3.unity",
        "Assets/Cover1/Scenes/03_PigBang_1-10.unity",
        "Assets/Cover1/Scenes/04_PigBang_1-14.unity"
    };

    [MenuItem("Cover/Open First Level")]
    public static void Open()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(Scenes[0]);
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        if (!Application.isBatchMode)
        {
            var level = UnityEngine.Object.FindFirstObjectByType<CoverLevel>();
            var view = EditorWindow.GetWindow<SceneView>();
            view.in2DMode = true;
            var bounds = level.GetOverviewBounds(3);
            bounds.Expand(2);
            view.Frame(bounds, true);
        }
    }

    // 构建前检查四个场景的引用、小猪刚体和缺失脚本。
    public static void Verify()
    {
        EditorBuildSettings.scenes = Scenes.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
        foreach (var path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path);
            var game = UnityEngine.Object.FindFirstObjectByType<SpaceGame>();
            if (!game || !game.level || !game.pulsePrefab || game.birdPrefabs == null || game.birdPrefabs.Length != 3 || game.birdPrefabs.Any(bird => !bird))
                throw new Exception("Missing game reference: " + path);
            var sling = game.GetComponent<SlingshotController>();
            if (!sling || !sling.lines || !sling.lines.preview || !sling.lines.elastic ||
                !game.GetComponent<TrajectoryPredictor>() || !game.GetComponent<ImpactHandler>())
                throw new Exception("Missing authored game component: " + path);
            if (!Camera.main || !Camera.main.GetComponent<CameraController>() || !Camera.main.GetComponent<CameraRig>())
                throw new Exception("Missing camera component: " + path);
            for (int i = 0; i < game.birdPrefabs.Length; i++)
            {
                var bird = game.birdPrefabs[i];
                if ((int)bird.kind != i || !bird.Body || !bird.GetComponent<TrailRenderer>() || !bird.GetComponent<WorldPhysics>())
                    throw new Exception("Invalid bird prefab: " + bird.name);
            }
            foreach (var block in game.level.GetComponentsInChildren<SpaceDebris>())
                if (!block.fragmentPrefab || !block.GetComponent<WorldPhysics>())
                    throw new Exception("Missing fragment prefab or gravity: " + block.name);
            if (UnityEngine.Object.FindObjectsByType<CoverLevel>(FindObjectsSortMode.None).Length != 1)
                throw new Exception("Expected one authored level: " + path);
            foreach (var target in game.level.GetComponentsInChildren<SpaceTarget>())
                if (!target.tnt && !target.GetComponent<Rigidbody2D>()) throw new Exception("Pig needs Rigidbody2D");
            foreach (var root in scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0)
                        throw new Exception("Missing script: " + t.name);

            EditorSceneManager.SaveScene(scene);
        }
        EditorSceneManager.OpenScene(Scenes[0]);
        AssetDatabase.SaveAssets();
        Debug.Log("COVER_STANDALONE_SCENES_PASS");
    }

    [MenuItem("Cover/Build Windows")]
    // 检查场景后生成 Windows 可执行文件。
    public static void BuildWindows()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Verify();
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        Build(BuildTarget.StandaloneWindows64, "Builds/Windows/AngryBirdCover.exe");
    }

    [MenuItem("Cover/Build WebGL")]
    // 生成不压缩的 WebGL 文件，方便上传 itch。
    public static void BuildWeb()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Verify();
        PlayerSettings.defaultWebScreenWidth = 1280;
        PlayerSettings.defaultWebScreenHeight = 720;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.threadsSupport = false;
        Build(BuildTarget.WebGL, "Builds/WebGL");
    }

    public static void BuildAll()
    {
        BuildWindows();
        BuildWeb();
    }

    // 构建失败时抛出错误，避免把未完成的产物当作成功。
    static void Build(BuildTarget target, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        var report = BuildPipeline.BuildPlayer(Scenes, destination, target, BuildOptions.None);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception("Build failed: " + report.summary.result);
        Debug.Log("COVER_STANDALONE_BUILD_PASS " + target);
    }
}
