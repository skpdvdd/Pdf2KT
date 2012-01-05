using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Collections;

namespace Pdf2KT
{
    /// <summary>
    /// Does no actual conversion, returning the unchanged pages
    /// of the input document.
    /// </summary>
    public class DummyDocumentConverter : IDocumentConverter
    {
        readonly IDocument _document;
        readonly List<int> _pages;

        BitmapSource _currentPage;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="document">The input document.</param>
        /// <param name="pages">The numbers of the pages that should be converted.</param>
        /// <exception cref="Pdf2KTException">If there are not pages to convert.</exception>
        public DummyDocumentConverter(IDocument document, IEnumerable<int> pages)
        {
            if (document.PageCount == 0)
                throw new Pdf2KTException("Document contains no pages.");

            if (!pages.Any())
                throw new Pdf2KTException("No pages defined.");

            _document = document;
            _pages = new List<int>(pages);

            CurrentProcessedPageID = -1;
        }

        /// <summary>
        /// The number of the page of the input document that is currently processed.
        /// </summary>
        public int CurrentProcessedPageNumber
        {
            get { return _pages[CurrentProcessedPageID]; }
        }

        /// <summary>
        /// The id of the page of the input document that is currently processed.
        /// </summary>
        public int CurrentProcessedPageID { get; private set; }

        /// <summary>
        /// The number of pages of the input document to convert.
        /// </summary>
        public int PageCount
        {
            get { return _pages.Count; }
        }

        /// <summary>
        /// The input document.
        /// </summary>
        public IDocument Document
        {
            get { return _document; }
        }

        /// <summary>
        /// Returns the current output page.
        /// </summary>
        public BitmapSource Current
        {
            get { return _currentPage; }
        }

        /// <summary>
        /// Returns the current output page.
        /// </summary>
        object IEnumerator.Current
        {
            get { return _currentPage; }
        }

        /// <summary>
        /// Creates the next output page, if there are input pages left.
        /// </summary>
        /// <returns>True if the creation was successful, false otherwise.</returns>
        public bool MoveNext()
        {
            CurrentProcessedPageID++;

            if (_pages.Count <= CurrentProcessedPageID)
                return false;

            _currentPage = _document.RenderPage(CurrentProcessedPageNumber);

            return true;
        }

        /// <summary>
        /// Resets the converter.
        /// </summary>
        public void Reset()
        {
            CurrentProcessedPageID = -1;
        }

        /// <summary>
        /// Disposeable implementation.
        /// </summary>
        public void Dispose() { }
    }
}
