using System.Linq;

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
            RunningSequences = Enumerable.Repeat(-1, 10).ToArray();
            RunningSelectors = Enumerable.Repeat(-1, 10).ToArray();
        }

        public float DeltaTime { get; set; }
        public int[] RunningSequences { get; set; }
        public int[] RunningSelectors { get; set; }
    }
}
