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
            string currentlyChosenMaze = "Example Maze";

            client.DefaultRequestHeaders.Add("Authorization", $"{authToken}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            AmazeingClient amazeingClient = new AmazeingClient("https://maze.hightechict.nl", client);

            try
            {
                //See if we registered, if yes then read info

                PlayerInfo playerInfo = new PlayerInfo();

                playerInfo = await amazeingClient.GetPlayerInfo();

                Console.WriteLine(playerInfo.Name, playerInfo.Maze);

                Console.ReadKey();

                //Let's see if we are in a maze already

                //If not, then enter one (TODO: Change it to search one from allMazes - also flag here what we have already entered)
                if (!playerInfo.IsInMaze)
                {
                    try
                    {
                        await amazeingClient.EnterMaze($"{currentlyChosenMaze}");
                    }
                    catch (ApiException mazeEnterException)
                    {
                        Console.WriteLine(mazeEnterException.Message);
                        return;
                    }
                }
                //If yes, we traverse it

                Console.WriteLine($"Current Maze: {playerInfo.Maze}");
                Console.ReadKey();

                //Get the possible actions first

                MazeInfo currentMazeInfo = new MazeInfo();

                PossibleActionsAndCurrentScore possibleActionsAndCurrentScore = new PossibleActionsAndCurrentScore();

                possibleActionsAndCurrentScore = await amazeingClient.PossibleActions();

                Console.WriteLine(possibleActionsAndCurrentScore.PossibleMoveActions.ToString());
                Console.ReadKey();

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
                        Console.WriteLine(registrationException.Message);
                        return;
                    }
                }
            }

            //Easily delete the player when we finished
            Console.WriteLine("Do you want to delete the player before exiting the program? Type  \"yes\" if yes");

            var answer = Console.ReadLine();

            if(answer != null && answer.ToString() == "yes")
            {
                Console.WriteLine("Are you sure you want to delete the player and forget all what you've done so far? Press ENTER then!");
                Console.ReadKey();
                if (Console.ReadKey().Key == ConsoleKey.Enter)
                {
                    await amazeingClient.ForgetPlayer();
                    Console.WriteLine($"{amazeingClient.ForgetPlayer().Status}");
                    return;
                }
                else return;
            }
        }

        private static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://maze.hightechict.nl"),
        };

    }
}