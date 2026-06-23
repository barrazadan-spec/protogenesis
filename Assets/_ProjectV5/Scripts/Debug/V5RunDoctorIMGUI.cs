using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Lightweight run doctor for playtesting: detects common dead-ends and offers one-key fixes.
    /// Toggle with Semicolon (;). Designed for prototype tuning, not final UI.
    /// </summary>
    public class V5RunDoctorIMGUI : MonoBehaviour
    {
        public bool ShowPanel;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private string lastDiagnosis = "";

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Semicolon)) ShowPanel = !ShowPanel;
            if (!ShowPanel) return;

            if (Input.GetKeyDown(KeyCode.F1)) FixResources();
            if (Input.GetKeyDown(KeyCode.F2)) FixStress();
            if (Input.GetKeyDown(KeyCode.F3)) FixEcology();
            if (Input.GetKeyDown(KeyCode.F4)) SpawnThreatProbe();
        }

        private void Diagnose(out string diagnosis)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null)
            {
                diagnosis = "Sin GameManager/madre. Crea escena desde Protogenesis > V5 > Create Full Prototype Scene.";
                return;
            }

            V5CellEntity mother = gm.MotherCell;
            string d = "Estado OK para playtest.";
            if (mother.Resources.atp < 18f) d = "ATP bajo: farmea, usa F1 o activa un gen metabolico en G.";
            if (mother.Resources.biomass < 14f) d = "Biomasa baja: farmear o F1 para continuar test.";
            if (mother.Stats.stress > 85f) d = "Stress crítico: usa R, F2 o reduce población.";
            if (mother.Stats.currentHp < mother.Stats.maxHp * 0.35f) d = "Madre dañada: usa R/F2 antes de seguir.";
            if (gm.Environment != null && gm.Environment.AverageToxins() > 0.55f) d = "El ecosistema está tóxico: catalasa, pulse homeostático o F3.";
            if (gm.PlayerCellCount() <= 1 && gm.ElapsedSeconds > 240f) d = "La run está estancada: divide con D o usa F1 para recursos.";
            if (gm.NonPlayerCells.Count == 0 && gm.ElapsedSeconds > 300f) d = "Sin presión enemiga: F4 genera amenaza de test.";
            diagnosis = d;
        }

        private void FixResources()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            gm.MotherCell.Resources.atp += 85f;
            gm.MotherCell.Resources.biomass += 55f;
            gm.MotherCell.Resources.aminoAcids += 35f;
            gm.MotherCell.Resources.lipids += 25f;
            gm.MotherCell.Resources.nucleotides += 18f;
            gm.MotherCell.Resources.minerals += 15f;
            Toast("Run Doctor: recursos de test añadidos");
        }

        private void FixStress()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] == null) continue;
                cells[i].Stats.stress = Mathf.Max(0f, cells[i].Stats.stress - 45f);
                cells[i].Stats.currentHp = Mathf.Min(cells[i].Stats.maxHp, cells[i].Stats.currentHp + 35f);
            }
            Toast("Run Doctor: stress/HP estabilizados");
        }

        private void FixEcology()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null || gm.MotherCell == null) return;
            gm.Environment.ModifyArea(gm.MotherCell.transform.position, 9f, 0.08f, 0.02f, 0.12f, -0.35f, -0.12f, 0.12f, -0.04f);
            Toast("Run Doctor: ecosistema local saneado");
        }

        private void SpawnThreatProbe()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.CellFactory == null || gm.MotherCell == null) return;
            Vector2 pos = (Vector2)gm.MotherCell.transform.position + Random.insideUnitCircle.normalized * 13f;
            V5EvolutionPath path = Random.value > 0.5f ? V5EvolutionPath.Amoeba : V5EvolutionPath.Bacteria;
            V5CellEntity enemy = gm.CellFactory.SpawnNeutral(pos, path);
            if (enemy != null && enemy.GetComponent<V5EnemyBrain>() == null) enemy.gameObject.AddComponent<V5EnemyBrain>();
            Toast("Run Doctor: amenaza de prueba generada");
        }

        private void Toast(string msg)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(msg);
            lastDiagnosis = msg;
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Diagnose(out lastDiagnosis);
            Rect r = new Rect(Screen.width - 430f, 128f, 410f, 196f);
            GUI.Box(r, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 10f, r.width - 24f, r.height - 20f));
            GUILayout.Label("RUN DOCTOR 0.9", titleStyle);
            GUILayout.Label(lastDiagnosis, bodyStyle);
            GUILayout.Space(8f);
            GUILayout.Label("; oculta/muestra | F1 recursos | F2 stress/HP | F3 sanea ecología | F4 amenaza", bodyStyle);
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.MotherCell != null)
            {
                V5CellEntity m = gm.MotherCell;
                GUILayout.Label("Madre HP " + m.Stats.currentHp.ToString("0") + "/" + m.Stats.maxHp.ToString("0") + "  Stress " + m.Stats.stress.ToString("0") + "  Células " + gm.PlayerCellCount(), bodyStyle);
            }
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = Texture2D.whiteTexture;
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(1f, 0.84f, 0.45f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.wordWrap = true;
            bodyStyle.normal.textColor = new Color(0.92f, 0.95f, 1f, 1f);
        }
    }
}
