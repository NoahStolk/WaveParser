using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoahStolk.WaveParser.Tests;

[TestClass]
public class ParseTests
{
	private static string GetPath(string fileName) => Path.Combine("Resources", fileName);

	[DataTestMethod]
	[DataRow("Sample1.wav", (short)1, 11000, 11000, (short)1, (short)8, 2208)]
	public void ParseWaveBytes(string fileName, short expectedChannels, int expectedSampleRate, int expectedByteRate, short expectedBlockAlign, short expectedBitsPerSample, int expectedDataSize)
	{
		byte[] bytes = File.ReadAllBytes(GetPath(fileName));
		WaveData wave = new(bytes);

		Assert.AreEqual(expectedChannels, wave.Channels);
		Assert.AreEqual(expectedSampleRate, wave.SampleRate);
		Assert.AreEqual(expectedByteRate, wave.ByteRate);
		Assert.AreEqual(expectedBlockAlign, wave.BlockAlign);
		Assert.AreEqual(expectedBitsPerSample, wave.BitsPerSample);
		Assert.AreEqual(expectedDataSize, wave.DataSize);
	}

	[DataTestMethod]
	[DataRow("Sample1.wav", (short)1, 11000, 11000, (short)1, (short)8, 2208)]
	public void ParseWaveStream(string fileName, short expectedChannels, int expectedSampleRate, int expectedByteRate, short expectedBlockAlign, short expectedBitsPerSample, int expectedDataSize)
	{
		using FileStream fs = new(GetPath(fileName), FileMode.Open);
		WaveData wave = new(fs);

		Assert.AreEqual(expectedChannels, wave.Channels);
		Assert.AreEqual(expectedSampleRate, wave.SampleRate);
		Assert.AreEqual(expectedByteRate, wave.ByteRate);
		Assert.AreEqual(expectedBlockAlign, wave.BlockAlign);
		Assert.AreEqual(expectedBitsPerSample, wave.BitsPerSample);
		Assert.AreEqual(expectedDataSize, wave.DataSize);
	}

	[DataTestMethod]
	[DataRow("Sample1.wav", (short)1, 11000, 11000, (short)1, (short)8, 2208)]
	public void ParseWaveFile(string fileName, short expectedChannels, int expectedSampleRate, int expectedByteRate, short expectedBlockAlign, short expectedBitsPerSample, int expectedDataSize)
	{
		WaveData wave = new(GetPath(fileName));

		Assert.AreEqual(expectedChannels, wave.Channels);
		Assert.AreEqual(expectedSampleRate, wave.SampleRate);
		Assert.AreEqual(expectedByteRate, wave.ByteRate);
		Assert.AreEqual(expectedBlockAlign, wave.BlockAlign);
		Assert.AreEqual(expectedBitsPerSample, wave.BitsPerSample);
		Assert.AreEqual(expectedDataSize, wave.DataSize);
	}
}
