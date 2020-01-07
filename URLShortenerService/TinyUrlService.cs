using Microsoft.Extensions.Configuration;
using NLog;
using RestSharp;
using SharedLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace URLShortenerService
{
    public class TinyUrlService : IShortenerService
    {
        private IConfiguration _configuration;

        private static RestClient _restClient = new RestClient("https://tinyurl.com");

        public TinyUrlService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> DoShort(string longUrl)
        {
            SharedLibrary.Logger.LogDebug(">> DoShort");

            var restRequest = new RestRequest($"/api-create.php?url={longUrl}", Method.GET, DataFormat.None);

            IRestResponse restResponse = await _restClient.ExecuteGetTaskAsync(restRequest);

            if (restResponse.StatusCode == System.Net.HttpStatusCode.Created ||
                restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return restResponse.Content;
            }

            return longUrl;
        }
    }
}
