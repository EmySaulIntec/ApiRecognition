
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApiRecognition.GroupPers
{
    public class SearchPerson
    {
        const int maxTransactionCount = 5;

        const string FaceListId = "face";
        const string _subscriptionKey = "06e5b9acb8064452b892004cc25fdfab";


        private readonly IFaceServiceClient faceClient = new FaceServiceClient(_subscriptionKey, "https://eastus2.api.cognitive.microsoft.com/face/v1.0/");


        public async void IdentifyPerson(string[] trainingPathPerson, string pathSearchPeople, string namePerson)
        {

            string personGroupId = "myfriends";

            try
            {
                var group = await faceClient.GetPersonGroupAsync(personGroupId);
                await faceClient.DeletePersonGroupAsync(personGroupId);
            }
            catch (Exception ex)
            {

            }

            await faceClient.CreatePersonGroupAsync(personGroupId, "My Friends");

            var person = await faceClient.CreatePersonAsync(personGroupId, namePerson);

            await AddFaceToPerson(personGroupId, person, trainingPathPerson);

            await faceClient.TrainPersonGroupAsync(personGroupId);

            await WaitForTrainedPersonGroup(personGroupId);

            Thread.Sleep(1000);

            var picturesPatch = await SeatchPersonInPictures(personGroupId, pathSearchPeople);


            // Do the same for Bill and Clare
            await faceClient.DeletePersonGroupAsync(personGroupId);

        }

        private async Task<List<string>> SeatchPersonInPictures(string personGroupId, string pathSearchPeople)
        {
            int transactionCount = 0;

            List<string> pictures = new List<string>();
            foreach (string imagePath in Directory.GetFiles(pathSearchPeople, "*.jpg"))
            {
                if (transactionCount == maxTransactionCount)
                {
                    Thread.Sleep(1000);
                    transactionCount = 0;
                }
                else
                    transactionCount += 1;

                List<Person> people = await IdentifyPersons(personGroupId, imagePath);
                if (people.Any())
                    pictures.Add(imagePath);
            }

            if (transactionCount == 10)
                Thread.Sleep(1000);

            return pictures;
        }

        private async Task<List<Person>> IdentifyPersons(string personGroupId, string testImageFile)
        {
            List<Person> people = new List<Person>();
            try
            {
                using (Stream s = File.OpenRead(testImageFile))
                {

                    Face[] faces;
                    try
                    {
                        faces = await faceClient.DetectAsync(s);
                    }
                    catch (Exception ex)
                    {
                        faces = new Face[3];
                    }


                    Guid[] faceIds = faces.Select(face => face.FaceId).ToArray();


                    IdentifyResult[] results;

                    try
                    {
                        results = await faceClient.IdentifyAsync(personGroupId, faceIds);
                    }
                    catch (Exception ex)
                    {
                        results = new IdentifyResult[2];
                    }

                    foreach (var identifyResult in results)
                    {
                        Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                        if (identifyResult.Candidates.Length == 0)
                        {
                            Console.WriteLine("No one identified");
                        }
                        else
                        {
                            var candidateId = identifyResult.Candidates[0].PersonId;

                            people.Add(new Person()
                            {
                                Name = candidateId.ToString()
                            });

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return people;
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

        private async Task AddFaceToPerson(string personGroupId, Microsoft.ProjectOxford.Face.Contract.CreatePersonResult person, string[] personPictures)
        {
            foreach (string imagePath in personPictures)
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    try
                    {
                        // Detect faces in the image and add to Anna
                        await faceClient.AddPersonFaceAsync(personGroupId, person.PersonId, s);
                    }
                    catch (Exception ex)
                    {

                    }

                    //await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(
                    //    personGroupId, ana.PersonId, s);
                }
            }
        }
    }
}
