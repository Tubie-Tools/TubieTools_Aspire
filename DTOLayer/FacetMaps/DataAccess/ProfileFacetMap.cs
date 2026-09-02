namespace DTOLayer.FacetMaps.DataAccess;

/// <summary>
/// Facet mapping for Profile entity.
/// </summary>
public class ProfileFacetMap
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Bio { get; set; }

    public static ProfileFacetMap FromEntity(dynamic entity)
    {
        return new ProfileFacetMap
        {
            Id = entity.Id,
            Name = entity.Name,
            DateOfBirth = entity.DateOfBirth,
            Bio = entity.Bio
        };
    }

    public dynamic ToEntity()
    {
        return new
        {
            Id,
            Name,
            DateOfBirth,
            Bio
        };
    }
}
