namespace PropertyApi.Entities;

public class Property
{
    public int Id { get; set; }
    public string? Suburb { get; set; }
    public decimal Value { get; set; }
    public string? Date { get; set; }
    public int NumberOfBedrooms { get; set; }
    public string? Type { get; set; }
}
