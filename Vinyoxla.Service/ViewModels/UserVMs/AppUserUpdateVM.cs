using System.ComponentModel.DataAnnotations;

namespace Vinyoxla.Service.ViewModels.UserVMs
{
    public class AppUserUpdateVM
    {
        public string Id { get; set; }

        [StringLength(9, MinimumLength = 9, ErrorMessage = "Phone must be 9 characters!")]
        [DataType(DataType.Text)]
        public string PhoneNumber { get; set; }

        public string Password { get; set; }

        [DataType(DataType.Text)]
        public int Balance { get; set; }
    }
}
