using System.Collections.Generic;

namespace FluentBehaviourTree
{
    /// <summary>
    /// Interface for behaviour tree nodes.
    /// </summary>
    public abstract class ParentBehaviourTreeNode : BehaviourTreeNode
    {
        /// <summary>
        /// List of child nodes
        /// </summary>
        private readonly List<BehaviourTreeNode> _children = new List<BehaviourTreeNode>();

        /// <summary>
        /// The number of children added to this node
        /// </summary>
        public int ChildCount => _children.Count;

        protected ParentBehaviourTreeNode(string name, int id) : base(name, id) { }

        /// <summary>
        /// Retrieve a child node by index.
        /// </summary>
        public BehaviourTreeNode this[int index] => _children[index];

        /// <summary>
        /// Marks that this node and all children have not execute yet.
        /// </summary>
        internal override void ResetLastExecStatus()
        {
            base.ResetLastExecStatus();
            for (int i = 0; i < ChildCount; i++)
            {
                this[i].ResetLastExecStatus();
            }
        }
        /// <summary>
        /// Add a child to the parent node.
        /// </summary>
        public virtual void AddChild(BehaviourTreeNode child)
        {
            _children.Add(child);
        }
    }
}
