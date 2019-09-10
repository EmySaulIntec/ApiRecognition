using ApiRecognition.GroupPers;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ApiRecognition
{
    public partial class Form1 : Form
    {
        const string FaceListId = "face";
        const string _subscriptionKey = "349042f6f0684a7093a55eda2a2d6660";

        public Form1()
        {
            InitializeComponent();
        }


        static async void MakeRequest()
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

            var uri = "https://eastus2.api.cognitive.microsoft.com/face/v1.0/findsimilars?" + queryString;

            HttpResponseMessage response;

            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes("{body}");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
            }

        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            //await AddPictures();
            FindP findP = new FindP();
            findP.CreatePersons();
            //FaceBox_Click();

            //OpenFileDialog of = new OpenFileDialog();
            ////For any other formats
            //of.Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG";
            //if (of.ShowDialog() == DialogResult.OK)
            //{
            //    pictureBox1.ImageLocation = of.FileName;
            //    //DetectImage(of.FileName);

            //}

        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public async Task AddPictures()
        {
            var faceServiceClientOne = new FaceServiceClient(_subscriptionKey,
                "https://eastus2.api.cognitive.microsoft.com/face/v1.0/");


            // Create a FaceList.
            const string ImageDir = @"C:\Users\Emy\Pictures\PictTest";

            var files = Directory.GetFiles(ImageDir, "*.jpg");

            // Add Faces to the FaceList.
            foreach (var imagePath in files)
            {
                using (Stream stream = File.OpenRead(imagePath))
                {
                    var a = await faceServiceClientOne.DetectAsync(stream);

                }
            }


        }
        public async Task FaceBox_Click()
        {
            try
            {

                //var faceServiceClient = new FaceServiceClient(_subscriptionKey, "https://eastus2.api.cognitive.microsoft.com/face/v1.0/findsimilars");

                //// Perform FindSimilar.
                //const string QueryImagePath = @"C:\Users\Emy\Pictures\Unique2.jpg";
                //var results = new List<SimilarPersistedFace[]>();
                //using (Stream stream = File.OpenRead(QueryImagePath))
                //{
                //    var faces = await faceServiceClient.DetectAsync(stream);
                //    foreach (var face in faces)
                //    {
                //        results.Add(await faceServiceClient.FindSimilarAsync(face.FaceId, FaceListId, 20));
                //    }
                //}
            }
            catch (Exception ex)
            {


            }
        }
    }
}
