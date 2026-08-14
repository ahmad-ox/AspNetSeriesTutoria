using Host.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Host.DTOS;

public class AddOwnerRequest
{
    [Required(ErrorMessage = "Name is required")]
    //[StringLength(600, ErrorMessage = "Name can't be longer than 60 characters")]
    public string Name { get; set; }
    public int MockCount { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Address is required")]
    //[StringLength(100, ErrorMessage = "Address can not be loner then 100 characters")]
    public Gender Gender { get; set; }
    public string Address { get; set; }
}