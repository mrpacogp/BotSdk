using Quantum.BotSDK;

namespace Quantum
{
  public static unsafe class CollectorSampleHelpers
  {
    // Tries to get an existing Collector entity based on the PlayerRef
    public static EntityRef GetCollectorEntity(Frame frame, PlayerRef playerRef)
    {
      var playerLinks = frame.GetComponentIterator<PlayerLink>();
      foreach (var (entity, playerLink) in playerLinks)
      {
        if (playerLink.Ref == playerRef)
        {
          return entity;
        }
      }

      return default;
    }


    public static void SetupAsBot(Frame frame, EntityRef entity)
    {
      // Create the HFSM Agent and pick the AIConfig, if there is any
      var hfsmAgent = new HFSMAgent();
      var hfsmRoot = frame.FindAsset<HFSMRoot>(frame.RuntimeConfig.CollectorsSampleConfig.ReplacementHFSM.Id);
      HFSMManager.Init(frame, &hfsmAgent.Data, entity, hfsmRoot);
      frame.Set(entity, hfsmAgent);

      // Setup the blackboard
      var blackboardComponent = new AIBlackboardComponent();
      var blackboardAsset = frame.FindAsset<AIBlackboard>(frame.RuntimeConfig.CollectorsSampleConfig.ReplacementAIBlackboard.Id);
      var blackboardInitializerAsset = frame.FindAsset<AIBlackboardInitializer>(blackboardAsset.InitializerRef);
      AIBlackboardInitializer.InitializeBlackboard(frame, &blackboardComponent, blackboardInitializerAsset);
      frame.Set(entity, blackboardComponent);

      // Add the Bot component to store AI relevant data
      frame.Set(entity, default(Bot));

      BotSDKDebuggerSystem.AddToDebugger(frame, entity, hfsmAgent);
    }
  }
}