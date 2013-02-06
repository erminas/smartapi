// Smart API - .Net programatical access to RedDot servers
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
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public abstract class VersionAttribute : Attribute
    {
        protected VersionAttribute(int major, int minor, int build, int rev)
        {
            Version = new Version(major, minor, build, rev);
        }

        public Version Version { get; set; }

        public string VersionName { get; set; }
        public abstract void Validate(ServerLogin login, Version actualVersion, string method);
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class VersionIsGreaterThanOrEqual : VersionAttribute
    {
        public VersionIsGreaterThanOrEqual(int major, int minor = 0, int build = 0, int rev = 0)
            : base(major, minor, build, rev)
        {
        }

        public override void Validate(ServerLogin login, Version actualVersion, string method)
        {
            if (actualVersion < Version)
            {
                string versionNameString = string.IsNullOrEmpty(VersionName) ? "" : " (" + VersionName + ")";
                throw new InvalidServerVersionException(login,
                                                        string.Format(
                                                            "Invalid server version. {0} only works on servers with version greater than or equal {1}{3}, but the current server version is {2}",
                                                            method, Version, actualVersion, versionNameString));
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class VersionIsLessThan : VersionAttribute
    {
        public VersionIsLessThan(int major, int minor = 0, int build = 0, int rev = 0) : base(major, minor, build, rev)
        {
        }

        public override void Validate(ServerLogin login, Version actualVersion, string method)
        {
            if (actualVersion >= Version)
            {
                string versionNameString = string.IsNullOrEmpty(VersionName) ? "" : " (" + VersionName + ")";
                throw new InvalidServerVersionException(login,
                                                        string.Format(
                                                            "Invalid server version. {0} only works on servers with version less than {1}{3}, but the current server version is {2}",
                                                            method, Version, actualVersion, versionNameString));
            }
        }
    }
}