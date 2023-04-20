namespace DemoCmd
{
    internal class AzureAdHttpClient : HttpClient
    {
        public AzureAdHttpClient(string tenantId, string clientId)
        {
            DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            BaseAddress = new Uri($"https://login.microsoftonline.com/{tenantId}/discovery/keys?appid={clientId}");
        }
    }
}
