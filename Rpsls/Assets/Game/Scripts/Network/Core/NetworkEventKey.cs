// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Kalkatos.Network
{
	public enum NetworkEventKey : byte
	{
		Connect = 201,
		FindMatch = 202,
		GetMatch = 203,
		LeaveMatch = 204,
		SetPlayerData = 205,
		SendAction = 206,
		GetMatchState = 207
	}
}
