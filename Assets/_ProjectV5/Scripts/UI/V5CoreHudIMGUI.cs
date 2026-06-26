using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CoreHudIMGUI : MonoBehaviour
    {
        private static Rect lastHudRect = new Rect(10f, 10f, 456f, 272f);
        private static Rect lastMotherPanelRect = new Rect(10f, 254f, 456f, 572f);
        private static Rect lastUnitPanelRect;
        private static V5CoreHudIMGUI instance;
        private GUIStyle box;
        private GUIStyle label;
        private GUIStyle title;
        private GUIStyle small;
        private GUIStyle readyLabel;
        private GUIStyle blockedLabel;
        private GUIStyle endBanner;
        private GUIStyle button;
        private GUIStyle disabledButton;
        private Vector2 motherUnitsScroll;
        private bool motherPanelVisible;
        private bool unitPanelVisible;
        public bool ShowMotherPanel;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            V5PanelRouter.Register("Madre", () => ShowMotherPanel, v => ShowMotherPanel = v);
        }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || !gm.CoreMode || gm.MotherCell == null) return;
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape)) OpenMotherPanel(true);
        }

        public static void SelectMother(bool focusCamera)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            if (gm.Selection != null)
            {
                gm.Selection.ClearSelection();
                gm.Selection.AddSelection(gm.MotherCell);
            }
            if (focusCamera && gm.CameraController != null) gm.CameraController.SnapTo(gm.MotherCell.transform);
        }

        public static void OpenMotherPanel(bool focusCamera)
        {
            if (instance == null) instance = FindFirstObjectByType<V5CoreHudIMGUI>();
            if (instance == null) return;
            instance.ShowMotherPanel = true;
            SelectMother(focusCamera);
        }

        private void OnGUI()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            EnsureStyles();
            DrawEndStateBanner(gm);
            if (gm.MotherCell == null) return;

            V5CellEntity mother = gm.MotherCell;
            float divideCost = V5Balance.DivisionCostBiomass(mother);
            V5CoreMotherProductionSystem production = gm.CoreMotherProduction != null ? gm.CoreMotherProduction : FindFirstObjectByType<V5CoreMotherProductionSystem>();
            string productionStatus = ProductionStatus(gm, mother, production, divideCost);

            V5OrganismMorph morph = gm.OrganismMorph;
            Rect r = new Rect(10f, 10f, 456f, morph != null ? 272f : 68f);
            lastHudRect = r;
            Color previousColor = GUI.color;
            GUI.color = new Color(0.03f, 0.05f, 0.05f, 0.78f);
            GUI.Box(r, GUIContent.none, box);
            GUI.color = previousColor;
            GUI.Label(new Rect(r.x + 10f, r.y + 7f, r.width - 20f, 18f), "Libres " + gm.PlayerFreeCellCount() + "/" + gm.PlayerCellCap() + " | Total " + gm.PlayerTotalCellCount() + "/" + V5Balance.CoreTotalPlayerCellHardCap, label);
            GUI.Label(new Rect(r.x + 10f, r.y + 25f, r.width - 20f, 18f), "Biomasa " + mother.Resources.biomass.ToString("0") + " | ADN " + (production != null ? production.DnaPoints.ToString() : "0"), label);
            GUI.Label(new Rect(r.x + 10f, r.y + 43f, r.width - 20f, 18f), productionStatus, ProductionReady(gm, mother, divideCost) ? readyLabel : blockedLabel);
            if (morph != null)
            {
                GUI.Label(new Rect(r.x + 10f, r.y + 61f, r.width - 20f, 18f), "Organismos: " + morph.UnlockedBlueprintsLabel(), label);
                GUI.Label(new Rect(r.x + 10f, r.y + 79f, r.width - 20f, 18f), "Produccion: click madre o Escape abre panel | Espacio vuelve a casa", label);
                GUI.Label(new Rect(r.x + 10f, r.y + 97f, r.width - 20f, 18f), "Evolucion: " + morph.PendingEvolutionLabel(), morph.IsTardigradeUnlocked ? readyLabel : blockedLabel);
                GUI.Label(new Rect(r.x + 10f, r.y + 115f, r.width - 20f, 18f), "Slots de tropas: " + morph.ActiveOrganismCount + "/" + Mathf.Max(1, morph.MaxActiveOrganisms) + " | " + morph.LastMessage, label);
                DrawTroopPanel(morph, r.x + 10f, r.y + 137f, r.width - 20f);
                DrawShortcutLegend(gm, r.x + 10f, r.y + 198f, r.width - 20f);
            }

            V5CellEntity focused = FocusedSelection(gm);
            motherPanelVisible = focused == mother || ShowMotherPanel;
            unitPanelVisible = focused != null && focused != mother;
            if (focused == mother)
            {
                DrawMotherProductionPanel(gm, mother, production);
            }
            else if (focused != null)
            {
                ShowMotherPanel = false;
                motherPanelVisible = false;
                DrawSelectedUnitPanel(gm, focused);
            }
            else if (ShowMotherPanel)
            {
                DrawMotherProductionPanel(gm, mother, production);
            }
        }

        private void DrawEndStateBanner(V5GameManager gm)
        {
            if (gm == null || (gm.Phase != V5GamePhase.Victory && gm.Phase != V5GamePhase.Defeat)) return;
            string text = gm.Phase == V5GamePhase.Victory ? "VICTORIA\nNucleo rival destruido" : "DERROTA\nLa madre fue destruida";
            Color old = GUI.color;
            Rect r = new Rect(Screen.width * 0.5f - 190f, 56f, 380f, 72f);
            GUI.color = gm.Phase == V5GamePhase.Victory ? new Color(0.12f, 0.28f, 0.08f, 0.92f) : new Color(0.32f, 0.04f, 0.03f, 0.92f);
            GUI.Box(r, GUIContent.none, box);
            GUI.color = old;
            GUI.Label(r, text, endBanner);
        }

        public static bool PointerBlocksWorldInput(Vector2 screenMousePosition)
        {
            Vector2 guiPosition = new Vector2(screenMousePosition.x, Screen.height - screenMousePosition.y);
            return lastHudRect.Contains(guiPosition) ||
                   (instance != null && instance.motherPanelVisible && lastMotherPanelRect.Contains(guiPosition)) ||
                   (instance != null && instance.unitPanelVisible && lastUnitPanelRect.Contains(guiPosition));
        }

        private V5CellEntity FocusedSelection(V5GameManager gm)
        {
            if (gm == null || gm.Selection == null || gm.Selection.Selected.Count == 0) return null;
            for (int i = 0; i < gm.Selection.Selected.Count; i++)
            {
                V5CellEntity cell = gm.Selection.Selected[i];
                if (cell != null && cell.Stats.currentHp > 0f) return cell;
            }
            return null;
        }

        private void DrawSelectedUnitPanel(V5GameManager gm, V5CellEntity focused)
        {
            if (gm == null || focused == null) return;
            V5OrganismMorph organism = FocusedOrganism(gm.OrganismMorph, focused);
            float panelWidth = Mathf.Min(448f, Screen.width - 20f);
            float panelHeight = 230f;
            Rect r = new Rect(10f, Mathf.Max(lastHudRect.yMax + 8f, Screen.height - panelHeight - 10f), panelWidth, panelHeight);
            lastUnitPanelRect = r;

            Color previousColor = GUI.color;
            GUI.color = new Color(0.025f, 0.045f, 0.045f, 0.92f);
            GUI.Box(r, GUIContent.none, box);
            GUI.color = previousColor;

            string unitName = organism != null ? organism.ActiveBlueprintName : CellDisplayName(focused);
            string role = organism != null ? OrganismRoleLabel(organism.ActiveBlueprintKind) : focused.Role + " | " + focused.FunctionalCasteLabel;
            GUI.Label(new Rect(r.x + 12f, r.y + 8f, r.width - 24f, 22f), unitName, title);
            GUI.Label(new Rect(r.x + 12f, r.y + 29f, r.width - 24f, 18f), role, small);

            Rect portraitRect = new Rect(r.x + 12f, r.y + 52f, 104f, 112f);
            GUI.color = new Color(0.08f, 0.12f, 0.12f, 0.78f);
            GUI.Box(portraitRect, GUIContent.none, box);
            GUI.color = previousColor;
            DrawUnitPortrait(focused, organism, new Rect(portraitRect.x + 7f, portraitRect.y + 7f, portraitRect.width - 14f, portraitRect.height - 14f));

            float statsX = portraitRect.xMax + 12f;
            float statsWidth = r.xMax - statsX - 12f;
            float health01;
            string healthText;
            float damage;
            float range;
            float speed;
            string cost;
            V5DamageKind damageKind;
            float armor;
            if (organism != null)
            {
                health01 = organism.Health01;
                healthText = "Vida " + Mathf.RoundToInt(health01 * 100f) + "% | Celulas " + organism.Members.Count + "/" + organism.ActiveRequiredFreeCells;
                damage = organism.EffectiveCombatDamagePerSecond;
                range = organism.NucleusCell != null ? organism.NucleusCell.Stats.attackRange : 0f;
                speed = organism.CurrentOrganismMoveSpeed;
                cost = organism.ActiveRequiredFreeCells + " celulas";
                damageKind = organism.ActiveDamageKind;
                armor = organism.ActiveArmor;
            }
            else
            {
                health01 = focused.Stats.maxHp > 0f ? Mathf.Clamp01(focused.Stats.currentHp / focused.Stats.maxHp) : 0f;
                healthText = "Vida " + focused.Stats.currentHp.ToString("0") + "/" + focused.Stats.maxHp.ToString("0") + " HP";
                damage = (focused.Stats.physicalDamagePerSecond + focused.Stats.chemicalDamagePerSecond) * focused.ModeDamageMultiplier();
                if (gm.LineageUpgrades != null && focused.IsPlayerOwned) damage *= gm.LineageUpgrades.DamageMultiplier(focused);
                range = focused.Stats.attackRange;
                speed = focused.Stats.speed;
                cost = "1 celula";
                damageKind = V5DamageKind.Physical;
                armor = Mathf.Clamp01(focused.Stats.physicalArmor);
            }

            GUI.Label(new Rect(statsX, r.y + 52f, statsWidth, 18f), healthText, label);
            DrawUnitHealthBar(new Rect(statsX, r.y + 72f, statsWidth, 16f), health01);
            GUI.Label(new Rect(statsX, r.y + 96f, statsWidth, 18f), "Dano: " + damage.ToString("0.0") + "/s", label);
            GUI.Label(new Rect(statsX, r.y + 116f, statsWidth, 18f), "Rango: " + RangeLabel(range), label);
            GUI.Label(new Rect(statsX, r.y + 136f, statsWidth, 18f), "Velocidad: " + SpeedLabel(speed), label);
            GUI.Label(new Rect(statsX, r.y + 156f, statsWidth, 18f), "Costo: " + cost, label);
            string damageKindText = organism != null && organism.ActiveBlueprintKind == V5OrganismBlueprintKind.Interdictor
                ? "T\u00f3xico (\u00e1rea)"
                : DamageKindLabel(damageKind);
            GUI.Label(new Rect(statsX, r.y + 180f, statsWidth, 18f), "Tipo de dano: " + damageKindText, small);
            GUI.Label(new Rect(statsX, r.y + 200f, statsWidth, 18f), "Armadura: " + ArmorLabel(armor), small);
        }

        private V5OrganismMorph FocusedOrganism(V5OrganismMorph manager, V5CellEntity focused)
        {
            if (manager == null || focused == null || !focused.IsMorphedOrganism) return null;
            System.Collections.Generic.IReadOnlyList<V5OrganismMorph> organisms = manager.ActiveOrganisms;
            for (int i = 0; i < organisms.Count; i++)
            {
                V5OrganismMorph organism = organisms[i];
                if (organism != null && organism.IsMember(focused)) return organism;
            }
            return null;
        }

        private void DrawUnitPortrait(V5CellEntity focused, V5OrganismMorph organism, Rect rect)
        {
            Color previous = GUI.color;
            if (organism != null)
            {
                Texture2D texture = organism.ActiveSilhouetteTexture;
                if (texture != null) GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
            }
            else
            {
                SpriteRenderer renderer = focused != null ? focused.GetComponent<SpriteRenderer>() : null;
                if (renderer != null && renderer.sprite != null)
                {
                    GUI.color = renderer.color;
                    GUI.DrawTexture(rect, renderer.sprite.texture, ScaleMode.ScaleToFit, true);
                }
            }
            GUI.color = previous;
        }

        private void DrawUnitHealthBar(Rect rect, float health01)
        {
            Color previous = GUI.color;
            GUI.color = new Color(0.015f, 0.02f, 0.02f, 0.92f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.Lerp(new Color(0.92f, 0.20f, 0.16f, 0.96f), new Color(0.28f, 0.92f, 0.42f, 0.96f), Mathf.Clamp01(health01));
            GUI.DrawTexture(new Rect(rect.x + 2f, rect.y + 2f, Mathf.Max(0f, (rect.width - 4f) * Mathf.Clamp01(health01)), rect.height - 4f), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private string CellDisplayName(V5CellEntity cell)
        {
            if (cell == null) return "Celula";
            if (cell.EvolutionPath != V5EvolutionPath.Uncommitted) return cell.EvolutionPath.ToString();
            return cell.Role == V5CellRole.Daughter ? "Celula hija" : "Celula libre";
        }

        private string OrganismRoleLabel(V5OrganismBlueprintKind kind)
        {
            switch (kind)
            {
                case V5OrganismBlueprintKind.Collector: return "Economia | Recolector fragil";
                case V5OrganismBlueprintKind.Harasser: return "Exploracion | Hostigador veloz";
                case V5OrganismBlueprintKind.Fighter: return "Linea | DPS cuerpo a cuerpo";
                case V5OrganismBlueprintKind.Interdictor: return "Control | Aura ralentizadora";
                case V5OrganismBlueprintKind.Lacrymaria: return "Artilleria | Perforante a distancia";
                case V5OrganismBlueprintKind.Anchor: return "Defensa | Tanque pesado";
                case V5OrganismBlueprintKind.Volvox: return "Produccion | Tanque colonial";
                case V5OrganismBlueprintKind.Tardigrade: return "Combate | Cazador";
                default: return "Organismo";
            }
        }

        private string RangeLabel(float range)
        {
            if (range > 3f) return range.ToString("0.0") + " \u2014 a distancia";
            return range <= 1.5f ? "cuerpo a cuerpo (" + range.ToString("0.0") + ")" : range.ToString("0.0");
        }

        private string SpeedLabel(float speed)
        {
            string category = speed < 1.5f ? "lenta" : (speed < 4f ? "media" : "rapida");
            return category + " (" + speed.ToString("0.0") + ")";
        }

        private string DamageKindLabel(V5DamageKind kind)
        {
            switch (kind)
            {
                case V5DamageKind.Physical: return "Contacto";
                case V5DamageKind.Piercing: return "Perforante";
                case V5DamageKind.Chemical: return "T\u00f3xico";
                default: return kind.ToString();
            }
        }

        private string ArmorLabel(float armor)
        {
            armor = Mathf.Clamp01(armor);
            string category = armor < 0.10f ? "Ninguna" : (armor < 0.25f ? "Baja" : (armor < 0.40f ? "Media" : "Alta"));
            return category + " (" + Mathf.RoundToInt(armor * 100f) + "%)";
        }

        private void DrawTroopPanel(V5OrganismMorph morph, float x, float y, float width)
        {
            GUI.Label(new Rect(x, y, width, 18f), "Tropas: " + TroopSummary(morph), label);
            float bw = (width - 12f) / 4f;
            DrawTroopButton(morph, V5OrganismBlueprintKind.Tardigrade, "Tard.", new Rect(x, y + 18f, bw, 20f));
            DrawTroopButton(morph, V5OrganismBlueprintKind.Volvox, "Volvox", new Rect(x + bw + 4f, y + 18f, bw, 20f));
            DrawTroopButton(morph, V5OrganismBlueprintKind.Anchor, "Foramin.", new Rect(x + (bw + 4f) * 2f, y + 18f, bw, 20f));
            DrawTroopButton(morph, V5OrganismBlueprintKind.Lacrymaria, "Lacrym.", new Rect(x + (bw + 4f) * 3f, y + 18f, bw, 20f));
            DrawTroopButton(morph, V5OrganismBlueprintKind.Collector, "Recolector", new Rect(x, y + 40f, bw, 20f));
            DrawTroopButton(morph, V5OrganismBlueprintKind.Harasser, "Hostig.", new Rect(x + bw + 4f, y + 40f, bw, 20f));
            DrawTroopButton(morph, V5OrganismBlueprintKind.Fighter, "Peleador", new Rect(x + (bw + 4f) * 2f, y + 40f, bw, 20f));
            DrawTroopButton(morph, V5OrganismBlueprintKind.Interdictor, "Dinoflag.", new Rect(x + (bw + 4f) * 3f, y + 40f, bw, 20f));
        }

        private void DrawTroopButton(V5OrganismMorph morph, V5OrganismBlueprintKind kind, string labelText, Rect rect)
        {
            int count = morph.CountActiveOrganismsByKind(kind);
            GUI.enabled = count > 0;
            if (GUI.Button(rect, labelText + " x" + count, button)) morph.SelectOrganismsByKind(kind, false);
            GUI.enabled = true;
        }

        private string TroopSummary(V5OrganismMorph morph)
        {
            return "T " + morph.CountActiveOrganismsByKind(V5OrganismBlueprintKind.Tardigrade) +
                   " | V " + morph.CountActiveOrganismsByKind(V5OrganismBlueprintKind.Volvox) +
                   " | Fm " + morph.CountActiveOrganismsByKind(V5OrganismBlueprintKind.Anchor) +
                   " | Lac " + morph.CountActiveOrganismsByKind(V5OrganismBlueprintKind.Lacrymaria) +
                   " | Pel " + morph.CountActiveOrganismsByKind(V5OrganismBlueprintKind.Fighter) +
                   " | Hos " + morph.CountActiveOrganismsByKind(V5OrganismBlueprintKind.Harasser) +
                   " | Din " + morph.CountActiveOrganismsByKind(V5OrganismBlueprintKind.Interdictor) +
                   " | Rec " + morph.CountActiveOrganismsByKind(V5OrganismBlueprintKind.Collector);
        }

        private void DrawShortcutLegend(V5GameManager gm, float x, float y, float width)
        {
            GUI.Label(new Rect(x, y, width, 18f), "Atajos Core:", label);
            GUI.Label(new Rect(x, y + 18f, width, 18f), "Click/drag selecciona | Shift suma | S libres | RMB mueve/ataca | A+clic attack-move", label);
            GUI.Label(new Rect(x, y + 36f, width, 18f), "Espacio madre | F farmea | Escape/click madre panel | D fuerza division | P +20 | K enemigo", label);
            GUI.Label(new Rect(x, y + 54f, width, 18f), "Shift+1..0 asigna grupo | 1..0 selecciona grupo | Tab overlay", label);
        }

        private void DrawMotherProductionPanel(V5GameManager gm, V5CellEntity mother, V5CoreMotherProductionSystem production)
        {
            V5OrganismMorph morph = gm != null ? gm.OrganismMorph : null;
            float panelHeight = Mathf.Min(572f, Mathf.Max(430f, Screen.height - 20f));
            float panelY = Mathf.Min(lastHudRect.yMax + 8f, Screen.height - panelHeight - 10f);
            panelY = Mathf.Max(10f, panelY);
            Rect r = new Rect(10f, panelY, 456f, panelHeight);
            lastMotherPanelRect = r;

            Color previousColor = GUI.color;
            GUI.color = new Color(0.025f, 0.045f, 0.045f, 0.92f);
            GUI.Box(r, GUIContent.none, box);
            GUI.color = previousColor;

            float x = r.x + 12f;
            float y = r.y + 10f;
            float w = r.width - 24f;
            GUI.Label(new Rect(x, y, w - 58f, 20f), "MADRE - CENTRO DE PRODUCCION", title);
            if (GUI.Button(new Rect(r.xMax - 52f, y - 1f, 38f, 22f), "x", button)) ShowMotherPanel = false;
            y += 24f;

            float hp01 = mother != null && mother.Stats.maxHp > 0f ? Mathf.Clamp01(mother.Stats.currentHp / mother.Stats.maxHp) : 0f;
            string interval = production != null ? production.EffectiveMotherProductionInterval.ToString("0.0") + "s" : "?";
            string next = production != null ? production.SecondsUntilNext.ToString("0.0") + "s" : "?";
            GUI.Label(new Rect(x, y, w, 18f), "Estado: HP " + Mathf.RoundToInt(hp01 * 100f) + "% | Biomasa " + mother.Resources.biomass.ToString("0") + " | ADN " + (production != null ? production.DnaPoints.ToString() : "0"), label);
            y += 20f;
            GUI.Label(new Rect(x, y, w, 18f), "Pool libre: " + gm.PlayerFreeCellCount() + "/" + gm.PlayerCellCap() + " | Slots tropas " + (morph != null ? morph.ActiveOrganismCount + "/" + Mathf.Max(1, morph.MaxActiveOrganisms) : "0/0") + " | Auto " + interval + " prox " + next, label);
            y += 26f;

            DrawForceDivisionButton(gm, production, new Rect(x, y, w, 34f));
            y += 44f;

            GUI.Label(new Rect(x, y, w, 18f), "Unidades", title);
            y += 22f;

            float upgradeHeight = 154f;
            float panelBottomPadding = 12f;
            float availableUnitsHeight = r.yMax - y - 12f - upgradeHeight - panelBottomPadding;
            Rect unitsScrollRect = new Rect(x, y, w, Mathf.Clamp(availableUnitsHeight, 108f, 220f));
            GUI.color = new Color(0.08f, 0.12f, 0.12f, 0.45f);
            GUI.Box(unitsScrollRect, GUIContent.none, box);
            GUI.color = previousColor;

            V5OrganismBlueprintKind[] unitKinds =
            {
                V5OrganismBlueprintKind.Collector,
                V5OrganismBlueprintKind.Harasser,
                V5OrganismBlueprintKind.Fighter,
                V5OrganismBlueprintKind.Interdictor,
                V5OrganismBlueprintKind.Anchor,
                V5OrganismBlueprintKind.Lacrymaria,
                V5OrganismBlueprintKind.Volvox,
                V5OrganismBlueprintKind.Tardigrade
            };
            string[] unitDescriptions =
            {
                "Economia movil, fragil",
                "Explorador veloz, golpea y corre",
                "DPS de linea: lento, solido y contundente",
                "Control fragil: ralentiza enemigos cerca",
                "Defensor lento y resistente",
                "Artilleria perforante de largo alcance",
                "Tanque productor, lento",
                "Cazador agil, combate"
            };

            float contentHeight = Mathf.Max(unitsScrollRect.height - 4f, unitKinds.Length * 40f + 4f);
            Rect contentRect = new Rect(0f, 0f, w - 20f, contentHeight);
            motherUnitsScroll = GUI.BeginScrollView(unitsScrollRect, motherUnitsScroll, contentRect, false, true);
            float unitY = 2f;
            for (int i = 0; i < unitKinds.Length; i++)
            {
                DrawProductionButton(morph, unitKinds[i], unitDescriptions[i], new Rect(2f, unitY, contentRect.width - 4f, 36f));
                unitY += 40f;
            }
            GUI.EndScrollView();
            y += unitsScrollRect.height + 12f;

            Rect upgrades = new Rect(x, y, w, upgradeHeight);
            GUI.color = new Color(0.08f, 0.12f, 0.12f, 0.62f);
            GUI.Box(upgrades, GUIContent.none, box);
            GUI.color = previousColor;
            GUI.Label(new Rect(upgrades.x + 10f, upgrades.y + 8f, upgrades.width - 20f, 18f), "Mejoras de la madre", title);
            if (production == null)
            {
                GUI.Label(new Rect(upgrades.x + 10f, upgrades.y + 30f, upgrades.width - 20f, 32f), "Sistema de mejoras no disponible.", small);
                return;
            }

            DrawUpgradeButton(production, V5MotherUpgradeId.SynthesisAcceleration, "Menos intervalo: produce celulas mas rapido.", new Rect(upgrades.x + 10f, upgrades.y + 30f, upgrades.width - 20f, 34f));
            DrawUpgradeButton(production, V5MotherUpgradeId.BiofilmProtector, "Aura defensiva: enemigos mas lentos cerca de la madre.", new Rect(upgrades.x + 10f, upgrades.y + 68f, upgrades.width - 20f, 34f));
            DrawUpgradeButton(production, V5MotherUpgradeId.ColonialCapacity, "+15 cap libre por nivel.", new Rect(upgrades.x + 10f, upgrades.y + 106f, upgrades.width - 20f, 34f));
        }

        private void DrawProductionButton(V5OrganismMorph morph, V5OrganismBlueprintKind kind, string description, Rect rect)
        {
            if (morph == null) return;
            string reason;
            bool can = morph.CanCreateOrganism(kind, out reason);
            string name = morph.BlueprintNameFor(kind);
            int cost = morph.RequiredFreeCellsFor(kind);
            string labelText = name + "  | costo " + cost + " celulas  | " + (can ? "crear" : reason);

            GUI.enabled = can;
            if (GUI.Button(rect, labelText + "\n" + description, can ? button : disabledButton))
            {
                morph.CreateOrganism(kind);
            }
            GUI.enabled = true;
        }

        private void DrawUpgradeButton(V5CoreMotherProductionSystem production, V5MotherUpgradeId id, string description, Rect rect)
        {
            if (production == null) return;
            string reason;
            bool can = production.CanBuyUpgrade(id, out reason);
            int level = production.UpgradeLevel(id);
            int max = production.UpgradeMaxLevel(id);
            int cost = production.UpgradeCost(id);
            string text = production.UpgradeName(id) + "  Nv " + level + "/" + max + "  | " + cost + " ADN  | " + (can ? "comprar" : reason) + "\n" + description;

            GUI.enabled = can;
            if (GUI.Button(rect, text, can ? button : disabledButton)) production.BuyUpgrade(id);
            GUI.enabled = true;
        }

        private void DrawForceDivisionButton(V5GameManager gm, V5CoreMotherProductionSystem production, Rect rect)
        {
            if (production == null || gm == null) return;
            string reason;
            bool can = production.CanForceDivision(gm, out reason);
            float cost = production.CurrentForceDivisionCost;
            string text = "Forzar division (D)  | " + cost.ToString("0") + " biomasa  | " + (can ? "crear ahora" : reason) +
                          "\nCosto sube por rafaga; calor " + production.ForceDivisionHeat.ToString("0.0");

            GUI.enabled = can;
            if (GUI.Button(rect, text, can ? button : disabledButton)) production.TryForceDivision(gm);
            GUI.enabled = true;
        }

        private string ProductionStatus(V5GameManager gm, V5CellEntity mother, V5CoreMotherProductionSystem production, float divideCost)
        {
            string timer = production != null ? production.SecondsUntilNext.ToString("0.0") + "s" : "?";
            string interval = production != null ? production.EffectiveMotherProductionInterval.ToString("0.0") + "s" : "?";
            string force = production != null ? " | D fuerza " + production.CurrentForceDivisionCost.ToString("0") + " bio" : "";
            return "Auto madre " + interval + " | prox " + timer + " | " + divideCost.ToString("0") + " bio - " + DivideStatus(gm, mother, divideCost) + force + " | P +20";
        }

        private bool ProductionReady(V5GameManager gm, V5CellEntity mother, float divideCost)
        {
            return gm != null && mother != null && gm.CanAddPlayerCellFrom(mother) && mother.Resources.biomass >= divideCost;
        }

        private string DivideStatus(V5GameManager gm, V5CellEntity mother, float divideCost)
        {
            if (gm != null && !gm.CanAddPlayerCellFrom(mother)) return "cap";
            if (mother.Stats.currentHp <= mother.Stats.maxHp * 0.35f) return "HP baja";
            float missing = divideCost - mother.Resources.biomass;
            if (missing > 0f) return "falta " + Mathf.Ceil(missing).ToString("0");
            return "listo";
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box);
            box.normal.background = Texture2D.whiteTexture;
            box.normal.textColor = Color.white;
            label = new GUIStyle(GUI.skin.label);
            label.fontSize = 12;
            label.normal.textColor = new Color(0.88f, 1f, 0.92f, 0.96f);
            title = new GUIStyle(label);
            title.fontSize = 13;
            title.fontStyle = FontStyle.Bold;
            title.normal.textColor = new Color(0.72f, 1f, 0.82f, 1f);
            small = new GUIStyle(label);
            small.fontSize = 11;
            small.wordWrap = true;
            small.normal.textColor = new Color(0.72f, 0.92f, 0.82f, 0.92f);
            readyLabel = new GUIStyle(label);
            readyLabel.normal.textColor = new Color(0.58f, 1f, 0.68f, 0.98f);
            blockedLabel = new GUIStyle(label);
            blockedLabel.normal.textColor = new Color(1f, 0.78f, 0.48f, 0.98f);
            endBanner = new GUIStyle(GUI.skin.label);
            endBanner.alignment = TextAnchor.MiddleCenter;
            endBanner.fontSize = 20;
            endBanner.fontStyle = FontStyle.Bold;
            endBanner.normal.textColor = Color.white;
            button = new GUIStyle(GUI.skin.button);
            button.fontSize = 11;
            button.alignment = TextAnchor.MiddleCenter;
            disabledButton = new GUIStyle(button);
            disabledButton.normal.textColor = new Color(1f, 0.74f, 0.54f, 0.88f);
        }
    }
}
