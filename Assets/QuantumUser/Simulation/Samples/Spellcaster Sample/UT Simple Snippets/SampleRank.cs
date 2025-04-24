namespace Quantum
{
  [System.Serializable]
  public unsafe class SampleRank : AIFunction<int>
  {
    // Inherit from AIFunction<int> to create your Rank assets
    // Implement the Execute method. The Integer returned will be the Consideration's Rank at that frame
    // The Rank returned here can be any value of your preference
    // When the UT runs, the Considerations with the highest Ranking will
    // be isolated and executed with absolute priority

    public override int Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
    {
      return 0;
    }
  }
}
