using System.Diagnostics;
using System.IO;
using System.Linq;
using BuildMonitor.Models.Home;
using BuildMonitorCore.Helpers;
using BuildMonitorCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;

namespace BuildMonitorCore.Controllers
{
	public class HomeController : Controller
	{
		private readonly IBuildMonitorModelHandler modelHandler;
        private readonly IOptions<TeamCityConfiguration> teamcityConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        public HomeController(IOptions<TeamCityConfiguration> teamcityConfig, IBuildMonitorModelHandler handler)
        {
            modelHandler = handler;
            //modelHandler = new CustomBuildMonitorModelHandler();

		    RequestHelper.Username = teamcityConfig.Value.UserName;
		    RequestHelper.Password = teamcityConfig.Value.Password;
            this.teamcityConfig = teamcityConfig;
        }

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
		{
			var model = modelHandler.GetModel();
			return View(model);
		}

        /// <summary>
        /// Gets the builds.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetBuilds()
		{
			var model = modelHandler.GetModel();

			var builds = model.Projects.SelectMany(p => p.Builds).ToList();

			var result = new BuildsJson();

			foreach (var build in builds)
			{
				result.Builds.Add(new BuildJson()
				{
					Id = build.Id,
				//	Content = RenderPartialViewToString("_BuildItem", build)
				});
			}

		    return Json(result);
		}

	    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	    public IActionResult Error()
	    {
	        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	    }

        //      /// <summary>
        //      /// Renders the partial view to string.
        //      /// </summary>
        //      /// <param name="viewName">Name of the view.</param>
        //      /// <param name="model">The model.</param>
        //      /// <returns></returns>
        //      protected string RenderPartialViewToString(string viewName, object model)
        //{
        //	if (string.IsNullOrEmpty(viewName))
        //	{
        //		viewName = ControllerContext.RouteData.GetRequiredString("action");
        //	}

        //	ViewData.Model = model;

        //	using (var stringWriter = new StringWriter())
        //	{
        //		ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
        //		ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, stringWriter);
        //		viewResult.View.Render(viewContext, stringWriter);

        //		return stringWriter.GetStringBuilder().ToString();
        //	}
        //}
    }
}