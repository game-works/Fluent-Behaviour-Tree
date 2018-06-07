namespace FluentBehaviourTree
{
    /// <summary>
    /// Selects the first node that succeeds. Tries successive nodes until it finds one that doesn't fail.
    /// </summary>
    public class SelectorNode<T> : ParentBehaviourTreeNode<T> where T : ITickData
    {
        private int _runningNodeIndex = -1;

        public SelectorNode(string name) : base(name) { }

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
             
                if (childStatus == Status.Success)
                    return Status.Success;

                if (childStatus == Status.Running)
                {
                    _runningNodeIndex = index;
                    return Status.Running;
                }
            }

            return Status.Failure;
        }
    }

    public class SelectorNode : SelectorNode<TimeData>
    {
        public SelectorNode(string name) : base(name) { }
    }
}
