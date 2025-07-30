namespace TestContainers.Api.Request
{
    public class CreateMessageRequest
    {
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
