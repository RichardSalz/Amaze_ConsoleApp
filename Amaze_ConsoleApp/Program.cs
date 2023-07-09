using HightechICT.Amazeing.Client.Rest;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MyApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            Console.WriteLine("Hello!");

            //List to keep track of discovered tiles - move it to traverseMaze

            var discoveredList = new List<string>();

            //Register here

            //RegisterPlayer();

            



        }


        private static AmazeingClient amazeingClient = new AmazeingClient("", client);

        private static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://maze.hightechict.nl"),
        };

    }
}