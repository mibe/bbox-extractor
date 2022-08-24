/*
 * Part of the bbox-extractor project
 * Copyright (c) 2022 Michael Bemmerl
 *
 * SPDX-License-Identifier: MIT
 */

namespace bbox_extractor
{
	using System;

	class ConsoleLogger : ILogger
	{
		public void Log(string message)
		{
			Console.Write(DateTime.Now);
			Console.Write(": ");
			Console.WriteLine(message);
		}
	}
}
