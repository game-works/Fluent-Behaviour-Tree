using System;
using System.Collections.Generic;

namespace FluentBehaviourTree
{
    /// <summary>
    /// Fluent API for building a behaviour tree.
    /// </summary>
    public class BehaviourTreeBuilder<T> where T : ITickData
    {
        private readonly List<string> _blacklist = new List<string>();
        private int _sequenceOccurenceCounter;
        private int _selectorOccurenceCounter;

        /// <summary>
        /// Last node created.
        /// </summary>
        private BehaviourTreeNode<T> _curNode;

        /// <summary>
        /// Stack node nodes that we are build via the fluent API.
        /// </summary>
        private readonly Stack<ParentBehaviourTreeNode<T>> _parentNodeStack = new Stack<ParentBehaviourTreeNode<T>>();

        public bool IsBlacklisted(string name) => _blacklist?.Contains(name) ?? false;

        public BehaviourTreeBuilder()
        {
        }

        public BehaviourTreeBuilder(List<string> blacklistedNodes)
        {
            _blacklist = blacklistedNodes;
        }

        /// <summary>
        /// Create an action node.
        /// </summary>
        public BehaviourTreeBuilder<T> Do(string name, Func<T, Status> fn)
        {
            if (_parentNodeStack.Count <= 0)
            {
                throw new ApplicationException("Can't create an unnested ActionNode, it must be a leaf node.");
            }
            if (string.IsNullOrEmpty(name))
            {
                name = fn.Method.Name;
            }

            var actionNode = new ActionNode<T>(name, fn)
            {
                IsDisabled = IsBlacklisted(name)
            };

            _parentNodeStack.Peek().AddChild(actionNode);

            return this;
        }

        /// <summary>
        /// Like an action node... but the function can return true/false and is mapped to success/failure.
        /// </summary>
        public BehaviourTreeBuilder<T> Condition(string name, Func<T, bool> fn)
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
        public BehaviourTreeBuilder<T> Inverter(string name)
        {
            var inverterNode = new InverterNode<T>(name);

            if (_parentNodeStack.Count > 0)
            {
                _parentNodeStack.Peek().AddChild(inverterNode);
            }

            _parentNodeStack.Push(inverterNode);
            return this;
        }

        /// <summary>
        /// Create a sequence node.
        /// </summary>
        public BehaviourTreeBuilder<T> Sequence(string name)
        {
            var sequenceNode = new SequenceNode<T>(name)
            {
                SequenceId = _sequenceOccurenceCounter,
                IsDisabled = IsBlacklisted(name)
            };

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
        public BehaviourTreeBuilder<T> Parallel(string name, int numRequiredToFail, int numRequiredToSucceed)
        {
            var parallelNode = new ParallelNode<T>(name, numRequiredToFail, numRequiredToSucceed)
            {
                IsDisabled = IsBlacklisted(name)
            };

            if (_parentNodeStack.Count > 0)
            {
                _parentNodeStack.Peek().AddChild(parallelNode);
            }

            _parentNodeStack.Push(parallelNode);
            return this;
        }

        /// <summary>
        /// Create a selector node.
        /// </summary>
        public BehaviourTreeBuilder<T> Selector(string name)
        {
            var selectorNode = new SelectorNode<T>(name)
            {
                SelectorId = _selectorOccurenceCounter,
                IsDisabled = IsBlacklisted(name)
            };

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
        public BehaviourTreeBuilder<T> Splice(BehaviourTreeNode<T> subTree)
        {
            if (subTree == null)
            {
                throw new ArgumentNullException("subTree");
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
        public BehaviourTreeNode<T> Build()
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
        public BehaviourTreeBuilder<T> End()
        {
            _curNode = _parentNodeStack.Pop();
            return this;
        }
    }

    public class BehaviourTreeBuilder : BehaviourTreeBuilder<TimeData>
    {
    }
}
