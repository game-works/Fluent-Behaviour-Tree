using FluentBehaviourTree;
using Moq;
using Xunit;

namespace tests
{
    public class SelectorNodeTests
    {
        SelectorNode testObject;

        void Init()
        {
            testObject = new SelectorNode("some-selector", 0);
        }

        [Fact]
        public void runs_the_first_node_if_it_succeeds()
        {
            Init();

            var time = new TimeData(0);

            var mockChild1 = new Mock<BehaviourTreeNode>();
            mockChild1
                .Setup(m => m.Tick(time))
                .Returns(Status.Success);

            var mockChild2 = new Mock<BehaviourTreeNode>();

            testObject.AddChild(mockChild1.Object);
            testObject.AddChild(mockChild2.Object);

            Assert.Equal(Status.Success, testObject.Tick(time));

            mockChild1.Verify(m => m.Tick(time), Times.Once());
            mockChild2.Verify(m => m.Tick(time), Times.Never());
        }

        [Fact]
        public void stops_on_the_first_node_when_it_is_running()
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
        public void skip_nodes_until_the_one_that_is_running()
        {
            Init();

            var time = new TimeData(0);

            var mockChild1 = new Mock<BehaviourTreeNode>();
            mockChild1
                .Setup(m => m.Tick(time))
                .Returns(Status.Failure);

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
            mockChild3.Verify(m => m.Tick(time), Times.Never);
        }

        [Fact]
        public void skip_nodes_until_the_one_that_is_running_and_continues_if_it_fails()
        {
            Init();

            var time = new TimeData(0);

            var mockChild1 = new Mock<BehaviourTreeNode>();
            mockChild1
                .Setup(m => m.Tick(time))
                .Returns(Status.Failure);

            var mockChild2 = new Mock<BehaviourTreeNode>();
            mockChild2
                .SetupSequence(node => node.Tick(time))
                .Returns(Status.Running)
                .Returns(Status.Failure);

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
        public void runs_the_second_node_if_the_first_fails()
        {
            Init();

            var time = new TimeData(0);

            var mockChild1 = new Mock<BehaviourTreeNode>();
            mockChild1
                .Setup(m => m.Tick(time))
                .Returns(Status.Failure);

            var mockChild2 = new Mock<BehaviourTreeNode>();
            mockChild2
                .Setup(m => m.Tick(time))
                .Returns(Status.Success);

            testObject.AddChild(mockChild1.Object);
            testObject.AddChild(mockChild2.Object);

            Assert.Equal(Status.Success, testObject.Tick(time));

            mockChild1.Verify(m => m.Tick(time), Times.Once());
            mockChild2.Verify(m => m.Tick(time), Times.Once());
        }

        [Fact]
        public void fails_when_all_children_fail()
        {
            Init();

            var time = new TimeData(0);

            var mockChild1 = new Mock<BehaviourTreeNode>();
            mockChild1
                .Setup(m => m.Tick(time))
                .Returns(Status.Failure);

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

    }
}

