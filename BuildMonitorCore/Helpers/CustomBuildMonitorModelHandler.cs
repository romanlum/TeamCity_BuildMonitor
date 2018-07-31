using System;
using System.IO;
using System.Xml.Serialization;
using BuildMonitorCore.Models.Home;
using BuildMonitorCore.Models;
using BuildMonitorCore.Models.Home.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BuildMonitorCore.Helpers
{
	public class CustomBuildMonitorModelHandler : BuildMonitorModelHandlerBase
	{
		private Settings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomBuildMonitorModelHandler"/> class.
        /// </summary>
        public CustomBuildMonitorModelHandler(IOptions<TeamCityConfiguration> config, IOptions<Settings> settings)
        :base(config)
        {
            this.settings = settings.Value;
		}

   

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <returns></returns>
        public override BuildMonitorViewModel GetModel()
		{
			var model = new BuildMonitorViewModel();

			GetTeamCityBuildsJson();

			foreach (var group in settings.Groups)
			{
				var project = new Project();
				project.Name = group.Name;

				AddBuilds(ref project, group);

				model.Projects.Add(project);
			}

			return model;
		}

        /// <summary>
        /// Adds the builds.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="group">The group.</param>
        private void AddBuilds(ref Project project, Group group)
		{
			foreach (var job in group.Jobs)
			{
				var buildTypeJson = GetJsonBuildTypeById(job.Id);

                var build = new Build()
                {
                    Id = buildTypeJson.id,
                    Name = job.Text ?? buildTypeJson.name,
                    Description = buildTypeJson.description,
                    ProjectName = buildTypeJson.projectName
                };

                var url = TeamCityConfig.ServerUrl + string.Format(TeamCityConfig.BuildStatusUrl, build.Id);
				var buildStatusJsonString = RequestHelper.GetJson(url);
				BuildStatusJson = JsonConvert.DeserializeObject<dynamic>(buildStatusJsonString ?? string.Empty);

                build.Branch = (BuildStatusJson != null)
                    ? (BuildStatusJson.branchName ?? "default")
                    : "unknown";

                build.Number = (BuildStatusJson != null)
                    ? (BuildStatusJson.number ?? "default")
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
        /// Gets the json build type by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private dynamic GetJsonBuildTypeById(string id)
		{
			var count = (int)BuildTypesJson.count;
			for (int i = 0; i < count; i++)
			{
				if (BuildTypesJson.buildType[i].id == id)
				{
					return BuildTypesJson.buildType[i];
				}
			}

			return null;
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
				if ((string)BuildStatusJson.triggered.type == "user")
				{
					return (string)BuildStatusJson.triggered.user.name;
				}
				else if ((string)BuildStatusJson.triggered.type == "unknown")
				{
					return "TeamCity";
				}
				else
				{
					return "Unknown";
				}
			}
			catch
			{
				return "Unknown";
			}
		}
	}
}