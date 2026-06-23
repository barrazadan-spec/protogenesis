namespace Protogenesis.V5
{
    public struct V5BodySlotDefinition
    {
        public int index;
        public V5BodyRing ring;
        public float angleDegrees;
        public float radius;
        public V5BodySlotRole preferredRole;

        public V5BodySlotDefinition(int index, V5BodyRing ring, float angleDegrees, float radius, V5BodySlotRole preferredRole)
        {
            this.index = index;
            this.ring = ring;
            this.angleDegrees = angleDegrees;
            this.radius = radius;
            this.preferredRole = preferredRole;
        }
    }
}
