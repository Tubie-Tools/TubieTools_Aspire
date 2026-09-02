namespace DTOLayer.FacetMaps.DataAccess;

/// <summary>
/// Facet mapping for OrderEntity.
/// Simplified DTO representation without navigation properties.
/// </summary>
public class OrderFacetMap
{
    public int Id { get; set; }
    public string? OrderId { get; set; }
    public int CareProviderId { get; set; }
    public string? Status { get; set; }
    public int TotalQuantity { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    public int PaymentFrequency { get; set; }
    public string? PromoCode { get; set; }
    public string? SpecialInstructions { get; set; }
    public DateTime ExpectedShipDate { get; set; }
    public DateTime? ActualShipDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public decimal EstimatedLaborHours { get; set; }
    public decimal EstimatedMaterialCost { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public static OrderFacetMap FromEntity(dynamic entity)
    {
        return new OrderFacetMap
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            CareProviderId = entity.CareProviderId,
            Status = entity.Status,
            TotalQuantity = entity.TotalQuantity,
            Subtotal = entity.Subtotal,
            DiscountAmount = entity.DiscountAmount,
            TaxAmount = entity.TaxAmount,
            ShippingCost = entity.ShippingCost,
            TotalAmount = entity.TotalAmount,
            PaymentFrequency = entity.PaymentFrequency,
            PromoCode = entity.PromoCode,
            SpecialInstructions = entity.SpecialInstructions,
            ExpectedShipDate = entity.ExpectedShipDate,
            ActualShipDate = entity.ActualShipDate,
            ExpectedDeliveryDate = entity.ExpectedDeliveryDate,
            ActualDeliveryDate = entity.ActualDeliveryDate,
            EstimatedLaborHours = entity.EstimatedLaborHours,
            EstimatedMaterialCost = entity.EstimatedMaterialCost,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };
    }

    public dynamic ToEntity()
    {
        return new
        {
            Id,
            OrderId,
            CareProviderId,
            Status,
            TotalQuantity,
            Subtotal,
            DiscountAmount,
            TaxAmount,
            ShippingCost,
            TotalAmount,
            PaymentFrequency,
            PromoCode,
            SpecialInstructions,
            ExpectedShipDate,
            ActualShipDate,
            ExpectedDeliveryDate,
            ActualDeliveryDate,
            EstimatedLaborHours,
            EstimatedMaterialCost,
            CreatedDate,
            ModifiedDate
        };
    }
}
