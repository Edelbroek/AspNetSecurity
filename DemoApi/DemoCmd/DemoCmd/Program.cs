
using DemoCmd;
using Microsoft.Identity.Client;

const string tenantId = "";
Uri authority = new Uri($"https://login.microsoftonline.com/{tenantId}");
const string clientId = "";
const string clientSecret = "";
const string apiResourceId = ""; // client id from the api
var jwtTokenValidator = new AzureAdJwtTokenValidator(tenantId, apiResourceId);


var app = ConfidentialClientApplicationBuilder.Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority(authority)
    .Build();

var result = await app.AcquireTokenForClient(new[] { $"{apiResourceId}/.default" }).ExecuteAsync();
Console.WriteLine(result.AccessToken);

var isValid = await jwtTokenValidator.ValidateToken(result.AccessToken);
Console.WriteLine(isValid);


var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
var response = await client.GetAsync("https://localhost:7034/WeatherForecast");
Console.WriteLine(response.Content.ToString());
// Retrieve token
