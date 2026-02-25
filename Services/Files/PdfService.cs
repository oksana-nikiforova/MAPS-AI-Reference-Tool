using CommunityToolkit.Maui.Storage;
using MAPSAI.Services.Files.Models;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using System.Diagnostics;

namespace MAPSAI.Services.Files
{

    public class PdfService
    {
        public PdfService() 
        {
            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
        }

        public async Task<DocumentResponse> GenerateDocument(Dictionary<string, string> Data, string Title, string filename)
        {
            try
            {
                Document document = new Document();

                var ProjectInfoSection = CreateProjectInfoSection(Data, Title);

                if (!ProjectInfoSection.Success || ProjectInfoSection.Section is null)
                {
                    return new()
                    {
                        Success = false,
                        Error = "No project info provided!",
                    };
                }

                document.Add(ProjectInfoSection.Section);

                var renderer = new PdfDocumentRenderer()
                {
                    Document = document
                };

                renderer.RenderDocument();

                using var stream = new MemoryStream();
                renderer.PdfDocument.Save(stream, false);
                stream.Position = 0;

                if (Application.Current?.Windows.LastOrDefault()?.Handler.PlatformView is Microsoft.UI.Xaml.Window window)
                {
                    window.Activate();
                }

                var fileSaverResult = await FileSaver.Default.SaveAsync(
                    $"{filename}.pdf",
                    stream,
                    CancellationToken.None);

                if (fileSaverResult.IsSuccessful)
                {
                    return new()
                    {
                        Success = true,
                    };
                }
                else
                {
                    return new()
                    {
                        Success = false,
                        Error = fileSaverResult.Exception?.Message ?? "Save canceled"
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new()
                {
                    Success = false,
                };
            }
        }

        private DocumentResponse CreateProjectInfoSection(Dictionary<string, string> data, string title)
        {
            try
            {
                Section section = new();

                var titleText = section.AddParagraph(title);
                titleText.Format.Font.Size = 18;
                titleText.Format.SpaceAfter = "10pt";

                var myFont = new XFont("Arial", 10, XFontStyleEx.Regular);
                var myBoldFont = new XFont("Arial", 10, XFontStyleEx.Bold);

                foreach (var pair in data)
                {
                    var heading = section.AddParagraph(pair.Key);
                    heading.Format.Font.Bold = true;
                    heading.Format.Font.Name = "Arial";
                    heading.Format.Font.Size = 11;

                    var par = section.AddParagraph(pair.Value);
                    par.Format.Font.Name = "Arial";
                    par.Format.Font.Size = 8;
                    par.Format.SpaceAfter = "10pt";
                }

                return new()
                {
                    Success = true,
                    Section = section,
                };
            }
            catch (Exception ex) 
            {
                Debug.WriteLine(ex);
                return new()
                {
                    Success = false
                };
            }
        }
    }
}
