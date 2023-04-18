﻿

using WalletApp.Model;

namespace WalletApp.Helper
{
    public class Configuration
    {
        private readonly IConfiguration _configuration;
        public Configuration(IConfiguration configuration) {
            _configuration = configuration;
        }

        public ConfigurationDTO getConfiguration()
        {
            ConfigurationDTO con = new ConfigurationDTO();
            con.Key= _configuration["Jwt:Key"] ?? "";
            con.FireBlocks_BaseURL = _configuration["FireBlocks_BaseURL"] ?? "";
            return con;
        }
    }
}
