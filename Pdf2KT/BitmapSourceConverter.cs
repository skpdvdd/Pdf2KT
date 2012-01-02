using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pdf2KT
{
    /// <summary>
    /// Converts and encodes bitmaps.
    /// </summary>
    public class BitmapSourceConverter
    {
        readonly PixelFormat _format;
        readonly Encoder _encoder;
        readonly int _rotate;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="format">The pixel format to use for conversion.</param>
        /// <param name="encoder">The encoder to use.</param>
        /// <param name="rotate">Degrees of rotation to apply (counterclock whise).</param>
        public BitmapSourceConverter(PixelFormat format, Encoder encoder, int rotate = 0)
        {
            _format = format;
            _encoder = encoder;
            _rotate = rotate;
        }

        /// <summary>
        /// Returns a new instance of the bitmap encoder to use.
        /// </summary>
        /// <returns></returns>
        public BitmapEncoder GetBitmapEncoder()
        {
            return _encoder.Get();
        }

        /// <summary>
        /// Converts a bitmap and returns the result.
        /// </summary>
        /// <param name="bitmap">The bitmap to convert.</param>
        /// <returns>The result.</returns>
        public BitmapSource Convert(BitmapSource bitmap)
        {
            BitmapSource result = bitmap;

            if(bitmap.Format != _format)
                result = new FormatConvertedBitmap(result, _format, null, 0);

            if(_rotate != 0)
                result = new TransformedBitmap(result, new RotateTransform(_rotate));

            return result;
        }

        /// <summary>
        /// An encoder.
        /// </summary>
        public abstract class Encoder
        {
            /// <summary>
            /// Returns a new instance of the bitmap encoder.
            /// </summary>
            /// <returns></returns>
            public abstract BitmapEncoder Get();
        }

        /// <summary>
        /// PNG encoder.
        /// </summary>
        public class PngEncoder : Encoder
        {
            readonly PngInterlaceOption _interlace;

            /// <summary>
            /// Ctor.
            /// </summary>
            /// <param name="interlace">Whether to enable interlace mode.</param>
            public PngEncoder(bool interlace = false)
            {
                _interlace = interlace ? PngInterlaceOption.On : PngInterlaceOption.Off;
            }

            /// <summary>
            /// Returns a new instance of the bitmap encoder.
            /// </summary>
            /// <returns></returns>
            public override BitmapEncoder Get()
            {
                return new PngBitmapEncoder() { Interlace = _interlace };
            }
        }

        /// <summary>
        /// JPEG encoder.
        /// </summary>
        public class JpegEncoder : Encoder
        {
            int _qualityLevel;

            /// <summary>
            /// Ctor.
            /// </summary>
            /// <param name="qualityLevel">The quality level.</param>
            public JpegEncoder(int qualityLevel = 60)
            {
                _qualityLevel = qualityLevel;
            }

            /// <summary>
            /// Returns a new instance of the bitmap encoder.
            /// </summary>
            /// <returns></returns>
            public override BitmapEncoder Get()
            {
                return new JpegBitmapEncoder() { QualityLevel = _qualityLevel };
            }
        }
    }
}
