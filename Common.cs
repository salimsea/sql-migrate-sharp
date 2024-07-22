﻿using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;

namespace SqlMigrate
{
    public class Common
    {
        private readonly string _connectionSqlServerString;
        private readonly string _connectionMySqlString;
        private readonly string _connectionPostgreSqlString;

        public Common()
        {
            _connectionSqlServerString = "Server=khansahanum;Database=db_sintadev;Integrated Security=True;Encrypt=False;TrustServerCertificate=False;";
            _connectionMySqlString = "Server=localhost;Database=todo;User Id=root;Password=;Allow Zero Datetime=true;Convert Zero Datetime=true;";
            _connectionPostgreSqlString = "Server=localhost;Database=todo;User Id=postgres;Password=postgres;";
        }

        public SqlConnection SqlServerDbConnection
        {
            get
            {
                return new SqlConnection(_connectionSqlServerString);
            }
        }
        public MySqlConnection MySqlDbConnection
        {
            get
            {
                return new MySqlConnection(_connectionMySqlString);
            }
        }
        public NpgsqlConnection PostgreSqlDbConnection
        {
            get
            {
                return new NpgsqlConnection(_connectionPostgreSqlString);
            }
        }
    }
}
