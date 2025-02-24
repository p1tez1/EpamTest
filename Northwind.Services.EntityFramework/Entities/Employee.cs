namespace Northwind.Services.EntityFramework.Entities;

public class Employee
{
    public long Id { get; set; }

    public string LastName { get; set; }

    public string FirstName { get; set; }

    public string Title { get; set; }

    public string TitleOfCourtesy { get; set; }

    public DateTime BirthDate { get; set; }

    public DateTime HireDate { get; set; }

    public string Address { get; set; }

    public string City { get; set; }

    public string Region { get; set; }

    public string PostalCode { get; set; }

    public string Country { get; set; }

    public string HomePhone { get; set; }

    public string Extension { get; set; }

    public string Notes {  get; set; }

    public long ReportTo { get; set; }

    public string PhotoPath { get; set; }

    public List<Order> Orders { get; set; }
}
