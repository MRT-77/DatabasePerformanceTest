using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DatabasePerformanceTest.DB.MongoDB
{
    public class Person
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
