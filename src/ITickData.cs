namespace FluentBehaviourTree
{
    public interface ITickData
    {
        int[] RunningSequences { get; set; }
        int[] RunningSelectors { get; set; }
    }
}
