#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Protogenesis.V5.EditorTools
{
    public static class V5PrototypeSceneMenu
    {
        [MenuItem("Protogenesis/V5/Create Full Prototype Scene")]
        public static void CreateScene()
        {
            CreateSceneWithScenario(V5ScenarioId.FirstDrop, "V5_FullPrototype.unity");
        }

        [MenuItem("Protogenesis/V5/Create Scenario/First Drop")]
        public static void CreateFirstDrop()
        {
            CreateSceneWithScenario(V5ScenarioId.FirstDrop, "V5_FirstDrop.unity");
        }

        [MenuItem("Protogenesis/V5/Create Scenario/Oxygen War")]
        public static void CreateOxygenWar()
        {
            CreateSceneWithScenario(V5ScenarioId.OxygenWar, "V5_OxygenWar.unity");
        }

        [MenuItem("Protogenesis/V5/Create Scenario/Acid Frontier")]
        public static void CreateAcidFrontier()
        {
            CreateSceneWithScenario(V5ScenarioId.AcidFrontier, "V5_AcidFrontier.unity");
        }

        [MenuItem("Protogenesis/V5/Create Scenario/Predator Bloom")]
        public static void CreatePredatorBloom()
        {
            CreateSceneWithScenario(V5ScenarioId.PredatorBloom, "V5_PredatorBloom.unity");
        }

        [MenuItem("Protogenesis/V5/Create Scenario/Freeplay Sandbox")]
        public static void CreateFreeplay()
        {
            CreateSceneWithScenario(V5ScenarioId.Freeplay, "V5_Freeplay.unity");
        }

        private static void CreateSceneWithScenario(V5ScenarioId scenario, string fileName)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject bootstrap = new GameObject("V5_FULL_GAME_BOOTSTRAP_" + scenario);
            V5GameBootstrap b = bootstrap.AddComponent<V5GameBootstrap>();
            b.scenario = scenario;
            b.useScenarioPreset = true;
            Selection.activeGameObject = bootstrap;
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/_ProjectV5/Scenes/" + fileName);
            Debug.Log("V5 1.0 scenario scene created: " + scenario + ". Press Play to start.");
        }
    }
}
#endif
