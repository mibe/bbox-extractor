/*
 * Part of the bbox-extractor project
 * Copyright (c) 2022 Michael Bemmerl
 *
 * SPDX-License-Identifier: MIT
 */

// ReSharper disable IdentifierTypo
namespace bbox_extractor
{
	using System;
	using System.Drawing;
	using System.IO;
	using CommunityToolkit.Diagnostics;

	class Extractor
	{
		/// <summary>
		/// The stream of the GeoJSONL source file.
		/// </summary>
		private readonly FileStream stream;

		private readonly IFormatProvider formatProvider;
		private readonly ILogger logger;

		/// <summary>
		/// Number of processed features to trigger a status log.
		/// </summary>
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

		/// <summary>
		/// Extracts all features in the source file which are inside the bounding box specified in <paramref name="bbox"/>.
		/// All features inside that bounding box will be written to the file in <paramref name="outFile"/>.
		/// </summary>
		/// <param name="bbox">Bounding box</param>
		/// <param name="outFile">Output file</param>
		internal void Extract(RectangleF bbox, FileInfo outFile)
		{
			Guard.IsNotNull(outFile);

			StreamWriter writer = new StreamWriter(outFile.FullName);
			Guard.CanWrite(writer.BaseStream);
			writer.Write("{\"type\": \"FeatureCollection\", \"features\": [");

			StreamReader reader = new StreamReader(this.stream);
			ulong counter = 0;
			bool first = true;

			// Repeat for every line in the file
			while (reader.ReadLine() is { } feature)
			{
				bool inside = processFeature(feature, ref bbox);
				counter++;

				// If the feature was inside the bounding box, write it to the output file.
				if (inside)
				{
					if (!first)
						writer.Write(",");
					else
						first = false;

					writer.Write(feature);

					this.logger.Log("Found a polygon.");
				}

				// Check if a status log is due.
				if (counter % StatsTrigger == 0)
				{
					float percent = this.stream.Position / (float)this.stream.Length;
					this.logger.Log($"Processed {counter:N0} features, {percent:P1} done.");
				}
			}

			writer.Write("]}");
			writer.Close();
			this.stream.Position = 0;
		}

		private bool processFeature(string json, ref RectangleF bbox)
		{
			// start at position 86 and ignore that "{"type": "Feature", "properties": {}..." JSON.
			int idx = 86;

			do
			{
				// Search the position of the comma and closing bracket.
				int commaIdx = json.IndexOf(',', idx);
				int endIdx = json.IndexOf(']', idx);

				if (commaIdx == -1)
					break;

				// Extract the longitude and latitude string using the comma and bracket positions.
				string lonString = json.Substring(idx, commaIdx - idx);
				string latString = json.Substring(commaIdx + 2, endIdx - commaIdx-2);

				float lon = float.Parse(lonString, this.formatProvider);
				float lat = float.Parse(latString, this.formatProvider);
				
				if (bbox.Contains(lon, lat))
					return true;

				// Set index to next feature, so jump over some JSON element closing chars.
				idx = endIdx + 4;

			} while (idx < json.Length);

			return false;
		}
	}
}
