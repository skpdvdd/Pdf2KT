using System.ComponentModel;

namespace Pdf2KT
{
    /// <summary>
    /// Writes a sequence of pages in form of a number of BitmapSources
    /// to a document in an asynchronous way.
    /// </summary>
    public abstract class DocumentWriter
    {
        /// <summary>
        /// Called on progress changes.
        /// </summary>
        /// <param name="progress">The new progress in percent.</param>
        public delegate void ProgressChangedEventHandler(int progress);

        /// <summary>
        /// Called on completion.
        /// </summary>
        /// <param name="filePath">The path to fhe generated file.</param>
        /// <param name="canceled">Whether the save process was canceled.</param>
        public delegate void CompletedEventHandler(string filePath, bool canceled);

        /// <summary>
        /// Raised on progress changes.
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged;

        /// <summary>
        /// Raised on completion.
        /// </summary>
        public event CompletedEventHandler Completed;

        /// <summary>
        /// The bitmap processor.
        /// </summary>
        protected readonly BitmapSourceConverter BitmapProcessor;

        /// <summary>
        /// The document converter.
        /// </summary>
        protected readonly DocumentConverter Converter;

        /// <summary>
        /// The destination path.
        /// </summary>
        protected readonly string FilePath;

        readonly BackgroundWorker _worker;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="filePath">The destination path.</param>
        /// <param name="converter">The converter to use.</param>
        /// <param name="bitmapProcessor">The bitmap processor to use to process the pages.</param>
        public DocumentWriter(string filePath, DocumentConverter converter, BitmapSourceConverter bitmapProcessor)
        {
            FilePath = filePath;
            Converter = converter;
            BitmapProcessor = bitmapProcessor;

            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            _worker.DoWork += new DoWorkEventHandler(WriteDocument);
            _worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(Worker_ProgressChanged);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
        }

        /// <summary>
        /// Starts writing the document.
        /// </summary>
        /// <exception cref="Pdf2KTException">If called while the writer is already writing.</exception>
        public void WriteDocument()
        {
            if (_worker.IsBusy)
                throw new Pdf2KTException("Write already in progress.");

            _worker.RunWorkerAsync();
        }

        /// <summary>
        /// Cancels the write process.
        /// </summary>
        public void Cancel()
        {
            if (!_worker.CancellationPending)
                _worker.CancelAsync();
        }

        /// <summary>
        /// Write the document.
        /// </summary>
        /// <param name="sender">The background worker.</param>
        /// <param name="e">Event args.</param>
        protected abstract void WriteDocument(object sender, DoWorkEventArgs e);

        void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Completed != null)
                Completed(FilePath, e.Cancelled);
        }

        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(e.ProgressPercentage);
        }
    }
}
