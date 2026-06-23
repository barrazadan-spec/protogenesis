using System;
using UnityEngine;
using Protogenesis.Views;
using Protogenesis.Progression;

namespace Protogenesis.Core
{
    /// <summary>
    /// Bus de eventos global (Observer Pattern). Todos los sistemas del juego
    /// se comunican a través de aquí sin acoplamiento directo entre sí.
    ///
    /// Uso:
    ///   Suscribirse:    EventBus.OnATPChanged += MiMetodo;
    ///   Desuscribirse:  EventBus.OnATPChanged -= MiMetodo;
    ///   Disparar:       EventBus.TriggerATPChanged(nuevoValor, delta);
    /// </summary>
    public static class EventBus
    {
        // ══════════════════════════════════════════════════════════════════════════
        // RECURSOS
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Se dispara cada vez que el ATP cambia. (valorActual, delta)</summary>
        public static event Action<float, float> OnATPChanged;

        /// <summary>ATP bajó del umbral crítico (menos del 20% del máximo).</summary>
        public static event Action OnATPCritical;

        /// <summary>ATP recuperado por encima del umbral crítico.</summary>
        public static event Action OnATPRecovered;

        /// <summary>Un recurso cualquiera cambió. (tipo, valorActual, delta)</summary>
        public static event Action<ResourceType, float, float> OnResourceChanged;

        /// <summary>Un recurso llegó a cero. (tipo)</summary>
        public static event Action<ResourceType> OnResourceDepleted;

        /// <summary>Un recurso bajó del umbral crítico (20% de su máximo). (tipo)</summary>
        public static event Action<ResourceType> OnResourceCritical;

        /// <summary>Un recurso se recuperó por encima del umbral crítico. (tipo)</summary>
        public static event Action<ResourceType> OnResourceRecovered;

        public static void TriggerResourceCritical(ResourceType type)  => OnResourceCritical?.Invoke(type);
        public static void TriggerResourceRecovered(ResourceType type) => OnResourceRecovered?.Invoke(type);

        public static void TriggerATPChanged(float current, float delta)
            => OnATPChanged?.Invoke(current, delta);

        public static void TriggerATPCritical()
            => OnATPCritical?.Invoke();

        public static void TriggerATPRecovered()
            => OnATPRecovered?.Invoke();

        public static void TriggerResourceChanged(ResourceType type, float current, float delta)
            => OnResourceChanged?.Invoke(type, current, delta);

        public static void TriggerResourceDepleted(ResourceType type)
            => OnResourceDepleted?.Invoke(type);

        // ══════════════════════════════════════════════════════════════════════════
        // APOPTOSIS & GAME OVER
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Se inicia la cuenta regresiva de apoptosis. (duracionSegundos)</summary>
        public static event Action<float> OnApoptosisStart;

        /// <summary>La apoptosis terminó. (sobrevivio: true = recuperó, false = murió)</summary>
        public static event Action<bool> OnApoptosisEnd;

        /// <summary>Game Over definitivo.</summary>
        public static event Action OnGameOver;

        public static void TriggerApoptosisStart(float duration) => OnApoptosisStart?.Invoke(duration);
        public static void TriggerApoptosisEnd(bool survived)    => OnApoptosisEnd?.Invoke(survived);
        public static void TriggerGameOver()                      => OnGameOver?.Invoke();

        // ══════════════════════════════════════════════════════════════════════════
        // ERAS EVOLUTIVAS
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>La célula avanzó de era. (eraAnterior, eraNueva)</summary>
        public static event Action<int, int> OnEraChanged;

        public static void TriggerEraChanged(int previous, int next) => OnEraChanged?.Invoke(previous, next);

        // ══════════════════════════════════════════════════════════════════════════
        // ORGÁNULOS
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Un orgánulo recibió daño. (gameObject, hpActual, hpMax)</summary>
        public static event Action<GameObject, float, float> OnOrganelleDamaged;

        /// <summary>Un orgánulo fue destruido. (gameObject, tipo)</summary>
        public static event Action<GameObject, string> OnOrganelleDestroyed;

        /// <summary>Un orgánulo fue construido. (gameObject, tipo)</summary>
        public static event Action<GameObject, string> OnOrganelleBuilt;

        /// <summary>Un orgánulo subió de nivel. (gameObject, nivelNuevo)</summary>
        public static event Action<GameObject, int> OnOrganelleUpgraded;

