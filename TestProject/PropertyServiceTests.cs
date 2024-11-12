using Moq;
using PropertyApi.Entities;
using PropertyApi.Models;
using Microsoft.Extensions.Options;
using Xunit;
using PropertyApi.Services;

namespace TestProject;

//[TestClass]
public class PropertyServiceTests
{
    private readonly Mock<IOptions<AppOptions>> _mockOptions;
    private readonly Mock<ICsvResultParser> _mockCsvParser;
    private readonly PropertyService _propertyService;
    private readonly List<Property> _testProperties;
    private readonly List<AverageSuburbs> _testAverageSuburbs;

    public PropertyServiceTests()
    {
        _testProperties = new List<Property>
            {
                new Property { Id = 1, Suburb = "Suburb1", Value = 500000 },
                new Property { Id = 2, Suburb = "Suburb2", Value = 600000 },
                new Property { Id = 3, Suburb = "Suburb1", Value = 550000 }
            };

        _testAverageSuburbs = new List<AverageSuburbs>
            {
                new AverageSuburbs { Name = "KEARNS",Units= 99937, Houses= 162466  },
                new AverageSuburbs { Name = "KELLYVILLE", Units = 101214, Houses = 146475 }
            };

        _mockOptions = new Mock<IOptions<AppOptions>>();
        _mockOptions.Setup(x => x.Value).Returns(new AppOptions { CsvFilePath = "test.csv" });

        _mockCsvParser = new Mock<ICsvResultParser>();
        _mockCsvParser
            .Setup(x => x.ParseResultAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProperties);

        _mockCsvParser
            .Setup(x => x.CalculateAverageSuburbValues(It.IsAny<List<Property>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testAverageSuburbs);

        // Create service instance
        _propertyService = new PropertyService(_mockOptions.Object, _mockCsvParser.Object);
    }

    [Fact]
    public void GetProperties_ReturnsAllProperties()
    {
        _propertyService.StartAsync(CancellationToken.None).Wait();

        var result = _propertyService.GetProperties();

        Xunit.Assert.Equal(_testProperties.Count, result.Count);
        Xunit.Assert.Equal(_testProperties, result);
    }

    [Fact]
    public void GetPropertyById_ExistingId_ReturnsProperty()
    {
        _propertyService.StartAsync(CancellationToken.None).Wait();

        var result = _propertyService.GetPropertyById(1);

        Xunit.Assert.NotNull(result);
        Xunit.Assert.Equal(1, result.Id);
        Xunit.Assert.Equal("Suburb1", result.Suburb);
    }

    [Fact]
    public void GetPropertyById_NonExistingId_ReturnsNull()
    {
        _propertyService.StartAsync(CancellationToken.None).Wait();

        var result = _propertyService.GetPropertyById(999);

        Xunit.Assert.Null(result);
    }

    [Fact]
    public void AverageSuburbs_ReturnsCorrectAverages()
    {
        _propertyService.StartAsync(CancellationToken.None).Wait();

        var result = _propertyService.AverageSuburbs();

        Xunit.Assert.Equal(_testAverageSuburbs.Count, result.Count);
        Xunit.Assert.Equal(_testAverageSuburbs, result);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    public void GetPropertiesWithPaging_ReturnsCorrectPage(int pageNumber, int pageSize)
    {
        _propertyService.StartAsync(CancellationToken.None).Wait();

        var (properties, metadata) = _propertyService.GetPropertiesWithPaging(pageNumber, pageSize);

        Xunit.Assert.Equal(Math.Min(pageSize, _testProperties.Count - (pageNumber - 1) * pageSize),
                    properties.Count());
        Xunit.Assert.Equal(_testProperties.Count, metadata.TotalItemCount);
        Xunit.Assert.Equal(pageSize, metadata.PageSize);
        Xunit.Assert.Equal(pageNumber, metadata.CurrentPage);
    }

    [Fact]
    public async Task StartAsync_LoadsPropertiesAndCalculatesAverages()
    {
        await _propertyService.StartAsync(CancellationToken.None);

        _mockCsvParser.Verify(x => x.ParseResultAsync(
            It.Is<string>(s => s == "test.csv"),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _mockCsvParser.Verify(x => x.CalculateAverageSuburbValues(
            It.Is<List<Property>>(p => p == _testProperties),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_ClearsCollections()
    {
        await _propertyService.StartAsync(CancellationToken.None);
        Xunit.Assert.NotEmpty(_propertyService.GetProperties());
        Xunit.Assert.NotEmpty(_propertyService.AverageSuburbs());

        await _propertyService.StopAsync(CancellationToken.None);

        Xunit.Assert.Empty(_propertyService.GetProperties());
        Xunit.Assert.Empty(_propertyService.AverageSuburbs());
    }
}