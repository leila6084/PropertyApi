using Microsoft.AspNetCore.Mvc;
using PropertyApi.Entities;
using PropertyApi.Services;

namespace PropertyApi.Controllers;

[Produces("application/json")]
public class PropertyController(IPropertyService propertyService) : ControllerBase
{
    private const int MaxPropertiesPageSize = 20;

    [HttpGet("property")]
    public ActionResult<IEnumerable<Property>> Get()
    {
        return Ok(propertyService.GetProperties());
    }

    [HttpGet("property/paged")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<Property>> GetProperties(
             int pageNumber = 1,
             int pageSize = 10)
    {
        if (pageNumber < 1)
        {
            return BadRequest("Page number must be greater than 0");
        }

        if (pageSize < 1)
        {
            return BadRequest("Page size must be greater than 0");
        }

        pageSize = Math.Min(pageSize, MaxPropertiesPageSize);


        var (propertEntities, paginationMetadata) =  propertyService
            .GetPropertiesWithPaging(pageNumber, pageSize);

        return Ok(propertEntities);
    }

    [HttpGet("property/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<Property>> GetById(int id)
    {
        var propertyValue =
            propertyService.GetPropertyById(id);

        if (propertyValue == null)
            return Task.FromResult<ActionResult<Property>>(NotFound());

        return Task.FromResult<ActionResult<Property>>(Ok(propertyValue));
    }

    [HttpGet("suburbs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<AverageSuburbs>> GetAverageSuburbs()
    {
        return Ok(propertyService.AverageSuburbs());
    }
}
