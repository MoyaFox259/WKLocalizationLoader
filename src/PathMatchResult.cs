using System;
using System.Text.RegularExpressions;

namespace WKLocalizationLoader
{
    public class PathMatchResult
    {
        public string Path;
        public Match MatchResult;

        public PathMatchResult(string path, Match matchResult)
        {
            Path = path;
            MatchResult = matchResult;
        }
    }
}

