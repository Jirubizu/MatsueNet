using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MatsueNet.Structures;

namespace MatsueNet.Services
{
    public class MinecraftService
    {
        private readonly HttpService _httpService;
        
        public MinecraftService(HttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<MinecraftUserJson> GetUuid(string username)
        {
            return await _httpService.GetJsonAsync<MinecraftUserJson>("https://api.mojang.com/users/profiles/minecraft/" + username);
        }

        public async Task<List<MinecraftNamesJson>> GetNames(string username)
        {
            var result = await GetUuid(username);
            return await _httpService.GetJsonAsync<List<MinecraftNamesJson>>($"https://api.mojang.com/user/profiles/{result.Uuid}/names");
        }
    }
}