using UnityEngine;

namespace Protogenesis.Archetypes
{
    /// <summary>
    /// Definición de un arquetipo emergente (ScriptableObject).
    /// Crea assets desde: Assets > Create > Protogenesis > Archetype Definition
    /// </summary>
    [CreateAssetMenu(fileName = "Archetype_New", menuName = "Protogenesis/Archetype Definition", order = 20)]
    public class ArchetypeDefinition : ScriptableObject
    {
        [Header("Identidad")]
        public ArchetypeType archetypeType;
        public string        displayName;
        [TextArea(2, 4)]
        public string        description;
        public Sprite        portrait;
        public Color         themeColor = Color.white;

        [Header("Condiciones de activación (score > 50 = activo)")]
        [Tooltip("Producción neta mínima de ATP/s para puntuar puntos por este arquetipo.")]
        public float minATPNetRate       = 0f;
        [Tooltip("Producción neta máxima de ATP/s (más alto = penalización).")]
        public float maxATPNetRate       = 999f;
        [Tooltip("Requiere que la fermentación esté activa.")]
        public bool  requiresFermentation = false;
        [Tooltip("Número mínimo de aliados en radio 8 para puntuar.")]
        public int   minAlliesNearby     = 0;
        [Tooltip("Muertes de enemigos en los últimos 30s para puntuar como agresivo.")]
        public int   minRecentKills      = 0;
        [Tooltip("Puntaje extra si el O2 está por debajo del 0.2 y el jugador sigue vivo.")]
        public bool  scoreIfLowO2        = false;

        [Header("Bonos pasivos (se aplican vía ArchetypeAbilityUnlocker)")]
        [Tooltip("Multiplicador adicional de daño. 0.3 = +30%.")]
        public float damageBonus         = 0f;
        [Tooltip("Multiplicador adicional de velocidad de movimiento.")]
        public float moveSpeedBonus      = 0f;
        [Tooltip("HP máximo adicional.")]
        public float maxHPBonus          = 0f;
        [Tooltip("Multiplicador adicional de producción de ATP.")]
        public float atpProductionBonus  = 0f;
        [Tooltip("Reducción de daño recibido (0-1).")]
        public float damageReduction     = 0f;
        [Tooltip("HP/s de regeneración adicional.")]
        public float hpRegenBonus        = 0f;

        [Header("Habilidad única")]
        [Tooltip("ID de la habilidad especial que se desbloquea con este arquetipo.")]
        public string uniqueAbilityId;
    }
}
