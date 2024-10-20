using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace shared.Model
{
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonIgnore]
        public string IdString => Id.ToString();

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreationTime { get; set; }

        public int Upvotes { get; set; }
        public int Downvotes { get; set; }

        [BsonIgnore]
        public int VoteScore => Upvotes - Downvotes;

        public User User { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();

        public Post(User user, string title = "", string content = "", int upvotes = 0, int downvotes = 0)
        {
            Title = title;
            Content = content;
            Upvotes = upvotes;
            Downvotes = downvotes;
            User = user;
            CreationTime = DateTime.UtcNow;
        }

        public Post()
        {
            Title = "";
            Content = "";
            Upvotes = 0;
            Downvotes = 0;
            User = new User();
            CreationTime = DateTime.UtcNow;
        }
    }
}
