using PropertyApi.Entities;

namespace PropertyApi.Services
{
    public interface ICsvResultParser
    {
        Task<List<Property>> ParseResultAsync(string filePath, CancellationToken cancellationToken);
        Task<List<AverageSuburbs>> CalculateAverageSuburbValues(List<Property> properties, CancellationToken cancellationToken);
    }
}