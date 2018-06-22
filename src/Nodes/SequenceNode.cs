namespace FluentBehaviourTree
{
    /// <summary>
    /// Runs child nodes in sequence, until one fails.
    /// </summary>
    public class SequenceNode<T> : ParentBehaviourTreeNode<T> where T : ITickData
    {
        public bool SkipConditionsIfRunning { get; set; }

        public int SequenceId { get; set; }

        public SequenceNode(string name, int id, bool skipConditionsIfRunning = true) : base(name, id)
        {
            SkipConditionsIfRunning = skipConditionsIfRunning;
        }

        protected override Status AbstractTick(T data)
        {
            int index = 0;

            int runningNodeIndex = data.RunningSequences[SequenceId];
            if (runningNodeIndex != -1)
            {
                if (SkipConditionsIfRunning)
                    index = runningNodeIndex;

                data.RunningSequences[SequenceId] = -1;
            }

            for (; index < ChildCount; index++)
            {
                var child = this[index];

                //Checking if we should check conditions up before a running node.
                bool checkCondtions = !SkipConditionsIfRunning &&
                        runningNodeIndex != -1 &&
                        index < runningNodeIndex;

                //If we are, dont check non-condition nodes.
                if (checkCondtions &&
                    !child.IsCondition)
                    continue;

                var childStatus = child.Tick(data);

                if (checkCondtions &&
                    childStatus == Status.Success)
                    continue;

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
        public SequenceNode(string name, int id, bool skipConditionsIfRunning = true) : base(name, id)
        {
            SkipConditionsIfRunning = skipConditionsIfRunning;
        }
    }
}
