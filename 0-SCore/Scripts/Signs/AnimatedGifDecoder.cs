//-----------------------------------------------------------------------------
// Copyright 2017 Old Moat Games. All rights reserved.
//
// Decoder adapted from NGif by gOODiDEA.NET.
//
//-----------------------------------------------------------------------------

using System;
using System.IO;
using UnityEngine;

namespace OldMoatGames
{
    public class GifDecoder
    {
        public enum Status
        {
            // File read status
            StatusOk, // No errors
            StatusFormatError, // Error decoding file (may be partially decoded)
            StatusOpenError // Unable to open source
        }

        public GifDecoder(bool compatibilityMode)
        {
            _compatibilityMode = compatibilityMode;
        }

        private void RewindReader()
        {
            _frameCount = 0;
            AllFramesRead = false;
            _inStream.Position = _imageDataOffset; //go to first image location in stream
        }


        private void SetPixels()
        {
            // fill in starting image contents based on last image's dispose code
            if (_lastDispose > 0)
            {
                //lastDispose = 1; Do not dispose. The graphic is to be left in place.
                //lastDispose = 2; Dispose to background. The area used by the graphic must be restored to the background color.
                //lastDispose = 3; Restore to previous. The decoder is required to restore the area overwritten by the graphic with what was there prior to rendering the graphic.
                var n = _frameCount - 1;
                if (n > 0)
                    if (_lastDispose == 2)
                    {
                        // Restore to background color. The area used by the graphic must be restored to the background color.
                        var fillcolor = _transparency ? 0 : _lastBgColor;
                        for (var i = 0; i < _lih; i++)
                        {
                            var line = i;
                            line += _liy;
                            if (line >= _height) continue;
                            var linein = _height - line - 1;
                            var dx = linein * _width + _lix;
                            var endx = dx + _liw;
                            while (dx < endx) _image[dx++] = fillcolor;
                        }
                    } /*else if (_compatibilityMode && _lastDispose == 3) {
                        for (var i = 0; i < _lih; i++) {
                            var line = i;
                            line += _liy;
                            if (line >= _height) continue;
                            var linein = _height - line - 1;
                            var dx = linein * _width + _lix;
                            var endx = dx + _liw;
                            while (dx < endx) {
                                _image[dx] = _prevImage[dx];
                                dx++;
                            }
                        }
                    }*/
            }

            // copy each source line to the appropriate place in the destination
            var pass = 1;
            var inc = 8;
            var iline = 0;
            for (var i = 0; i < _ih; i++)
            {
                var line = i;
                if (_interlace)
                {
                    if (iline >= _ih)
                    {
                        pass++;
                        switch (pass)
                        {
                            case 2:
                                iline = 4;
                                break;
                            case 3:
                                iline = 2;
                                inc = 4;
                                break;
                            case 4:
                                iline = 1;
                                inc = 2;
                                break;
                        }
                    }

                    line = iline;
                    iline += inc;
                }

                line += _iy;
                if (line >= _height) continue;

                var sx = i * _iw; // start of line in source
                var linein = _height - line - 1;
                var dx = linein * _width + _ix;
                var endx = dx + _iw;

                for (; dx < endx; dx++)
                {
                    var c = _act[_pixelIndexes[sx++] & 0xff];
                    if (c != 0) _image[dx] = c;
                }
            }
        }

