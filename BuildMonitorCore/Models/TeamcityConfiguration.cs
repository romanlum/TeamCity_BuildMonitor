using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildMonitorCore.Models
{

    /// <summary>
    /// TeamCity configuration data
    /// </summary>
    public class TeamCityConfiguration
    {
        public string TeamCityUrl { get; set; }
        public string ProjectsUrl { get; set; }
        public string BuildTypesUrl { get; set; }
        public string RunningBuildsUrl { get; set; }
        public string BuildStatusUrl { get; set; }
        public string BuildStatisticsUrl { get; set; }
        public string BuildQueueUrl { get; set; }
    }
}
