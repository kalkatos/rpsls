namespace Kalkatos.Network.Model
{
	public struct NetworkError
	{
		public NetworkErrorTag Tag;
		public string Message;
	}

	public enum NetworkErrorTag
	{
		Undefined,
		NotConnected,
		WrongParameters,
		NotFound,
	}

	public struct NetworkSuccess
	{
		public NetworkSuccessTag Tag;
		public string Message;
	}

	public enum NetworkSuccessTag
	{
		Undefined,
		NewPlayer,
		FoundCredentials,
	}
}