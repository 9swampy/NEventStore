﻿namespace NEventStore
{
    using System;
    using NEventStore.Dispatcher;
    using NEventStore.Persistence.AcceptanceTests.BDD;
    using Xunit;
    using Xunit.Should;

    public class SynchrounousDispatcherSchedulerWireupTests
    {
        public class when_configured_to_auto_start_by_default : SpecificationBase
        {
            private IStoreEvents _eventStore;
            private DummyDispatchCommits _dummyDispatchCommits;

            protected override void Context()
            {
                _dummyDispatchCommits = new DummyDispatchCommits();
                _eventStore = Wireup
                    .Init()
                    .UsingInMemoryPersistence()
                    .UsingAsynchronousDispatchScheduler(_dummyDispatchCommits)
                    .Build();
            }

            protected override void Because()
            {
                using (var stream = _eventStore.OpenStream(Guid.NewGuid()))
                {
                    stream.Add(new EventMessage {Body = "Body"});
                    stream.CommitChanges(Guid.NewGuid());
                }
            }

            protected override void Cleanup()
            {
                _eventStore.Dispose();
            }

            [Fact]
            public void should_dispatch_event()
            {
                _dummyDispatchCommits.Dispatched.ShouldBeTrue();
            }
        }

        public class when_configured_to_auto_start_explicitly_and_not_started : SpecificationBase
        {
            private IStoreEvents _eventStore;
            private DummyDispatchCommits _dummyDispatchCommits;

            protected override void Context()
            {
                _dummyDispatchCommits = new DummyDispatchCommits();
                _eventStore = Wireup
                    .Init()
                    .UsingInMemoryPersistence()
                    .UsingAsynchronousDispatchScheduler(_dummyDispatchCommits, DispatcherSchedulerStartup.Explicit)
                    .Build();
            }

            protected override void Because()
            {
                using (var stream = _eventStore.OpenStream(Guid.NewGuid()))
                {
                    stream.Add(new EventMessage {Body = "Body"});
                    stream.CommitChanges(Guid.NewGuid());
                }
            }

            protected override void Cleanup()
            {
                _eventStore.Dispose();
            }

            [Fact]
            public void should_not_dispatch_event()
            {
                _dummyDispatchCommits.Dispatched.ShouldBeFalse();
            }
        }

        public class when_configured_to_auto_start_explicitly_and_started : SpecificationBase
        {
            private IStoreEvents _eventStore;
            private DummyDispatchCommits _dummyDispatchCommits;

            protected override void Context()
            {
                _dummyDispatchCommits = new DummyDispatchCommits();
                _eventStore = Wireup
                    .Init()
                    .UsingInMemoryPersistence()
                    .UsingAsynchronousDispatchScheduler(_dummyDispatchCommits, DispatcherSchedulerStartup.Explicit)
                    .Build();
            }

            protected override void Because()
            {
                using (var stream = _eventStore.OpenStream(Guid.NewGuid()))
                {
                    stream.Add(new EventMessage {Body = "Body"});
                    stream.CommitChanges(Guid.NewGuid());
                }
                _eventStore.StartDispatchScheduler();
            }

            protected override void Cleanup()
            {
                _eventStore.Dispose();
            }

            [Fact]
            public void should_dispatch_event()
            {
                _dummyDispatchCommits.Dispatched.ShouldBeTrue();
            }
        }

        private class DummyDispatchCommits : IDispatchCommits
        {
            private bool _dispatched;

            public bool Dispatched
            {
                get { return _dispatched; }
            }

            public void Dispose()
            {}

            public void Dispatch(ICommit commit)
            {
                _dispatched = true;
            }
        }
    }
}