        public static void TriggerOrganelleDamaged(GameObject go, float hp, float maxHp)
            => OnOrganelleDamaged?.Invoke(go, hp, maxHp);

        public static void TriggerOrganelleDestroyed(GameObject go, string type)
            => OnOrganelleDestroyed?.Invoke(go, type);

        public static void TriggerOrganelleBuilt(GameObject go, string type)
            => OnOrganelleBuilt?.Invoke(go, type);

        public static void TriggerOrganelleUpgraded(GameObject go, int newLevel)
            => OnOrganelleUpgraded?.Invoke(go, newLevel);

        // ══════════════════════════════════════════════════════════════════════════
        // UNIDADES
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Una unidad fue spawneada. (gameObject, tipo)</summary>
        public static event Action<GameObject, string> OnUnitSpawned;

        /// <summary>Una unidad murió. (gameObject, tipo)</summary>
        public static event Action<GameObject, string> OnUnitDied;

        /// <summary>
        /// Quorum Sensing: N unidades del mismo tipo están agrupadas en un radio.
        /// CANON BIOLÓGICO — las bacterias reales coordinan comportamientos colectivos
        /// mediante moléculas de señalización (AHL) cuando superan un umbral poblacional.
        /// (tipo, cantidad, posicionCentro)
        /// </summary>
        public static event Action<string, int, Vector2> OnQuorumReached;

        /// <summary>El quorum de un tipo de unidad se rompió. (tipo)</summary>
        public static event Action<string> OnQuorumLost;

        public static void TriggerUnitSpawned(GameObject go, string type) => OnUnitSpawned?.Invoke(go, type);
        public static void TriggerUnitDied(GameObject go, string type)    => OnUnitDied?.Invoke(go, type);

        public static void TriggerQuorumReached(string unitType, int count, Vector2 center)
            => OnQuorumReached?.Invoke(unitType, count, center);

        public static void TriggerQuorumLost(string unitType)
            => OnQuorumLost?.Invoke(unitType);

        // ══════════════════════════════════════════════════════════════════════════
        // OLEADAS
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Una oleada está a punto de comenzar. (numeroOleada, segundosRestantes)</summary>
        public static event Action<int, float> OnWaveWarning;

        /// <summary>Una oleada comenzó. (numeroOleada)</summary>
        public static event Action<int> OnWaveStart;

        /// <summary>Todas las unidades de la oleada fueron eliminadas. (numeroOleada)</summary>
        public static event Action<int> OnWaveEnd;

        /// <summary>Un boss fue spawneado. (gameObject, nombreBoss)</summary>
        public static event Action<GameObject, string> OnBossSpawn;

        /// <summary>Un boss fue derrotado. (nombreBoss, indiceOleada)</summary>
        public static event Action<string, int> OnBossDefeated;

        public static void TriggerWaveWarning(int wave, float seconds) => OnWaveWarning?.Invoke(wave, seconds);
        public static void TriggerWaveStart(int wave)                   => OnWaveStart?.Invoke(wave);
        public static void TriggerWaveEnd(int wave)                     => OnWaveEnd?.Invoke(wave);
        public static void TriggerBossSpawn(GameObject go, string name) => OnBossSpawn?.Invoke(go, name);
        public static void TriggerBossDefeated(string name, int wave)   => OnBossDefeated?.Invoke(name, wave);

        // ══════════════════════════════════════════════════════════════════════════
        // AMBIENTE
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>El pH del entorno cambió. (valorNuevo)</summary>
        public static event Action<float> OnPHChanged;

        /// <summary>El O2 ambiental cambió. (porcentaje 0-1)</summary>
        public static event Action<float> OnOxygenChanged;

        /// <summary>La temperatura cambió. (valorNuevo 0-100)</summary>
        public static event Action<float> OnTemperatureChanged;

        /// <summary>Condición ambiental crítica detectada. (tipo, valorActual)</summary>
        public static event Action<EnvironmentalHazard, float> OnEnvironmentalHazard;

        public static void TriggerPHChanged(float ph)           => OnPHChanged?.Invoke(ph);
        public static void TriggerOxygenChanged(float o2)       => OnOxygenChanged?.Invoke(o2);
        public static void TriggerTemperatureChanged(float temp) => OnTemperatureChanged?.Invoke(temp);

        public static void TriggerEnvironmentalHazard(EnvironmentalHazard hazard, float value)
            => OnEnvironmentalHazard?.Invoke(hazard, value);

