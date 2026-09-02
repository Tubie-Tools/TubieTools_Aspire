// Script to generate facet map template for new entities
// Usage: Modify ProjectName, EntityName, and Properties, then run in C# REPL or as part of code generation

using System;
using System.Collections.Generic;
using System.Text;

public class FacetMapGenerator
{
    public class EntityProperty
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string? Comment { get; set; }
    }

    public static string GenerateFacetMap(
        string projectName,
        string entityName,
        string facetNamespace,
        List<EntityProperty> properties)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// AUTO-GENERATED - Review and customize as needed");
        sb.AppendLine();
        sb.AppendLine($"namespace DTOLayer.FacetMaps.{facetNamespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Facet mapping for {entityName} entity.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class {entityName}FacetMap");
        sb.AppendLine("{");

        // Properties
        foreach (var prop in properties)
        {
            if (!string.IsNullOrEmpty(prop.Comment))
                sb.AppendLine($"    /// <summary>{prop.Comment}</summary>");

            sb.AppendLine($"    public {prop.Type} {prop.Name} {{ get; set; }}");
        }

        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Maps from entity to facet.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static {entityName}FacetMap FromEntity({entityName} entity)");
        sb.AppendLine("    {");
        sb.AppendLine($"        if (entity == null)");
        sb.AppendLine($"            return null;");
        sb.AppendLine();
        sb.AppendLine($"        return new {entityName}FacetMap");
        sb.AppendLine("        {");

        foreach (var prop in properties)
        {
            sb.AppendLine($"            {prop.Name} = entity.{prop.Name},");
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Maps from facet to entity.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public {entityName} ToEntity()");
        sb.AppendLine("    {");
        sb.AppendLine($"        return new {entityName}");
        sb.AppendLine("        {");

        foreach (var prop in properties)
        {
            sb.AppendLine($"            {prop.Name} = {prop.Name},");
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}

// EXAMPLE USAGE:
//var properties = new List<FacetMapGenerator.EntityProperty>
//{
//    new() { Type = "int", Name = "Id" },
//    new() { Type = "string?", Name = "Name", Comment = "Entity name" },
//    new() { Type = "DateTime", Name = "CreatedDate", Comment = "Creation timestamp" },
//    new() { Type = "bool", Name = "IsActive", Comment = "Active status" }
//};

//var code = FacetMapGenerator.GenerateFacetMap(
//    "MyProject",
//    "MyEntity",
//    "MyDomain",
//    properties);

//Console.WriteLine(code);

/* OUTPUT:
// AUTO-GENERATED - Review and customize as needed

namespace DTOLayer.FacetMaps.MyDomain;

/// <summary>
/// Facet mapping for MyEntity entity.
/// </summary>
public class MyEntityFacetMap
{
    /// <summary>Entity name</summary>
    public string? Name { get; set; }
    /// <summary>Creation timestamp</summary>
    public DateTime CreatedDate { get; set; }
    /// <summary>Active status</summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Maps from entity to facet.
    /// </summary>
    public static MyEntityFacetMap FromEntity(MyEntity entity)
    {
        if (entity == null)
            return null;

        return new MyEntityFacetMap
        {
            Name = entity.Name,
            CreatedDate = entity.CreatedDate,
            IsActive = entity.IsActive,
        };
    }

    /// <summary>
    /// Maps from facet to entity.
    /// </summary>
    public MyEntity ToEntity()
    {
        return new MyEntity
        {
            Name = Name,
            CreatedDate = CreatedDate,
            IsActive = IsActive,
        };
    }
}
*/
