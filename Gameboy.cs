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
using System.IO;
using System.IO.Compression;
using EmulatorFrontend;
using EmulatorFrontend.Events;
using Microsoft.Xna.Framework;

namespace CoreBoy
{
	public class Gameboy
	{
		public MainWindow Window { get; set; }
		public Bit Bit { get; set; }
		public Memory Memory { get; set; }
		public Bios Bios { get; set; }
		public Rom Rom { get; set; }
		public Mbc Mbc { get; set; }
		public Ppu Ppu { get; set; }
		public Interrupts Interrupts { get; set; }
		public Timer Timer { get; set; }
		public Flags Flags { get; set; }
		public Input Input { get; set; }
		public Cpu Cpu { get; set; }

		public Gameboy(MainWindow window)
		{
			Window = window;
			Bit = new Bit(this);
			Memory = new Memory(this);
			Bios = new Bios(this);
			Rom = new Rom(this);
			Mbc = new Mbc(this);
			Ppu = new Ppu(this);
			Interrupts = new Interrupts(this);
			Timer = new Timer(this);
			Flags = new Flags(this);
			Input = new Input(this);
			Cpu = new Cpu(this);
			window.Emulator.MainLoop += Run;
			window.Emulator.OnExiting += OnExiting;
			window.FileEvent.EventHandler += File_Event;
			window.EmulatorEvent.EventHandler += Emulator_Event;
		}

		public void Run()
		{
			if (Window.Emulator.Running)
			{
				Cpu.Cycles = 0;

				while (Cpu.Cycles < (4194304 / 60))
				{
					Cpu.Step();
				}

				Input.Listen();
			}

			UpdateFrontend();

			if (Window.Input.IsKeyHeld(Microsoft.Xna.Framework.Input.Keys.F))
			{
				Cpu.Step();
			}
		}

		public void OnExiting()
		{
			Rom.SaveRam(0);
		}

		private void UpdateFrontend()
		{
			Window.Emulator.Registers[0].Value = Cpu.AF.Reg;
			Window.Emulator.Registers[1].Value = Ppu.Lcdc;
			Window.Emulator.Registers[2].Value = Cpu.BC.Reg;
			Window.Emulator.Registers[3].Value = Ppu.Stat;
			Window.Emulator.Registers[4].Value = Cpu.DE.Reg;
			Window.Emulator.Registers[5].Value = Ppu.Ly;
			Window.Emulator.Registers[6].Value = Cpu.HL.Reg;
			Window.Emulator.Registers[7].Value = Convert.ToInt32(Interrupts.Ime);
			Window.Emulator.Registers[8].Value = Cpu.SP.Reg;
			Window.Emulator.Registers[9].Value = Interrupts.Ie;
			Window.Emulator.Registers[10].Value = Cpu.PC.Reg;
			Window.Emulator.Registers[11].Value = Interrupts.If;
			Window.Emulator.Registers[12].Value = Timer.Tima;
			Window.Emulator.Registers[13].Value = Timer.Div;
			Window.Emulator.Registers[14].Value = Timer.Tac;
			Window.Emulator.Registers[15].Value = Timer.Tma;
			Window.Emulator.Flags[0].Value = Flags.Get(Flags.Z) == 1;
			Window.Emulator.Flags[1].Value = Flags.Get(Flags.N) == 1;
			Window.Emulator.Flags[2].Value = Flags.Get(Flags.H) == 1;
			Window.Emulator.Flags[3].Value = Flags.Get(Flags.C) == 1;
			Window.Emulator.PC = Cpu.PC.Reg;
			Window.Emulator.InstructionsRan = Cpu.InstructionsRan;
			Array.Copy(Memory.Get(), Window.Emulator.Memory, Memory.Get().Length);
		}

		private void Reset(bool reloadRom = false)
		{
			Rom.Init();
			Ppu.Init();
			Interrupts.Init();
			Timer.Init();
			Input.Init();
			Memory.Init();
			//Bios.Load(args.FilePath);

			if (reloadRom)
			{
				Rom.Reload();
			}

			Cpu.Init();
		}

