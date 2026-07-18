namespace TubieTools_PublicAPI.Controllers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiredScopesAttribute : Attribute
    {
        public string[] RequiredScopes { get; }

        public RequiredScopesAttribute(params string[] scopes)
        {
            RequiredScopes = scopes;
        }
    }
}
