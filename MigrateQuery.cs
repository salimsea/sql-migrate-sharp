using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using Npgsql;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;

namespace SqlMigrate
{
    class MigrateQuery
    {
        private readonly Common common = new();

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
                        row[i] = new MySqlDateTime(new DateTime(1999, 1, 1));
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
        private void InsertDataDynamic(DataTable dataTable, string insertQuery, string[] parameters, Dictionary<string, bool> dateTimeColumns)
        {
            using SqlConnection sqlConn = common.SqlServerDbConnection;
            sqlConn.Open();

            foreach (DataRow row in dataTable.Rows)
            {
                var msgId = row[0]?.ToString() ?? "Unknown ID";
                try
                {
                    using SqlCommand cmd = new(insertQuery, sqlConn);
                    foreach (var param in parameters)
                    {
                        object value = row[param];
                        if (dateTimeColumns.ContainsKey(param) && dateTimeColumns[param])
                        {
                            value = DBNull.Value.Equals(value) ? (object)DBNull.Value : Convert.ToDateTime(value);
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
        }
        public void InsertHris_TCuti(DataTable dataTable)
        {
            string insertQuery = @"
            INSERT INTO dbo.Hris_TCuti (ct_kode, ct_tran, mk_nopeg, ct_from, ct_to, ct_korin, 
                                        ct_notes, ct_address, ct_create, ct_createby, ct_update, 
                                        ct_updateby, ct_app1_unit, ct_app1_time, ct_app1_by, 
                                        ct_app1_status, ct_app2_unit, ct_app2_time, ct_app2_by, 
                                        ct_app2_status, [status], koreksi, status_sap, jml_hari_cuti, 
                                        mk_nopeg_delegasi)
            VALUES (@ct_kode, @ct_tran, @mk_nopeg, @ct_from, @ct_to, @ct_korin, 
                    @ct_notes, @ct_address, @ct_create, @ct_createby, @ct_update, 
                    @ct_updateby, @ct_app1_unit, @ct_app1_time, @ct_app1_by, 
                    @ct_app1_status, @ct_app2_unit, @ct_app2_time, @ct_app2_by, 
                    @ct_app2_status, @status, @koreksi, @status_sap, @jml_hari_cuti, 
                    @mk_nopeg_delegasi)";

            string[] parameters =
            [
            "ct_kode", "ct_tran", "mk_nopeg", "ct_from", "ct_to", "ct_korin",
            "ct_notes", "ct_address", "ct_create", "ct_createby", "ct_update",
            "ct_updateby", "ct_app1_unit", "ct_app1_time", "ct_app1_by",
            "ct_app1_status", "ct_app2_unit", "ct_app2_time", "ct_app2_by",
            "ct_app2_status", "status", "koreksi", "status_sap", "jml_hari_cuti",
            "mk_nopeg_delegasi"
            ];

            Dictionary<string, bool> dateTimeColumns = new()
            {
                { "ct_from", true },
                { "ct_to", true },
                { "ct_create", true },
                { "ct_update", true },
                { "ct_app1_time", true },
                { "ct_app2_time", true }
            };

            InsertDataDynamic(dataTable, insertQuery, parameters, dateTimeColumns);
        }
        #endregion


    }
}
