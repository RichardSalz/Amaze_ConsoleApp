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

            Console.WriteLine("Hello! Welcome to the Amazeing Console App");

            //Global variables
            string authToken = "HTI Thanks You [3KE]";
            string playerName = "RichardS";

            client.DefaultRequestHeaders.Add("Authorization", $"{authToken}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            AmazeingClient amazeingClient = new AmazeingClient("https://maze.hightechict.nl", client);

            try
            {
                //See if we registered, if yes then read info

                PlayerInfo playerInfo = new PlayerInfo();

                playerInfo = await amazeingClient.GetPlayerInfo();

                Console.WriteLine(playerInfo);
            }
            catch (ApiException ex)
            {
                //We haven't registered yet
                if(ex.StatusCode == 404)
                {
                    try
                    {
                        //Register
                        await amazeingClient.RegisterPlayer($"{playerName}");
                    }
                    catch (ApiException registrationException)
                    {
                        Console.WriteLine(registrationException);
                        return;
                    }
                }
            }
        }

        private static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://maze.hightechict.nl"),
        };

    }
}