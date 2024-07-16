using SqlMigrate;
using System.Data;

class Program
{
    static void Main()
    {
        try
        {
            MigrateQuery migrateQuery = new();
            // string query = "SELECT * FROM t_cuti ORDER BY ct_id LIMIT 100000 OFFSET 200000";
            string query = "SELECT * FROM t_cuti ORDER BY ct_id";

            DataTable mysqlData = migrateQuery.GetDataFromMySql(query);
            migrateQuery.InsertHris_TCuti(mysqlData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
