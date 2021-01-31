using FMdotNet__DataAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartImageForm_v1
{
	public partial class Form1 : Form
	{
		private readonly string relativePath = string.Empty;
		private readonly string projectRecordId = "1404";
		private readonly string filePath = @"C:\Users\user\Desktop\Smart_Image_Form_Data.txt";
		private readonly string server = "prod01.thesmartdesigner.com";
		private readonly string database = "V6_Data_99999";
		private readonly string userName = "tss";
		private readonly string password = "2504B";
		private readonly string itemLayout = "|Specs";
		private readonly string productLayout = "|Products";
		private readonly string graphicLayout = "|Graphics";
		private FMS18 fmServer;
		private string token = string.Empty;
		private IEnumerable<string> fileLines = new List<string>();

		public Form1()
		{
			InitializeComponent();
			fmServer = new FMS18(server, userName, password);
			relativePath = new Uri(Directory.GetCurrentDirectory()).LocalPath.Replace("bin", "").Replace("Debug", "").Replace(@"\\", @"\");
		}

		private async void Form1_Load(object sender, EventArgs e)
		{
			logoPictureBox.Image = Image.FromFile($@"{relativePath}Images\logo.jpg");
			await AuthenticateFileMakerServer();

			try
			{
				fileLines = File.ReadLines(filePath).ToList();
				await PopulateData(173956, 91806);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error: {ex.Message}");
			}
		}

		private async Task PopulateData(int itemRecordId, int productRecordId)
		{
			var graphicsPortalData = await GetProductGraphicsPortal(productRecordId);

			fmServer.SetLayout(itemLayout);
			var getItemRequest = fmServer.FindRequest(itemRecordId);
			var getItemResponse = await getItemRequest.Execute();
			var itemDetails = getItemResponse.data.foundSet.records.First().fieldsAndData;
			lblItemNumber.Text = itemDetails["ItemNumber"];
			lblItemName.Text = itemDetails["ItemName"];

			for (int i = 0; i < 9; i++)
			{
				var currentButton = GetCurrentButton(i);

				if (i < graphicsPortalData.records.Count())
				{
					try
					{
						var currentRow = graphicsPortalData.records.ElementAt(i);
						var file = currentRow.fieldsAndData["File"];
						WebClient client = new WebClient();
						byte[] bytes = client.DownloadData(file);
						MemoryStream ms = new MemoryStream(bytes);
						currentButton.Tag = $"{currentRow.recordId}, {productRecordId}";
						currentButton.BackgroundImage = Image.FromStream(ms);
						currentButton.BackgroundImageLayout = ImageLayout.Stretch;
					}
					catch (Exception ex)
					{
						currentButton.Tag = $", {productRecordId}";
						currentButton.Image = ResizedSelectImage();
						MessageBox.Show($"Error: {ex.Message}");
					}
				}
				else
				{
					currentButton.Image = ResizedSelectImage();
				}
			}
		}

		private async Task AuthenticateFileMakerServer()
		{
			try
			{
				fmServer.SetFile(database);
				token = await fmServer.Authenticate();
			}
			catch (Exception ex)
			{
				if (fmServer.lastErrorCode > 0)
					MessageBox.Show($"FileMaker error code, message: {fmServer.lastErrorCode}, {fmServer.lastErrorMessage}");
				else
					MessageBox.Show(ex.Message);
			}
		}

		private async Task<FMRecordSet> GetProductGraphicsPortal(int productRecordId)
		{
			fmServer.SetLayout(productLayout);
			var getProductRequest = fmServer.FindRequest(productRecordId);
			var getProductResponse = await getProductRequest.Execute();
			FMRecord findResultRow = getProductResponse.data.foundSet.records.First();
			FMRecordSet graphicsPortalData = null;

			foreach (var relatedData in findResultRow.relatedRecordSets)
			{
				if (!string.IsNullOrEmpty(relatedData.tableLayoutObjectName) && relatedData.tableLayoutObjectName.ToLower() == "graphicsportal")
				{
					graphicsPortalData = relatedData;
					break;
				}
			}

			return graphicsPortalData;
		}

		private void btnImage1_Click(object sender, EventArgs e)
		{
			CreateOrEditImage(sender);
		}

		private void btnImage2_Click(object sender, EventArgs e)
		{
			CreateOrEditImage(sender);
		}

		private void btnImage3_Click(object sender, EventArgs e)
		{
			CreateOrEditImage(sender);
		}

		private void btnImage4_Click(object sender, EventArgs e)
		{
			CreateOrEditImage(sender);
		}

		private void btnImage5_Click(object sender, EventArgs e)
		{
			CreateOrEditImage(sender);
		}

		private void btnImage6_Click(object sender, EventArgs e)
		{
			CreateOrEditImage(sender);
		}

		private void btnImage7_Click(object sender, EventArgs e)
		{
			CreateOrEditImage(sender);
		}

		private void btnImage8_Click(object sender, EventArgs e)
		{
			CreateOrEditImage(sender);
		}

		private void btnImage9_Click(object sender, EventArgs e)
		{
			CreateOrEditImage(sender);
		}

		private string OpenDialog()
		{
			var dialog = new OpenFileDialog();
			dialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.png) | *.jpg; *.jpeg; *.jpe; *.png";
			dialog.Title = "Please select an image";

			if (dialog.ShowDialog() == DialogResult.OK)
				return dialog.FileName;
			else
				return string.Empty;
		}

		private async void CreateOrEditImage(object sender)
		{
			var fileName = OpenDialog();

			if (!string.IsNullOrEmpty(fileName))
			{
				var button = sender as Button;
				var tagList = button.Tag.ToString().Split(',');

				if (string.IsNullOrEmpty(tagList[0]))
					CreateImage(tagList[1], fileName);
				else
					await EditImage(tagList[0], fileName);
			}
		}

		private async Task EditImage(string graphicRecordId, string fileName)
		{
			string fileToUpload = $@"{fileName}";
			FileInfo fileInfo = new FileInfo(fileToUpload);
			fmServer.SetLayout("|Graphics");

			int uploadContainerResponse = await fmServer.UploadFileIntoContainerField(Convert.ToInt32(graphicRecordId), "File", fileInfo);

			if (fmServer.lastErrorCode == 0)
				Console.WriteLine("file uploaded to container");
			else
				Console.WriteLine(fmServer.lastErrorCode.ToString() + " - " + fmServer.lastErrorMessage);
		}

		private void CreateImage(string productRecordId, string fileName)
		{

		}

		private Image ResizedSelectImage()
		{
			var imageSize = new Size(194, 119);
			return new Bitmap(Image.FromFile($@"{relativePath}Images\select.png"), imageSize);
		}

		private Image Convertbase64ToImage(string base64String)
		{
			Image image;
			var bytes = Convert.FromBase64String(base64String);
			using (MemoryStream ms = new MemoryStream(bytes))
			{
				image = Image.FromStream(ms);
			}

			return image;
		}

		private Button GetCurrentButton(int index)
		{
			if (index == 0)
				return btnImage1;
			else if (index == 1)
				return btnImage2;
			else if (index == 2)
				return btnImage3;
			else if (index == 3)
				return btnImage4;
			else if (index == 4)
				return btnImage5;
			else if (index == 5)
				return btnImage6;
			else if (index == 6)
				return btnImage7;
			else if (index == 7)
				return btnImage8;
			else
				return btnImage9;
		}
	}
}
