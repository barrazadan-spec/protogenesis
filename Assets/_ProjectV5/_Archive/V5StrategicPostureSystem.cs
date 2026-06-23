using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5StrategicPostureSystem : MonoBehaviour
    {
        public enum Posture
        {
            Balanced,
            Expansion,
            Fortify,
            Predation,
            Terraform,
            Recovery
        }

        public Posture CurrentPosture = Posture.Balanced;
        public string LastEffect = "sin postura aplicada";
        public float PostureIntensity = 1f;

        private bool showPanel;
        private float tick;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;

        private void Start() => V5PanelRouter.Register("Postura", () => showPanel, v => showPanel = v);

        private void Update()
        {
            // Home removed — EvolutionRouteBoardSystem also uses it; open via Paneles menu
            tick += Time.deltaTime;
            if (tick >= 0.65f)
            {
                tick = 0f;
                ApplyPostureTick();
            }
        }

        private void ApplyPostureTick()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.PlayerCells == null) return;
            V5EnvironmentGrid env = gm.Environment;
            V5CellEntity mother = gm.MotherCell;
            int applied = 0;

            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity cell = gm.PlayerCells[i];
                if (cell == null) continue;
                applied++;
                if (CurrentPosture == Posture.Expansion)
                {
                    if (env != null && (cell.Directive == V5Directive.Colonize || cell.LineageRole == V5LineageRole.Colonizer))
                        env.ModifyArea(cell.transform.position, 2.1f, -0.0005f, 0f, 0.0006f, -0.0008f, 0f, 0.0028f * PostureIntensity, 0f);
                    cell.Stats.stress = Mathf.Clamp(cell.Stats.stress + 0.015f * PostureIntensity, 0f, 100f);
                }
                else if (CurrentPosture == Posture.Fortify)
                {
                    if (mother != null && Vector2.Distance(cell.transform.position, mother.transform.position) < 6.5f)
                    {
                        cell.Stats.currentHp = Mathf.Min(cell.Stats.maxHp, cell.Stats.currentHp + 0.28f * PostureIntensity);
                        cell.Stats.stress = Mathf.Max(0f, cell.Stats.stress - 0.06f * PostureIntensity);
                    }
                    if (env != null && (cell.Directive == V5Directive.Defend || cell.Role == V5CellRole.Mother))
                        env.ModifyArea(cell.transform.position, 1.6f, 0f, 0f, 0f, -0.0015f * PostureIntensity, 0f, 0.0008f * PostureIntensity, 0f);
                }
                else if (CurrentPosture == Posture.Predation)
                {
                    if (cell.Directive == V5Directive.Attack || cell.LineageRole == V5LineageRole.Predator)
                        ApplyPredationPulse(cell, gm);
                    cell.Resources.atp = Mathf.Max(0f, cell.Resources.atp - 0.018f * PostureIntensity);
                }
                else if (CurrentPosture == Posture.Terraform)
                {
                    if (env == null) continue;
                    if (cell.Metabolism == V5MetabolismType.Photosynthesis || cell.HasStructure(V5StructureId.Thylakoid))
                        env.ModifyArea(cell.transform.position, 2.4f, -0.0004f, 0.0002f, 0.0035f * PostureIntensity, -0.001f, 0f, 0.001f, 0f);
                    else if (cell.Metabolism == V5MetabolismType.Chemolithotrophy || cell.EvolutionPath == V5EvolutionPath.Archaea)
                        env.ModifyArea(cell.transform.position, 2.2f, -0.0002f, 0f, 0f, 0.0005f, 0.0023f * PostureIntensity, 0.0006f, 0f);
                    else
                        env.ModifyArea(cell.transform.position, 1.8f, 0f, 0f, 0.0009f * PostureIntensity, -0.0016f * PostureIntensity, 0f, 0.0007f, 0f);
                }
                else if (CurrentPosture == Posture.Recovery)
                {
                    cell.Stats.stress = Mathf.Max(0f, cell.Stats.stress - 0.12f * PostureIntensity);
                    cell.Stats.currentHp = Mathf.Min(cell.Stats.maxHp, cell.Stats.currentHp + 0.18f * PostureIntensity);
                    cell.Resources.atp = Mathf.Max(0f, cell.Resources.atp - 0.025f * PostureIntensity);
                }
            }

            LastEffect = string.Format("{0}: {1} células afectadas", CurrentPosture, applied);
        }

        private void ApplyPredationPulse(V5CellEntity cell, V5GameManager gm)
        {
            IReadOnlyList<V5CellEntity> enemies = gm.NonPlayerCells;
            for (int i = 0; i < enemies.Count; i++)
            {
                V5CellEntity enemy = enemies[i];
                if (enemy == null) continue;
                float distance = Vector2.Distance(cell.transform.position, enemy.transform.position);
                if (distance <= Mathf.Max(1.2f, cell.Stats.attackRange + 0.4f))
                {
                    float damage = (0.18f + cell.Stats.physicalDamagePerSecond * 0.04f) * PostureIntensity;
                    enemy.Damage(damage, V5DamageKind.Physical, cell.transform.position);
                    if (cell.HasPhagocytosis || cell.HasStructure(V5StructureId.Lysosome))
                        cell.Resources.biomass += 0.025f * PostureIntensity;
                }
            }
        }

        private void OnGUI()
        {
            if (!showPanel) return;
            EnsureStyles();
            Rect r = new Rect(Screen.width - 390, 78, 375, 330);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "POSTURA ESTRATÉGICA — 1.6", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 20), "Activa una política global suave para toda la colonia.");
            GUI.Label(new Rect(r.x + 12, r.y + 62, r.width - 24, 20), "Actual: " + CurrentPosture + " | Intensidad " + PostureIntensity.ToString("0.0"));
            PostureIntensity = GUI.HorizontalSlider(new Rect(r.x + 12, r.y + 88, r.width - 24, 20), PostureIntensity, 0.4f, 1.8f);

            float y = r.y + 114;
            DrawPostureButton(r, ref y, Posture.Balanced, "Sin sesgo. Útil cuando no sabes qué optimizar.");
            DrawPostureButton(r, ref y, Posture.Expansion, "+colonización, +riesgo de stress. Ideal para cerrar victoria de mapa.");
            DrawPostureButton(r, ref y, Posture.Fortify, "+reparación y detox cerca de la madre. Defensa y estabilización.");
            DrawPostureButton(r, ref y, Posture.Predation, "+presión de contacto en atacantes. Consume ATP.");
            DrawPostureButton(r, ref y, Posture.Terraform, "Refuerza el metabolismo como herramienta de world builder.");
            DrawPostureButton(r, ref y, Posture.Recovery, "Baja stress y cura lentamente a costa de ATP.");
            GUI.Label(new Rect(r.x + 12, r.y + r.height - 26, r.width - 24, 20), LastEffect);
        }

        private void DrawPostureButton(Rect r, ref float y, Posture posture, string desc)
        {
            if (GUI.Button(new Rect(r.x + 12, y, 118, 28), posture.ToString(), button)) SetPosture(posture);
            GUI.Label(new Rect(r.x + 138, y + 2, r.width - 150, 28), desc);
            y += 34;
        }

        public void SetPosture(Posture posture)
        {
            CurrentPosture = posture;
            LastEffect = "Postura cambiada a " + posture;
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(LastEffect);
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.normal.textColor = Color.white; box.fontSize = 12;
            title = new GUIStyle(GUI.skin.label); title.fontSize = 16; title.fontStyle = FontStyle.Bold; title.normal.textColor = new Color(0.85f, 1f, 1f, 1f);
            button = new GUIStyle(GUI.skin.button); button.fontSize = 12; button.wordWrap = true;
        }
    }
}
