namespace Illallangi.DropBox.StartMenu
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    public sealed class ShortcutSource : IShortcutSource
    {
        private log4net.ILog currentLogger;
        private readonly IConfig currentConfig;

        public ShortcutSource(IConfig config)
        {
            this.currentConfig = config;
        }

        public IEnumerable<Shortcut> GetShortcuts()
        {
            foreach (var shortcut in Directory.GetFiles(this.Config.DropboxPath, "*.shortcut", SearchOption.AllDirectories))
            {
                Shortcut y = null;
                try
                {
                    y = 0 == new FileInfo(shortcut).Length ? new Shortcut().ToFile(shortcut) : Shortcut.FromFile(shortcut);
                }
                catch (XmlException e)
                {
                    this.Logger.ErrorFormat("XmlException reading {0}\r\n{1}", shortcut, e.Message);
                }
                catch (InvalidOperationException e)
                {
                    this.Logger.ErrorFormat("InvalidOperationException reading {0}\r\n{1}", shortcut, e.Message);
                }

                if (null != y)
                {
                    yield return y;
                }
            }
        }

        private IConfig Config
        {
            get
            {
                return this.currentConfig;
            }
        }

        private log4net.ILog Logger
        {
            get
            {
                return this.currentLogger ?? (this.currentLogger = log4net.LogManager.GetLogger(typeof(ShortcutSource)));
            }
        }
    }

    public interface IShortcutSource
    {
        IEnumerable<Shortcut> GetShortcuts();
    }
}
