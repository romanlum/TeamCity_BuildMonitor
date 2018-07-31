using System;
using System.Collections.Generic;
using System.Globalization;
using BuildMonitor.Models.Home;
using BuildMonitorCore.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BuildMonitorCore.Helpers
{
	public abstract class BuildMonitorModelHandlerBase : IBuildMonitorModelHandler
	{
		protected TeamCityConfiguration TeamCityConfig { get; private set; }
	    protected Dictionary<string, dynamic> RunningBuilds { get; set; }

	    protected dynamic ProjectsJson { get; set; }
	    protected dynamic BuildTypesJson { get; set; }
	    protected dynamic BuildQueueJson { get; set; }
	    protected dynamic BuildStatusJson { get; set; }
	    protected dynamic BuildStatisticsJson { get; }

	    /// <summary>
        /// Initializes a new instance of the <see cref="BuildMonitorModelHandlerBase"/> class.
        /// </summary>
        protected BuildMonitorModelHandlerBase(IOptions<TeamCityConfiguration> teamcityConfig)
        {
            TeamCityConfig = teamcityConfig.Value;
        }

        /// <summary>
        /// Gets the team city builds json.
        /// </summary>
        protected void GetTeamCityBuildsJson()
		{
			var projectsJsonString = RequestHelper.GetJson(TeamCityConfig.ProjectsUrl);
			ProjectsJson = JsonConvert.DeserializeObject<dynamic>(projectsJsonString);

			var buildTypesJsonString = RequestHelper.GetJson(TeamCityConfig.BuildTypesUrl);
			BuildTypesJson = JsonConvert.DeserializeObject<dynamic>(buildTypesJsonString);

			var buildQueueJsonString = RequestHelper.GetJson(TeamCityConfig.BuildQueueUrl);
			BuildQueueJson = buildQueueJsonString != null ? JsonConvert.DeserializeObject<dynamic>(buildQueueJsonString) : null;

			UpdateRunningBuilds();
		}

        /// <summary>
        /// Updates the running builds.
        /// </summary>
        private void UpdateRunningBuilds()
		{
			try
			{
				RunningBuilds = new Dictionary<string, dynamic>();

				var runningBuildsJsonString = RequestHelper.GetJson(TeamCityConfig.RunningBuildsUrl);
				var runningBuildsJson = runningBuildsJsonString != null ? JsonConvert.DeserializeObject<dynamic>(runningBuildsJsonString) : null;

				var count = (int)runningBuildsJson.count;
				for (int i = 0; i < count; i++)
				{
					var buildJson = runningBuildsJson.build[i];

					var buildId = (string)buildJson.buildTypeId;
					var url = TeamCityConfig.TeamCityUrl + (string)buildJson.href;

					var buildStatusJsonString = RequestHelper.GetJson(url);
					var buildStatusJson = JsonConvert.DeserializeObject<dynamic>(buildStatusJsonString ?? string.Empty);

					RunningBuilds.Add(buildId, buildStatusJson);
				}
			}
			catch
			{
			}
		}

        /// <summary>
        /// Updates the build status from running build json.
        /// </summary>
        /// <param name="buildId">The build identifier.</param>
        protected void UpdateBuildStatusFromRunningBuildJson(string buildId)
		{
			BuildStatusJson = RunningBuilds[buildId];
		}

        /// <summary>
        /// Gets the build status for running build.
        /// </summary>
        /// <param name="buildId">The build identifier.</param>
        /// <returns></returns>
        protected BuildStatus GetBuildStatusForRunningBuild(string buildId)
		{
			if (RunningBuilds.ContainsKey(buildId))
			{
				return BuildStatus.Running;
			}

			if (BuildStatusJson == null)
			{
				return BuildStatus.None;
			}

			switch ((string)BuildStatusJson.status)
			{
				case "SUCCESS":
					return BuildStatus.Success;

				case "FAILURE":
					return BuildStatus.Failure;

				case "ERROR":
					return BuildStatus.Error;

				default:
					return BuildStatus.None;
			}
		}

        /// <summary>
        /// Gets the running build branch and progress.
        /// </summary>
        /// <param name="buildId">The build identifier.</param>
        /// <returns></returns>
        protected string[] GetRunningBuildBranchAndProgress(string buildId)
		{
			var result = new[]
            {
                string.Empty,
                string.Empty
            };

			try
			{
				result[0] = (string)RunningBuilds[buildId].branchName ?? "default";

				var percentage = (string)RunningBuilds[buildId].percentageComplete;
				result[1] = !string.IsNullOrWhiteSpace(percentage) ? percentage + "%" : "0%";
			}
			catch
			{
			}

			return result;
		}

		public abstract BuildMonitorViewModel GetModel();

        /// <summary>
        /// Gets the last run text.
        /// </summary>
        /// <returns></returns>
        protected string GetLastRunText()
		{
			const int second = 1;
			const int minute = 60 * second;
			const int hour = 60 * minute;
			const int day = 24 * hour;
			const int month = 30 * day;

			try
			{
				var dateTime = DateTime.ParseExact((string)BuildStatusJson.startDate, "yyyyMMdd'T'HHmmsszzz", CultureInfo.InvariantCulture);

				var timeSpan = new TimeSpan(DateTime.Now.Ticks - dateTime.Ticks);
				double delta = Math.Abs(timeSpan.TotalSeconds);

				if (delta < 1 * minute)
				{
					return timeSpan.Seconds == 1 ? "one second ago" : timeSpan.Seconds + " seconds ago";
				}
				if (delta < 2 * minute)
				{
					return "a minute ago";
				}
				if (delta < 45 * minute)
				{
					return timeSpan.Minutes + " minutes ago";
				}
				if (delta < 90 * minute)
				{
					return "an hour ago";
				}
				if (delta < 24 * hour)
				{
					return timeSpan.Hours + " hours ago";
				}
				if (delta < 48 * hour)
				{
					return "yesterday";
				}
				if (delta < 30 * day)
				{
					return timeSpan.Days + " days ago";
				}

				if (delta < 12 * month)
				{
					int months = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 30));
					return months <= 1 ? "one month ago" : months + " months ago";
				}
				else
				{
					int years = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 365));
					return years <= 1 ? "one year ago" : years + " years ago";
				}
			}
			catch
			{
				return string.Empty;
			}
		}
	}
}