		private void SaveState(int num)
		{
			if (!Directory.Exists("Saves"))
			{
				Directory.CreateDirectory("Saves");
			}

			if (!Directory.Exists("Saves/States"))
			{
				Directory.CreateDirectory("Saves/States");
			}

			if (!Directory.Exists($"Saves/States/{Rom.RomName}"))
			{
				Directory.CreateDirectory($"Saves/States/{Rom.RomName}");
			}

			if (!Directory.Exists($"Saves/States/{Rom.RomName}/{num}"))
			{
				Directory.CreateDirectory($"Saves/States/{Rom.RomName}/{num}");
			}

			string stateFolderPath = $"Saves/States/{Rom.RomName}/{num}";
			string statePath = $"{stateFolderPath}/state.bin";
			string stateZipPath = $"Saves/States/{Rom.RomName}/state_{num}.zip";

			// write registers
			using (var fileStream = new FileStream(statePath, FileMode.Create))
			{
				using (var binaryWriter = new BinaryWriter(fileStream))
				{
					binaryWriter.Write(Cpu.AF.Reg);
					binaryWriter.Write(Cpu.BC.Reg);
					binaryWriter.Write(Cpu.DE.Reg);
					binaryWriter.Write(Cpu.HL.Reg);
					binaryWriter.Write(Cpu.SP.Reg);
					binaryWriter.Write(Cpu.PC.Reg);
					binaryWriter.Write(Cpu.Cycles);
					binaryWriter.Write(Cpu.InstructionsRan);
					binaryWriter.Write(Cpu.Halted);
					binaryWriter.Write(Cpu.HaltBug);
					binaryWriter.Write(Cpu.Stopped);
					binaryWriter.Write(Cpu.PendingInterrupt);
					binaryWriter.Write(Rom.CurrentMode);
					binaryWriter.Write(Rom.RomBank);
					binaryWriter.Write(Rom.RamBank);
					binaryWriter.Write(Memory.UseRomBank);
					binaryWriter.Write(Memory.UseRamBank);
					binaryWriter.Write(Ppu.ScanlineCounter);
					binaryWriter.Write(Timer.TimerCounter);
					binaryWriter.Write(Timer.DivCounter);
					binaryWriter.Write(Interrupts.Ime);
					binaryWriter.Write(Interrupts.ClearIf);
					binaryWriter.Write(Interrupts.ShouldExecute);
					binaryWriter.Write(Interrupts.PendingCount);
					binaryWriter.Write(Memory.Get());
					binaryWriter.Write(Rom.Ram);

					for (int y = 0; y < 144; y++)
					{
						for (int x = 0; x < 160; x++)
						{
							byte[] color = new byte[]
							{
								Ppu.Screen[y * 160 + x].R, Ppu.Screen[y * 160 + x].G, Ppu.Screen[y * 160 + x].B, Ppu.Screen[y * 160 + x].A
							};
							binaryWriter.Write(color);
						}
					}
				}
			}

			// compress & remove directory
			if (File.Exists(stateZipPath))
			{
				File.Delete(stateZipPath);
			}

			ZipFile.CreateFromDirectory(stateFolderPath, stateZipPath, CompressionLevel.Fastest, false);
			Directory.Delete(stateFolderPath, true);
		}

