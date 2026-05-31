namespace BusinessObjects.DTOs;

public class EmployerDto
{
    public int Id  { get; set; }
    public int AccountId  { get; set; }
    public string FullName  { get; set; }
    public string CompanyName { get; set; }
    public string Description { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string Logo { get; set; }
}

public class UpdateEmployerProfileDto
{
    public string FullName { get; set; }
    public string CompanyName { get; set; }
    public string Description { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}