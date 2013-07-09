namespace NEventStore.Persistence.RavenPersistence.Tests
{
    using System;
    using NEventStore.Persistence.RavenPersistence;

    public class RavenPersistenceEngineFixture : IDisposable
    {
        public RavenPersistenceEngineFixture()
        {
            Persistence = (RavenPersistenceEngine)new InMemoryRavenPersistenceFactory(TestRavenConfig.GetDefaultConfig()).Build();
            Persistence.Initialize();
        }

        public RavenPersistenceEngine Persistence { get; private set; }

        public void Dispose()
        {
            Persistence.Dispose();
        }
    }
}