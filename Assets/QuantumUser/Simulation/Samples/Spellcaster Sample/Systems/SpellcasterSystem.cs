using Photon.Deterministic;
using Quantum.BotSDK;

namespace Quantum
{
  public unsafe class SpellcasterSystem : SystemMainThreadFilter<SpellcasterSystem.Filter>, ISignalOnSpellUsed
  {
    public struct Filter
    {
      public EntityRef Entity;
      public Spellcaster* Spellcaster;
      public UTAgent* utAgent;
    }

    public override void OnInit(Frame frame)
    {
      var spellcaster = frame.Unsafe.GetPointerSingleton<Spellcaster>();
      var spellcasterEntity = frame.GetSingletonEntityRef<Spellcaster>();
      if (frame.Unsafe.TryGetPointer<AIBlackboardComponent>(spellcasterEntity, out var blackboardComponent))
      {
        var blackboardAsset = frame.FindAsset<AIBlackboard>(frame.RuntimeConfig.SpellcasterSampleConfig.UTBlackboard.Id);
        var bbInitializer = frame.FindAsset<AIBlackboardInitializer>(blackboardAsset.InitializerRef.Id);
        AIBlackboardInitializer.InitializeBlackboard(frame, blackboardComponent, bbInitializer);

        // We pre-set the castle entity on the Blackboard as it doesn't need to be done more than
        // once during gameplay
        var castleEntity = frame.GetSingletonEntityRef<Castle>();
        blackboardComponent->Set(frame, "CastleEntity", castleEntity);
      }

      // Initialize the cooldowns dictionary
      var cooldowns = frame.AllocateDictionary<ESpell, FP>();
      cooldowns.Add(ESpell.Fire, 0);
      cooldowns.Add(ESpell.Lightning, 0);
      cooldowns.Add(ESpell.Ice, 0);
      cooldowns.Add(ESpell.Swamp, 0);
      cooldowns.Add(ESpell.Holy, 0);
      spellcaster->SpellsCooldown = cooldowns;

      // Initialize the UT Agent
      if (frame.Unsafe.TryGetPointer<UTAgent>(spellcasterEntity, out var utAgent))
      {
        UTManager.Init(frame, &utAgent->UtilityReasoner, utAgent->UtilityReasoner.UTRoot, spellcasterEntity);
        BotSDKDebuggerSystem.AddToDebugger(frame, spellcasterEntity, *utAgent);
      }
    }

    public override void Update(Frame frame, ref Filter filter)
    {
      TickCooldowns(frame, ref filter);
      UTManager.Update(frame, &filter.utAgent->UtilityReasoner, filter.Entity);
    }

    private void TickCooldowns(Frame frame, ref Filter filter)
    {
      var spellcaster = frame.GetSingleton<Spellcaster>();
      var cooldowns = frame.ResolveDictionary(spellcaster.SpellsCooldown);
      foreach (var kvp in cooldowns)
      {
        if(kvp.Value > 0)
        {
          var newValue = kvp.Value - frame.DeltaTime;
          if(newValue < 0)
          {
            newValue = 0;
          }
          cooldowns[kvp.Key] = newValue;
        }
      }
    }

    // When a spell is used, we set it's cooldown
    public void OnSpellUsed(Frame frame, EntityRef casterEntity, ESpell spellType)
    {
      var spellcaster = frame.Unsafe.GetPointer<Spellcaster>(casterEntity);
      var cooldowns = frame.ResolveDictionary(spellcaster->SpellsCooldown);

      if (cooldowns.ContainsKey(spellType) == true)
      {
        if (spellType == ESpell.Fire)
        {
          cooldowns[spellType] = 1;
        }

        if (spellType == ESpell.Lightning)
        {
          cooldowns[spellType] = 2;
        }

        if (spellType == ESpell.Ice)
        {
          cooldowns[spellType] = 4;
        }

        if (spellType == ESpell.Swamp)
        {
          cooldowns[spellType] = 3;
        }

        if (spellType == ESpell.Holy)
        {
          cooldowns[spellType] = 2;
        }
      }
    }
  }
}
