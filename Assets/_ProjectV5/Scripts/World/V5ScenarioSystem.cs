using UnityEngine;

namespace Protogenesis.V5
{
    public class V5ScenarioSystem : MonoBehaviour
    {
        public string ObjectiveText = "";
        public string ScenarioName = "";
        public string ScenarioDescription = "";
        public float ObjectiveProgress01 { get; private set; }
        public V5ScenarioDefinition Definition { get; private set; }

        private V5ScenarioId scenario;
        private float timer;

        public void BeginScenario(V5ScenarioId id)
        {
            scenario = id;
            Definition = V5ScenarioLibrary.Get(id);
            ScenarioName = Definition.displayName;
            ScenarioDescription = Definition.description;
            ObjectiveText = Definition.primaryObjective;
            ObjectiveProgress01 = 0f;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < V5Balance.ObjectiveTick) return;
            timer = 0f;
            Evaluate();
        }

        private void Evaluate()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat) return;
            if (Definition == null) Definition = V5ScenarioLibrary.Get(scenario);
            if (gm.MotherCell == null)
            {
                gm.Lose("sin célula madre");
                return;
            }

            float colonized = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
            float discovered = gm.Fog != null ? gm.Fog.DiscoveredPercent : 0f;
            float oxygen = gm.Environment != null ? gm.Environment.AverageOxygen() : 0f;
            float target = Mathf.Max(0.01f, Definition.colonizationTarget);
            bool enoughColonized = colonized >= target;
            bool enoughDiscovered = discovered >= Definition.minimumDiscovered;
            bool apexOk = !Definition.requireApex || (gm.Apex != null && gm.Apex.ApexSpawned);
            bool enemiesOk = !Definition.requireEnemyClear || gm.NonPlayerCells.Count <= 3;
            bool oxygenWarOk = scenario != V5ScenarioId.OxygenWar || oxygen >= 0.38f;

            ObjectiveProgress01 = Mathf.Clamp01(colonized / target);
            ObjectiveText = Definition.displayName + ": " + (colonized * 100f).ToString("0") + "% / " + (target * 100f).ToString("0") + "% colonizado";
            if (Definition.minimumDiscovered > 0f) ObjectiveText += " | mapa " + (discovered * 100f).ToString("0") + "%";
            if (scenario == V5ScenarioId.OxygenWar) ObjectiveText += " | O₂ medio " + (oxygen * 100f).ToString("0") + "%";
            if (Definition.requireEnemyClear) ObjectiveText += " | amenazas " + gm.NonPlayerCells.Count;

            if (enoughColonized && enoughDiscovered && apexOk && enemiesOk && oxygenWarOk)
            {
                gm.Win("objetivo completado: " + Definition.displayName);
                return;
            }

            if (gm.NonPlayerCells.Count == 0 && gm.PlayerCellCount() >= 6 && gm.ElapsedSeconds > 180f && scenario == V5ScenarioId.FirstDrop)
            {
                gm.Win("ecosistema estabilizado");
                return;
            }

            if (gm.ElapsedSeconds > Definition.survivalSeconds)
            {
                if (colonized >= Definition.colonizationTarget * 0.75f || scenario == V5ScenarioId.Freeplay) gm.Win("supervivencia al cierre de la gota");
                else gm.Lose(Definition.failureText);
            }
        }
    }
}
