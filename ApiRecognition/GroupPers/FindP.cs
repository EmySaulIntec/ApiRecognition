using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiRecognition.GroupPers
{
    public class FindP
    {
        const string FaceListId = "face";
        const string _subscriptionKey = "349042f6f0684a7093a55eda2a2d6660";

        const string urlBaseData = @"C:\Users\Emy\Downloads\Compressed\Cognitive-Face-Windows-master\Cognitive-Face-Windows-master\Data";

        private readonly IFaceServiceClient faceClient = new FaceServiceClient(_subscriptionKey, "https://eastus2.api.cognitive.microsoft.com/face/v1.0/");

        public async void CreatePersons()
        {

            // Create an empty PersonGroup
            string personGroupId = "myfriends";
            //await faceClient.PersonGroup.CreateAsync(personGroupId, "My Friends");
            await faceClient.CreatePersonGroupAsync(personGroupId, "My Friends");
            //await faceClient.DeletePersonGroupAsync(personGroupId);

            // Define Anna

            var dad = await faceClient.CreatePersonAsync(personGroupId, "Dad");
            var mom = await faceClient.CreatePersonAsync(personGroupId, "mom");
            var girl = await faceClient.CreatePersonAsync(personGroupId, "girl");

            //CreatePersonResult friend1 = await faceClient.PersonGroupPerson.CreateAsync(
            //    // Id of the PersonGroup that the person belonged to
            //    personGroupId,
            //    // Name of the person
            //    "Anna"
            //);

            // Define Bill and Clare in the same way



            // Directory contains image files of Anna
            string dadDir = $@"{urlBaseData}\PersonGroup\Family1-Dad";

            string momDir = $@"{urlBaseData}\PersonGroup\Family1-Mom";
            string girlDad = $@"{urlBaseData}\PersonGroup\Family1-Daughter";

            await AddFaceToPerson(personGroupId, dad, dadDir);
            await AddFaceToPerson(personGroupId, mom, momDir);
            await AddFaceToPerson(personGroupId, girl, girlDad);

            await faceClient.TrainPersonGroupAsync(personGroupId);

            await WaitForTrainedPersonGroup(personGroupId);

            string familyPerson = $@"{urlBaseData}\identification1.jpg";
            await IdentifyPersons(personGroupId, familyPerson);

            // Do the same for Bill and Clare

        }

        private async Task IdentifyPersons(string personGroupId, string testImageFile)
        {

            using (Stream s = File.OpenRead(testImageFile))
            {
                var faces = await faceClient.DetectAsync(s);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                var results = await faceClient.IdentifyAsync(personGroupId, faceIds);
                foreach (var identifyResult in results)
                {
                    Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                    if (identifyResult.Candidates.Length == 0)
                    {
                        Console.WriteLine("No one identified");
                    }
                    else
                    {
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceClient.GetPersonAsync(personGroupId, candidateId);
                        //var person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId);
                        Console.WriteLine("Identified as {0}", person.Name);
                    }
                }
            }
        }

        private async Task WaitForTrainedPersonGroup(string personGroupId)
        {
            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                if (trainingStatus.Status != Status.Running)
                {
                    break;
                }

                await Task.Delay(1000);
            }
        }

        private async Task AddFaceToPerson(string personGroupId, Microsoft.ProjectOxford.Face.Contract.CreatePersonResult person, string friendImageDir)
        {
            foreach (string imagePath in Directory.GetFiles(friendImageDir, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    // Detect faces in the image and add to Anna
                    await faceClient.AddPersonFaceAsync(personGroupId, person.PersonId, s);

                    //await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(
                    //    personGroupId, ana.PersonId, s);
                }
            }
        }
    }
}
