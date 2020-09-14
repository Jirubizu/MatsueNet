using System.Threading.Tasks;
using MatsueNet.Structures;

namespace MatsueNet.Services
{
    public class TronaldDumpService
    {
        public HttpService HttpService { get; set; }
        
        public async Task<DonaldTagGetJson> GetTags()
        {
            return await HttpService.GetWithHostAsync<DonaldTagGetJson>("https://tronalddump.io/tag", "tronalddump.io");
        }

        public async Task<RandomQuoteJson> GetRandom()
        {
            return await HttpService.GetWithHostAsync<RandomQuoteJson>("https://tronalddump.io/random/quote", "tronalddump.io");
        }
    }
}