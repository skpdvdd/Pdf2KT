using System;
using System.Windows.Media.Imaging;

namespace Pdf2KT
{
    /// <summary>
    /// A document consisting of a number of pages.
    /// </summary>
    public interface IDocument : IDisposable
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        string Author { get; set; }

        /// <summary>
        /// Gets the number of pages.
        /// </summary>
        int PageCount { get; }

        /// <summary>
        /// Gets or sets the desired width of rendered pages.
        /// </summary>
        int RenderWidth { get; set; }

        /// <summary>
        /// Renders a page to a BitmapSource and returns the result.
        /// The result has a width equal to RenderWidth. The height is
        /// calculated automatically. The rendered page is 8-bit grayscale.
        /// </summary>
        /// <param name="page">The number of the page to render.</param>
        /// <returns>The rendered page</returns>
        /// <exception cref="Pdf2KTException">On errors.</exception>
        BitmapSource RenderPage(int page);
    }
}
