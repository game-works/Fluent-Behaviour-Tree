using System;

namespace FluentBehaviourTree
{
    /// <summary>
    /// A behaviour tree leaf node for running an action.
    /// </summary>
    public class FunctionNode : BehaviourTreeNode
    {
        /// <summary>
        /// Function to invoke for the action.
        /// </summary>
        private readonly Func<float, Status> _fn;

        public FunctionNode(int id, Func<float, Status> fn) : base(id)
        {
            _fn = fn;
        }

        protected override Status AbstractTick(float dt)
        {
            return _fn(dt);
        }
    }
}
