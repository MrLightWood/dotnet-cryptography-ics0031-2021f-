using System.Linq;
using System.Security.Claims;

namespace WebApp.Helpers
{
    public static class IdentityExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.Claims.FirstOrDefault(c =>
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            return idClaim?.Value;
        }
    }
}