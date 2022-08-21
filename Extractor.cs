namespace bbox_extractor
{
	using System;
	using System.Drawing;
	using System.IO;
	using CommunityToolkit.Diagnostics;

	class Extractor
	{
		private readonly FileStream stream;
		private IFormatProvider formatProvider;
		private ILogger logger;
		private const uint StatsTrigger = 100000;

		internal Extractor(FileInfo source, ILogger logger)
		{
			Guard.IsNotNull(source);
			Guard.IsTrue(source.Exists);

			this.stream = source.OpenRead();

			Guard.CanRead(stream);

			this.formatProvider = System.Globalization.CultureInfo.InvariantCulture;
			this.logger = logger;
		}

		internal void Extract(RectangleF bbox, FileInfo outFile)
		{
			Guard.IsNotNull(outFile);

			FileStream outStream = outFile.OpenWrite();
			Guard.CanWrite(outStream);
			StreamWriter writer = new StreamWriter(outStream);

			StreamReader reader = new StreamReader(this.stream);
			ulong counter = 0;

			while (reader.ReadLine() is { } feature)
			{
				bool inside = processFeature(feature, ref bbox);
				counter++;

				if (inside)
				{
					writer.WriteLine(feature);
					this.logger.Log("Found a polygon.");
				}

				if (counter % StatsTrigger == 0)
				{
					float percent = this.stream.Position / (float)this.stream.Length;
					this.logger.Log($"Processed {counter:N0} features, {percent:P1} done.");
				}
			}

			writer.Close();
			this.stream.Position = 0;
		}

		private bool processFeature(string json, ref RectangleF bbox)
		{
			// start at position 86 and ignore that "{"type": "Feature", "properties": {}..." JSON.
			int idx = 86;

			do
			{
				int commaIdx = json.IndexOf(',', idx);
				int endIdx = json.IndexOf(']', idx);

				if (commaIdx == -1)
					break;

				string lonString = json.Substring(idx, commaIdx - idx);
				string latString = json.Substring(commaIdx + 2, endIdx - commaIdx-2);

				float lon = float.Parse(lonString, this.formatProvider);
				float lat = float.Parse(latString, this.formatProvider);
				
				if (bbox.Contains(lon, lat))
					return true;

				idx = endIdx + 4;

			} while (idx < json.Length);

			return false;
		}
	}
}
