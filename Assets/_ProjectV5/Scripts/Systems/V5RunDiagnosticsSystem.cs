using System.Text;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5RunDiagnosticsSystem : MonoBehaviour, IV5RunResettable
    {
        public bool ShowPanel;
        public int Score { get; private set; }
        public int WarningCount { get; private set; }
        public string ShortStatus { get; private set; } = "Esperando run.";
        public string PriorityAdvice { get; private set; } = "Sin diagnostico todavia.";
        public string CoachAdvice { get; private set; } = "La colonia todavia no necesita intervencion.";
        public string CoachAction { get; private set; } = "Sigue el loop principal.";
        public V5AdaptationId CoachAdaptation { get; private set; } = V5AdaptationId.None;
        public string CoachAdaptationLabel { get; private set; } = "Sin adaptacion sugerida.";
        public string CoachAdaptationStatus { get; private set; } = "Sin estado.";
        public string LastReport { get; private set; } = "Sin diagnostico.";

        private float nextScan;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle statusStyle;

        private void Start()
        {
            V5PanelRouter.Register("Diagnostico", () => ShowPanel, value => ShowPanel = value);
            Scan();
        }

        public void ResetForNewRun()
        {
            ShowPanel = false;
            Score = 0;
            WarningCount = 0;
            ShortStatus = "Esperando run.";
            PriorityAdvice = "Sin diagnostico todavia.";
            CoachAdvice = "La colonia todavia no necesita intervencion.";
            CoachAction = "Sigue el loop principal.";
            CoachAdaptation = V5AdaptationId.None;
            CoachAdaptationLabel = "Sin adaptacion sugerida.";
            CoachAdaptationStatus = "Sin estado.";
            LastReport = "Sin diagnostico.";
            nextScan = 0f;
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 2f;
            Scan();
        }

        public void Scan()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null)
            {
                Score = 0;
                WarningCount = 1;
                ShortStatus = "Sin GameManager";
                PriorityAdvice = "La escena no tiene runtime V5 activo.";
                CoachAdvice = "No hay runtime activo.";
                CoachAction = "Carga una escena V5 con bootstrap.";
                CoachAdaptation = V5AdaptationId.None;
                CoachAdaptationLabel = "Sin adaptacion sugerida.";
                CoachAdaptationStatus = "Sin GameManager.";
                LastReport = "No hay GameManager.";
                return;
            }

            int score = 100;
            int warnings = 0;
            StringBuilder sb = new StringBuilder(2048);
            sb.AppendLine("DIAGNOSTICO DE RUN v5.15.6");
            sb.AppendLine("Escenario: " + gm.ScenarioId + " | Fase: " + gm.Phase + " | Tiempo: " + (gm.ElapsedSeconds / 60f).ToString("0.0") + "m");

            string firstAdvice = "";
            bool hasAdaptations = gm.Adaptations != null;
            int activeCount = hasAdaptations ? gm.Adaptations.ActiveCount() : 0;
            int activeCap = hasAdaptations ? gm.Adaptations.ActiveCap : 0;
            float installs = gm.Telemetry != null ? gm.Telemetry.AdaptationsInstalled : activeCount;
            float failures = gm.Telemetry != null ? gm.Telemetry.FailedAdaptationAttempts : 0f;
            float capBlocks = gm.Telemetry != null ? gm.Telemetry.CapBlockedAttempts : 0f;
            float stress = gm.MotherCell != null ? gm.MotherCell.Stats.stress : 100f;
            float toxins = gm.Environment != null ? gm.Environment.AverageToxins() : 0f;
            int playerCells = gm.PlayerCellCount();
            int enemies = gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0;
            bool earlyRun = gm.ElapsedSeconds < 180f;

            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                hasAdaptations,
                "Genoma adaptativo activo",
                "No hay sistema de adaptaciones activo.",
                25,
                "Revisar bootstrap: el arbol de Genoma debe ser la fuente principal.");

            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                earlyRun || activeCount > 0,
                "El jugador ya instalo adaptaciones",
                "Run avanzada sin adaptaciones instaladas.",
                14,
                "El tutorial inicial debe empujar una primera adaptacion obvia y barata.");

            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                activeCap <= 0 || activeCount < activeCap - 1 || activeCount < 6,
                "Cap de adaptaciones respirando",
                "Cap casi lleno: " + activeCount + "/" + activeCap + ".",
                10,
                "Si esto pasa temprano, revisar costos, hitos gratis o cap base.");

            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                failures <= Mathf.Max(3f, installs + 2f),
                "Bloqueos de Genoma aceptables",
                "Demasiados intentos bloqueados: " + failures.ToString("0") + " fallos para " + installs.ToString("0") + " instalaciones.",
                13,
                "El arbol puede estar comunicando mal requisitos o recursos.");

            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                capBlocks < 2f || activeCount >= activeCap - 1,
                "Bloqueos por cap explicables",
                "El jugador choco con el cap antes de llenar una build clara.",
                8,
                "Marcar mejor que rasgos cuentan cap y cuales son hitos.");

            bool routeClear = gm.Identity != null &&
                              gm.Identity.Identity != V5IdentityId.LUCA &&
                              gm.Identity.EvolutionPath != V5EvolutionPath.Uncommitted;
            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                gm.ElapsedSeconds < 240f || routeClear || activeCount < 4,
                "Identidad biologica legible",
                "La colonia sigue sin identidad clara despues de varias adaptaciones.",
                12,
                "Revisar pesos/canon: el jugador necesita entender hacia donde va.");

            bool basicAdhesin = hasAdaptations && gm.Adaptations.Has(V5AdaptationId.BasicAdhesin);
            int bodySlots = gm.Body != null ? gm.Body.OccupiedSlots : 0;
            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                gm.ElapsedSeconds < 240f || !basicAdhesin || bodySlots > 0,
                "Adesion lleva a cuerpo",
                "Adesina instalada pero el cuerpo no se usa.",
                8,
                "Despues de Adesina, el coach debe sugerir adherir una hija.");

            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                gm.ElapsedSeconds < 300f || playerCells >= 3,
                "Poblacion suficiente para RTS",
                "Pocas unidades controlables para la fase actual.",
                9,
                "Revisar produccion germinal, division o costos tempranos.");

            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                stress < 80f,
                "Stress bajo control",
                "Stress de la madre alto: " + stress.ToString("0") + ".",
                10,
                "El jugador necesita una salida clara: reparar, retirar o adaptar homeostasis.");

            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                toxins < 0.72f,
                "Toxinas ambientales jugables",
                "Toxinas promedio peligrosas: " + (toxins * 100f).ToString("0") + "%.",
                8,
                "Si no es intencional, bajar director quimico o mejorar counters tempranos.");

            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                enemies <= playerCells * 4 + 16,
                "Presion enemiga razonable",
                "Demasiados organismos no-jugador para la colonia actual.",
                8,
                "Revisar Director Ecologico: la presion puede estar escalando antes de tiempo.");

            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                gm.Battlefield != null,
                "Living Battlefield visible para debug",
                "No hay recognizer de estados de campo.",
                6,
                "Sin estados de campo, cuesta explicar por que el mapa cambia.");

            V5PlaytestReportSystem report = FindFirstObjectByType<V5PlaytestReportSystem>();
            AddCheck(sb, ref score, ref warnings, ref firstAdvice,
                report != null,
                "Exportador JSON disponible",
                "No hay exportador de reporte JSON.",
                8,
                "Sin reportes, el balance queda a ojo.");

            score = Mathf.Clamp(score, 0, 100);
            Score = score;
            WarningCount = warnings;
            ShortStatus = score >= 86 ? "RUN LEGIBLE" : score >= 70 ? "JUGABLE CON ALERTAS" : score >= 50 ? "REVISAR BALANCE" : "RUN CONFUSA";
            PriorityAdvice = string.IsNullOrEmpty(firstAdvice) ? "La run se ve legible. Seguir observando builds y tiempos." : firstAdvice;
            BuildCoachText(gm, firstAdvice);

            sb.AppendLine("");
            sb.AppendLine("Score: " + Score + "/100 | " + ShortStatus + " | Alertas: " + WarningCount);
            sb.AppendLine("Prioridad: " + PriorityAdvice);
            sb.AppendLine("Coach: " + CoachAdvice + " | " + CoachAction);
            if (CoachAdaptation != V5AdaptationId.None)
                sb.AppendLine("Genoma sugerido: " + CoachAdaptationLabel + " | " + CoachAdaptationStatus);
            if (gm.Telemetry != null)
            {
                sb.AppendLine("Telemetria: " + gm.Telemetry.Summary);
                sb.AppendLine("Genoma: " + gm.Telemetry.AdaptationSummary);
                sb.AppendLine("Fallos: " + gm.Telemetry.FailureSummary);
            }
            if (gm.Identity != null) sb.AppendLine("Identidad: " + gm.Identity.Summary);
            if (gm.Body != null) sb.AppendLine("Cuerpo: " + gm.Body.Summary);

            LastReport = sb.ToString();
        }

        private void BuildCoachText(V5GameManager gm, string advice)
        {
            ClearCoachAdaptation();

            if (string.IsNullOrEmpty(advice))
            {
                CoachAdvice = "La colonia se ve legible.";
                CoachAction = "Sigue expandiendo, especializando o empujando victoria.";
                SuggestIdentityNext(gm);
                return;
            }

            if (advice.Contains("primera adaptacion"))
            {
                CoachAdvice = "Aun falta la primera decision biologica.";
                SuggestAdaptation(gm, PreferredFirstAdaptation(gm));
                CoachAction = GenomeActionOrFallback("Abre Genoma (G) y toma Pared, Flagelo, Tilacoide, Bomba o Adesina basica.");
            }
            else if (advice.Contains("cap"))
            {
                CoachAdvice = "El Genoma esta quedando muy apretado.";
                CoachAction = "Prioriza adaptaciones de identidad y deja utilidades para despues.";
                SuggestIdentityNext(gm);
            }
            else if (advice.Contains("requisitos") || advice.Contains("arbol"))
            {
                CoachAdvice = "Estas chocando mucho con bloqueos del Genoma.";
                SuggestIdentityNext(gm);
                CoachAction = GenomeActionOrFallback("Selecciona un nodo disponible, lee sus requisitos y sigue un camino corto de identidad.");
            }
            else if (advice.Contains("entender hacia donde va") || advice.Contains("pesos/canon"))
            {
                CoachAdvice = "La identidad de la colonia todavia no se lee clara.";
                SuggestIdentityNext(gm);
                CoachAction = GenomeActionOrFallback("Elige una ruta: bacteria, ameba, ciliado, microalga, hongo o moho, y completa su siguiente adaptacion sugerida.");
            }
            else if (advice.Contains("adherir una hija") || advice.Contains("Adesina"))
            {
                CoachAdvice = "Ya tienes adhesina: toca convertirla en cuerpo.";
                CoachAction = "Selecciona una hija cercana y usa A para adherirla a la madre.";
            }
            else if (advice.Contains("produccion") || advice.Contains("division"))
            {
                CoachAdvice = "La colonia necesita mas presencia en el mapa.";
                SuggestAdaptation(gm, V5AdaptationId.RapidDivision);
                CoachAction = GenomeActionOrFallback("Divide la madre o produce una unidad libre desde el laboratorio germinal.");
            }
            else if (advice.Contains("stress") || advice.Contains("homeostasis"))
            {
                CoachAdvice = "La madre esta bajo stress peligroso.";
                SuggestAdaptation(gm, PreferredHomeostasisAdaptation(gm));
                CoachAction = GenomeActionOrFallback("Retirala, repara, baja presion o toma una adaptacion de homeostasis.");
            }
            else if (advice.Contains("toxinas") || advice.Contains("quimico"))
            {
                CoachAdvice = "El campo quimico se esta volviendo hostil.";
                SuggestAdaptation(gm, V5AdaptationId.CatalaseROS);
                CoachAction = GenomeActionOrFallback("Sal de la zona toxica o toma Catalasa/ROS antes de seguir expandiendo.");
            }
            else if (advice.Contains("Director") || advice.Contains("presion"))
            {
                CoachAdvice = "La presion ecologica esta escalando.";
                SuggestAdaptation(gm, PreferredCombatAdaptation(gm));
                CoachAction = GenomeActionOrFallback("Agrupa defensores, elimina amenazas cercanas y evita sobreexpandirte.");
            }
            else
            {
                CoachAdvice = "La run muestra una alerta de legibilidad.";
                CoachAction = "Abre Diagnostico y revisa la prioridad actual.";
                SuggestIdentityNext(gm);
            }
        }

        private void ClearCoachAdaptation()
        {
            CoachAdaptation = V5AdaptationId.None;
            CoachAdaptationLabel = "Sin adaptacion sugerida.";
            CoachAdaptationStatus = "Sin estado.";
        }

        private void SuggestIdentityNext(V5GameManager gm)
        {
            if (gm == null || gm.Identity == null) return;
            SuggestAdaptation(gm, gm.Identity.SuggestedNext);
        }

        private void SuggestAdaptation(V5GameManager gm, V5AdaptationId id)
        {
            ClearCoachAdaptation();
            if (gm == null || gm.Adaptations == null || id == V5AdaptationId.None) return;

            V5AdaptationDefinition def = V5AdaptationLibrary.Get(id);
            if (def == null) return;

            CoachAdaptation = id;
            CoachAdaptationLabel = def.displayName;

            if (gm.Adaptations.Has(id))
            {
                CoachAdaptationStatus = "Ya instalada.";
                return;
            }

            string reason;
            bool canInstall = gm.Adaptations.CanInstall(id, gm.MotherCell, out reason);
            CoachAdaptationStatus = canInstall ? "Lista para instalar." : "Bloqueada: " + reason;
        }

        private string GenomeActionOrFallback(string fallback)
        {
            if (CoachAdaptation == V5AdaptationId.None) return fallback;
            return "Abre Genoma (G) e instala " + CoachAdaptationLabel + ". " + CoachAdaptationStatus;
        }

        private V5AdaptationId PreferredFirstAdaptation(V5GameManager gm)
        {
            if (gm == null || gm.MotherCell == null) return V5AdaptationId.BacterialWall;
            V5EnvironmentGrid env = gm.Environment;
            float light = env != null ? env.Sample(V5OverlayMode.Light, gm.MotherCell.transform.position) : 0.5f;
            float oxygen = env != null ? env.Sample(V5OverlayMode.Oxygen, gm.MotherCell.transform.position) : 0.35f;
            if (light > 0.56f) return V5AdaptationId.ProkaryoticThylakoid;
            if (oxygen < 0.23f) return V5AdaptationId.ProtonPump;
            return V5AdaptationId.BacterialWall;
        }

        private V5AdaptationId PreferredHomeostasisAdaptation(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return V5AdaptationId.CatalaseROS;
            if (!gm.Adaptations.Has(V5AdaptationId.CatalaseROS)) return V5AdaptationId.CatalaseROS;
            if (!gm.Adaptations.Has(V5AdaptationId.ContractileVacuole)) return V5AdaptationId.ContractileVacuole;
            if (!gm.Adaptations.Has(V5AdaptationId.SignalingCommunication)) return V5AdaptationId.SignalingCommunication;
            return V5AdaptationId.None;
        }

        private V5AdaptationId PreferredCombatAdaptation(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return V5AdaptationId.BacterialFlagellum;
            if (!gm.Adaptations.Has(V5AdaptationId.Lysosome)) return V5AdaptationId.Lysosome;
            if (!gm.Adaptations.Has(V5AdaptationId.Pseudopods)) return V5AdaptationId.Pseudopods;
            if (!gm.Adaptations.Has(V5AdaptationId.Cilia)) return V5AdaptationId.Cilia;
            return V5AdaptationId.None;
        }

        private void AddCheck(
            StringBuilder sb,
            ref int score,
            ref int warnings,
            ref string firstAdvice,
            bool ok,
            string okLabel,
            string warnLabel,
            int penalty,
            string advice)
        {
            if (ok)
            {
                sb.AppendLine("[OK] " + okLabel);
                return;
            }

            warnings++;
            score -= penalty;
            sb.AppendLine("[!] " + warnLabel + " (-" + penalty + ")");
            if (string.IsNullOrEmpty(firstAdvice)) firstAdvice = advice;
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect rect = new Rect(18f, 86f, 520f, 520f);
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(rect.x + 14f, rect.y + 12f, rect.width - 28f, rect.height - 24f));
            GUILayout.Label("DIAGNOSTICO DE RUN", titleStyle);
            GUILayout.Label(ShortStatus + " | Score " + Score + "/100 | Alertas " + WarningCount, statusStyle);
            GUILayout.Label("Prioridad: " + PriorityAdvice, bodyStyle);
            GUILayout.Space(8f);
            GUILayout.TextArea(LastReport, bodyStyle, GUILayout.ExpandHeight(true));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Recalcular")) Scan();
            if (GUILayout.Button("Exportar JSON"))
            {
                V5PlaytestReportSystem report = FindFirstObjectByType<V5PlaytestReportSystem>();
                if (report != null) report.ExportReport("diagnostics_panel");
            }
            if (GUILayout.Button("Cerrar")) ShowPanel = false;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 17;
            titleStyle.normal.textColor = new Color(0.88f, 1f, 0.95f, 1f);
            statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.fontStyle = FontStyle.Bold;
            statusStyle.normal.textColor = Score >= 70 ? new Color(0.7f, 1f, 0.72f, 1f) : new Color(1f, 0.78f, 0.45f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.wordWrap = true;
            bodyStyle.normal.textColor = Color.white;
        }
    }
}
