﻿// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Illallangi Enterprises">
// Copyright (C) 2013 Illallangi Enterprises
// </copyright>
// -----------------------------------------------------------------------

using System;
using Illallangi.ShellLink;

namespace Illallangi.DropBox.StartMenu
{
    using System.IO;

    using Ninject;

    /// <summary>
    /// The Program.
    /// </summary>
    public sealed class Program : IProgram
    {
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
                (new ShellShortcut(Path.Combine(this.Config.ProgramsPath, string.Format("{0}.{1}", shortcut.Name, "lnk")))
                                        {
                                            Path = Path.Combine(shortcut.Working, shortcut.Target),
                                            WorkingDirectory = shortcut.Working,
                                            IconPath = Path.Combine(shortcut.Working, shortcut.IconPath),
                                            IconIndex = shortcut.IconIndex,
                                        }).Save();
                
                this.Logger.InfoFormat("Created or Updated Shortcut:\r\n\tName:\t{0}\r\n\tTarget:\t{1}\r\n\tWorking:{2}", shortcut.Name, shortcut.Target, shortcut.Working);
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