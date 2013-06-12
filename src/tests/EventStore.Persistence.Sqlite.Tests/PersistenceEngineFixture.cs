﻿namespace EventStore.Persistence.AcceptanceTests
{
    using Serialization;
    using SqlPersistence;
    using SqlPersistence.SqlDialects;

    public partial class PersistenceEngineFixture
    {
        public PersistenceEngineFixture()
        {
            this.createPersistence = () => 
                new SqlPersistenceFactory(
                    new ConfigurationConnectionFactory("EventStore.Persistence.AcceptanceTests.Properties.Settings.SQLite"),
                    new BinarySerializer(),
                    new SqliteDialect()).Build();
        }
    }
}