		private void LoadState(int num)
		{
			string stateFolderPath = $"Saves/States/{Rom.RomName}/.current";
			string statePath = $"{stateFolderPath}/state.bin";
			string stateZipPath = $"Saves/States/{Rom.RomName}/state_{num}.zip";

			if (!Directory.Exists(stateFolderPath))
			{
				Directory.CreateDirectory(stateFolderPath);
			}
			else
			{
				Directory.Delete(stateFolderPath, true);
			}

			// decompress to .current folder
			ZipFile.ExtractToDirectory(stateZipPath, stateFolderPath);

			// read registers
			using (var fileStream = new FileStream(statePath, FileMode.Open))
			{
				using (var binaryReader = new BinaryReader(fileStream))
				{
					Cpu.AF.Reg = binaryReader.ReadUInt16();
					Cpu.BC.Reg = binaryReader.ReadUInt16();
					Cpu.DE.Reg = binaryReader.ReadUInt16();
					Cpu.HL.Reg = binaryReader.ReadUInt16();
					Cpu.SP.Reg = binaryReader.ReadUInt16();
					Cpu.PC.Reg = binaryReader.ReadUInt16();
					Cpu.Cycles = binaryReader.ReadInt32();
					Cpu.InstructionsRan = binaryReader.ReadInt32();
					Cpu.Halted = binaryReader.ReadBoolean();
					Cpu.HaltBug = binaryReader.ReadBoolean();
					Cpu.Stopped = binaryReader.ReadBoolean();
					Cpu.PendingInterrupt = binaryReader.ReadBoolean();
					Rom.CurrentMode = binaryReader.ReadByte();
					Rom.RomBank = binaryReader.ReadUInt16();
					Rom.RamBank = binaryReader.ReadByte();
					Memory.UseRomBank = binaryReader.ReadBoolean();
					Memory.UseRamBank = binaryReader.ReadBoolean();
					Ppu.ScanlineCounter = binaryReader.ReadInt32();
					Timer.TimerCounter = binaryReader.ReadInt32();
					Timer.DivCounter = binaryReader.ReadInt32();
					Interrupts.Ime = binaryReader.ReadBoolean();
					Interrupts.ClearIf = binaryReader.ReadBoolean();
					Interrupts.ShouldExecute = binaryReader.ReadBoolean();
					Interrupts.PendingCount = binaryReader.ReadInt32();
					// load memory
					Array.Copy(binaryReader.ReadBytes(Memory.Get().Length), Memory.Get(), Memory.Get().Length);
					// load ram
					Array.Copy(binaryReader.ReadBytes(Rom.Ram.Length), Rom.Ram, Rom.Ram.Length);
					// load screen
					for (int y = 0; y < 144; y++)
					{
						for (int x = 0; x < 160; x++)
						{
							byte[] color = binaryReader.ReadBytes(4);
							Color screenColor = new Color(color[0], color[1], color[2], color[3]);

							Ppu.Screen[y * 160 + x] = screenColor;
						}
					}
				}
			}
		}

		private void Emulator_Event(object sender, EmulatorEvent.EventArgs args)
		{
			switch (args.Phase)
			{
				case EmulatorEvent.Phase.Run:
					Window.Emulator.Running = true;
					break;
				case EmulatorEvent.Phase.Stop:
					Window.Emulator.Running = false;
					File.WriteAllLines("coreBoyRun.log", Cpu.Log);
					break;
				case EmulatorEvent.Phase.Pause:
					Window.Emulator.Running = false;
					break;
				case EmulatorEvent.Phase.Reset:
					Reset(true);
					break;
				case EmulatorEvent.Phase.StepForward:
					Cpu.Step();
					UpdateFrontend();
					break;
			}
		}

		private void File_Event(object sender, FileEvent.EventArgs args)
		{
			switch (args.Phase)
			{
				case FileEvent.Phase.Open:
					switch (args.FileType)
					{
						case FileEvent.FileType.Rom:
							Window.Emulator.Running = false;
							Reset();
							//Bios.Load(args.FilePath);
							Rom.Load(args.FilePath);
							Window.ProgramSettings.Save();
							break;

						case FileEvent.FileType.SaveState:
							switch (args.FileType)
							{
								case FileEvent.FileType.SaveState:
									LoadState(args.StateNumber);
									break;
							}
							break;
					}
					break;

				case FileEvent.Phase.Save:
					switch (args.FileType)
					{
						case FileEvent.FileType.SaveState:
							SaveState(args.StateNumber);
							break;
					}
					break;
			}
		}
	}
}
