﻿
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

        const string _subscriptionKey = "06e5b9acb8064452b892004cc25fdfab";
        const string personGroupId = "myfriends";

        private readonly IFaceServiceClient faceClient = new FaceServiceClient(_subscriptionKey, "https://eastus2.api.cognitive.microsoft.com/face/v1.0/");


        public async void IdentifyPerson(IEnumerable<Stream> trainingPathPerson, IEnumerable<Stream> PeopleTest, string namePerson)
        {
            await CreateGroup();

            var person = await faceClient.CreatePersonAsync(personGroupId, namePerson);

            await AddFaceToPerson(personGroupId, person, trainingPathPerson);

            await faceClient.TrainPersonGroupAsync(personGroupId);

            await WaitForTrainedPersonGroup(personGroupId);

            Thread.Sleep(1000);

            var picturesPatch = await SeatchPersonInPictures(personGroupId, PeopleTest);

            // Do the same for Bill and Clare
            await faceClient.DeletePersonGroupAsync(personGroupId);
        }

        private async Task CreateGroup()
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

        private async Task<List<Stream>> SeatchPersonInPictures(string personGroupId, IEnumerable<Stream> pathSearchPeople)
        {
            int transactionCount = 0;

            List<Stream> pictures = new List<Stream>();
            foreach (Stream person in pathSearchPeople)
            {
                if (transactionCount == maxTransactionCount)
                {
                    Thread.Sleep(1000);
                    transactionCount = 0;
                }
                else
                    transactionCount += 1;

                List<Person> people = await IdentifyPersons(personGroupId, person);
                if (people.Any())
                    pictures.Add(person);
            }

            if (transactionCount == 10)
                Thread.Sleep(1000);

            return pictures;
        }

        private async Task<List<Person>> IdentifyPersons(string personGroupId, Stream streamPerson)
        {
            List<Person> people = new List<Person>();
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
