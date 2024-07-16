# SQL Migrate Sharp

SQL Migrate Sharp is a .NET library designed to simplify the process of migrating and inserting data into SQL databases. It provides dynamic methods for inserting data into tables, handling different data types, and ensuring robust error handling.

## Features

- **Dynamic Data Insertion**: Insert data dynamically into SQL tables using DataTables.
- **Type Handling**: Automatic handling of `DateTime` types and other common SQL types.
- **Error Handling**: Detailed logging for successful and failed operations.
- **Configurable**: Easily configurable to work with various SQL table schemas and data structures.

## Requirements

- .NET 8.0 or later
- SQL Server or compatible database
- MySQL or compatible database (for MySQL-specific queries)
- PostgreSQL

## Installation

To install SQL Migrate Sharp, clone the repository and include the project in your .NET solution:

```bash
git clone https://github.com/salimsea/sql-migrate-sharp.git
```

Then, add the project to your solution:

```bash
dotnet add reference path/to/sql-migrate-sharp.csproj
```

## Usage

### Configuration

Before using the library, configure the SQL Server connection in your `common.SqlServerDbConnection` class. Ensure it returns a valid `SqlConnection` object.

### Inserting Data

To insert data into the `dbo.Hris_TCuti` table, prepare a `DataTable` with the necessary data and call the `InsertHris_TCuti` method:

```csharp
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

public class DatabaseHelper
{
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

        string[] parameters = new string[]
        {
            "ct_kode", "ct_tran", "mk_nopeg", "ct_from", "ct_to", "ct_korin",
            "ct_notes", "ct_address", "ct_create", "ct_createby", "ct_update",
            "ct_updateby", "ct_app1_unit", "ct_app1_time", "ct_app1_by",
            "ct_app1_status", "ct_app2_unit", "ct_app2_time", "ct_app2_by",
            "ct_app2_status", "status", "koreksi", "status_sap", "jml_hari_cuti",
            "mk_nopeg_delegasi"
        };

        // Define a dictionary to indicate which parameters are DateTime types
        Dictionary<string, bool> dateTimeColumns = new Dictionary<string, bool>
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
}
```

### Customizing Queries

You can use the `InsertDataDynamic` method for different tables by providing the appropriate insert query and parameters.

## MySQL Support

To handle large datasets in MySQL, you can split your queries to fetch rows in batches. For example, to fetch 2000 rows at a time:

```sql
-- Fetch rows 1 to 2000
SELECT * FROM your_table LIMIT 0, 2000;

-- Fetch rows 2001 to 4000
SELECT * FROM your_table LIMIT 2000, 2000;

-- Fetch rows 4001 to 6000
SELECT * FROM your_table LIMIT 4000, 2000;

-- And so on...
```

## Handling Timeout

To set a query timeout in .NET, use the `CommandTimeout` property:

```csharp
using SqlCommand cmd = new SqlCommand(query, sqlConn);
cmd.CommandTimeout = 600; // Timeout set to 10 minutes (600 seconds)
```

## License

SQL Migrate Sharp is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

---

Feel free to contribute, report issues, or suggest features to improve SQL Migrate Sharp. Your feedback is appreciated!
