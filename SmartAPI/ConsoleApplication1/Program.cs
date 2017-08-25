using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net;
using erminas.SmartAPI.CMS;
using erminas.SmartAPI.Utils;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            // RedDot Login with (user/password)
            var authData = new PasswordAuthentication("user", "password");

            var login = new ServerLogin { Address = new Uri("http://localhost/cms"), AuthData = authData };

            // Session is the entry point to interact with the RedDot server.
            // Creating the session object automatically creates a connection.
            // Dispose() closes the connection in a a clean way (this is done
            // automatically at the end of the using block).
            using(var session = SessionBuilder.CreateOrReplaceOldestSession(login))
            {
                // Select a project based on the name
                var project = session.ServerManager.Projects.GetByName("MyProjekt");

                // Find all pages based on the Content Class "MyContentClass"
                var pageSearch = project.Pages.CreateSearch();
                pageSearch.ContentClass = project.ContentClasses.GetByName("MyContentClass");

                var pages = pageSearch.Execute();

                // Attach suffix ".php" to all filenames of the pages found
                foreach(var curPage in pages)
                {
                    curPage.Filename = curPage.Filename + ".php";

                    // Commit changes to the server
                    curPage.Commit();
                }
            }
        }
    }
}
