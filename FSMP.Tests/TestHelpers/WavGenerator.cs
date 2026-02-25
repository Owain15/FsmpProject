namespace FSMP.Tests.TestHelpers;

/// <summary>
/// Generates valid WAV files for integration testing without external file dependencies.
/// </summary>
public static class WavGenerator
{
    /// <summary>
    /// Creates a valid WAV file with a low-amplitude sine wave.
    /// </summary>
    /// <param name="path">Output file path.</param>
    /// <param name="durationSeconds">Duration in seconds (default 0.5).</param>
    /// <param name="frequency">Sine wave frequency in Hz (default 440).</param>
    public static void CreateTestWav(string path, double durationSeconds = 0.5, int frequency = 440)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        int sampleRate = 44100;
        short channels = 1;
        short bitsPerSample = 16;
        int numSamples = (int)(sampleRate * durationSeconds);
        int dataSize = numSamples * channels * (bitsPerSample / 8);

        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8);

        // fmt chunk
        writer.Write("fmt "u8);
        writer.Write(16); // chunk size
        writer.Write((short)1); // PCM format
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitsPerSample / 8); // byte rate
        writer.Write((short)(channels * bitsPerSample / 8)); // block align
        writer.Write(bitsPerSample);

        // data chunk
        writer.Write("data"u8);
        writer.Write(dataSize);

        // Write sine wave (low amplitude to avoid loud test output)
        for (int i = 0; i < numSamples; i++)
        {
            short sample = (short)(Math.Sin(2.0 * Math.PI * frequency * i / sampleRate) * 1000);
            writer.Write(sample);
        }
    }
}
