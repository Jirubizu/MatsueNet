using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace MatsueNet.Services
{
    public class MarriageService
    {
        private readonly DatabaseService _databaseService;
        private readonly DiscordShardedClient _client;

        public MarriageService(DatabaseService databaseService, DiscordShardedClient client)
        {
            _databaseService = databaseService;
            _client = client;
        }

        public async Task<ulong?> Marry(ulong user,  ulong toMarry)
        {
            var marriedTo = await MarriedTo(user).ConfigureAwait(false);
            if (marriedTo != null)
            {
                return marriedTo;
            }
            // _databaseService.UpdateUser()
            
            return null;
        }
        
        public async Task<ulong?> MarriedTo(ulong user)
        {
            var result = await _databaseService.LoadRecordsByUserId(user);
            return result.MarriedTo;
        }

        public async Task Propose(ulong proposing, ulong proposeTo)
        {
            
        }

        public async Task ListProposals(ulong user)
        {
            
        }

        public async Task AcceptProposal(ulong userToAccept)
        {
            
        }

        public async Task DenyProposal(ulong userToDeny)
        {
            
        }
        
    }
}