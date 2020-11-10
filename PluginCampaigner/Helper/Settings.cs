using System;
using System.Collections.Generic;

namespace PluginCampaigner.Helper
{
    public class Settings
    {
        public string ApiKey { get; set; }

        /// <summary>
        /// Validates the settings input object
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new Exception("The Api Key property must be set");
            }
        }
    }
}