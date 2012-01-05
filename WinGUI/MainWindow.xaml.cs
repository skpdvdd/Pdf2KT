using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Pdf2KT;

namespace WinGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int _currentPage;
        readonly LinkedList<int> _excludedPages;

        public MainWindow()
        {
            InitializeComponent();

            _currentPage = 1;
            _excludedPages = new LinkedList<int>();

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {               
               _displayPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Config.Document.Dispose();
                Application.Current.Shutdown(1);
            }
        }

        void _nextPage()
        {
            _currentPage = Math.Min(Config.Document.PageCount, _currentPage + 1);
            _displayPage();
        }

        void _previousPage()
        {
            _currentPage = Math.Max(1, _currentPage - 1);
            _displayPage();
        }

        void _firstPage()
        {
            _currentPage = 1;
            _displayPage();
        }

        void _lastPage()
        {
            _currentPage = Config.Document.PageCount;
            _displayPage();
        }

        void _displayPage()
        {
            lblInfo.Content = string.Format("{0} ({1}/{2}) [PageUp/Down to navigate]", Config.Document.Title, _currentPage, Config.Document.PageCount);
            
            BitmapSource page = Config.Document.RenderPage(_currentPage);
            imgPage.Source = page;
        }

        bool _converting, _savingPages;
        DocumentWriter _writer;

        private void btnSavePages_Click(object sender, RoutedEventArgs e)
        {
            btnConvert.IsEnabled = false;

            if (_savingPages)
            {
                btnSavePages.Content = "Canceling";
                _writer.Cancel();
                return;
            }

            _savingPages = true;
            btnSavePages.Content = "Cancel";

            Config.BitmapConverter.Rotate = 0;
            DummyDocumentConverter converter = new DummyDocumentConverter(Config.Document, Enumerable.Range(1, Config.Document.PageCount));
            
            _convert(converter);
        }

        void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            btnSavePages.IsEnabled = false;

            if (_converting)
            {
                btnConvert.Content = "Canceling";
                _writer.Cancel();
                return;
            }

            _converting = true;
            btnConvert.Content = "Cancel";

            Config.BitmapConverter.Rotate = 270;
            KindleDocumentConverter converter = new KindleDocumentConverter(Config.Document, Enumerable.Range(1, Config.Document.PageCount), Config.PageHeight);
            
            _convert(converter);
        }

        void _convert(IDocumentConverter converter)
        {
            if (Config.WriterType == Config.DocumentWriterType.PDF)
                _writer = new PDFWriter(Config.OutputPath, converter, Config.BitmapConverter);
            else
                _writer = new ImageSequenceWriter(Config.OutputPath, converter, Config.BitmapConverter);

            _writer.ProgressChanged += new DocumentWriter.ProgressChangedEventHandler(writer_ProgressChanged);
            _writer.Completed += new DocumentWriter.CompletedEventHandler(writer_Completed);

            _writer.WriteDocument();
        }

        void writer_Completed(string filePath, bool canceled)
        {
            btnConvert.IsEnabled = false;
            btnSavePages.IsEnabled = false;

            if(_converting)
                btnConvert.Content = canceled ? "canceled" : "completed";

            if (_savingPages)
                btnSavePages.Content = canceled ? "canceled" : "completed";
        }

        void writer_ProgressChanged(int progress)
        {
            progressBar.Value = progress;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageDown)
                _nextPage();

            if (e.Key == Key.PageUp)
                _previousPage();

            if (e.Key == Key.Home)
                _firstPage();

            if (e.Key == Key.End)
                _lastPage();
        }
    }
}