        private void DecodeImageData()
        {
            var pixelCount = _iw * _ih;

            int datum, bi = 0;

            if (_pixelIndexes == null || _pixelIndexes.Length < pixelCount) _pixelIndexes = new byte[pixelCount]; // allocate new pixel array
            if (_prefix == null) _prefix = new short[MaxStackSize];
            if (_suffix == null) _suffix = new byte[MaxStackSize];
            if (_pixelStack == null) _pixelStack = new byte[MaxStackSize + 1];

            //  Initialize GIF data stream decoder.
            var litWidth = Read();

            if (litWidth < 2 || 8 < litWidth)
            {
                Debug.Log("litWidth out of range");
                return;
            }


            const int maxWidth = 12;
            const int decoderInvalidCode = 0xffff;

            var width = 1 + litWidth;
            var clear = 1 << litWidth;
            var eof = clear + 1;
            var hi = clear + 1;
            var overflow = 1 << width;
            var last = (ushort)0xffff;

            var o = 0;

            var suffix = new byte[1 << maxWidth];
            var prefix = new ushort[1 << maxWidth];
            var meaningfulBitsInDatum = 0;
            var bytesToExtract = 0;

            var maxCode = (1 << width) - 1;

            datum = 0;

            while (true)
            {
                if (meaningfulBitsInDatum < width)
                {
                    if (bytesToExtract == 0)
                    {
                        bytesToExtract = ReadBlock();
                        if (bytesToExtract <= 0) break;
                        bi = 0;
                    }

                    var newDatum = _block[bi] << meaningfulBitsInDatum;
                    datum += newDatum;

                    meaningfulBitsInDatum += 8;

                    bi++;

                    bytesToExtract--;

                    continue;
                }

                var code = (ushort)(datum & maxCode);
                datum >>= width;

                meaningfulBitsInDatum -= width;

                if (code < clear)
                {
                    _pixelIndexes[o] = (byte)code;
                    o++;
                    if (last != decoderInvalidCode)
                    {
                        suffix[hi] = (byte)code;
                        prefix[hi] = last;
                    }
                }
                else if (code == clear)
                {
                    width = 1 + litWidth;
                    maxCode = (1 << width) - 1;
                    hi = eof;
                    overflow = 1 << width;
                    last = decoderInvalidCode;
                    continue;
                }
                else if (code == eof)
                {
                    break;
                }
                else if (code <= hi)
                {
                    var c = code;
                    var i = _pixelIndexes.Length - 1;

                    if (code == hi && last != decoderInvalidCode)
                    {
                        c = last;
                        while (c >= clear) c = prefix[c];

                        _pixelIndexes[i] = (byte)c;

                        i--;
                        c = last;
                    }

                    while (c >= clear)
                    {
                        _pixelIndexes[i] = suffix[c];
                        i--;
                        c = prefix[c];
                    }

                    _pixelIndexes[i] = (byte)c;

                    var m = Math.Min(_pixelIndexes.Length - o, _pixelIndexes.Length - i);
                    Buffer.BlockCopy(_pixelIndexes, i, _pixelIndexes, o, m);
                    o += m;

                    if (last != decoderInvalidCode)
                    {
                        suffix[hi] = (byte)c;
                        prefix[hi] = last;
                    }
                }
                else
                {
                    Debug.Log("Invalid code");
                    break;
                }

                last = code;
                hi = hi + 1;
                if (hi != overflow) continue;
                if (width == maxWidth)
                {
                    last = decoderInvalidCode;

                    hi--;
                }
                else
                {
                    width++;
                    maxCode = (1 << width) - 1;

                    overflow <<= 1;
                }
            }
        }

        /**
		 * Initializes or re-initializes reader
		 */
        private void Init()
        {
            _status = Status.StatusOk;
            _frameCount = 0;
            _currentFrame = null;
            AllFramesRead = false;
            _gct = null;
            _lct = null;
        }

        /**
		 * Reads a single byte from the input stream.
		 */
        private int Read()
        {
            var curByte = 0;
            try
            {
                curByte = _inStream.ReadByte();
            }
            catch (IOException)
            {
                _status = Status.StatusFormatError;
            }

            return curByte;
        }


        /**
         * Reads next variable length block from input.
         * 
         * @return number of bytes stored in "buffer"
         */
        private int ReadBlock()
        {
            _blockSize = Read();
            var n = 0;
            if (_blockSize <= 0) return n;
            try
            {
                while (n < _blockSize)
                {
                    var count = _inStream.Read(_block, n, _blockSize - n);
                    if (count == -1)
                        break;
                    n += count;
                }
            }
            catch (IOException ex)
            {
                Debug.Log("exception " + ex);
            }

            if (n < _blockSize) _status = Status.StatusFormatError;
            return n;
        }

