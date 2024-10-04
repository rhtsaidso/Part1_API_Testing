using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace TestProject1
{
    public class UnitTest1
    {
        private readonly HttpClient _client;

        public UnitTest1()
        {
            // Initialize HttpClient with the base address for API calls
            _client = new HttpClient { BaseAddress = new Uri("https://jsonplaceholder.typicode.com/") };
        }

        // Helper method to send a POST request with JSON content
        private async Task<HttpResponseMessage> SendPostAsync(string url, object data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            return await _client.PostAsync(url, content);
        }

        // Helper method to send a PUT request with JSON content
        private async Task<HttpResponseMessage> SendPutAsync(string url, object data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            return await _client.PutAsync(url, content);
        }

        // Helper method to send a DELETE request
        private async Task<HttpResponseMessage> SendDeleteAsync(string url)
        {
            return await _client.DeleteAsync(url);
        }

        // Helper method to read the response content and ensure the request was successful
        private async Task<string> ReadResponseContentAsync(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode(); // Throws an exception if the response is not successful
            return await response.Content.ReadAsStringAsync(); // Read the response content as a string
        }

        [Fact]
        public async Task TestGetPosts()
        {
            // Act: Send a GET request to retrieve posts
            var response = await _client.GetAsync("posts");

            // Assert: Verify that the response indicates success
            Assert.True(response.IsSuccessStatusCode);
            var content = await ReadResponseContentAsync(response);
            Assert.NotEmpty(content); // Ensure that the content is not empty

            var posts = JsonConvert.DeserializeObject<dynamic[]>(content);
            Assert.NotEmpty(posts); // Ensure that the posts are not empty
            Assert.True(posts.Length > 0); // Ensure that there are multiple posts
            Assert.Contains(posts, post => post.title != null); // Check that at least one post has a title
        }

        [Fact]
        public async Task TestCreatePost()
        {
            // Arrange: Create a new post object to send in the request
            var newPost = new { title = "foo", body = "bar", userId = 1 };

            // Act: Send a POST request to create a new post
            var response = await SendPostAsync("posts", newPost);

            // Assert: Verify that the response indicates success
            var responseBody = await ReadResponseContentAsync(response);
            var createdPost = JsonConvert.DeserializeObject<dynamic>(responseBody);

            // Assert: Validate the properties of the created post
            Assert.Equal("foo", (string)createdPost.title);
            Assert.Equal("bar", (string)createdPost.body);
            Assert.Equal(1, (int)createdPost.userId);
            Assert.True((int)createdPost.id > 0); // Ensure the id is greater than 0
        }

        [Fact]
        public async Task TestUpdatePost()
        {
            // Arrange: Create an updated post object
            var updatedPost = new { id = 1, title = "updated title", body = "updated body", userId = 1 };

            // Act: Send a PUT request to update the existing post
            var response = await SendPutAsync("posts/1", updatedPost);

            // Assert: Verify that the response indicates success
            var responseBody = await ReadResponseContentAsync(response);
            var post = JsonConvert.DeserializeObject<dynamic>(responseBody);

            // Assert: Validate the properties of the updated post
            Assert.Equal("updated title", (string)post.title);
            Assert.Equal("updated body", (string)post.body);
            Assert.Equal(1, (int)post.userId); // Validate userId remains unchanged
        }

        [Fact]
        public async Task TestDeletePost()
        {
            // Act: Send a DELETE request to remove a post
            var response = await SendDeleteAsync("posts/1");

            // Assert: Verify that the response indicates success
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode); // Ensure that status is OK
        }

        [Fact]
        public async Task TestAddCommentToPost()
        {
            // Arrange: Create a new comment object to send in the request
            var newComment = new { name = "foo", email = "foo@bar.com", body = "Nice post!", postId = 1 };

            // Act: Send a POST request to add a comment to the specified post
            var response = await SendPostAsync("posts/1/comments", newComment);

            // Assert: Verify that the response indicates success
            var responseBody = await ReadResponseContentAsync(response);
            var createdComment = JsonConvert.DeserializeObject<dynamic>(responseBody);

            // Assert: Validate the properties of the created comment
            Assert.Equal("foo", (string)createdComment.name);
            Assert.Equal("foo@bar.com", (string)createdComment.email);
            Assert.Equal("Nice post!", (string)createdComment.body);
            Assert.True((int)createdComment.postId == 1); // Ensure comment is linked to postId 1
        }

        [Fact]
        public async Task TestGetCommentsForPost()
        {
            // Act: Send a GET request to retrieve comments for a specific post
            var response = await _client.GetAsync("comments?postId=1");

            // Assert: Verify that the response indicates success
            Assert.True(response.IsSuccessStatusCode);

            // Read and validate the response content
            var content = await ReadResponseContentAsync(response);
            Assert.NotEmpty(content); // Ensure content is not empty

            var comments = JsonConvert.DeserializeObject<dynamic[]>(content);
            Assert.NotNull(comments); // Ensure comments are not null
            Assert.True(comments.Length > 0); // Validate that there are comments for the post
            Assert.Contains(comments, comment => comment.body != null); // Check that at least one comment has a body
        }
    }
}
