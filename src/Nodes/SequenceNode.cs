namespace FluentBehaviourTree
{
    /// <summary>
    /// Runs child nodes in sequence, until one fails.
    /// </summary>
    public class SequenceNode : ParentBehaviourTreeNode
    {
        private int _lastRunningChildIndex;

        public SequenceNode(string name, int id) : base(name, id)
        {
        }

        protected override Status AbstractTick(float dt)
        {
            for (var index = _lastRunningChildIndex; index < ChildCount; index++)
            {
                var child = this[index];

                var childStatus = child.Tick(dt);

                if (childStatus == Status.Success)
                    continue;

                if (childStatus == Status.Failure)
                {
                    _lastRunningChildIndex = 0;
                    return childStatus;
                }

                if (childStatus == Status.Running)
                {
                    _lastRunningChildIndex = index;
                    return Status.Running;
                }
            }

            _lastRunningChildIndex = 0;
            return Status.Success;
        }
    }
}
