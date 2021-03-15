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

namespace CoreBoy
{
	using u8 = Byte;
	using u16 = UInt16;

	public class Rom
	{
		public u8[] Ram { get; set; }
		public u8 MbcType { get; set; }
		public u8 RomSize { get; set; }
		public u8 RamSize { get; set; }
		public u16 RomBank { get; set; }
		public u8 RamBank { get; set; }
		public u8 CurrentMode { get; set; }
		public bool HasBatteryBackup { get; set; }
		public string Filename { get; set; }
		public string RomName { get; set; }
		private u8[] _rom { get; set; }
		private readonly Gameboy _gameboy;

		public Rom(Gameboy gameboy)
		{
			_gameboy = gameboy;
			Ram = new u8[0x2000 * 16];
			_rom = new u8[0x4000 * 512];
			Init();
		}

		public void Init()
		{
			MbcType = 0x00;
			RomSize = 0x00;
			RamSize = 0x00;
			RomBank = 0x01;
			RamBank = 0x00;
			CurrentMode = 0x00;
			HasBatteryBackup = false;
			Array.Clear(Ram, 0, Ram.Length);
			Array.Clear(_rom, 0, _rom.Length);
		}

		public u8 Read(int addresss)
		{
			return _rom[addresss];
		}

		public void Write(int address, u8 data)
		{
			_rom[address] = data;
		}

		public void Load(string filename)
		{
			if (File.Exists(filename))
			{
				u8[] rom = File.ReadAllBytes(filename);
				RomBank = 0x01;
				RamSize = 0x00;
				CurrentMode = 0x00;
				Filename = filename;

				// load the rom into the _rom array
				Array.Copy(rom, _rom, rom.Length);
				// load the first rom bank into memory
				Array.Copy(_rom, _gameboy.Memory.Get(), 0x3FFF);

				MbcType = _gameboy.Memory.ReadByte(Memory.Address.ROM_TYPE);
				RomSize = _gameboy.Memory.ReadByte(Memory.Address.ROM_SIZE);
				RamSize = _gameboy.Memory.ReadByte(Memory.Address.ROM_RAM_SIZE);
				RomName = "";

				switch (MbcType)
				{
					case 0x3:
					case 0x6:
					case 0x9:
					case 0xD:
					case 0x13:
					case 0x1B:
					case 0x1E:
					case 0x20:
					case 0x22:
					case 0xFF:
						HasBatteryBackup = true;
						break;
				}

				for (u16 i = Memory.Address.ROM_NAME_START; i < Memory.Address.ROM_NAME_END; i++)
				{
					if (_gameboy.Memory.ReadByte(i) != 0)
					{
						RomName += (char)_gameboy.Memory.ReadByte(i);
					}
				}

				_gameboy.Window.Emulator.RomTitle = RomName;

				Console.WriteLine($"{RomName} - Mbc Type: {MbcType:X2} | Rom Size: {RomSize:X2} | Ram Size: {RamSize:X2}");

				LoadRam(0);
			}
		}

		public void Reload()
		{
			Load(Filename);
		}

		// responsible for loading the games ram bank from a file
		public void LoadRam(int num)
		{
			if (!HasBatteryBackup) return;

			string savePath = String.Format("Saves/{0}/{1}.sav", RomName, num);

			if (File.Exists(savePath))
			{
				u8[] ram = File.ReadAllBytes(savePath);
				Array.Copy(ram, Ram, ram.Length);
			}
		}

		// responsible for saving the games ram bank to a file
		public void SaveRam(int num)
		{
			if (!HasBatteryBackup) return;

			string savePath = String.Format("Saves/{0}/{1}.sav", RomName, num);

			if (!Directory.Exists("Saves"))
			{
				Directory.CreateDirectory("Saves");
			}

			if (!Directory.Exists($"Saves/{RomName}"))
			{
				Directory.CreateDirectory($"Saves/{RomName}");
			}

			File.WriteAllBytes(savePath, Ram);
		}
	}
}
