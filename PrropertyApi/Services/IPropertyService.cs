using PropertyApi.Entities;
namespace PropertyApi.Services;

public interface IPropertyService
{
    List<Property> GetProperties();
    List<AverageSuburbs> AverageSuburbs();
    Property GetPropertyById(int id);
    (IEnumerable<Property>, PaginationMetadata) GetPropertiesWithPaging(int pageNumber, int pageSize);
}