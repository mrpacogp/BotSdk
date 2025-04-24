using Photon.Deterministic;

namespace Quantum
{
  [System.Serializable]
  public unsafe partial class InputSkillCooldown : AIFunction<FP>
  {
    // [Bot SDK Sample]
    // Based on the type of spell, get it's cooldown

    public ESpell SpellType;

    public override FP Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
    {
      var spellcaster = frame.Get<Spellcaster>(entity);
      var cooldowns = frame.ResolveDictionary(spellcaster.SpellsCooldown);
      if(cooldowns.TryGetValue(SpellType, out var cooldown) == true)
      {
        return cooldown;
      }

      return 0;
    }
  }
}
