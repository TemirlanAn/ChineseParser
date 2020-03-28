using ChineseParser.Parser;
using System.Threading.Tasks;

namespace ChineseParser.Csl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var parserWorker = new ParserWorker("联系");
            var result = await parserWorker.Parse();
            System.Console.WriteLine(result.Pinyn);
            System.Console.WriteLine(result.Translate);
        }
    }
}
