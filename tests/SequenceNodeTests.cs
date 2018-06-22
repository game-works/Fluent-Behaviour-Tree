using FluentBehaviourTree;
using Moq;
using Xunit;

namespace tests
{
    public class SequenceNodeTests
    {
        SequenceNode testObject;

        void Init()
        {
            testObject = new SequenceNode("some-sequence", 0);
        }
        
        [Fact]
        public void can_run_all_children_in_order()
        {
            Init();

            var time = new TimeData(0);

            var callOrder = 0;

            var mockChild1 = new Mock<BehaviourTreeNode>();
            mockChild1
                .Setup(m => m.Tick(time))
                .Returns(Status.Success)
                .Callback(() =>
                 {
                     Assert.Equal(1, ++callOrder);
                 });

            var mockChild2 = new Mock<BehaviourTreeNode>();
            mockChild2
                .Setup(m => m.Tick(time))
                .Returns(Status.Success)
                .Callback(() =>
                {
                    Assert.Equal(2, ++callOrder);
                });

            testObject.AddChild(mockChild1.Object);
            testObject.AddChild(mockChild2.Object);

            Assert.Equal(Status.Success, testObject.Tick(time));

            Assert.Equal(2, callOrder);

            mockChild1.Verify(m => m.Tick(time), Times.Once());
            mockChild2.Verify(m => m.Tick(time), Times.Once());
        }

        [Fact]
        public void when_first_child_is_running_second_child_is_supressed()
        {
            Init();

            var time = new TimeData(0);

            var mockChild1 = new Mock<BehaviourTreeNode>();
            mockChild1
                .Setup(m => m.Tick(time))
                .Returns(Status.Running);

            var mockChild2 = new Mock<BehaviourTreeNode>();

            testObject.AddChild(mockChild1.Object);
            testObject.AddChild(mockChild2.Object);

            Assert.Equal(Status.Running, testObject.Tick(time));

            mockChild1.Verify(m => m.Tick(time), Times.Once());
            mockChild2.Verify(m => m.Tick(time), Times.Never());
        }

        [Fact]
        public void when_second_child_is_running_first_child_is_supressed()
        {
            Init();

            var time = new TimeData(0);

            var mockChild1 = new Mock<BehaviourTreeNode>();
            mockChild1
                .Setup(m => m.Tick(time))
                .Returns(Status.Success);

            var mockChild2 = new Mock<BehaviourTreeNode>();
            mockChild2
                .SetupSequence(node => node.Tick(time))
                .Returns(Status.Running)
                .Returns(Status.Success);

            var mockChild3 = new Mock<BehaviourTreeNode>();
            mockChild3
                .Setup(m => m.Tick(time))
                .Returns(Status.Success);

            testObject.AddChild(mockChild1.Object);
            testObject.AddChild(mockChild2.Object);
            testObject.AddChild(mockChild3.Object);

            Assert.Equal(Status.Running, testObject.Tick(time));
            Assert.Equal(Status.Success, testObject.Tick(time));

            mockChild1.Verify(m => m.Tick(time), Times.Once());
            mockChild2.Verify(m => m.Tick(time), Times.Exactly(2));
            mockChild3.Verify(m => m.Tick(time), Times.Once);
        }

        [Fact]
        public void when_first_child_fails_then_entire_sequence_fails()
        {
            Init();

            var time = new TimeData(0);

            var mockChild1 = new Mock<BehaviourTreeNode>();
            mockChild1
                .Setup(m => m.Tick(time))
                .Returns(Status.Failure);

            var mockChild2 = new Mock<BehaviourTreeNode>();

            testObject.AddChild(mockChild1.Object);
            testObject.AddChild(mockChild2.Object);

            Assert.Equal(Status.Failure, testObject.Tick(time));

            mockChild1.Verify(m => m.Tick(time), Times.Once());
            mockChild2.Verify(m => m.Tick(time), Times.Never());
        }

        [Fact]
        public void when_second_child_fails_then_entire_sequence_fails()
        {
            Init();

            var time = new TimeData(0);

            var mockChild1 = new Mock<BehaviourTreeNode>();
            mockChild1
                .Setup(m => m.Tick(time))
                .Returns(Status.Success);

            var mockChild2 = new Mock<BehaviourTreeNode>();
            mockChild2
                .Setup(m => m.Tick(time))
                .Returns(Status.Failure);

            testObject.AddChild(mockChild1.Object);
            testObject.AddChild(mockChild2.Object);

            Assert.Equal(Status.Failure, testObject.Tick(time));

            mockChild1.Verify(m => m.Tick(time), Times.Once());
            mockChild2.Verify(m => m.Tick(time), Times.Once());
        }
        
        [Fact]
        public void redoes_conditions_when_continuing_to_running_node()
        {
            testObject = new SequenceNode("some-sequence", 0, false);

            var time = new TimeData(0);

            //Condition 1
            var mockChild1 = new Mock<BehaviourTreeNode>();
            mockChild1.Object.IsCondition = true;
            mockChild1
                .Setup(node => node.Tick(time))
                .Returns(Status.Success);

            var mockChild5 = new Mock<BehaviourTreeNode>();
            mockChild5
                .Setup(node => node.Tick(time))
                .Returns(Status.Success);

            //Condition 2
            var mockChild2 = new Mock<BehaviourTreeNode>();
            mockChild2.Object.IsCondition = true;
            mockChild2
                .Setup(m => m.Tick(time))
                .Returns(Status.Success);

            var mockChild3 = new Mock<BehaviourTreeNode>();
            mockChild3
                .SetupSequence(node => node.Tick(time))
                .Returns(Status.Running)
                .Returns(Status.Success);

            var mockChild4 = new Mock<BehaviourTreeNode>();
            mockChild4
                .Setup(m => m.Tick(time))
                .Returns(Status.Success);

            testObject.AddChild(mockChild1.Object);
            testObject.AddChild(mockChild2.Object);
            testObject.AddChild(mockChild3.Object);
            testObject.AddChild(mockChild4.Object);
            testObject.AddChild(mockChild5.Object);

            Assert.Equal(Status.Running, testObject.Tick(time));
            Assert.Equal(Status.Success, testObject.Tick(time));

            mockChild1.Verify(m => m.Tick(time), Times.Exactly(2));
            mockChild2.Verify(m => m.Tick(time), Times.Exactly(2));
            mockChild3.Verify(m => m.Tick(time), Times.Exactly(2));
            mockChild4.Verify(m => m.Tick(time), Times.Once());
            mockChild5.Verify(m => m.Tick(time), Times.Once());
        }
    }
}
