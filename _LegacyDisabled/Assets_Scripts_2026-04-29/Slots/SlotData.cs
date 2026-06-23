using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Slots
{
    /// <summary>
    /// Definición de un slot instalable (ScriptableObject).
    /// Crea assets desde el menú: Assets > Create > Protogenesis > Slot Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewSlot", menuName = "Protogenesis/Slot Data", order = 10)]
    public class SlotData : ScriptableObject
    {
        [Header("Identificación")]
        public string slotId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Requisitos")]
        public SlotType requiredSlotType;
        [Tooltip("Era mínima para instalar este slot (0 = desde el inicio).")]
        public int requiredEra = 0;

        [Header("Nivel de estructura (GDD v3 §8.2)")]
        [Tooltip("I=1 rudimentario, II=2 especializado, III=3 maestro, IV=4 legendario.")]
        [Range(1, 4)]
        public int structureLevel = 1;

        [Header("Costo de instalación")]
        public ResourceType[] costTypes;
        public float[]        costAmounts;

        [Header("Efectos")]
        [Tooltip("Lista de efectos que se aplican al instalar este slot.")]
        public SlotEffectBase[] effects;

        [Header("Visual")]
        public Color  slotColor = Color.cyan;
        public bool   isPassive = true;

        /// <summary>Verifica si los arrays de costo están bien formados.</summary>
        public bool HasValidCosts =>
            costTypes != null && costAmounts != null &&
            costTypes.Length == costAmounts.Length;
    }
}
