using System;
using DatabasePerformanceTest.DB.MS_SQL;
using DatabasePerformanceTest.DB.MongoDB;

namespace DatabasePerformanceTest
{
    class Program
    {
        private static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"Read {nameof(AppConfig)}.json ...");
            AppConfig appConfig = AppConfig.Read();

            // Hold Random Data in Array of DataModel
            Console.WriteLine("Create Random Data...");
            Tools.RandomDataBuilder(appConfig.PersonCount, appConfig.EachPersonValueCount, out var dataModels);

            /*
             * MS SQL Server (Multi Query Mode)
             */
            MsSql.SingleQuery = false;
            if (!MsSql.Init(appConfig.MsSqlConnectionString, out var msDbCtx)) return;
            var msSqlInsertM = MsSql.Create(msDbCtx, dataModels);
            var msSqlSelectM = MsSql.Read(msDbCtx, out var allPersons);
            var msSqlUpdateM = MsSql.Update(msDbCtx, allPersons);
            var msSqlDeleteM = MsSql.Delete(msDbCtx, allPersons);
            // Close Connection
            msDbCtx.Dispose();

            /*
             * MS SQL Server (Single Query Mode)
             */
            MsSql.SingleQuery = true;
            if (!MsSql.Init(appConfig.MsSqlConnectionString, out msDbCtx)) return;
            var msSqlInsertS = MsSql.Create(msDbCtx, dataModels);
            var msSqlSelectS = MsSql.Read(msDbCtx, out allPersons);
            var msSqlUpdateS = MsSql.Update(msDbCtx, allPersons);
            var msSqlDeleteS = MsSql.Delete(msDbCtx, allPersons);
            // Close Connection
            msDbCtx.Dispose();

            /*
             * MongoDB (Multi Query Mode)
             */
            MongoDb.SingleQuery = false;
            if (!MongoDb.Init(appConfig.MongoDbConnectionString, appConfig.MongoDbDatabaseName, out var mongoDb)) return;
            var mongoInsertM = MongoDb.Create(mongoDb, dataModels);
            var mongoSelectM = MongoDb.Read(mongoDb, out var persons, out var personsData);
            var mongoUpdateM = MongoDb.Update(mongoDb, persons, personsData);
            var mongoDeleteM = MongoDb.Delete(mongoDb, persons, personsData);

            /*
             * MongoDB (Single Query Mode)
             */
            MongoDb.SingleQuery = true;
            if (!MongoDb.Init(appConfig.MongoDbConnectionString, appConfig.MongoDbDatabaseName, out mongoDb)) return;
            var mongoInsertS = MongoDb.Create(mongoDb, dataModels);
            var mongoSelectS = MongoDb.Read(mongoDb, out persons, out personsData);
            var mongoUpdateS = MongoDb.Update(mongoDb, persons, personsData);
            var mongoDeleteS = MongoDb.Delete(mongoDb, persons, personsData);

            Console.Write("\nThe Output Format Is In ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Seconds\n");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Database    Create     Read       Update     Delete");
            Console.WriteLine("----------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"MS SQL      {msSqlInsertM.TF()}    {msSqlSelectM.TF()}    {msSqlUpdateM.TF()}    {msSqlDeleteM.TF()}");
            Console.WriteLine($"MongoDB     {mongoInsertM.TF()}    {mongoSelectM.TF()}    {mongoUpdateM.TF()}    {mongoDeleteM.TF()}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"MS SQL      {msSqlInsertS.TF()}    {msSqlSelectS.TF()}    {msSqlUpdateS.TF()}    {msSqlDeleteS.TF()}");
            Console.WriteLine($"MongoDB     {mongoInsertS.TF()}    {mongoSelectS.TF()}    {mongoUpdateS.TF()}    {mongoDeleteS.TF()}");

            while (true)
                Console.ReadKey(true);
        }
    }
}
