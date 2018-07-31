using System.Diagnostics;
using System.IO;
using System.Linq;
using BuildMonitor.Models.Home;
using BuildMonitorCore.Helpers;
using BuildMonitorCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;

namespace BuildMonitorCore.Controllers
{
	public class HomeController : Controller
	{
		private readonly IBuildMonitorModelHandler modelHandler;
        private readonly ICompositeViewEngine viewEngine;
        private readonly IOptions<TeamCityConfiguration> teamcityConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        public HomeController(IOptions<TeamCityConfiguration> teamcityConfig, IBuildMonitorModelHandler handler, ICompositeViewEngine viewEngine)
        {
            modelHandler = handler;
            this.viewEngine = viewEngine;
            //modelHandler = new CustomBuildMonitorModelHandler();

            RequestHelper.Username = teamcityConfig.Value.UserName;
		    RequestHelper.Password = teamcityConfig.Value.Password;
            this.teamcityConfig = teamcityConfig;
        }

       
        public ActionResult Index()
		{
			var model = modelHandler.GetModel();
			return View(model);
		}

	    public ActionResult Update()
	    {
	        var model = modelHandler.GetModel();
	        return PartialView("_Projects", model);
	    }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	    public IActionResult Error()
	    {
	        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	    }

        
    }
}