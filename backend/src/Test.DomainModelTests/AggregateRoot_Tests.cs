using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common.Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Core.DomainModelTests
{
    public class TestUpdateEvent : UpdateEvent
    {
        public TestUpdateEvent() : base("testUpd")
        {
        }
    }

    public class TestEvent : Event
    {
        public TestEvent() : base("testEv")
        {
        }
    }

    public class TestUpdateEventGroup : UpdateEventGroup<Guid>
    {
        public TestUpdateEventGroup() : base("testGroup")
        {
        }
    }

    public class TestAggRoot : Common.Domain.Default.AggregateRoot<TestAggRoot, TestUpdateEventGroup>
    {
        public void CallTest()
        {
            AddEvent(new TestEvent());
        }

        public void CallTestUpdate()
        {
            AddUpdateEvent(new TestUpdateEvent());
        }

        protected override void Apply(Event @event)
        {
            throw new NotImplementedException();
        }

        protected override TestUpdateEventGroup CreateUpdateEventGroup()
        {
            return new TestUpdateEventGroup();
        }
    }

    [TestFixture]
    public class AggregateRoot_Tests
    {
        private TestAggRoot aggRoot;

        [SetUp]
        public void SetUp()
        {
            aggRoot = new TestAggRoot();
        }

        [Test]
        public void AddUpdate_event_generates_valid_single_event()
        {
            aggRoot.CallTestUpdate();
            aggRoot.PendingEvents.Count.Should().Be(1);
            aggRoot.PendingEvents.Last().Should().BeOfType<TestUpdateEventGroup>();
            var group = aggRoot.PendingEvents.Last() as TestUpdateEventGroup;
            group.UpdateEvents.Count.Should().Be(1);
            group.AggregateId.Should().Be(aggRoot.AggregateId);
        }

        [Test]
        public void AddUpdate_event_generates_valid_multiple_events()
        {
            aggRoot.CallTest();
            aggRoot.CallTestUpdate();
            aggRoot.CallTestUpdate();


            aggRoot.PendingEvents.Count.Should().Be(2);
            aggRoot.PendingEvents.Last().Should().BeOfType<TestUpdateEventGroup>();
            aggRoot.PendingEvents.First().Should().BeOfType<TestEvent>();
            var group = aggRoot.PendingEvents.Last() as TestUpdateEventGroup;
            group.UpdateEvents.Count.Should().Be(2);
            group.AggVersion.Should().Be(2);
            group.AggregateId.Should().Be(aggRoot.AggregateId);
        }

        [Test]
        public void AddUpdate_event_generates_valid_multiple_events_in_order()
        {
            aggRoot.CallTestUpdate();
            aggRoot.CallTest();
            aggRoot.CallTestUpdate();

            aggRoot.PendingEvents.Count.Should().Be(2);
            aggRoot.PendingEvents.First().Should().BeOfType<TestUpdateEventGroup>();
            var group = aggRoot.PendingEvents.First() as TestUpdateEventGroup;
            group.UpdateEvents.Count.Should().Be(2);
            group.AggVersion.Should().Be(1);
            group.AggregateId.Should().Be(aggRoot.AggregateId);
        }
    }
}
