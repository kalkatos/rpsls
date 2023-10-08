using System;
using System.Collections.Generic;
using System.Linq;

namespace Kalkatos.Network.Model
{
	public class StateInfo
	{
		public Dictionary<string, string> PublicProperties;
		public Dictionary<string, string> PrivateProperties;
		public int Hash;

		public StateInfo ()
		{
			PublicProperties = new Dictionary<string, string>();
			PrivateProperties = new Dictionary<string, string>();
		}

		public StateInfo Clone ()
		{
			StateInfo clone = new StateInfo();
			clone.PublicProperties = PublicProperties.ToDictionary(e => e.Key, e => e.Value);
			clone.PrivateProperties = PrivateProperties.ToDictionary(e => e.Key, e => e.Value);
			clone.Hash = Hash;
			return clone;
		}

		public bool HasAnyPublicProperty ()
		{
			return PublicProperties != null && PublicProperties.Count > 0;
		}

		public bool HasAnyPrivateProperty ()
		{
			return PrivateProperties != null && PrivateProperties.Count > 0;
		}

		public bool HasPublicProperty (string key)
		{
			return PublicProperties != null && PublicProperties.ContainsKey(key);
		}

		public bool IsPublicPropertyEqualsIfPresent (string key, params string[] values)
		{
			return !HasPublicProperty(key) || (values != null && values.Contains(PublicProperties[key]));
		}

		public bool HasPrivateProperty (string key)
		{
			return PrivateProperties != null && PrivateProperties.ContainsKey(key);
		}

		public bool IsPrivatePropertyEqualsIfPresent (string key, params string[] values)
		{
			return !HasPublicProperty(key) || (values != null && values.Contains(PrivateProperties[key]));
		}

		public bool OnlyHasThesePublicProperties (params string[] keys)
		{
			return PublicProperties == null || PublicProperties.Keys.All(key => keys.Contains(key));
		}

		public bool OnlyHasThesePrivateProperties (params string[] keys)
		{
			return PrivateProperties == null || PrivateProperties.Keys.All(key => keys.Contains(key));
		}
	}
}
