using HightechICT.Amazeing.Client.Rest;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MyApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            Console.WriteLine("Hello! Welcome to the Amazeing Console App");

            //Setup variables
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

                Console.WriteLine(playerInfo.Name, playerInfo.Maze);

                Console.ReadKey();

                //All the mazes with info
                ICollection<MazeInfo> allMazeInfo = await amazeingClient.AllMazes();

                foreach (var maze in allMazeInfo)
                {
                    //var currentPlayerInfo = await amazeingClient.GetPlayerInfo();
                    playerInfo = await TraverseMaze(amazeingClient, playerInfo, maze);
                }
            }
            catch (ApiException ex)
            {
                //We haven't registered yet
                if(ex.StatusCode == 404)
                {
                    try
                    {
                        //Register
                        //This is definitely not good here because we will exit without playing after registration
                        await amazeingClient.RegisterPlayer($"{playerName}");
                    }
                    catch (ApiException registrationException)
                    {
                        Console.WriteLine(registrationException.Message);
                        return;
                    }
                }
            }

            Console.ReadKey();

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
                    return;
                }
                else return;
            }
        }

        //Calls choose next maze, enters it, and get's 
        private static async Task<PlayerInfo> TraverseMaze(AmazeingClient amazeingClient,PlayerInfo playerInfo , MazeInfo currentMazeInfo)
        {
            //Maze-level variables
            List<Coordinate> visitedCoordinates = new List<Coordinate>();

            //Enter a maze - this chooses one and returns the info of the current maze
            await amazeingClient.EnterMaze($"{currentMazeInfo.Name}");
                
            //We just entered a new Maze so we are at 0,0 coordinates
            Coordinate currentCoordinate = new Coordinate(0, 0);

            //We also visited it
            visitedCoordinates.Add(currentCoordinate);

            //Get the possible actions first
            PossibleActionsAndCurrentScore possibleActionsAndCurrentScore = new PossibleActionsAndCurrentScore();

            possibleActionsAndCurrentScore = await amazeingClient.PossibleActions();

            ICollection<MoveAction> possibleMoveActions = possibleActionsAndCurrentScore.PossibleMoveActions;

            //First decide what the goal is, there are 3 stages
            // - Look for scores and collect them by stepping on it. We only need to look further if we don't have the max score
            // - Look for a score collection point
            // - Look for exit point

            //The moveAction we will take 
            MoveAction chosenAction = DecideNextTargetForTraversing(amazeingClient, possibleMoveActions, currentCoordinate, visitedCoordinates);
            possibleActionsAndCurrentScore = await GoToNextTile(amazeingClient, chosenAction.Direction);

            //First stage
            while (currentMazeInfo.PotentialReward > playerInfo.MazeScoreInBag)
            {
                chosenAction = DecideNextTargetForTraversing(amazeingClient, possibleMoveActions, currentCoordinate, visitedCoordinates);

                possibleActionsAndCurrentScore = await amazeingClient.Move(chosenAction.Direction);

                //Set the new coordinates based on the direction we traveled
                currentCoordinate = GetNewCoordinates(currentCoordinate, chosenAction.Direction);

                if (!visitedCoordinates.Contains(currentCoordinate))
                {
                    visitedCoordinates.Add(currentCoordinate);
                }
                Console.WriteLine($"Current coordinates: {currentCoordinate.X} , {currentCoordinate.Y}");

            }
            //Second stage - look for scorecollection

            if (currentMazeInfo.PotentialReward == playerInfo.MazeScoreInHand)
            {
                //chosenAction = DecideNextTargetForScoreCollection();
            }

            //Third stage - look for exit and exit there

            if(playerInfo.MazeScoreInBag == playerInfo.MazeScoreInHand && playerInfo.MazeScoreInBag == currentMazeInfo.PotentialReward)
            {
                //chosenAction = DecideNextTargetForExit();
                await amazeingClient.ExitMaze();
            }

            return await amazeingClient.GetPlayerInfo();
        }

        private static MoveAction DecideNextTargetForTraversing(AmazeingClient amazeingClient, ICollection<MoveAction> possibleMoveActions, Coordinate currentCoordinate, List<Coordinate> visitedCoordinates)
        {
            //Decide which direction to go 

            //Actual task: Decide and return a possibleMoveAction depending on the direction we choose.

            //For now we just take the first available action
            MoveAction chosenAction = new MoveAction();

            var chosenAction2 = new MoveAction();

            //Maybe give a score to each possibility to help decide?
            //This foreach doesn't have to go over everything if we found a suitable candidate to go to
            foreach (var moveAction in possibleMoveActions)
            {
                //If we don't have the coordinate in the list BUT we get back has been visited, it's a looped maze. (Or a bug in the coordinate creation)
                //Also what to do now that we know it's a loop
                //Well we know the X or Y coordinate, despite not knowing where are we on the other axis
                //If we come from left or right, we know X - that didn't change - look for highest Y value with the current X and there we are
                //If we come from top or bottom, we know Y - that didn't change - look for highest X value with the current Y and there we are
                if (!visitedCoordinates.Contains(GetNewCoordinates(currentCoordinate, moveAction.Direction)) && moveAction.HasBeenVisited)
                {
                    //For now we just notify
                    Console.WriteLine("This might be a looped maze");
                }
                //If we haven't visited yet, we do want to visit it
                else if (!moveAction.HasBeenVisited)
                {
                    chosenAction = moveAction;
                    break;
                }
                //If we visited it and it is in the list - skip for now?
                else if (moveAction.HasBeenVisited)
                {
                    //We only want to come here if there is no other option in the list
                }

            }

            //if (chosenAction.Direction != Direction.Up)
            //{

            //}

            //Stage 2 - This can help decide in lookahead
            //if (!visitedCoordinates.Contains(GetNewCoordinates(currentCoordinate, moveAction.Direction)))
            //{

            //}

            return chosenAction;
        }

        //Goes to the chosen Tile, returns the actions possible there
        public static async Task<PossibleActionsAndCurrentScore> GoToNextTile(AmazeingClient amazeingClient, Direction chosenDirection)
        {
            await amazeingClient.Move(chosenDirection);
            return await amazeingClient.PossibleActions();
        }

        private static Coordinate GetNewCoordinates(Coordinate currentCoordinate, Direction chosenDirection)
        {
            if (chosenDirection == Direction.Right)
            {
                return new Coordinate((int)currentCoordinate.X + 1, (int)currentCoordinate.Y);
            }
            else if (chosenDirection == Direction.Down)
            {
                return new Coordinate((int)currentCoordinate.X, (int)currentCoordinate.Y - 1);
            }
            else if (chosenDirection == Direction.Left)
            {
                return new Coordinate((int)currentCoordinate.X - 1, (int)currentCoordinate.Y);
            }
            else if (chosenDirection == Direction.Up)
            {
                return new Coordinate((int)currentCoordinate.X, (int)currentCoordinate.Y + 1);
            }
            else return currentCoordinate;
        }

        private static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://maze.hightechict.nl"),
        };

        public struct Coordinate
        {
            private int x;
            private int y;

            public int X { get { return x; } }
            public int Y { get { return y; } }

            public Coordinate(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

    }
}