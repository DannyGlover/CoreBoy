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

namespace CoreBoy.CpuOps
{
	using u8 = Byte;
	using u16 = UInt16;

	public partial class CpuOps
	{
		public void Daa(int cycles)
		{
			u16 a = _gameboy.Cpu.A;

			if (_gameboy.Flags.Get(Flags.N) != 0x1)
			{
				if ((_gameboy.Flags.Get(Flags.H) == 0x1) || ((a & 0xF) > 0x09)) a += 0x06;
				if ((_gameboy.Flags.Get(Flags.C) == 0x1) || (a > 0x9F)) a += 0x60;
			}
			else
			{
				if (_gameboy.Flags.Get(Flags.H) == 0x1) a = (u8)((a - 0x06) & 0xFF);
				if (_gameboy.Flags.Get(Flags.C) == 0x1) a -= 0x60;
			}

			if ((u16)(a & 0x100) == 0x100) _gameboy.Flags.Set(Flags.C);
			a &= 0xFF;

			_gameboy.Flags.Clear(Flags.Z | Flags.H);

			if (a == 0) _gameboy.Flags.Set(Flags.Z);

			_gameboy.Cpu.A = (u8)a;
			_gameboy.Cpu.Cycles += cycles;
		}
	}
}
