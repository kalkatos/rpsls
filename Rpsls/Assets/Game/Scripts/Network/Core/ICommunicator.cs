// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using System;

namespace Kalkatos.Network
{
	/// <summary>
	/// Interface for sending web requests according to each platform.
	/// </summary>
	public interface ICommunicator
	{
		void Get (string url, Action<string> callback);
		void Post (string url, string message, Action<string> callback);
	}
}