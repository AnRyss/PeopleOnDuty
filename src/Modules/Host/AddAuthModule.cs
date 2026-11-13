using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;

namespace src.Modules.Host
{
    public static class AuthExtension
    {
        public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration config)
        {
            var jwt = config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key missing")));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidIssuer = jwt["Issuer"],
         ValidateAudience = true,
         ValidAudience = jwt["Audience"],
         ValidateLifetime = true,
         IssuerSigningKey = key,
         ValidateIssuerSigningKey = true,
         ClockSkew = TimeSpan.FromSeconds(30)
     });
            services.AddAuthorization();
            return services;
        }
    }

}