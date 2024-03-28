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
                var response = await _httpClient.GetStringAsync(url);
                return response.Trim() == "1";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred: {ex.Message}");
                return false;
            }
        }
    }
}
