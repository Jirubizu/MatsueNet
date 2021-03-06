﻿using System.IO;
using MatsueNet.Structures;
using Newtonsoft.Json;

namespace MatsueNet.Services
{
    public class ConfigService
    {
        public ConfigStruct Config { get; }

        public ConfigService(string path)
        {
            Config = JsonConvert.DeserializeObject<ConfigStruct>(File.ReadAllText(path));
        }
    }
}