        // ══════════════════════════════════════════════════════════════════════════
        // INFECCIONES
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Un orgánulo fue infectado por un bacteriófago. (gameObject orgánulo)</summary>
        public static event Action<GameObject> OnOrganelleInfected;

        /// <summary>Una infección fue eliminada. (gameObject orgánulo)</summary>
        public static event Action<GameObject> OnInfectionCleared;

        public static void TriggerOrganelleInfected(GameObject go) => OnOrganelleInfected?.Invoke(go);
        public static void TriggerInfectionCleared(GameObject go)  => OnInfectionCleared?.Invoke(go);

        // ══════════════════════════════════════════════════════════════════════════
        // PROGRESIÓN
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Un nodo del árbol de mejoras fue desbloqueado. (nodeID)</summary>
        public static event Action<string> OnUpgradeUnlocked;

        /// <summary>Un Gen permanente fue desbloqueado. (nombreGen)</summary>
        public static event Action<string> OnGeneUnlocked;

        /// <summary>Un nodo fue encolado para investigación. (nodeId, displayName)</summary>
        public static event Action<string, string> OnResearchQueued;

        /// <summary>La investigación de un nodo comenzó. (nodeId, displayName, duracionSegundos)</summary>
        public static event Action<string, string, float> OnResearchStarted;

        /// <summary>La investigación de un nodo completó. (nodeId, displayName)</summary>
        public static event Action<string, string> OnResearchCompleted;

        /// <summary>
        /// Deriva Genética: una línea de unidades sufrió mutación beneficiosa.
        /// CANON BIOLÓGICO — mutaciones aleatorias por acumulación de replicaciones.
        /// (lineaje, tipoBono, valor)
        /// </summary>
        public static event Action<string, string, float> OnGeneticDrift;

        public static void TriggerUpgradeUnlocked(string nodeId) => OnUpgradeUnlocked?.Invoke(nodeId);
        public static void TriggerGeneUnlocked(string geneName)  => OnGeneUnlocked?.Invoke(geneName);

        public static void TriggerResearchQueued(string nodeId, string name)
            => OnResearchQueued?.Invoke(nodeId, name);

        public static void TriggerResearchStarted(string nodeId, string name, float duration)
            => OnResearchStarted?.Invoke(nodeId, name, duration);

        public static void TriggerResearchCompleted(string nodeId, string name)
            => OnResearchCompleted?.Invoke(nodeId, name);

        public static void TriggerGeneticDrift(string lineage, string bonusType, float value)
            => OnGeneticDrift?.Invoke(lineage, bonusType, value);

        // ══════════════════════════════════════════════════════════════════════════
        // SLOTS (Plasticidad Fenotípica — GDD v2)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Un slot fue instalado. (slotType, slotIndex, effectId)</summary>
        public static event Action<Slots.SlotType, int, string> OnSlotInstalled;

        /// <summary>Un slot fue desinstalado. (slotType, slotIndex)</summary>
        public static event Action<Slots.SlotType, int> OnSlotUninstalled;

        /// <summary>Un efecto de slot fue activado. (effectId, valor)</summary>
        public static event Action<string, float> OnSlotEffectActivated;

        public static void TriggerSlotInstalled(Slots.SlotType t, int idx, string effectId)
            => OnSlotInstalled?.Invoke(t, idx, effectId);
        public static void TriggerSlotUninstalled(Slots.SlotType t, int idx)
            => OnSlotUninstalled?.Invoke(t, idx);
        public static void TriggerSlotEffectActivated(string effectId, float value)
            => OnSlotEffectActivated?.Invoke(effectId, value);

        // ══════════════════════════════════════════════════════════════════════════
        // ARQUETIPOS (8 Arquetipos Emergentes — GDD v2)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>El arquetipo activo cambió. (anterior, nuevo)</summary>
        public static event Action<Archetypes.ArchetypeType, Archetypes.ArchetypeType> OnArchetypeChanged;

        // ── Fenotipo (GDD v4.6 — reemplaza arquetipo como sistema central) ─────────
        // TODO: Primordia — OnPhenotypeChanged deprecado. En Primordia usar OnSpecializationConsolidated (Fase 4.2).

        // ── Metabolismo v4.6 ──────────────────────────────────────────────────────
        public static event Action<string> OnMetabolicRouteChanged;
        public static void TriggerMetabolicRouteChanged(string routeName)
            => OnMetabolicRouteChanged?.Invoke(routeName);

