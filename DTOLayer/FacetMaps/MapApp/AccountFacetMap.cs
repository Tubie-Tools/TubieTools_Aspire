//namespace DTOLayer.FacetMaps.MapApp;

///// <summary>
///// Facet mapping for Account entity.
///// </summary>
//public class AccountFacetMap
//{
//    public int AccountId { get; set; }
//    public string? Email { get; set; }
//    public string? FullName { get; set; }
//    public string? Role { get; set; }
//    public bool IsActive { get; set; }
//    public string? EntraObjectId { get; set; }
//    public DateTime CreatedDate { get; set; }
//    public DateTime? LastLoginDate { get; set; }

//    public static AccountFacetMap FromEntity(Account entity)
//    {
//        return new AccountFacetMap
//        {
//            AccountId = entity.AccountId,
//            Email = entity.Email,
//            FullName = entity.FullName,
//            Role = entity.Role,
//            IsActive = entity.IsActive,
//            EntraObjectId = entity.EntraObjectId,
//            CreatedDate = entity.CreatedDate,
//            LastLoginDate = entity.LastLoginDate
//        };
//    }

//    public Account ToEntity()
//    {
//        return new Account
//        {
//            AccountId = AccountId,
//            Email = Email,
//            FullName = FullName,
//            Role = Role,
//            IsActive = IsActive,
//            EntraObjectId = EntraObjectId,
//            CreatedDate = CreatedDate,
//            LastLoginDate = LastLoginDate
//        };
//    }
//}
