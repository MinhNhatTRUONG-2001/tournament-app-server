using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace tournament_app_server
{
    public class TokenValidation
    {
        public static JwtSecurityToken ValidateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                // Note: manually padding to 512 bits if it is a short key, as the SymmetricSignatureProvider does not do the HMACSHA512 RFC2104 padding for you.
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY").PadRight(512 / 8, '\0'))),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha512]
            };
            // Validate the token
            handler.ValidateToken(token, validationParameters, out var validatedToken);
            return handler.ReadJwtToken(token);
        }
    }
}
