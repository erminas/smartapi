using erminas.SmartAPI.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using erminas.SmartAPI.CMS;

namespace erminas_Smart_API_Unit_Tests
{


    /// <summary>
    ///Dies ist eine Testklasse für "ExportSettingsTest" und soll
    ///alle ExportSettingsTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ExportSettingsTest
    {


        /// <summary>
        ///Ein Test für "ExportSettings-Konstruktor"
        ///</summary>
        [TestMethod()]
        public void ExportSettingsConstructorTest()
        {
            var exportsettings = new ExportSettings(@"c:\Test", true, true, true, true, null, null, true, null, "Test", "This is a Test");
            Assert.AreEqual(@"c:\Test", exportsettings.TargetPath);
            Assert.AreEqual("1", exportsettings.CreateFolderForEachExport);
            Assert.AreEqual("1", exportsettings.IncludeAdminData);
            Assert.AreEqual("1", exportsettings.IncludeArchive);
            Assert.AreEqual("1", exportsettings.LogoutUsers);
            Assert.AreEqual("1", exportsettings.EmailNotification);
            Assert.AreEqual(null, exportsettings.SendTo);
            Assert.AreEqual(null, exportsettings.Subject);
            Assert.AreEqual(null, exportsettings.Message);

            exportsettings = new ExportSettings(@"c:\Test2");
            Assert.AreEqual(@"c:\Test2", exportsettings.TargetPath);
            Assert.AreEqual("0", exportsettings.CreateFolderForEachExport);
            Assert.AreEqual("0", exportsettings.IncludeAdminData);
            Assert.AreEqual("0", exportsettings.IncludeArchive);
            Assert.AreEqual("0", exportsettings.LogoutUsers);
            Assert.AreEqual("0", exportsettings.EmailNotification);
            Assert.AreEqual(null, exportsettings.SendTo);
            Assert.AreEqual(null, exportsettings.Subject);
            Assert.AreEqual(null, exportsettings.Message);
        }

        /// <summary>
        ///Ein Test für "CreateExportSettingsSQL"
        ///</summary>
        [TestMethod()]
        public void ToRQLStringtest()
        {
            var exportsettings = new ExportSettings(@"c:\Test", true, true, true, true, null, null, true, null, "Test", "This is a Test");
            var exportSettingsAsString = exportsettings.ToRQLString();
            Assert.AreEqual(@"targetpath=""c:\Test"" createFolderForEachExport=""1"" includeAdminData=""1"" logoutUsers=""1"" emailnotification=""0"" ", exportSettingsAsString);
        }
    }
}
