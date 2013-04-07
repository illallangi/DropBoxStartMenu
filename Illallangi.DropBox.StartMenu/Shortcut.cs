using System;
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
        private string currentIconPath;
        private int? currentIconIndex;

        #endregion

        #region Methods

        private string GetName()
        {
            return Path.GetFileNameWithoutExtension(this.FileBackedSource);
        }

        private string GetTarget()
        {
            return string.Format("{0}.{1}", this.Name, "exe");
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

        #endregion
    }
}