namespace EventStore.Persistence.SqlPersistence.SqlDialects
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Transactions;
	using Logging;

	public class CommonDbStatement : IDbStatement
	{
		private const int InfinitePageSize = 0;
		private static readonly ILog Logger = LogFactory.BuildLogger(typeof(CommonDbStatement));
		private readonly ISqlDialect dialect;
		private readonly TransactionScope transactionScope;
		private readonly ConnectionScope connectionScope;
		private readonly IDbTransaction transaction;

		protected IDictionary<string, object> Parameters { get; private set; }

		public CommonDbStatement(
			ISqlDialect dialect,
			TransactionScope transactionScope,
			ConnectionScope connectionScope,
			IDbTransaction transaction)
		{
			this.Parameters = new Dictionary<string, object>();

			this.dialect = dialect;
			this.transactionScope = transactionScope;
			this.connectionScope = connectionScope;
			this.transaction = transaction;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			Logger.Verbose(Messages.DisposingStatement);

			if (this.transaction != null)
				this.transaction.Dispose();

			if (this.connectionScope != null)
				this.connectionScope.Dispose();

			if (this.transactionScope != null)
				this.transactionScope.Dispose();
		}

		public virtual void AddParameter(string name, object value)
		{
			Logger.Debug(Messages.AddingParameter, name);
			this.Parameters[name] = this.dialect.CoalesceParameterValue(value);
		}

		public virtual int ExecuteWithoutExceptions(string commandText)
		{
			try
			{
				return this.Execute(commandText);
			}
			catch (Exception)
			{
				Logger.Debug(Messages.ExceptionSuppressed);
				return 0;
			}
		}
		public virtual int Execute(string commandText)
		{
			return this.ExecuteNonQuery(commandText);
		}
		protected virtual int ExecuteNonQuery(string commandText)
		{
			try
			{
				using (var command = this.BuildCommand(commandText))
					return command.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				Logger.Debug(Messages.CommandThrewException, e.GetType());
				if (!this.dialect.IsDuplicate(e))
					throw;

				Logger.Debug(Messages.DuplicateCommit);
				throw new DuplicateCommitException(e.Message, e);
			}
		}

		public virtual IEnumerable<T> ExecuteWithQuery<T>(string queryText, Func<IDataRecord, T> select)
		{
			return this.ExecutePagedQuery(queryText, select, (query, latest) => { }, InfinitePageSize);
		}
		public virtual IEnumerable<T> ExecutePagedQuery<T>(
			string queryText, Func<IDataRecord, T> select, NextPageDelegate<T> onNextPage, int pageSize)
		{
			pageSize = this.dialect.CanPage ? pageSize : InfinitePageSize;
			if (pageSize > 0)
			{
				Logger.Verbose(Messages.MaxPageSize, pageSize);
				this.Parameters.Add(this.dialect.Limit, pageSize);
			}

			var command = this.BuildCommand(queryText);

			try
			{
				return new PagedEnumerationCollection<T>(
					command, select, onNextPage, pageSize, this.transactionScope, this);
			}
			catch (Exception)
			{
				command.Dispose();
				throw;
			}
		}
		protected virtual IDbCommand BuildCommand(string statement)
		{
			Logger.Verbose(Messages.CreatingCommand);
			var command = this.connectionScope.Current.CreateCommand();
			command.Transaction = this.transaction;
			command.CommandText = statement;

			Logger.Verbose(Messages.ClientControlledTransaction, this.transaction != null);
			Logger.Verbose(Messages.CommandTextToExecute, statement);

			this.BuildParameters(command);

			return command;
		}
		protected virtual void BuildParameters(IDbCommand command)
		{
			foreach (var item in this.Parameters)
				this.BuildParameter(command, item.Key, item.Value);
		}
		protected virtual void BuildParameter(IDbCommand command, string name, object value)
		{
			var parameter = command.CreateParameter();
			parameter.ParameterName = name;
			this.SetParameterValue(parameter, value, null);

			Logger.Verbose(Messages.BindingParameter, name, parameter.Value);
			command.Parameters.Add(parameter);
		}
		protected virtual void SetParameterValue(IDataParameter param, object value, DbType? type)
		{
			param.Value = value ?? DBNull.Value;
			param.DbType = type ?? (value == null ? DbType.Binary : param.DbType);
		}
	}
}