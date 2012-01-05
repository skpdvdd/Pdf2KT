using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Pdf2KT
{
    /// <summary>
    /// Converts pages of an input document.
    /// </summary>
    public interface IDocumentConverter : IEnumerator<BitmapSource>
    {
        /// <summary>
        /// The number of the page of the input document that is currently processed.
        /// </summary>
        int CurrentProcessedPageNumber { get; }

        /// <summary>
        /// The id of the page of the input document that is currently processed.
        /// </summary>
        int CurrentProcessedPageID { get; }

        /// <summary>
        /// The number of pages of the input document to convert.
        /// </summary>
        int PageCount { get; }

        /// <summary>
        /// The input document.
        /// </summary>
        IDocument Document { get; }
    }
}
