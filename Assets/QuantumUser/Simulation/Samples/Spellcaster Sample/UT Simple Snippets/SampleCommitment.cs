namespace Quantum
{
  [System.Serializable]
  public unsafe class SampleCommitment : AIFunction<bool>
  {
    // Inherit from AIFunction<bool> to create Commitment assets
    // Implement the Execute method. The Commitment to a Consideration is canceled when Execute() returns False
    // Commitment is highly tied to the concept of Ranking and Momentum
    // If you are not aware of such concepts, you can find it's explanations into the website documentation

    public override bool Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
    {
      return false;
    }
  }
}
