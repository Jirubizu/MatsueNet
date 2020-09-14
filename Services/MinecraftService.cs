using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MatsueNet.Structures;

namespace MatsueNet.Services
{
    public class MinecraftService
    {
        public HttpService Http { get; set; }

        public async Task<MinecraftUserJson> GetUuid(string username)
        {
            return await Http.GetJsonAsync<MinecraftUserJson>("https://api.mojang.com/users/profiles/minecraft/" + username);
        }

        public async Task<List<MinecraftNamesJson>> GetNames(string username)
        {
            var result = await GetUuid(username);
            return await Http.GetJsonAsync<List<MinecraftNamesJson>>($"https://api.mojang.com/user/profiles/{result.Uuid}/names");
        }
    }
}