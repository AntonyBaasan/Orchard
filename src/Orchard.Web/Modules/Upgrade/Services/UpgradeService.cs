﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Logging;

namespace Upgrade.Services {
    public class UpgradeService : IUpgradeService {
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private readonly ShellSettings _shellSettings;

        public UpgradeService(ISessionFactoryHolder sessionFactoryHolder, ShellSettings shellSettings) {
            _sessionFactoryHolder = sessionFactoryHolder;
            _shellSettings = shellSettings;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void CopyTable(string fromTableName, string toTableName, string[] ignoreColumns) {
            var fromPrefixedTableName = GetPrefixedTableName(fromTableName);
            var toPrefixedTableName = GetPrefixedTableName(toTableName);

            var selectStatement = String.Format(@"SELECT * FROM {0}", fromPrefixedTableName);
            var values = new List<IDictionary<string, object>>();

            ExecuteReader(selectStatement, (reader, conn) => {
                var parameters = new SortedDictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++) {
                    parameters.Add(reader.GetName(i), reader.GetValue(i));
                }
                
                values.Add(parameters);
            });

            var sessionFactory = _sessionFactoryHolder.GetSessionFactory();
            var session = sessionFactory.OpenSession();

            var connection = session.Connection;

            foreach (var record in values) {
                var command = connection.CreateCommand();
                var statement = String.Format("INSERT INTO {0} (", toPrefixedTableName);

                foreach (var keyValuePair in record) {
                    if (ignoreColumns.Contains(keyValuePair.Key)) {
                        continue;
                    }

                    statement += keyValuePair.Key;
                    if (keyValuePair.Key != record.Last().Key) {
                        statement += ", ";
                    }
                }

                statement += ") VALUES ( ";

                foreach (var keyValuePair in record) {
                    if (ignoreColumns.Contains(keyValuePair.Key)) {
                        continue;
                    }

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@" + keyValuePair.Key;
                    parameter.Value = keyValuePair.Value;

                    statement += parameter.ParameterName;
                    if (keyValuePair.Key != record.Last().Key) {
                        statement += ", ";
                    }

                    command.Parameters.Add(parameter);
                }

                statement += ")";

                command.CommandText = statement;

                command.ExecuteNonQuery();                
            }
        }

        public void ExecuteReader(string sqlStatement, Action<IDataReader, IDbConnection> action) {
            var sessionFactory = _sessionFactoryHolder.GetSessionFactory();
            var session = sessionFactory.OpenSession();

            var command = session.Connection.CreateCommand();
            command.CommandText = string.Format(sqlStatement);
                
            var reader = command.ExecuteReader();

            while (reader != null && reader.Read()) {
                try {
                    action(reader, session.Connection);
                }
                catch (Exception e) {
                    Logger.Error(e, "Error while executing custom SQL Statement in Upgrade.");
                }
            }

            if (reader != null && !reader.IsClosed) {
                reader.Close();
            }
        }

        public string GetPrefixedTableName(string tableName) {
            if (string.IsNullOrWhiteSpace(_shellSettings.DataTablePrefix)) {
                return tableName;
            }

            return _shellSettings.DataTablePrefix + "_" + tableName;
        }
    }
}