using System.Text;

namespace NoahStolk.WaveParser;

public class WaveData
{
	private const string _riffHeader = "RIFF";
	private const string _formatHeader = "WAVE";
	private const string _fmtHeader = "fmt ";
	private const string _dataHeader = "data";
	private const int _fmtSize = 16;
	private const int _audioFormat = 1;

	// TODO: Add unit tests.
	public WaveData(string path)
		: this(new FileStream(path, FileMode.Open))
	{
	}

	public WaveData(byte[] contents)
		: this(new MemoryStream(contents))
	{
	}

	public WaveData(Stream stream)
	{
		BinaryReader br = new(stream);
		string riffHeader = Encoding.Default.GetString(br.ReadBytes(4));
		if (riffHeader != _riffHeader)
			throw new WaveParseException($"Expected '{_riffHeader}' header (got '{riffHeader}').");

		_ = br.ReadInt32(); // Amount of bytes remaining at this point (after these 4).

		string format = Encoding.Default.GetString(br.ReadBytes(4));
		if (format != _formatHeader)
			throw new WaveParseException($"Expected '{_formatHeader}' header (got '{format}').");

		string fmtHeader = Encoding.Default.GetString(br.ReadBytes(4));
		if (fmtHeader != _fmtHeader)
			throw new WaveParseException($"Expected '{_fmtHeader}' header (got '{fmtHeader}').");

		int fmtSize = br.ReadInt32();
		if (fmtSize != _fmtSize)
			throw new WaveParseException($"Expected FMT data chunk size to be {_fmtSize} (got {fmtSize}).");

		short audioFormat = br.ReadInt16();
		if (audioFormat != _audioFormat)
			throw new WaveParseException($"Expected audio format to be {_audioFormat} (got {audioFormat}).");

		Channels = br.ReadInt16();
		SampleRate = br.ReadInt32();
		ByteRate = br.ReadInt32();
		BlockAlign = br.ReadInt16();
		BitsPerSample = br.ReadInt16();

		int expectedByteRate = SampleRate * Channels * BitsPerSample / 8;
		int expectedBlockAlign = Channels * BitsPerSample / 8;
		if (ByteRate != expectedByteRate)
			throw new WaveParseException($"Expected byte rate to be {expectedByteRate} (got {ByteRate}).");
		if (BlockAlign != expectedBlockAlign)
			throw new WaveParseException($"Expected block align to be {expectedBlockAlign} (got {BlockAlign}).");

		long finalDataHeaderPosition = br.BaseStream.Length - (_dataHeader.Length + sizeof(int));
		string dataHeader;
		do
		{
			if (br.BaseStream.Position >= finalDataHeaderPosition)
				throw new WaveParseException($"Could not find '{_dataHeader}' header.");

			dataHeader = Encoding.Default.GetString(br.ReadBytes(4));
		}
		while (dataHeader != _dataHeader);

		Size = br.ReadInt32();
		Data = br.ReadBytes(Size);

		br.Dispose();
		stream.Dispose();
	}

	public short Channels { get; }
	public int SampleRate { get; }
	public int ByteRate { get; }
	public short BlockAlign { get; }
	public short BitsPerSample { get; }
	public int Size { get; }
	public byte[] Data { get; }

	public double GetLength()
	{
		int sampleCount = Size / (BitsPerSample / 8) / Channels;
		return sampleCount / (double)SampleRate;
	}
}
