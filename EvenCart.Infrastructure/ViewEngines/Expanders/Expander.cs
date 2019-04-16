﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EvenCart.Core.Infrastructure;
using EvenCart.Infrastructure.Localization;

namespace EvenCart.Infrastructure.ViewEngines.Expanders
{
    public abstract class Expander : IExpander
    {
        public string TagName { get; set; }

        private Regex AssociatedRegEx { get; set; }

        private readonly ILocalizer _localizer;
        protected Expander()
        {
            _localizer = DependencyResolver.Resolve<ILocalizer>();
        }

        private static readonly List<Expander> Expanders = new List<Expander>()
        {
            new LayoutExpander() {TagName = "layout"},
            new PartialExpander() {TagName = "partial"},
            new WidgetExpander() {TagName = "widget"},
            new ComponentExpander() {TagName = "component"},
            new ControlExpander() {TagName = "control"},
            new UrlRouteExpander() {TagName = "route"},
            new NavigationExpander()
            {
                TagName = ApplicationConfig.PrimaryNavigationName,
                NavigationType = ApplicationConfig.PrimaryNavigationName
            },
            new NavigationExpander()
            {
                TagName = ApplicationConfig.SecondaryNavigationName,
                NavigationType = ApplicationConfig.SecondaryNavigationName
            },
            new GlobalExpander() {TagName = "global"},
            new JsonExpander() { TagName = "json"}
        };

        private const string TagMatcherPattern =
            @"{%\s*{tagName}(?:\s+([\w\d._\\/]+|""[^""]+"")(?!\w*=))*(\s+[\w\d\s_]+(?>=(?:@t)?"")[^""]*(?="")"")*\s*%}";

        private static Regex GetTagRegex(string tagName)
        {
            return new Regex(TagMatcherPattern.Replace("{tagName}", tagName));
        }
        public static void ExpandView(string viewName, object parameters, out string expandedContent)
        {
            var readFile = ReadFile.From(viewName);
            expandedContent = SafeExpandView(readFile, readFile.Content, parameters, true);
        }

        private static string SafeExpandView(ReadFile readFile, string inputContent, object parameters = null, bool prePostRun = false)
        {
            foreach (var e in Expanders)
            {
                var tagRegex = e.AssociatedRegEx ?? GetTagRegex(e.TagName);
                e.AssociatedRegEx = tagRegex;
                if (prePostRun)
                    //run before expansion
                    e.PreRun(readFile);
                //run the expansion
                inputContent = e.Expand(readFile, tagRegex, inputContent, parameters);
                if (prePostRun)
                    //run the post expansion
                    e.PostRun(readFile);
            }

            //run the matchers again if required after expansion
            foreach (var e in Expanders)
            {
                if (e.AssociatedRegEx.Matches(inputContent).Any())
                    inputContent = SafeExpandView(readFile, inputContent, parameters);
            }
            return inputContent;
        }

        protected void ExtractMatch(Match match, out string[] straightParameters, out Dictionary<string, string> keyValuePairs)
        {
            straightParameters = null;
            keyValuePairs = null;

            if (match.Groups[1].Success)
            {
                straightParameters = match.Groups[1].Captures.Select(x => x.Value.Trim('"')).ToArray();
            }
            if (match.Groups[2].Success)
            {
                //let's populate dictionary with keyvalue pairs, stripping out everything that's not needed
                keyValuePairs = new Dictionary<string, string>();
                foreach (Capture capture in match.Groups[2].Captures)
                {
                    var pSplit = capture.Value.Split('=', 2)./*Select(x => x.Trim('"')).*/ToList();
                    if (pSplit[1].StartsWith("@t"))
                    {
                        //the value needs a translation
                        pSplit[1] = _localizer.Localize(pSplit[1].Substring(2).Trim('"'));
                    }
                    else
                        pSplit[1] = pSplit[1].Trim('"');
                    pSplit[0] = pSplit[0].Trim();
                    keyValuePairs.Add(pSplit[0], pSplit[1]);
                }
            }
        }

        public abstract string Expand(ReadFile readFile, Regex regEx, string inputContent, object parameters = null);

        public virtual void PreRun(ReadFile readFile)
        {
            //execute anything that needs to be done before expansion
        }
        public virtual void PostRun(ReadFile readFile)
        {
            //execute anything that needs to be done before expansion
        }

    }
}