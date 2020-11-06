using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FlyJetsV2.Services
{
    internal class AppConfig
    {
        private static AppConfig _instance;
        private IConfigurationRoot _root;

        private AppConfig()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            _root = configurationBuilder.Build();
        }

        public static AppConfig Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new AppConfig(); 
                }

                return _instance;
            }
        }

        public string GetValue(string key)
        {
            return _root[key];
        }
    }
}
