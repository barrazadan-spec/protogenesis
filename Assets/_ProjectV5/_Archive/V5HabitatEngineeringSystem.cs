using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5HabitatProject
    {
        NutrientBloom,
        OxygenPocket,
        DetoxBiofilm,
        AcidBuffer,
        LightLens,
        DefensiveMatrix
    }

    /// <summary>
    /// Player-facing world-builder layer. It lets the colony spend cellular resources to engineer
    /// local habitat conditions instead of only reacting to the environment.
    /// Toggle with 0, execute selected project at mouse with Shift+0.
    /// </summary>
    public class V5HabitatEngineeringSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public V5HabitatProject SelectedProject = V5HabitatProject.NutrientBloom;
        public string LastAction = "Habitat engineering listo.";
        public int ProjectsBuilt { get; private set; }
        public float CooldownRemaining { get; private set; }

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;
        private Vector2 scroll;

        private void Update()
        {
            if (CooldownRemaining > 0f) CooldownRemaining = Mathf.Max(0f, CooldownRemaining - Time.deltaTime);

            // Alpha0/Keypad0 toggle removed — CommandCenter also used 0; open via Paneles menu
            // Shift+0 execute still works:
            bool zeroPressed = Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0);
            if (zeroPressed && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                TryExecuteAtMouse();
        }

        public bool CanExecute(V5HabitatProject project, V5CellEntity mother, out string reason)
        {
            reason = "OK";
            if (mother == null) { reason = "No hay célula madre."; return false; }
            if (CooldownRemaining > 0f) { reason = "En cooldown: " + CooldownRemaining.ToString("0.0") + "s"; return false; }
            V5ResourceWallet cost = Cost(project);
            if (!mother.Resources.CanPay(cost)) { reason = "Recursos insuficientes: " + CostText(cost); return false; }
            return true;
        }

        public bool TryExecuteAtMouse()
        {
            Vector2 pos = WorldMouseOrMother();
            return TryExecute(SelectedProject, pos);
        }

        public bool TryExecute(V5HabitatProject project, Vector2 world)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null || gm.Environment == null) return false;

            string reason;
            if (!CanExecute(project, gm.MotherCell, out reason))
            {
                LastAction = reason;
                Toast(reason);
                return false;
            }

            V5ResourceWallet cost = Cost(project);
            gm.MotherCell.Resources.Pay(cost);
            ApplyProject(project, gm.Environment, world, gm.MotherCell);
            ProjectsBuilt++;
            CooldownRemaining = project == V5HabitatProject.DefensiveMatrix ? 14f : 9f;
            LastAction = DisplayName(project) + " aplicado en " + world.ToString("0.0") + ".";
            Toast(LastAction);

            V5FeedbackSystem fb = FindFirstObjectByType<V5FeedbackSystem>();
            if (fb != null) fb.Push(DisplayName(project), world, new Color(0.7f, 1f, 0.9f, 1f));
            if (gm.Codex != null) gm.Codex.Unlock("Ingeniería de hábitat", "La colonia puede gastar recursos para terraformar microzonas: nutrientes, oxígeno, toxinas, pH, luz y matriz defensiva.");
            return true;
        }

        private void ApplyProject(V5HabitatProject project, V5EnvironmentGrid env, Vector2 world, V5CellEntity mother)
        {
            switch (project)
            {
                case V5HabitatProject.NutrientBloom:
                    env.ModifyArea(world, 4.4f, 0.22f, 0f, 0.015f, -0.015f, 0f, 0.015f, 0.04f);
                    break;
                case V5HabitatProject.OxygenPocket:
                    env.ModifyArea(world, 4.0f, -0.015f, 0.02f, 0.25f, -0.025f, -0.005f, 0.01f, 0f);
                    break;
                case V5HabitatProject.DetoxBiofilm:
                    env.ModifyArea(world, 4.2f, -0.01f, 0f, 0.035f, -0.30f, -0.01f, 0.08f, -0.04f);
                    break;
                case V5HabitatProject.AcidBuffer:
                    env.ModifyArea(world, 4.6f, 0.02f, 0f, 0.015f, -0.02f, -0.22f, 0.025f, -0.02f);
                    break;
                case V5HabitatProject.LightLens:
                    env.ModifyArea(world, 4.0f, -0.02f, 0.30f, 0.05f, 0f, 0f, 0.01f, 0f);
                    break;
                case V5HabitatProject.DefensiveMatrix:
                    env.ModifyArea(world, 5.2f, -0.015f, 0f, 0.025f, -0.08f, -0.01f, 0.16f, -0.025f);
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 7f);
                    mother.Stats.currentHp = Mathf.Min(mother.Stats.maxHp, mother.Stats.currentHp + 8f);
                    break;
            }
        }

        private Vector2 WorldMouseOrMother()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 m = Input.mousePosition;
                Vector3 w = cam.ScreenToWorldPoint(new Vector3(m.x, m.y, -cam.transform.position.z));
                return new Vector2(w.x, w.y);
            }
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.MotherCell != null) return gm.MotherCell.transform.position;
            return Vector2.zero;
        }

        private V5ResourceWallet Cost(V5HabitatProject project)
        {
            switch (project)
            {
                case V5HabitatProject.NutrientBloom: return V5ResourceWallet.Cost(18f, 8f, 4f, 2f, 1f, 3f);
                case V5HabitatProject.OxygenPocket: return V5ResourceWallet.Cost(22f, 7f, 3f, 3f, 1f, 2f);
                case V5HabitatProject.DetoxBiofilm: return V5ResourceWallet.Cost(24f, 10f, 5f, 6f, 1f, 2f);
                case V5HabitatProject.AcidBuffer: return V5ResourceWallet.Cost(20f, 8f, 3f, 4f, 1f, 5f);
                case V5HabitatProject.LightLens: return V5ResourceWallet.Cost(26f, 7f, 4f, 3f, 2f, 4f);
                case V5HabitatProject.DefensiveMatrix: return V5ResourceWallet.Cost(34f, 14f, 6f, 9f, 3f, 4f);
                default: return V5ResourceWallet.Cost(20f, 8f, 4f, 4f, 1f, 2f);
            }
        }

        private string DisplayName(V5HabitatProject project)
        {
            switch (project)
            {
                case V5HabitatProject.NutrientBloom: return "Bloom nutritivo";
                case V5HabitatProject.OxygenPocket: return "Bolsa de oxígeno";
                case V5HabitatProject.DetoxBiofilm: return "Biofilm detox";
                case V5HabitatProject.AcidBuffer: return "Buffer ácido";
                case V5HabitatProject.LightLens: return "Lente de luz";
                case V5HabitatProject.DefensiveMatrix: return "Matriz defensiva";
                default: return project.ToString();
            }
        }

        private string Description(V5HabitatProject project)
        {
            switch (project)
            {
                case V5HabitatProject.NutrientBloom: return "Sube nutrientes y detritus útil. Ideal cuando falta biomasa.";
                case V5HabitatProject.OxygenPocket: return "Crea oxígeno local. Favorece respiración y reduce presión anaerobia.";
                case V5HabitatProject.DetoxBiofilm: return "Limpia toxinas y aumenta colonización. Bueno contra blooms tóxicos.";
                case V5HabitatProject.AcidBuffer: return "Acerca el pH a zona habitable y reduce daño por acidificación.";
                case V5HabitatProject.LightLens: return "Aumenta luz local. Excelente para fotosíntesis/cianobacteria.";
                case V5HabitatProject.DefensiveMatrix: return "Fortifica territorio, baja stress y repara a la madre.";
                default: return "Proyecto de hábitat.";
            }
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(Screen.width - 460f, 86f, 440f, 520f);
            GUI.Box(r, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 10f, r.width - 24f, r.height - 20f));
            GUILayout.Label("INGENIERÍA DE HÁBITAT — 1.8", titleStyle);
            GUILayout.Label("0 abre/cierra | Shift+0 aplica proyecto en el mouse. Convierte recursos celulares en cambios del mundo.", bodyStyle);
            GUILayout.Space(6f);
            GUILayout.Label("Proyecto activo: " + DisplayName(SelectedProject) + " | Cooldown " + CooldownRemaining.ToString("0.0") + "s", bodyStyle);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(310f));
            DrawProjectButton(V5HabitatProject.NutrientBloom);
            DrawProjectButton(V5HabitatProject.OxygenPocket);
            DrawProjectButton(V5HabitatProject.DetoxBiofilm);
            DrawProjectButton(V5HabitatProject.AcidBuffer);
            DrawProjectButton(V5HabitatProject.LightLens);
            DrawProjectButton(V5HabitatProject.DefensiveMatrix);
            GUILayout.EndScrollView();
            GUILayout.Space(8f);
            if (GUILayout.Button("Aplicar en posición del mouse", buttonStyle, GUILayout.Height(34f))) TryExecuteAtMouse();
            GUILayout.Label("Última acción: " + LastAction, bodyStyle);
            GUILayout.Label("Proyectos construidos: " + ProjectsBuilt, bodyStyle);
            if (GUILayout.Button("Cerrar", buttonStyle)) ShowPanel = false;
            GUILayout.EndArea();
        }

        private void DrawProjectButton(V5HabitatProject project)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            string reason;
            bool can = CanExecute(project, mother, out reason);
            GUI.enabled = true;
            string prefix = SelectedProject == project ? "✓ " : "";
            if (GUILayout.Button(prefix + DisplayName(project) + "\n" + Description(project) + "\nCoste: " + CostText(Cost(project)) + (can ? "" : "\nBloqueado: " + reason), buttonStyle, GUILayout.Height(74f)))
            {
                SelectedProject = project;
            }
        }

        private string CostText(V5ResourceWallet c)
        {
            return string.Format("ATP {0:0} Bio {1:0} AA {2:0} Lip {3:0} NT {4:0} Min {5:0}", c.atp, c.biomass, c.aminoAcids, c.lipids, c.nucleotides, c.minerals);
        }

        private void Toast(string msg)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(msg);
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            titleStyle = new GUIStyle(GUI.skin.label); titleStyle.fontStyle = FontStyle.Bold; titleStyle.fontSize = 17; titleStyle.normal.textColor = new Color(0.86f, 1f, 1f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label); bodyStyle.wordWrap = true; bodyStyle.normal.textColor = Color.white;
            buttonStyle = new GUIStyle(GUI.skin.button); buttonStyle.wordWrap = true; buttonStyle.fontSize = 12;
        }
    }
}
