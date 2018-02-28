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
using System.IO;
using Mono.Unix.Native;
using FIXED_Security;

namespace Xamarin.Mac.AuthPromptTest
{
	class MainClass
	{
		public static int Main (string[] args)
		{
			try {
				string fileName = typeof (MainClass).Assembly.Location;
				fileName = Path.GetFullPath (typeof (MainClass).Assembly.Location);
				return Run (args);
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return -2;
			}
		}

		static int Run (string[] args)
		{
			var flags = AuthorizationFlags.ExtendRights |
				AuthorizationFlags.InteractionAllowed |
				AuthorizationFlags.PreAuthorize;

			var directory = Path.GetFullPath (Path.GetDirectoryName (typeof (MainClass).Assembly.Location));

			var parameters = new AuthorizationParameters ();
			parameters.Prompt = "Test";
			parameters.PathToSystemPrivilegeTool = "";

			using (var auth = Authorization.Create (parameters, null, flags)) {

				string command = "/Library/Frameworks/Mono.framework/Versions/Current/Commands/mono";
				string fileName = Path.Combine (directory, "RunAsRootConsoleApp.exe");

				var arguments = new [] { fileName };
				int result = auth.ExecuteWithPrivileges (command, AuthorizationFlags.Defaults, arguments);
				if (result != 0) {
					if (Enum.TryParse (result.ToString (), out AuthorizationStatus authStatus)) {
						throw new InvalidOperationException ($"Could not get authorization. {authStatus}");
					}
					throw new InvalidOperationException ($"Could not get authorization. {result}");
				}

				int status;
				if (Syscall.wait (out status) == -1) {
					throw new InvalidOperationException ("Failed to start child process.");
				}

				if (!Syscall.WIFEXITED (status)) {
					throw new InvalidOperationException ("Child process terminated abnormally.");
				}

				int exitCode = Syscall.WEXITSTATUS (status);
				return exitCode;
			}
		}
	}
}
