using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestContainers.Api.Data
{
    public class Message
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public required  string Subject { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}