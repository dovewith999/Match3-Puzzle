using Unity.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Match3.ECS.Authoring.Editor
{
    public static class SubSceneSetup
    {
        private const string IngameScenePath = "Assets/Scenes/Ingame.unity";
        private const string SubScenePath = "Assets/Scenes/IngameSubScene.unity";

        [MenuItem("Match3/Setup/1. Configure SubScene GameObjects")]
        public static void ConfigureSubSceneGameObjects()
        {
            var subScene = EditorSceneManager.OpenScene(SubScenePath, OpenSceneMode.Single);

            EnsureAuthoring<GameConfigAuthoring>("GameConfig");
            EnsureAuthoring<BoardConfigAuthoring>("BoardConfig");

            EditorSceneManager.MarkSceneDirty(subScene);
            EditorSceneManager.SaveScene(subScene);

            Debug.Log("[SubSceneSetup] IngameSubScene GameObjects configured and saved.");
        }

[MenuItem("Match3/Setup/2. Link SubScene to Ingame")]
        public static void LinkSubSceneToIngame()
        {
            var ingameScene = EditorSceneManager.OpenScene(IngameScenePath, OpenSceneMode.Single);

            var subSceneGO = GameObject.Find("SubScene");

            if (subSceneGO == null)
            {
                subSceneGO = new GameObject("SubScene");
            }

            var subSceneComp = subSceneGO.GetComponent<SubScene>();

            if (subSceneComp == null)
            {
                subSceneComp = subSceneGO.AddComponent<SubScene>();
            }

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SubScenePath);

            if (sceneAsset == null)
            {
                Debug.LogError($"[SubSceneSetup] SubScene asset not found at: {SubScenePath}");
                return;
            }

            subSceneComp.SceneAsset = sceneAsset;

            subSceneComp.AutoLoadScene = true;

            EditorUtility.SetDirty(subSceneGO);
            EditorSceneManager.MarkSceneDirty(ingameScene);
            EditorSceneManager.SaveScene(ingameScene);

            Debug.Log($"[SubSceneSetup] SubScene linked to {SubScenePath} and Ingame scene saved.");
        }

        private static void EnsureAuthoring<T>(string goName) where T : UnityEngine.Component
        {
            var go = GameObject.Find(goName);

            if (go == null)
            {
                go = new GameObject(goName);
            }

            if (go.GetComponent<T>() == null)
            {
                go.AddComponent<T>();
            }
        }
    

[MenuItem("Match3/Setup/3. Add Scenes to Build Settings")]
        public static void AddScenesToBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Ingame.unity", true),
            };

            EditorBuildSettings.scenes = scenes;

            Debug.Log("[SubSceneSetup] Build Settings updated with Ingame scene.");
        }

[MenuItem("Match3/Setup/4. Create PieceColorConfig Asset")]
        public static void CreatePieceColorConfigAsset()
        {
            var config = ScriptableObject.CreateInstance<Match3.ECS.PieceColorConfig>();

            config.entries = new Match3.ECS.PieceColorConfig.ColorEntry[]
            {
                new Match3.ECS.PieceColorConfig.ColorEntry { colorType = Game.ColorTypeECS.Yellow, fallbackColor = new UnityEngine.Color(1f, 0.9f, 0.2f) },
                new Match3.ECS.PieceColorConfig.ColorEntry { colorType = Game.ColorTypeECS.Purple, fallbackColor = new UnityEngine.Color(0.7f, 0.3f, 1f) },
                new Match3.ECS.PieceColorConfig.ColorEntry { colorType = Game.ColorTypeECS.Red,    fallbackColor = new UnityEngine.Color(1f, 0.3f, 0.3f) },
                new Match3.ECS.PieceColorConfig.ColorEntry { colorType = Game.ColorTypeECS.Blue,   fallbackColor = new UnityEngine.Color(0.3f, 0.6f, 1f) },
                new Match3.ECS.PieceColorConfig.ColorEntry { colorType = Game.ColorTypeECS.Green,  fallbackColor = new UnityEngine.Color(0.3f, 0.9f, 0.4f) },
                new Match3.ECS.PieceColorConfig.ColorEntry { colorType = Game.ColorTypeECS.Pink,   fallbackColor = new UnityEngine.Color(1f, 0.5f, 0.8f) },
            };

            AssetDatabase.CreateAsset(config, "Assets/Resources/PieceColorConfig.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("[SubSceneSetup] PieceColorConfig created at Assets/Resources/PieceColorConfig.asset");
        }

}
}
