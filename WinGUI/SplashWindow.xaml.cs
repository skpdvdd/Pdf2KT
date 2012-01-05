using System.Windows;
using System.Windows.Media;
using Pdf2KT;
using System.IO;

namespace WinGUI
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        IDocument _document;

        readonly PixelFormat[] _outputFormats;
        readonly Config.DocumentWriterType[] _writerTypes;

        public SplashWindow()
        {
            InitializeComponent();

            _outputFormats = new PixelFormat[] { PixelFormats.Gray2, PixelFormats.Gray4, PixelFormats.Gray8 };
            _writerTypes = new Config.DocumentWriterType[] { Config.DocumentWriterType.PDF, Config.DocumentWriterType.ImageSequence };
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);

            if (files.Length != 1)
                return;

            string filename = files[0];
            string extension = System.IO.Path.GetExtension(filename);

            if (!Directory.Exists(filename) && extension.ToLower() != ".pdf")
            {
                labelInfo.Content = "Only PDF files are supported.";
                return;
            }

            AllowDrop = false;

            labelInfo.Content = System.IO.Path.GetFileName(filename);
            gridInfo.Visibility = Visibility.Visible;

            if(Directory.Exists(filename))
                _document = new ImageSequenceDocument(filename);
            else
                _document = new PDFDocument(filename);

            txtDocumentTitle.Text = _document.Title;
            txtDocumentAuthor.Text = _document.Author;

            Config.InputPath = filename;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            buttonContinue.Click += new RoutedEventHandler(buttonContinue_Click);
        }

        void buttonContinue_Click(object sender, RoutedEventArgs e)
        {
            string documentTitle = txtDocumentTitle.Text;

            if (string.IsNullOrWhiteSpace(documentTitle))
            {
                labelInfo.Content = "Empty document title.";
                e.Handled = true;
                return;
            }

            string documentAuthor = txtDocumentAuthor.Text;

            if (string.IsNullOrWhiteSpace(documentAuthor))
            {
                labelInfo.Content = "Empty document author.";
                e.Handled = true;
                return;
            }

            int pageWidth;

            if (!int.TryParse(txtPageWidth.Text, out pageWidth) || pageWidth <= 0)
            {
                labelInfo.Content = "Invalid page width.";
                e.Handled = true;
                return;
            }

            int pageHeight;

            if (!int.TryParse(txtPageHeight.Text, out pageHeight) || pageWidth <= 0)
            {
                labelInfo.Content = "Invalid page heigh.";
                e.Handled = true;
                return;
            }

            buttonContinue.IsEnabled = false;

            _document.Title = documentTitle;
            _document.Author = documentAuthor;
            _document.RenderWidth = pageWidth;

            Config.Document = _document;
            Config.PageHeight = pageHeight;

            Config.WriterType = _writerTypes[lbWriter.SelectedIndex];
            PixelFormat outputFormat = _outputFormats[lbColors.SelectedIndex];
            Config.BitmapConverter = new BitmapSourceConverter(outputFormat, new BitmapSourceConverter.PngEncoder());

            string inputDirectory = System.IO.Path.GetDirectoryName(Config.InputPath);
            string inputFileName = System.IO.Path.GetFileNameWithoutExtension(Config.InputPath);

            if (Config.WriterType == Config.DocumentWriterType.ImageSequence)
                Config.OutputPath = System.IO.Path.Combine(inputDirectory, inputFileName + "_pdf2kt");
            else
                Config.OutputPath = System.IO.Path.Combine(inputDirectory, inputFileName + "_pdf2kt.pdf");
            
            MainWindow mainWindow = new MainWindow();
            mainWindow.Loaded += new RoutedEventHandler(mainWindow_Loaded);
            mainWindow.Show();
        }

        void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
