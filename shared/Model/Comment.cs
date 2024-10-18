using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace shared.Model
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId(); // Generer ID automatisk

        [Required]
        public string Content { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public User User { get; set; }

        public Comment(string content = "", int upvotes = 0, int downvotes = 0, User user = null)
        {
            Content = content;
            Upvotes = upvotes;
            Downvotes = downvotes;
            User = user;
        }

        public Comment()
        {
            Content = "";
            Upvotes = 0;
            Downvotes = 0;
            User = new User(); // Antager, at User har en parameterløs konstruktor
        }
    }
}