        /**
         * Reads color table as 256 RGB integer values
         * 
         * @param ncolors int number of colors to read
         * @return int array containing 256 colors (packed ARGB with full alpha)
         */
        private int[] ReadColorTable(int ncolors)
        {
            var nbytes = 3 * ncolors;
            int[] tab = null;
            var c = new byte[nbytes];
            var n = 0;
            try
            {
                n = _inStream.Read(c, 0, c.Length);
            }
            catch (IOException)
            {
            }

            if (n < nbytes)
            {
                _status = Status.StatusFormatError;
            }
            else
            {
                tab = new int[256]; // max size to avoid bounds checks
                var i = 0;
                var j = 0;
                while (i < ncolors)
                {
                    uint r = c[j++];
                    var g = c[j++] & (uint)0xff;
                    var b = c[j++] & (uint)0xff;
                    tab[i++] = (int)(0xff000000 | (b << 16) | (g << 8) | r);
                }
            }

            return tab;
        }

        /**
		 * Reads Graphics Control Extension values
		 */
        private void ReadGraphicControlExt()
        {
            Read(); // block size
            var packed = Read(); // packed fields
            _dispose = (packed & 0x1c) >> 2; // disposal method
            if (_dispose == 0) _dispose = 1; // elect to keep old image if discretionary
            _transparency = (packed & 1) != 0;
            _delay = ReadShort() / 100f; // delay in seconds
            _transIndex = Read(); // transparent color index
            Read(); // block terminator
        }

        /**
		 * Reads GIF file header information.
		 */
        private void ReadHeader()
        {
            var id = "";
            for (var i = 0; i < 6; i++) id += (char)Read();
            if (!id.StartsWith("GIF"))
            {
                _status = Status.StatusFormatError;
                return;
            }

            ReadLsd();
            if (_gctFlag && !Error())
            {
                _gct = ReadColorTable(_gctSize);
                _bgColor = _gct[_bgIndex];
            }

            _imageDataOffset = _inStream.Position;
        }

        /**
		 * Reads next frame image
		 */
        private void ReadImage()
        {
            _ix = ReadShort(); // (sub)image position & size
            _iy = ReadShort();
            _iw = ReadShort();
            _ih = ReadShort();
            var packed = Read();
            _lctFlag = (packed & 0x80) != 0; // 1 - local color table flag
            _interlace = (packed & 0x40) != 0; // 2 - interlace flag
            // 3 - sort flag
            // 4-5 - reserved
            _lctSize = 2 << (packed & 7); // 6-8 - local color table size

            if (_lctFlag)
            {
                _lct = ReadColorTable(_lctSize); // read table
                _act = _lct; // make local table active
            }
            else
            {
                _act = _gct; // make global table active
                if (_bgIndex == _transIndex)
                    _bgColor = 0;
            }

            var save = 0;
            if (_transparency)
            {
                save = _act[_transIndex];
                _act[_transIndex] = 0; // set transparent color if specified
            }

            if (_act == null) _status = Status.StatusFormatError; // no color table defined

            if (Error()) return;

            DecodeImageData(); // decode pixel data
            Skip();

            if (Error()) return;


            // create new image to receive frame data
            if (_image == null) _image = new int[_width * _height];
            if (_compatibilityMode && _prevImage == null) _prevImage = new int[_width * _height];
            if (_bimage == null) _bimage = new byte[_width * _height * sizeof(int)];

            if (_compatibilityMode && _lastDispose != 3) Buffer.BlockCopy(_image, 0, _prevImage, 0, _image.Length * sizeof(int)); // keep a copy of the image in compatibility mode

            SetPixels(); // transfer pixel data to image

            //copy image
            Buffer.BlockCopy(_image, 0, _bimage, 0, _bimage.Length);

            if (_compatibilityMode && _dispose == 3) Buffer.BlockCopy(_prevImage, 0, _image, 0, _prevImage.Length * sizeof(int)); // keep a copy of the image in compatibility mode


            _currentFrame = new GifFrame(_bimage, _delay); //save image as current image

            _frameCount++;

            if (_transparency) _act[_transIndex] = save;
            ResetFrame();
        }

