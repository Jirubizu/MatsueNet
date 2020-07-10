using System.Threading.Tasks;

namespace MatsueNet
{
    public class Program
    {
        private static async Task Main(string[] args)
            => await new MatsueNet().SetupAsync();
    }
}