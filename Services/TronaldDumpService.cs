using System.Threading.Tasks;
using MatsueNet.Structures;

namespace MatsueNet.Services
{
    public class TronaldDumpService
    {
        private readonly HttpService _httpService;

        public TronaldDumpService(HttpService service)
        {
            _httpService = service;
        }

        public async Task<DonaldTagGetJson> GetTags()
        {
            return await _httpService.GetWithHostAsync<DonaldTagGetJson>("https://tronalddump.io/tag", "tronalddump.io");
        }

        public async Task<RandomQuoteJson> GetRandom()
        {
            return await _httpService.GetWithHostAsync<RandomQuoteJson>("https://tronalddump.io/random/quote", "tronalddump.io");
        }
    }
}