        // ── Balance / Presión sistémica v4.6 ─────────────────────────────────────
        public static event Action OnPositiveDominanceActivated;
        public static event Action OnPositiveDominanceDeactivated;
        public static void TriggerPositiveDominanceActivated()   => OnPositiveDominanceActivated?.Invoke();
        public static void TriggerPositiveDominanceDeactivated() => OnPositiveDominanceDeactivated?.Invoke();

        // ── Árbol genético v4.6 ───────────────────────────────────────────────────
        public static event Action<int> OnNucleoInstalled;
        public static void TriggerNucleoInstalled(int tier) => OnNucleoInstalled?.Invoke(tier);


        /// <summary>Una habilidad de arquetipo fue desbloqueada. (archetype, abilityId)</summary>
        public static event Action<Archetypes.ArchetypeType, string> OnArchetypeAbilityUnlocked;

        public static void TriggerArchetypeChanged(Archetypes.ArchetypeType prev, Archetypes.ArchetypeType next)
            => OnArchetypeChanged?.Invoke(prev, next);
        public static void TriggerArchetypeAbilityUnlocked(Archetypes.ArchetypeType archetype, string abilityId)
            => OnArchetypeAbilityUnlocked?.Invoke(archetype, abilityId);

        // ══════════════════════════════════════════════════════════════════════════
        // ECOSISTEMA DINÁMICO (Lotka-Volterra — GDD v2)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>La población de una especie cambió. (speciesId, poblacion, delta)</summary>
        public static event Action<string, float, float> OnPopulationChanged;

        /// <summary>Un evento ecológico ocurrió. (eventId, descripción)</summary>
        public static event Action<string, string> OnEcosystemEvent;

        /// <summary>El jugador entró en una zona de bioma. (biomeId)</summary>
        public static event Action<string> OnBiomeEntered;

        public static void TriggerPopulationChanged(string speciesId, float population, float delta)
            => OnPopulationChanged?.Invoke(speciesId, population, delta);
        public static void TriggerEcosystemEvent(string eventId, string description)
            => OnEcosystemEvent?.Invoke(eventId, description);
        public static void TriggerBiomeEntered(string biomeId)
            => OnBiomeEntered?.Invoke(biomeId);

        // ══════════════════════════════════════════════════════════════════════════
        // VICTORIA Y CAMPAÑA (7 condiciones — GDD v2)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Progreso de una condición de victoria. (conditionId, progreso, objetivo)</summary>
        public static event Action<string, float, float> OnVictoryConditionProgress;

        /// <summary>Condición de victoria alcanzada. (conditionId)</summary>
        public static event Action<string> OnVictoryAchieved;

        /// <summary>Misión de campaña iniciada. (missionId, displayName)</summary>
        public static event Action<string, string> OnMissionStarted;

        /// <summary>Misión completada. (missionId)</summary>
        public static event Action<string> OnMissionCompleted;

        /// <summary>Misión fallida. (missionId, razón)</summary>
        public static event Action<string, string> OnMissionFailed;

        public static void TriggerVictoryConditionProgress(string id, float progress, float target)
            => OnVictoryConditionProgress?.Invoke(id, progress, target);
        public static void TriggerVictoryAchieved(string conditionId)
            => OnVictoryAchieved?.Invoke(conditionId);
        public static void TriggerMissionStarted(string id, string name)
            => OnMissionStarted?.Invoke(id, name);
        public static void TriggerMissionCompleted(string missionId)
            => OnMissionCompleted?.Invoke(missionId);
        public static void TriggerMissionFailed(string id, string reason)
            => OnMissionFailed?.Invoke(id, reason);

        // ══════════════════════════════════════════════════════════════════════════
        // PRIMORDIA — ESTRÉS CELULAR (Prompt 0.4)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>El nivel de estrés celular cambió de umbral. (anterior, nuevo)</summary>
        public static event Action<StressLevel, StressLevel> OnStressLevelChanged;

        /// <summary>El valor numérico de estrés cambió. (valor 0-100, delta)</summary>
        public static event Action<float, float> OnStressChanged;

        public static void TriggerStressLevelChanged(StressLevel previous, StressLevel next)
            => OnStressLevelChanged?.Invoke(previous, next);
        public static void TriggerStressChanged(float value, float delta)
            => OnStressChanged?.Invoke(value, delta);

        // ══════════════════════════════════════════════════════════════════════════
        // PRIMORDIA — VISTA DUAL (Prompt 1.1)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>La vista activa cambió entre Interior y Exterior. (vistaAnterior, vistaNueva)</summary>
        public static event Action<ViewType, ViewType> OnViewChanged;

