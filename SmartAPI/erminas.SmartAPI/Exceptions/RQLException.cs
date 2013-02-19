// Smart API - .Net programmatic access to RedDot servers
//  
// Copyright (C) 2013 erminas GbR
// 
// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with this program.
// If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;

namespace erminas.SmartAPI.Exceptions
{

    #region ErrorCode

    public enum ErrorCode
    {
        Unknown = 0,
        PleaseLogin,
        RDError1,
        RDError2,
        RDError3,
        RDError4,
        RDError5,
        RDError6,
        RDError7,
        RDError8,
        RDError9,
        RDError10,
        RDError11,
        RDError12,
        RDError13,
        RDError14,
        RDError15,
        RDError16,
        RDError17,
        RDError101,
        RDError110,
        RDError201,
        RDError301,
        RDError401,
        RDError510,
        RDError511,
        RDError707,
        RDError800,
        RDError2910,
        RDError2911,
        RDError3000,
        RDError3032,
        Error3049,
        RDError4005,
        RDError5001,
        RDError6001,
        RDError15805,
        RDError16997
    }

    internal static class ErrorCodeUtils
    {
        internal static ErrorCode ToErrorCode(this string value)
        {
            if (value == "Please Login")
            {
                return ErrorCode.PleaseLogin;
            }
            return (ErrorCode) Enum.Parse(typeof (ErrorCode), value);
        }
    }

    #endregion

    [Serializable]
    public class RQLException : SmartAPIException
    {
        private const string RESPONSE = "response";
        private const string SERVER = "server";
        private const string ERROR_MESSAGE = "error_message";

        #region error codes

        private readonly List<Tuple<string, string>> _errorCodes = new List<Tuple<string, string>>
            {
                Tuple.Create("Please Login",
                             "The user session has timed out or the Login GUID is no longer valid. Please login again."),
                Tuple.Create("RDError1", "The number of modules in the license key does not correspond to the checksum."),
                Tuple.Create("RDError2", "The license key is only valid for the Beta test."),
                Tuple.Create("RDError3",
                             "The license key is not correct. An error which could not be classified has occurred during the check."),
                Tuple.Create("RDError4", "License is no longer valid."),
                Tuple.Create("RDError5", "The server IP address is different than specified in the license."),
                Tuple.Create("RDError6", "License is not yet valid."),
                Tuple.Create("RDError7",
                             "License is a cluster license. This error message is no longer supported beginning with CMS 6.0."),
                Tuple.Create("RDError8", "The IP address check in the license key is not correct."),
                Tuple.Create("RDError9", "Invalid version of the license key."),
                Tuple.Create("RDError10", "There are duplicate modules in the license."),
                Tuple.Create("RDError11", "A module in the license is flawed."),
                Tuple.Create("RDError12", "There are illegal characters in the license."),
                Tuple.Create("RDError13", "The checksum is not correct."),
                Tuple.Create("RDError14", "The serial number in the license is not correct."),
                Tuple.Create("RDError15",
                             "The serial number of the license key is different from the serial number of the previous license key."),
                Tuple.Create("RDError16", "The IP address of the Loopback adapter is not supported in this license."),
                Tuple.Create("RDError17", "The license key contains no valid serial number."),
                Tuple.Create("RDError101", "The user is already logged on."),
                Tuple.Create("RDError110",
                             "The user does not have the required privileges to execute the RQL. The cause of this situation may be that the logon GUID or session key has expired or the session has timed out."),
                Tuple.Create("RDError201", "Access to database \"ioAdministration\" has failed."),
                Tuple.Create("RDError301", "Defective asynchronous component."),
                Tuple.Create("RDError401", "A project is locked for the executing user or the user level."),
                Tuple.Create("RDError510", "The application server is not available."),
                Tuple.Create("RDError511", "The application server or the Management Server databases are updated."),
                Tuple.Create("RDError707", "The Login GUID does not correspond to the logged on user."),
                Tuple.Create("RDError800", "The maximum number of logins for a user has been reached."),
                Tuple.Create("RDError2910", "References still point to elements of this page."),
                Tuple.Create("RDError2911", "At least one element is still assigned as target container to a link."),
                Tuple.Create("RDError3000", "Package already exists."),
                Tuple.Create("RDError3032",
                             "You have tried to delete a content class on the basis of which pages have already been created."),
                Tuple.Create("Error3049", "Too many users to a license. Login to CMS failed. Please login again later."),
                Tuple.Create("RDError4005", "This user name already exists."),
                Tuple.Create("RDError5001", "The folder path could not be found or the folder does no longer exist."),
                Tuple.Create("RDError6001", "A file is already being used in the CMS."),
                Tuple.Create("RDError15805", "You have no right to delete this page."),
                Tuple.Create("RDError16997",
                             "You cannot delete the content class. There are pages which were created on the basis of this content class in other projects.")
            };

        #endregion

        public RQLException(string server, string reason, string responseXML) : base(server, reason)
        {
            Server = server;
            //use lastordefault because the numbers are increasing/more specific at the end. E.g. RDError1 is also contained in RDError110.
            Tuple<string, string> msg = _errorCodes.LastOrDefault(x => reason.Contains(x.Item1));
            ErrorMessage = msg != null ? msg.Item2 : reason;
            ErrorCode = msg != null ? msg.Item1.ToErrorCode() : ErrorCode.Unknown;
            Response = responseXML;
        }

        protected RQLException(RQLException rqlException)
            : this(rqlException.Server, rqlException.ErrorMessage, rqlException.Response)
        {
        }

        public ErrorCode ErrorCode { get; set; }

        public string ErrorMessage
        {
            get { return (Data[ERROR_MESSAGE] ?? "").ToString(); }
            private set { Data.Add(ERROR_MESSAGE, value); }
        }

        public override string Message
        {
            get { return string.Format("RQL request to '{0}' failed. Error message: '{1}'", Server, ErrorMessage); }
        }

        public string Response
        {
            get { return (Data[RESPONSE] ?? "").ToString(); }
            private set { Data.Add(RESPONSE, value); }
        }
    }
}