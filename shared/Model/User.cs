using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace shared.Model
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId(); // Generer ID automatisk

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
