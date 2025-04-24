using Photon.Deterministic;
using System;

namespace Quantum
{
  [Serializable]
  public struct CollectorsSampleRuntimeConfig
  {
    public AssetRef<EntityPrototype> CollectiblePrototype;

     // The prototype to be spawned for players, by default without any bot specific component
    public AssetRef<EntityPrototype> PlayerCollectorPrototype;

    // Which HFSMs should take control when replacing disconnected players
    public AssetRef<HFSMRoot> ReplacementHFSM;
    public AssetRef<AIBlackboard> ReplacementAIBlackboard;

    // Should players be replaced if they disconnect?
    public bool ReplaceOnDisconnect;

    // Should the room be filled if not enough players connect to it?
    public bool FillRoom;

    // How many time should be waited before filling the room with bots?
    public FP FillRoomCooldown;

    // Bots to be created independently of player replacement and fill room features
    public AssetRef<EntityPrototype>[] Bots;
  }

  [Serializable]
  public struct SpellcasterSampleRuntimeConfig
  {
    public AssetRef<AIBlackboard> UTBlackboard;
  }

  partial class RuntimeConfig
  {
    public CollectorsSampleRuntimeConfig CollectorsSampleConfig;
    public SpellcasterSampleRuntimeConfig SpellcasterSampleConfig;
    public string tonterias = "none";
    partial void SerializeUserData(BitStream stream)
    {
      stream.Serialize(ref CollectorsSampleConfig.CollectiblePrototype);
      stream.Serialize(ref CollectorsSampleConfig.PlayerCollectorPrototype);
      stream.Serialize(ref CollectorsSampleConfig.ReplacementHFSM);
      stream.Serialize(ref CollectorsSampleConfig.ReplacementAIBlackboard);
      stream.Serialize(ref CollectorsSampleConfig.ReplaceOnDisconnect);
      stream.Serialize(ref CollectorsSampleConfig.FillRoom);
      stream.Serialize(ref CollectorsSampleConfig.FillRoomCooldown);

      stream.SerializeArrayLength(ref CollectorsSampleConfig.Bots);
      for (int i = 0; i < CollectorsSampleConfig.Bots.Length; i++)
      {
        stream.Serialize(ref CollectorsSampleConfig.Bots[i].Id.Value);
      }

      stream.Serialize(ref SpellcasterSampleConfig.UTBlackboard);
    }
  }
}