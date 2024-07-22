using SqlMigrate;
using System.Data;

class Program
{
    static void Main()
    {
        try
        {
            MigrateQuery migrateQuery = new();
            string query = "SELECT * FROM tb_mhs ORDER BY id";

            DataTable mysqlData = migrateQuery.GetDataFromMySql(query);
            migrateQuery.InsertTbMahasiswa(mysqlData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
