using System.Collections.Generic;

namespace BuildMonitor.Models.Home
{
	public class BuildMonitorViewModel
	{
		public List<Project> Projects { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildMonitorViewModel"/> class.
        /// </summary>
        public BuildMonitorViewModel()
		{
			Projects = new List<Project>();
		}
	}
}