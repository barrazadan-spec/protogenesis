using UnityEngine;

namespace Protogenesis.Progression
{
    /// <summary>
    /// UpgradeNodeData — ScriptableObject que describe un nodo del árbol evolutivo.
    ///
    /// Hay 16 nodos distribuidos en 4 ramas biológicas:
    ///   METABOLISMO  — Mejoras de producción de ATP/NADH/Glucosa
    ///   DEFENSA      — HP, resistencias, HSP y membrana
    ///   PRODUCCIÓN   — Proteínas, ARN, velocidad de ribosomas
    ///   EXPANSIÓN    — Radios de zona, límites de orgánulos, unidades
    ///
    /// Un nodo puede requerir otro nodo previo (prerequisiteNodeId).
    /// El efecto se aplica a través de UpgradeTree.ApplyEffect().
    /// </summary>
    [CreateAssetMenu(menuName = "Protogenesis/Progression/Upgrade Node",
                     fileName = "UpgradeNode_New")]
    public class UpgradeNodeData : ScriptableObject
    {
        // ── Identificación ────────────────────────────────────────────────────────
        [Header("Identificación")]
        public string nodeId;           // Clave única, ej. "META_01"
        public string displayName;      // Nombre visible en UI
        [TextArea(2, 4)]
        public string description;      // Descripción del efecto
        public Sprite icon;

        // ── Rama y posición ───────────────────────────────────────────────────────
        [Header("Árbol")]
        public UpgradeBranch branch;
        public int            column;   // 0-3 dentro de la rama
        public string         prerequisiteNodeId;  // "" = sin requisito

        // ── Requisito de era ──────────────────────────────────────────────────────
        [Header("Requisitos")]
        public int  requiredEra   = 0;
        public float costATP      = 500f;
        public float costRNA      = 10f;
        [Tooltip("Segundos que tarda la investigación.")]
        public float researchTime = 30f;

        // ── Efecto ────────────────────────────────────────────────────────────────
        [Header("Efecto")]
        public UpgradeEffectType effectType;
        [Tooltip("Valor numérico del efecto (porcentaje como 0.15 = +15%, o valor absoluto).")]
        public float             effectValue = 0.15f;
        [Tooltip("Identificador secundario (e.g. tipo de unidad, tipo de recurso).")]
        public string            effectTarget;
    }

    // ── Enums ─────────────────────────────────────────────────────────────────────

    public enum UpgradeBranch
    {
        Metabolism,   // Producción energética
        Defense,      // Supervivencia y resistencia
        Production,   // Biosíntesis (proteínas/ARN)
        Expansion     // Crecimiento y colonización
    }

    public enum UpgradeEffectType
    {
        // METABOLISMO
        ATPProductionBonus,         // +X% ATP global
        NADHProductionBonus,        // +X% NADH global
        FermentationEfficiency,     // +X% eficiencia en fermentación
        OxidativePhosphorylation,   // Mitocondria L3 produce +X NADH extra

        // DEFENSA
        CAPMaxHPBonus,              // +X% HP máximo de la CAP
        OrganelleMaxHPBonus,        // +X% HP máximo de orgánulos
        HSPRadius,                  // +X unidades al radio de HSP
        MembraneResistance,         // -X% daño recibido por invasores en membrana

        // PRODUCCIÓN
        ProteinProductionBonus,     // +X% producción de proteínas
        RNAProductionBonus,         // +X% producción de ARN
        RibosomeQueueCapacity,      // +X slots en cola de Ribosoma
        UnitDamageBonus,            // +X% daño de todas las unidades aliadas

        // EXPANSIÓN
        ZoneRadiusExpansion,        // Expande el radio celular en X unidades
        ExtraOrganelleSlot,         // +1 Mitocondria permitida (MaxAllowed++)
        UnitSpeedBonus,             // +X% velocidad de todas las unidades
        NewUnitType                 // Desbloquea un tipo de unidad extra
    }
}
