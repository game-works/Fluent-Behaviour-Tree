namespace FluentBehaviourTree
{
    /// <summary>
    /// Selects the first node that succeeds. Tries successive nodes until it finds one that doesn't fail.
    /// </summary>
    public class SelectorNode : ParentBehaviourTreeNode
    {
        private int _lastRunningChildIndex;

        public SelectorNode(int id) : base(id) { }

        protected override Status AbstractTick(float dt)
        {
            for (var index = _lastRunningChildIndex ; index < ChildCount; index++)
            {
                var child = this[index];
                var childStatus = child.Tick(dt);

                if (childStatus == Status.Success)
                {
                    _lastRunningChildIndex = 0;
                    return Status.Success;
                }

                if (childStatus == Status.Running)
                {
                    _lastRunningChildIndex = index;
                    return Status.Running;
                }
            }

            _lastRunningChildIndex = 0;
            return Status.Failure;
        }
    }
}
