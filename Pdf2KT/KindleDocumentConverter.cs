using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pdf2KT
{
    /// <summary>
    /// Converts a document so that the pages match a given height.
    /// </summary>
    public class KindleDocumentConverter : IDocumentConverter
    {
        /// <summary>
        /// Returns the page number of the page of the input document
        /// that is being converted.
        /// </summary>
        public int CurrentProcessedPageNumber
        {
            get { return _fragmentSource.PageNumber; }
        }

        /// <summary>
        /// Returns the id of the page of the input document
        /// that is being converted.
        /// </summary>
        public int CurrentProcessedPageID
        {
            get { return _fragmentSource.PageID; }
        }

        /// <summary>
        /// Returns the total number of pages to convert from.
        /// </summary>
        public int PageCount
        {
            get { return _fragmentSource.NumPages; }
        }

        /// <summary>
        /// Returns the input document.
        /// </summary>
        public IDocument Document { get; private set; }

        readonly int _pageHeight;
        readonly PageFragmentSource _fragmentSource;

        int _currentFragmentY;
        WriteableBitmap _pageTemplate;
        WriteableBitmap _currentConvertedPage;
        BitmapSource _currentFragment;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="document">The input document.</param>
        /// <param name="pages">The numbers of the pages that should be converted.</param>
        /// <param name="pageHeight">The height of the output pages.</param>
        /// <exception cref="Pdf2KTException">If there are not pages to convert.</exception>
        public KindleDocumentConverter(IDocument document, IEnumerable<int> pages, int pageHeight)
        {
            if (document.PageCount == 0)
                throw new Pdf2KTException("Document contains no pages.");

            if (!pages.Any())
                throw new Pdf2KTException("No pages defined.");

            Document = document;

            _pageHeight = pageHeight;
            _fragmentSource = new PageFragmentSource(document, pages, pageHeight);
        }

        /// <summary>
        /// Returns the current output page.
        /// </summary>
        public BitmapSource Current
        {
            get { return _currentConvertedPage; }
        }

        /// <summary>
        /// Returns the current output page.
        /// </summary>
        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        /// Creates the next output page, if there are input pages left.
        /// </summary>
        /// <returns>True if the creation was successful, false otherwise.</returns>
        public bool MoveNext()
        {
            int pageY = 0;

            while (pageY < _pageHeight)
            {
                if (_currentFragment == null)
                {
                    if (_fragmentSource.MoveNext())
                    {
                        _currentFragment = _fragmentSource.Current;
                        _currentFragmentY = 0;
                    }
                    else
                        return false;
                }

                if (_pageTemplate == null)
                {
                    _pageTemplate = new WriteableBitmap(_currentFragment.PixelWidth, _pageHeight, _currentFragment.DpiX, _currentFragment.DpiY, PixelFormats.Gray8, null);

                    byte[] data = new byte[_pageTemplate.PixelWidth * _pageTemplate.PixelHeight];

                    for (int i = 0; i < data.Length; i++)
                        data[i] = byte.MaxValue;

                    _pageTemplate.WritePixels(new Int32Rect(0, 0, _pageTemplate.PixelWidth, _pageTemplate.PixelHeight), data, _pageTemplate.PixelWidth, 0);
                }

                if (pageY == 0)
                    _currentConvertedPage = _pageTemplate.Clone();

                if (pageY + _currentFragment.PixelHeight <= _pageHeight)
                {
                    _writeInto(_currentConvertedPage, _currentFragment, pageY);
                    pageY += _currentFragment.PixelHeight;
                    _currentFragment = null;
                }
                else
                {
                    int splitY = Math.Min(_pageHeight - pageY, _currentFragment.PixelHeight - _currentFragmentY);
                    BitmapSource croppedFragment = new CroppedBitmap(_currentFragment, new Int32Rect(0, _currentFragmentY, _currentFragment.PixelWidth, splitY));
                    int firstWhiteY = _getFirstWhiteLine(_currentFragment, croppedFragment.PixelHeight);

                    if (firstWhiteY != int.MaxValue && firstWhiteY != -splitY)
                    {
                        splitY += firstWhiteY;

                        croppedFragment = new CroppedBitmap(croppedFragment, new Int32Rect(0, 0, croppedFragment.PixelWidth, splitY));

                        _writeInto(_currentConvertedPage, croppedFragment, pageY);
                        _currentFragment = new CroppedBitmap(_currentFragment, new Int32Rect(0, splitY, _currentFragment.PixelWidth, _currentFragment.PixelHeight - splitY));
                        _currentFragmentY = splitY;
                    }

                    pageY = _pageHeight;
                }
            }

            return true;
        }

        /// <summary>
        /// Resets the converter.
        /// </summary>
        public void Reset()
        {
            _currentFragmentY = 0;
            _currentFragment = null;
            _fragmentSource.Reset();
        }

        /// <summary>
        /// Disposeable implementation.
        /// </summary>
        public void Dispose() { }

        static void _writeInto(WriteableBitmap target, BitmapSource source, int y)
        {
            byte[] data = new byte[target.PixelWidth * target.PixelHeight];

            source.CopyPixels(data, source.PixelWidth, 0);
            target.WritePixels(new Int32Rect(0, y, source.PixelWidth, source.PixelHeight), data, source.PixelWidth, 0);
        }

        static int _getFirstWhiteLine(BitmapSource bitmapSource, int yStart, bool up = true)
        {
            int offset = 0;
            int y = Math.Min(bitmapSource.PixelHeight - 1, yStart);

            while (true)
            {
                if (y < 0 || y >= bitmapSource.PixelHeight)
                    break;

                int i = 0;
                foreach (byte pixel in _getLine(bitmapSource, y))
                {
                    if (pixel != byte.MaxValue)
                        break;

                    i++;
                }

                if (i == bitmapSource.PixelWidth)
                    return offset;

                y += up ? -1 : y + 1;
                offset += up ? -1 : 1;
            }

            return int.MaxValue;
        }

        static byte[] _getLine(BitmapSource bitmap, int y)
        {
            byte[] pixels = new byte[bitmap.PixelWidth];
            bitmap.CopyPixels(new Int32Rect(0, y, bitmap.PixelWidth, 1), pixels, bitmap.PixelWidth, 0);

            return pixels;
        }

        /// <summary>
        /// Loads page fragments. A page fragment is a part of the original page.
        /// </summary>
        private class PageFragmentSource : IEnumerator<BitmapSource>
        {
            /// <summary>
            /// The id of the currently converted page.
            /// </summary>
            public int PageID { get; private set; }

            /// <summary>
            /// The number of the currently converted page.
            /// </summary>
            public int PageNumber
            {
                get { return _pages[PageID]; }
            }

            /// <summary>
            /// The total number of pages.
            /// </summary>
            public int NumPages { get; private set; }

            readonly IDocument _document;
            readonly int _maxHeight;
            readonly List<int> _pages;

            int _documentY;
            BitmapSource _fragment, _documentPage;

            /// <summary>
            /// Ctor.
            /// </summary>
            /// <param name="document">The document to read from.</param>
            /// <param name="pages">The numbers of the pages that should be converted.</param>
            /// <param name="maxHeight">The maximum fragment height</param>
            /// <exception cref="Pdf2KTException">If there are not pages to convert.</exception>
            public PageFragmentSource(IDocument document, IEnumerable<int> pages, int maxHeight)
            {
                if (document.PageCount == 0)
                    throw new Pdf2KTException("Document contains no pages.");

                if (!pages.Any())
                    throw new Pdf2KTException("No pages defined.");

                _document = document;
                _pages = new List<int>(pages);
                _maxHeight = maxHeight;

                PageID = -1;
                NumPages = _pages.Count;
            }

            /// <summary>
            /// Gets the current page fragment.
            /// </summary>
            public BitmapSource Current
            {
                get { return _fragment; }
            }

            /// <summary>
            /// Gets the current page fragment.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            /// <summary>
            /// Advances the fragment source, generating the next fragment.
            /// </summary>
            /// <returns>True if a new fragment was generated, false otherwise.</returns>
            public bool MoveNext()
            {
                if (_documentPage == null || _documentY >= _documentPage.PixelHeight)
                {
                    PageID++;

                    if (PageID >= _pages.Count)
                        return false;

                    _documentY = 0;
                    _documentPage = _document.RenderPage(_pages[PageID]);

                    int yEnd = _documentPage.PixelHeight;
                    int yEndOffset = _getFirstWhiteLine(_documentPage, _documentPage.PixelHeight);

                    if (yEndOffset == int.MaxValue || yEndOffset == -yEnd)
                        yEndOffset = 0;

                    yEnd += yEndOffset;

                    _documentPage = new CroppedBitmap(_documentPage, new Int32Rect(0, 0, _documentPage.PixelWidth, yEnd));
                }

                int documentYEnd = Math.Min(_documentPage.PixelHeight - _documentY, _maxHeight);
                int cropOffset = _getFirstWhiteLine(_documentPage, _documentY + documentYEnd);

                if (cropOffset == int.MaxValue || cropOffset == -documentYEnd)
                    cropOffset = 0;

                documentYEnd += cropOffset;

                _fragment = new CroppedBitmap(_documentPage, new Int32Rect(0, _documentY, _documentPage.PixelWidth, documentYEnd));
                _documentY += documentYEnd;

                return true;
            }

            /// <summary>
            /// Resets the fragment source.
            /// </summary>
            public void Reset()
            {
                PageID = 0;
                _documentY = 0;
                _fragment = null;
                _documentPage = null;
            }

            /// <summary>
            /// Disposeable implementation.
            /// </summary>
            public void Dispose() { }
        }
    }
}
