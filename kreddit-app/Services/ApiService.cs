using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq; // Husk at inkludere System.Linq for SortByDescending
using shared.Model;
using MongoDB.Driver;

namespace kreddit_app.Data
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // API'er for Posts
        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            var response = await _httpClient.GetAsync("api/posts");
            response.EnsureSuccessStatusCode();

            var posts = await response.Content.ReadFromJsonAsync<IEnumerable<Post>>();

            // Debugging: Check om hver post har en gyldig Id
            foreach (var post in posts)
            {
                Console.WriteLine($"Post ID from API: {post.Id}"); // Log fra API
            }

            // Sorter posts efter oprettelsesdato i faldende rækkefølge
            return posts.OrderByDescending(p => p.CreationTime);
        }

        public async Task<Post> GetPostByIdAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<Post>($"api/posts/{id}");
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/posts", post);
                response.EnsureSuccessStatusCode();

                // Hent den oprettede post fra svaret
                var createdPost = await response.Content.ReadFromJsonAsync<Post>();
                return createdPost; // Returner den oprettede post
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to create post", ex);
            }
        }

        // API'er for Votes
        public async Task UpvotePostAsync(string postId)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/posts/{postId}/upvote", new { });
            response.EnsureSuccessStatusCode();
        }

        public async Task DownvotePostAsync(string postId)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/posts/{postId}/downvote", new { });
            response.EnsureSuccessStatusCode();
        }

        // API'er for Comments
        public async Task AddCommentToPostAsync(string postId, Comment newComment)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/posts/{postId}/comments", newComment);
            response.EnsureSuccessStatusCode();
        }




        public async Task<List<Post>> GetPostsByPageAsync(int pageNumber, int pageSize)
        {
            var response = await _httpClient.GetAsync($"api/posts?pageNumber={pageNumber}&pageSize={pageSize}");
            response.EnsureSuccessStatusCode();

            var posts = await response.Content.ReadFromJsonAsync<List<Post>>();
            return posts;
        }
    }
}
