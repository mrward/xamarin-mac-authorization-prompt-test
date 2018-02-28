//
// Program.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2018 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Diagnostics;
using System.IO;
using Mono.Unix.Native;

namespace RunAsRootConsoleApp
{
	class MainClass
	{
		public static int Main (string[] args)
		{
			try {
				return Run (args);
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return -2;
			}
		}

		static int Run (string[] args)
		{
			int uid = (int)Syscall.getuid ();
			Console.WriteLine ("UserId: " + uid);

			string directory = Path.GetDirectoryName (typeof (MainClass).Assembly.Location);
			string fileName = Path.Combine (directory, "result.txt");

			var info = new ProcessStartInfo ("whoami") {
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			Process p = Process.Start (info);
			string whoami = p.StandardOutput.ReadToEnd ();
			p.WaitForExit ();
			Console.WriteLine ("whoami: " + whoami);

			// Change user id to 0.
			int result = Syscall.setuid (0);

			File.WriteAllText (fileName, string.Format ("UserId: {0}\nsetuid: {1}\nwhoami: {2}", uid, result, whoami));

			return uid;
		}
	}
}
