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
			var productPortalAndRecordId = await GetProductGraphicsPortalAndRecordID(Convert.ToInt32(productRecordId));
			var graphicsPortalData = productPortalAndRecordId.GraphicsPortalData;
			var productRecordID = productPortalAndRecordId.RecordID;

			fmServer.SetLayout(itemLayout);
			var getItemBySearchFields = fmServer.FindRequest();
			var searchFields = getItemBySearchFields.SearchCriterium();
			searchFields.AddFieldSearch("specs_PRODUCTS::RecordID", productRecordID);
			searchFields.AddFieldSearch("ProjectRecordID", projectRecordId);
			getItemBySearchFields.AddPortal("Spec_to_Graphics", 9);
			RecordsGetResponse getItemSearchFieldsResponse = await getItemBySearchFields.Execute();
			var itemGraphicsPortalData = getItemSearchFieldsResponse.data.foundSet.records.First().relatedRecordSets.First().records;

			var itemDetails = getItemSearchFieldsResponse.data.foundSet.records.First().fieldsAndData;
			lblItemNumber.Text = itemDetails["ItemNumber"];
			lblItemName.Text = itemDetails["ItemName"];

			for (int i = 0; i < 9; i++)
			{
				var currentButton = GetCurrentButton(i);

				if (i < itemGraphicsPortalData.Count())
				{
					try
					{
						var currentRow = itemGraphicsPortalData.ElementAt(i);
						fmServer.SetLayout(graphicLayout);
						var graphicImageId = currentRow.fieldsAndData["GraphicImage_ID"];
						var getGraphicRequest = fmServer.FindRequest();
						searchFields = getGraphicRequest.SearchCriterium();
						searchFields.AddFieldSearch("ImageRecordID", graphicImageId);
						var getGraphicResponse = await getGraphicRequest.Execute();
						var graphicRecordId = getGraphicResponse.data.foundSet.records.First().recordId;
						var file = getGraphicResponse.data.foundSet.records.First().fieldsAndData["File"];
						WebClient client = new WebClient();
						byte[] bytes = client.DownloadData(file);
						MemoryStream ms = new MemoryStream(bytes);
						currentButton.Tag = $"{graphicRecordId}, {productRecordId}";
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
					currentButton.Tag = $", {productRecordId}";
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

		private async Task<(FMRecordSet GraphicsPortalData, string RecordID)> GetProductGraphicsPortalAndRecordID(int productRecordId)
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

			return (graphicsPortalData, findResultRow.fieldsAndData["RecordID"]);
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
					await CreateImage(tagList[1], fileName, sender);
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

			if (fmServer.lastErrorCode != 0)
				MessageBox.Show(fmServer.lastErrorCode.ToString() + " - " + fmServer.lastErrorMessage);
		}

		private async Task CreateImage(string productRecordId, string fileName, object sender)
		{
			fmServer.SetLayout(productLayout);
			var newEditRequest = fmServer.EditRequest(Convert.ToInt32(productRecordId));
			newEditRequest.AddRelatedField("PositionNumber", "products||SPECGRAPHICS", "1", "graphicsportal");
			var editRequestResponse = await newEditRequest.Execute();
			var errorCheck = fmServer.lastErrorCode;

			var productPortalAndRecordId = await GetProductGraphicsPortalAndRecordID(Convert.ToInt32(productRecordId));
			var productPortalData = productPortalAndRecordId.GraphicsPortalData;

			var latestPortalRecordId = 0;
			if (productPortalData != null)
			{
				var portalDataRecordCount = productPortalData.records.Count();
				latestPortalRecordId = Convert.ToInt32(productPortalData.records.ElementAt(portalDataRecordCount - 1).recordId);
			}

			fmServer.SetLayout(graphicLayout);

			var graphicsRequest = fmServer.NewRecordRequest();
			graphicsRequest.AddField("GraphicURL", "");
			var newGraphicRecordId = await graphicsRequest.Execute();

			if (fmServer.lastErrorCode != 0)
				MessageBox.Show($"Error: {fmServer.lastErrorMessage}");

			FileInfo fileInfo = new FileInfo(fileName);
			fmServer.SetLayout(graphicLayout);

			int uploadContainerResponse = await fmServer.UploadFileIntoContainerField(newGraphicRecordId, "File", fileInfo);

			if (fmServer.lastErrorCode != 0)
				MessageBox.Show(fmServer.lastErrorCode.ToString() + " - " + fmServer.lastErrorMessage);

			var findGraphicRequest = fmServer.FindRequest(newGraphicRecordId);
			var findGraphicResponse = await findGraphicRequest.Execute();
			var graphicFile = findGraphicResponse.data.foundSet.records.First().fieldsAndData["File"];

			fmServer.SetLayout(productLayout);

			newEditRequest = fmServer.EditRequest(Convert.ToInt32(productRecordId));
			newEditRequest.ModifyRelatedField("File", "products||SPECGRAPHICS||GRAPHICS", graphicFile, latestPortalRecordId, "graphicsportal");
			editRequestResponse = await newEditRequest.Execute();

			fmServer.SetLayout(graphicLayout);
			uploadContainerResponse = await fmServer.UploadFileIntoContainerField(newGraphicRecordId + 1, "File", fileInfo);
			errorCheck = fmServer.lastErrorCode;

			var base64String = string.Empty;

			using (Image image = Image.FromFile(fileName))
			{
				using (MemoryStream m = new MemoryStream())
				{
					image.Save(m, image.RawFormat);
					byte[] imageBytes = m.ToArray();
					base64String = Convert.ToBase64String(imageBytes);
				}
			}

			var button = sender as Button;
			button.BackgroundImage = Convertbase64ToImage(base64String);
			button.BackgroundImageLayout = ImageLayout.Stretch;
			button.Tag = $"{newGraphicRecordId + 1}, {productRecordId}";
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
