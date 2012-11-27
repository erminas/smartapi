Für eine deutsche Beschreibung siehe unten.

Welcome to erminas SmartAPI, the free .NET RQL library for OpenText WSM Management Server (RedDot CMS)!
================

erminas SmartAPI is the extension of SmartEdit and SmartTree in regard of development. Where SmartEdit allows you to edit conveniently and SmartTree makes administration in your browser possible, SmartAPI creates a comfortable programming interface for the OpenText WSM Management Server.

The complete library is written in C# / .NET and can be used from within the CMS as plugin or from an external .NET (web) application. As .NET is being used there is no break in the technology you use and the application can be hosted directly in your Windows Server in the IIS. You do not need any other hardware or server installed.

The goal of SmartAPI is to provide a comfortable, fully object-oriented and well-tested library for all developers and users of OpenText WSM. There is no expensive in-house development necessary but you can choose a ready-to-use library that just works.

First steps
---
```csharp
// Check which URL is used for connection to RQL Service.
// This depends on the RedDot CMS / OT WSM MS Version used.
var rqlUri = WebServiceURLProber.RqlUri("http://my-reddot-server/cms");
 
// RedDot Login with (user/password)
var authData = new PasswordAuthentication("user", "password");
 
var login = new ServerLogin {Address = rqlUri, AuthData = authData};
 
// Session is the entry point to interact with the RedDot server.
// Creating the session object automatically creates a connection.
// Dispose() closes the connection in a a clean way (this is done
// automatically at the end of the using block).
using(var session = new Session(login))
{
  // Select a project based on the name
  var project = session.Projects.GetByName("MyProjekt");
 
  // Find all pages based on the Content Class "MyContentClass"
  var pageSearch = project.CreatePageSearch();
  pageSearch.ContentClass = project.ContentClasses.GetByName("MyContentClass");
 
  var pages = pageSearch.Execute();
 
  // Attach suffix ".php" to all filenames of the pages found
  foreach(var curPage in pages )
  {
    curPage.Filename = curPage.Filename + ".php";
 
    // Commit changes to the server
    curPage.Commit();
  }
}
```

Mailing list
---
We are using <a href="https://groups.google.com/forum/?#!forum/smartapi">Google Group "SmartAPI"</a> for discussions, questions and announcements. You do not need a Google account to join this group but you can also use it via mail.

* Join with Google account: You can “Join this group” at <a href="https://groups.google.com/forum/?#!forum/smartapi">Google Group SmartAPI</a>.
* Join without Google account: Send an email to <a href="smartapi+subscribe@googlegroups.com">smartapi+subscribe@googlegroups.com</a>. You will receive a confirmation mail that you need to reply to (just “reply” is fine).

Messages to the group can be posted directly on the group’s page (with a Google account), or after subscription via mail you can send an email to: <a href="smartapi@googlegroups.com">smartapi@googlegroups.com</a>.

More information 
---
You can find more information at http://www.smartapi.de (Deutsch / English)
