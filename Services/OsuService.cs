using System.Threading.Tasks;
using OsuSharp;

namespace MatsueNet.Services
{
    public class OsuService
    {
        private readonly OsuClient _osuClient;

        public OsuService(ConfigService configService)
        {
            _osuClient = new OsuClient(new OsuSharpConfiguration
            {
                ApiKey = configService.Config.OsuToken
            });
        }

        public async Task<User> GetUser(string username, GameMode gameMode)
        {
            return await _osuClient.GetUserByUsernameAsync(username, gameMode);
        }
    }
}