        public static void TriggerViewChanged(ViewType previous, ViewType next)
            => OnViewChanged?.Invoke(previous, next);

        // ══════════════════════════════════════════════════════════════════════════
        // PRIMORDIA — ORGÁNULOS (Prompt 2.x)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Un orgánulo fue instalado en un slot. (slotIndex, organelleId)</summary>
        public static event Action<int, string> OnOrganelleInstalled;

        /// <summary>Un orgánulo fue retirado de un slot. (slotIndex, organelleId)</summary>
        public static event Action<int, string> OnOrganelleRemoved;

        public static void TriggerOrganelleInstalled(int slotIndex, string organelleId)
            => OnOrganelleInstalled?.Invoke(slotIndex, organelleId);

        public static void TriggerOrganelleRemoved(int slotIndex, string organelleId)
            => OnOrganelleRemoved?.Invoke(slotIndex, organelleId);

        // ══════════════════════════════════════════════════════════════════════════
        // PRIMORDIA — ESPECIALIZACIÓN (Prompt 4.x)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// La especialización alcanzó el 60% y quedó consolidada permanentemente.
        /// (type, displayName)
        /// </summary>
        public static event Action<SpecializationType, string> OnSpecializationConsolidated;

        /// <summary>
        /// El progreso de una especialización cambió. (type, progreso 0-100)
        /// </summary>
        public static event Action<SpecializationType, float> OnSpecializationProgress;

        public static void TriggerSpecializationConsolidated(SpecializationType type, string displayName)
            => OnSpecializationConsolidated?.Invoke(type, displayName);

        public static void TriggerSpecializationProgress(SpecializationType type, float progress)
            => OnSpecializationProgress?.Invoke(type, progress);

        // ══════════════════════════════════════════════════════════════════════════
        // PRIMORDIA — DESAFÍO 1v1 (Prompt 5.1)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Un encuentro 1v1 fue detectado. (opponentGO, opponentName)</summary>
        public static event Action<GameObject, string> OnChallengeDetected;

        /// <summary>El desafío comenzó (ambas partes aceptaron o es vs IA). (opponentName)</summary>
        public static event Action<string> OnChallengeStarted;

        /// <summary>El desafío terminó. (result: Victory / Defeat / Draw / Fled)</summary>
        public static event Action<ChallengeResult> OnChallengeEnded;

        /// <summary>El jugador huyó del desafío antes de resolverse.</summary>
        public static event Action OnChallengeFled;

        public static void TriggerChallengeDetected(GameObject opponent, string name)
            => OnChallengeDetected?.Invoke(opponent, name);
        public static void TriggerChallengeStarted(string opponentName)
            => OnChallengeStarted?.Invoke(opponentName);
        public static void TriggerChallengeEnded(ChallengeResult result)
            => OnChallengeEnded?.Invoke(result);
        public static void TriggerChallengeFled()
            => OnChallengeFled?.Invoke();

        // ══════════════════════════════════════════════════════════════════════════
        // PRIMORDIA — SISTEMA LEGADO (Prompt 5.x)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Una unidad pasó a modo legado (autónomo permanente).
        /// Se dispara al momento de la consolidación de especialización.
        /// (gameObject, unitType)
        /// </summary>
        public static event Action<GameObject, string> OnLegacyTriggered;

        public static void TriggerLegacyTriggered(GameObject go, string unitType)
            => OnLegacyTriggered?.Invoke(go, unitType);

        // ══════════════════════════════════════════════════════════════════════════
        // PRIMORDIA — MUERTE CELULAR Y VICTORIA (Prompt 6.x)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>La célula jugador murió (apoptosis consumada). Distinto de OnGameOver (game over de sesión).</summary>
        public static event Action OnCellDeath;

        /// <summary>Condición de victoria de Primordia alcanzada. (conditionId)</summary>
        public static event Action<string> OnVictory;

        public static void TriggerCellDeath()                 => OnCellDeath?.Invoke();
        public static void TriggerVictory(string conditionId) => OnVictory?.Invoke(conditionId);

        // ══════════════════════════════════════════════════════════════════════════
        // MODO MICROSCOPIO (Estética óptica — GDD v2)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>El modo microscopio fue activado/desactivado.</summary>
        public static event Action<bool> OnMicroscopeModeChanged;

        /// <summary>El nivel de zoom del microscopio cambió. (zoom 0.5x-40x)</summary>
        public static event Action<float> OnZoomLevelChanged;

