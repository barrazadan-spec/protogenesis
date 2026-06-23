using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5GenomePanelIMGUI : MonoBehaviour
    {
        public bool ShowPanel;

        private Vector2 treeScroll;
        private V5AdaptationId selected = V5AdaptationId.None;
        private V5AdaptationId focusedFromCoach = V5AdaptationId.None;
        private float coachFocusUntil;
        private bool canonFocusOnly;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle small;
        private GUIStyle node;
        private GUIStyle nodeSmall;
        private GUIStyle nodeLocked;
        private GUIStyle nodeInstalled;
        private GUIStyle laneHeader;

        private static readonly V5AdaptationTier[] TierOrder =
        {
            V5AdaptationTier.T1Prokaryote,
            V5AdaptationTier.T2Eukaryogenesis,
            V5AdaptationTier.T3Specialization,
            V5AdaptationTier.T4ColonialBody,
            V5AdaptationTier.T5Apex
        };

        private void Start()
        {
            V5PanelRouter.Register("Genoma", () => ShowPanel, v => ShowPanel = v);
        }

        public void TogglePanel()
        {
            bool next = !ShowPanel;
            if (next)
            {
                V5PanelRouter.CloseOthers("Genoma");
                FocusCoachSuggestion();
            }
            ShowPanel = next;
        }

        public V5AdaptationId SelectedAdaptation
        {
            get { return selected; }
        }

        public void OpenFocused(V5AdaptationId id)
        {
            FocusAdaptation(id);
            V5PanelRouter.CloseOthers("Genoma");
            ShowPanel = true;
        }

        public void FocusAdaptation(V5AdaptationId id)
        {
            if (id == V5AdaptationId.None || V5AdaptationLibrary.Get(id) == null) return;
            selected = id;
            focusedFromCoach = id;
            coachFocusUntil = Time.unscaledTime + 8f;
            canonFocusOnly = false;
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null || gm.Adaptations == null) return;

            EnsureSelected(gm);
            Rect r = PanelRect();
            GUI.Box(r, "", box);
            const float headerHeight = 190f;
            DrawHeader(gm, new Rect(r.x + 12f, r.y + 10f, r.width - 24f, headerHeight));

            Rect tree = new Rect(r.x + 12f, r.y + headerHeight + 24f, r.width - 24f, Mathf.Max(250f, r.height - 412f));
            DrawAdaptationTree(gm, tree);

            Rect detail = new Rect(r.x + 12f, tree.yMax + 10f, r.width - 24f, r.yMax - tree.yMax - 22f);
            DrawDetail(gm, detail);
        }

        private Rect PanelRect()
        {
            float w = Mathf.Min(980f, Screen.width - 24f);
            float h = Mathf.Min(780f, Screen.height - 24f);
            float x = Screen.width >= 1240f ? 522f : 12f;
            if (x + w > Screen.width - 12f) x = 12f;
            float y = Screen.height >= 840f ? 12f : 252f;
            if (y + h > Screen.height - 12f) h = Mathf.Max(420f, Screen.height - y - 12f);
            return new Rect(x, y, w, h);
        }

        private void DrawHeader(V5GameManager gm, Rect r)
        {
            V5AdaptationSystem adaptations = gm.Adaptations;
            V5IdentityRecognizer identity = gm.Identity;
            GUI.Label(new Rect(r.x, r.y, 320f, 24f), "GENOMA - ARBOL DE ADAPTACIONES", title);
            GUI.Label(new Rect(r.x, r.y + 28f, r.width * 0.56f, 20f), adaptations.Summary(), small);
            GUI.Label(new Rect(r.x, r.y + 50f, r.width * 0.56f, 20f), identity != null ? identity.Summary : "Identidad: sin recognizer.", small);
            V5MvpRoute mvpRoute = ActiveMvpRoute(gm);
            GUI.Label(new Rect(r.x, r.y + 72f, r.width * 0.56f, 20f), "Ruta MVP: " + V5MvpCanon.ProgressText(mvpRoute, adaptations) + " | " + V5MvpCanon.BuildProgressText(mvpRoute, adaptations), small);
            GUI.Label(new Rect(r.x, r.y + 94f, r.width * 0.56f, 20f), "Instaladas: " + adaptations.InstalledNames(), small);
            DrawRouteIntentCards(gm, new Rect(r.x, r.y + 122f, r.width * 0.56f, 58f));

            Rect actions = new Rect(r.x + r.width * 0.58f, r.y, r.width * 0.42f, r.height);
            V5AdaptationId suggestedId = SuggestedId(gm);
            bool fromCoach = IsCoachSuggestion(gm, suggestedId);
            V5AdaptationDefinition suggested = V5AdaptationLibrary.Get(suggestedId);
            string suggestedText = suggested != null ? suggested.shortName : "ninguna";
            string label = fromCoach ? "Coach: " : "Sugerida: ";
            GUI.Label(new Rect(actions.x, actions.y, actions.width, 22f), label + suggestedText, small);
            string description = suggested != null ? suggested.description : "Elige una adaptacion segun ambiente y plan de combate.";
            if (fromCoach && gm.Diagnostics != null)
                description = gm.Diagnostics.CoachAdaptationStatus + " | " + gm.Diagnostics.CoachAction;
            GUI.Label(new Rect(actions.x, actions.y + 22f, actions.width, 38f), description, small);

            if (GUI.Button(new Rect(actions.x, actions.y + 72f, 132f, 28f), canonFocusOnly ? "Ver todo" : "Solo ruta", nodeSmall))
            {
                canonFocusOnly = !canonFocusOnly;
            }

            bool canPrepareRoute = gm.MvpIntent != null && ActiveMvpRoute(gm) != V5MvpRoute.None;
            GUI.enabled = canPrepareRoute;
            if (GUI.Button(new Rect(actions.x, actions.y + 106f, 132f, 28f), "Preparar test", nodeSmall) && gm.MvpIntent != null)
            {
                if (gm.MvpIntent.PrepareRoutePlaytest(gm))
                {
                    V5AdaptationId next = gm.MvpIntent.SuggestedCoreAdaptation(gm);
                    if (next != V5AdaptationId.None) FocusAdaptation(next);
                }
            }
            GUI.enabled = true;
            if (gm.MvpIntent != null)
                GUI.Label(new Rect(actions.x + 140f, actions.y + 104f, actions.width - 140f, 58f), gm.MvpIntent.LastMessage, small);

            if (suggested != null)
            {
                string reason;
                bool can = adaptations.CanInstall(suggested.id, gm.MotherCell, out reason);
                if (GUI.Button(new Rect(actions.x + 140f, actions.y + 72f, 80f, 28f), "Enfocar", nodeSmall))
                {
                    FocusAdaptation(suggested.id);
                }
                GUI.enabled = can && !adaptations.Has(suggested.id);
                if (GUI.Button(new Rect(actions.x + 226f, actions.y + 72f, actions.width - 226f, 28f), fromCoach ? "Instalar coach" : "Instalar sugerida", nodeSmall))
                {
                    adaptations.Install(suggested.id, gm.MotherCell);
                    selected = suggested.id;
                }
                GUI.enabled = true;
            }
        }

        private void DrawRouteIntentCards(V5GameManager gm, Rect r)
        {
            if (gm == null || gm.MvpIntent == null) return;
            bool compact = r.width < 440f;
            float gap = compact ? 4f : 6f;
            float autoW = compact ? 50f : 58f;
            DrawRouteButton(gm, V5MvpRoute.None, new Rect(r.x, r.y, autoW, r.height), compact ? "Auto\nID\ncoach" : "Auto\nidentidad\ncoach", true);

            float x = r.x + autoW + gap;
            float w = Mathf.Max(54f, (r.width - autoW - gap * 4f) / 4f);
            DrawRouteButton(gm, V5MvpRoute.Bacteria, new Rect(x, r.y, w, r.height), RouteCardLabel(gm, V5MvpRoute.Bacteria, compact), false);
            x += w + gap;
            DrawRouteButton(gm, V5MvpRoute.Amoeba, new Rect(x, r.y, w, r.height), RouteCardLabel(gm, V5MvpRoute.Amoeba, compact), false);
            x += w + gap;
            DrawRouteButton(gm, V5MvpRoute.PhotosyntheticProducer, new Rect(x, r.y, w, r.height), RouteCardLabel(gm, V5MvpRoute.PhotosyntheticProducer, compact), false);
            x += w + gap;
            DrawRouteButton(gm, V5MvpRoute.Volvox, new Rect(x, r.y, w, r.height), RouteCardLabel(gm, V5MvpRoute.Volvox, compact), false);
        }

        private void DrawRouteButton(V5GameManager gm, V5MvpRoute route, Rect rect, string label, bool autoButton)
        {
            bool selectedRoute = gm != null && gm.MvpIntent != null &&
                (gm.MvpIntent.Intent == route ||
                 (!autoButton && gm.MvpIntent.Intent == V5MvpRoute.None && gm.MvpIntent.EffectiveRoute(gm) == route && V5MvpCanon.Progress01(route, gm.Adaptations) > 0f));
            Color old = GUI.color;
            GUI.color = selectedRoute ? RouteCardColor(route, true) : RouteCardColor(route, false);
            if (GUI.Button(rect, label, nodeSmall) && gm != null && gm.MvpIntent != null)
            {
                gm.MvpIntent.SetIntent(route);
                V5AdaptationId next = route == V5MvpRoute.None ? SuggestedId(gm) : SuggestedRouteCardAdaptation(gm, route);
                if (next != V5AdaptationId.None) FocusAdaptation(next);
            }
            GUI.color = old;
        }

        private string RouteCardLabel(V5GameManager gm, V5MvpRoute route, bool compact)
        {
            int progress = Mathf.RoundToInt(V5MvpCanon.Progress01(route, gm != null ? gm.Adaptations : null) * 100f);
            V5AdaptationDefinition next = V5AdaptationLibrary.Get(SuggestedRouteCardAdaptation(gm, route));
            string nextText = next != null ? next.shortName : "lista";
            return RouteCardName(route, compact) + " " + progress + "%\n" + RouteCardSubtitle(route, compact) + "\nprox " + nextText;
        }

        private string RouteCardName(V5MvpRoute route, bool compact)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return "Bacteria";
                case V5MvpRoute.Amoeba: return "Ameba";
                case V5MvpRoute.PhotosyntheticProducer: return compact ? "Prod" : "Productor";
                case V5MvpRoute.Volvox: return "Volvox";
                default: return "Auto";
            }
        }

        private string RouteCardSubtitle(V5MvpRoute route, bool compact)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return compact ? "swarm" : "swarm/coloniza";
                case V5MvpRoute.Amoeba: return compact ? "caza" : "caza/fagocita";
                case V5MvpRoute.PhotosyntheticProducer: return compact ? "luz/O2" : "luz/oxigeno";
                case V5MvpRoute.Volvox: return compact ? "cuerpo" : "cuerpo/castas";
                default: return "autodetecta";
            }
        }

        private Color RouteCardColor(V5MvpRoute route, bool active)
        {
            Color baseColor;
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    baseColor = new Color(0.50f, 0.92f, 0.84f, 1f);
                    break;
                case V5MvpRoute.Amoeba:
                    baseColor = new Color(0.96f, 0.66f, 0.84f, 1f);
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    baseColor = new Color(0.60f, 0.94f, 0.58f, 1f);
                    break;
                case V5MvpRoute.Volvox:
                    baseColor = new Color(0.62f, 0.88f, 1f, 1f);
                    break;
                default:
                    baseColor = new Color(0.76f, 0.82f, 0.88f, 1f);
                    break;
            }
            return active ? baseColor : Color.Lerp(baseColor, Color.white, 0.35f);
        }

        private V5AdaptationId SuggestedRouteCardAdaptation(V5GameManager gm, V5MvpRoute route)
        {
            if (route == V5MvpRoute.None || gm == null || gm.Adaptations == null) return V5AdaptationId.None;

            V5AdaptationId installableCore = FirstInstallable(gm, V5MvpCanon.CoreAdaptationArray(route));
            if (installableCore != V5AdaptationId.None) return installableCore;

            if (route != V5MvpRoute.Bacteria && gm.Adaptations.CountInTier(V5AdaptationTier.T1Prokaryote) < 2)
            {
                V5AdaptationId primer = FirstInstallable(gm, V5MvpCanon.PrimerAdaptationArray(route));
                if (primer != V5AdaptationId.None) return primer;

                V5AdaptationId missingPrimer = FirstMissing(gm, V5MvpCanon.PrimerAdaptationArray(route));
                if (missingPrimer != V5AdaptationId.None) return missingPrimer;
            }

            return FirstMissing(gm, V5MvpCanon.CoreAdaptationArray(route));
        }

        private V5AdaptationId FirstInstallable(V5GameManager gm, V5AdaptationId[] ids)
        {
            if (gm == null || gm.Adaptations == null || ids == null) return V5AdaptationId.None;
            for (int i = 0; i < ids.Length; i++)
            {
                if (gm.Adaptations.Has(ids[i])) continue;
                string reason;
                if (gm.MotherCell != null && gm.Adaptations.CanInstall(ids[i], gm.MotherCell, out reason)) return ids[i];
            }
            return V5AdaptationId.None;
        }

        private V5AdaptationId FirstMissing(V5GameManager gm, V5AdaptationId[] ids)
        {
            if (gm == null || gm.Adaptations == null || ids == null) return V5AdaptationId.None;
            for (int i = 0; i < ids.Length; i++)
                if (!gm.Adaptations.Has(ids[i])) return ids[i];
            return V5AdaptationId.None;
        }

        private void DrawAdaptationTree(V5GameManager gm, Rect view)
        {
            GUI.Box(view, "", box);
            V5AdaptationId[] canon = CurrentCanon(gm);
            IReadOnlyList<V5AdaptationDefinition> all = V5AdaptationLibrary.All();
            Dictionary<V5AdaptationId, Rect> rects = new Dictionary<V5AdaptationId, Rect>(all.Count);

            float gap = 10f;
            float contentWidth = Mathf.Max(view.width - 22f, 860f);
            float colW = (contentWidth - gap * (TierOrder.Length - 1)) / TierOrder.Length;
            float nodeH = 44f;
            float nodeGap = 8f;
            float top = 34f;
            float contentHeight = top + MaxVisibleTierCount(gm, canon) * (nodeH + nodeGap) + 18f;

            Rect content = new Rect(0f, 0f, contentWidth, Mathf.Max(contentHeight, view.height - 18f));
            treeScroll = GUI.BeginScrollView(view, treeScroll, content);

            for (int t = 0; t < TierOrder.Length; t++)
            {
                float x = t * (colW + gap);
                Rect header = new Rect(x, 0f, colW, 26f);
                Color old = GUI.color;
                GUI.color = TierColor(TierOrder[t]);
                GUI.Box(header, TierTitle(TierOrder[t]), laneHeader);
                GUI.color = old;

                int row = 0;
                for (int i = 0; i < all.Count; i++)
                {
                    V5AdaptationDefinition def = all[i];
                    if (def.tier != TierOrder[t] || !ShouldShow(def, gm, canon)) continue;
                    rects[def.id] = new Rect(x, top + row * (nodeH + nodeGap), colW, nodeH);
                    row++;
                }
            }

            DrawPrerequisiteLines(all, rects, gm.Adaptations);
            V5AdaptationId hovered = V5AdaptationId.None;
            for (int i = 0; i < all.Count; i++)
            {
                V5AdaptationDefinition def = all[i];
                Rect rect;
                if (!rects.TryGetValue(def.id, out rect)) continue;
                V5AdaptationId h = DrawNode(def, rect, gm, IsCanon(def.id, canon));
                if (h != V5AdaptationId.None) hovered = h;
            }

            if (hovered != V5AdaptationId.None) selected = hovered;
            GUI.EndScrollView();
        }

        private V5AdaptationId DrawNode(V5AdaptationDefinition def, Rect rect, V5GameManager gm, bool canon)
        {
            V5AdaptationSystem adaptations = gm.Adaptations;
            bool installed = adaptations.Has(def.id);
            string reason;
            bool can = adaptations.CanInstall(def.id, gm.MotherCell, out reason);
            bool isSelected = selected == def.id;
            bool isCoach = IsCoachSuggestion(gm, def.id);
            bool isPulse = focusedFromCoach == def.id && Time.unscaledTime <= coachFocusUntil;

            Color oldColor = GUI.color;
            Color bg = installed ? new Color(0.56f, 1f, 0.62f, 1f) : (can ? def.color : new Color(0.42f, 0.45f, 0.48f, 0.84f));
            GUI.color = bg;
            GUIStyle style = installed ? nodeInstalled : (can ? node : nodeLocked);
            string label = (installed ? "[x] " : "") + def.shortName + "\n" + NodeStatus(installed, can, def, reason, gm);
            GUI.enabled = can && !installed;
            if (GUI.Button(rect, label, style))
            {
                adaptations.Install(def.id, gm.MotherCell);
                selected = def.id;
            }
            GUI.enabled = true;
            GUI.color = oldColor;

            if (canon) DrawSolidRect(new Rect(rect.x, rect.y, rect.width, 3f), new Color(1f, 0.92f, 0.34f, 0.95f));
            if (!installed && !can) DrawSolidRect(new Rect(rect.x, rect.yMax - 4f, rect.width, 4f), BlockColor(reason));
            if (isCoach || isPulse) DrawSelectionFrame(new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, rect.height - 2f), new Color(1f, 0.72f, 0.28f, 0.95f));
            if (isSelected) DrawSelectionFrame(rect, new Color(0.55f, 0.95f, 1f, 0.95f));
            if (def.latentInChampion) DrawSolidRect(new Rect(rect.x + rect.width - 5f, rect.y + 4f, 3f, rect.height - 8f), new Color(1f, 0.76f, 0.24f, 0.95f));

            return rect.Contains(Event.current.mousePosition) ? def.id : V5AdaptationId.None;
        }

        private void DrawDetail(V5GameManager gm, Rect r)
        {
            GUI.Box(r, "", box);
            V5AdaptationDefinition def = V5AdaptationLibrary.Get(selected);
            if (def == null)
            {
                GUI.Label(new Rect(r.x + 10f, r.y + 10f, r.width - 20f, 24f), "Selecciona una adaptacion para ver detalles.", small);
                return;
            }

            bool installed = gm.Adaptations.Has(def.id);
            string reason;
            bool can = gm.Adaptations.CanInstall(def.id, gm.MotherCell, out reason);
            string state = installed ? "Instalada" : (can ? "Disponible" : reason);
            GUI.Label(new Rect(r.x + 10f, r.y + 8f, r.width * 0.55f, 22f), def.displayName + " | " + state, title);
            GUI.Label(new Rect(r.x + 10f, r.y + 34f, r.width * 0.56f, 38f), def.description, small);
            GUI.Label(new Rect(r.x + 10f, r.y + 74f, r.width * 0.56f, 20f), "Costo: " + gm.Adaptations.CostText(def.cost), small);
            GUI.Label(new Rect(r.x + 10f, r.y + 96f, r.width * 0.56f, 38f), "Efecto: " + JoinEffects(def), small);
            GUI.Label(new Rect(r.x + 10f, r.y + 136f, r.width * 0.56f, 30f), "Siguiente: " + gm.Adaptations.NextStepFor(def.id, gm.MotherCell), small);
            V5AdaptationId missing = gm.Adaptations.FirstMissingPrerequisite(def.id);
            if (missing != V5AdaptationId.None)
            {
                V5AdaptationDefinition missingDef = V5AdaptationLibrary.Get(missing);
                string text = "Ver requisito: " + (missingDef != null ? missingDef.shortName : missing.ToString());
                if (GUI.Button(new Rect(r.x + 10f, r.y + 166f, 150f, 24f), text, nodeSmall)) FocusAdaptation(missing);
            }

            float rx = r.x + r.width * 0.60f;
            float rw = r.width * 0.38f;
            GUI.Label(new Rect(rx, r.y + 10f, rw, 20f), "Ruta afin: " + def.routeHint + " | " + KindLabel(def), small);
            GUI.Label(new Rect(rx, r.y + 32f, rw, 42f), "Prerequisitos: " + gm.Adaptations.PrerequisiteChecklist(def.id), small);
            GUI.Label(new Rect(rx, r.y + 76f, rw, 42f), "Recursos: " + gm.Adaptations.ResourceChecklist(def.id, gm.MotherCell), small);
            GUI.Label(new Rect(rx, r.y + 120f, rw, 34f), "Counters: " + (string.IsNullOrEmpty(def.naturalCounters) ? "playtest" : def.naturalCounters), small);
            float bottomY = r.y + 154f;
            if (def.latentInChampion && !string.IsNullOrEmpty(def.championEffect))
            {
                GUI.Label(new Rect(rx, bottomY, rw, 22f), "Champion: " + def.championEffect, small);
                bottomY += 24f;
            }
            if (IsCoachSuggestion(gm, def.id) && gm.Diagnostics != null)
                GUI.Label(new Rect(rx, bottomY, rw, 36f), "Coach: " + gm.Diagnostics.CoachAction, small);
        }

        private void DrawPrerequisiteLines(IReadOnlyList<V5AdaptationDefinition> all, Dictionary<V5AdaptationId, Rect> rects, V5AdaptationSystem adaptations)
        {
            for (int i = 0; i < all.Count; i++)
            {
                V5AdaptationDefinition def = all[i];
                Rect child;
                if (!rects.TryGetValue(def.id, out child)) continue;
                for (int p = 0; p < def.prerequisites.Length; p++)
                {
                    Rect parent;
                    if (!rects.TryGetValue(def.prerequisites[p], out parent)) continue;
                    bool parentInstalled = adaptations.Has(def.prerequisites[p]);
                    bool active = parentInstalled && adaptations.Has(def.id);
                    bool missingForSelection = def.id == selected && !parentInstalled;
                    Color color = missingForSelection ? new Color(1f, 0.35f, 0.28f, 0.80f) : (active ? new Color(0.46f, 1f, 0.62f, 0.70f) : new Color(0.56f, 0.76f, 0.84f, 0.35f));
                    DrawConnector(parent, child, color);
                }
            }
        }

        private void DrawConnector(Rect from, Rect to, Color color)
        {
            float x1 = from.xMax;
            float y1 = from.y + from.height * 0.5f;
            float x2 = to.x;
            float y2 = to.y + to.height * 0.5f;
            float mid = (x1 + x2) * 0.5f;
            DrawSolidRect(new Rect(x1, y1 - 1f, Mathf.Max(1f, mid - x1), 2f), color);
            DrawSolidRect(new Rect(mid - 1f, Mathf.Min(y1, y2), 2f, Mathf.Max(2f, Mathf.Abs(y2 - y1))), color);
            DrawSolidRect(new Rect(mid, y2 - 1f, Mathf.Max(1f, x2 - mid), 2f), color);
        }

        private void EnsureSelected(V5GameManager gm)
        {
            if (V5AdaptationLibrary.Get(selected) != null) return;
            V5AdaptationId diagnosticSuggested = SuggestedId(gm);
            if (V5AdaptationLibrary.Get(diagnosticSuggested) != null)
            {
                selected = diagnosticSuggested;
                return;
            }
            if (gm.Identity != null && V5AdaptationLibrary.Get(gm.Identity.SuggestedNext) != null)
            {
                selected = gm.Identity.SuggestedNext;
                return;
            }

            IReadOnlyList<V5AdaptationDefinition> all = V5AdaptationLibrary.All();
            for (int i = 0; i < all.Count; i++)
            {
                if (!gm.Adaptations.Has(all[i].id))
                {
                    selected = all[i].id;
                    return;
                }
            }
        }

        private void FocusCoachSuggestion()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Diagnostics == null) return;
            if (gm.Diagnostics.CoachAdaptation == V5AdaptationId.None) return;
            FocusAdaptation(gm.Diagnostics.CoachAdaptation);
        }

        private V5AdaptationId SuggestedId(V5GameManager gm)
        {
            if (gm != null && gm.MvpIntent != null && gm.MvpIntent.HasIntent)
            {
                V5AdaptationId routeSuggested = gm.MvpIntent.SuggestedCoreAdaptation(gm);
                if (routeSuggested != V5AdaptationId.None) return routeSuggested;
            }
            if (gm != null && gm.Diagnostics != null && gm.Diagnostics.CoachAdaptation != V5AdaptationId.None)
                return gm.Diagnostics.CoachAdaptation;
            if (gm != null && gm.Identity != null)
                return gm.Identity.SuggestedNext;
            return V5AdaptationId.None;
        }

        private bool IsCoachSuggestion(V5GameManager gm, V5AdaptationId id)
        {
            return id != V5AdaptationId.None && gm != null && gm.Diagnostics != null && gm.Diagnostics.CoachAdaptation == id;
        }

        private V5AdaptationId[] CurrentCanon(V5GameManager gm)
        {
            V5MvpRoute route = ActiveMvpRoute(gm);
            if (route != V5MvpRoute.None) return V5MvpCanon.PlaytestAdaptationArray(route);

            if (gm != null && gm.Identity != null && gm.Identity.Identity != V5IdentityId.LUCA)
                return V5BiologyCanon.AdaptationsForIdentity(gm.Identity.Identity);
            V5EvolutionPath path = gm != null && gm.MotherCell != null ? gm.MotherCell.EvolutionPath : V5EvolutionPath.Uncommitted;
            return V5BiologyCanon.AdaptationsForRoute(path);
        }

        private V5MvpRoute ActiveMvpRoute(V5GameManager gm)
        {
            if (gm != null && gm.MvpIntent != null) return gm.MvpIntent.EffectiveRoute(gm);
            return V5MvpCanon.CurrentRoute(gm);
        }

        private bool ShouldShow(V5AdaptationDefinition def, V5GameManager gm, V5AdaptationId[] canon)
        {
            if (!canonFocusOnly) return true;
            if (IsCanon(def.id, canon)) return true;
            if (gm != null && gm.Adaptations != null && gm.Adaptations.Has(def.id)) return true;
            for (int i = 0; i < canon.Length; i++)
            {
                V5AdaptationDefinition c = V5AdaptationLibrary.Get(canon[i]);
                if (c == null || c.prerequisites == null) continue;
                for (int p = 0; p < c.prerequisites.Length; p++)
                    if (c.prerequisites[p] == def.id) return true;
            }
            return false;
        }

        private int MaxVisibleTierCount(V5GameManager gm, V5AdaptationId[] canon)
        {
            int max = 1;
            IReadOnlyList<V5AdaptationDefinition> all = V5AdaptationLibrary.All();
            for (int t = 0; t < TierOrder.Length; t++)
            {
                int count = 0;
                for (int i = 0; i < all.Count; i++)
                    if (all[i].tier == TierOrder[t] && ShouldShow(all[i], gm, canon)) count++;
                max = Mathf.Max(max, count);
            }
            return max;
        }

        private bool IsCanon(V5AdaptationId id, V5AdaptationId[] canon)
        {
            if (canon == null) return false;
            for (int i = 0; i < canon.Length; i++)
                if (canon[i] == id) return true;
            return false;
        }

        private string NodeStatus(bool installed, bool can, V5AdaptationDefinition def, string reason, V5GameManager gm)
        {
            if (installed) return def.countsTowardCap ? "activa" : "hito";
            if (can) return def.countsTowardCap ? "lista" : "hito listo";
            if (reason.StartsWith("Requiere")) return reason.Replace("Requiere ", "req ");
            if (reason.StartsWith("Faltan") && gm != null && gm.Adaptations != null)
            {
                string missing = gm.Adaptations.MissingResourceSummary(def.id, gm.MotherCell);
                return string.IsNullOrEmpty(missing) ? "recursos" : missing;
            }
            if (reason.StartsWith("Cap")) return "cap lleno";
            return "bloqueada";
        }

        private Color BlockColor(string reason)
        {
            if (string.IsNullOrEmpty(reason)) return new Color(0.70f, 0.70f, 0.70f, 0.88f);
            if (reason.StartsWith("Requiere")) return new Color(1f, 0.35f, 0.28f, 0.92f);
            if (reason.StartsWith("Faltan")) return new Color(0.35f, 0.70f, 1f, 0.92f);
            if (reason.StartsWith("Cap")) return new Color(0.82f, 0.42f, 1f, 0.92f);
            return new Color(0.95f, 0.78f, 0.32f, 0.92f);
        }

        private string KindLabel(V5AdaptationDefinition def)
        {
            if (def.kind == V5AdaptationKind.Milestone) return "Hito";
            if (def.kind == V5AdaptationKind.Apex) return "Apex";
            if (!def.countsTowardCap) return "No cuenta cap";
            return "Activa";
        }

        private string PrerequisiteText(V5AdaptationDefinition def)
        {
            if (def.prerequisites == null || def.prerequisites.Length == 0) return "ninguno";
            string s = "";
            for (int i = 0; i < def.prerequisites.Length; i++)
            {
                V5AdaptationDefinition p = V5AdaptationLibrary.Get(def.prerequisites[i]);
                s += p != null ? p.shortName : def.prerequisites[i].ToString();
                if (i < def.prerequisites.Length - 1) s += ", ";
            }
            return s;
        }

        private string JoinEffects(V5AdaptationDefinition def)
        {
            string result = "";
            AddEffect(ref result, def.motherEffect);
            AddEffect(ref result, def.productionEffect);
            AddEffect(ref result, def.bodyEffect);
            AddEffect(ref result, def.identityEffect);
            return result.Length > 0 ? result : "Actualiza identidad y estadisticas.";
        }

        private void AddEffect(ref string result, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (result.Length > 0) result += " | ";
            result += text;
        }

        private string TierTitle(V5AdaptationTier tier)
        {
            switch (tier)
            {
                case V5AdaptationTier.T1Prokaryote: return "T1 Supervivencia";
                case V5AdaptationTier.T2Eukaryogenesis: return "T2 Eucariota";
                case V5AdaptationTier.T3Specialization: return "T3 Especializacion";
                case V5AdaptationTier.T4ColonialBody: return "T4 Cuerpo";
                case V5AdaptationTier.T5Apex: return "T5 Apex";
                default: return tier.ToString();
            }
        }

        private Color TierColor(V5AdaptationTier tier)
        {
            switch (tier)
            {
                case V5AdaptationTier.T1Prokaryote: return new Color(0.42f, 0.74f, 0.88f, 1f);
                case V5AdaptationTier.T2Eukaryogenesis: return new Color(0.78f, 0.72f, 0.96f, 1f);
                case V5AdaptationTier.T3Specialization: return new Color(0.92f, 0.72f, 0.54f, 1f);
                case V5AdaptationTier.T4ColonialBody: return new Color(0.72f, 0.92f, 0.58f, 1f);
                case V5AdaptationTier.T5Apex: return new Color(1f, 0.88f, 0.36f, 1f);
                default: return Color.white;
            }
        }

        private void DrawSelectionFrame(Rect rect, Color color)
        {
            DrawSolidRect(new Rect(rect.x, rect.y, rect.width, 2f), color);
            DrawSolidRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), color);
            DrawSolidRect(new Rect(rect.x, rect.y, 2f, rect.height), color);
            DrawSolidRect(new Rect(rect.xMax - 2f, rect.y, 2f, rect.height), color);
        }

        private void DrawSolidRect(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box);
            box.alignment = TextAnchor.UpperLeft;
            box.normal.textColor = Color.white;
            title = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, wordWrap = true };
            title.normal.textColor = Color.white;
            small = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
            small.normal.textColor = new Color(0.86f, 0.94f, 0.96f, 1f);
            node = new GUIStyle(GUI.skin.button) { fontSize = 10, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            nodeSmall = new GUIStyle(GUI.skin.button) { fontSize = 10, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            nodeLocked = new GUIStyle(node);
            nodeLocked.normal.textColor = new Color(0.72f, 0.75f, 0.76f, 1f);
            nodeInstalled = new GUIStyle(node);
            nodeInstalled.normal.textColor = new Color(0.05f, 0.22f, 0.08f, 1f);
            laneHeader = new GUIStyle(GUI.skin.box) { fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            laneHeader.normal.textColor = Color.white;
        }
    }
}
