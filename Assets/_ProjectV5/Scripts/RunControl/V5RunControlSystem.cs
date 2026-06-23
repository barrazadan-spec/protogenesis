using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Prototype 1.9: run-level alerting, victory-track clarity and one-click response tools.
    /// Kept as IMGUI/runtime-only so it can be dropped into any older V5 scene by the auto-installer.
    /// </summary>
    public class V5RunControlSystem : MonoBehaviour
    {
        [Serializable]
        private class AlertRecord
        {
            public string severity;
            public string title;
            public string detail;
        }

        [Serializable]
        private class VictoryTrackRecord
        {
            public string name;
            public float progress;
            public string advice;
        }

        [Serializable]
        private class RunControlSnapshot
        {
            public string version = "1.9";
            public string scenario;
            public string phase;
            public float elapsedSeconds;
            public float riskScore;
            public float colonization;
            public float oxygen;
            public float toxins;
            public float acidity;
            public int playerCells;
            public int enemies;
            public string bestVictoryTrack;
            public float bestVictoryProgress;
            public List<AlertRecord> alerts = new List<AlertRecord>();
            public List<VictoryTrackRecord> victoryTracks = new List<VictoryTrackRecord>();
        }

        private struct Alert
        {
            public int level;
            public string title;
            public string detail;
        }

        private struct VictoryTrack
        {
            public string name;
            public float progress;
            public string advice;
        }

        public bool ShowPanel;
        public float RiskScore { get; private set; }
        public string BestVictoryTrack { get; private set; } = "Ecological Dominance";
        public float BestVictoryProgress { get; private set; }
        public string LastAction { get; private set; } = "sin acción";
        public string LastExportPath { get; private set; } = "sin export";

        private readonly List<Alert> alerts = new List<Alert>(12);
        private readonly List<VictoryTrack> victoryTracks = new List<VictoryTrack>(6);
        private float tick;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle body;
        private GUIStyle warn;
        private GUIStyle critical;
        private GUIStyle button;

        private void Start()
        {
            ShowPanel = false;
            V5PanelRouter.Register("Run", () => ShowPanel, v => ShowPanel = v);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.CapsLock)) { if (!ShowPanel) V5PanelRouter.CloseOthers("Run"); ShowPanel = !ShowPanel; }
            tick += Time.deltaTime;
            if (tick >= 0.5f)
            {
                tick = 0f;
                Evaluate();
            }
        }

        private void Evaluate()
        {
            V5GameManager gm = V5GameManager.Instance;
            alerts.Clear();
            victoryTracks.Clear();
            RiskScore = 0f;
            BestVictoryProgress = 0f;
            BestVictoryTrack = "Ecological Dominance";
            if (gm == null) return;

            V5CellEntity mother = gm.MotherCell;
            V5EnvironmentGrid env = gm.Environment;
            float colonization = env != null ? env.AverageColonization() : 0f;
            float oxygen = env != null ? env.AverageOxygen() : 0f;
            float toxins = env != null ? env.AverageToxins() : 0f;
            float acidity = env != null ? env.AverageAcidity() : 0.5f;
            int enemies = gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0;
            int cells = gm.PlayerCells != null ? gm.PlayerCells.Count : 0;
            float populationLoad = gm.PlayerPopulationLoad();
            bool apex = gm.Apex != null && gm.Apex.ApexSpawned;

            if (mother == null)
            {
                AddAlert(3, "Madre perdida", "La run no tiene célula madre registrada.");
                RiskScore = 1f;
                return;
            }

            float hp01 = mother.Stats.maxHp > 0f ? mother.Stats.currentHp / mother.Stats.maxHp : 0f;
            if (hp01 < 0.25f) AddAlert(3, "Madre crítica", "HP de madre bajo 25%. Usa Emergency Repair o retira amenazas.");
            else if (hp01 < 0.50f) AddAlert(2, "Madre dañada", "HP de madre bajo 50%. Repara o asigna defensoras.");

            if (mother.Stats.stress > 90f) AddAlert(3, "Stress extremo", "La madre puede entrar en lisis/colapso. Baja población o estabiliza ambiente.");
            else if (mother.Stats.stress > 70f) AddAlert(2, "Stress alto", "La colonia está forzando demasiado la biología de la madre.");

            if (mother.Resources.atp < 18f) AddAlert(2, "ATP bajo", "No tendrás margen para dividir, reparar o responder a crisis.");
            if (mother.Resources.biomass < 12f) AddAlert(2, "Biomasa baja", "Faltan materiales para división y estructuras.");
            if (toxins > 0.42f) AddAlert(3, "Toxicidad global", "La gota está envenenada. Prioriza catalasa, detox o biofilm.");
            else if (toxins > 0.28f) AddAlert(2, "Toxinas subiendo", "La química empieza a castigar células no resistentes.");
            if (Mathf.Abs(acidity - 0.5f) > 0.34f) AddAlert(2, "pH inestable", "La acidez media se aleja de zona neutra.");
            if (oxygen < 0.16f && mother.Domain == V5CellDomain.Eukaryote) AddAlert(2, "Oxígeno bajo", "Respiradores/eucariotas pierden eficiencia.");
            if (cells >= V5Balance.HardControllableEntityCap - 1 || populationLoad >= V5Balance.HardPopulationLoad - 1f) AddAlert(2, "Cap de control cercano", "Reabsorbe, divide tareas o especializa antes de bloquear division.");
            else if (populationLoad > V5Balance.SoftPopulationLoad) AddAlert(1, "Carga biologica alta", "La carga extra agrega stress progresivo; los swarms dependen de densidad y espacio.");

            float nearestEnemy = NearestEnemyDistanceTo(mother.transform.position, gm);
            if (nearestEnemy < 4f) AddAlert(3, "Amenaza sobre la madre", "Un enemigo está a menos de 4u. Activa defensa o ataque prioritario.");
            else if (nearestEnemy < 8f) AddAlert(2, "Amenaza cercana", "Hay enemigos acercándose al núcleo de la colonia.");

            float ecologicalDominance = Mathf.Clamp01(colonization / 0.40f);
            float stableBiosphere = Mathf.Clamp01(colonization / 0.30f) * 0.28f
                + Mathf.Clamp01(oxygen / 0.42f) * 0.22f
                + Mathf.Clamp01((0.35f - toxins) / 0.35f) * 0.24f
                + Mathf.Clamp01(1f - Mathf.Abs(acidity - 0.52f) * 2.3f) * 0.26f;
            float predatorClear = Mathf.Clamp01(1f - enemies / 16f) * 0.70f + Mathf.Clamp01(cells / 10f) * 0.30f;
            float apexAscension = apex ? 1f : Mathf.Clamp01(gm.ElapsedSeconds / V5Balance.ApexMinimumTime) * 0.25f
                + Mathf.Clamp01(mother.Resources.atp / V5Balance.ApexCostATP) * 0.24f
                + Mathf.Clamp01(mother.Resources.biomass / V5Balance.ApexCostBiomass) * 0.22f
                + ((gm.Genes != null && gm.Genes.HasGene(V5GeneId.ApexMaturation)) ? 0.29f : 0f);

            AddTrack("Dominancia ecológica", ecologicalDominance, "Coloniza hasta 40%. Usa modo 5, biofilm y proyectos de hábitat.");
            AddTrack("Biosfera estable", Mathf.Clamp01(stableBiosphere), "Mantén colonización, O2, toxinas bajas y pH moderado.");
            AddTrack("Limpieza predatoria", Mathf.Clamp01(predatorClear), "Reduce amenazas activas y protege la madre.");
            AddTrack("Ascensión apex", Mathf.Clamp01(apexAscension), "Acumula recursos, activa Apex Maturation y presiona P.");

            for (int i = 0; i < victoryTracks.Count; i++)
            {
                if (victoryTracks[i].progress > BestVictoryProgress)
                {
                    BestVictoryProgress = victoryTracks[i].progress;
                    BestVictoryTrack = victoryTracks[i].name;
                }
            }

            float risk = 0f;
            risk += Mathf.Clamp01((1f - hp01) * 0.35f);
            risk += Mathf.Clamp01(mother.Stats.stress / 100f) * 0.22f;
            risk += Mathf.Clamp01(toxins / 0.55f) * 0.18f;
            risk += Mathf.Clamp01(Mathf.Abs(acidity - 0.5f) / 0.5f) * 0.12f;
            risk += nearestEnemy < 8f ? Mathf.Clamp01((8f - nearestEnemy) / 8f) * 0.13f : 0f;
            RiskScore = Mathf.Clamp01(risk);
        }

        private void AddTrack(string name, float progress, string advice)
        {
            VictoryTrack t = new VictoryTrack();
            t.name = name;
            t.progress = Mathf.Clamp01(progress);
            t.advice = advice;
            victoryTracks.Add(t);
        }

        private void AddAlert(int level, string title, string detail)
        {
            Alert a = new Alert();
            a.level = level;
            a.title = title;
            a.detail = detail;
            alerts.Add(a);
        }

        private float NearestEnemyDistanceTo(Vector2 point, V5GameManager gm)
        {
            if (gm == null || gm.NonPlayerCells == null || gm.NonPlayerCells.Count == 0) return 999f;
            float best = 999f;
            for (int i = 0; i < gm.NonPlayerCells.Count; i++)
            {
                V5CellEntity c = gm.NonPlayerCells[i];
                if (c == null) continue;
                best = Mathf.Min(best, Vector2.Distance(point, c.transform.position));
            }
            return best;
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(Screen.width - 440f, 190f, 430f, 430f);
            GUI.Box(r, GUIContent.none, box);
            GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 10f, r.width - 24f, r.height - 20f));
            GUILayout.Label("RUN CONTROL - DEBUG", title);
            GUILayout.Label("CapsLock oculta/muestra | Riesgo " + (RiskScore * 100f).ToString("0") + "% | Mejor cierre: " + BestVictoryTrack + " " + (BestVictoryProgress * 100f).ToString("0") + "%", body);
            GUILayout.Space(5f);

            GUILayout.Label("Alertas", title);
            if (alerts.Count == 0) GUILayout.Label("Sin alertas críticas. Mantén expansión y estabilidad.", body);
            int max = Mathf.Min(5, alerts.Count);
            for (int i = 0; i < max; i++)
            {
                GUIStyle s = alerts[i].level >= 3 ? critical : (alerts[i].level == 2 ? warn : body);
                GUILayout.Label("• " + alerts[i].title + ": " + alerts[i].detail, s);
            }

            GUILayout.Space(5f);
            GUILayout.Label("Rutas de victoria", title);
            for (int i = 0; i < victoryTracks.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(victoryTracks[i].name, GUILayout.Width(132f));
                Rect bar = GUILayoutUtility.GetRect(150f, 16f);
                GUI.Box(bar, GUIContent.none);
                Rect fill = new Rect(bar.x + 2f, bar.y + 2f, (bar.width - 4f) * victoryTracks[i].progress, bar.height - 4f);
                GUI.Box(fill, GUIContent.none);
                GUILayout.Label((victoryTracks[i].progress * 100f).ToString("0") + "%", GUILayout.Width(42f));
                GUILayout.EndHorizontal();
            }
            if (victoryTracks.Count > 0)
            {
                GUILayout.Label("Consejo: " + BestAdvice(), body);
            }

            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Defender madre", button)) AssignDefenders();
            if (GUILayout.Button("Mandar farmers", button)) AssignFarmers();
            if (GUILayout.Button("Estabilizar", button)) StabilizeMotherArea();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Priorizar amenaza", button)) FocusNearestThreat();
            if (GUILayout.Button("Exportar snapshot", button)) ExportSnapshot();
            GUILayout.EndHorizontal();
            GUILayout.Label("Última acción: " + LastAction, body);
            GUILayout.Label("Export: " + LastExportPath, body);
            GUILayout.EndArea();
        }

        private string BestAdvice()
        {
            for (int i = 0; i < victoryTracks.Count; i++)
                if (victoryTracks[i].name == BestVictoryTrack) return victoryTracks[i].advice;
            return "Sigue expandiendo y estabilizando la gota.";
        }

        private void AssignDefenders()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            int changed = 0;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                if (c.LineageRole == V5LineageRole.Defender || changed < 3)
                {
                    c.Directive = V5Directive.Defend;
                    c.Mother = gm.MotherCell;
                    changed++;
                }
            }
            LastAction = "defensa asignada a " + changed + " células";
            Toast(LastAction);
        }

        private void AssignFarmers()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            int changed = 0;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                if (c.LineageRole == V5LineageRole.Farmer || changed < 3)
                {
                    c.Directive = V5Directive.Farm;
                    c.Mother = gm.MotherCell;
                    changed++;
                }
            }
            LastAction = "farmeo asignado a " + changed + " células";
            Toast(LastAction);
        }

        private void FocusNearestThreat()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null || gm.NonPlayerCells == null) return;
            V5CellEntity target = null;
            float best = 999f;
            for (int i = 0; i < gm.NonPlayerCells.Count; i++)
            {
                V5CellEntity e = gm.NonPlayerCells[i];
                if (e == null) continue;
                float d = Vector2.Distance(gm.MotherCell.transform.position, e.transform.position);
                if (d < best) { best = d; target = e; }
            }
            if (target == null)
            {
                LastAction = "no hay amenaza activa";
                Toast(LastAction);
                return;
            }
            int assigned = 0;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                if (c.LineageRole == V5LineageRole.Predator || c.LineageRole == V5LineageRole.Defender || assigned < 4)
                {
                    c.Directive = V5Directive.Attack;
                    c.AttackTarget = target;
                    assigned++;
                }
            }
            LastAction = "objetivo prioritario asignado a " + assigned + " células";
            Toast(LastAction);
        }

        private void StabilizeMotherArea()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null || gm.Environment == null) return;
            V5CellEntity m = gm.MotherCell;
            V5ResourceWallet cost = V5ResourceWallet.Cost(18f, 4f, 3f, 5f, 0f, 2f);
            if (!m.Resources.CanPay(cost))
            {
                LastAction = "faltan recursos para estabilizar";
                Toast(LastAction);
                return;
            }
            m.Resources.Pay(cost);
            m.Stats.stress = Mathf.Max(0f, m.Stats.stress - 18f);
            m.Stats.currentHp = Mathf.Min(m.Stats.maxHp, m.Stats.currentHp + 14f);
            gm.Environment.ModifyArea(m.transform.position, 5.0f, 0.025f, 0f, 0.025f, -0.070f, -0.018f, 0.018f, -0.025f);
            LastAction = "área de madre estabilizada";
            Toast(LastAction);
        }

        private void ExportSnapshot()
        {
            V5GameManager gm = V5GameManager.Instance;
            RunControlSnapshot snap = BuildSnapshot(gm);
            string json = JsonUtility.ToJson(snap, true);
            string path = Path.Combine(Application.persistentDataPath, "protogenesis_v5_runcontrol_1_9.json");
            File.WriteAllText(path, json);
            LastExportPath = path;
            LastAction = "snapshot 1.9 exportado";
            Toast(LastAction);
        }

        private RunControlSnapshot BuildSnapshot(V5GameManager gm)
        {
            RunControlSnapshot snap = new RunControlSnapshot();
            if (gm == null) return snap;
            snap.scenario = gm.ScenarioId.ToString();
            snap.phase = gm.Phase.ToString();
            snap.elapsedSeconds = gm.ElapsedSeconds;
            snap.riskScore = RiskScore;
            snap.playerCells = gm.PlayerCells != null ? gm.PlayerCells.Count : 0;
            snap.enemies = gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0;
            snap.bestVictoryTrack = BestVictoryTrack;
            snap.bestVictoryProgress = BestVictoryProgress;
            if (gm.Environment != null)
            {
                snap.colonization = gm.Environment.AverageColonization();
                snap.oxygen = gm.Environment.AverageOxygen();
                snap.toxins = gm.Environment.AverageToxins();
                snap.acidity = gm.Environment.AverageAcidity();
            }
            for (int i = 0; i < alerts.Count; i++)
            {
                AlertRecord r = new AlertRecord();
                r.severity = alerts[i].level >= 3 ? "critical" : (alerts[i].level == 2 ? "warning" : "info");
                r.title = alerts[i].title;
                r.detail = alerts[i].detail;
                snap.alerts.Add(r);
            }
            for (int i = 0; i < victoryTracks.Count; i++)
            {
                VictoryTrackRecord r = new VictoryTrackRecord();
                r.name = victoryTracks[i].name;
                r.progress = victoryTracks[i].progress;
                r.advice = victoryTracks[i].advice;
                snap.victoryTracks.Add(r);
            }
            return snap;
        }

        private void Toast(string text)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(text);
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.normal.textColor = Color.white; box.fontSize = 12;
            title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 15; title.normal.textColor = new Color(0.84f, 1f, 1f, 1f);
            body = new GUIStyle(GUI.skin.label); body.wordWrap = true; body.normal.textColor = Color.white; body.fontSize = 12;
            warn = new GUIStyle(body); warn.normal.textColor = new Color(1f, 0.82f, 0.35f, 1f);
            critical = new GUIStyle(body); critical.normal.textColor = new Color(1f, 0.45f, 0.42f, 1f); critical.fontStyle = FontStyle.Bold;
            button = new GUIStyle(GUI.skin.button); button.fontSize = 11; button.wordWrap = true;
        }
    }
}
