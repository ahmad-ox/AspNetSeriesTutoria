using Host.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Host.Models;

public class Owner : BaseEntity
{

    [Required(ErrorMessage = "Name is required")]
    //[StringLength(600, ErrorMessage = "Name can't be longer than 60 characters")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Address is required")]
    //[StringLength(100, ErrorMessage = "Address can not be loner then 100 characters")]
    public string Address { get; set; }
    public Gender Gender { get; set; }
    public Account Account { get; set; }
}
public class BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}
