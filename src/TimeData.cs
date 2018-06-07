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
        }

        public float DeltaTime;
    }
}
