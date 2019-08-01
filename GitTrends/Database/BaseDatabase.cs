﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using SQLite;
using Xamarin.Essentials;

namespace GitTrends
{
    public abstract class BaseDatabase
    {
        #region Constant Fields
        static readonly string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, $"{nameof(GitTrends)}.db3");

        static readonly Lazy<SQLiteAsyncConnection> _databaseConnectionHolder =
            new Lazy<SQLiteAsyncConnection>(() => new SQLiteAsyncConnection(DatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache));
        #endregion

        #region Properties
        static SQLiteAsyncConnection DatabaseConnection => _databaseConnectionHolder.Value;
        #endregion

        #region Methods
        protected static async ValueTask<SQLiteAsyncConnection> GetDatabaseConnectionAsync<T>()
        {
            if (!DatabaseConnection.TableMappings.Any(x => x.MappedType.Name == typeof(T).Name))
            {
                await DatabaseConnection.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
                await DatabaseConnection.CreateTablesAsync(CreateFlags.None, typeof(T)).ConfigureAwait(false);
            }

            return DatabaseConnection;
        }

        protected static Task<T> ExecutePollyFunction<T>(Func<Task<T>> action, int numRetries = 3)
        {
            return Policy.Handle<Exception>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).ExecuteAsync(action);

            TimeSpan pollyRetryAttempt(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
        }
        #endregion
    }
}
