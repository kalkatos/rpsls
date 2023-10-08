using System.Collections.Generic;
using System.Linq;

namespace Kalkatos.Network.Model
{
	public class ActionInfo
	{
		public Dictionary<string, string> PublicChanges;
		public Dictionary<string, string> PrivateChanges;

		public ActionInfo()
		{
			PublicChanges = new Dictionary<string, string>();
			PrivateChanges = new Dictionary<string, string>();
		}

		public ActionInfo Clone ()
		{
			ActionInfo result = new ActionInfo();
			foreach (var item in PublicChanges)
				result.PublicChanges.Add(item.Key, item.Value);
			foreach (var item in PrivateChanges)
				result.PrivateChanges.Add(item.Key, item.Value);
			return result;
		}

		public bool HasAnyPublicChange ()
		{
			return PublicChanges != null && PublicChanges.Count > 0;
		}

		public bool HasAnyPrivateChange ()
		{
			return PrivateChanges != null && PrivateChanges.Count > 0;
		}

		public bool HasPublicChange (string key)
		{
			return PublicChanges != null && PublicChanges.ContainsKey(key);
		}

		public bool IsPublicChangeEqualsIfPresent (string key, params string[] values)
		{
			return !HasPublicChange(key) || (values != null && values.Contains(PublicChanges[key]));
		}

		public bool HasPrivateChange (string key)
		{
			return PrivateChanges != null && PrivateChanges.ContainsKey(key);
		}

		public bool IsPrivateChangeEqualsIfPresent (string key, params string[] values)
		{
			return !HasPublicChange(key) || (values != null && values.Contains(PrivateChanges[key]));
		}

		public bool OnlyHasThesePublicChanges (params string[] keys)
		{
			return PublicChanges == null || PublicChanges.Keys.All(key => keys.Contains(key));
		}

		public bool OnlyHasThesePrivateChanges (params string[] keys)
		{
			return PrivateChanges == null || PrivateChanges.Keys.All(key => keys.Contains(key));
		}
	}
}
