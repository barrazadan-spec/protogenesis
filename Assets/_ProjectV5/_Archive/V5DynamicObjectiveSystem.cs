using System;
using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5SideObjectiveKind
    {
        ColonizePatch,
        DetoxArea,
        DefeatThreats,
        StockpileATP,
        DiversifyLineages,
        StabilizeStress,
        RevealMap
    }

    [Serializable]
    public class V5SideObjective
    {
        public V5SideObjectiveKind kind;
        public string title;
        public string description;
        public float target;
        public float progress;
        public float timeLimit;
        public float age;
        public bool completed;
        public bool failed;
    }

    public class V5DynamicObjectiveSystem : MonoBehaviour
    {
        public bool Visible;
        public readonly List<V5SideObjective> ActiveObjectives = new List<V5SideObjective>();
        public int CompletedCount;
        public int FailedCount;

        private float spawnTimer;
        private float tick;
        private Vector2 scroll;

        private void Update()
        {
            // Delete removed — CounterplayIntelSystem also used it; open via Paneles menu
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Paused || gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat) return;

            tick += Time.deltaTime;
            spawnTimer += Time.deltaTime;
            if (spawnTimer > 125f && ActiveObjectives.Count < 3)
            {
                spawnTimer = 0f;
                SpawnObjective();
            }
            if (tick >= 0.5f)
            {
                tick = 0f;
                TickObjectives(0.5f);
            }
        }

        public void SpawnObjective()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            V5SideObjective o = new V5SideObjective();
            int roll = UnityEngine.Random.Range(0, 7);
            if (gm.PlayerCellCount() < 4) roll = 3;
            if (gm.Environment != null && gm.Environment.AverageColonization() < 0.12f) roll = 0;
            if (gm.NonPlayerCells.Count > 8 && UnityEngine.Random.value < 0.45f) roll = 2;

            o.kind = (V5SideObjectiveKind)roll;
            switch (o.kind)
            {
                case V5SideObjectiveKind.ColonizePatch:
                    o.title = "Colonizar microzona"; o.description = "Aumenta la matriz colonial del ecosistema."; o.target = 0.08f; o.timeLimit = 240f; break;
                case V5SideObjectiveKind.DetoxArea:
                    o.title = "Detoxificar entorno"; o.description = "Reduce toxinas promedio antes de que se propague la crisis."; o.target = 0.08f; o.timeLimit = 220f; break;
                case V5SideObjectiveKind.DefeatThreats:
                    o.title = "Eliminar amenazas"; o.description = "Destruye organismos hostiles cercanos para liberar presión."; o.target = Mathf.Max(2, Mathf.Min(6, gm.NonPlayerCells.Count / 2)); o.timeLimit = 260f; break;
                case V5SideObjectiveKind.StockpileATP:
                    o.title = "Reserva energética"; o.description = "Acumula ATP suficiente en la madre para una expansión segura."; o.target = 160f; o.timeLimit = 210f; break;
                case V5SideObjectiveKind.DiversifyLineages:
                    o.title = "Diversificar linajes"; o.description = "Mantén varias funciones celulares activas."; o.target = 4f; o.timeLimit = 260f; break;
                case V5SideObjectiveKind.StabilizeStress:
                    o.title = "Bajar stress colonial"; o.description = "Mantén el stress medio bajo para evitar mutaciones negativas."; o.target = 22f; o.timeLimit = 180f; break;
                case V5SideObjectiveKind.RevealMap:
                    o.title = "Explorar la gota"; o.description = "Aumenta mapa revelado para encontrar nichos y amenazas."; o.target = 0.55f; o.timeLimit = 240f; break;
            }
            ActiveObjectives.Add(o);
            if (gm.Hud != null) gm.Hud.Toast("Nuevo objetivo secundario: " + o.title);
        }

        private void TickObjectives(float dt)
        {
            for (int i = ActiveObjectives.Count - 1; i >= 0; i--)
            {
                V5SideObjective o = ActiveObjectives[i];
                o.age += dt;
                o.progress = Evaluate(o);
                if (!o.completed && IsComplete(o))
                {
                    o.completed = true;
                    CompletedCount++;
                    Reward(o);
                    ActiveObjectives.RemoveAt(i);
                    continue;
                }
                if (o.age > o.timeLimit)
                {
                    o.failed = true;
                    FailedCount++;
                    ApplyFailure(o);
                    ActiveObjectives.RemoveAt(i);
                }
            }
        }

        private float Evaluate(V5SideObjective o)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return 0f;
            V5CellEntity mother = gm.MotherCell;
            switch (o.kind)
            {
                case V5SideObjectiveKind.ColonizePatch: return gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
                case V5SideObjectiveKind.DetoxArea: return gm.Environment != null ? Mathf.Max(0f, 0.18f - gm.Environment.AverageToxins()) : 0f;
                case V5SideObjectiveKind.DefeatThreats: return Mathf.Max(0f, o.target - gm.NonPlayerCells.Count);
                case V5SideObjectiveKind.StockpileATP: return mother != null ? mother.Resources.atp : 0f;
                case V5SideObjectiveKind.DiversifyLineages: return CountDistinctRoles(gm);
                case V5SideObjectiveKind.StabilizeStress: return mother != null ? Mathf.Max(0f, 100f - mother.Stats.stress) : 0f;
                case V5SideObjectiveKind.RevealMap: return gm.Fog != null ? gm.Fog.DiscoveredPercent : 0f;
                default: return 0f;
            }
        }

        private bool IsComplete(V5SideObjective o)
        {
            if (o.kind == V5SideObjectiveKind.StabilizeStress) return o.progress >= 100f - o.target;
            return o.progress >= o.target;
        }

        private int CountDistinctRoles(V5GameManager gm)
        {
            bool[] seen = new bool[Enum.GetValues(typeof(V5LineageRole)).Length];
            int count = 0;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null) continue;
                int idx = (int)c.LineageRole;
                if (idx >= 0 && idx < seen.Length && !seen[idx]) { seen[idx] = true; count++; }
            }
            return count;
        }

        private void Reward(V5SideObjective o)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;
            mother.Resources.atp += 25f;
            mother.Resources.biomass += 10f;
            mother.Resources.aminoAcids += 8f;
            mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 8f);
            if (gm.Hud != null) gm.Hud.Toast("Objetivo completado: " + o.title + " (+recursos)");
            if (gm.Codex != null) gm.Codex.Unlock("Objetivos dinámicos", "Completaste contratos secundarios que empujan la ecología de la run.");
        }

        private void ApplyFailure(V5SideObjective o)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.MotherCell != null) gm.MotherCell.Stats.stress = Mathf.Min(100f, gm.MotherCell.Stats.stress + 6f);
            if (gm != null && gm.Hud != null) gm.Hud.Toast("Objetivo fallido: " + o.title);
        }

        private void OnGUI()
        {
            if (!Visible) return;
            GUILayout.BeginArea(new Rect(500, 70, 420, 400), GUI.skin.box);
            GUILayout.Label("OBJETIVOS SECUNDARIOS DINÁMICOS 1.4");
            GUILayout.Label("Delete: cerrar · Completados " + CompletedCount + " | Fallidos " + FailedCount);
            if (GUILayout.Button("Forzar nuevo objetivo")) SpawnObjective();
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(310));
            for (int i = 0; i < ActiveObjectives.Count; i++)
            {
                V5SideObjective o = ActiveObjectives[i];
                float pct = o.kind == V5SideObjectiveKind.StabilizeStress ? Mathf.Clamp01(o.progress / Mathf.Max(1f, 100f - o.target)) : Mathf.Clamp01(o.progress / Mathf.Max(1f, o.target));
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(o.title + " — " + Mathf.RoundToInt(pct * 100f) + "%");
                GUILayout.Label(o.description);
                GUILayout.Label("Tiempo: " + Mathf.Max(0f, o.timeLimit - o.age).ToString("0") + "s");
                GUILayout.EndVertical();
            }
            if (ActiveObjectives.Count == 0) GUILayout.Label("Sin objetivos activos. Espera o fuerza uno.");
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
