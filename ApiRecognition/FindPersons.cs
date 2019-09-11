using ApiRecognition.GroupPers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ApiRecognition
{
    public partial class FindPersons : Form
    {
        SearchPerson p = new SearchPerson();
        public FindPersons()
        {
            InitializeComponent();

            // Set the file dialog to filter for graphics files.
            this.openFileDialog1.Filter =
                "Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|" +
                "All files (*.*)|*.*";

            //  Allow the user to select multiple images.
            this.openFileDialog1.Multiselect = true;
            //                   ^  ^  ^  ^  ^  ^  ^

            this.openFileDialog1.Title = "My Image Browser";
        }

        private void Button1_Click(object sender, EventArgs e)

        {
            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                // Read the files
                foreach (String file in openFileDialog1.FileNames)
                {
                    // Create a PictureBox.
                    try
                    {
                        PictureBox pb = new PictureBox();
                        Image loadedImage = Image.FromFile(file);
                        pb.Height = loadedImage.Height;
                        pb.Width = loadedImage.Width;
                        pb.Image = loadedImage;
                        flowLayoutPanel1.Controls.Add(pb);
                    }
                    catch (SecurityException ex)
                    {
                        // The user lacks appropriate permissions to read files, discover paths, etc.
                        MessageBox.Show("Security error. Please contact your administrator for details.\n\n" +
                            "Error message: " + ex.Message + "\n\n" +
                            "Details (send to Support):\n\n" + ex.StackTrace
                        );
                    }
                    catch (Exception ex)
                    {
                        // Could not load the image - probably related to Windows file system permissions.
                        MessageBox.Show("Cannot display the image: " + file.Substring(file.LastIndexOf('\\'))
                            + ". You may not have permission to read the file, or " +
                            "it may be corrupt.\n\nReported error: " + ex.Message);
                    }
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            txtFolderPath.Text = folderBrowserDialog1.SelectedPath;
        }

        private void AddImage(FileStream streamImageFinded)
        {
            PictureBox pb = new PictureBox();
            Image loadedImage = Image.FromFile(streamImageFinded.Name);
            pb.Height = loadedImage.Height;
            pb.Width = loadedImage.Width;
            pb.Image = loadedImage;
            flowLayoutPanel2.Controls.Add(pb);
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            IEnumerable<FileStream> facePersons = this.openFileDialog1.FileNames.Select(patch => File.OpenRead(patch));

            var facesNotDetected = await p.CreatePerson(facePersons, txtNamePerson.Text);

            MessageBox.Show("Persona agregada");

        }

        private async void button4_Click(object sender, EventArgs e)
        {
            IEnumerable<FileStream> filesTest = Directory.GetFiles(txtFolderPath.Text, "*.jpg", SearchOption.AllDirectories)
                .Select(patch => File.OpenRead(patch)).ToList();

            await p.SearchPersonInPictures(filesTest, (streamImageFinded) =>
            {
                AddImage(streamImageFinded);
            }, checkBox1.Checked);

            await p.DeleteGroup();

            MessageBox.Show("Busqueda Finalizada");
        }

        private async void FindPersons_Load(object sender, EventArgs e)
        {
            await p.CreateGroupAsync();
        }
    }
}
