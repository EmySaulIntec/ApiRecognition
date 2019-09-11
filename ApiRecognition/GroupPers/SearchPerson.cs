
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiRecognition.GroupPers
{
    public class SearchPerson
    {
        const int MAX_TRANSACTION_COUNT = 5;

        const string _subscriptionKey = "06e5b9acb8064452b892004cc25fdfab";
        const string personGroupId = "myfriends";
        List<Person> people = new List<Person>();

        private readonly IFaceServiceClient faceClient = new FaceServiceClient(_subscriptionKey, "https://eastus2.api.cognitive.microsoft.com/face/v1.0/");

        int personCreateds = 0;


        public async Task DeleteGroup()
        {
            await faceClient.DeletePersonGroupAsync(personGroupId);
        }

        public async Task CreatePerson(IEnumerable<Stream> trainingPathPerson, string namePerson)
        {
            CreatePersonResult person = await faceClient.CreatePersonAsync(personGroupId, namePerson);

            people.Add(new Person()
            {
                Name = namePerson,
                PersonId = person.PersonId
            });

            await AddFaceToPerson(personGroupId, person, trainingPathPerson);

            await faceClient.TrainPersonGroupAsync(personGroupId);

            await WaitForTrainedPersonGroup(personGroupId);

            personCreateds += 1;
        }

        public async Task CreateGroupAsync()
        {
            try
            {
                var group = await faceClient.GetPersonGroupAsync(personGroupId);
                await faceClient.DeletePersonGroupAsync(personGroupId);
            }
            catch (Exception ex)
            {

            }
            await faceClient.CreatePersonGroupAsync(personGroupId, "My Friends");
        }

        public async Task SearchPersonInPictures(IEnumerable<FileStream> pathSearchPeople, Action<FileStream> processImageAction = null, bool personTogueter = false)
        {
            int transactionCount = 0;

            foreach (FileStream pictureToSearch in pathSearchPeople)
            {
                if (transactionCount == MAX_TRANSACTION_COUNT)
                {
                    await Task.Delay(1000);
                    transactionCount = 0;
                }
                else
                    transactionCount += 1;

                var isIdentifiedInPictured = await IdentifyPersons(personGroupId, pictureToSearch, personTogueter);

                if (isIdentifiedInPictured)
                    processImageAction?.Invoke(pictureToSearch);

            }

            if (transactionCount == 10)
                await Task.Delay(1000);

        }

        private async Task<bool> IdentifyPersons(string personGroupId, FileStream streamPerson, bool personTogueter)
        {
            try
            {
                Face[] faces;
                try
                {
                    faces = await faceClient.DetectAsync(streamPerson);
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

                return (personTogueter && results.Count(e => e.Candidates.Any()) == personCreateds) || (!personTogueter && results.Any(e => e.Candidates.Any()));
            }
            catch (Exception ex)
            {
                return false;
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

        private async Task AddFaceToPerson(string personGroupId, CreatePersonResult person, IEnumerable<Stream> personPictures)
        {
            foreach (var streamFace in personPictures)
            {
                try
                {
                    // Detect faces in the image and add to Anna
                    await faceClient.AddPersonFaceAsync(personGroupId, person.PersonId, streamFace);
                }
                catch (Exception ex)
                {

                }

            }
        }
    }
}
