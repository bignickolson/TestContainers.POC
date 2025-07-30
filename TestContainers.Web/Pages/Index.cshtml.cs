using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestContainers.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MessageClient client;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(MessageClient client, ILogger<IndexModel> logger)
        {
            this.client = client;
            _logger = logger;
        }

        public ICollection<MessageSummary> Messages { get; private set; } = [];

        public async Task OnGet()
        {
            Messages = await client.MessagesAllAsync();
        }
    }
}
