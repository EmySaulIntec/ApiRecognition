
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

        const string SUBSCRIPTION_KEY = "06e5b9acb8064452b892004cc25fdfab";
        const string PERSON_GROUP_ID = "myfriends";

        private int PersonCreateds = 0;

        private readonly IFaceServiceClient faceClient = new FaceServiceClient(SUBSCRIPTION_KEY, "https://eastus2.api.cognitive.microsoft.com/face/v1.0/");

        public async Task DeleteGroup()
        {
            await faceClient.DeletePersonGroupAsync(PERSON_GROUP_ID);
        }

        public async Task<List<Stream>> CreatePerson(IEnumerable<Stream> trainingPathPerson, string namePerson)
        {
            CreatePersonResult person = await faceClient.CreatePersonAsync(PERSON_GROUP_ID, namePerson);

            var facesNotDetected = await AddFaceToPerson(PERSON_GROUP_ID, person, trainingPathPerson);

            await faceClient.TrainPersonGroupAsync(PERSON_GROUP_ID);

            await WaitForTrainedPersonGroup(PERSON_GROUP_ID);

            PersonCreateds += 1;

            return facesNotDetected;
        }

        public async Task CreateGroupAsync()
        {
            var groups = await faceClient.ListPersonGroupsAsync();

            if (groups.Any(e => e.PersonGroupId == PERSON_GROUP_ID))
                await faceClient.DeletePersonGroupAsync(PERSON_GROUP_ID);

            await faceClient.CreatePersonGroupAsync(PERSON_GROUP_ID, "My Friends");
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

                var isIdentifiedInPictured = await IdentifyPersons(PERSON_GROUP_ID, pictureToSearch, personTogueter);

                if (isIdentifiedInPictured)
                    processImageAction?.Invoke(pictureToSearch);
            }

            if (transactionCount == 10)
                await Task.Delay(1000);
        }

        private async Task<bool> IdentifyPersons(string personGroupId, FileStream streamPerson, bool personTogueter)
        {
            Face[] faces = await faceClient.DetectAsync(streamPerson);

            Guid[] faceIds = faces.Select(face => face.FaceId).ToArray();

            if (faceIds.Length == 0)
                return false;

            IdentifyResult[] results = await faceClient.IdentifyAsync(personGroupId, faceIds);

            return (personTogueter && results.Count(e => e.Candidates.Any()) == PersonCreateds) || (!personTogueter && results.Any(e => e.Candidates.Any()));
        }

        private async Task WaitForTrainedPersonGroup(string personGroupId)
        {
            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                if (trainingStatus.Status != Status.Running)
                    break;

                await Task.Delay(1000);
            }
        }

        private async Task<List<Stream>> AddFaceToPerson(string personGroupId, CreatePersonResult person, IEnumerable<Stream> personPictures)
        {
            List<Stream> facesNotDetected = new List<Stream>();

            foreach (var streamFace in personPictures)
            {
                try
                {
                    await faceClient.AddPersonFaceAsync(personGroupId, person.PersonId, streamFace);
                }
                catch (FaceAPIException ex)
                {
                    facesNotDetected.Add(streamFace);
                }
            }
            return facesNotDetected;
        }
    }
}
