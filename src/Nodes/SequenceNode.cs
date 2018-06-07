namespace FluentBehaviourTree
{
    /// <summary>
    /// Runs child nodes in sequence, until one fails.
    /// </summary>
    public class SequenceNode<T> : ParentBehaviourTreeNode<T> where T : ITickData
    {
        public int SequenceId { get; set; }

        public SequenceNode(string name) : base(name) { }

        protected override Status AbstractTick(T data)
        {
            int index = 0;

            int runningNodeIndex = data.RunningSequences[SequenceId];
            if (runningNodeIndex != -1)
            {
                index = runningNodeIndex;
                data.RunningSequences[SequenceId] = -1;
            }

            for (; index < ChildCount; index++)
            {
                var child = this[index];
                var childStatus = child.Tick(data);
             
                if (childStatus == Status.Failure)
                    return childStatus;

                if (childStatus == Status.Running)
                {
                    data.RunningSequences[SequenceId] = index;
                    return Status.Running;
                }
            }

            return Status.Success;
        }
    }

    public class SequenceNode : SequenceNode<TimeData>
    {
        public SequenceNode(string name) : base(name) { }
    }
}
