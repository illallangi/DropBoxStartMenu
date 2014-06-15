// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Illallangi Enterprises">
// Copyright (C) 2013 Illallangi Enterprises
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Security.Principal;

using Ninject;
using Illallangi.ShellLink;

namespace Illallangi.DropBox.StartMenu
{

    /// <summary>
    /// The Program.
    /// </summary>
    public sealed class Program : IProgram
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        #region Fields

        private log4net.ILog currentLogger;

        private readonly IConfig currentConfig;
        private readonly IShortcutSource currentShortcutSource;

        #endregion

        #region Constructors

        public Program(IConfig config, IShortcutSource shortcutSource)
        {
            this.currentConfig = config;
            this.currentShortcutSource = shortcutSource;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The Main method for the Program.
        /// </summary>
        /// <param name="arguments">
        /// The command line arguments passed to the program.
        /// </param>
        public static void Main(string[] arguments)
        {
            try
            {
                new StandardKernel(new ProgramModule<Program, IProgram>(arguments)).Get<IProgram>().Execute();
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(Program)).FatalFormat(e.ToString());
            }
        }

        /// <summary>
        /// The main loop for the Program.
        /// </summary>
        /// <exception cref="NotImplementedException" />
        public void Execute()
        {
            foreach (var shortcut in this.ShortcutSource.GetShortcuts())
            {
                var shortcutPath = Path.Combine(this.Config.ProgramsPath, string.Format("{0}.{1}", shortcut.Name, "lnk"));
                var shortcutDirectory = Path.GetDirectoryName(shortcutPath);
                if (!Directory.Exists(shortcutDirectory))
                {
                    Directory.CreateDirectory(shortcutDirectory);
                    this.Logger.InfoFormat("Created directory:\r\n\tPath:\t{0}", shortcutDirectory);
                }

                (new ShellShortcut(shortcutPath)
                                        {
                                            Path = Path.Combine(shortcut.Working, shortcut.Target),
                                            WorkingDirectory = shortcut.Working,
                                            IconPath = Path.Combine(shortcut.Working, shortcut.IconPath),
                                            IconIndex = shortcut.IconIndex,
                                            Arguments = Environment.ExpandEnvironmentVariables(shortcut.Arguments.Replace(@"%working%", shortcut.Working))
                                        }).Save();
                
                this.Logger.InfoFormat("Created or Updated Shortcut:\r\n\tName:\t{0}\r\n\tTarget:\t{1}\r\n\tWorking:{2}\r\n\tArguments:{3}", 
                    shortcut.Name, 
                    shortcut.Target, 
                    shortcut.Working, 
                    shortcut.Arguments);

                foreach (var link in shortcut.GetLinks())
                {
                    if (!Directory.Exists(link.Key))
                    {
                        if (!Program.IsElevated)
                        {
                            this.Logger.InfoFormat("Elevating");
                            var startInfo = new ProcessStartInfo
                            {
                                UseShellExecute = true,
                                WorkingDirectory = Environment.CurrentDirectory,
                            };

                            var uri = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);
                            startInfo.FileName = uri.LocalPath;
                            startInfo.Verb = "runas";
                            Process p = Process.Start(startInfo);
                            p.WaitForExit();
                            return;
                        }
                        else
                        {
                            this.Logger.InfoFormat(@"Making link from ""{0}"" to ""{1}""", link.Key, link.Value);
                            Program.CreateSymbolicLink(link.Key, link.Value, SymbolicLink.Directory);
                        }
                    }
                }
            }
        }

        public static bool IsElevated
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        #endregion

        #region Properties

        private IConfig Config
        {
            get { return this.currentConfig; }
        }
        
        private IShortcutSource ShortcutSource
        {
            get { return this.currentShortcutSource; }
        }

        private log4net.ILog Logger
        {
            get
            {
                return this.currentLogger ?? (this.currentLogger = log4net.LogManager.GetLogger(typeof(ShortcutSource)));
            }
        }

        #endregion
    }

    public interface IProgram
    {
        #region Data Members

        void Execute();

        #endregion
    }
}