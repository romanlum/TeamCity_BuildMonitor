namespace BuildMonitorCore.Models.Home
{
	public class Build
	{
		public string Id { get; set; }
		public string Name { get; set; }
        public string Description { get; set; }
        public string ProjectName { get; set; }
        public string Number { get; set; }
        public BuildStatus Status { get; set; }
        public string StatusText { get; set; }
        public string Branch { get; set; }
		public string Progress { get; set; }
		public string UpdatedBy { get; set; }
		public string LastRunText { get; set; }
		public bool IsQueued { get; set; }
		public string StatusDescription { get; set; }

        // Build Statistics
        public int BuildDuration { get; set; }
        public double CodeCoverageC { get; set; }
        public double CodeCoverageM { get; set; }
        public double CodeCoverageS { get; set; }
        public int IgnoredTestCount { get; set; }
        public int PassedTestCount { get; set; }
        public int TotalTestCount { get; set; }

        /// <summary>
        /// Gets the status text.
        /// </summary>
        /// <value>
        /// The status text.
        /// </value>
        public string StatusStateText
		{
			get
			{
				switch (Status)
				{
					case BuildStatus.Success:
						return "OK";

					case BuildStatus.Failure:
						return "FAILED";

					case BuildStatus.Running:
						return "RUNNING";

					case BuildStatus.Error:
						return "ERROR";

					default:
						return "UNKNOWN";
				}
			}
		}
	}
}