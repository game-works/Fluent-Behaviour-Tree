using System;
using System.Collections.Generic;

namespace FluentBehaviourTree
{
    /// <summary>
    /// Fluent API for building a behaviour tree.
    /// </summary>
    public class BehaviourTreeBuilder
    {
        private int _idCounter = -1;
        private int _sequenceOccurenceCounter;
        private int _selectorOccurenceCounter;

        /// <summary>
        /// Last node created.
        /// </summary>
        private BehaviourTreeNode _curNode;

        /// <summary>
        /// Stack node nodes that we are build via the fluent API.
        /// </summary>
        private readonly Stack<ParentBehaviourTreeNode> _parentNodeStack = new Stack<ParentBehaviourTreeNode>();

        public BehaviourTreeBuilder()
        {
        }
        
        /// <summary>
        /// Add an action node.
        /// </summary>
        public BehaviourTreeBuilder Do(string name, BehaviourTreeNode node)
        {
            if (_parentNodeStack.Count <= 0)
            {
                throw new ApplicationException("Can't create an unnested ActionNode, it must be a leaf node.");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var id = ++_idCounter;
            
            node.Id = id;
            
            _parentNodeStack.Peek().AddChild(node);

            return this;
        }

        /// <summary>
        /// Create an action node.
        /// </summary>
        public BehaviourTreeBuilder Do(string name, Func<float, Status> fn)
        {
            if (_parentNodeStack.Count <= 0)
            {
                throw new ApplicationException("Can't create an unnested ActionNode, it must be a leaf node.");
            }
            if (string.IsNullOrEmpty(name))
            {
                name = fn.Method.Name;
            }

            var id = ++_idCounter;
            var actionNode = new FunctionNode(name, id, fn);
            
            _parentNodeStack.Peek().AddChild(actionNode);

            return this;
        }

        /// <summary>
        /// Like an action node... but the function can return true/false and is mapped to success/failure.
        /// </summary>
        public BehaviourTreeBuilder Condition(string name, Func<float, bool> fn)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = fn.Method.Name;
            }

            return Do(name, t => fn(t) ? Status.Success : Status.Failure);
        }

        /// <summary>
        /// Create an inverter node that inverts the success/failure of its children.
        /// </summary>
        public BehaviourTreeBuilder Inverter(string name)
        {
            var inverterNode = new InverterNode(name, ++_idCounter);

            if (_parentNodeStack.Count > 0)
            {
                _parentNodeStack.Peek().AddChild(inverterNode);
            }

            _parentNodeStack.Push(inverterNode);
            return this;
        }

        /// <summary>
        /// Runs each child node in sequence.
        /// Fails for the first child node that fails.
        /// Moves to the next child when the current running child succeeds.
        /// Stays on the current child node while it returns running.
        /// Succeeds when all child nodes have succeeded.
        /// </summary>
        public BehaviourTreeBuilder Sequence(string name)
        {
            var id = ++_idCounter;
            var sequenceNode = new SequenceNode(name, id);

            _sequenceOccurenceCounter++;

            if (_parentNodeStack.Count > 0)
            {
                _parentNodeStack.Peek().AddChild(sequenceNode);
            }

            _parentNodeStack.Push(sequenceNode);
            return this;
        }

        /// <summary>
        /// Create a parallel node.
        /// </summary>
        public BehaviourTreeBuilder Parallel(string name, int numRequiredToFail, int numRequiredToSucceed)
        {
            var id = ++_idCounter;
            var parallelNode = new ParallelNode(name, id, numRequiredToFail, numRequiredToSucceed);

            if (_parentNodeStack.Count > 0)
            {
                _parentNodeStack.Peek().AddChild(parallelNode);
            }

            _parentNodeStack.Push(parallelNode);
            return this;
        }

        /// <summary>
        /// Runs child nodes in sequence until it finds one that succeeds.
        /// Succeeds when it finds the first child that succeeds.
        /// For child nodes that fail it moves forward to the next child node.
        /// While a child is running it stays on that child node without moving forward.
        /// </summary>
        public BehaviourTreeBuilder Selector(string name)
        {
            var id = ++_idCounter;
            var selectorNode = new SelectorNode(name, id);

            _selectorOccurenceCounter++;

            if (_parentNodeStack.Count > 0)
            {
                _parentNodeStack.Peek().AddChild(selectorNode);
            }

            _parentNodeStack.Push(selectorNode);
            return this;
        }

        /// <summary>
        /// Splice a sub tree into the parent tree.
        /// </summary>
        public BehaviourTreeBuilder Splice(BehaviourTreeNode subTree)
        {
            if (subTree == null)
            {
                throw new ArgumentNullException(nameof(subTree));
            }
            if (_parentNodeStack.Count <= 0)
            {
                throw new ApplicationException("Can't splice an unnested sub-tree, there must be a parent-tree.");
            }

            _parentNodeStack.Peek().AddChild(subTree);
            return this;
        }

        /// <summary>
        /// Build the actual tree.
        /// </summary>
        public BehaviourTreeNode Build()
        {
            if (_curNode == null)
                throw new ApplicationException("Can't create a behaviour tree with zero nodes");

            _selectorOccurenceCounter = 0;
            _sequenceOccurenceCounter = 0;

            return _curNode;
        }

        /// <summary>
        /// Ends a sequence of children.
        /// </summary>
        public BehaviourTreeBuilder End()
        {
            _curNode = _parentNodeStack.Pop();
            return this;
        }
    }
}
