using System;
using System.Windows.Media.Imaging;
using MuPDFLib;

namespace Pdf2KT
{
    /// <summary>
    /// A PDF document.
    /// </summary>
    public class PDFDocument : IDocument
    {
        bool _disposed;

        readonly string _path;
        readonly MuPDF _mupdf;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="path">The path to the document.</param>
        /// <param name="renderWidth">The width of the rendered pages. Must be > 0.</param>
        /// <param name="renderDPI">The dpi to use when rendering the pages. Must be > 0.</param>
        /// <param name="password">The password (null if none).</param>
        /// <exception cref="Pdf2KTException">If the document cannot be opened.</exception>
        public PDFDocument(string path, int renderWidth = 800, int renderDPI = 96, string password = null)
        {
            RenderWidth = renderWidth;
            RenderDPI = renderDPI;

            _path = path;

            try
            {
                _mupdf = new MuPDF(path, password);
                _mupdf.AntiAlias = true;
            }
            catch (Exception e)
            {
                throw new Pdf2KTException("Error while opening PDF document.", e);
            }
        }

        /// <summary>
        /// Gets the path to the document.
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// Gets the number of pages.
        /// </summary>
        public int PageCount
        {
            get { return _mupdf.PageCount; }
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
        /// Gets or sets the DPI used when rendering pages.
        /// </summary>
        public int RenderDPI { get; set; }

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
            if (page <= 0 || page > _mupdf.PageCount)
                throw new Pdf2KTException("Page does not exist.");

            _mupdf.Page = page;

            try
            {
                //TODO mupdf seems to render 1 pixel too large in width
                return _mupdf.GetBitmapSource(RenderWidth - 1, 0, RenderDPI, RenderDPI, 0, RenderType.Grayscale, false, false, 10000000);
            }
            catch (Exception e)
            {
                throw new Pdf2KTException("Error while rendering the page.", e);
            }
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
            if (_disposed)
                return;

            if (dispose)
                _mupdf.Dispose();

            _disposed = true;
        }

        /// <summary>
        /// Dtor.
        /// </summary>
        ~PDFDocument()
        {
            Dispose(false);
        }
    }
}
