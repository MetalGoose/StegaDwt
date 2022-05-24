using CenterSpace.NMath.Core;
using System;
using System.IO;
using System.Linq;
using System.Text;
using ZXing.Common.ReedSolomon;
using static CenterSpace.NMath.Core.Wavelet;

namespace StegaDwt.DWT
{
    public class Encoder
    {
        private readonly Encoding _encoding;
        private readonly int _parityLength;
        private readonly int _sampleRate;
        private readonly GenericGF _currentField;
        private readonly int _decomposeLevel;
        private readonly int _bytesPerSample;
        private readonly float[] _originFilBuffer;
        private readonly int _channels;
        private readonly Wavelets _waveletType;
        private readonly string _txtKeyFile;

        public Encoder(WavFile wavFile, string keyFilePath, int decompLevel, Wavelets waveletType)
        {
            _decomposeLevel = decompLevel;
            _parityLength = 12;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _encoding = Encoding.GetEncoding("windows-1251");
            _currentField = new GenericGF(285, 256, 0);
            _bytesPerSample = wavFile.BytesPerSample;
            _originFilBuffer = wavFile.FloatAudioBuffer;
            _channels = wavFile.Channels;
            _sampleRate = wavFile.SampleRate;
            _txtKeyFile = keyFilePath;
            _waveletType = waveletType;
        }

        /// <summary>
        /// Encodes the message with the Reed-Solomon code and hides it at a given level of decomposition of the Wav file
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>Samples with a message recorded in them</returns>
        public DwtTransformInfo EncodeMsg(string msg)
        {
            // Choose wavelet
            var wavelet = new FloatWavelet(_waveletType);
            // Build DWT object
            var dwt = new FloatDWT(_originFilBuffer, wavelet);
            // Decompose signal with DWT
            dwt.Decompose(_decomposeLevel);


            var details = dwt.WaveletCoefficients(DiscreteWaveletTransform.WaveletCoefficientType.Details, _decomposeLevel);
            var approx = dwt.WaveletCoefficients(DiscreteWaveletTransform.WaveletCoefficientType.Approximation, _decomposeLevel);
            var changedDetails = InsertMsg(msg, details);

            // Rebuild the signal
            float[] approxFromUpperLevel;
            float[] detailsFromUpperLevel;

            approxFromUpperLevel = dwt.IDWT(approx, changedDetails);

            var currentDecompLevel = dwt.CurrentDecompLevel() - 1;

            for (int i = currentDecompLevel; i > 0; i--)
            {
                detailsFromUpperLevel = dwt.WaveletCoefficients(DiscreteWaveletTransform.WaveletCoefficientType.Details, i);
                var sizeMatch = !(approxFromUpperLevel.Length > detailsFromUpperLevel.Length);
                approxFromUpperLevel = dwt.IDWT(approxFromUpperLevel, detailsFromUpperLevel, sizeMatch);
            }

            return new DwtTransformInfo
            {
                DecompLvl = _decomposeLevel,
                Details = details,
                Approx = approx,
                ResultSamples = approxFromUpperLevel
            };
        }

        public string DecodeMsg()
        {
            if (_originFilBuffer is null || _originFilBuffer.Length == 0)
            {
                throw new ArgumentException("Incorrect data");
            }

            // Choose wavelet
            var wavelet = new FloatWavelet(_waveletType);
            // Build DWT object
            var dwt = new FloatDWT(_originFilBuffer, wavelet);
            // Decompose signal with DWT
            dwt.Decompose(_decomposeLevel);

            var details = dwt.WaveletCoefficients(DiscreteWaveletTransform.WaveletCoefficientType.Details, _decomposeLevel);
            var encodedMessageBytes = ExtractMsg(details);

            var rsd = new ReedSolomonDecoder(_currentField);
            var encodedMessageInts = encodedMessageBytes.Select(x => (int)x).ToArray();
            var canDecode = rsd.decode(encodedMessageInts, _parityLength);

            if (canDecode)
            {
                return _encoding.GetString(encodedMessageInts.Select(x => (byte)x).ToArray());
            }
            else
            {
                return string.Empty;
            }
        }

