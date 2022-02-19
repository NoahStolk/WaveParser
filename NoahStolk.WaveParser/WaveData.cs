using System.Text;

namespace NoahStolk.WaveParser;

/// <summary>
/// Reads wave data from a .wav file based on the <see href="http://soundfile.sapp.org/doc/WaveFormat/">wave format specification</see>.
/// </summary>
public class WaveData
{
	private const string _riffHeader = "RIFF";
	private const string _formatHeader = "WAVE";
	private const string _fmtHeader = "fmt ";
	private const string _dataHeader = "data";
	private const int _fmtMinimumSize = 16;
	private const int _audioFormat = 1;

	public WaveData(byte[] waveFileContents)
	{
		using MemoryStream ms = new(waveFileContents);
		using BinaryReader br = new(ms);
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
		if (fmtSize < _fmtMinimumSize)
			throw new WaveParseException($"Expected FMT data chunk size to be at least {_fmtMinimumSize} (got {fmtSize}).");

		short audioFormat = br.ReadInt16();
		if (audioFormat != _audioFormat)
			throw new WaveParseException($"Expected audio format to be {_audioFormat} (got {audioFormat}).");

		Channels = br.ReadInt16();
		SampleRate = br.ReadInt32();
		ByteRate = br.ReadInt32();
		BlockAlign = br.ReadInt16();
		BitsPerSample = br.ReadInt16();

		br.BaseStream.Seek(_fmtMinimumSize - fmtSize, SeekOrigin.Current);

		int expectedByteRate = SampleRate * Channels * BitsPerSample / 8;
		int expectedBlockAlign = Channels * BitsPerSample / 8;
		if (ByteRate != expectedByteRate)
			throw new WaveParseException($"Expected byte rate to be {expectedByteRate} (got {ByteRate}).");
		if (BlockAlign != expectedBlockAlign)
			throw new WaveParseException($"Expected block align to be {expectedBlockAlign} (got {BlockAlign}).");

		for (long i = br.BaseStream.Position; i < br.BaseStream.Length - (_dataHeader.Length + sizeof(int)); i += 4)
		{
			string dataHeader = Encoding.Default.GetString(br.ReadBytes(4));
			if (dataHeader == _dataHeader)
			{
				DataSize = br.ReadInt32();
				Data = br.ReadBytes(DataSize);

				SampleCount = DataSize / (BitsPerSample / 8) / Channels;
				LengthInSeconds = SampleCount / (double)SampleRate;
				return;
			}
		}

		throw new WaveParseException($"Could not find '{_dataHeader}' header.");
	}

	public short Channels { get; }
	public int SampleRate { get; }
	public int ByteRate { get; }
	public short BlockAlign { get; }
	public short BitsPerSample { get; }
	public int DataSize { get; }
	public byte[] Data { get; }

	public int SampleCount { get; }
	public double LengthInSeconds { get; }
}
