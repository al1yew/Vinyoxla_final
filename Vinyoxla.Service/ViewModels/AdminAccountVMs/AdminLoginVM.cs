using System.ComponentModel.DataAnnotations;

namespace Vinyoxla.Service.ViewModels.AdminAccountVMs
{
    public class AdminLoginVM
    {
        [Required]
        public string Login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
