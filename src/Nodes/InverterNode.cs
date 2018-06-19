using System;

namespace FluentBehaviourTree
{
    /// <summary>
    /// Decorator node that inverts the success/failure of its child.
    /// </summary>
    public class InverterNode<T> : ParentBehaviourTreeNode<T> where T : ITickData
    {
        /// <summary>
        /// The child to be inverted.
        /// </summary>
        private BehaviourTreeNode<T> ChildNode => ChildCount == 0 ? null : this[0];

        public InverterNode(string name, int id) : base(name, id) { }

        protected override Status AbstractTick(T data)
        {
            if (ChildNode == null)
                throw new ApplicationException("InverterNode must have a child node!");

            var result = ChildNode.Tick(data);

            switch (result)
            {
                case Status.Failure:
                    return Status.Success;
                case Status.Success:
                    return Status.Failure;
                default:
                    return result;
            }
        }

        public override void AddChild(BehaviourTreeNode<T> child)
        {
            if (ChildNode != null)
                throw new ApplicationException("Can't add more than a single child to InverterNode!");
            
            base.AddChild(child);
        }
    }

    public class InverterNode : InverterNode<TimeData>
    {
        public InverterNode(string name, int id) : base(name, id) { }
    }
}
