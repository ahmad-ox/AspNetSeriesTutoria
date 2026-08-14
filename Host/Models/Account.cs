using System.ComponentModel.DataAnnotations;

namespace Host.Models;

public class Account
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "Date created is required")]
    public DateTime DateCreated { get; set; }

    [Required(ErrorMessage = "Account type is required")]
    public string AccountType { get; set; }

    [Required(ErrorMessage = "Owner Id is required")]
    public string OwnerId { get; set; }
    public Owner Owner { get; set; }
}
public enum AccountType
{
    Domestic,
    Savings,
    Foreign
}