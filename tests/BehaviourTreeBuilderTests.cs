using System;
using System.Collections.Generic;
using FluentBehaviourTree;
using Xunit;

namespace tests
{
    public class BehaviourTreeBuilderTests
    {
        BehaviourTreeBuilder testObject;

        void Init()
        {
            testObject = new BehaviourTreeBuilder();
        }

        [Fact]
        public void cant_create_a_behaviour_tree_with_zero_nodes()
        {
            Init();

            Assert.Throws<ApplicationException>(() =>
                {
                    testObject.Build();
                }
            );

        }

        [Fact]
        public void cant_create_an_unested_action_node()
        {
            Init();

            Assert.Throws<ApplicationException>(() =>
                {
                    testObject
                         .Do("some-node-1", t => Status.Running)
                         .Build();
                }
            );
        }

        [Fact]
        public void can_create_inverter_node()
        {
            Init();

            var node = testObject
                .Inverter("some-inverter")
                    .Do("some-node", t =>Status.Success)
                .End()
                .Build();

            Assert.IsType<InverterNode<TimeData>>(node);
            Assert.Equal(Status.Failure, node.Tick(new TimeData(0)));
        }

        [Fact]
        public void cant_create_an_unbalanced_behaviour_tree()
        {
            Init();

            Assert.Throws<ApplicationException>(() =>
            {
                testObject
                    .Inverter("some-inverter")
                    .Do("some-node", t => Status.Success)
                .Build();
            });
        }

        [Fact]
        public void condition_is_syntactic_sugar_for_do()
        {
            Init();

            var node = testObject
                .Inverter("some-inverter")
                    .Condition("some-node", t => true)
                .End()
                .Build();

            Assert.IsType<InverterNode<TimeData>>(node);
            Assert.Equal(Status.Failure, node.Tick(new TimeData(0)));
        }

        [Fact]
        public void can_invert_an_inverter()
        {
            Init();

            var node = testObject
                .Inverter("some-inverter")
                    .Inverter("some-other-inverter")
                        .Do("some-node", t => Status.Success)
                    .End()
                .End()
                .Build();

            Assert.IsType<InverterNode<TimeData>>(node); 
            Assert.Equal(Status.Success, node.Tick(new TimeData(0)));
        }

        [Fact]
        public void adding_more_than_a_single_child_to_inverter_throws_exception()
        {
            Init();

            Assert.Throws<ApplicationException>(() =>
            {
                testObject
                    .Inverter("some-inverter")
                        .Do("some-node", t => Status.Success)
                        .Do("some-other-node", t => Status.Success)
                    .End()
                    .Build();
            });
        }

        [Fact]
        public void can_create_a_sequence()
        {
            Init();

            var invokeCount = 0;

            var sequence = testObject
                .Sequence("some-sequence")
                    .Do("some-action-1", t => 
                    {
                        ++invokeCount;
                        return Status.Success;
                    })
                    .Do("some-action-2", t =>
                    {
                        ++invokeCount;
                        return Status.Success;
                    })
                .End()
                .Build();

            Assert.IsType<SequenceNode<TimeData>>(sequence);
            Assert.Equal(Status.Success, sequence.Tick(new TimeData(0)));
            Assert.Equal(2, invokeCount);
        }

        [Fact]
        public void can_create_parallel()
        {
            Init();

            var invokeCount = 0;

            var parallel = testObject
                .Parallel("some-parallel", 2, 2)
                    .Do("some-action-1", t =>
                    {
                        ++invokeCount;
                        return Status.Success;
                    })
                    .Do("some-action-2", t =>
                    {
                        ++invokeCount;
                        return Status.Success;
                    })
                .End()
                .Build();

            Assert.IsType<ParallelNode<TimeData>>(parallel);
            Assert.Equal(Status.Success, parallel.Tick(new TimeData(0)));
            Assert.Equal(2, invokeCount);
        }

        [Fact]
        public void can_create_selector()
        {
            Init();

            var invokeCount = 0;

            var parallel = testObject
                .Selector("some-selector")
                    .Do("some-action-1", t =>
                    {
                        ++invokeCount;
                        return Status.Failure;
                    })
                    .Do("some-action-2", t =>
                    {
                        ++invokeCount;
                        return Status.Success;
                    })
                .End()
                .Build();

            Assert.IsType<SelectorNode<TimeData>>(parallel);
            Assert.Equal(Status.Success, parallel.Tick(new TimeData(0)));
            Assert.Equal(2, invokeCount);
        }

        [Fact]
        public void can_splice_sub_tree()
        {
            Init();

            var invokeCount = 0;

            var spliced = testObject
                .Sequence("spliced")
                    .Do("test", t =>
                    {
                        ++invokeCount;
                        return Status.Success;
                    })
                .End()
                .Build();

            var tree = testObject
                .Sequence("parent-tree")
                    .Splice(spliced)                    
                .End()
                .Build();

            tree.Tick(new TimeData(0));

            Assert.Equal(1, invokeCount);
        }

        [Fact]
        public void splicing_an_unnested_sub_tree_throws_exception()
        {
            Init();

            var invokeCount = 0;

            var spliced = testObject
                .Sequence("spliced")
                    .Do("test", t =>
                    {
                        ++invokeCount;
                        return Status.Success;
                    })
                .End()
                .Build();

            Assert.Throws<ApplicationException>(() =>
            {
                testObject
                    .Splice(spliced);
            });
        }

        [Fact]
        public void disabled_parent_nodes_is_ignored()
        {
            TimeData data = new TimeData(0);
            var invokeCount = 0;

            var builder = new BehaviourTreeBuilder<TimeData>(new List<string>
            {
                "seq1",
                "sel1",
                "para1"
            });

            builder
                .Sequence("seq1")
                .Do("do1", t =>
                {
                    ++invokeCount;
                    return Status.Success;
                })
                .End()
                .Build()
                .Tick(data);

            builder
                .Selector("sel1")
                .Do("do2", t =>
                {
                    ++invokeCount;
                    return Status.Success;
                })
                .End()
                .Build()
                .Tick(data);

            builder
                .Parallel("para1", 0, 0)
                .Do("do3", t =>
                {
                    ++invokeCount;
                    return Status.Success;
                })
                .End()
                .Build()
                .Tick(data);

            Assert.Equal(0, invokeCount);
        }

        [Fact]
        public void disabled_leaf_nodes_is_ignored()
        {
            TimeData data = new TimeData(0);
            var invokeCount = 0;

            var builder = new BehaviourTreeBuilder<TimeData>(new List<string>
            {
                "do1",
                "do2",
            });

            builder
                .Sequence("seq1")
                .Do("do1", t =>
                {
                    ++invokeCount;
                    return Status.Success;
                })
                .End()
                .Build()
                .Tick(data);

            builder
                .Selector("sel1")
                .Do("do2", t =>
                {
                    ++invokeCount;
                    return Status.Success;
                })
                .End()
                .Build()
                .Tick(data);

            builder
                .Parallel("para1", 0, 0)
                .Do("do3", t =>
                {
                    ++invokeCount;
                    return Status.Success;
                })
                .End()
                .Build()
                .Tick(data);

            Assert.Equal(1, invokeCount);
        }
    }
}
