using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace shared.Model
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

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
            User = new User(); // Assume User has a parameterless constructor
        }
    }
}
