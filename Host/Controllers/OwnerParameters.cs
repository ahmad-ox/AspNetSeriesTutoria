namespace Host.Controllers;

public class OwnerParameters : QueryStringParameters
{
    public OwnerParameters()
    {
        OrderBy = "name";
    }
    public uint MinYearOfBirth { get; set; }
    public string Name { get; set; }
    public uint MaxYearOfBirth { get; set; } = (uint)DateTime.Now.Year;
    public bool ValidYearRange => MaxYearOfBirth > MinYearOfBirth;
}
