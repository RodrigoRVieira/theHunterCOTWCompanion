using Microsoft.Extensions.Configuration;
using RestSharp;
using SharedLibrary;
using SharedLibrary.Interfaces;
using System;
using System.Threading.Tasks;

namespace URLShortenerService
{
    public class BitLyService : IShortenerService
    {
        private IConfiguration _configuration;

        private static RestClient _restClient = new RestClient("https://api-ssl.bitly.com");

        private Parameter authorizationHeader;

        public BitLyService(IConfiguration configuration)
        {
            _configuration = configuration;

            authorizationHeader = new Parameter("Authorization", _configuration.GetSection("Token").Value, ParameterType.HttpHeader);
        }

        public async Task<string> DoShort(string longUrl)
        {
            Logger.LogDebug(">> DoShort");

            if (!_restClient.DefaultParameters.Contains(authorizationHeader))
            {
                _restClient.DefaultParameters.Add(authorizationHeader);
            }

            var restRequest = new RestRequest("v4/shorten", Method.POST, DataFormat.Json);

            restRequest.AddJsonBody(new { domain = "bit.ly", long_url = longUrl });

            IRestResponse restResponse = await _restClient.ExecutePostTaskAsync(restRequest);

            if (restResponse.StatusCode == System.Net.HttpStatusCode.Created ||
                restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(restResponse.Content);

                return response.link;
            }

            return longUrl;
        }
    }
}
