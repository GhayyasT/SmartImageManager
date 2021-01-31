using FMdotNet__DataAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
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

		public Form1()
		{
			InitializeComponent();
			fmServer = new FMS18(server, userName, password);
			relativePath = new Uri(Directory.GetCurrentDirectory()).LocalPath.Replace("bin", "").Replace("Debug", "").Replace(@"\\", @"\");
		}

		private async void Form1_Load(object sender, EventArgs e)
		{
			logoPictureBox.Image = Image.FromFile($@"{relativePath}Images\logo.jpg");
			var lines = File.ReadLines(filePath).ToList();
			await AuthenticateFileMakerServer();
			await PopulateData(173956, 91086);
		}

		private async Task PopulateData(int itemRecordId, int porductRecordId)
		{
			fmServer.SetLayout(productLayout);
			var getProductRequest = fmServer.FindRequest(91806);
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

			for (int i = 0; i < 9; i++)
			{
				var pictureBox = GetCurrentPictureBox(i);

				if (i > 10 && i < graphicsPortalData.records.Count())
				{
					var currentRow = graphicsPortalData.records.ElementAt(i);
					var file = currentRow.fieldsAndData["File"];

				}
				else
				{
					pictureBox.Image = ResizedSelectImage();
				}
			}

			//MessageBox.Show($"Error while retrieving data, {fmServer.lastErrorMessage}");
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

		private Button GetCurrentPictureBox(int index)
		{
			if (index == 0)
				return btnImage1;
			else if (index == 1)
				return btnImage1;
			else if (index == 2)
				return btnImage1;
			else if (index == 3)
				return btnImage1;
			else if (index == 4)
				return btnImage1;
			else if (index == 5)
				return btnImage1;
			else if (index == 6)
				return btnImage1;
			else if (index == 7)
				return btnImage1;
			else
				return btnImage1;
		}
	}
}
