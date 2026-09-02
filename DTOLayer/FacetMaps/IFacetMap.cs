namespace DTOLayer.FacetMaps;

/// <summary>
/// Base facet mapping interface for all entity-to-DTO conversions.
/// All facet maps should implement this interface for consistency.
/// </summary>
public interface IFacetMap<TEntity, TFacet> where TEntity : class where TFacet : class
{
    /// <summary>
    /// Converts an entity to its facet (DTO) representation.
    /// </summary>
    /// <param name="entity">The entity to convert</param>
    /// <returns>The facet representation</returns>
    TFacet FromEntity(TEntity entity);

    /// <summary>
    /// Converts a facet to its entity representation.
    /// </summary>
    /// <param name="facet">The facet to convert</param>
    /// <returns>The entity representation</returns>
    TEntity ToEntity(TFacet facet);
}
