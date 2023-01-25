using System.ComponentModel.DataAnnotations;

namespace Vinyoxla.Service.ViewModels.AppUserToVincodeVMs
{
    public class AppUserToVincodeCreateVM
    {
        [StringLength(9, MinimumLength = 9, ErrorMessage = "Number must be 9 characters!")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "{0} is required!")]
        public string PhoneNumber { get; set; }

        [StringLength(17, MinimumLength = 17, ErrorMessage = "Vin Code must be 17 characters!")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "{0} is required!")]
        public string VinCode { get; set; }
    }
}
