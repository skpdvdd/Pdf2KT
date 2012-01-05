using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Pdf2KT
{
    public class PDFWriter : DocumentWriter
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="filePath">The destination path.</param>
        /// <param name="converter">The converter to use.</param>
        /// <param name="bitmapProcessor">The bitmap processor to use to process the pages.</param>
        public PDFWriter(string filePath, IDocumentConverter converter, BitmapSourceConverter bitmapProcessor) : base(filePath, converter, bitmapProcessor) { }

        protected override void WriteDocument(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (File.Exists(FilePath))
                throw new Pdf2KTException("File already exists.");

            Document document = null;
            PdfWriter writer = null;

            while(Converter.MoveNext())
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }

                BitmapEncoder encoder = BitmapProcessor.GetBitmapEncoder();
                BitmapSource processed = BitmapProcessor.Convert(Converter.Current);

                if (document == null)
                {
                    document = new Document(new Rectangle(processed.PixelWidth, processed.PixelHeight));
                    writer = PdfWriter.GetInstance(document, new FileStream(FilePath, FileMode.Create));

                    document.Open();
                    document.AddTitle(Converter.Document.Title);
                    document.AddAuthor(Converter.Document.Author);
                    document.AddCreationDate();
                    document.AddCreator("Pdf2KT");
                }

                document.NewPage();

                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Frames.Add(BitmapFrame.Create(processed));
                    encoder.Save(ms);

                    ms.Position = 0;

                    Image pdfpage = Image.GetInstance(ms);
                    pdfpage.SetAbsolutePosition(0, 0);

                    document.Add(pdfpage);
                }

                worker.ReportProgress((int)((Converter.CurrentProcessedPageNumber * 1f) / Converter.PageCount * 100));
            }

            document.Close();
        }
    }
}
