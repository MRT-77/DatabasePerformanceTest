using System;
using System.Diagnostics;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace DatabasePerformanceTest.DB.MongoDB
{
    public static class MongoDb
    {
        public static bool SingleQuery = false;

        public static bool Init(string cString, string dbName, out IMongoDatabase mongoDb)
        {
            Console.WriteLine("\nConnect to MongoDB Server...");
            try
            {
                // Create Database and All Tables
                mongoDb = new MongoClient(cString).GetDatabase(dbName);

                // Remove Collections If Exist
                mongoDb.DropCollection(nameof(PersonData), nameof(Person));

                // Create Collection
                mongoDb.CreateCollection(nameof(PersonData));
                // Add Schema Validator
                var personDataSchema = new
                {
                    collMod = nameof(PersonData),
                    validator = new BsonDocument("$jsonSchema", new
                    {
                        bsonType = "object",
                        required = new[] { nameof(PersonData.PersonId), nameof(PersonData.Value) },
                        properties = new
                        {
                            PersonId = new { bsonType = "objectId" },
                            Value = new { bsonType = "double" }
                        }
                    }.ToBsonDocument())
                }.ToBsonDocument();
                mongoDb.RunCommand<BsonDocument>(personDataSchema);

                // Create Collection
                mongoDb.CreateCollection(nameof(Person));
                // Add Schema Validator
                var personSchema = new
                {
                    collMod = nameof(Person),
                    validator = new BsonDocument("$jsonSchema", new
                    {
                        bsonType = "object",
                        required = new[] { nameof(Person.Name) },
                        properties = new
                        {
                            Name = new { bsonType = "string" }
                        }
                    }.ToBsonDocument())
                }.ToBsonDocument();
                mongoDb.RunCommand<BsonDocument>(personSchema);
            }
            catch (Exception e)
            {
                // Show Error Message and Exit
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ReadKey();

                mongoDb = null;
                return false;
            }

            Console.Write("Ready For ");
            if (SingleQuery)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Single");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Multi");
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" Query Mode CRUD\n");
            return true;
        }

        public static long Create(IMongoDatabase db, DataModel[] dataModels)
        {
            Console.WriteLine("Create...");
            var stopwatch = new Stopwatch();
            var person = db.GetCollection<Person>();
            var pData = db.GetCollection<PersonData>();
            // Start to Insert Data
            if (SingleQuery)
            {
                stopwatch.Start();

                var persons = dataModels.Select(s => new Person { Name = s.Name }).ToArray();
                person.InsertMany(persons);

                for (int i = 0; i < persons.Length; i++)
                    pData.InsertMany(dataModels[i].Values.Select(s => new PersonData { PersonId = persons[i].Id, Value = s }).ToArray());
            }
            else
            {
                stopwatch.Start();
                Array.ForEach(dataModels, item =>
                {
                    var p = new Person { Name = item.Name };
                    person.InsertOne(p);

                    Array.ForEach(item.Values, value =>
                    {
                        pData.InsertOne(new PersonData
                        {
                            PersonId = p.Id,
                            Value = value
                        });
                    });
                });
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        public static long Read(IMongoDatabase db, out Person[] persons, out PersonData[] personsData)
        {
            Console.WriteLine("Read...");
            var person = db.GetCollection<Person>();
            var pData = db.GetCollection<PersonData>();
            var stopwatch = new Stopwatch();
            if (SingleQuery) stopwatch.Start();
            // Get All Data from DB
            persons = person.Find(Builders<Person>.Filter.Empty).ToList().ToArray();
            personsData = pData.Find(Builders<PersonData>.Filter.Empty).ToList().ToArray();
            if (!SingleQuery)
            {
                stopwatch.Start();
                for (int i = 0; i < persons.Length; i++)
                    persons[i] = person.Find(Builders<Person>.Filter.Eq(f => f.Id, persons[i].Id)).Single();
                for (int i = 0; i < personsData.Length; i++)
                    personsData[i] = pData.Find(Builders<PersonData>.Filter.Eq(f => f.Id, personsData[i].Id)).Single();
            }

            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }

        public static long Update(IMongoDatabase db, Person[] persons, PersonData[] personsData)
        {
            Console.WriteLine("Update...");
            var person = db.GetCollection<Person>();
            var pData = db.GetCollection<PersonData>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            // Get All Data from DB
            if (SingleQuery)
            {
                var upd = BsonSerializer.Deserialize<BsonArray>("[{$set:{Name:{$concat:[\"$Name\",\"*\"]}}}]");
               // person.UpdateMany(Builders<Person>.Filter.Empty, upd);

                pData.UpdateMany(Builders<PersonData>.Filter.Empty, Builders<PersonData>.Update.Inc(f => f.Value, 1.1));
            }
            else
            {
                Array.ForEach(persons, p =>
                {
                    person.UpdateOne(Builders<Person>.Filter.Eq(f => f.Id, p.Id),
                        Builders<Person>.Update.Set(f => f.Name, p.Name + "*"));
                });
                Array.ForEach(personsData, pd =>
                {
                    pData.UpdateOne(Builders<PersonData>.Filter.Eq(q => q.Id, pd.Id),
                        Builders<PersonData>.Update.Inc(f => f.Value, 1.1));
                });
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        public static long Delete(IMongoDatabase db, Person[] persons, PersonData[] personsData)
        {
            Console.WriteLine("Delete...");
            var person = db.GetCollection<Person>();
            var pData = db.GetCollection<PersonData>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            // Get All Data from DB
            if (SingleQuery)
            {
                person.DeleteMany(Builders<Person>.Filter.Empty);
                pData.DeleteMany(Builders<PersonData>.Filter.Empty);
            }
            else
            {
                Array.ForEach(personsData, pd =>
                {
                    pData.DeleteOne(Builders<PersonData>.Filter.Eq(q => q.Id, pd.Id));
                });
                Array.ForEach(persons, p =>
                {
                    person.DeleteOne(Builders<Person>.Filter.Eq(f => f.Id, p.Id));
                });
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        public static void DropCollection(this IMongoDatabase db, params string[] collName)
        {
            using var collections = db.ListCollectionNames();
            var names = collections.ToEnumerable().Join(collName, o => o, i => i, (o, i) => i).ToArray();
            Array.ForEach(names, name =>
            {
                db.DropCollection(name);
            });
        }

        public static IMongoCollection<T> GetCollection<T>(this IMongoDatabase db)
        {
            return db.GetCollection<T>(typeof(T).Name);
        }
    }
}
