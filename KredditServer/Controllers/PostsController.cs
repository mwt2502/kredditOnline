using kreddit_app.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            var posts = await _postsCollection.Find(_ => true).ToListAsync();
            return Ok(posts);
        }

        [HttpGet("{postId}")]
        public async Task<ActionResult<Post>> GetPost(string postId)
        {
            if (!ObjectId.TryParse(postId, out var objectId))
            {
                return BadRequest("Invalid post ID format.");
            }

            var post = await _postsCollection.Find(p => p.Id == objectId).FirstOrDefaultAsync();
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

            // Generer Id for Post
            post.Id = ObjectId.GenerateNewId();
            Console.WriteLine($"Generated Post Id: {post.Id}");

            // Generer Id for User
            if (post.User != null)
            {
                post.User.Id = ObjectId.GenerateNewId();
                Console.WriteLine($"Generated User Id: {post.User.Id}");
            }

            // Generer Id'er for kommentarer, hvis der er nogen
            foreach (var comment in post.Comments)
            {
                comment.Id = ObjectId.GenerateNewId();
                if (comment.User != null)
                {
                    comment.User.Id = ObjectId.GenerateNewId();
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

            return CreatedAtAction(nameof(GetPost), new { postId = post.Id.ToString() }, post);
        }



        // ...

        [HttpPost("{postId}/upvote")]
        public async Task<IActionResult> UpvotePost(string postId)
        {
            // Forsøg at konvertere postId til ObjectId
            if (!ObjectId.TryParse(postId, out var objectId))
            {
                return BadRequest("Invalid post ID format.");
            }

            var filter = Builders<Post>.Filter.Eq(post => post.Id, objectId);
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
            // Forsøg at konvertere postId til ObjectId
            if (!ObjectId.TryParse(postId, out var objectId))
            {
                return BadRequest("Invalid post ID format.");
            }

            var filter = Builders<Post>.Filter.Eq(post => post.Id, objectId);
            var update = Builders<Post>.Update.Inc(post => post.Downvotes, 1);
            var result = await _postsCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return NotFound("Post not found");
            }

            return Ok();
        }

        [HttpPost("{postId}/comments/{commentId}/upvote")]
        public async Task<IActionResult> UpvoteComment(string postId, ObjectId commentId)
        {
            // Forsøg at konvertere postId til ObjectId
            if (!ObjectId.TryParse(postId, out var objectId))
            {
                return BadRequest("Invalid post ID format.");
            }

            var postFilter = Builders<Post>.Filter.Eq(p => p.Id, objectId);
            var commentFilter = Builders<Post>.Filter.ElemMatch(p => p.Comments, c => c.Id == commentId);
            var update = Builders<Post>.Update.Inc("Comments.$.Upvotes", 1);
            var result = await _postsCollection.UpdateOneAsync(postFilter & commentFilter, update);

            if (result.ModifiedCount == 0)
            {
                return NotFound("Post or comment not found");
            }
            return Ok();
        }   

        [HttpPost("{postId}/comments/{commentId}/downvote")]
        public async Task<IActionResult> DownvoteComment(string postId, ObjectId commentId)
        {
            // Forsøg at konvertere postId til ObjectId
            if (!ObjectId.TryParse(postId, out var objectId))
            {
                return BadRequest("Invalid post ID format.");
            }

            var postFilter = Builders<Post>.Filter.Eq(p => p.Id, objectId);
            var commentFilter = Builders<Post>.Filter.ElemMatch(p => p.Comments, c => c.Id == commentId);
            var update = Builders<Post>.Update.Inc("Comments.$.Downvotes", 1);

            var result = await _postsCollection.UpdateOneAsync(postFilter & commentFilter, update);

            if (result.ModifiedCount == 0)
            {
                return NotFound("Post or comment not found");
            }
            return Ok();
        }
    }
}