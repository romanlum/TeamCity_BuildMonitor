using BuildMonitor.Models.Home;
using Newtonsoft.Json;

namespace BuildMonitor.Helpers
{
	public class DefaultBuildMonitorModelHandler : BuildMonitorModelHandlerBase
	{
        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <returns></returns>
        public override BuildMonitorViewModel GetModel()
		{
			var model = new BuildMonitorViewModel();

			GetTeamCityBuildsJson();

			var count = (int)projectsJson.count;
			for (int i = 0; i < count; i++)
			{
				var project = new Project();
				var projectJson = projectsJson.project[i];

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
			var count = (int)buildTypesJson.count;
			for (int i = 0; i < count; i++)
			{
				var buildTypeJson = buildTypesJson.buildType[i];
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

                var url = string.Format(buildStatusUrl, build.Id);
				var buildStatusJsonString = RequestHelper.GetJson(url);
				buildStatusJson = JsonConvert.DeserializeObject<dynamic>(buildStatusJsonString ?? string.Empty);

                build.Branch = (buildStatusJson != null)
                    ? (buildStatusJson.branchName ?? "default")
                    : "unknown";

                build.Status = GetBuildStatusForRunningBuild(build.Id);
				if (build.Status == BuildStatus.Running)
				{
					UpdateBuildStatusFromRunningBuildJson(build.Id);
				}

                build.Number = (string)buildStatusJson.number;
                build.UpdatedBy = GetUpdatedBy();
				build.LastRunText = GetLastRunText();
				build.IsQueued = IsBuildQueued(build.Id);
				build.StatusDescription = (string)buildStatusJson.statusText;

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
				var count = (int)buildQueueJson.count;
				for (int i = 0; i < count; i++)
				{
					var build = buildQueueJson.build[i];

					if (buildId == (string)build.buildTypeId && (string)build.state == "queued")
					{
						return true;
					}
				}
			}
			catch
			{
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
				var triggerType = (string)buildStatusJson.triggered.type;
                if (triggerType == "user")
				{
					return (string)buildStatusJson.triggered.user.name;
				}

				if (triggerType == "vcs" && buildStatusJson.lastChanges != null)
				{
					var result = RequestHelper.GetJson(teamCityUrl + buildStatusJson.lastChanges.change[0].href);
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