        private float[] InsertMsg(string msg, float[] data)
        {
            MemoryStream sourceStream = null;
            MemoryStream destinationStream = null;
            Stream messageStream = null;
            //open the key file
            Stream keyStream = new FileStream(_txtKeyFile, FileMode.Open);

            try
            {
                //create a stream that contains the message, preceeded by its length
                messageStream = GetRsMessageStream(msg);
                var buffer = data.ToByteArray();
                sourceStream = new MemoryStream(buffer);
                //create an empty stream for the carrier wave
                destinationStream = new MemoryStream(buffer.Length);

                Hide(messageStream, sourceStream, destinationStream, keyStream);

                var result = destinationStream.ToArray().ToFloatArray();

                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error insert message", ex);
            }
            finally
            {
                if (messageStream != null) { messageStream.Close(); }
                if (sourceStream != null) { sourceStream.Close(); }
                if (destinationStream != null) { destinationStream.Close(); }
                if (keyStream != null) { keyStream.Close(); }
            }
        }

        private byte[] ExtractMsg(float[] dataWithMessage)
        {
            MemoryStream sourceStream = null;
            Stream messageStream = null;
            //open the key file
            Stream keyStream = new FileStream(_txtKeyFile, FileMode.Open);

            try
            {
                var buffer = dataWithMessage.ToByteArray();
                sourceStream = new MemoryStream(buffer);
                messageStream = new MemoryStream();

                Extract(messageStream, sourceStream, keyStream);

                messageStream.Seek(0, SeekOrigin.Begin);

                var resultMessage = new StreamReader(messageStream).ReadToEnd();
                return _encoding.GetBytes(resultMessage);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error writing message", ex);
            }
            finally
            {
                if (messageStream != null) { messageStream.Close(); }
                if (sourceStream != null) { sourceStream.Close(); }
                if (keyStream != null) { keyStream.Close(); }
            }
        }

        private void Hide(Stream messageStream, Stream sourceStream, Stream destinationStream, Stream keyStream)
        {
            byte[] waveBuffer = new byte[_bytesPerSample];
            byte message, bit, waveByte;
            int messageBuffer; //receives the next byte of the message or -1
            int keyByte;

            while ((messageBuffer = messageStream.ReadByte()) >= 0)
            {
                //read one byte of the message stream
                message = (byte)messageBuffer;

                //for each bit in message
                for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                {
                    //read a next byte from the key
                    keyByte = GetNextKeyByte(keyStream);

                    //skip a couple of samples
                    for (int n = 0; n < keyByte - 1; n++)
                    {
                        //copy one sample from the clean stream to the carrier stream
                        var toread = (int)Math.Min(waveBuffer.Length, sourceStream.Length - sourceStream.Position);
                        int read = sourceStream.Read(waveBuffer, 0, toread);
                        destinationStream.Write(waveBuffer, 0, read);
                    }

                    //read one sample from the wave stream
                    sourceStream.Read(waveBuffer, 0, waveBuffer.Length);
                    waveByte = waveBuffer[_bytesPerSample - 1];

                    //get the next bit from the current message byte...
                    bit = (byte)(((message & (byte)(1 << bitIndex)) > 0) ? 1 : 0);

                    //...place it in the last bit of the sample
                    if ((bit == 1) && ((waveByte % 2) == 0))
                    {
                        waveByte += 1;
                    }
                    else if ((bit == 0) && ((waveByte % 2) == 1))
                    {
                        waveByte -= 1;
                    }

                    waveBuffer[_bytesPerSample - 1] = waveByte;

                    //write the result to destinationStream
                    destinationStream.Write(waveBuffer, 0, _bytesPerSample);
                }
            }

            //copy the rest of the wave without changes
            waveBuffer = new byte[sourceStream.Length - sourceStream.Position];
            sourceStream.Read(waveBuffer, 0, waveBuffer.Length);
            destinationStream.Write(waveBuffer, 0, waveBuffer.Length);
        }

