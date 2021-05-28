using CartaWeb.Models.Meta;

namespace CartaWeb.Models.Data
{
    [ApiType(typeof(string))]
    public enum UserAttributeEnumeration
    {
        UserId,
        UserName,
        Email/*,
        GivenName,
        Surname*/
    }
}
