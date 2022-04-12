#pragma warning disable 1633 // disable unrecognized pragma directive warning
#pragma reference "JWT.dll"

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;

/// <summary>
/// An example script to be executed by C#.exe
/// </summary>
public class AdHoc
{
    public static void Main(string[] args)
    {
        try
        {
            Console.WriteLine($"hello {args[0]} Script path is {ScriptGlobals.ScriptPath}");

            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["test"] = args[0];

            string token = JwtTokenHandler.generateJWTToken(payload);
            Console.WriteLine(token);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

public class JwtTokenHandler
{
    private const string AUTHORIZATION = "Authorization";
    private const string BEARER = "Bearer";
    private const string SECRET = "2zyMq8fAEgXLy7DpdywHmtNRmThyCTa";

    public static string generateJWTToken(Dictionary<string, object> payload)
    {
        string token = string.Empty;
        try
        {
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            token = encoder.Encode(payload, SECRET);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex}");
        }
        return token;
    }

    public static string decodeJWTToken(string token, string secret)
    {
        try
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); // symmetric
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

            var json = decoder.Decode(token, secret, verify: true);
            return json;
        }
        catch (TokenExpiredException)
        {
            Console.WriteLine($"Token has expired {secret}");
            return null;
        }
        catch (SignatureVerificationException svex)
        {
            Console.WriteLine($"Token has invalid signature: {svex}");
            return null;
        }
    }
}
