using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class PathingCubeParser
{
    public static string GetValue(string signText, string key)
    {
        if ( string.IsNullOrEmpty( signText ) )
            return string.Empty; 

        foreach (var text in signText.Split(';'))
        {
            var parse = text.Split('=');
            if (parse.Length == 2)
                if (parse[0].ToLower() == key.ToLower())
                    return parse[1];
        }

        return "";
    }
}

