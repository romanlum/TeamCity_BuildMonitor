using System;
using System.IO;
using System.Net;

namespace BuildMonitorCore.Helpers
{
	public static class RequestHelper
	{
		public static string Username { get; set; }
		public static string Password { get; set; }

        /// <summary>
        /// Gets the json.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string GetJson(string url)
		{
			try
			{
				var request = (HttpWebRequest)WebRequest.Create(url);
				request.Accept = "application/json";
				request.Headers.Add("ts", DateTime.Now.ToFileTime().ToString());
				request.Credentials = new NetworkCredential(Username, Password);

				var response = request.GetResponse().GetResponseStream();
				if (response == null)
				{
					return null;
				}

				var reader = new StreamReader(response);
				return reader.ReadToEnd();
			}
			catch(Exception e)
			{
			    throw e;
			}
		}
	}
}