using exercise.wwwapi.Helpers;
using exercise.wwwapi.Models;
using exercise.wwwapi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using System.Security.Claims;

namespace exercise.wwwapi.EndPoints
{
    public static class SecureApi
    {
        public static void ConfigureSecureApi(this WebApplication app)
        {
            app.MapGet("message", GetMessage);
           

        }
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        private static async Task<IResult> GetMessage(IRepository<User> service, ClaimsPrincipal user, ILogger logger)
        {
            logger.LogDebug(new string('*', 1000));
            return TypedResults.Ok(new { LoggedIn = true, UserId=user.UserRealId().ToString(), Email = $"{user.Email()}", Message = "Pulled the userid and email out of the claims" });
        }
    }
}
