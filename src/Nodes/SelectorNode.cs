namespace FluentBehaviourTree
{
    /// <summary>
    /// Selects the first node that succeeds. Tries successive nodes until it finds one that doesn't fail.
    /// </summary>
    public class SelectorNode<T> : ParentBehaviourTreeNode<T> where T : ITickData
    {
        public SelectorNode(string name) : base(name) { }

        protected override Status AbstractTick(T data)
        {
            for (int i = 0; i < ChildCount; i++)
            {
                var child = this[i];
                var childStatus = child.Tick(data);
             
                if (childStatus != Status.Failure)
                    return childStatus;
            }

            return Status.Failure;
        }
    }

    public class SelectorNode : SelectorNode<TimeData>
    {
        public SelectorNode(string name) : base(name) { }
    }
}
