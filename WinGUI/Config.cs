using Pdf2KT;

namespace WinGUI
{
    static class Config
    {
        public enum DocumentWriterType { ImageSequence, PDF };

        public static int PageHeight { get; set; }
        public static string InputPath { get; set; }
        public static string OutputPath { get; set; }
        public static IDocument Document { get; set; }
        public static BitmapSourceConverter BitmapConverter { get; set; }
        public static DocumentWriterType WriterType { get; set; }
    }
}
