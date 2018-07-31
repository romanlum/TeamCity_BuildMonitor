using System.Xml.Serialization;

namespace BuildMonitorCore.Models.Home.Settings
{
	public class Job
	{
		[XmlAttribute("id")]
		public string Id { get; set; }

		[XmlAttribute("text")]
		public string Text { get; set; }
	}
}