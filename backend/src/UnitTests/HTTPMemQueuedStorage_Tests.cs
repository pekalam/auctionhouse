using System;
using System.Collections.Generic;
using System.Text;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Command;
using Core.Common.EventBus;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests.HTTPMemQueuedStorage
{
    public class TestCommandBase : CommandBase
    {
        public int Param { get; set; }
    }

    [TestFixture]
    public class HTTPMemQueuedStorage_Tests
    {
        private HTTPMemQueuedCommandStatusStorage _commandStatusStorage;

        [SetUp]
        public void SetUp()
        {
            _commandStatusStorage = new HTTPMemQueuedCommandStatusStorage();
        }

        [Test]
        public void SaveStatus_saves_and_get_status_returns_it()
        {
            var cmd = new TestCommandBase();
            var requestStatus1 = new RequestStatus(Status.COMPLETED);
            var requestStatus2 = new RequestStatus(Status.FAILED);
            _commandStatusStorage.SaveStatus(requestStatus1, cmd);
            _commandStatusStorage.SaveStatus(requestStatus2, cmd);

            _commandStatusStorage.GetCommandStatus(requestStatus1.CorrelationId).Item1.Should()
                .BeEquivalentTo(requestStatus1);
            _commandStatusStorage.GetCommandStatus(requestStatus2.CorrelationId).Item1.Should()
                .BeEquivalentTo(requestStatus2);
        }

        [Test]
        public void UpdateStatus_updates_saved_status()
        {
            var cmd = new TestCommandBase();
            var requestStatus1 = new RequestStatus(Status.PENDING);
            _commandStatusStorage.SaveStatus(requestStatus1, cmd);

            var upd = new RequestStatus(requestStatus1.CorrelationId, Status.COMPLETED);
            cmd.Param = 100;
            _commandStatusStorage.UpdateCommandStatus(upd, cmd);

            var updated = _commandStatusStorage.GetCommandStatus(upd.CorrelationId);

            updated.Item1.Should().BeEquivalentTo(upd);
        }

        [Test]
        public void GetStatus_when_status_does_not_exist_returns_null()
        {
            var result = _commandStatusStorage.GetCommandStatus(new CorrelationId("123"));
            result.Item1.Should().BeNull();
            result.Item2.Should().BeNull();
        }

        [Test]
        public void GetStatus_when_returned_status_is_completed_removes_it_from_storage()
        {
            var cmd = new TestCommandBase();
            var requestStatus1 = new RequestStatus(Status.COMPLETED);
            _commandStatusStorage.SaveStatus(requestStatus1, cmd);

            _commandStatusStorage.GetCommandStatus(requestStatus1.CorrelationId).Item1.Should()
                .BeEquivalentTo(requestStatus1);
            _commandStatusStorage.GetCommandStatus(requestStatus1.CorrelationId).Item1.Should().BeNull();
        }

        [Test]
        public void GetStatus_when_returned_status_is_failed_removes_it_from_storage()
        {
            var cmd = new TestCommandBase();
            var requestStatus1 = new RequestStatus(Status.FAILED);
            _commandStatusStorage.SaveStatus(requestStatus1, cmd);

            _commandStatusStorage.GetCommandStatus(requestStatus1.CorrelationId).Item1.Should()
                .BeEquivalentTo(requestStatus1);
            _commandStatusStorage.GetCommandStatus(requestStatus1.CorrelationId).Item1.Should().BeNull();
        }
    }
}