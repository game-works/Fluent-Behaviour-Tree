using System;

namespace FluentBehaviourTree
{
    /// <summary>
    /// A behaviour tree leaf node for running an action.
    /// </summary>
    public class ActionNode<T> : BehaviourTreeNode<T> where T : ITickData
    {
        /// <summary>
        /// Function to invoke for the action.
        /// </summary>
        private readonly Func<T, Status> _fn;

        public ActionNode(string name, int id, Func<T, Status> fn) : base(name, id)
        {
            _fn = fn;
        }

        protected override Status AbstractTick(T data)
        {
            return _fn(data);
        }
    }

    public class ActionNode : ActionNode<TimeData>
    {
        public ActionNode(string name, int id, Func<TimeData, Status> fn) : base(name, id, fn) { }
    }
}
