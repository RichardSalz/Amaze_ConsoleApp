using ExtensionMethods;
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
            //The maze we choose to enter, need to make this choice dynamic
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

                //This is the info about a single maze we need this only where we decide which maze to enter
                MazeInfo currentMazeInfo = new MazeInfo();

                PossibleActionsAndCurrentScore possibleActionsAndCurrentScore = new PossibleActionsAndCurrentScore();

                possibleActionsAndCurrentScore = await amazeingClient.PossibleActions();

                string readableInfo;

                if(possibleActionsAndCurrentScore.CurrentScoreInBag == 0)
                {
                    readableInfo = "No score yet in bag!";
                }
                else readableInfo = possibleActionsAndCurrentScore.CurrentScoreInBag.ToString();

                Console.WriteLine(readableInfo);

                //Make a map somehow
                //This will be a coordinate system - need to assign coordinate beside (together?) with the int ID
                //Encode coordinate into id
                //Decode coordinate from id 

                //Tag current tile

                await amazeingClient.TagTile();

                //Decide where we can go and where we should go - compare map? do we need map if we can just look ahead
                //Need to save those tiles, that we have visited but need to visit again bc didn't visit one of their neighbours

                ICollection<MoveAction> possibleMoveActions = possibleActionsAndCurrentScore.PossibleMoveActions;

                foreach (var action in possibleMoveActions)
                {
                    Console.WriteLine(action.Direction.ToString());

                    //amazeingClient.Move(action.Direction);
                }

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

namespace ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static Task TagTile(this AmazeingClient amazeingClient)
        {
            return TagTile(CancellationToken.None, amazeingClient);
        }

        public static async Task TagTile(CancellationToken cancellationToken, AmazeingClient amazeingClient)
        {
            client.DefaultRequestHeaders.Add("Authorization", "HTI Thanks You [3KE]");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var tagValue = 10;
            string BaseAddress = "https://maze.hightechict.nl";
            StringBuilder urlBuilder_ = new StringBuilder();
            urlBuilder_.Append((BaseAddress != null) ? BaseAddress.TrimEnd('/') : "").Append("/api/maze/tag?");
            urlBuilder_.Append("tagValue" + "=").Append(tagValue.ToString());
            //urlBuilder_.Length--;
            HttpClient client_ = client;
            try
            {
                using HttpRequestMessage request_ = new HttpRequestMessage();
                request_.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                string url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);
                HttpResponseMessage response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                try
                {
                    Dictionary<string, IEnumerable<string>> headers_ = response_.Headers.ToDictionary<KeyValuePair<string, IEnumerable<string>>, string, IEnumerable<string>>((KeyValuePair<string, IEnumerable<string>> h_) => h_.Key, (KeyValuePair<string, IEnumerable<string>> h_) => h_.Value);
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (KeyValuePair<string, IEnumerable<string>> item_ in response_.Content.Headers)
                        {
                            headers_[item_.Key] = item_.Value;
                        }
                    }

                    string status_ = ((int)response_.StatusCode).ToString();
                    if (status_ == "200")
                    {
                        //return (await ReadObjectResponse<PossibleActionsAndCurrentScore>(response_, headers_).ConfigureAwait(continueOnCapturedContext: false)).Object;
                        return;
                    }

                    if (status_ == "412")
                    {
                        string text = ((response_.Content != null) ? (await response_.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false)) : string.Empty);
                        string responseText_ = text;
                        throw new ApiException("You haven't entered a maze yet. Use the enter action to get this party started.", (int)response_.StatusCode, responseText_, headers_, null);
                    }

                    if (status_ != "200" && status_ != "204")
                    {
                        string text2 = ((response_.Content != null) ? (await response_.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false)) : null);
                        string responseData_ = text2;
                        throw new ApiException("The HTTP status code of the response was not expected (" + (int)response_.StatusCode + ").", (int)response_.StatusCode, responseData_, headers_, null);
                    }

                    return;
                }
                finally
                {
                    response_?.Dispose();
                }
            }
            finally
            {

            }
        }

        private static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://maze.hightechict.nl"),
        };

    }
}