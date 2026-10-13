using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;

namespace LeaseBackend.API.ActionFilter
{
    public class SSOTokenValidateFilter : IAuthorizationFilter
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContext;

        public SSOTokenValidateFilter(IConfiguration configuration, IHttpContextAccessor httpContext)
        {
            this.configuration = configuration;
            this.httpContext = httpContext;
        }

        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if the action is allowed for anonymous access
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em is AllowAnonymousAttribute);
            if (allowAnonymous)
            {
                return; // Skip token validation if anonymous access is allowed
            }

            // Get the Authorization header
            var authorizationHeader = httpContext.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                SetUnauthorized(context);
                return;
            }

            // Extract the Bearer token from the Authorization header
            var token = authorizationHeader.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase)
                        ? authorizationHeader.Substring("Bearer ".Length).Trim()
                        : null;

          

            if (string.IsNullOrEmpty(token))
            {
                SetUnauthorized(context);
                return;
            }

            bool isValidToken = false;

            try
            {
                if (IsOIDCToken(token))
                {
                    isValidToken = await ValidateOIDCTokenAsync(token);  // Ensure async token validation
                }
                else
                {
                    bool isInteger = int.TryParse(token, out int result);

                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging (logging can be added here)
                isValidToken = false;
            }

            // If the token is invalid, return unauthorized
            if (!isValidToken)
            {
                SetUnauthorized(context);
                return;
            }
        }

        private void SetUnauthorized(AuthorizationFilterContext context)
        {
            context.Result = new JsonResult(new { message = "User is unauthorized.", statusCode = StatusCodes.Status401Unauthorized })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        private bool IsOIDCToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);


                var issuer = jwtToken.Claims.FirstOrDefault(x => x.Type == "iss")?.Value;
                var expectedIssuer = "https://app2.dfccil.com";

                var Environment = configuration["DeploymentModes"]?.ToString().Trim();
                expectedIssuer = GetSSOBaseUrl(Environment);

                return issuer == expectedIssuer;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ValidateOIDCTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var username = jwtToken.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value ??
                               jwtToken.Claims.FirstOrDefault(x => x.Type == "username")?.Value ??
                               jwtToken.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value ??
                               jwtToken.Claims.FirstOrDefault(x => x.Type == "name")?.Value;

                if (string.IsNullOrEmpty(username))
                {
                    return false;
                }


                httpContext.HttpContext.Items["EmployeeCode"] = username;

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                return false;
            }
        }

        private string GetSSOBaseUrl(string environment)
        {
            return environment switch
            {
                "DFCCIL" => configuration["ApiBaseUrlsProd:SSO"],
                "DFCCIL_UAT" => configuration["ApiBaseUrlsDfcuat:SSO"],
                _ => configuration["ApiBaseUrlsCetpauat:SSO"]
            };
        }
    }

    // Root class structure to map SSO user details
    public class Root
    {
        public string sub { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string email_verified { get; set; }
        public string display_name { get; set; }
        public string UserId { get; set; }
        public string username { get; set; }
        public string preferred_username { get; set; }
        public string EmailId { get; set; }
        public string UnitName { get; set; }
        public string UnitId { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Level { get; set; }
        public string role { get; set; }
    }
}
