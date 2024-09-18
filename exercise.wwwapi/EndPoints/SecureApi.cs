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
        private static async Task<IResult> GetMessage(IDatabaseRepository<User> service, ClaimsPrincipal user)
        {
            return TypedResults.Ok($"you are logged in and you are userid: {user.UserRealId().ToString()}!");
        }
    }
}
