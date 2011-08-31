namespace EventStore.Persistence.SqlPersistence.SqlDialects
{
	public class PostgreSqlDialect : CommonSqlDialect
	{
		public override string InitializeStorage
		{
			get { return PostgreSqlStatements.InitializeStorage; }
		}
		public override string GetStreamsRequiringSnapshots
		{
			get { return Paged(base.GetStreamsRequiringSnapshots); }
		}

		public override string GetCommitsFromInstant
		{
			get { return Paged(base.GetCommitsFromInstant); }
		}
		public override string GetCommitsFromStartingRevision
		{
			get { return Paged(base.GetCommitsFromStartingRevision); }
		}

		public override string GetUndispatchedCommits
		{
			get { return Paged(base.GetUndispatchedCommits.Replace("0", "false")); }
		}
		public override string MarkCommitAsDispatched
		{
			get { return base.MarkCommitAsDispatched.Replace("1", "true"); }
		}

		public override bool CanPage
		{
			get { return true; }
		}

		private static string Paged(string statement)
		{
			return statement.Replace("LIMIT @Skip, @Limit;", "LIMIT @Limit OFFSET @Skip;");
		}
	}
}