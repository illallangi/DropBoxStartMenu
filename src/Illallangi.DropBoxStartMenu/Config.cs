using System;
using System.Data.SQLite;
using System.IO;
using Microsoft.Win32;

namespace Illallangi.DropBox.StartMenu
{
    public interface IConfig
    {
        string DropboxInstallPath { get; }
        string DropboxConfigDbPath { get; }
        string DropboxPath { get; }
        string ProgramsPath { get; }
    }

    public sealed class Config : IConfig
    {
        #region Fields

        private static log4net.ILog currentLogger;
        private string currentDropboxInstallPath;
        private string currentDropboxConfigDbPath;
        private string currentDropboxPath;

        private string currentProgramsPath;

        #endregion

        #region Methods

        private static string GetDropboxInstallPath()
        {
            var dropboxInstallPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Dropbox", "InstallPath", string.Empty).ToString();

            if (string.IsNullOrWhiteSpace(dropboxInstallPath))
            {
                Config.Logger.DebugFormat("DropboxInstallPath: Value not found in registry, returning null.");
                return null;
            }

            if (!Directory.Exists(dropboxInstallPath))
            {
                Config.Logger.DebugFormat("DropboxInstallPath: DropboxInstallPath \"{0}\" does not exist; returning null.", dropboxInstallPath);
                return null;
            }

            Config.Logger.DebugFormat("DropboxInstallPath: \"{0}\"", dropboxInstallPath);
            return dropboxInstallPath;
        }

        private static string GetDropboxConfigDbPath(string dropboxInstall)
        {
            if (string.IsNullOrWhiteSpace(dropboxInstall))
            {
                Config.Logger.DebugFormat("DropboxConfigDbPath: DropboxInstallPath is null or is whitespace; returning null.");
                return null;
            }

            var dropboxInstallPath = new DirectoryInfo(dropboxInstall);
            if (null == dropboxInstallPath.Parent)
            {
                Config.Logger.DebugFormat("DropboxConfigDbPath: DropboxInstallPath Parent is null; returning null.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(dropboxInstallPath.Parent.FullName))
            {
                Config.Logger.DebugFormat("DropboxConfigDbPath: DropboxInstallPath Parent FullName is null or is whitepace; returning null.");
                return null;
            }

            var configDbPath = Path.Combine(dropboxInstallPath.Parent.FullName, "config.db");
            if (!File.Exists(configDbPath))
            {
                Config.Logger.DebugFormat("DropboxConfigDbPath: ConfigDbPath \"{0}\" doesn't exist; returning null.", configDbPath);
                return null;
            }

            Config.Logger.DebugFormat("DropboxConfigDbPath: \"{0}\"", configDbPath);
            return configDbPath;
        }

        private static string GetDropboxPath(string dropboxConfigDbPath)
        {
            var dropboxPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE") ?? string.Empty, "dropbox");
            Config.Logger.DebugFormat("DropboxPath: %USERPROFILE%\\dropbox \"{0}\"", dropboxPath);

            if (!string.IsNullOrWhiteSpace(dropboxConfigDbPath))
            {
                using (var connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;New=True;Compress=True;", dropboxConfigDbPath)).OpenAndReturn())
                using (var command = new SQLiteCommand("select value from config where key='dropbox_path'", connection))
                using (var dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        dropboxPath = dataReader["value"].ToString();
                        Config.Logger.DebugFormat("DropboxPath: dropbox_path from SQLite \"{0}\"", dropboxPath);
                    }
                }
            }

            if (!Directory.Exists(dropboxPath))
            {
                Config.Logger.DebugFormat("DropboxPath: \"{0}\" does not exist; returning null.", dropboxPath);
                return null;
            }

            Config.Logger.DebugFormat("DropboxPath: \"{0}\"", dropboxPath);
            return dropboxPath;
        }

        private static string GetProgramsPath()
        {
            var programsPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Programs, Environment.SpecialFolderOption.Create);
            if (string.IsNullOrWhiteSpace(programsPath))
            {
                Config.Logger.DebugFormat("ProgramsPath: Special folder not found in registry, returning null.");
                return null;
            }

            if (!Directory.Exists(programsPath))
            {
                Config.Logger.DebugFormat("ProgramsPath: ProgramsPath \"{0}\" does not exist; returning null.", programsPath);
                return null;
            }

            Config.Logger.DebugFormat("ProgramsPath: \"{0}\"", programsPath);
            return programsPath;
        }

        #endregion

        #region Properties

        private static log4net.ILog Logger
        {
            get
            {
                return Config.currentLogger ?? (Config.currentLogger = log4net.LogManager.GetLogger(typeof(Config)));
            }
        }

        public string DropboxInstallPath
        {
            get
            {
                return this.currentDropboxInstallPath ?? (this.currentDropboxInstallPath = Config.GetDropboxInstallPath());
            }
        }

        public string DropboxConfigDbPath
        {
            get
            {
                return this.currentDropboxConfigDbPath ?? (this.currentDropboxConfigDbPath = Config.GetDropboxConfigDbPath(this.DropboxInstallPath));
            }
        }

        public string DropboxPath
        {
            get
            {
                return this.currentDropboxPath ?? (this.currentDropboxPath = Config.GetDropboxPath(this.DropboxConfigDbPath));
            }
        }

        public string ProgramsPath
        {
            get
            {
                return this.currentProgramsPath ?? (this.currentProgramsPath = Config.GetProgramsPath());
            }
        }

        #endregion
    }
}
