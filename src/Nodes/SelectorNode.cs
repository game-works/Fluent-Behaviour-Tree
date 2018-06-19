namespace FluentBehaviourTree
{
    /// <summary>
    /// Selects the first node that succeeds. Tries successive nodes until it finds one that doesn't fail.
    /// </summary>
    public class SelectorNode<T> : ParentBehaviourTreeNode<T> where T : ITickData
    {
        public int SelectorId { get; set; }

        public SelectorNode(string name, int id) : base(name, id) { }

        protected override Status AbstractTick(T data)
        {
            int index = 0;

            int runningNodeIndex = data.RunningSelectors[SelectorId];
            if (runningNodeIndex != -1)
            {
                index = runningNodeIndex;
                data.RunningSelectors[SelectorId] = -1;
            }

            for (; index < ChildCount; index++)
            {
                var child = this[index];
                var childStatus = child.Tick(data);
             
                if (childStatus == Status.Success)
                    return Status.Success;

                if (childStatus == Status.Running)
                {
                    data.RunningSelectors[SelectorId] = index;
                    return Status.Running;
                }
            }

            return Status.Failure;
        }
    }

    public class SelectorNode : SelectorNode<TimeData>
    {
        public SelectorNode(string name, int id) : base(name, id) { }
    }
}
