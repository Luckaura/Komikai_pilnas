using Komikai_pilnas.Auth.Model;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Komikai_pilnas.Auth
{
    public static class AuthEndpoints
    {

        public static void AddAuthApi(this WebApplication app)
        {
            // register
            app.MapPost("api/accounts", async (UserManager<ForumUser> userManager, RegisterUserDto registerUserDto) =>
            {
                // check user exists
                var user = await userManager.FindByNameAsync(registerUserDto.Username);
                if (user != null)
                    return Results.UnprocessableEntity("User name already taken");

                var newUser = new ForumUser
                {
                    Email = registerUserDto.Email,
                    UserName = registerUserDto.Username
                };
                //i need to wrap this in transaction
                var createUserResult = await userManager.CreateAsync(newUser, registerUserDto.Password);
                if (!createUserResult.Succeeded)
                    return Results.UnprocessableEntity();

                await userManager.AddToRoleAsync(newUser, ForumRoles.ForumUser);

                return Results.Created();
            });

            app.MapPost("api/login", async (UserManager<ForumUser> userManager, JwtTokenService jwtTokenService, HttpContext httpContext, LoginDto loginDto, SessionService sessionService) =>
            {
                // check user exists
                var user = await userManager.FindByNameAsync(loginDto.Username);
                if (user == null)
                    return Results.UnprocessableEntity("User does not exist.");

                var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
                if (!isPasswordValid)
                    return Results.UnprocessableEntity("Username or password was incorrect.");

                //user.ForceRelogin = false;
                await userManager.UpdateAsync(user);


                var sessionId = Guid.NewGuid();
                var roles = await userManager.GetRolesAsync(user);
                var expiresAt = DateTime.UtcNow.AddDays(3);
                var accessToken = jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
                var refreshToken = jwtTokenService.CreateRefreshToken(sessionId,user.Id,expiresAt);

                await sessionService.CreateSessionAsync(sessionId, user.Id, refreshToken, expiresAt);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Expires = expiresAt,
                   // Secure = false; should be true
                };

                httpContext.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);

                return Results.Ok(new SuccessfulLoginDto(accessToken));
            });

            app.MapPost("api/accessToken", async (UserManager<ForumUser> userManager, JwtTokenService jwtTokenService,HttpContext httpContext, SessionService sessionService) =>
             {
                 if(!httpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
                 {
                     return Results.UnprocessableEntity("Token not found");
                 }

                 if (!jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
                 {
                     return Results.UnprocessableEntity("tryparse");
                 }

                 var sessionId = claims.FindFirstValue("SessionId");
                 if(string.IsNullOrWhiteSpace(sessionId))
                 {
                     return Results.UnprocessableEntity();
                 }

                 var sessionIdAsGuid = Guid.Parse(sessionId);
                 if(!await sessionService.IsSessionValidAsync(sessionIdAsGuid,refreshToken))
                 {
                     return Results.UnprocessableEntity();
                 }



                 var userId = claims.FindFirstValue(JwtRegisteredClaimNames.Sub);

                 var user = await userManager.FindByIdAsync(userId);
                 if (user == null)
                 {
                     return Results.UnprocessableEntity("Invalid token");
                 }

                 var roles = await userManager.GetRolesAsync(user);
                 var expiresAt = DateTime.UtcNow.AddDays(3);
                 var accessToken = jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
                 var newrefreshToken = jwtTokenService.CreateRefreshToken(sessionIdAsGuid, user.Id, expiresAt);

                 var cookieOptions = new CookieOptions
                 {
                     HttpOnly = true,
                     SameSite = SameSiteMode.None,
                     Expires = expiresAt,
                     // Secure = false; should be true
                 };

                 httpContext.Response.Cookies.Append("RefreshToken", newrefreshToken, cookieOptions);

                 await sessionService.ExtendSessionAsync(sessionIdAsGuid, newrefreshToken, expiresAt);

                 return Results.Ok(new SuccessfulLoginDto(accessToken));
             });


            app.MapPost("api/logout", async (UserManager<ForumUser> userManager, JwtTokenService jwtTokenService, HttpContext httpContext, SessionService sessionService) =>
            {
                if (!httpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
                {
                    return Results.UnprocessableEntity("http");
                }

                if (!jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
                {
                    return Results.UnprocessableEntity("tryparse");
                }

                var sessionId = claims.FindFirstValue("SessionId");
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return Results.UnprocessableEntity();
                }


                await sessionService.InvalidSessionAsync(Guid.Parse(sessionId));
                httpContext.Response.Cookies.Delete("RefreshToken");




                return Results.Ok();
            });

        }

        public record RegisterUserDto(string Username, string Email, string Password);
        public record LoginDto(string Username, string Password);
        public record SuccessfulLoginDto(string AccessToken);
    }
}
