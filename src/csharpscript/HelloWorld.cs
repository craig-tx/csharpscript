#pragma warning disable 1633 // disable unrecognized pragma directive warning
#pragma reference "JWT.dll"
#pragma reference "Flurl.dll"
#pragma reference "Flurl.Http.dll"

using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using Flurl;
using Flurl.Http;

/// <summary>
/// Publish a release to a website - experimental
/// </summary>
public class HelloWorld
{
    public static void Main(string[] args)
    {
        bool bOK = false;
        try
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["DateNow"] = DateTime.UtcNow;

            string token = JwtTokenHandler.generateJWTToken(payload);
            Console.WriteLine(token);

            var someSite = "https://gocircle.ai".GetStringAsync().Result;
            Console.WriteLine($"Fetch live HTML ----> {someSite.Substring(0, 100)}");

            Console.WriteLine($"FULLPATH: {ScriptGlobals.ScriptPath}");
            Console.WriteLine($"FOLDERPATH: {ScriptGlobals.ScriptFolder}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        Environment.Exit(bOK ? 0 : 1);
    }
}

//#include TestSubFolder/JwtTokenHandler.txt
//#include TestSubFolder\OtherFile.txt