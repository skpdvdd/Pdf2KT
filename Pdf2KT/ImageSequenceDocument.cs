using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pdf2KT
{
    /// <summary>
    /// A document based on a sequence of PNG images.
    /// </summary>
    public class ImageSequenceDocument : IDocument
    {
        bool _disposed;

        readonly string _path;
        readonly List<string> _files;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="path">The path to the directory that contains the image sequence</param>
        /// <exception cref="Pdf2KTException">If the path is not a directory or if there are no images</exception>
        public ImageSequenceDocument(string path)
        {
            _path = path;
            _files = new List<string>();

            if (!Directory.Exists(path))
                throw new Pdf2KTException("path does not point to a directory.");

            foreach (string file in Directory.EnumerateFiles(path, "*.png"))
                _files.Add(file);

            _files.Sort();

            if (_files.Count == 0)
                throw new Pdf2KTException("path contains no PNG files.");
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the desired width of rendered pages.
        /// </summary>
        public int RenderWidth { get; set; }

        /// <summary>
        /// Gets the number of pages.
        /// </summary>
        public int PageCount
        {
            get { return _files.Count; }
        }

        /// <summary>
        /// Renders a page to a BitmapSource and returns the result.
        /// The result has a width equal to RenderWidth. The height is
        /// calculated automatically. The rendered page is 8-bit grayscale.
        /// </summary>
        /// <param name="page">The number of the page to render.</param>
        /// <returns>The rendered page</returns>
        /// <exception cref="Pdf2KTException">On errors.</exception>
        public BitmapSource RenderPage(int page)
        {
            page = page - 1;

            if (page < 0 || page >= _files.Count)
                throw new Pdf2KTException("Page not found.");

            BitmapSource result = new BitmapImage(new Uri(_files[page]));

            if (result.Format != PixelFormats.Gray8)
                result = new FormatConvertedBitmap(result, PixelFormats.Gray8, null, 0);

            if (result.PixelWidth != RenderWidth)
            {
                double scale = (RenderWidth * 1.0) / result.PixelWidth;
                result = new TransformedBitmap(result, new ScaleTransform(scale, scale));
            }

            return result;
        }

        /// <summary>
        /// Frees resources associated with this document.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposeable pattern.
        /// </summary>
        /// <param name="dispose">Whether to release managed resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            // nothing to do ...

            if (_disposed)
                return;

            if (dispose)
                _files.Clear();

            _disposed = true;
        }

        /// <summary>
        /// Dtor.
        /// </summary>
        ~ImageSequenceDocument()
        {
            Dispose(false);
        }
    }
}