        /**
		 * Reads Logical Screen Descriptor
		 */
        private void ReadLsd()
        {
            // logical screen size
            _width = ReadShort();
            _height = ReadShort();

            // packed fields
            var packed = Read();
            _gctFlag = (packed & 0x80) != 0; // 1   : global color table flag
            // 2-4 : color resolution
            // 5   : gct sort flag
            _gctSize = 2 << (packed & 7); // 6-8 : gct size

            _bgIndex = Read(); // background color index
            //_pixelAspect = Read(); // pixel aspect ratio
            Read(); // pixel aspect ratio
        }

        /**
		 * Reads Netscape extenstion to obtain iteration count
		 */
        private void ReadNetscapeExt()
        {
            do
            {
                ReadBlock();
                if (_block[0] != 1) continue;
                // loop count sub-block
                var b1 = _block[1] & 0xff;
                var b2 = _block[2] & 0xff;
                _loopCount = (b2 << 8) | b1;
            } while (_blockSize > 0 && !Error());
        }

        /**
		 * Reads next 16-bit value, LSB first
		 */
        private int ReadShort()
        {
            // read 16-bit value, LSB first
            return Read() | (Read() << 8);
        }

        /**
		 * Resets frame state for reading next image.
		 */
        private void ResetFrame()
        {
            _lastDispose = _dispose;
            _lix = _ix;
            _liy = _iy;
            _liw = _iw;
            _lih = _ih;
            _lastBgColor = _bgColor;
            _lct = null;
        }

        /**
         * Skips variable length blocks up to and including
         * next zero length block.
         */
        private void Skip()
        {
            do
            {
                ReadBlock();
            } while (_blockSize > 0 && !Error());
        }

        /// <summary>
        ///     Holds frame data and frame delay
        /// </summary>
        public class GifFrame
        {
            /// <summary>
            ///     Time in seconds till next frame
            /// </summary>
            public float Delay;

            /// <summary>
            ///     Frame data
            /// </summary>
            public byte[] Image;

            public GifFrame(byte[] im, float del)
            {
                Image = im;
                Delay = del;
            }
        }

        #region Public Fields

        /// <summary>
        ///     Number of frames in GIF. Value is 0 as long as the last frame has not been reached
        /// </summary>
        public int NumberOfFrames { get; private set; }

        /// <summary>
        ///     Set to true when all frames have been read
        /// </summary>
        public bool AllFramesRead { get; private set; }

        /// <summary>
        /// </summary>

        #endregion

        #region Internal fields

        private Stream _inStream; // Data stream holding the GIF data

        private Status _status; // Status of the decoder
        private int _width; // full image width
        private int _height; // full image height
        private bool _gctFlag; // global color table used
        private int _gctSize; // size of global color table
        private int _loopCount = 1; // iterations; 0 = repeat forever
        private int[] _gct; // global color table
        private int[] _lct; // local color table
        private int[] _act; // active color table

        private int _bgIndex; // background color index
        private int _bgColor; // background color

        private int _lastBgColor; // previous bg color
        //private int _pixelAspect; // pixel aspect ratio

        private bool _lctFlag; // local color table flag
        private bool _interlace; // interlace flag
        private int _lctSize; // local color table size

        private int _ix, _iy, _iw, _ih; // current image rectangle
        private int _lix, _liy, _liw, _lih; // last image rect
        private int[] _image; // current frame
        private byte[] _bimage; // current frame

        private readonly byte[] _block = new byte[256]; // current data block
        private int _blockSize; // block size

        private readonly bool _compatibilityMode; // when set to true all disposal methods are supported
        private int[] _prevImage; // previous image. Used in compatibility mode


