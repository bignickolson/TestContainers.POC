using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestContainers.Web.Pages
{
    public class AddMessageModel : PageModel
    {
        private readonly MessageClient client;

        public AddMessageModel(MessageClient client)
        {
            this.client = client;
        }

        [BindProperty]
        public CreateMessageRequest Message { get; set; } = new CreateMessageRequest();
        public void OnGet()
        {
        }

        public async Task<ActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                await client.MessagesAsync(Message);
                return Redirect("/");
            }
            return Page();
        }
    }
}
