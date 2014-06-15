using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Illallangi.DropBox.StartMenu
{
    [XmlRoot("shortcut")]
    public sealed class Shortcut : XmlFileBackedObject<Shortcut>
    {
        #region Fields

        private string currentName;
        private string currentTarget;
        private string currentArguments;
        private string currentIconPath;
        private int? currentIconIndex;

        #endregion

        #region Methods

        public IEnumerable<KeyValuePair<string, string>> GetLinks()
        {
            foreach (var linkedFolder in this.LinkedFolders)
            {
                if (Directory.Exists(linkedFolder.Key))
                {
                    foreach (var path in Directory.GetDirectories(linkedFolder.Key))
                    {
                        yield return new KeyValuePair<string, string>(Path.Combine(linkedFolder.Value, Path.GetFileName(path)), path);
                    }
                }
            }
        }

        private string GetName()
        {
            return Path.GetFileNameWithoutExtension(this.FileBackedSource);
        }

        private string GetTarget()
        {
            return string.Format("{0}.{1}", this.Name, "exe");
        }

        private string GetArguments()
        {
            return string.Empty;
        }

        private string GetIconPath()
        {
            return this.Target;
        }

        private int GetIconIndex()
        {
            return 0;
        }

        #endregion

        #region Properties

        [XmlAttribute("name")]
        public string Name
        {
            get
            {
                return this.currentName ?? (this.currentName = this.GetName());
            }

            set
            {
                this.currentName = value;
            }
        }

        [XmlAttribute("target")]
        public string Target
        {
            get
            {
                return this.currentTarget ?? (this.currentTarget = this.GetTarget());
            }

            set
            {
                this.currentTarget = value;
            }
        }

        [XmlAttribute("arguments")]
        public string Arguments
        {
            get
            {
                return this.currentArguments ?? (this.currentArguments = this.GetArguments());
            }
            set
            {
                this.currentArguments = value;
            }
        }

        [XmlIgnore]
        public string Working
        {
            get
            {
                return Path.GetDirectoryName(this.FileBackedSource);
            }
        }

        [XmlAttribute("iconpath")]
        public string IconPath
        {
            get
            {
                return this.currentIconPath ?? (this.currentIconPath = this.GetIconPath());
            }

            set
            {
                this.currentIconPath = value;
            }
        }

        [XmlAttribute("iconindex")]  
        public int IconIndex
        {
            get
            {
                if (this.currentIconIndex.HasValue)
                {
                    return this.currentIconIndex.Value;
                }

                var result = this.GetIconIndex();
                this.currentIconIndex = result;
                return result;
            }
            
            set
            {
                this.currentIconIndex = value;
            }
        }

        [XmlIgnore]
        public string Folder
        {
            get
            {
                return Path.GetDirectoryName(this.FileBackedSource);
            }
        }

        [XmlIgnore]
        public IEnumerable<KeyValuePair<string, string>> LinkedFolders
        {
            get
            {
                yield return new KeyValuePair<string, string>(this.AppData, Environment.ExpandEnvironmentVariables(@"%appdata%"));
                yield return new KeyValuePair<string, string>(this.LocalAppData, Environment.ExpandEnvironmentVariables(@"%localappdata%"));

            }
        }
        [XmlIgnore]
        public string AppData
        {
            get
            {
                return Path.Combine(this.Folder, "AppData");
            }
        }

        [XmlIgnore]
        public string LocalAppData
        {
            get
            {
                return Path.Combine(this.Folder, "LocalAppData");
            }
        }


        #endregion
    }
}