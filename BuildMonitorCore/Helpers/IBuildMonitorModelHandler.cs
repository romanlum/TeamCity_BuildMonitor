using BuildMonitorCore.Models.Home;

namespace BuildMonitorCore.Helpers
{
	public interface IBuildMonitorModelHandler
	{
		BuildMonitorViewModel GetModel();
	}
}