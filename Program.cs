using SqlMigrate;
using SqlMigrate.Helpers;
using System.Data;

class Program
{
    static void Main()
    {
        try
        {
            using var loading = new LoadingHelper();
            using var stopwatch = new StopwatchHelper();

            MigrateQuery migrateQuery = new();

            string query = "SELECT * FROM t_email";
            DataTable mysqlData = migrateQuery.GetDataFromMySql(query);
            migrateQuery.InsertTbMahasiswa(mysqlData);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] TASK : {ex.Message}");
        }
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
