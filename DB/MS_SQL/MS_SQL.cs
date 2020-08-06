using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DatabasePerformanceTest.DB.MS_SQL
{
    public static class MsSql
    {
        public static bool SingleQuery = false;

        public static bool Init(string cString, out MsDbCtx msDbCtx)
        {
            Console.WriteLine("\nConnect to MS SQL Server...");
            // Create EF DbContext
            msDbCtx = new MsDbCtx(cString);
            try
            {
                // Create Database and All Tables
                if (!msDbCtx.Database.EnsureCreated())
                {
                    // Clear Tables if Database Already Exist
                    msDbCtx.Database.ExecuteSqlRaw($"DELETE FROM {nameof(PersonData)}");
                    msDbCtx.Database.ExecuteSqlRaw($"DBCC CHECKIDENT ([{nameof(PersonData)}], RESEED, 0)");
                    msDbCtx.Database.ExecuteSqlRaw($"DELETE FROM {nameof(Person)}");
                    msDbCtx.Database.ExecuteSqlRaw($"DBCC CHECKIDENT ([{nameof(Person)}], RESEED, 0)");
                }
            }
            catch (Exception e)
            {
                // Show Error Message and Exit
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ReadKey();
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

        public static long Create(MsDbCtx db, DataModel[] dataModels)
        {
            Console.WriteLine("Create...");
            var stopwatch = new Stopwatch();
            // Start to Insert Data to MS SQL Server
            if (SingleQuery)
            {
                Person[] inserted = new Person[dataModels.Length];
                stopwatch.Start();

                for (int i = 0; i < dataModels.Length; i++)
                {
                    var p = new Person { Name = dataModels[i].Name };
                    db.Person.Add(p);
                    inserted[i] = p;
                }

                db.SaveChanges();

                for (int i = 0; i < inserted.Length; i++)
                    Array.ForEach(dataModels[i].Values, value =>
                    {
                        db.PersonData.Add(new PersonData
                        {
                            PersonId = inserted[i].Id,
                            Value = value
                        });
                    });

                db.SaveChanges();
            }
            else
            {
                stopwatch.Start();
                Array.ForEach(dataModels, item =>
                {
                    var person = new Person { Name = item.Name };
                    db.Person.Add(person);
                    db.SaveChanges();

                    Array.ForEach(item.Values, value =>
                    {
                        db.PersonData.Add(new PersonData
                        {
                            PersonId = person.Id,
                            Value = value
                        });
                        db.SaveChanges();
                    });
                });
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        public static long Read(MsDbCtx db, out Person[] allPersons)
        {
            Console.WriteLine("Read...");
            var stopwatch = new Stopwatch();
            // Get All Data from DB
            allPersons = db.Person.Include(i => i.PersonData).ToArray();
            if (!SingleQuery)
            {
                stopwatch.Start();

                Person[] persons = new Person[allPersons.Length];
                for (int i = 0; i < persons.Length; i++)
                {
                    var tP = allPersons[i];
                    persons[i] = db.Person.Single(s => s.Id == tP.Id && s.Name == tP.Name);

                    var vls = tP.PersonData.ToArray();
                    for (int j = 0; j < vls.Length; j++)
                        tP.PersonData.Add(db.PersonData.Single(s => s.Id == vls[j].Id && s.PersonId == vls[j].PersonId && s.Value == vls[j].Value));
                }

                allPersons = persons;
            }

            stopwatch.Stop();

            // Should Never Occurs Error
            foreach (var person in allPersons)
                if (person.PersonData.Count == 0) throw new NullReferenceException(nameof(person.PersonData));

            return stopwatch.ElapsedMilliseconds;
        }

        public static long Update(MsDbCtx db, Person[] allPersons)
        {
            Console.WriteLine("Update...");
            var allPersonData = allPersons.SelectMany(s => s.PersonData).ToArray();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            // Get All Data from DB
            if (SingleQuery)
            {
                Array.ForEach(allPersons, person =>
                {
                    person.Name += "*";
                    Array.ForEach(person.PersonData.ToArray(), data =>
                    {
                        data.Value += 1.1;
                    });
                });
                db.SaveChanges();
            }
            else
            {
                Array.ForEach(allPersons, person =>
                {
                    var p = db.Person.Single(s => s.Id == person.Id);
                    p.Name += "*";
                    db.SaveChanges();
                });
                Array.ForEach(allPersonData, personData =>
                {
                    var p = db.PersonData.Single(s => s.Id == personData.Id);
                    p.Value += 0.1;
                    db.SaveChanges();
                });
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        public static long Delete(MsDbCtx db, Person[] allPersons)
        {
            Console.WriteLine("Delete...");
            var allPersonData = allPersons.SelectMany(s => s.PersonData).ToArray();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            // Get All Data from DB
            if (SingleQuery)
            {
                db.PersonData.RemoveRange(allPersonData);
                db.Person.RemoveRange(allPersons);
                db.SaveChanges();
            }
            else
            {
                Array.ForEach(allPersonData, personData =>
                {
                    var p = db.PersonData.Single(s => s.Id == personData.Id);
                    db.PersonData.Remove(p);
                    db.SaveChanges();
                });
                Array.ForEach(allPersons, person =>
                {
                    var p = db.Person.Single(s => s.Id == person.Id);
                    db.Person.Remove(p);
                    db.SaveChanges();
                });
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
