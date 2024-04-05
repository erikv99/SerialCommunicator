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
            return await _getRemoteKillSwitchStatusAsync("https://github.com/erikv99/data/blob/main/r_ks_00.txt");
        }

        private async Task<bool> _getRemoteKillSwitchStatusAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
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
                _logger.LogError(ex, $"An error occurred: {ex.Message}");
                return false;
            }
        }
    }
}
