namespace FluentBehaviourTree
{
    /// <summary>
    /// Runs child nodes in sequence, until one fails.
    /// </summary>
    public class SequenceNode<T> : ParentBehaviourTreeNode<T> where T : ITickData
    {
        public SequenceNode(string name) : base(name) { }

        protected override Status AbstractTick(T data)
        {
            for (int i = 0; i < ChildCount; i++)
            {
                var child = this[i];
                var childStatus = child.Tick(data);
             
                if (childStatus != Status.Success)
                    return childStatus;
            }
            return Status.Success;
        }
    }

    public class SequenceNode : SequenceNode<TimeData>
    {
        public SequenceNode(string name) : base(name) { }
    }
}