        public static void TriggerMicroscopeModeChanged(bool enabled)
            => OnMicroscopeModeChanged?.Invoke(enabled);
        public static void TriggerZoomLevelChanged(float zoom)
            => OnZoomLevelChanged?.Invoke(zoom);

        // ══════════════════════════════════════════════════════════════════════════
        // GDD v3 — ESPECIALIZACIÓN (score continuo)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>El puntaje de una especialización cambió. (type, scoreActual)</summary>
        public static event Action<SpecializationType, float> OnSpecializationScoreChanged;
        public static void TriggerSpecializationScoreChanged(SpecializationType type, float score)
            => OnSpecializationScoreChanged?.Invoke(type, score);

        // ══════════════════════════════════════════════════════════════════════════
        // GDD v3 — MEMBRANA POR SEGMENTOS (MembraneSegmentSystem)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Un segmento de membrana recibió daño. (segIndex, hpActual, hpMax)</summary>
        public static event Action<int, float, float> OnSegmentDamaged;

        /// <summary>Un segmento de membrana llegó a 0 HP. (segIndex)</summary>
        public static event Action<int> OnSegmentBroken;

        /// <summary>Un segmento de membrana fue reparado. (segIndex, hpRestored)</summary>
        public static event Action<int, float> OnSegmentRepaired;

        public static void TriggerSegmentDamaged(int i, float hp, float max)
            => OnSegmentDamaged?.Invoke(i, hp, max);
        public static void TriggerSegmentBroken(int i)
            => OnSegmentBroken?.Invoke(i);
        public static void TriggerSegmentRepaired(int i, float hp)
            => OnSegmentRepaired?.Invoke(i, hp);

        // ══════════════════════════════════════════════════════════════════════════
        // GDD v3 — FAGOCITOSIS (FagocytosisSystem)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Inició la secuencia de fagocitosis sobre un objetivo. (target)</summary>
        public static event Action<GameObject> OnFagocytosisStart;

        /// <summary>Fagocitosis completada. Los recursos del objetivo fueron absorbidos. (target, resourcesGained)</summary>
        public static event Action<GameObject, float> OnFagocytosisComplete;

        public static void TriggerFagocytosisStart(GameObject target)
            => OnFagocytosisStart?.Invoke(target);
        public static void TriggerFagocytosisComplete(GameObject target, float res)
            => OnFagocytosisComplete?.Invoke(target, res);

        // ══════════════════════════════════════════════════════════════════════════
        // GDD v3 — GRADIENTES QUÍMICOS (ChemicalGradientSystem)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Un agente químico fue emitido al entorno. (agentType, posición, intensidad)</summary>
        public static event Action<string, Vector2, float> OnChemicalGradientEmitted;

        public static void TriggerChemicalGradientEmitted(string type, Vector2 pos, float intensity)
            => OnChemicalGradientEmitted?.Invoke(type, pos, intensity);

        // ══════════════════════════════════════════════════════════════════════════
        // GDD v3 — VICTORIA / DERROTA DEL MINI-JUEGO
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>La partida del mini-juego terminó. (true = jugador ganó)</summary>
        public static event Action<bool> OnMatchEnd;

        /// <summary>
        /// La célula murió con diagnóstico de tipo. Complementa el OnCellDeath sin parámetro.
        /// (deathType: "Lysis" | "MetabolicCollapse" | "Intoxication" | "Apoptosis" | "Freezing")
        /// </summary>
        public static event Action<string> OnCellDeathTyped;

        /// <summary>
        /// El Legado fue activado — N unidades convertidas a autónomas. (unitCount)
        /// Complementa el OnLegacyTriggered(GameObject, string) existente.
        /// </summary>
        public static event Action<int> OnLegacyActivated;

        public static void TriggerMatchEnd(bool playerWon)
            => OnMatchEnd?.Invoke(playerWon);
        public static void TriggerCellDeath(string deathType)
            => OnCellDeathTyped?.Invoke(deathType);
        public static void TriggerLegacyActivated(int unitCount)
            => OnLegacyActivated?.Invoke(unitCount);

        // ══════════════════════════════════════════════════════════════════════════
        // GDD v4.2 — DIVISIÓN CELULAR (CellDivisionSystem / ReabsorptionSystem)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>División completada. (madre, hija recién creada)</summary>
        public static event Action<GameObject, GameObject> OnCellDivided;

        /// <summary>Hija reabsorbida por R. (célula reabsorbida)</summary>
        public static event Action<GameObject> OnCellReabsorbed;

