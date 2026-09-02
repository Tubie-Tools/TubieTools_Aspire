namespace DTOLayer.FacetMaps;

/// <summary>
/// Central registry for all facet maps in the application.
/// Provides a single point of access for entity-to-DTO conversions.
/// </summary>
public static class FacetMapRegistry
{
    /// <summary>
    /// Extension method to convert any entity to its facet map representation.
    /// Usage: var facet = entity.ToFacet();
    /// </summary>
    public static TFacet ToFacet<TFacet>(this object entity) where TFacet : class
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityType = entity.GetType();
        var facetType = typeof(TFacet);

        // Find the appropriate FromEntity method
        var fromEntityMethod = facetType.GetMethod("FromEntity", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
            null, new[] { entityType }, null);

        if (fromEntityMethod == null)
            throw new InvalidOperationException($"No facet converter found for {entityType.Name} to {facetType.Name}");

        return (TFacet)fromEntityMethod.Invoke(null, new[] { entity })!;
    }

    /// <summary>
    /// Extension method to convert any facet to its entity representation.
    /// Usage: var entity = facet.ToEntity<EntityType>();
    /// </summary>
    public static TEntity FromFacet<TEntity>(this object facet) where TEntity : class
    {
        if (facet == null)
            throw new ArgumentNullException(nameof(facet));

        var facetType = facet.GetType();

        // Find the appropriate ToEntity method
        var toEntityMethod = facetType.GetMethod("ToEntity",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
            null, Type.EmptyTypes, null);

        if (toEntityMethod == null)
            throw new InvalidOperationException($"No reverse converter found for {facetType.Name}");

        var result = toEntityMethod.Invoke(facet, null);

        if (result is TEntity entity)
            return entity;

        throw new InvalidOperationException($"Failed to convert {facetType.Name} to {typeof(TEntity).Name}");
    }
}
