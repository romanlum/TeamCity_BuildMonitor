using System.Collections.Generic;
using System.Xml.Serialization;

namespace BuildMonitorCore.Models.Home.Settings
{
	public class Group
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		public List<Job> Jobs { get; set; }
	}
}