﻿using System.Net;
using System.Net.Http.Headers;

namespace SerialCommunicator.Services
{
    public class RemoteKillSwitchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RemoteKillSwitchService> _logger;

        public RemoteKillSwitchService(ILogger<RemoteKillSwitchService> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;
        }

        public async Task<bool> IsKillSwitchActive()
        {
            return await _getRemoteKillSwitchStatusAsync("https://raw.githubusercontent.com/erikv99/data/main/r_ks_00.txt");
        }


        private async Task<bool> _getRemoteKillSwitchStatusAsync(string url)

        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
                client.DefaultRequestHeaders.Pragma.Add(new NameValueHeaderValue("no-cache"));
                client.DefaultRequestHeaders.IfModifiedSince = DateTime.UtcNow;

                var response = await client.GetAsync(url);
                
                // Dangerous since NotModified does not equal no active killswitch
                // But this should only happen if we spam it i believe, so we should be good
                // Todo, verify
                if (response.StatusCode != HttpStatusCode.NotModified)
                {
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    return content.Trim() == "1";
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
                return false;
            }
        }
    }
}
