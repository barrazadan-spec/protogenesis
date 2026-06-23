namespace Protogenesis.Core
{
    /// <summary>
    /// Puede recibir daño. Implementado por EnemyBase, UnitBase, OrganelleBase y CAP.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }

    /// <summary>
    /// Puede ser curado. Implementado por UnitBase.
    /// </summary>
    public interface IHealable
    {
        void Heal(float amount);
        void HealToFull();
    }

    /// <summary>
    /// Puede ser aturdido temporalmente. Implementado por UnitBase.
    /// </summary>
    public interface IStunnable
    {
        void Stun(float duration);
    }

    /// <summary>
    /// Puede recibir un plásmido (transferencia horizontal de genes). Implementado por UnitBase.
    /// </summary>
    public interface IGeneReceiver
    {
        void ReceivePlasmid();
    }

    /// <summary>
    /// Puede entrar en modo de esporulación defensiva. Implementado por UnitBase.
    /// </summary>
    public interface ISporulatable
    {
        void EnterSporulationMode(float duration);
    }

    /// <summary>
    /// Puede recibir y perder un buff de daño (tormenta de citoquinas). Implementado por UnitBase.
    /// </summary>
    public interface IDamageBuffReceiver
    {
        void ApplyDamageBuff(float multiplier);
        void RemoveDamageBuff();
    }
}
