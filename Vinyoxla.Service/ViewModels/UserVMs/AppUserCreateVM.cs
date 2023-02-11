using System.ComponentModel.DataAnnotations;

namespace Vinyoxla.Service.ViewModels.UserVMs
{
    public class AppUserCreateVM
    {
        [StringLength(9, MinimumLength = 9, ErrorMessage = "Phone must be 9 characters!")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "{0} is required!")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        public string Password { get; set; }

        [DataType(DataType.Text)]
        public int Balance { get; set; }
    }
}
