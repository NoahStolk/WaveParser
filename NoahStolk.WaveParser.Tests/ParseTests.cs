using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoahStolk.WaveParser.Tests;

[TestClass]
public class ParseTests
{
	private static string GetPath(string fileName) => Path.Combine("Resources", fileName);

	[DataTestMethod]
	[DataRow("Sample1.wav", (short)1, 11000, 11000, (short)1, (short)8, 2208, 2208, 0.2)]
	[DataRow("Sample2.wav", (short)1, 11025, 22050, (short)2, (short)16, 61544, 30772, 2.791)]
	public void ParseWaveBytes(string fileName, short expectedChannels, int expectedSampleRate, int expectedByteRate, short expectedBlockAlign, short expectedBitsPerSample, int expectedDataSize, int expectedSampleCount, double expectedLengthInSeconds)
	{
		byte[] bytes = File.ReadAllBytes(GetPath(fileName));
		WaveData wave = new(bytes);

		Assert.AreEqual(expectedChannels, wave.Channels);
		Assert.AreEqual(expectedSampleRate, wave.SampleRate);
		Assert.AreEqual(expectedByteRate, wave.ByteRate);
		Assert.AreEqual(expectedBlockAlign, wave.BlockAlign);
		Assert.AreEqual(expectedBitsPerSample, wave.BitsPerSample);
		Assert.AreEqual(expectedDataSize, wave.DataSize);

		Assert.AreEqual(expectedSampleCount, wave.SampleCount);
		Assert.AreEqual(expectedLengthInSeconds, wave.LengthInSeconds, 0.01);
	}
}
