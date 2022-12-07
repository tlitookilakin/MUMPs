using AeroCore;
using System;
using System.Collections.Generic;

namespace MUMPs.Integration
{
	public interface CustomOreNodesAPI
	{
		public int GetCustomOreNodeIndex(string id);
		public List<object> GetCustomOreNodes();
		public List<string> GetCustomOreNodeIDs();
	}

	[ModInit(WhenHasMod = "aedenthorn.CustomOreNodes")]
	internal class CustomOreNodes
	{
		internal static CustomOreNodesAPI API;
		internal static List<string> knownNodes;
		internal static void Init()
		{
			API = ModEntry.helper.ModRegistry.GetApi<CustomOreNodesAPI>("aedenthorn.CustomOreNodes");
			knownNodes = API.GetCustomOreNodeIDs();
		}
	}
}
