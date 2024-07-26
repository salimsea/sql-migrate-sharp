using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using Npgsql;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SqlMigrate.Helpers;
using System.Data;

namespace SqlMigrate
{
    class MigrateQuery
    {
        private readonly ConnectionHelper common = new();

        public void UploadExcelToSql(string excelFilePath, string tableName)
        {
            try
            {
                using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new XSSFWorkbook(stream);
                    ISheet sheet = workbook.GetSheetAt(0);
                    DataTable dataTable = new DataTable();

                    IRow headerRow = sheet.GetRow(0);
                    for (int i = 0; i < headerRow.LastCellNum; i++)
                    {
                        dataTable.Columns.Add(headerRow.GetCell(i).ToString());
                    }

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        DataRow dataRow = dataTable.NewRow();
                        for (int j = 0; j < row.LastCellNum; j++)
                        {
                            dataRow[j] = row.GetCell(j).ToString();
                        }
                        dataTable.Rows.Add(dataRow);
                    }

                    using SqlConnection connection = common.SqlServerDbConnection;
                    connection.Open();
                    using (SqlBulkCopy bulkCopy = new(connection))
                    {
                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.WriteToServer(dataTable);
                    }
                }

                Console.WriteLine("Upload berhasil.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Terjadi kesalahan: {ex.Message}");
            }
        }

        #region MYSQL
        private static DataTable MappingColumnMySql(MySqlCommand cmd)
        {
            DataTable dataTable = new();

            using MySqlDataReader reader = cmd.ExecuteReader();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }

            while (reader.Read())
            {
                DataRow row = dataTable.NewRow();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    object value = reader.GetValue(i);
                    if (value.ToString() is "01/01/0001 00:00:00")
                    {
                        row[i] = new MySqlDateTime(new DateTime(1900, 1, 1));
                    }
                    else if (value.ToString() is "00/00/0000 00:00:00")
                    {
                        row[i] = new MySqlDateTime(new DateTime(1900, 1, 1));
                    }
                    else if (value.ToString() is "00/00/0000")
                    {
                        row[i] = new MySqlDateTime(new DateTime(1900, 1, 1));
                    }
                    else
                    {
                        row[i] = value == DBNull.Value ? DBNull.Value : value;
                    }
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
        public DataTable GetDataFromMySql(string query)
        {
            Console.WriteLine($"[START] EXECUTE GET QUERY : {query}");
            using MySqlConnection mysqlConn = common.MySqlDbConnection;
            mysqlConn.Open();
            MySqlCommand cmd = new(query, mysqlConn)
            {
                CommandTimeout = 600
            };
            DataTable dataTable = MappingColumnMySql(cmd);
            return dataTable;
        }
        #endregion

        #region POSTGRESQL
        private static DataTable MappingColumnPgSql(NpgsqlCommand cmd)
        {
            DataTable dataTable = new();
            using NpgsqlDataReader reader = cmd.ExecuteReader();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }

            while (reader.Read())
            {
                DataRow row = dataTable.NewRow();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetFieldType(i) == typeof(DateTime))
                    {
                        var pgsqlDate = reader.GetValue(i);
                        if (pgsqlDate != DBNull.Value && DateTime.TryParse(pgsqlDate.ToString(), out DateTime parsedDate))
                        {
                            if (parsedDate > DateTime.MinValue)
                            {
                                row[i] = parsedDate;
                            }
                            else
                            {
                                row[i] = DBNull.Value;
                            }
                        }
                        else
                        {
                            row[i] = DBNull.Value;
                        }
                    }
                    else
                    {
                        row[i] = reader.GetValue(i);
                    }
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
        public DataTable GetDataFromPgSql(string query)
        {
            Console.WriteLine($"[START] EXECUTE GET QUERY : {query}");
            using NpgsqlConnection pgsqlConn = common.PostgreSqlDbConnection;
            pgsqlConn.Open();
            NpgsqlCommand cmd = new(query, pgsqlConn);
            DataTable dataTable = MappingColumnPgSql(cmd);
            return dataTable;
        }
        #endregion

        #region SQLSERVER
        public void InsertDataDynamic(DataTable dataTable,
                                  string insertQuery,
                                  string[] parameters,
                                  Dictionary<string, bool> dateTimeColumns,
                                  Dictionary<string, bool> intColumns,
                                  string identityTable)
        {
            using SqlConnection sqlConn = common.SqlServerDbConnection;
            sqlConn.Open();

            if (!string.IsNullOrEmpty(identityTable))
            {
                string enableIdentityInsert = $"SET IDENTITY_INSERT {identityTable} ON";
                using SqlCommand enableCmd = new(enableIdentityInsert, sqlConn);
                enableCmd.ExecuteNonQuery();
            }

            foreach (DataRow row in dataTable.Rows)
            {
                var msgId = row[0]?.ToString() ?? "Unknown ID";
                try
                {
                    using SqlCommand cmd = new SqlCommand(insertQuery, sqlConn);

                    foreach (var param in parameters)
                    {
                        object value = row[param];
                        if (dateTimeColumns.ContainsKey(param) && dateTimeColumns[param])
                        {
                            value = DBNull.Value.Equals(value) ? (object)DBNull.Value : Convert.ToDateTime(value);
                        }
                        else if (intColumns.ContainsKey(param) && intColumns[param])
                        {
                            value = DBNull.Value.Equals(value) ? (object)DBNull.Value : Convert.ToInt32(value);
                        }
                        cmd.Parameters.AddWithValue($"@{param}", value == DBNull.Value ? (object)DBNull.Value : value);
                    }

                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"[SUCCESS] EXECUTE ROW : {msgId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] FAILED TO EXECUTE ROW : {msgId}. \n Error: {ex.Message}");
                }
            }

            if (!string.IsNullOrEmpty(identityTable))
            {
                string disableIdentityInsert = $"SET IDENTITY_INSERT {identityTable} OFF";
                using SqlCommand disableCmd = new(disableIdentityInsert, sqlConn);
                disableCmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region TESTER
        public void InsertTbMahasiswa(DataTable dataTable)
        {
            string identityScript = string.Empty;
            string insertQuery = @" INSERT INTO dbo.tb_mahasiswa (id, nama, kelas, prodi, npm, createddate)
                                    VALUES (@id, @nama, @kelas, @prodi, @npm, @createddate)";

            string[] parameters =
            {
                "id", "nama", "kelas", "prodi", "npm", "createddate"
            };

            Dictionary<string, bool> dateTimeColumns = new()
            {
                { "createddate", true }
            };

            Dictionary<string, bool> intColumns = new();

            InsertDataDynamic(dataTable, insertQuery, parameters, dateTimeColumns, intColumns, identityScript);
        }
        #endregion


    }
}
