//-----------------------------------------------------------------------
// <copyright file="DependentClass.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSignalDependentLibrary
{
    using Newtonsoft.Json;

    public class DependentClass
    {
        public string GetString()
        {
            return "with dependency";
        }

        public string ObjectToString(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
