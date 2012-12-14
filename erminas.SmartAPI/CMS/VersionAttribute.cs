using System;
using erminas.SmartAPI.Exceptions;

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
        public abstract void Validate(Version actualVersion, string method);
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class VersionIsGreaterThanOrEqual : VersionAttribute
    {
        public VersionIsGreaterThanOrEqual(int major, int minor = 0, int build = 0, int rev = 0)
            : base(major, minor, build, rev)
        {
        }

        public override void Validate(Version actualVersion, string method)
        {
            if (actualVersion < Version)
            {
                string versionNameString = string.IsNullOrEmpty(VersionName) ? "" : " (" + VersionName + ")";
                throw new InvalidServerVersionException(
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

        public override void Validate(Version actualVersion, string method)
        {
            if (actualVersion >= Version)
            {
                string versionNameString = string.IsNullOrEmpty(VersionName) ? "" : " (" + VersionName + ")";
                throw new InvalidServerVersionException(
                    string.Format(
                        "Invalid server version. {0} only works on servers with version less than {1}{3}, but the current server version is {2}",
                        method, Version, actualVersion, versionNameString));
            }
        }
    }
}