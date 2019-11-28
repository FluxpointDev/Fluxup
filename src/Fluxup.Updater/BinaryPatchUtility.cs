using System;
using System.IO;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;

// Adapted from https://github.com/LogosBible/bsdiff.net/blob/master/src/bsdiff/BinaryPatchUtility.cs

//TODO: Use this for delta packages...
namespace Fluxup.Updater
{
    /*
    The original bsdiff.c source code (http://www.daemonology.net/bsdiff/) is
    distributed under the following license:

    Copyright 2003-2005 Colin Percival
    All rights reserved

    Redistribution and use in source and binary forms, with or without
    modification, are permitted providing that the following conditions
    are met:
    1. Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
    2. Redistributions in binary form must reproduce the above copyright
        notice, this list of conditions and the following disclaimer in the
        documentation and/or other materials provided with the distribution.

    THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
    IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
    WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
    ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
    DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
    OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
    HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
    STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
    IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
    POSSIBILITY OF SUCH DAMAGE.
    */
    internal class BsdiffPatchUtility
    {
        /// <summary>
        /// Applies a binary patch (in <a href="http://www.daemonology.net/bsdiff/">bsdiff</a> format) to the data in
        /// <paramref name="input"/> and writes the results of patching to <paramref name="output"/>.
        /// </summary>
        /// <param name="input">A <see cref="Stream"/> containing the input data.</param>
        /// <param name="openPatchStream">A func that can open a <see cref="Stream"/> positioned at the start of the patch data.
        /// This stream must support reading and seeking, and <paramref name="openPatchStream"/> must allow multiple streams on
        /// the patch to be opened concurrently.</param>
        /// <param name="output">A <see cref="Stream"/> to which the patched data is written.</param>
        public static void Apply(Stream input, Func<Stream> openPatchStream, Stream output)
        {
            // check arguments
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (openPatchStream == null)
            {
                throw new ArgumentNullException(nameof(openPatchStream));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            /*
            File format:
                0   8   "BSDIFF40"
                8   8   X
                16  8   Y
                24  8   sizeof(newfile)
                32  X   bzip2(control block)
                32+X    Y   bzip2(diff block)
                32+X+Y  ??? bzip2(extra block)
            with control block a set of triples (x,y,z) meaning "add x bytes
            from oldfile to x bytes from the diff block; copy y bytes from the
            extra block; seek forwards in oldfile by z bytes".
            */
            // read header
            long controlLength, diffLength, newSize;
            using (var patchStream = openPatchStream())
            {
                // check patch stream capabilities
                if (!patchStream.CanRead)
                {
                    throw new ArgumentException("Patch stream must be readable.", nameof(openPatchStream));
                }

                if (!patchStream.CanSeek)
                {
                    throw new ArgumentException("Patch stream must be seekable.", nameof(openPatchStream));
                }

                var header = patchStream.ReadExactly(HeaderSize);

                // check for appropriate magic
                var signature = ReadInt64(header, 0);
                if (signature != FileSignature)
                {
                    throw new InvalidOperationException("Corrupt patch.");
                }

                // read lengths from header
                controlLength = ReadInt64(header, 8);
                diffLength = ReadInt64(header, 16);
                newSize = ReadInt64(header, 24);
                if (controlLength < 0 || diffLength < 0 || newSize < 0)
                {
                    throw new InvalidOperationException("Corrupt patch.");
                }
            }

            // preallocate buffers for reading and writing
            var newData = new byte[BufferSize];
            var oldData = new byte[BufferSize];

            // prepare to read three parts of the patch in parallel
            using var compressedControlStream = openPatchStream();
            using var compressedDiffStream = openPatchStream();
            using var compressedExtraStream = openPatchStream();
            // seek to the start of each part
            compressedControlStream.Seek(HeaderSize, SeekOrigin.Current);
            compressedDiffStream.Seek(HeaderSize + controlLength, SeekOrigin.Current);
            compressedExtraStream.Seek(HeaderSize + controlLength + diffLength, SeekOrigin.Current);

            // decompress each part (to read it)
            using var controlStream = new BZip2Stream(compressedControlStream, CompressionMode.Decompress, false);
            using var diffStream = new BZip2Stream(compressedDiffStream, CompressionMode.Decompress, false);
            using var extraStream = new BZip2Stream(compressedExtraStream, CompressionMode.Decompress, false);
            var control = new long[3];
            var buffer = new byte[8];

            var oldPosition = 0;
            var newPosition = 0;
            while (newPosition < newSize)
            {
                // read control data
                for (var i = 0; i < 3; i++)
                {
                    controlStream.ReadExactly(buffer, 0, 8);
                    control[i] = ReadInt64(buffer, 0);
                }

                // sanity-check
                if (newPosition + control[0] > newSize)
                {
                    throw new InvalidOperationException("Corrupt patch.");
                }

                // seek old file to the position that the new data is diffed against
                input.Position = oldPosition;

                var bytesToCopy = (int)control[0];
                while (bytesToCopy > 0)
                {
                    var actualBytesToCopy = Math.Min(bytesToCopy, BufferSize);

                    // read diff string
                    diffStream.ReadExactly(newData, 0, actualBytesToCopy);

                    // add old data to diff string
                    var availableInputBytes = Math.Min(actualBytesToCopy, (int)(input.Length - input.Position));
                    input.ReadExactly(oldData, 0, availableInputBytes);

                    for (var index = 0; index < availableInputBytes; index++)
                    {
                        newData[index] += oldData[index];
                    }

                    output.Write(newData, 0, actualBytesToCopy);

                    // adjust counters
                    newPosition += actualBytesToCopy;
                    oldPosition += actualBytesToCopy;
                    bytesToCopy -= actualBytesToCopy;
                }

                // sanity-check
                if (newPosition + control[1] > newSize)
                {
                    throw new InvalidOperationException("Corrupt patch.");
                }

                // read extra string
                bytesToCopy = (int)control[1];
                while (bytesToCopy > 0)
                {
                    var actualBytesToCopy = Math.Min(bytesToCopy, BufferSize);

                    extraStream.ReadExactly(newData, 0, actualBytesToCopy);
                    output.Write(newData, 0, actualBytesToCopy);

                    newPosition += actualBytesToCopy;
                    bytesToCopy -= actualBytesToCopy;
                }

                // adjust position
                oldPosition = (int)(oldPosition + control[2]);
            }
        }

        private static long ReadInt64(byte[] buf, int offset)
        {
            long value = buf[offset + 7] & 0x7F;

            for (var index = 6; index >= 0; index--)
            {
                value *= 256;
                value += buf[offset + index];
            }

            if ((buf[offset + 7] & 0x80) != 0)
            {
                value = -value;
            }

            return value;
        }

        private const long FileSignature = 0x3034464649445342L;
        private const int HeaderSize = 32;
        private const int BufferSize = 1048576;
    }
    
