using Aspose.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDFRendererHelpers
{
    public static class Helpers
    {
        public static MemoryStream LoadHTMLStreamFromUrl(string url)
        {
            // Create a request for the URL.
            WebRequest request = HttpWebRequest.Create(url);
            
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();

            MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseFromServer));

            return stream;
        }

        public static MemoryStream CreatePDFFromStream(MemoryStream stream, string optionsUrl)
        {
            // Load Options
            HtmlLoadOptions options = new HtmlLoadOptions();
            if (!optionsUrl.Equals(string.Empty))
            {
                options = new HtmlLoadOptions(optionsUrl);
            }

            // Load HTML file
            Document pdfDocument = new Document(stream, options);
            // Transform it to MemoryStream
            MemoryStream outputStream = new MemoryStream();
            return outputStream;

        }

        public static bool SavePDFFile(MemoryStream pdfStream, string localPath)
        {
            try
            {
                // Save file in local disk
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }
                using (FileStream file = File.Create(localPath))
                {
                    pdfStream.CopyTo(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }
    }
}
