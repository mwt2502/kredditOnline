using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using shared.Model;

namespace kreddit_app.Data
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Post>>("api/posts");
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



        public async Task CreateCommentAsync(Comment comment, string postId)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/posts/{postId}/comments", comment);
            response.EnsureSuccessStatusCode();
        }

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

        public async Task UpvoteCommentAsync(string postId, string commentId)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/posts/{postId}/comments/{commentId}/upvote", new { });
            response.EnsureSuccessStatusCode();
        }

        public async Task DownvoteCommentAsync(string postId, string commentId)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/posts/{postId}/comments/{commentId}/downvote", new { });
            response.EnsureSuccessStatusCode();
        }
    }
}
