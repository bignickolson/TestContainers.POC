using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace TestContainers.Data
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public required string Subject { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}