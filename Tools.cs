using DatabasePerformanceTest.DB;
using System;

namespace DatabasePerformanceTest
{
    public static class Tools
    {
        public static TimeSpan RandomDataBuilder(int persons, int values, out DataModel[] dataModels)
        {
            dataModels = new DataModel[persons];
            var rand = new Random(DateTime.Now.Millisecond);

            var dtStartCreateData = DateTime.Now;
            // Start to Creating Random Data

            for (int i = 0; i < persons; i++)
            {
                //string name;
                //do
                //{
                //} while (dataModels.Any(t => t != null && t.Name == name));

                string name = "";
                for (int j = 0; j < 6; j++)
                    name += (char)rand.Next('a', 'z');

                dataModels[i] = new DataModel
                {
                    Name = name,
                    Values = new double[values]
                };

                for (int j = 0; j < values; j++)
                    dataModels[i].Values[j] = rand.NextDouble();
            }

            return DateTime.Now - dtStartCreateData;
        }

        // Custom Time Format
        public static string TF(this long milliseconds)
        {
            return (milliseconds / 1000.0).ToString("000.000");
        }
    }
}
