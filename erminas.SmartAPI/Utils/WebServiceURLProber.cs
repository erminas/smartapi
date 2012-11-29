/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Net;
using System.Text.RegularExpressions;

namespace erminas.SmartAPI.Utils
{
    /// <summary>
    ///   Provides <see cref="RqlUri" /> to get the correct web service URI of a RedDotServer. The URIs for the web service description have changed with version 11, so if the target server version is not known, this method has to be used to determine the correct URI.
    /// </summary>
    public static class WebServiceURLProber
    {
        private static readonly Regex VERSION_REGEXP =
            new Regex("Management Server&nbsp;\\d+(\\.\\d+)*&nbsp;Build&nbsp;(\\d+)\\.");

        /// <summary>
        ///   Get the correct web service URI of a RedDotServer. The URIs for the web service description have changed with version 11, so if the target server version is not known, this method has to be used to determine the correct URI.
        /// </summary>
        /// <param name="baseURL"> Base url to check, e.g. http://localhost/cms/ </param>
        /// <returns> </returns>
        public static Uri RqlUri(string baseURL)
        {
            int version = VersionInfo(baseURL);
            return version < 11
                       ? new Uri(baseURL + "webservice/RDCMSXMLServer.WSDL")
                       : new Uri(baseURL + "WebService/RQLWebService.svc");
        }

        private static int VersionInfo(string baseURL)
        {
            var versionURI = baseURL + (baseURL.EndsWith("/") ? "ioVersionInfo.asp" : "/ioVersionInfo.asp");
            using (var client = new WebClient())
            {
                var responseText = client.DownloadString(versionURI);
                var match = VERSION_REGEXP.Match(responseText);
                if (match.Groups.Count != 3)
                {
                    throw new Exception("Could not retrieve version info of RedDot server at " + baseURL + "\n" +
                                        responseText);
                }

                return int.Parse(match.Groups[2].Value);
            }
        }
    }
}