//  Author:
//       Revolt64 <revolt64@outlook.com>
//
//  Copyright (c) 2020 Revolt64
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmulatorFrontend;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CoreBoy
{
	using ProgramInfo = MainWindow.ProgramInformation;
	using Emulator = MainWindow.EmulatorData;

	public static class Program
	{
		private static MainWindow _mainWindow;

		//[STAThread]
		static void Main(string[] args)
		{
			using (var mainWindow = new MainWindow(160, 144))
			{
				_mainWindow = mainWindow;

				// assignments
				mainWindow.ProgramInfo = new ProgramInfo
				{
					GitHubUrl = "https://github.com/revolt64",
					PatreonUrl = "https://www.patreon.com/user/creators?u=13010859",
					Title = "CoreBoy",
					Version = "0.1",
					AboutBlurb = "A Nintendo GameBoy Emulator.",
					Programmers = "Revolt64",
					DeveloperTools = "Written in C# using .NET Core.",
					Libraries = "MonoGame & ImGui.NET.",
					ThanksTo = "My amazing Wife & Kids for always believing in me."
				};
				mainWindow.Emulator = new Emulator
				{
					Memory = new int[0x10000],
					PC = 0x0000,
					ScreenWidth = 160,
					ScreenHeight = 144,
					WindowWidth = 640,
					WindowHeight = 480,
					ClearColor = Color.Black,
					ValidFileExtensions = new string[] { "*.bin", "*.BIN", "*.gb" },
					RegisterLabels = new string[] { "AF", "LCDC", "BC", "STAT", "DE", "LY", "HL", "IME", "SP", "IE", "PC", "IF", "TIMA", "DIV", "TAC", "TMA" },
					Registers = new List<Emulator.Register>
					{
						new Emulator.Register { Name = "AF", Value = 0x00, Type = Emulator.Register.RegisterTypes.SixteenBit },
						new Emulator.Register { Name = "LCDC", Value = 0x00, Type = Emulator.Register.RegisterTypes.EightBit },
						new Emulator.Register { Name = "BC", Value = 0x00, Type = Emulator.Register.RegisterTypes.SixteenBit },
						new Emulator.Register { Name = "STAT", Value = 0x00, Type = Emulator.Register.RegisterTypes.EightBit },
						new Emulator.Register { Name = "DE", Value = 0x00, Type = Emulator.Register.RegisterTypes.SixteenBit },
						new Emulator.Register { Name = "LY", Value = 0x00, Type = Emulator.Register.RegisterTypes.EightBit },
						new Emulator.Register { Name = "HL", Value = 0x00, Type = Emulator.Register.RegisterTypes.SixteenBit },
						new Emulator.Register { Name = "IME", Value = 0x00, Type = Emulator.Register.RegisterTypes.EightBit },
						new Emulator.Register { Name = "SP", Value = 0x00, Type = Emulator.Register.RegisterTypes.SixteenBit },
						new Emulator.Register { Name = "IE", Value = 0x00, Type = Emulator.Register.RegisterTypes.EightBit },
						new Emulator.Register { Name = "PC", Value = 0x00, Type = Emulator.Register.RegisterTypes.SixteenBit },
						new Emulator.Register { Name = "IF", Value = 0x00, Type = Emulator.Register.RegisterTypes.EightBit },
						new Emulator.Register { Name = "TIMA", Value = 0x00, Type = Emulator.Register.RegisterTypes.EightBit },
						new Emulator.Register { Name = "DIV", Value = 0x00, Type = Emulator.Register.RegisterTypes.EightBit },
						new Emulator.Register { Name = "TAC", Value = 0x00, Type = Emulator.Register.RegisterTypes.EightBit },
						new Emulator.Register { Name = "TMA", Value = 0x00, Type = Emulator.Register.RegisterTypes.EightBit },
					},
					Flags = new List<Emulator.Flag>
					{
						new Emulator.Flag{ Name = "Z", Value = false },
						new Emulator.Flag{ Name = "N", Value = false },
						new Emulator.Flag{ Name = "H", Value = false },
						new Emulator.Flag{ Name = "C", Value = false },
					},
					DefaultKeys = new List<EmulatorFrontend.Input.KeyDefinitions>
					{
						new EmulatorFrontend.Input.KeyDefinitions { KeyName = "DPad Left", Key = Keys.A, CtrlModifier = false },
						new EmulatorFrontend.Input.KeyDefinitions { KeyName = "DPad Right", Key = Keys.D, CtrlModifier = false },
						new EmulatorFrontend.Input.KeyDefinitions { KeyName = "DPad Up", Key = Keys.W, CtrlModifier = false },
						new EmulatorFrontend.Input.KeyDefinitions { KeyName = "DPad Down", Key = Keys.S, CtrlModifier = false },
						new EmulatorFrontend.Input.KeyDefinitions { KeyName = "A", Key = Keys.OemOpenBrackets, CtrlModifier = false },
						new EmulatorFrontend.Input.KeyDefinitions { KeyName = "B", Key = Keys.OemCloseBrackets, CtrlModifier = false },
						new EmulatorFrontend.Input.KeyDefinitions { KeyName = "Select", Key = Keys.RightShift, CtrlModifier = false },
						new EmulatorFrontend.Input.KeyDefinitions { KeyName = "Start", Key = Keys.Enter, CtrlModifier = false },
					},
					DefaultButtons = new List<EmulatorFrontend.Input.ButtonDefinitions>
					{
						new EmulatorFrontend.Input.ButtonDefinitions { ButtonName = "DPad Left", Button = Buttons.DPadLeft, SelectModifier = false },
						new EmulatorFrontend.Input.ButtonDefinitions { ButtonName = "DPad Right", Button = Buttons.DPadRight, SelectModifier = false },
						new EmulatorFrontend.Input.ButtonDefinitions { ButtonName = "DPad Up", Button = Buttons.DPadUp, SelectModifier = false },
						new EmulatorFrontend.Input.ButtonDefinitions { ButtonName = "DPad Down", Button = Buttons.DPadDown, SelectModifier = false },
						new EmulatorFrontend.Input.ButtonDefinitions { ButtonName = "A", Button = Buttons.A, SelectModifier = false },
						new EmulatorFrontend.Input.ButtonDefinitions { ButtonName = "B", Button = Buttons.B, SelectModifier = false },
						new EmulatorFrontend.Input.ButtonDefinitions { ButtonName = "Select", Button = Buttons.Back, SelectModifier = false },
						new EmulatorFrontend.Input.ButtonDefinitions { ButtonName = "Start", Button = Buttons.Start, SelectModifier = false },
					},
					SystemInputTexturePath = "Gameboy_Controller.png",
				};
				mainWindow.Emulator.Keys = mainWindow.Emulator.DefaultKeys.ToList();
				mainWindow.Emulator.Buttons = mainWindow.Emulator.DefaultButtons.ToList();

				if (args.Length > 0)
				{
					CommandLineArgs.Parse(args);

					mainWindow.ProgramSettings.General.Debugger.Enabled = CommandLineArgs.EnableDebugger;

					if (CommandLineArgs.LoadRomOnStart)
					{
						string gamePath = CommandLineArgs.RomPath;
						mainWindow.Emulator.RomTitle = Path.GetFileNameWithoutExtension(gamePath);

						if (mainWindow.ProgramSettings.General.File.RecentGames != null)
						{
							mainWindow.ProgramSettings.General.File.RecentGames.Add(gamePath);
						}

						// set the file open event and dispatch it
						mainWindow.FileEvent.Args.Phase = EmulatorFrontend.Events.FileEvent.Phase.Open;
						mainWindow.FileEvent.Args.FileType = EmulatorFrontend.Events.FileEvent.FileType.Rom;
						mainWindow.FileEvent.Args.FilePath = gamePath;
						mainWindow.FileEvent.Dispatch();
					}

					if (CommandLineArgs.WindowWidth != 0 && CommandLineArgs.WindowHeight != 0)
					{
						mainWindow.UpdateWindowSize(CommandLineArgs.WindowWidth, CommandLineArgs.WindowHeight);
					}

					if (CommandLineArgs.UseFullScreen)
					{
						mainWindow.SetFullScreen();
					}
				}

				var gameboy = new Gameboy(mainWindow);
				mainWindow.Run();
			}
		}
	}
}