        private int _dispose; // last graphic control extension info. 0=no action; 1=leave in place; 2=restore to bg; 3=restore to prev

        private int _lastDispose;
        private bool _transparency; // use transparent color
        private float _delay; // delay in milliseconds
        private int _transIndex; // transparent color index
        private long _imageDataOffset; // start of image data in stream

        private const int MaxStackSize = 4096; // max decoder pixel stack size
        private const int NullCode = -1; // max decoder pixel stack size

        // LZW decoder working arrays
        private short[] _prefix;
        private byte[] _suffix;
        private byte[] _pixelStack;
        private byte[] _pixelIndexes;

        //protected ArrayList frames; // frames read from current file
        //protected bool CacheFrames;
        private GifFrame _currentFrame;
        private int _frameCount;

        #endregion

        #region Public API

        /**
         * Gets display duration for specified frame.
         * 
         * @param n int index of frame
         * @return delay in milliseconds
         */
        public float GetDelayCurrentFrame()
        {
            return _currentFrame.Delay;
        }

        /**
         * Gets the number of frames read from file.
         * @return frame count. Only those frames that are decoded are counted
         */
        public int GetFrameCount()
        {
            return _frameCount;
        }

        /**
         * Gets the "Netscape" iteration count, if any.
         * A count of 0 means repeat indefinitely.
         * 
         * @return iteration count if one was specified, else 1.
         */
        public int GetLoopCount()
        {
            return _loopCount;
        }

        /**
         * Gets the next image frame
         * 
         * @return BufferedImage representation of frame, or null if n is invalid.
         */
        public GifFrame GetCurrentFrame()
        {
            return _currentFrame;
        }

        /**
         * Gets image width.
         * 
         * @return GIF image width
         */
        public int GetFrameWidth()
        {
            return _width;
        }

        /**
         * Gets image height.
         * 
         * @return GIF image height
         */
        public int GetFrameHeight()
        {
            return _height;
        }

        /**
         * Reads GIF image from stream
         * 
         * @param BufferedInputStream containing GIF file.
         * @return read status code (0 = no errors)
         */
        public Status Read(Stream inStream)
        {
            Init();
            if (inStream != null)
            {
                _inStream = inStream;
                ReadHeader();
                if (Error()) _status = Status.StatusFormatError;
            }
            else
            {
                _status = Status.StatusOpenError;
            }

            return _status;
        }

        /**
         * Reset reading of the GIF
         */
        public void Reset()
        {
            _inStream.Position = 0;
            Read(_inStream);
        }

        /**
         * Close the stream
         */
        public void Close()
        {
            _inStream.Dispose();
        }

        /**
         * Returns true if an error was encountered during reading/decoding
         */
        private bool Error()
        {
            return _status != Status.StatusOk;
        }

        /**
         * Reads the next frame
         */
        public void ReadNextFrame(bool loop)
        {
            // read GIF file content blocks
            while (!Error())
            {
                var code = Read();
                switch (code)
                {
                    case 0x2C: // image separator
                        ReadImage();
                        //if (readSingleFrame) return;
                        return;
                    case 0x21: // extension
                        code = Read();
                        switch (code)
                        {
                            case 0xf9: // graphics control extension
                                ReadGraphicControlExt();
                                break;

                            case 0xff: // application extension
                                ReadBlock();
                                var app = "";
                                for (var i = 0; i < 11; i++) app += (char)_block[i];
                                if (app.Equals("NETSCAPE2.0"))
                                    ReadNetscapeExt();
                                else
                                    Skip(); // don't care
                                break;

                            default: // uninteresting extension
                                Skip();
                                break;
                        }

                        break;
                    case 0x3b: // terminator
                        //we read all frames
                        NumberOfFrames = _frameCount;
                        if (loop)
                        {
                            RewindReader();
                            break;
                        }

                        AllFramesRead = true;
                        return;
                    case 0x00: // bad byte, but keep going and see what happens
                        break;
                    default:
                        _status = Status.StatusFormatError;
                        break;
                }
            }
        }

        #endregion
    }
}