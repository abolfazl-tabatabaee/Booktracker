using System.ComponentModel.DataAnnotations;

namespace bookTracker.Models.ViewModels;

public class EditProfileVm
{
    [Required(ErrorMessage = "نام الزامی است.")]
    [MaxLength(60)]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
    [MaxLength(60)]
    public string? LastName { get; set; }
}
