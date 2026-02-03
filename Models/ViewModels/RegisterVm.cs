using System.ComponentModel.DataAnnotations;

namespace bookTracker.Models.ViewModels;

public class RegisterVm
{
    [Required(ErrorMessage = "نام الزامی است.")]
    [MaxLength(50)]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
    [MaxLength(50)]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "ایمیل الزامی است.")]
    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [MinLength(6, ErrorMessage = "حداقل ۶ کاراکتر.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "تکرار رمز عبور الزامی است.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "رمز عبور و تکرار آن یکی نیست.")]
    public string ConfirmPassword { get; set; } = "";

    [Range(typeof(bool),"true","true", ErrorMessage = "باید قوانین و حریم خصوصی را بپذیرید.")]
    public bool AcceptTerms { get; set; }
   

}
