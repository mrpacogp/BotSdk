using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
  public unsafe class PlayerSystem : SystemMainThreadFilter<PlayerSystem.Filter>, ISignalOnPlayerAdded, ISignalOnPlayerDisconnected
  {
    public struct Filter
    {
      public EntityRef EntityRef;
      public PlayerLink* PlayerLink;
    }

    #region Player Joining
    // When a player joins from this callback, create a non-bot entity
    public void OnPlayerAdded(Frame frame, PlayerRef player, bool firstTime)
    {
      // First, check if there is already an entity with this PlayerRef
      // If there is, do not create a new entity for the joining player, it will just take the entity's control instead
      bool alreadyCreated = PlayerEntityExists(frame, player);
      if (alreadyCreated == false)
      {
        CreatePlayerEntity(frame, player);
      }
    }

    // Checks if there is already an entity with this PlayerRef
    private bool PlayerEntityExists(Frame frame, PlayerRef playerRef)
    {
      var playerEntity = GetCollectorEntity(frame, playerRef);
      return playerEntity != default;
    }

    // Try to get an existing Collector entity based on the PlayerRef
    private EntityRef GetCollectorEntity(Frame frame, PlayerRef playerRef)
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

    private void CreatePlayerEntity(Frame frame, PlayerRef player)
    {
      var playerData = frame.GetPlayerData(player);

      var collectorEntity = frame.Create(frame.RuntimeConfig.CollectorsSampleConfig.PlayerCollectorPrototype);

      // Add the player link component and set the player ref on it
      frame.AddOrGet<PlayerLink>(collectorEntity, out var playerLink);
      playerLink->Ref = player;

      // Set an offsetted position just to avoid creating it inside other collector entities
      frame.Unsafe.GetPointer<Transform2D>(collectorEntity)->Position = FPVector2.Right * player * 2;
    }
    #endregion

    #region Update and Switching Player/Bot Controls
    public override void Update(Frame frame, ref Filter filter)
    {
      var physicsBody = frame.Unsafe.GetPointer<PhysicsBody2D>(filter.EntityRef);
      var movementVector = frame.GetPlayerInput(filter.PlayerLink->Ref)->Movement;
      physicsBody->Velocity = movementVector * 5;

      // Apply rotation based on the velocity
      if (physicsBody->Velocity != FPVector2.Zero)
      {
        var transform = frame.Unsafe.GetPointer<Transform2D>(filter.EntityRef);
        transform->Rotation = FPMath.Atan2(-physicsBody->Velocity.X, physicsBody->Velocity.Y);
      }
    }

    // Setup AI data for a player entity when it gets disconnected from the game
    // This method is only called for entities which were controlled by a player from the very beginning.
    public void OnPlayerDisconnected(Frame frame, PlayerRef playerRef)
    {
      // Return if disconnected players shouldn't be replaced by bots
      if (frame.RuntimeConfig.CollectorsSampleConfig.ReplaceOnDisconnect == false)
        return;

      // Only update this information if this frame is Verified
      // PlayerInputFlags are not trustful during Predicted frames
      if (frame.IsVerified == false)
        return;

      var collectorEntity = GetCollectorEntity(frame, playerRef);
      CollectorSampleHelpers.SetupAsBot(frame, collectorEntity);
    }
    #endregion
  }
}