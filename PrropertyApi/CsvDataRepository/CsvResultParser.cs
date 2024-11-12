using CsvHelper;
using PropertyApi.Entities;
using PropertyApi.Services;
using System.Globalization;

namespace PropertyApi.CsvDataRepository;
public class CsvResultParser : ICsvResultParser
{
    public async Task<List<Property>> ParseResultAsync(string filePath, CancellationToken cancellationToken)
    {
        List<Property> properties = new();

        if (File.Exists(filePath))
        {

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                await foreach (var record in csv.GetRecordsAsync<Property>(cancellationToken))
                {
                    properties.Add(record);
                }

                Console.WriteLine($"Loaded {properties.Count} properties from {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading CSV data: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"File not found: {filePath}");
        }

        return properties;
    }

    public async Task<List<AverageSuburbs>> CalculateAverageSuburbValues(List<Property> properties, CancellationToken cancellationToken)
    {
        var averageSuburbValues = properties
                .GroupBy(p => p.Suburb?.Trim().ToUpperInvariant() ?? "UNKNOWN")
                .Select(suburbGroup =>
                {
                    var unitProperties = suburbGroup
                        .Where(p => p.Type == "unit")
                        .ToList();

                    var houseProperties = suburbGroup
                        .Where(p => p.Type == "house")
                        .ToList();

                    return new AverageSuburbs
                    {
                        Name = suburbGroup.Key,
                        Units = CalculateAverage(unitProperties),
                        Houses = CalculateAverage(houseProperties),
                    };
                })
                .OrderBy(s => s.Name)
                .ToList();

        return await Task.FromResult(averageSuburbValues);
    }

    private static int CalculateAverage(List<Property> properties)
    {
        return properties.Any()
            ? ConvertDecimal(properties.Average(p => p.Value)) 
            : 0;
    }

    private static int ConvertDecimal(decimal input)
    {
        return (int)(input * 100);
    }
}
