using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5HudIMGUI : MonoBehaviour
    {
        public string EndMessage = "";
        private bool showInterior;
        private bool showGenes;
        private bool showMission;
        private Vector2 scroll;
        private Vector2 codexScroll;
        private Vector2 geneScroll;
        private bool showCodex;
        private bool showAdvisor;
        private bool showLegacyStructureCatalog;
        private string toastMessage = "";
        private float toastUntil;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;
        private GUIStyle smallLabel;
        private GUIStyle ringHeader;
        private GUIStyle geneNode;

        public void Toast(string message)
        {
            toastMessage = message;
            toastUntil = Time.unscaledTime + 3.5f;
        }

        private void Start()
        {
            V5PanelRouter.Register("Interior", () => showInterior, v => showInterior = v);
            if (GenomePanel() == null) V5PanelRouter.Register("Genes", () => showGenes, v => showGenes = v);
            V5PanelRouter.Register("Mision", () => showMission, v => showMission = v);
            V5PanelRouter.Register("Codex", () => showCodex, v => showCodex = v);
            V5PanelRouter.Register("Consejo", () => showAdvisor, v => showAdvisor = v);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E)) ToggleHudPanel(ref showInterior, "Interior");
            if (Input.GetKeyDown(KeyCode.G))
            {
                V5GenomePanelIMGUI genome = GenomePanel();
                if (genome != null) genome.TogglePanel();
                else ToggleHudPanel(ref showGenes, "Genes");
            }
            if (Input.GetKeyDown(KeyCode.M)) ToggleHudPanel(ref showMission, "Mision");
            if (Input.GetKeyDown(KeyCode.C)) ToggleHudPanel(ref showCodex, "Codex");
            if (Input.GetKeyDown(KeyCode.H)) ToggleHudPanel(ref showAdvisor, "Consejo");
            if (Input.GetKeyDown(KeyCode.P) && V5GameManager.Instance != null && V5GameManager.Instance.Apex != null) V5GameManager.Instance.Apex.SpawnRecommended();
            if (Input.GetKeyDown(KeyCode.F) && V5GameManager.Instance != null && V5GameManager.Instance.CameraController != null)
            {
                V5GameManager.Instance.CameraController.FollowTarget = V5GameManager.Instance.MotherCell != null ? V5GameManager.Instance.MotherCell.transform : null;
            }
        }

        private void ToggleHudPanel(ref bool flag, string label)
        {
            bool next = !flag;
            if (next) V5PanelRouter.CloseOthers(label);
            flag = next;
        }

        private V5GenomePanelIMGUI GenomePanel()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.GenomePanel != null) return gm.GenomePanel;
            return FindFirstObjectByType<V5GenomePanelIMGUI>();
        }

        private void OnGUI()
        {
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            DrawTopHud(gm);
            DrawDirectiveBar(gm);
            if (showInterior) DrawInteriorPanel(gm);
            if (showGenes) DrawGenePanel(gm);
            if (showMission) DrawMissionPanel(gm);
            if (showCodex) DrawCodexPanel(gm);
            if (showAdvisor) DrawAdvisorPanel(gm);
            DrawToast();
            if (!string.IsNullOrEmpty(EndMessage)) DrawEndMessage();
        }

        private void EnsureStyles()
        {
            if (box != null && smallLabel != null && ringHeader != null && geneNode != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.fontSize = 13; box.normal.textColor = Color.white;
            title = new GUIStyle(GUI.skin.label); title.fontSize = 16; title.fontStyle = FontStyle.Bold; title.normal.textColor = new Color(0.86f, 1f, 1f, 1f);
            button = new GUIStyle(GUI.skin.button); button.fontSize = 12; button.wordWrap = true;
            smallLabel = new GUIStyle(GUI.skin.label); smallLabel.fontSize = 11; smallLabel.wordWrap = true; smallLabel.normal.textColor = new Color(0.86f, 0.96f, 1f, 1f);
            ringHeader = new GUIStyle(GUI.skin.box); ringHeader.alignment = TextAnchor.MiddleCenter; ringHeader.fontSize = 11; ringHeader.fontStyle = FontStyle.Bold; ringHeader.normal.textColor = new Color(0.9f, 1f, 1f, 1f);
            geneNode = new GUIStyle(GUI.skin.button); geneNode.fontSize = 10; geneNode.wordWrap = true; geneNode.alignment = TextAnchor.MiddleCenter;
        }

        private void DrawTopHud(V5GameManager gm)
        {
            V5CellEntity m = gm.MotherCell;
            GUI.Box(new Rect(10, 10, 500, 366), "", box);
            GUI.Label(new Rect(20, 16, 350, 24), "PROTOGENESIS PRIMORDIA - V5 BUILD 2.22", title);
            if (m != null)
            {
                GUI.Label(new Rect(20, 44, 460, 20), "Madre: " + m.EvolutionPath + " / " + m.Domain + " / " + m.Metabolism);
                GUI.Label(new Rect(20, 64, 470, 20), string.Format("HP {0:0}/{1:0} | Entidades {2}/{3} | Carga {4:0.0}/{5:0}", m.Stats.currentHp, m.Stats.maxHp, gm.PlayerCellCount(), V5Balance.HardControllableEntityCap, gm.PlayerPopulationLoad(), V5Balance.HardPopulationLoad));
                GUI.Label(new Rect(20, 84, 460, 20), PrimaryResourceText(m));
                GUI.Label(new Rect(20, 104, 460, 20), "Fase: " + gm.Phase + " | Objetivo: coloniza 40% o elimina amenazas");
                if (gm.Identity != null) GUI.Label(new Rect(20, 124, 460, 20), "Identidad: " + gm.Identity.DisplayName + " | " + (gm.Adaptations != null ? gm.Adaptations.Summary() : ""));
                else if (gm.RouteLifecycle != null) GUI.Label(new Rect(20, 124, 460, 20), "Afinidad: " + gm.RouteLifecycle.Summary);
                GUI.Label(new Rect(20, 144, 460, 20), MvpRouteHudText(gm));
                if (gm.NichePressure != null) GUI.Label(new Rect(20, 164, 460, 20), gm.NichePressure.Summary);
                if (gm.WorldEvents != null) GUI.Label(new Rect(20, 184, 460, 20), "Evento: " + gm.WorldEvents.CurrentEvent);
                if (gm.WorldEvents != null) GUI.Label(new Rect(20, 204, 460, 20), gm.WorldEvents.RouteOpportunitySummary);
                if (gm.RouteClimax != null) GUI.Label(new Rect(20, 224, 460, 20), gm.RouteClimax.Summary);
                if (gm.Mission != null) GUI.Label(new Rect(20, 244, 460, 20), "Misión: " + gm.Mission.CurrentObjective);
                if (gm.Director != null) GUI.Label(new Rect(20, 264, 460, 20), "Amenaza: " + (gm.Director.ThreatLevel * 100f).ToString("0") + "% | " + gm.Director.LastDirectorAction + BattlefieldHudSuffix(gm));
                if (gm.Continuity != null) GUI.Label(new Rect(20, 284, 460, 20), "Continuidad: " + gm.Continuity.StatusText);
                if (gm.Body != null) GUI.Label(new Rect(20, 304, 460, 20), "Cuerpo: " + gm.Body.Summary + " | B panel");
                if (gm.PlayableLoop != null) GUI.Label(new Rect(20, 324, 460, 20), gm.PlayableLoop.LoopSummary + " | " + gm.PlayableLoop.NextAction);
                GUI.Label(new Rect(20, 344, 460, 20), "Castas: " + V5CasteLibrary.CompositionSummary(gm.PlayerCells));
            }
            else GUI.Label(new Rect(20, 44, 340, 20), "Sin célula madre.");
        }

        private string PrimaryResourceText(V5CellEntity cell)
        {
            return string.Format("ATP {0:0}  Biomasa {1:0}  Stress {2:0}", cell.Resources.atp, cell.Resources.biomass, cell.Stats.stress);
        }

        private string BattlefieldHudSuffix(V5GameManager gm)
        {
            if (gm == null || gm.Battlefield == null) return "";
            return " | Campo: " + gm.Battlefield.Summary;
        }

        private string MvpRouteHudText(V5GameManager gm)
        {
            V5MvpRoute route = ActiveMvpRoute(gm);
            if (route == V5MvpRoute.None) return "MVP: elige una ruta biologica en Genoma";

            V5AdaptationId next = gm != null && gm.MvpIntent != null
                ? gm.MvpIntent.SuggestedCoreAdaptation(gm)
                : V5MvpCanon.NextMissingCoreAdaptation(route, gm != null ? gm.Adaptations : null);
            V5AdaptationDefinition def = V5AdaptationLibrary.Get(next);
            string nextText = def != null ? def.shortName : "ruta completa";
            string prefix = gm != null && gm.MvpIntent != null && gm.MvpIntent.HasIntent ? "MVP objetivo: " : "MVP: ";
            string text = prefix + V5MvpCanon.ProgressText(route, gm != null ? gm.Adaptations : null) + " | prox " + nextText;
            if (gm != null && gm.MvpIntent != null)
                text += " | meta " + (gm.MvpIntent.RouteGoalProgress01(gm) * 100f).ToString("0") + "%";
            if (gm != null && gm.MvpIntent != null && gm.MvpIntent.HasIntent)
                text += " | micro " + (gm.MvpIntent.RouteMicroObjectiveProgress01(gm) * 100f).ToString("0") + "%";
            if (gm != null && gm.MvpIntent != null && gm.MvpIntent.WorldCueCount > 0)
                text += " | mundo x" + gm.MvpIntent.WorldCueCount;
            if (gm != null && gm.MvpIntent != null && gm.MvpIntent.PrepareCount > 0)
                text += " | test x" + gm.MvpIntent.PrepareCount;
            if (gm != null && gm.RouteBuilds != null)
                text += " | build " + gm.RouteBuilds.ActiveStage + "/" + gm.RouteBuilds.ActiveTargetCount;
            if (gm != null && gm.RouteMastery != null)
                text += " | mastery " + (gm.RouteMastery.Mastery01(route) * 100f).ToString("0") + "%";
            return text;
        }

        private V5MvpRoute ActiveMvpRoute(V5GameManager gm)
        {
            if (gm != null && gm.MvpIntent != null) return gm.MvpIntent.EffectiveRoute(gm);
            return V5MvpCanon.CurrentRoute(gm);
        }

        private void DrawDirectiveBar(V5GameManager gm)
        {
            float barHeight = 126f;
            float width = Mathf.Min(Screen.width - 20f, 1120f);
            Rect r = new Rect(10, Screen.height - barHeight - 10f, width, barHeight);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 10, r.y + 8, r.width - 20, 20), "Controles: Click/drag selecciona | Click der. mueve | D divide | E interior | G genoma/adaptaciones | 8 germinal | B cuerpo | L/V linaje | A/X cuerpo");
            V5EnvironmentOverlay overlay = FindFirstObjectByType<V5EnvironmentOverlay>();
            GUI.Label(new Rect(r.x + 10, r.y + 32, 240, 20), "Overlay: " + (overlay != null ? overlay.Mode.ToString() : "N/A"));
            if (gm.Selection != null) GUI.Label(new Rect(r.x + 250, r.y + 32, 180, 20), "Seleccionadas: " + gm.Selection.Selected.Count);
            if (gm.Scenario != null) GUI.Label(new Rect(r.x + 430, r.y + 32, r.width - 440, 20), gm.Scenario.ObjectiveText);

            V5CellEntity selected = PrimarySelected(gm);
            if (selected != null)
            {
                GUI.Label(new Rect(r.x + 10, r.y + 56, r.width - 20, 20), UnitReadout(selected));
                GUI.Label(new Rect(r.x + 10, r.y + 78, r.width - 20, 20), CombatReadout(selected) + " | " + ModeReadout(selected));
                GUI.Label(new Rect(r.x + 10, r.y + 100, r.width - 20, 20), InheritanceReadout(selected, gm) + " | " + InstallReadout(selected));
            }
            else if (gm.Squads != null)
            {
                GUI.Label(new Rect(r.x + 10, r.y + 56, r.width - 20, 20), gm.Squads.Summary + " | Shift+S reagrupar");
                GUI.Label(new Rect(r.x + 10, r.y + 78, r.width - 20, 20), "Selecciona una celula para ver dano, modo celular y herencia de division.");
            }
        }

        private V5CellEntity PrimarySelected(V5GameManager gm)
        {
            if (gm == null || gm.Selection == null) return gm != null ? gm.MotherCell : null;
            for (int i = 0; i < gm.Selection.Selected.Count; i++)
            {
                V5CellEntity c = gm.Selection.Selected[i];
                if (c != null) return c;
            }
            return gm.MotherCell;
        }

        private string UnitReadout(V5CellEntity cell)
        {
            string unit = cell.Role == V5CellRole.Mother ? "MADRE" : cell.Role + " G" + cell.Generation;
            string body = cell.IsAttachedToBody ? " | ADHERIDA slot " + cell.BodySlotIndex : "";
            string caste = cell.Role == V5CellRole.Mother ? "Madre" : cell.FunctionalCasteLabel;
            return unit + " | Casta " + caste + " | " + cell.PhenotypeLabel + " [" + cell.PhenotypeRecipeCode + "] | Ruta " + cell.EvolutionPath + " | Escala " + V5RosterBalance.EntityScaleLabel(cell.EvolutionPath) + " | Modo " + cell.CellModeLabel + " | Orden " + cell.Directive + body + " | HP " + cell.Stats.currentHp.ToString("0") + "/" + cell.Stats.maxHp.ToString("0") + " | Stress " + cell.Stats.stress.ToString("0");
        }

        private string CombatReadout(V5CellEntity cell)
        {
            float dps = cell.Stats.physicalDamagePerSecond + cell.Stats.chemicalDamagePerSecond;
            return "Combate: DPS base " + dps.ToString("0.0") + " (fis " + cell.Stats.physicalDamagePerSecond.ToString("0.0") + " / tox " + cell.Stats.chemicalDamagePerSecond.ToString("0.0") + ") rango " + cell.Stats.attackRange.ToString("0.0") + " sensor " + cell.Stats.sensorRange.ToString("0.0");
        }

        private string ModeReadout(V5CellEntity cell)
        {
            V5CellModeDefinition mode = V5CellModeLibrary.Get(cell.CellMode);
            V5FunctionalCasteDefinition caste = V5CasteLibrary.Get(cell.FunctionalCaste);
            return "Modo: " + mode.displayName + " (" + mode.effectSummary + ") | Casta: " + caste.effectSummary + " | Receta: " + cell.PhenotypeRecipeSummary + " | Conducta K: pulso " + PulseMode(cell);
        }

        private string PulseMode(V5CellEntity cell)
        {
            if (cell.HasPiercingStylet || cell.EvolutionPath == V5EvolutionPath.Nematode) return "puncion";
            if (cell.HasPhagocytosis || cell.EvolutionPath == V5EvolutionPath.Amoeba || cell.EvolutionPath == V5EvolutionPath.Rotifer || HasAdaptation(cell, V5AdaptationId.Lysosome) || HasAdaptation(cell, V5AdaptationId.Pseudopods)) return "digestion";
            if (cell.HasStructure(V5StructureId.Cilia) || cell.HasStructure(V5StructureId.CoronaCilia) || cell.EvolutionPath == V5EvolutionPath.Ciliate || HasAdaptation(cell, V5AdaptationId.Cilia)) return "corriente ciliada";
            if (cell.Metabolism == V5MetabolismType.Photosynthesis || cell.HasPhotosynthesis || cell.EvolutionPath == V5EvolutionPath.Cyanobacteria || HasAdaptation(cell, V5AdaptationId.ProkaryoticThylakoid) || HasAdaptation(cell, V5AdaptationId.Chloroplast)) return "bloom O2";
            if (cell.Metabolism == V5MetabolismType.Chemolithotrophy || cell.EvolutionPath == V5EvolutionPath.Archaea || HasAdaptation(cell, V5AdaptationId.ProtonPump) || HasAdaptation(cell, V5AdaptationId.ExtremophileMembrane)) return "bolsa extrema";
            if (cell.HasMucilage || cell.EvolutionPath == V5EvolutionPath.Bacteria || cell.EvolutionPath == V5EvolutionPath.Fungus || cell.EvolutionPath == V5EvolutionPath.SlimeMold || cell.HasStructure(V5StructureId.AzurophilicGranule) || HasAdaptation(cell, V5AdaptationId.FungalHypha) || HasAdaptation(cell, V5AdaptationId.SlimePlasmodium)) return "toxina/red";
            return "homeostasis";
        }

        private bool HasAdaptation(V5CellEntity cell, V5AdaptationId id)
        {
            if (cell == null || !cell.IsPlayerOwned) return false;
            V5GameManager gm = V5GameManager.Instance;
            return gm != null && gm.Adaptations != null && gm.Adaptations.Has(id);
        }

        private string InheritanceReadout(V5CellEntity cell, V5GameManager gm)
        {
            if (cell.Role == V5CellRole.Apex) return "Division: apex no se divide";
            if (cell.Role != V5CellRole.Mother && cell.Generation >= 2) return "Division: limite G2";
            float chance = cell.HasStructure(V5StructureId.StemPlasticity) ? V5Balance.StrongInheritanceChance : V5Balance.DaughterInheritanceChance;
            if (gm != null && gm.Genes != null) chance = gm.Genes.InheritanceChance(cell, chance);
            if (gm != null && gm.Adaptations != null) chance = gm.Adaptations.InheritanceChance(cell, chance);
            string traitLabel = gm != null && gm.Adaptations != null ? "rasgos corporales" : "estructuras";
            return "Division: hija hereda ruta/metabolismo y " + (chance * 100f).ToString("0") + "% de " + traitLabel;
        }

        private string InstallReadout(V5CellEntity cell)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Adaptations != null)
            {
                if (cell.Role == V5CellRole.Mother) return "El Genoma (G) define adaptaciones y futuras hijas";
                return "Fenotipo heredado; usa Genoma (G) para cambiar el linaje";
            }
            if (cell.Role == V5CellRole.Mother) return "Instalar aqui modifica el genoma/banco de futuras hijas";
            return "Instalar aqui especializa esta unidad; si faltan recursos paga la madre";
        }

        private void DrawInteriorPanel(V5GameManager gm)
        {
            V5CellEntity cell = null;
            if (gm.Selection != null && gm.Selection.Selected.Count > 0) cell = gm.Selection.Selected[0];
            if (cell == null) cell = gm.MotherCell;
            if (cell == null) return;
            bool adaptationMode = gm.Adaptations != null;
            Rect r = new Rect(Screen.width - 430, 10, 420, Screen.height - 156);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "PANEL INTERIOR - FENOTIPO " + cell.EvolutionPath, title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 20), "Dominio: " + cell.Domain + " | Metabolismo: " + cell.Metabolism);
            GUI.Label(new Rect(r.x + 12, r.y + 58, r.width - 24, 20), "Carga biomasa: " + (V5Balance.BiomassLoadRatio(cell) * 100f).ToString("0") + "%");
            GUI.Label(new Rect(r.x + 12, r.y + 78, r.width - 24, 20), PrimaryResourceText(cell));
            GUI.Label(new Rect(r.x + 12, r.y + 98, r.width - 24, 20), InstallReadout(cell));

            float y = r.y + 124;
            if (adaptationMode)
            {
                GUI.Label(new Rect(r.x + 12, y, r.width - 24, 20), "Identidad: " + (gm.Identity != null ? gm.Identity.DisplayName : cell.EvolutionPath.ToString()), smallLabel);
                y += 22;
                V5MvpRoute mvpRoute = ActiveMvpRoute(gm);
                GUI.Label(new Rect(r.x + 12, y, r.width - 24, 38), "Ruta MVP: " + V5MvpCanon.ProgressText(mvpRoute, gm.Adaptations) + " | " + V5MvpCanon.Fantasy(mvpRoute), smallLabel);
                y += 42;
                GUI.Label(new Rect(r.x + 12, y, r.width - 24, 20), gm.Adaptations.Summary(), smallLabel);
                y += 22;
                GUI.Label(new Rect(r.x + 12, y, r.width - 24, 38), "Canon actual: " + CurrentCanonText(gm, cell), smallLabel);
                y += 42;

                if (GUI.Button(new Rect(r.x + 12, y, 132, 34), "Abrir Genoma", button))
                {
                    showInterior = false;
                    V5GenomePanelIMGUI genome = GenomePanel();
                    if (genome != null) genome.TogglePanel();
                    else
                    {
                        V5PanelRouter.CloseOthers("Genes");
                        showGenes = true;
                    }
                }
                if (GUI.Button(new Rect(r.x + 152, y, 132, 34), showLegacyStructureCatalog ? "Ocultar legacy" : "Modo avanzado", button))
                {
                    showLegacyStructureCatalog = !showLegacyStructureCatalog;
                }
                if (gm.Identity != null)
                {
                    V5AdaptationDefinition suggested = V5AdaptationLibrary.Get(gm.Identity.SuggestedNext);
                    GUI.enabled = suggested != null && !gm.Adaptations.Has(suggested.id);
                    if (GUI.Button(new Rect(r.x + 292, y, 116, 34), "Sugerida", button) && suggested != null)
                    {
                        gm.Adaptations.Install(suggested.id, gm.MotherCell);
                    }
                    GUI.enabled = true;
                }
                y += 46;

                GUI.Label(new Rect(r.x + 12, y, r.width - 24, 20), "Rasgos corporales generados: " + cell.Structures.Count, smallLabel);
                y += 22;
                GUI.Label(new Rect(r.x + 12, y, r.width - 24, 62), PhenotypeStructureText(cell), smallLabel);
                y += 70;

                GUI.Label(new Rect(r.x + 12, y, r.width - 24, 22), "Lectura: " + PulseMode(cell) + " | " + cell.FunctionalCasteLabel + " | " + cell.PhenotypeLabel + " [" + cell.PhenotypeRecipeCode + "]", smallLabel);
                y += 26;

                if (!showLegacyStructureCatalog)
                {
                    GUI.Label(new Rect(r.x + 12, y, r.width - 24, 86), "El catalogo manual de estructuras queda oculto. El flujo principal es Genoma -> Adaptaciones -> Identidad -> Produccion. Usa Modo avanzado solo para debug, saves antiguos o pruebas de balance.", smallLabel);
                    return;
                }

                GUI.Label(new Rect(r.x + 12, y, r.width - 24, 20), "Catalogo avanzado legacy: " + V5BiologyCanon.RouteDesignNote(cell.EvolutionPath), smallLabel);
                y += 24;
                DrawLegacyStructureCatalog(gm, cell, r, y);
                return;
            }

            GUI.Label(new Rect(r.x + 12, y, 180, 20), "Genoma legacy:");
            y += 22;
            GUI.Label(new Rect(r.x + 12, y, 252, 34), "Modo antiguo: estructuras y genes siguen disponibles como fallback.", smallLabel);
            if (GUI.Button(new Rect(r.x + 272, y, 120, 34), "Abrir Genes", button))
            {
                showInterior = false;
                V5PanelRouter.CloseOthers("Genes");
                showGenes = true;
            }
            y += 46;

            GUI.Label(new Rect(r.x + 12, y, 340, 20), "Estructuras instaladas: " + cell.Structures.Count);
            y += 22;
            GUI.Label(new Rect(r.x + 12, y, r.width - 24, 42), PhenotypeStructureText(cell), smallLabel);
            y += 50;

            GUI.Label(new Rect(r.x + 12, y, 340, 20), "Catalogo biologico: " + V5BiologyCanon.RouteDesignNote(cell.EvolutionPath));
            y += 24;
            DrawLegacyStructureCatalog(gm, cell, r, y);
        }

        private void DrawLegacyStructureCatalog(V5GameManager gm, V5CellEntity cell, Rect panel, float y)
        {
            Rect view = new Rect(panel.x + 10, y, panel.width - 20, Mathf.Max(90f, panel.yMax - y - 12f));
            int structureCount = 0;
            foreach (V5StructureDefinition ignored in V5EvolutionLibrary.AllStructures()) structureCount++;
            Rect content = new Rect(0, 0, view.width - 22, Mathf.Max(view.height, structureCount * 58f + 8f));
            scroll = GUI.BeginScrollView(view, scroll, content);
            float cy = 0f;
            foreach (V5StructureDefinition def in V5EvolutionLibrary.AllStructures())
            {
                bool can = cell.CanInstall(def.id);
                GUI.enabled = can;
                V5ResourceWallet effectiveCost = cell.EffectiveInstallCost(def.id);
                string txt = def.displayName + " [" + StructureCanonLabel(def.id, cell, gm) + "]\n" + CostText(effectiveCost) + " | " + V5BiologyCanon.StructureInstallCostNote(def.id, cell, gm != null ? gm.Genes : null) + " | " + def.description;
                if (GUI.Button(new Rect(0, cy, content.width, 54), txt, button)) cell.InstallStructure(def.id);
                GUI.enabled = true;
                cy += 58;
            }
            GUI.EndScrollView();
        }

        private string CurrentCanonText(V5GameManager gm, V5CellEntity cell)
        {
            if (gm != null && gm.Adaptations != null)
            {
                V5MvpRoute route = ActiveMvpRoute(gm);
                if (route != V5MvpRoute.None) return V5MvpCanon.CoreAdaptationText(route);

                V5AdaptationId[] ids = gm.Identity != null && gm.Identity.Identity != V5IdentityId.LUCA
                    ? V5BiologyCanon.AdaptationsForIdentity(gm.Identity.Identity)
                    : V5BiologyCanon.AdaptationsForRoute(cell.EvolutionPath);
                return V5BiologyCanon.AdaptationListText(ids);
            }
            return GeneListText(V5BiologyCanon.GenesForRoute(cell.EvolutionPath));
        }

        private string PhenotypeStructureText(V5CellEntity cell)
        {
            if (cell == null || cell.Structures.Count == 0) return "ninguno";
            string installed = "";
            for (int i = 0; i < cell.Structures.Count; i++)
            {
                V5StructureDefinition def = V5EvolutionLibrary.GetStructure(cell.Structures[i]);
                installed += !string.IsNullOrEmpty(def.displayName) ? def.displayName : cell.Structures[i].ToString();
                if (i < cell.Structures.Count - 1) installed += ", ";
                if (installed.Length > 260)
                {
                    installed += "...";
                    break;
                }
            }
            return installed;
        }


        private void DrawGenePanel(V5GameManager gm)
        {
            if (gm == null || gm.MotherCell == null || gm.Genes == null) return;
            V5CellEntity cell = gm.MotherCell;
            float panelY = 252f;
            float panelW = Mathf.Min(560f, Screen.width - 20f);
            float availableH = Screen.height - panelY - 148f;
            float panelH = Mathf.Max(220f, Mathf.Min(540f, availableH));
            Rect r = new Rect(10, panelY, panelW, panelH);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "ÁRBOL GÉNICO — 4 ANILLOS", title);
            GUI.Label(new Rect(r.x + 12, r.y + 36, r.width - 24, 20), gm.Genes.Summary());
            GUI.Label(new Rect(r.x + 12, r.y + 58, r.width - 24, 20), gm.Genes.LastMessage);
            if (gm.RouteLifecycle != null) GUI.Label(new Rect(r.x + 12, r.y + 80, r.width - 24, 20), "Afinidad: " + gm.RouteLifecycle.Summary + " | " + gm.RouteLifecycle.ApexSummary);
            string canon = gm.Adaptations != null
                ? V5BiologyCanon.AdaptationListText(gm.Identity != null ? V5BiologyCanon.AdaptationsForIdentity(gm.Identity.Identity) : V5BiologyCanon.AdaptationsForRoute(cell.EvolutionPath))
                : GeneListText(V5BiologyCanon.GenesForRoute(cell.EvolutionPath));
            GUI.Label(new Rect(r.x + 12, r.y + 102, r.width - 24, 20), "Canon " + cell.EvolutionPath + ": " + canon);

            Rect view = new Rect(r.x + 10, r.y + 128, r.width - 20, r.height - 140);
            Rect content = new Rect(0, 0, Mathf.Max(view.width - 22, 540f), 560f);
            geneScroll = GUI.BeginScrollView(view, geneScroll, content);
            DrawGeneTree(gm, cell, content.width);
            GUI.EndScrollView();
        }

        private void DrawGeneTree(V5GameManager gm, V5CellEntity cell, float contentWidth)
        {
            V5GeneSystem genes = gm.Genes;
            V5GeneId[][] rings = new V5GeneId[][]
            {
                new V5GeneId[] { V5GeneId.Respiration, V5GeneId.Photosynthesis, V5GeneId.Fermentation, V5GeneId.Chemolithotrophy },
                new V5GeneId[] { V5GeneId.Motility, V5GeneId.Secretion, V5GeneId.Recognition, V5GeneId.Adhesion },
                new V5GeneId[] { V5GeneId.RapidDivision, V5GeneId.StrongInheritance, V5GeneId.Autonomy, V5GeneId.TotalReabsorption },
                new V5GeneId[] { V5GeneId.Symbiosis, V5GeneId.ApexMaturation }
            };

            float margin = 10f;
            float colGap = 12f;
            float colW = (contentWidth - margin * 2f - colGap * 3f) / 4f;
            float nodeH = 72f;
            float rowGap = 10f;
            float nodeY = 38f;
            Dictionary<V5GeneId, Rect> nodes = new Dictionary<V5GeneId, Rect>();

            for (int col = 0; col < rings.Length; col++)
            {
                float x = margin + col * (colW + colGap);
                Rect header = new Rect(x, 0, colW, 28);
                GUI.Box(header, GeneRingTitle(col), ringHeader);
                if (col < rings.Length - 1)
                {
                    Rect line = new Rect(header.xMax, header.y + 13f, colGap, 2f);
                    DrawSolidRect(line, new Color(0.45f, 0.75f, 0.82f, 0.45f));
                }

                float offsetRows = Mathf.Max(0f, (4f - rings[col].Length) * 0.5f);
                for (int row = 0; row < rings[col].Length; row++)
                {
                    float y = nodeY + (row + offsetRows) * (nodeH + rowGap);
                    nodes[rings[col][row]] = new Rect(x, y, colW, nodeH);
                }
            }

            DrawGeneOptionConnectors(genes.Ring1, genes.Ring2, rings[1], nodes);
            DrawGeneOptionConnectors(genes.Ring2, genes.Ring3, rings[2], nodes);
            DrawGeneOptionConnectors(genes.Ring3, genes.Ring4, rings[3], nodes);

            V5GeneId hovered = V5GeneId.None;
            for (int col = 0; col < rings.Length; col++)
            {
                for (int row = 0; row < rings[col].Length; row++)
                {
                    V5GeneId id = rings[col][row];
                    V5GeneId h = DrawGeneNode(gm, cell, genes.Get(id), nodes[id]);
                    if (h != V5GeneId.None) hovered = h;
                }
            }

            V5GeneId detail = hovered != V5GeneId.None ? hovered : NextSuggestedGene(gm, cell);
            DrawGeneDetail(gm, cell, detail, new Rect(margin, 386f, contentWidth - margin * 2f, 92f));

            if (gm.Apex != null)
            {
                Rect apexButton = new Rect(margin, 490f, 210f, 38f);
                GUI.enabled = gm.Apex.CanSpawn(gm.Apex.RecommendedForm(cell), cell);
                if (GUI.Button(apexButton, "Invocar Apex: " + gm.Apex.RecommendedForm(cell), button)) gm.Apex.SpawnRecommended();
                GUI.enabled = true;
                GUI.Label(new Rect(apexButton.xMax + 12f, apexButton.y, contentWidth - apexButton.width - margin * 2f - 12f, 40f), gm.Apex.LastMessage, smallLabel);
            }
        }

        private V5GeneId DrawGeneNode(V5GameManager gm, V5CellEntity cell, V5GeneDefinition def, Rect rect)
        {
            bool active = gm.Genes.HasGene(def.id);
            bool can = gm.Genes.CanUnlock(def.id, cell);
            bool adhesionPriority = def.id == V5GeneId.Adhesion && gm.Body != null && !gm.Body.AttachmentUnlocked;
            string state = active ? "ACTIVO" : (can ? (adhesionPriority ? "CUERPO" : "DISPONIBLE") : GeneLockReason(gm, cell, def));
            string footer = active ? GeneEffectShort(def.id) : GeneShortCost(def.cost);
            string label = def.name + "\n" + state + "\n" + footer;

            Color old = GUI.color;
            if (active) GUI.color = new Color(0.55f, 1f, 0.65f, 1f);
            else if (can && adhesionPriority) GUI.color = new Color(1f, 0.88f, 0.35f, 1f);
            else if (can) GUI.color = new Color(0.72f, 0.95f, 1f, 1f);
            else GUI.color = new Color(0.54f, 0.58f, 0.62f, 0.78f);

            GUI.enabled = can && !active;
            if (GUI.Button(rect, label, geneNode)) gm.Genes.Unlock(def.id, cell);
            GUI.enabled = true;
            GUI.color = old;

            if (adhesionPriority && !active)
                DrawSolidRect(new Rect(rect.x, rect.y, rect.width, 3f), new Color(1f, 0.82f, 0.22f, 1f));

            return rect.Contains(Event.current.mousePosition) ? def.id : V5GeneId.None;
        }

        private void DrawGeneDetail(V5GameManager gm, V5CellEntity cell, V5GeneId id, Rect rect)
        {
            GUI.Box(rect, "", box);
            if (id == V5GeneId.None)
            {
                GUI.Label(new Rect(rect.x + 10f, rect.y + 8f, rect.width - 20f, rect.height - 16f), "Ruta estable. El siguiente gen depende de tu plan de combate, economia o apex.", smallLabel);
                return;
            }

            V5GeneDefinition def = gm.Genes.Get(id);
            string status = gm.Genes.HasGene(id) ? "Activo" : (gm.Genes.CanUnlock(id, cell) ? "Disponible" : GeneLockReason(gm, cell, def));
            GUI.Label(new Rect(rect.x + 10f, rect.y + 8f, rect.width - 20f, 18f), def.name + " | " + status + " | " + GeneShortCost(def.cost), smallLabel);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 30f, rect.width - 20f, 52f), def.description + " " + GeneEffectShort(id), smallLabel);
        }

        private void DrawGeneOptionConnectors(V5GeneId from, V5GeneId chosenTo, V5GeneId[] options, Dictionary<V5GeneId, Rect> nodes)
        {
            if (from == V5GeneId.None || !nodes.ContainsKey(from)) return;
            for (int i = 0; i < options.Length; i++)
            {
                V5GeneId to = options[i];
                if (!nodes.ContainsKey(to)) continue;
                bool chosen = chosenTo == to;
                bool pending = chosenTo == V5GeneId.None;
                Color color = chosen ? new Color(0.45f, 1f, 0.6f, 0.85f) :
                    (pending ? new Color(0.38f, 0.75f, 0.85f, 0.55f) : new Color(0.35f, 0.40f, 0.44f, 0.35f));
                DrawGeneConnector(nodes[from], nodes[to], color);
            }
        }

        private void DrawGeneConnector(Rect from, Rect to, Color color)
        {
            float x1 = from.xMax;
            float y1 = from.y + from.height * 0.5f;
            float x2 = to.x;
            float y2 = to.y + to.height * 0.5f;
            float mid = (x1 + x2) * 0.5f;
            DrawSolidRect(new Rect(x1, y1 - 1f, Mathf.Max(0f, mid - x1), 2f), color);
            DrawSolidRect(new Rect(mid - 1f, Mathf.Min(y1, y2), 2f, Mathf.Abs(y2 - y1)), color);
            DrawSolidRect(new Rect(mid, y2 - 1f, Mathf.Max(0f, x2 - mid), 2f), color);
        }

        private void DrawSolidRect(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private string GeneRingTitle(int index)
        {
            if (index == 0) return "1. Metabolismo";
            if (index == 1) return "2. Funcion";
            if (index == 2) return "3. Division";
            return "4. Ecosistema";
        }

        private V5GeneId NextSuggestedGene(V5GameManager gm, V5CellEntity cell)
        {
            if (gm.Genes.Ring1 == V5GeneId.None)
            {
                float light = gm.Environment != null ? gm.Environment.Sample(V5OverlayMode.Light, cell.transform.position) : 0.4f;
                float oxygen = gm.Environment != null ? gm.Environment.Sample(V5OverlayMode.Oxygen, cell.transform.position) : 0.3f;
                if (light > 0.55f) return V5GeneId.Photosynthesis;
                if (oxygen > 0.28f) return V5GeneId.Respiration;
                return V5GeneId.Fermentation;
            }
            if (gm.Genes.Ring2 == V5GeneId.None) return V5GeneId.Adhesion;
            if (gm.Genes.Ring3 == V5GeneId.None) return V5GeneId.RapidDivision;
            if (gm.Genes.Ring4 == V5GeneId.None) return V5GeneId.Symbiosis;
            return V5GeneId.None;
        }

        private string GeneLockReason(V5GameManager gm, V5CellEntity cell, V5GeneDefinition def)
        {
            V5GeneId ringGene = ActiveGeneForRing(gm.Genes, def.ring);
            if (ringGene != V5GeneId.None && ringGene != def.id) return "anillo ocupado";
            if (gm.ElapsedSeconds < def.minTime) return "madura en " + Mathf.CeilToInt(def.minTime - gm.ElapsedSeconds) + "s";
            if (def.ring != V5GeneRing.Ring1Metabolism && gm.Genes.Ring1 == V5GeneId.None) return "requiere metabolismo";
            if (def.ring == V5GeneRing.Ring3Division && gm.Genes.Ring2 == V5GeneId.None) return "requiere funcion";
            if (def.ring == V5GeneRing.Ring4Ecosystem && gm.Genes.Ring3 == V5GeneId.None) return "requiere division";
            bool domainOk = def.requiredDomain == V5CellDomain.LUCA || cell.Domain == def.requiredDomain || (cell.Domain == V5CellDomain.Multicellular && def.requiredDomain == V5CellDomain.Eukaryote);
            if (!domainOk) return "requiere " + def.requiredDomain;
            if (!cell.Resources.CanPay(def.cost)) return "faltan recursos";
            return "bloqueado";
        }

        private V5GeneId ActiveGeneForRing(V5GeneSystem genes, V5GeneRing ring)
        {
            if (ring == V5GeneRing.Ring1Metabolism) return genes.Ring1;
            if (ring == V5GeneRing.Ring2Function) return genes.Ring2;
            if (ring == V5GeneRing.Ring3Division) return genes.Ring3;
            if (ring == V5GeneRing.Ring4Ecosystem) return genes.Ring4;
            return V5GeneId.None;
        }

        private string GeneShortCost(V5ResourceWallet c)
        {
            return "ATP" + c.atp.ToString("0") + " Bio" + c.biomass.ToString("0");
        }

        private string GeneEffectShort(V5GeneId id)
        {
            switch (id)
            {
                case V5GeneId.Respiration: return "Metabolismo: alto ATP con O2.";
                case V5GeneId.Photosynthesis: return "Metabolismo: ATP/O2 con luz.";
                case V5GeneId.Fermentation: return "Metabolismo: ATP rapido, mas stress.";
                case V5GeneId.Chemolithotrophy: return "Metabolismo: ATP extremo y acidez.";
                case V5GeneId.Motility: return "Instala flagelo y sensor.";
                case V5GeneId.Secretion: return "Toxinas y secrecion pasiva.";
                case V5GeneId.Recognition: return "Lectura de firmas biologicas.";
                case V5GeneId.Adhesion: return "Desbloquea cuerpo pegado.";
                case V5GeneId.RapidDivision: return "Division mas barata.";
                case V5GeneId.StrongInheritance: return "Hijas heredan mejor.";
                case V5GeneId.Autonomy: return "Hijas mas autonomas.";
                case V5GeneId.TotalReabsorption: return "Reciclaje al morir.";
                case V5GeneId.Symbiosis: return "Colonizacion y estabilidad.";
                case V5GeneId.ApexMaturation: return "Ruta hacia forma apex.";
                default: return "";
            }
        }

        private string StructureCanonLabel(V5StructureId id, V5CellEntity cell, V5GameManager gm)
        {
            bool recommended = cell != null && V5BiologyCanon.IsStructureRecommendedForCell(id, cell);
            bool usesAdaptations = gm != null && gm.Adaptations != null;
            bool licensed = cell != null && gm != null && (usesAdaptations
                ? V5BiologyCanon.IsStructureLicensedByAdaptations(id, gm.Adaptations)
                : V5BiologyCanon.IsStructureLicensedByGenes(id, gm.Genes, cell));
            string tags = StructureTagText(V5BiologyCanon.TagsForStructure(id));
            string source = usesAdaptations ? "adaptacion" : "gen";
            if (recommended && licensed) return "ruta + " + source + " | " + tags;
            if (recommended) return "ruta | " + tags;
            if (licensed) return source + " | " + tags;
            return "exploratoria | " + tags;
        }

        private string StructureTagText(V5StructureTag[] tags)
        {
            if (tags == null || tags.Length == 0) return "sin tag";
            string s = "";
            for (int i = 0; i < tags.Length; i++)
            {
                s += tags[i].ToString();
                if (i < tags.Length - 1) s += "/";
            }
            return s;
        }

        private string GeneListText(V5GeneId[] genes)
        {
            if (genes == null || genes.Length == 0) return "sin genes canonicos";
            string s = "";
            for (int i = 0; i < genes.Length; i++)
            {
                s += genes[i].ToString();
                if (i < genes.Length - 1) s += ", ";
            }
            return s;
        }

        private void GeneButton(V5GameManager gm, V5CellEntity cell, V5GeneId id, float x, float y, float w)
        {
            if (gm == null || gm.Genes == null) return;
            V5GeneDefinition def = gm.Genes.Get(id);
            bool can = gm.Genes.CanUnlock(id, cell);
            bool active = gm.Genes.HasGene(id);
            GUI.enabled = can && !active;
            string label = active ? ("✓ " + def.name) : def.name + "\n" + CostText(def.cost);
            if (GUI.Button(new Rect(x, y, w, 54), label, button)) gm.Genes.Unlock(id, cell);
            GUI.enabled = true;
        }

        private string CostText(V5ResourceWallet c)
        {
            return string.Format("ATP {0:0} Bio {1:0}", c.atp, c.biomass);
        }

        private void DrawMissionPanel(V5GameManager gm)
        {
            Rect r = new Rect(10, 530, 430, 486);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "MISIÓN / PLAYTEST", title);
            if (gm.Mission != null)
            {
                GUI.Label(new Rect(r.x + 12, r.y + 40, r.width - 24, 34), gm.Mission.CurrentObjective);
                if (gm.MvpIntent != null)
                {
                    GUI.Label(new Rect(r.x + 12, r.y + 76, r.width - 24, 42), "Ruta MVP: " + gm.MvpIntent.RouteObjectiveText(gm), smallLabel);
                    GUI.Label(new Rect(r.x + 12, r.y + 118, r.width - 24, 36), gm.MvpIntent.RouteMicroObjectiveText(gm), smallLabel);
                }
                if (gm.NichePressure != null)
                    GUI.Label(new Rect(r.x + 12, r.y + 146, r.width - 24, 20), gm.NichePressure.LastNicheAdvice, smallLabel);
                if (gm.RouteBuilds != null)
                    GUI.Label(new Rect(r.x + 12, r.y + 166, r.width - 24, 20), gm.RouteBuilds.Summary, smallLabel);
                if (gm.Abilities != null)
                    GUI.Label(new Rect(r.x + 12, r.y + 186, r.width - 24, 20), gm.Abilities.RouteFantasyStatus(gm), smallLabel);
                if (gm.RouteChapters != null)
                    GUI.Label(new Rect(r.x + 12, r.y + 206, r.width - 24, 20), gm.RouteChapters.ChapterStatus(gm), smallLabel);
                if (gm.RouteBranches != null)
                    GUI.Label(new Rect(r.x + 12, r.y + 226, r.width - 24, 20), gm.RouteBranches.BranchStatus(gm), smallLabel);
                if (gm.RouteBranches != null)
                    GUI.Label(new Rect(r.x + 12, r.y + 246, r.width - 24, 20), gm.RouteBranches.BranchObjectiveStatus(gm), smallLabel);
                if (gm.RouteBranches != null)
                    GUI.Label(new Rect(r.x + 12, r.y + 266, r.width - 24, 20), gm.RouteBranches.BranchPassiveStatus(), smallLabel);
                if (gm.RouteBranches != null)
                {
                    GUI.Label(new Rect(r.x + 12, r.y + 286, r.width - 24, 20), gm.RouteBranches.BranchDoctrineStatus(gm), smallLabel);
                    GUI.Label(new Rect(r.x + 12, r.y + 306, r.width - 24, 20), gm.RouteBranches.BranchDoctrineObjectiveStatus(gm), smallLabel);
                    if (gm.RouteBranches.BranchDoctrineAvailable && gm.RouteBranches.ActiveBranchDoctrine == V5BranchDoctrineChoice.None)
                    {
                        GUI.enabled = gm.RouteBranches.CanCommitBranchDoctrine(V5BranchDoctrineChoice.Stabilize, gm);
                        if (GUI.Button(new Rect(r.x + 12, r.y + 328, 126, 22), "Anclar", button))
                            gm.RouteBranches.CommitBranchDoctrine(V5BranchDoctrineChoice.Stabilize, gm);
                        GUI.enabled = gm.RouteBranches.CanCommitBranchDoctrine(V5BranchDoctrineChoice.Radicalize, gm);
                        if (GUI.Button(new Rect(r.x + 146, r.y + 328, 126, 22), "Radicalizar", button))
                            gm.RouteBranches.CommitBranchDoctrine(V5BranchDoctrineChoice.Radicalize, gm);
                        GUI.enabled = true;
                    }
                }
                if (gm.RouteCounters != null)
                    GUI.Label(new Rect(r.x + 12, r.y + 354, r.width - 24, 20), gm.RouteCounters.LastCounterSummary, smallLabel);
                if (gm.WorldEvents != null && gm.WorldEvents.RouteAbilityComboCount > 0)
                    GUI.Label(new Rect(r.x + 12, r.y + 374, r.width - 24, 20), gm.WorldEvents.LastRouteAbilityCombo, smallLabel);
                GUI.Box(new Rect(r.x + 12, r.y + 394, r.width - 24, 18), "");
                float progress = gm.MvpIntent != null && gm.MvpIntent.HasIntent ? gm.MvpIntent.RouteMicroObjectiveProgress01(gm) : gm.Mission.Progress01;
                GUI.Box(new Rect(r.x + 14, r.y + 396, (r.width - 28) * Mathf.Clamp01(progress), 14), "");
                string milestone = "Ultimo hito: " + gm.Mission.LastCompleted;
                if (gm.MvpIntent != null && gm.MvpIntent.CompletedGoalCount > 0) milestone += " | " + gm.MvpIntent.LastGoalMilestone;
                if (gm.MvpIntent != null && gm.MvpIntent.MicroObjectiveCompletedCount > 0) milestone += " | " + gm.MvpIntent.LastMicroMilestone;
                GUI.Label(new Rect(r.x + 12, r.y + 416, r.width - 24, 30), milestone, smallLabel);
                if (gm.MvpIntent != null && gm.MvpIntent.WorldCueCount > 0)
                    GUI.Label(new Rect(r.x + 12, r.y + 446, r.width - 24, 20), "Mundo: " + gm.MvpIntent.LastWorldCue, smallLabel);
            }
            if (gm.Director != null) GUI.Label(new Rect(r.x + 12, r.y + 464, r.width - 24, 20), "Director: " + (gm.Director.ThreatLevel * 100f).ToString("0") + "% / olas " + gm.Director.WavesSpawned);
        }

        private void DrawAdvisorPanel(V5GameManager gm)
        {
            if (gm == null || gm.Advisor == null) return;
            bool diagnostic = gm.Advisor.UsingDiagnosticAdvice && gm.Diagnostics != null;
            bool hasGenomeSuggestion = diagnostic && gm.Diagnostics.CoachAdaptation != V5AdaptationId.None;
            float height = diagnostic ? (hasGenomeSuggestion ? 184f : 132f) : 96f;
            Rect r = new Rect(Screen.width - 430, Screen.height - height - 92f, 420, height);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 8, r.width - 24, 22), "CONSEJO CELULAR", title);
            GUI.Label(new Rect(r.x + 12, r.y + 34, r.width - 24, diagnostic ? 42 : 24), gm.Advisor.CurrentAdvice, smallLabel);
            GUI.Label(new Rect(r.x + 12, r.y + (diagnostic ? 80 : 62), r.width - 24, 24), "Accion: " + gm.Advisor.RecommendedAction, smallLabel);
            if (diagnostic)
            {
                GUI.Label(new Rect(r.x + 12, r.y + 106, r.width - 24, 20), "Diagnostico: " + gm.Diagnostics.Score + "/100 | " + gm.Diagnostics.ShortStatus, smallLabel);
                if (hasGenomeSuggestion)
                {
                    GUI.Label(new Rect(r.x + 12, r.y + 128, r.width - 24, 20), "Genoma: " + gm.Diagnostics.CoachAdaptationLabel + " | " + gm.Diagnostics.CoachAdaptationStatus, smallLabel);
                    V5GenomePanelIMGUI genome = GenomePanel();
                    V5AdaptationId coachId = gm.Diagnostics.CoachAdaptation;
                    if (GUI.Button(new Rect(r.x + 12, r.y + 152, 126, 24), "Abrir Genoma", button))
                    {
                        if (genome != null) genome.OpenFocused(coachId);
                    }

                    string reason;
                    bool installed = gm.Adaptations != null && gm.Adaptations.Has(coachId);
                    bool canInstall = gm.Adaptations != null && gm.Adaptations.CanInstall(coachId, gm.MotherCell, out reason);
                    GUI.enabled = canInstall && !installed;
                    if (GUI.Button(new Rect(r.x + 146, r.y + 152, 104, 24), installed ? "Instalada" : "Instalar", button))
                    {
                        gm.Adaptations.Install(coachId, gm.MotherCell);
                        if (genome != null) genome.FocusAdaptation(coachId);
                    }
                    GUI.enabled = true;
                }
            }
        }

        private void DrawCodexPanel(V5GameManager gm)
        {
            if (gm == null || gm.Codex == null) return;
            Rect r = new Rect(Screen.width - 500, 94, 490, 390);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "CODEX BIOLÓGICO", title);
            GUI.Label(new Rect(r.x + 12, r.y + 36, r.width - 24, 20), "Entradas desbloqueadas: " + gm.Codex.Entries.Count + " | recientes: " + string.Join(", ", gm.Codex.RecentUnlocks.ToArray()));
            Rect view = new Rect(r.x + 10, r.y + 62, r.width - 20, r.height - 74);
            Rect content = new Rect(0, 0, view.width - 22, Mathf.Max(300, gm.Codex.Entries.Count * 56));
            codexScroll = GUI.BeginScrollView(view, codexScroll, content);
            float y = 0f;
            for (int i = 0; i < gm.Codex.Entries.Count; i++)
            {
                GUI.Label(new Rect(0, y, content.width, 52), gm.Codex.Entries[i]);
                y += 56f;
            }
            GUI.EndScrollView();
        }

        private void DrawToast()
        {
            if (string.IsNullOrEmpty(toastMessage) || Time.unscaledTime > toastUntil) return;
            GUI.Box(new Rect(Screen.width / 2 - 220, 24, 440, 44), "", box);
            GUI.Label(new Rect(Screen.width / 2 - 205, 36, 410, 22), toastMessage);
        }

        private void DrawEndMessage()
        {
            GUI.Box(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 60, 440, 120), "", box);
            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 30, 400, 60), EndMessage, title);
        }
    }
}
