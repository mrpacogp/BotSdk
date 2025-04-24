using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
  public unsafe class RoomFillSystem : SystemMainThread
  {
    public override void Update(Frame frame)
    {
      CheckFillRoom(frame);
    }

    // Wait for some seconds, defined on the RuntimeConfig, to actually
    // fill the room with bots. This is a time window in which players can
    // still connect to the game before being replaced by a Bot
    // These Bots cannot be further replaced by a player who joined the game later
    // If you need this, you just need to change these Bot's player refs to be different from default,
    // so player inputs can be applied to the bot
    private void CheckFillRoom(Frame frame)
    {
      // Return if the room shouldn't be filled with Bots
      if (frame.RuntimeConfig.CollectorsSampleConfig.FillRoom == false)
        return;

      // Update the fill room timer and, when it is past the delay, fill the room
      frame.Global->FillRoomTimer += frame.DeltaTime;

      if (frame.Global->AlreadyFilledRoom == false && frame.Global->FillRoomTimer >= frame.RuntimeConfig.CollectorsSampleConfig.FillRoomCooldown)
      {
        FillRoomWithBots(frame);
        frame.Global->AlreadyFilledRoom = true;
      }
    }

    private void FillRoomWithBots(Frame frame)
    {
      // Create as many Bot entities as needed to fill the room
      // These are player agnostic, bot-only entities (there is PlayerLink component on them)
      for (int i = 0; i < frame.PlayerCount; i++)
      {
        var botIsNeeded = CollectorSampleHelpers.GetCollectorEntity(frame, i) == default;

        if (botIsNeeded)
        {
          var collectorEntity = frame.Create(frame.RuntimeConfig.CollectorsSampleConfig.PlayerCollectorPrototype);
          frame.Unsafe.GetPointer<Transform2D>(collectorEntity)->Position = FPVector2.Right * i * 2;

          CollectorSampleHelpers.SetupAsBot(frame, collectorEntity);
        }
      }
    }
  }
}