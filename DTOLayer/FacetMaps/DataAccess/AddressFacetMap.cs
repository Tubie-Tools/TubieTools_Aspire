namespace DTOLayer.FacetMaps.DataAccess;

/// <summary>
/// Facet mapping for AddressEntity.
/// </summary>
public class AddressFacetMap
{
    public int Id { get; set; }
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsPrimaryAddress { get; set; }
    public string? AddressType { get; set; }
    public DateTime CreatedDate { get; set; }

    // Namespace reference would be: using DataAccessLayer.Data.Entities;
    public static AddressFacetMap FromEntity(dynamic entity) // Using dynamic to avoid direct reference
    {
        return new AddressFacetMap
        {
            Id = entity.Id,
            StreetAddress = entity.StreetAddress,
            City = entity.City,
            State = entity.State,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            IsPrimaryAddress = entity.IsPrimaryAddress,
            AddressType = entity.AddressType,
            CreatedDate = entity.CreatedDate
        };
    }

    public dynamic ToEntity()
    {
        // Returns dynamic to allow caller to cast to AddressEntity
        return new
        {
            Id,
            StreetAddress,
            City,
            State,
            PostalCode,
            Country,
            IsPrimaryAddress,
            AddressType,
            CreatedDate
        };
    }
}
