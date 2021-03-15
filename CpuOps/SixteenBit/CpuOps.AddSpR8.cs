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
	using s8 = SByte;
	using u16 = UInt16;

	public partial class CpuOps
	{
		public void AddSpR8(int cycles)
		{
			s8 r8 = (s8)_gameboy.Memory.ReadByte(_gameboy.Cpu.PC.Reg);

			_gameboy.Flags.Clear(Flags.All);

			if (_gameboy.Bit.DidHalfCarry(_gameboy.Cpu.SP.Lo, (u8)r8, 0xF)) _gameboy.Flags.Set(Flags.H);
			if (_gameboy.Bit.DidCarry(_gameboy.Cpu.SP.Lo + (u8)r8, 0xFF)) _gameboy.Flags.Set(Flags.C);

			_gameboy.Cpu.SP.Reg = (u16)(_gameboy.Cpu.SP.Reg + r8);
			_gameboy.Cpu.Cycles += cycles;
		}
	}
}
