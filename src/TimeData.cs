namespace FluentBehaviourTree
{
    /// <summary>
    /// Represents time. Used to pass time values to behaviour tree nodes.
    /// </summary>
    public struct TimeData : ITickData
    {
        public TimeData(float deltaTime)
        {
            DeltaTime = deltaTime;
            RunningSequences = new int[10];
            RunningSelectors = new int[10];
        }

        public float DeltaTime;

        public int[] RunningSequences { get; set; }
        public int[] RunningSelectors { get; set; }
    }
}
