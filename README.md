TeamCity BuildMonitor
===================

A simple build monitor for TeamCity using ASP.NET MVC Core with the following features:

- Build configuration name
- Active branch
- Triggered-by user name
- Running build completion percentage
- Queued builds
- Automatic refresh with a 15 seconds interval
- Groups (shown as backend, frontend and tests in the screenshot below)
- Displays all build configurations automatically (default)
- Can be customized to display custom groups and build configurations

![](https://raw.githubusercontent.com/JohanGl/TeamCity_BuildMonitor/master/BuildMonitor.png)

----------

Installation
-------------

Download the repository and compile it on order to download all required NuGet packages. If you dont have automatic NuGet package restore enabled in Visual Studio then it will have to be enabled.

Open appsettings.json and enter your TeamCity server information into the appSettings labeled userName, password and serverUrl.

In the IOC configuration of the application (Startup.cs), you can switch between using DefaultBuildMonitorModelHandler (shows all jobs in TeamCity automatically) or the CustomBuildMonitorModelHandler which allows you to customize what to display. You can customize your personal view by editing the file buildsettings.json.