        public static void TriggerCellDivided(GameObject mother, GameObject daughter)
            => OnCellDivided?.Invoke(mother, daughter);
        public static void TriggerCellReabsorbed(GameObject daughter)
            => OnCellReabsorbed?.Invoke(daughter);

        // ══════════════════════════════════════════════════════════════════════════
        // GDD v4.2 — DIRECTIVAS (DirectiveSystem)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Directiva asignada a una hija. (célula, directiva)</summary>
        public static event Action<GameObject, DirectiveType> OnDirectiveChanged;

        public static void TriggerDirectiveChanged(GameObject cell, DirectiveType directive)
            => OnDirectiveChanged?.Invoke(cell, directive);

        // ══════════════════════════════════════════════════════════════════════════
        // GDD v4.2 — GENES (GeneticRingSystem)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Gen activado en un anillo. (anillo, índice del gen 0-3)</summary>
        public static event Action<GeneticRing, int> OnGeneActivated;

        public static void TriggerGeneActivated(GeneticRing ring, int geneIndex)
            => OnGeneActivated?.Invoke(ring, geneIndex);

        // ══════════════════════════════════════════════════════════════════════════
        // GDD v4.2 — ESTADOS VISUALES (CellStateVisuals)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Estado visual de una célula cambió. (célula, estado)</summary>
        public static event Action<GameObject, CellVisualState> OnCellVisualStateChanged;

        public static void TriggerCellVisualStateChanged(GameObject cell, CellVisualState state)
            => OnCellVisualStateChanged?.Invoke(cell, state);

        // ══════════════════════════════════════════════════════════════════════════
        // GDD v4.2 — MUERTE Y POWER MOMENTS
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>Célula madre eliminada con diagnóstico tipado. (deathType)</summary>
        public static event Action<DeathType> OnMotherDied;

        /// <summary>Momento cinematográfico clave. (type: "consolidation" | "premium" | "elimination")</summary>
        public static event Action<string> OnPowerMoment;

        public static void TriggerMotherDied(DeathType deathType)
            => OnMotherDied?.Invoke(deathType);
        public static void TriggerPowerMoment(string type)
            => OnPowerMoment?.Invoke(type);