        /// <summary>Extract a message from [sourceStream] into [messageStream]</summary>
        /// <param name="messageStream">Empty stream to receive the extracted message</param>
        /// <param name="sourceStream">
        /// A key stream that specifies how many samples shall be
        /// skipped between two carrier samples
        /// </param>
        private void Extract(Stream messageStream, Stream sourceStream, Stream keyStream)
        {
            byte[] waveBuffer = new byte[_bytesPerSample];
            byte message, bit, waveByte;
            int messageLength = 0; //expected length of the message
            int keyByte; //distance of the next carrier sample

            while ((messageLength == 0 || messageStream.Length < messageLength))
            {
                //clear the message-byte
                message = 0;

                //for each bit in message
                for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                {
                    //read a byte from the key
                    keyByte = GetNextKeyByte(keyStream);

                    //skip a couple of samples
                    for (int n = 0; n < keyByte - 1; n++)
                    {
                        //read one sample from the wave stream
                        sourceStream.Read(waveBuffer, 0, waveBuffer.Length);
                    }

                    sourceStream.Read(waveBuffer, 0, waveBuffer.Length);
                    waveByte = waveBuffer[_bytesPerSample - 1];

                    //get the last bit of the sample...
                    bit = (byte)(((waveByte % 2) == 0) ? 0 : 1);

                    //...write it into the message-byte
                    message += (byte)(bit << bitIndex);
                }

                //add the re-constructed byte to the message
                messageStream.WriteByte(message);

                if (messageLength == 0 && messageStream.Length == 4)
                {
                    //first 4 bytes contain the message's length
                    messageStream.Seek(0, SeekOrigin.Begin);
                    messageLength = new BinaryReader(messageStream).ReadInt32();
                    messageStream.Seek(0, SeekOrigin.Begin);
                    messageStream.SetLength(0);
                }
            }

        }

        /// <summary>
        /// Read the next byte of the key stream.
        /// Reset the stream if it is too short.
        /// </summary>
        /// <param name="keyStream">The key stream</param>
        /// <returns>The next key byte</returns>
        private static byte GetNextKeyByte(Stream keyStream)
        {
            int keyValue;
            if ((keyValue = keyStream.ReadByte()) < 0)
            {
                keyStream.Seek(0, SeekOrigin.Begin);
                keyValue = keyStream.ReadByte();
                if (keyValue == 0)
                {
                    keyValue = 1;
                }
            }
            return (byte)keyValue;
        }

        /// <summary>
        /// Write length an content of the message file/text into a stream
        /// </summary>
        private Stream GetRsMessageStream(string msg)
        {
            var rse = new ReedSolomonEncoder(_currentField);
            var encodedMsg = _encoding.GetBytes(msg);

            var msgBytesPlusParity = new byte[encodedMsg.Length + _parityLength];

            for (int i = 0; i < encodedMsg.Length; i++)
            {
                msgBytesPlusParity[i] = encodedMsg[i];
            }

            var rsEncodedMsg = msgBytesPlusParity.Select(x => (int)x).ToArray();

            rse.encode(rsEncodedMsg, _parityLength);
            var rsEncodedMsgBytes = rsEncodedMsg.Select(x => (byte)x).ToArray();

            var stream = new MemoryStream();
            var messageWriter = new BinaryWriter(stream);

            messageWriter.Write(rsEncodedMsgBytes.Length);
            messageWriter.Write(rsEncodedMsgBytes);
            messageWriter.Seek(0, SeekOrigin.Begin);
            return messageWriter.BaseStream;
        }

        private long CheckKeyForMessage(Stream keyStream, long messageLength)
        {
            long messageLengthBits = messageLength * 8;
            long countRequiredSamples = 0;

            if (messageLengthBits > keyStream.Length)
            {
                long keyLength = keyStream.Length;

                // read existing key
                byte[] keyBytes = new byte[keyLength];
                keyStream.Read(keyBytes, 0, keyBytes.Length);

                // Every byte stands for the distance between two useable samples.
                // The sum of those distances is the required count of samples.
                countRequiredSamples = SumKeyArray(keyBytes);

                // The key must be repeated, until every bit of the message has a key byte.
                double countKeyCopies = messageLengthBits / keyLength;
                countRequiredSamples = (long)(countRequiredSamples * countKeyCopies);
            }
            else
            {
                byte[] keyBytes = new byte[messageLengthBits];
                keyStream.Read(keyBytes, 0, keyBytes.Length);
                countRequiredSamples = SumKeyArray(keyBytes);
            }

            keyStream.Seek(0, SeekOrigin.Begin);
            return countRequiredSamples;
        }

        private long SumKeyArray(byte[] values)
        {
            long sum = 0;
            foreach (int value in values)
            {   // '0' causes a distance of one sample,
                // every other key causes a distance of its exact value.
                sum += (value == 0) ? 1 : value;
            }
            return sum;
        }
    }
}
