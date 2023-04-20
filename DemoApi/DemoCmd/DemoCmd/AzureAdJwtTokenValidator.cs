using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Nodes;

namespace DemoCmd
{
    public class AzureAdJwtTokenValidator
    {
        private readonly AzureAdHttpClient _azureAdClient;
        private readonly string _tenantId;
        private readonly string _clientId;

        public AzureAdJwtTokenValidator(string tenantId, string clientId)
        {
            _azureAdClient = new AzureAdHttpClient(tenantId, clientId);
            _tenantId = tenantId;
            _clientId = clientId;
        }

        public async Task<bool> ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;

            try
            {
                var issuerSigningKeys = await RetrieveAzureAdPublicKeys();

                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    IssuerSigningKeys = issuerSigningKeys,
                    ValidAudience = _clientId,
                    ValidIssuer = $"https://sts.windows.net/{_tenantId}/",
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                return jwtToken is not null;
            }
            catch
            {
                return false;
            }
        }

        private async Task<IEnumerable<X509SecurityKey>> RetrieveAzureAdPublicKeys()
        {
            var response = await _azureAdClient.GetAsync(string.Empty);
            response.EnsureSuccessStatusCode();
            var stringContent = await response.Content.ReadAsStringAsync();

            var jsonObject = JsonNode.Parse(stringContent)
                ?? throw new InvalidOperationException("Could not parse string to jsonobject");

            return GetSecurityKeys(jsonObject);
        }

        private IEnumerable<X509SecurityKey> GetSecurityKeys(JsonNode jsonObject)
        {
            var keysObject = jsonObject["keys"]?.AsArray()
                ?? throw new InvalidOperationException("Could not retrieve public key!");

            foreach (var keyObject in keysObject)
            {
                var certificateString = keyObject["x5c"]?[0]?.ToString()
                    ?? throw new InvalidOperationException("Structure of jsonobject does not match the implementation!");

                var certificate = new X509Certificate2(Convert.FromBase64String(certificateString));
                yield return new Microsoft.IdentityModel.Tokens.X509SecurityKey(certificate);

            }
        }
    }
}
