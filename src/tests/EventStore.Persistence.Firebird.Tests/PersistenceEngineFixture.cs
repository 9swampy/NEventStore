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
                    new EnviromentConnectionFactory("Firebird", "FirebirdSql.Data.FirebirdClient"),
                    new BinarySerializer(),
                    new FirebirdSqlDialect()).Build();
        }
    }
}