    /// <summary>
    /// Provides helper methods for working with <see cref="Stream"/>.
    /// </summary>
    internal static class StreamUtility
    {
        /// <summary>
        /// Reads exactly <paramref name="count"/> bytes from <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="count">The count of bytes to read.</param>
        /// <returns>A new byte array containing the data read from the stream.</returns>
        public static byte[] ReadExactly(this Stream stream, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var buffer = new byte[count];
            ReadExactly(stream, buffer, 0, count);
            return buffer;
        }

        /// <summary>
        /// Reads exactly <paramref name="count"/> bytes from <paramref name="stream"/> into
        /// <paramref name="buffer"/>, starting at the byte given by <paramref name="offset"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="buffer">The buffer to read data into.</param>
        /// <param name="offset">The offset within the buffer at which data is first written.</param>
        /// <param name="count">The count of bytes to read.</param>
        public static void ReadExactly(this Stream stream, byte[] buffer, int offset, int count)
        {
            // check arguments
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 0 || buffer.Length - offset < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            while (count > 0)
            {
                // read data
                var bytesRead = stream.Read(buffer, offset, count);

                // check for failure to read
                if (bytesRead == 0)
                    throw new EndOfStreamException();

                // move to next block
                offset += bytesRead;
                count -= bytesRead;
            }
        }
    }
}