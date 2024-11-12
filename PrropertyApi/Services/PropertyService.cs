using PropertyApi.Entities;

namespace PropertyApi.Services
{
    using Microsoft.Extensions.Options;
    using Models;

    public class PropertyService(
        IOptions<AppOptions> appOptions,
        ICsvResultParser csvResultParser) : IPropertyService, IHostedService
    {
        private List<Property> _properties = new();
        private List<AverageSuburbs> _averageSuburbs;
        private readonly ICsvResultParser _resultParser = csvResultParser;

        public List<Property> GetProperties()
        {
            return _properties;
        }

        public List<AverageSuburbs> AverageSuburbs()
        {
            return _averageSuburbs;
        }

        public Property GetPropertyById(int id) => GetProperties().FirstOrDefault(p => p.Id == id);

        public (IEnumerable<Property>, PaginationMetadata) GetPropertiesWithPaging(int pageNumber, int pageSize)
        {
            var totalItemCount =  _properties.Count();

            var paginationMetadata = new PaginationMetadata(
                totalItemCount, pageSize, pageNumber);

            var collectionToReturn =  _properties.OrderBy(c => c.Suburb)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToList();

            return (collectionToReturn, paginationMetadata);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _properties = await _resultParser.ParseResultAsync(appOptions.Value.CsvFilePath, cancellationToken);
            _averageSuburbs = await _resultParser.CalculateAverageSuburbValues(_properties, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _properties.Clear();
            _averageSuburbs.Clear();
            return Task.CompletedTask;
        }
    }
}
