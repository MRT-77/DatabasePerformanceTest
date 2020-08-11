using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace DatabasePerformanceTest
{
    public class AppConfig
    {
        public static AppConfig Default()
        {
            return new AppConfig
            {
                PersonCount = 50,
                EachPersonValueCount = 100,
                MsSqlConnectionString = @"Data Source=.\SQL19;Initial Catalog=PerformanceTestDB;Integrated Security=True",
                MongoDbConnectionString = "mongodb://localhost:27017",
                MongoDbDatabaseName = "PerformanceTestDB"
            };
        }

        public static AppConfig Read()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, nameof(AppConfig) + ".json");

            if (File.Exists(path)) return JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(path));

            var config = Default();
            File.WriteAllText(path, JsonConvert.SerializeObject(config, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            }));
            return config;
        }

        public int PersonCount { get; set; }
        public int EachPersonValueCount { get; set; }
        public string MsSqlConnectionString { get; set; }
        public string MongoDbConnectionString { get; set; }
        public string MongoDbDatabaseName { get; set; }
    }
}
