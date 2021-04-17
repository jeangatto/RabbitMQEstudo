using System.ComponentModel.DataAnnotations;

namespace RabbitWebApp.ViewModels
{
    public class EmailViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 10)]
        [DataType(DataType.Text)]
        public string Title { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string From { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string To { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 10)]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }
    }
}