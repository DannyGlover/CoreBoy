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
using System.Diagnostics;
using System.IO;

namespace CoreBoy
{
	using u8 = Byte;
	using u16 = UInt16;

	public class Bios
	{
		public string Filename { get; set; }
		private readonly Gameboy _gameboy;

		public Bios(Gameboy gameboy)
		{
			_gameboy = gameboy;
		}

		// responsible for loading the bios
		public void Load(string filename)
		{
			if (File.Exists(filename))
			{
				Filename = filename;
				_gameboy.Cpu.DidLoadBios = true;
				u8[] bios = File.ReadAllBytes(filename);

				Console.WriteLine("loaded bios");

				Array.Copy(bios, 0, _gameboy.Memory.Get(), 0, 0x100);
			}
		}

		// responsible for reloading the bios
		public void Reload()
		{
			Load(Filename);
		}
	}
}
