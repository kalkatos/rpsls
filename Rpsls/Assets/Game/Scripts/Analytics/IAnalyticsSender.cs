// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Kalkatos.Analytics
{
    public interface IAnalyticsSender
	{
		void SendEvent (string name);
		void SendEventWithString (string name, string str);
		void SendEventWithNumber (string name, float value);
	}
}