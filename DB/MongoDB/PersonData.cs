using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DatabasePerformanceTest.DB.MongoDB
{
    public class PersonData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string PersonId { get; set; }
        public double Value { get; set; }
    }
}
