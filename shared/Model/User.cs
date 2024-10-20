using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace shared.Model
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Username { get; set; }

        public User(string username = "")
        {
            Username = username;
        }

        public User()
        {
            Username = "";
        }
    }
}
