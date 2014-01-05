// Guids.cs
// MUST match guids.h
using System;

namespace asinbow.AttachIT
{
    static class GuidList
    {
        public const string guidAttachITPkgString = "243330d8-2660-436c-abc6-35264564931e";
        public const string guidAttachITCmdSetString = "52966d2b-19be-4a99-8787-49c621e8e0d2";
        public const string guidToolWindowPersistanceString = "61f11703-3c56-4f1a-994c-e5dabbf9ee89";

        public static readonly Guid guidAttachITCmdSet = new Guid(guidAttachITCmdSetString);
    };
}