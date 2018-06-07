namespace FluentBehaviourTree
{
    /// <summary>
    /// Runs child nodes in sequence, until one fails.
    /// </summary>
    public class SequenceNode<T> : ParentBehaviourTreeNode<T> where T : ITickData
    {
        private int _runningNodeIndex = -1;

        public SequenceNode(string name) : base(name) { }

        protected override Status AbstractTick(T data)
        {
            int index = 0;
            
            if (_runningNodeIndex != -1)
            {
                index = _runningNodeIndex;
                _runningNodeIndex = -1;
            }

            for (; index < ChildCount; index++)
            {
                var child = this[index];
                var childStatus = child.Tick(data);
             
                if (childStatus == Status.Failure)
                    return childStatus;

                if (childStatus == Status.Running)
                {
                    _runningNodeIndex = index;
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