        // ══════════════════════════════════════════════════════════════════════════
        // UTILIDADES
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Desuscribe todos los listeners de todos los eventos.
        /// Llamar al cambiar de escena para evitar memory leaks.
        /// </summary>
        public static void ClearAllListeners()
        {
            OnATPChanged          = null;
            OnATPCritical         = null;
            OnATPRecovered        = null;
            OnResourceChanged     = null;
            OnResourceDepleted    = null;
            OnResourceCritical    = null;
            OnResourceRecovered   = null;
            OnApoptosisStart      = null;
            OnApoptosisEnd        = null;
            OnGameOver            = null;
            OnEraChanged          = null;
            OnOrganelleDamaged    = null;
            OnOrganelleDestroyed  = null;
            OnOrganelleBuilt      = null;
            OnOrganelleUpgraded   = null;
            OnUnitSpawned         = null;
            OnUnitDied            = null;
            OnQuorumReached       = null;
            OnQuorumLost          = null;
            OnWaveWarning         = null;
            OnWaveStart           = null;
            OnWaveEnd             = null;
            OnBossSpawn           = null;
            OnBossDefeated        = null;
            OnPHChanged           = null;
            OnOxygenChanged       = null;
            OnTemperatureChanged  = null;
            OnEnvironmentalHazard = null;
            OnOrganelleInfected   = null;
            OnInfectionCleared    = null;
            OnUpgradeUnlocked           = null;
            OnGeneUnlocked              = null;
            OnResearchQueued            = null;
            OnResearchStarted           = null;
            OnResearchCompleted         = null;
            OnGeneticDrift              = null;
            OnSlotInstalled             = null;
            OnSlotUninstalled           = null;
            OnSlotEffectActivated       = null;
            OnArchetypeChanged          = null;
            OnArchetypeAbilityUnlocked  = null;
            // OnPhenotypeChanged — removed in Primordia migration
            OnMetabolicRouteChanged     = null;
            OnPositiveDominanceActivated   = null;
            OnPositiveDominanceDeactivated = null;
            OnNucleoInstalled           = null;
            OnPopulationChanged         = null;
            OnEcosystemEvent            = null;
            OnBiomeEntered              = null;
            OnVictoryConditionProgress  = null;
            OnVictoryAchieved           = null;
            OnMissionStarted            = null;
            OnMissionCompleted          = null;
            OnMissionFailed             = null;
            OnMicroscopeModeChanged     = null;
            OnZoomLevelChanged          = null;
            // Primordia events
            OnStressLevelChanged           = null;
            OnStressChanged                = null;
            OnViewChanged                  = null;
            OnOrganelleInstalled           = null;
            OnOrganelleRemoved             = null;
            OnSpecializationConsolidated   = null;
            OnSpecializationProgress       = null;
            OnChallengeDetected            = null;
            OnChallengeStarted             = null;
            OnChallengeEnded               = null;
            OnChallengeFled                = null;
            OnLegacyTriggered              = null;
            OnCellDeath                    = null;
            OnVictory                      = null;
            // GDD v3
            OnSpecializationScoreChanged   = null;
            OnSegmentDamaged               = null;
            OnSegmentBroken                = null;
            OnSegmentRepaired              = null;
            OnFagocytosisStart             = null;
            OnFagocytosisComplete          = null;
            OnChemicalGradientEmitted      = null;
            OnMatchEnd                     = null;
            OnCellDeathTyped               = null;
            OnLegacyActivated              = null;
            // GDD v4.2
            OnCellDivided                  = null;
            OnCellReabsorbed               = null;
            OnDirectiveChanged             = null;
            OnGeneActivated                = null;
            OnCellVisualStateChanged       = null;
            OnMotherDied                   = null;
            OnPowerMoment                  = null;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // ENUMS DE SOPORTE
    // ══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Tipos de recurso del juego.
    ///
    /// Primordia (GDD v2): ATP, AminoAcids, Nucleotides, Lipids.
    /// Los valores legacy (Glucose…GenomicPoints) se mantienen para compilación;
    /// ResourceManager ya no los inicializa — las operaciones sobre ellos son no-op.
    /// </summary>
    public enum ResourceType
    {
        // ── Primordia (activos) ───────────────────────────────────────────────
        ATP,
        AminoAcids,
        Nucleotides,
        Lipids,

        // ── Legacy v4.6 (compilación only — no-op en runtime) ────────────────
        // TODO: Primordia — eliminar cuando todos los sistemas legacy sean migrados
        Glucose,
        O2,
        Nitrogen,
        Silica,
        Biomass,
        GenomicPoints
    }

    /// <summary>Tipos de peligro ambiental que pueden disparar alertas.</summary>
    public enum EnvironmentalHazard
    {
        AcidicPH,       // pH < 5
        AlkalinePH,     // pH > 9
        LowOxygen,      // O2 < 20%
        HighTemperature,// Temp > 70
        OxidativeStress // ROS acumulados
    }

    /// <summary>
    /// Nivel de estrés celular (Protogenesis: Primordia — GDD v2).
    /// Determinado por la suma ponderada de los 8 factores de estrés activos.
    /// </summary>
    public enum StressLevel
    {
        Calm        = 0,   // 0–20   — operación normal
        Mild        = 1,   // 21–40  — leve penalización de eficiencia
        Moderate    = 2,   // 41–60  — -15% eficiencia, feedback visual
        Critical    = 3,   // 61–80  — -30% eficiencia, advertencia de apoptosis
        Catastrophic = 4   // 81–100 — apoptosis inminente
    }

    /// <summary>Resultado de un encuentro 1v1 (Primordia Prompt 5.1).</summary>
    public enum ChallengeResult
    {
        Victory = 0,  // el jugador ganó
        Defeat  = 1,  // el jugador perdió
        Draw    = 2,  // empate por tiempo o recursos iguales
        Fled    = 3,  // el jugador huyó antes de resolverse
    }

    /// <summary>
    /// Los 8 factores de estrés que StressSystem monitorea (Primordia GDD v2).
    /// Cada factor puede ser activado/desactivado independientemente.
    /// </summary>
    public enum StressFactor
    {
        ATPStarvation,       // ATP < 20% del máximo
        AminoAcidStarvation, // AminoAcids < 20% del máximo
        NucleotideStarvation,// Nucleotides < 20% del máximo
        LipidStarvation,     // Lipids < 20% del máximo
        AcidicEnvironment,   // pH fuera del rango tolerable (< 5.5 ó > 8.5)
        ThermalStress,       // temperatura > 70°C o < 5°C
        HypoxicStress,       // O2 ambiental < 20%
        MechanicalDamage     // daño recibido de enemigos en los últimos 5 s
    }
}
