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

	// TODO: Add more ctors for Stream and byte[].
	// TODO: Add unit tests.
	public WaveData(string path)
	{
		using FileStream fs = new(path, FileMode.Open);
		using BinaryReader br = new(fs);
		string riffHeader = Encoding.Default.GetString(br.ReadBytes(4));
		if (riffHeader != _riffHeader)
			throw new WaveParseException($"Expected '{_riffHeader}' header (got '{riffHeader}') for .wav file '{path}'.");

		_ = br.ReadInt32(); // Amount of bytes remaining at this point (after these 4).

		string format = Encoding.Default.GetString(br.ReadBytes(4));
		if (format != _formatHeader)
			throw new WaveParseException($"Expected '{_formatHeader}' header (got '{format}') for .wav file '{path}'.");

		string fmtHeader = Encoding.Default.GetString(br.ReadBytes(4));
		if (fmtHeader != _fmtHeader)
			throw new WaveParseException($"Expected '{_fmtHeader}' header (got '{fmtHeader}') for .wav file '{path}'.");

		int fmtSize = br.ReadInt32();
		if (fmtSize != _fmtSize)
			throw new WaveParseException($"Expected FMT data chunk size to be {_fmtSize} (got {fmtSize}) for .wav file '{path}'.");

		short audioFormat = br.ReadInt16();
		if (audioFormat != _audioFormat)
			throw new WaveParseException($"Expected audio format to be {_audioFormat} (got {audioFormat}) for .wav file '{path}'.");

		Channels = br.ReadInt16();
		SampleRate = br.ReadInt32();
		ByteRate = br.ReadInt32();
		BlockAlign = br.ReadInt16();
		BitsPerSample = br.ReadInt16();

		int expectedByteRate = SampleRate * Channels * BitsPerSample / 8;
		int expectedBlockAlign = Channels * BitsPerSample / 8;
		if (ByteRate != expectedByteRate)
			throw new WaveParseException($"Expected byte rate to be {expectedByteRate} (got {ByteRate}) for .wav file '{path}'.");
		if (BlockAlign != expectedBlockAlign)
			throw new WaveParseException($"Expected block align to be {expectedBlockAlign} (got {BlockAlign}) for .wav file '{path}'.");

		long finalDataHeaderPosition = br.BaseStream.Length - (_dataHeader.Length + sizeof(int));
		string dataHeader;
		do
		{
			if (br.BaseStream.Position >= finalDataHeaderPosition)
				throw new WaveParseException($"Could not find '{_dataHeader}' header for .wav file '{path}'.");

			dataHeader = Encoding.Default.GetString(br.ReadBytes(4));
		}
		while (dataHeader != _dataHeader);

		Size = br.ReadInt32();
		Data = br.ReadBytes(Size);
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
