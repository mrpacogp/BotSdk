namespace Quantum
{
	using Photon.Deterministic;

	[System.Serializable]
  public unsafe partial class UseSpellAction : AIAction
  {
    // [Bot SDK Sample]
    // References an Entity Prototype which is a spell to be used
    // The spell is defined directly on Unity, the spell's target entity is stored on the Blackboard
    // and the spell origin is a fixed position

    public AssetRef<EntityPrototype> Spell;
    public AIBlackboardValueKey TargetEntity;

    private readonly FPVector3 _spellcasterPosition = new FPVector3(0, 2, -12);

    public override void Execute(Frame frame, EntityRef e, ref AIContext aiContext)
    {
      var newSpell = frame.Create(Spell);
      
      // We get our target from the blackboard
      var targetEntity = frame.Get<AIBlackboardComponent>(e).GetEntityRef(frame, TargetEntity.Key);
      frame.Unsafe.GetPointer<Spell>(newSpell)->TargetEntity = targetEntity;

      // Then we set the spell initial position
      // From there, the projectile itself will move towards its TargetEntity from a System
      frame.Unsafe.GetPointer<Transform3D>(newSpell)->Position = _spellcasterPosition;

      // A signal is invoked so this spell's cooldown is properly updated
      var spellType = frame.Get<Spell>(newSpell).SpellType;
      frame.Signals.OnSpellUsed(e, spellType);

      frame.Events.OnSpellUsed();
    }
  }
}
