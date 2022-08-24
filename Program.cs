/*
 * Part of the bbox-extractor project
 * Copyright (c) 2022 Michael Bemmerl
 *
 * SPDX-License-Identifier: MIT
 */

namespace bbox_extractor
{
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.IO;

	class Program
	{
		const string Version = "1.0";

		static void Main(string[] args)
		{
			if (!parseArgs(args, out FileInfo infile, out RectangleF bbox, out FileInfo outfile))
				return;

			if (outfile == null)
			{
				string path = Path.Combine(infile.DirectoryName, Path.GetFileNameWithoutExtension(infile.Name) + "-extracted.geojson");
				outfile = new FileInfo(path);
			}

			ILogger log = new ConsoleLogger();
			Extractor ex = new Extractor(infile, log);
			ex.Extract(bbox, outfile);
			
			log.Log("Done.");
		}

		static bool parseArgs(string[] args, out FileInfo infile, out RectangleF bbox, out FileInfo outfile)
		{
			infile = null;
			outfile = null;
			bbox = RectangleF.Empty;

			if (args.Length < 5 || args.Length > 6)
			{
				printUsage("Wrong number of arguments.");
				return false;
			}

			infile = new FileInfo(args[0]);

			if (!infile.Exists)
			{
				printUsage("Input file not found.");
				return false;
			}

			// 8.7853 49.0499 8.8199 49.0749
			IFormatProvider ifp = CultureInfo.InvariantCulture;
			NumberStyles ns = NumberStyles.Float | NumberStyles.AllowDecimalPoint;

			if (!float.TryParse(args[1], ns, ifp, out float lon1) || !float.TryParse(args[2], ns, ifp, out float lat1) ||
			    !float.TryParse(args[3], ns, ifp, out float lon2) || !float.TryParse(args[4], ns, ifp, out float lat2))
			{
				printUsage("Bounding box coordinates were not parseable.");
				return false;
			}

			float latMin = Math.Min(lat1, lat2);
			float latMax = Math.Max(lat1, lat2);
			float lonMin = Math.Min(lon1, lon2);
			float lonMax = Math.Max(lon1, lon2);

			bbox = new RectangleF(lonMin, latMin, lonMax - lonMin, latMax - latMin);

			if (args.Length > 5)
				outfile = new FileInfo(args[5]);

			return true;
		}

		static void printUsage(string error = null)
		{
			TextWriter tw = Console.Error;

			if (error != null)
			{
				tw.WriteLine($"Error: {error}");
				tw.WriteLine();
			}

			tw.WriteLine($"bbox-extractor version {Version}");
			tw.WriteLine("A tool to extract polygons from Microsoft's GlobalMLBuildingFootprints which are located inside a bounding box.");
			tw.WriteLine("This tool also converts GeoJSONL to GeoJSON.");
			tw.WriteLine();
			tw.WriteLine("This program is using CommunityToolkit (c) .NET Foundation and Contributors; MIT license");
			tw.WriteLine();
			tw.WriteLine("Usage:");
			tw.WriteLine("bbox-extractor Infile MinLon MinLat MaxLon MaxLat [Outfile]");
			tw.WriteLine();
			tw.WriteLine("Infile\t\tPath to source file in GeoJSONL format");
			tw.WriteLine("MinLon\t\tMinimum longitude");
			tw.WriteLine("MinLat\t\tMinimum latitude");
			tw.WriteLine("MaxLon\t\tMaximum longitude");
			tw.WriteLine("MaxLat\t\tMaximum latitude");
			tw.WriteLine("Outfile\t\tPath to file with extracted polygons in GeoJOSN format");
			tw.WriteLine("\t\tIf not specified, the file will be saved in the");
			tw.WriteLine("\t\tsame directory as the Infile.");
			tw.WriteLine();
			tw.WriteLine("The longitude and latitude values specify a bounding box.");
			tw.WriteLine();
			tw.WriteLine("Example:");
			tw.WriteLine(@"bbox-extractor D:\Germany.geojsonl 10.2 50.1 11.9 51.6 D:\extracted.geojson");
			tw.WriteLine();
		}
	}
}
