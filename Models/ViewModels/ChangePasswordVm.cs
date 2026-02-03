using System.ComponentModel.DataAnnotations;

namespace bookTracker.Models.ViewModels;

public class ChangePasswordVm
{
    [Required(ErrorMessage = "رمز فعلی الزامی است.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "رمز جدید الزامی است.")]
    [MinLength(6, ErrorMessage = "رمز جدید حداقل 6 کاراکتر باشد.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "تکرار رمز جدید الزامی است.")]
    [Compare(nameof(NewPassword), ErrorMessage = "رمز جدید و تکرارش یکسان نیستند.")]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; } = "";
}
