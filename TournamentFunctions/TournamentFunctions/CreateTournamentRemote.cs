using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Kalkatos.Tournament.Actions;
using Kalkatos.Tournament.Model;
using Azure.Storage.Blobs.Specialized;
using System.Text;

namespace Kalkatos.TournamentFunctions
{
	public static class CreateTournamentRemote
	{
        [FunctionName("CreateTournamentRemote")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] CreateTournamentRequest request,
			ILogger log)
        {
            log.LogInformation("Started CreateTournamentRemote.");

			// Request validations
            if (request == null)
            {
                string error = "Request is null.";
                log.LogError(error);
                return new OkObjectResult(error);
            }

            if (string.IsNullOrEmpty(request.RoomId))
            {
				string error = "Request Room Id must be supplied.";
				log.LogError(error);
				return new OkObjectResult(error);
			}

			if (request.PlayerIds == null || request.PlayerIds.Length < 2)
			{
				string error = "Request Player Ids must be supplied and contain two or more ids.";
				log.LogError(error);
				return new OkObjectResult(error);
			}

			// Access to blob client
			log.LogInformation("Creating block blob client.");
			string blobClientAuthStr = "DefaultEndpointsProtocol=https;AccountName=tournamentgames917c;AccountKey=0vtWKFBXuYMk+w9RebiByjfHcY6qliWexdpSx69MXnBF/9+LFmTIiw3jHo9JTFlFIRbTxxNaPhQT+AStnvsQWA==;EndpointSuffix=core.windows.net";
			BlockBlobClient blobClient = new BlockBlobClient(blobClientAuthStr, "tournaments", $"{request.RoomId}.json");

			// Read file or create new
			string tournamentSerialized;
			log.LogInformation("Accessing file.");
			if (await blobClient.ExistsAsync())
			{
				// File exists
				using (var stream = await blobClient.OpenReadAsync())
				{
					log.LogInformation("File exists! Getting it.");
					// Read the source file into a byte array.
					byte[] bytes = new byte[stream.Length];
					int numBytesToRead = (int)stream.Length;
					int numBytesRead = 0;
					while (numBytesToRead > 0)
					{
						// Read may return anything from 0 to numBytesToRead.
						int n = stream.Read(bytes, numBytesRead, numBytesToRead);

						// Break when the end of the file is reached.
						if (n == 0)
							break;

						numBytesRead += n;
						numBytesToRead -= n;
					}
					tournamentSerialized = Encoding.UTF8.GetString(bytes);
				}
			}
			else
			{
				// Create tournament file
				log.LogInformation("Creating tournament.");
				Tournament.Model.Tournament tournament = TournamentActions.CreateTournament(request.PlayerIds);
				TournamentInfo tournamentInfo = tournament.GetInfo();
				log.LogInformation("Serializing tournament.");
				tournamentSerialized = JsonConvert.SerializeObject(tournamentInfo, Formatting.Indented);

				using (var stream = await blobClient.OpenWriteAsync(true))
				{
					log.LogInformation("Writing tournament to file.");
					stream.Write(Encoding.UTF8.GetBytes(tournamentSerialized));
				}

				// Create round file
				Round round = tournament.Rounds[0];
				string roundSerialized = JsonConvert.SerializeObject(round.GetInfo());
				blobClient = new BlockBlobClient(blobClientAuthStr, "rounds", $"{round.RoundId}.json");
				using (var stream = await blobClient.OpenWriteAsync(true))
				{
					log.LogInformation("Writing round to file.");
					stream.Write(Encoding.UTF8.GetBytes(roundSerialized));
				}

				// Create match files
				for (int i = 0; i < round.Matches.Length; i++)
				{
					Match match = round.Matches[i];
					string matchSerialized = JsonConvert.SerializeObject(match);
					blobClient = new BlockBlobClient(blobClientAuthStr, "matches", $"{match.MatchId}.json");
					using (var stream = await blobClient.OpenWriteAsync(true))
					{
						log.LogInformation($"Writing match {i} to file.");
						stream.Write(Encoding.UTF8.GetBytes(matchSerialized));
					}
				}
			}

			log.LogInformation("Finished.");
			return new OkObjectResult(tournamentSerialized);
        }
    }

    public class CreateTournamentRequest
    {
        public string RoomId;
        public string[] PlayerIds;
    }
}
/*

iwr `
  -Method Post `
  -Uri https://tournament-functions.azurewebsites.net/api/CreateTournamentRemote?code=UIYCIhecY0apJmI653hiBD6bA4IaL8eoKaC8zphRh6v7AzFuNrs5-w== `
  -Body "{""RoomId"":""TestRoom1"",""PlayerIds"":[""Player1"",""Player2"",""Player3"",""Player4""]}" `
  -Headers @{ "Content-Type"="application/json" }


iwr `
  -Method Post `
  -Uri https://tournament-functions.azurewebsites.net/api/CreateTournamentRemote?code=UIYCIhecY0apJmI653hiBD6bA4IaL8eoKaC8zphRh6v7AzFuNrs5-w== `
  -Body "{""RoomId"":""TestRoom1"",""PlayerIds"":[""Player1"",""Player2"",""Player3"",""Player4""]}" `
  -Headers @{ "Content-Type"="application/json" }


$guid = New-Guid
for ($i = 0; $i -lt 2; $i++)
{
iwr `
  -Method Post `
  -Uri https://tournament-functions.azurewebsites.net/api/CreateTournamentRemote?code=UIYCIhecY0apJmI653hiBD6bA4IaL8eoKaC8zphRh6v7AzFuNrs5-w== `
  -Body "{""RoomId"":""$guid"",""PlayerIds"":[""Player1"",""Player2"",""Player3"",""Player4""]}" `
  -Headers @{ "Content-Type"="application/json" }
}

*/
