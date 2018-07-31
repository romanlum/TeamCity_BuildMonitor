using System;
using BuildMonitor.Models.Home;
using BuildMonitorCore.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BuildMonitorCore.Helpers
{
	public class DefaultBuildMonitorModelHandler : BuildMonitorModelHandlerBase
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBuildMonitorModelHandler"/> class.
        /// </summary>
        /// <param name="teamcityConfig"></param>
        public DefaultBuildMonitorModelHandler(IOptions<TeamCityConfiguration> teamcityConfig) : base(teamcityConfig)
	    {
	    }

	    /// <summary>
        /// Gets the model.
        /// </summary>
        /// <returns></returns>
        public override BuildMonitorViewModel GetModel()
		{
			var model = new BuildMonitorViewModel();

			GetTeamCityBuildsJson();

			var count = (int)ProjectsJson.count;
			for (int i = 0; i < count; i++)
			{
				var project = new Project();
				var projectJson = ProjectsJson.project[i];

				project.Id = projectJson.id;
				project.Name = projectJson.name;
				AddBuilds(ref project);

				model.Projects.Add(project);
			}

			return model;
		}

        /// <summary>
        /// Adds the builds.
        /// </summary>
        /// <param name="project">The project.</param>
        private void AddBuilds(ref Project project)
		{
			var count = (int)BuildTypesJson.count;
			for (int i = 0; i < count; i++)
			{
				var buildTypeJson = BuildTypesJson.buildType[i];
				if (buildTypeJson.projectId != project.Id)
				{
					continue;
				}

                var build = new Build()
                {
                    Id = buildTypeJson.id,
                    Name = buildTypeJson.name,
                    Description = buildTypeJson.description,
                    ProjectName = buildTypeJson.projectName
                };

                var url = string.Format(TeamCityConfig.BuildStatusUrl, build.Id);
				var buildStatusJsonString = RequestHelper.GetJson(url);
				BuildStatusJson = JsonConvert.DeserializeObject<dynamic>(buildStatusJsonString ?? string.Empty);

                build.Branch = (BuildStatusJson != null)
                    ? (BuildStatusJson.branchName ?? "default")
                    : "unknown";

                build.Status = GetBuildStatusForRunningBuild(build.Id);
				if (build.Status == BuildStatus.Running)
				{
					UpdateBuildStatusFromRunningBuildJson(build.Id);
				}

                build.Number = (string)BuildStatusJson.number;
                build.UpdatedBy = GetUpdatedBy();
				build.LastRunText = GetLastRunText();
				build.IsQueued = IsBuildQueued(build.Id);
				build.StatusDescription = (string)BuildStatusJson.statusText;

				if (build.Status == BuildStatus.Running)
				{
					var result = GetRunningBuildBranchAndProgress(build.Id);
					build.Branch = result[0];
					build.Progress = result[1];
				}
				else
				{
					build.Progress = string.Empty;
				}

				project.Builds.Add(build);
			}
		}

        /// <summary>
        /// Determines whether [is build queued] [the specified build identifier].
        /// </summary>
        /// <param name="buildId">The build identifier.</param>
        /// <returns></returns>
        private bool IsBuildQueued(string buildId)
		{
			try
			{
				var count = (int)BuildQueueJson.count;
				for (int i = 0; i < count; i++)
				{
					var build = BuildQueueJson.build[i];

					if (buildId == (string)build.buildTypeId && (string)build.state == "queued")
					{
						return true;
					}
				}
			}
		    catch (Exception)
		    {
		        // ignored
		    }

		    return false;
		}

        /// <summary>
        /// Gets the updated by.
        /// </summary>
        /// <returns></returns>
        private string GetUpdatedBy()
		{
			try
			{
				var triggerType = (string)BuildStatusJson.triggered.type;
                if (triggerType == "user")
				{
					return (string)BuildStatusJson.triggered.user.name;
				}

				if (triggerType == "vcs" && BuildStatusJson.lastChanges != null)
				{
					var result = RequestHelper.GetJson(TeamCityConfig.TeamCityUrl + BuildStatusJson.lastChanges.change[0].href);
					var change = JsonConvert.DeserializeObject<dynamic>(result);

					return (string)change.user.name;
				}

				if (triggerType == "unknown")
				{
					return "TeamCity";
				}
			}
			catch
			{
			}

			return "Unknown";
		}
	}
}