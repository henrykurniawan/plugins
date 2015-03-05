﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class Utilities
    {
        private const string FileName = "narratorNames.xml";
        private static readonly string DictionaryFolder = GetDicTionaryFolder();
        private static List<string> _listNames = LoadNames(Path.Combine(DictionaryFolder, "names_etc.xml")); // Uppercase names
        private static List<string> _listNewName = LoadNames(Path.Combine(DictionaryFolder, "narratorNames.xml"));


        public static IList<string> ListNames
        {
            get { return _listNames.Concat(_listNewName).ToList(); }
        }

        public static List<string> LoadNames(string fileName)
        {
            if (_listNames != null && fileName.EndsWith("names_etc.xml", StringComparison.OrdinalIgnoreCase))
                _listNames.Clear();

            if (File.Exists(fileName))
            {
                return (from elem in XElement.Load(fileName).Elements()
                        select elem.Value.ToUpperInvariant()).ToList();
            }
            else
            {
                try
                {
                    // Create file
                    var xdoc = new XDocument(
                        new XElement("names",
                            new XElement("name", "JOHN"),
                            new XElement("name", "MAN"),
                            new XElement("name", "WOMAN"),
                            new XElement("name", "CAITLIN")
                            ));

                    if (!Directory.Exists(DictionaryFolder))
                        Directory.CreateDirectory(DictionaryFolder);
                    xdoc.Save(Path.Combine(DictionaryFolder, "narratorNames.xml"), SaveOptions.None);
                    return (from elem in xdoc.Root.Elements()
                            select elem.Value.ToUpperInvariant()).ToList();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            return new List<string>();
        }

        public static string GetDicTionaryFolder()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Dictionaries");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtiitle Edit", "Dictionary"));
            return path;
        }

        public static int NumberOfLines(string text)
        {
            var ln = 0;
            var idx = 0;
            do
            {
                ln++;
                idx = text.IndexOf('\n', idx + 1);
            } while (idx > -1);
            return ln;
        }

        public static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static bool IsInteger(string s)
        {
            int i;
            return int.TryParse(s, out i);
        }

        internal static string RemoveHtmlFontTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?font>", string.Empty);
            return RemoveTag(s, "<font");
        }

        public static string RemoveHtmlTags(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            if (s.IndexOf('<') < 0)
                return s;
            s = Regex.Replace(s, "(?i)</?[uib]>", string.Empty);
            return RemoveHtmlFontTag(s).Trim();
        }

        public static string RemoveTag(string text, string tag)
        {
            var idx = text.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                var endIndex = text.IndexOf('>', idx + tag.Length);
                if (endIndex < idx) break;
                text = text.Remove(idx, endIndex - idx + 1);
                idx = text.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
            }
            return text;
        }

        public static string RemoveBrackets(string inputString)
        {
            return Regex.Replace(inputString, @"^[\[\(]|[\]\)]$", string.Empty).Trim();
        }

        public static bool FixIfInList(string name)
        {
            if (ListNames == null)
                return false;

            // ListNames is already loaded with all names to UpperCase
            return ListNames.Contains(name.ToUpperInvariant());
        }

        public static void AddNameToList(string name)
        {
            if (name == null || name.Trim().Length == 0)
                return;
            //var normalCase = Regex.Replace(name.ToLowerInvariant(), "\\b\\w", x => x.Value.ToUpperInvariant(), RegexOptions.Compiled);
            name = name.ToUpperInvariant();
            if (!ListNames.Contains(name))
            {
                _listNewName.Add(name);
                SaveToXmlFile();
            }
        }

        private static void SaveToXmlFile()
        {
            if (_listNewName == null || _listNewName.Count == 0)
                return;
            var filePath = Path.Combine(DictionaryFolder, FileName);
            try
            {
                var xelem = XDocument.Load(filePath);
                xelem.Root.ReplaceAll((from name in _listNewName
                                       select new XElement("name", name)).ToList());
                /*
                    foreach (var name in _listNewName)
                        xelem.Root.Add(new XElement("name", name));*/

                xelem.Save(filePath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

    }
}