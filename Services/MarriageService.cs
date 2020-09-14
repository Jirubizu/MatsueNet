using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace MatsueNet.Services
{
    public class MarriageService
    {
        public DatabaseService Database { get; set; }

        public async Task<ulong?> Marry(ulong user, ulong toMarry)
        {
            var marriedTo = await MarriedTo(user);
            if (marriedTo != null)
            {
                return marriedTo;
            }
            // Database.UpdateUser()

            return null;
        }

        public async Task<ulong?> MarriedTo(ulong user)
        {
            var result = await Database.LoadRecordsByUserId(user);
            return result.MarriedTo;
        }

        public async Task Propose(ulong proposing, ulong proposeTo)
        {
            // Implemented in next iteration
            throw new NotSupportedException();
        }

        public async Task ListProposals(ulong user)
        {
            // Implemented in next iteration
            throw new NotSupportedException();
        }

        public async Task AcceptProposal(ulong userToAccept)
        {
            // Implemented in next iteration
            throw new NotSupportedException();
        }

        public async Task DenyProposal(ulong userToDeny)
        {
            // Implemented in next iteration
            throw new NotSupportedException();
        }
    }
}