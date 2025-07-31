using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestContainers.Web.Pages
{
    public class MessageModel : PageModel
    {
        private readonly MessageClient client;

        public MessageModel(MessageClient client)
        {
            this.client = client;
        }

        public Message Message { get; private set; } = new Message();

        public async Task OnGet(int id)
        {
            Message = await client.MessageGETAsync(id);
        }

        
        public async Task<ActionResult> OnPostDeleteAsync(int id)
        {
            await client.MessageDELETEAsync(id);
            return RedirectToPage("Index");
        }
    }
}
