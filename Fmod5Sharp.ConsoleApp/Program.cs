using Fmod5Sharp.FmodTypes;
using Fmod5Sharp.Util;

namespace Fmod5Sharp.ConsoleApp
{
	internal class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("This program takes exactly one argument: the path to an fsb file.");
				return;
			}

			string path = args[0];
			
			if (!File.Exists(path))
			{
				Console.WriteLine($"No file exists at {path}");
				return;
			}

			byte[] rawData = File.ReadAllBytes(path);
			FmodSoundBank fsbData = FsbLoader.LoadFsbFromByteArray(rawData);
			FmodAudioType audioType = fsbData.Header.AudioType;
			if (audioType.IsSupported() && fsbData.Samples.Single().RebuildAsStandardFileFormat(out var decodedData, out var fileExtension))
			{
				File.WriteAllBytes("output." + fileExtension, decodedData);
				Console.WriteLine("Done!");
				return;
			}
			Console.WriteLine($"Failed to convert audio ({audioType})");
		}
	}
}