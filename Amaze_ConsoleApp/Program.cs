using HightechICT.Amazeing.Client.Rest;
using System.Globalization;
using System.Net.Http;
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
            MazeInfo currentMazeInfo = new MazeInfo();

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

                //All the mazes with info
                ICollection<MazeInfo> allMazeInfo = await amazeingClient.AllMazes();

                TraverseAMaze(amazeingClient, playerInfo, allMazeInfo);


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

        //Calls choose next maze, enters it, and get's 
        private static async void TraverseAMaze(AmazeingClient amazeingClient,PlayerInfo playerInfo ,ICollection<MazeInfo> allMazeInfo)
        {
            //If we are in a maze somehow already it's testing phase and we can ignore this

            //If not, then enter one - this just chooses one and returns the list without the current maze
            if (!playerInfo.IsInMaze)
            {
                allMazeInfo = await ChooseNextMaze(amazeingClient, allMazeInfo);
            }
            //Get the possible actions first

            PossibleActionsAndCurrentScore possibleActionsAndCurrentScore = new PossibleActionsAndCurrentScore();

            possibleActionsAndCurrentScore = await amazeingClient.PossibleActions();

            //Tile - how to map this

            //I want a list - where we put coordinates

            //I want to make a method for creating the new coordinates

            //This reduces the need for API calls right?

            //IF moveaction up - add y+1
            //IF moveaction right - add x+1
            //IF moveaction down - add y-1
            //IF moveaction left - add x-1

            //Then we compare 

            //If we have coordinates we could save exit points and collecting points globally, so we can always ask them
            //If we have the current coordinates and where do we want to go we at least can make a direction suggestion
            //So we go in the right direction and don't wast time
            //This suggestion could be a param (optional?) to the moves so it prefers those directions that are the closest



            //MoveActions 

            ICollection<MoveAction> possibleMoveActions = possibleActionsAndCurrentScore.PossibleMoveActions;

            //First decide what the goal is, there are 3 stages
            // - Look for scores and collect them by stepping on it. We only need to look further if we don't have the max score
            // - Look for a score collection point
            // - Look for exit point




            foreach (var adjacentTile in possibleMoveActions)
            {
                //This doesnt need to be a foreach 

                //Option1: - we can only go where we came from
                //We have 1 possibleMoveAction, we go that way which is probably visited (except start)

                //Option2: - we can go where we came from and one more, so
                //We have 2 possibleMoveAction
                // - we go one way - which is not visited
                // - the other is where we coming from

                //Option3:
                //We have 3 possibleMoveAction
                // - one is where we came from
                // - Need to choose between the two
                //IF we have visited one and not the other, then go that way
                //ELSE IF we have visited both 


                //Option4: this is a cross section we can go anywhere
                //We have 4 possibleMoveAction
                // - one where we came from
                // - three others

                //If we haven't visited this tile yet
                if (!adjacentTile.HasBeenVisited)
                {
                    await amazeingClient.Move(adjacentTile.Direction);
                    possibleActionsAndCurrentScore = await amazeingClient.PossibleActions();

                }

            }

            await amazeingClient.ExitMaze();


            //If we have more mazes in our list, we enter the next one
            if(allMazeInfo.Count > 0)
            {
                var updatedPlayerInfo = await amazeingClient.GetPlayerInfo();
                TraverseAMaze(amazeingClient, updatedPlayerInfo, allMazeInfo);
            }
        }

        public static async Task<ICollection<MazeInfo>> ChooseNextMaze(AmazeingClient amazeingClient, ICollection<MazeInfo> allMazeInfo)
        {
            try
            {
                //Choose one to enter from the allMazeInfo list
                foreach (var maze in allMazeInfo)
                {
                    await amazeingClient.EnterMaze($"{maze.Name}");

                    //If we succeed, we need to remove it from the list 
                    allMazeInfo.Remove(maze);
                    //Return the shortened list
                    return allMazeInfo;
                }
                //No more mazes:
                return allMazeInfo;
            }
            catch (ApiException mazeEnterException)
            {
                Console.WriteLine(mazeEnterException.Message);
                return allMazeInfo;
            }
        }

        private static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://maze.hightechict.nl"),
        };

    }
}