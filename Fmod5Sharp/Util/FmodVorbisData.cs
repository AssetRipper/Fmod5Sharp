using Sewer56.BitStream;
using Sewer56.BitStream.ByteStreams;
using System;
using System.Text;
using System.Text.Json.Serialization;

namespace Fmod5Sharp.Util;

internal class FmodVorbisData
{
    [JsonPropertyName("headerBytes")]
    public byte[] HeaderBytes { get; set; }
    
    [JsonPropertyName("seekBit")]
    public int SeekBit { get; set; }
    
    [JsonConstructor]
    public FmodVorbisData(byte[] headerBytes, int seekBit)
    {
        HeaderBytes = headerBytes;
        SeekBit = seekBit;
    }

    [JsonIgnore] private byte[] BlockFlags { get; set; } = Array.Empty<byte>();
    
    private bool _initialized;

    internal void InitBlockFlags()
    {
        if(_initialized)
            return;

        _initialized = true;
        
        var bitStream = new BitStream<ArrayByteStream>(new ArrayByteStream(HeaderBytes));

        if (bitStream.Read8(8) != 5) //packing type 5 == books
            return;

        if (ReadString(bitStream, 6) != "vorbis") //validate magic
            return;

        //Whole bytes, bit remainder
        bitStream.Seek(SeekBit / 8, (byte)(SeekBit % 8));

        //Read 6 bits and add one
        var numModes = bitStream.Read8(6) + 1;

        //Read the first bit of each mode and skip the rest of the mode data. These are our flags.
        BlockFlags = new byte[numModes];
		for (int i = 0; i < numModes; i++)
        {
			var flag = bitStream.ReadBit();

			//Skip the bits we don't care about
			bitStream.Read16(16);
			bitStream.Read16(16);
			bitStream.Read8(8);

			BlockFlags[i] = flag;
		}
    }

    public int GetPacketBlockSize(byte[] packetBytes)
    {
        var bitStream = new BitStream<ArrayByteStream>(new ArrayByteStream(packetBytes));

        if (bitStream.ReadBit() != 0)
            return 0;

        var mode = 0;

        if (BlockFlags.Length > 1)
            mode = bitStream.Read8(BlockFlags.Length - 1);
		
		if (BlockFlags[mode] == 1)
            return 2048;

        return 256;
    }

	/// <summary>
	/// Read a string based on the current stream and bit position and the <see cref="BitStream"/> encoding
	/// </summary>
	/// <param name="length">Length of the string to read</param>
	private static string ReadString(BitStream<ArrayByteStream> bitStream, int length)
	{
        Span<byte> span = new byte[length];
        bitStream.Read(span);
		return Encoding.ASCII.GetString(span);
	}
}