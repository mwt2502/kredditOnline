using kreddit_app.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using shared.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KredditServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IMongoCollection<Post> _postsCollection;

        public PostsController(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("MongoDB");
            _postsCollection = database.GetCollection<Post>("Posts");
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            var posts = await _postsCollection.Find(_ => true).ToListAsync();
            return Ok(posts.Select(post => new
            {
                Id = post.Id.ToString(), // Convert ObjectId to string
                post.Title,
                post.Content,
                post.CreationTime,
                post.Upvotes,
                post.Downvotes,
                post.User,
                Comments = post.Comments.Select(comment => new
                {
                    Id = comment.Id.ToString(), // Convert ObjectId to string
                    comment.Content,
                    comment.Upvotes,
                    comment.Downvotes,
                    comment.User
                })
            }));
        }



[HttpGet("{postId}")]
public async Task<ActionResult<Post>> GetPost(string postId)
{
    // Prøv at konvertere postId (string) til ObjectId
    if (!ObjectId.TryParse(postId, out var objectId))
    {
        return BadRequest("Invalid post ID format.");
    }

    // Find posten baseret på det konverterede ObjectId
    var post = await _postsCollection.Find(p => p.Id == postId).FirstOrDefaultAsync();

    if (post == null)
    {
        return NotFound();
    }

    return Ok(post);
}

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] Post post)
        {
            if (post == null)
            {
                Console.Error.WriteLine("Post is null");
                return BadRequest("Post cannot be null");
            }

            // Initialize the User if it's null
            if (post.User == null)
            {
                post.User = new User(); // Ensure User is not null
            }

            // Generate Id for Post
            post.Id = ObjectId.GenerateNewId().ToString();
            Console.WriteLine($"Generated Post Id: {post.Id}");

            // Generate Id for User
            post.User.Id = ObjectId.GenerateNewId().ToString();
            Console.WriteLine($"Generated User Id: {post.User.Id}");

            // Generate Ids for comments, if there are any
            foreach (var comment in post.Comments)
            {
                comment.Id = ObjectId.GenerateNewId().ToString();
                if (comment.User != null)
                {
                    comment.User.Id = ObjectId.GenerateNewId().ToString(); // Ensure User is initialized
                    Console.WriteLine($"Generated Comment User Id: {comment.User.Id}");
                }
            }

            try
            {
                await _postsCollection.InsertOneAsync(post);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error inserting post: {ex.Message}");
                return StatusCode(500, "Internal server error while inserting post");
            }

            return CreatedAtAction(nameof(GetPost), new { postId = post.Id }, post);
        }



        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> AddCommentToPost(string postId, [FromBody] Comment comment)
        {
            // Check if postId is valid
            if (!ObjectId.TryParse(postId, out var objectId))
            {
                return BadRequest("Invalid post ID format.");
            }

            // Find the post based on postId
            var post = await _postsCollection.Find(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null)
            {
                return NotFound("Post not found.");
            }

            // Generate a new ID for the comment
            comment.Id = ObjectId.GenerateNewId().ToString(); // Ensure Id is a string

            // If username is empty, set it to "Anonymous"
            if (string.IsNullOrWhiteSpace(comment.User?.Username))
            {
                comment.User = new User { Username = "Anonym" }; // Set anonymous user
            }
            else
            {
                comment.User.Id = ObjectId.GenerateNewId().ToString(); // Convert ObjectId to string
            }

            // Add the comment to the post's comment list
            post.Comments.Add(comment);

            // Update the post in the database
            var updateResult = await _postsCollection.ReplaceOneAsync(p => p.Id == postId, post);
            if (updateResult.ModifiedCount == 0)
            {
                return StatusCode(500, "Failed to update the post with the new comment.");
            }

            // Return a Created status with the new comment
            return CreatedAtAction(nameof(GetPost), new { postId = post.Id }, comment);
        }



        //-- Controllers for Votes --//

        [HttpPost("{postId}/upvote")]
        public async Task<IActionResult> UpvotePost(string postId)
        {
        
            var filter = Builders<Post>.Filter.Eq(post => post.Id, postId);
            var update = Builders<Post>.Update.Inc(post => post.Upvotes, 1);
            var result = await _postsCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return NotFound("Post not found");
            }

            return Ok();
        }


        [HttpPost("{postId}/downvote")]
        public async Task<IActionResult> DownvotePost(string postId)
        {
        
            var filter = Builders<Post>.Filter.Eq(post => post.Id, postId);
            var update = Builders<Post>.Update.Inc(post => post.Downvotes, 1);
            var result = await _postsCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return NotFound("Post not found");
            }

            return Ok();
        }


        //-- Controllers VOTE PÅ COMMENTS (NOT IMPLEMENTED) --//

        [HttpPost("{postId}/comments/{commentId}/upvote")]
        public async Task<IActionResult> UpvoteComment(string postId, string commentId)
        {
            if (!ObjectId.TryParse(postId, out var objectId))
            {
                return BadRequest("Invalid post ID format.");
            }

            // Post-filter bruger string nu
            var postFilter = Builders<Post>.Filter.Eq(p => p.Id, postId);

            // Kommentar-filter bruger string for commentId og sammenligner med Id som string
            var commentFilter = Builders<Post>.Filter.ElemMatch(p => p.Comments, c => c.Id.ToString() == commentId);

            // Opdatering for at inkrementere Upvotes på kommentaren
            var update = Builders<Post>.Update.Inc("Comments.$.Upvotes", 1);
            var result = await _postsCollection.UpdateOneAsync(postFilter & commentFilter, update);

            if (result.ModifiedCount == 0)
            {
                return NotFound("Post or comment not found");
            }
            return Ok();
        }



        [HttpPost("{postId}/comments/{commentId}/downvote")]
        public async Task<IActionResult> DownvoteComment(string postId, string commentId)
        {
            if (!ObjectId.TryParse(postId, out var objectId))
            {
                return BadRequest("Invalid post ID format.");
            }

            // Post-filter bruger string nu, så vi sammenligner postens Id som string
            var postFilter = Builders<Post>.Filter.Eq(p => p.Id, postId);

            // Kommentar-filter bruger string for commentId, sammenligner med IdString
            var commentFilter = Builders<Post>.Filter.ElemMatch(p => p.Comments, c => c.Id.ToString() == commentId);

            // Opdatering for at inkrementere Downvotes på kommentaren
            var update = Builders<Post>.Update.Inc("Comments.$.Downvotes", 1);

            // Kombiner filtrene og opdater
            var result = await _postsCollection.UpdateOneAsync(postFilter & commentFilter, update);

            if (result.ModifiedCount == 0)
            {
                return NotFound("Post or comment not found");
            }
            return Ok();
        }


    }
}