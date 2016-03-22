using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using BuildMonitor.Helpers;
using BuildMonitor.Models.Home;

namespace BuildMonitor.Controllers
{
	public class HomeController : Controller
	{
		private readonly IBuildMonitorModelHandler modelHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        public HomeController()
		{
            modelHandler = new DefaultBuildMonitorModelHandler();
            //modelHandler = new CustomBuildMonitorModelHandler();

            RequestHelper.Username = ConfigurationManager.AppSettings["teamcity_username"];
			RequestHelper.Password = ConfigurationManager.AppSettings["teamcity_password"];
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
					Content = RenderPartialViewToString("_BuildItem", build)
				});
			}

			return Json(result, JsonRequestBehavior.AllowGet);
		}

        /// <summary>
        /// Renders the partial view to string.
        /// </summary>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        protected string RenderPartialViewToString(string viewName, object model)
		{
			if (string.IsNullOrEmpty(viewName))
			{
				viewName = ControllerContext.RouteData.GetRequiredString("action");
			}

			ViewData.Model = model;

			using (var stringWriter = new StringWriter())
			{
				ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
				ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, stringWriter);
				viewResult.View.Render(viewContext, stringWriter);

				return stringWriter.GetStringBuilder().ToString();
			}
		}
	}
}