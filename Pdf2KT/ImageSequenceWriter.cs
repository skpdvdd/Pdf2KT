using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace Pdf2KT
{
    /// <summary>
    /// Writes an image sequence to disk.
    /// </summary>
    public class ImageSequenceWriter : DocumentWriter
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="filePath">The destination path.</param>
        /// <param name="converter">The converter to use.</param>
        /// <param name="bitmapProcessor">The bitmap processor to use to process the pages.</param>
        public ImageSequenceWriter(string filePath, IDocumentConverter converter, BitmapSourceConverter bitmapProcessor) : base(filePath, converter, bitmapProcessor) { }

        /// <summary>
        /// Write the document.
        /// </summary>
        /// <param name="sender">The background worker.</param>
        /// <param name="e">Event args.</param>
        /// <exception cref="Pdf2KTException">If the directory already exists.</exception>
        protected override void WriteDocument(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (Directory.Exists(FilePath))
                throw new Pdf2KTException("Directory already exists.");

            Directory.CreateDirectory(FilePath);

            int pageNum = 1;

            while (Converter.MoveNext())
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }

                BitmapEncoder encoder = BitmapProcessor.GetBitmapEncoder();
                string extension = encoder.CodecInfo.FileExtensions;

                using (FileStream fs = new FileStream(Path.Combine(FilePath, string.Format("page_{0:0000}{1}", pageNum, extension)), FileMode.Create))
                {
                    BitmapSource processed = BitmapProcessor.Convert(Converter.Current);

                    encoder.Frames.Add(BitmapFrame.Create(processed));
                    encoder.Save(fs);

                    pageNum++;
                }

                worker.ReportProgress((int)((Converter.CurrentProcessedPageID * 1f) / Converter.PageCount * 100));
            }
        }
    }
}
