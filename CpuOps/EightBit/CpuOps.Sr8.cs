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
		public u8 Sr8(u8 inVal, int cycles)
		{
			u8 result = (u8)(inVal >> 1);
			u8 oldMsb = _gameboy.Bit.Get(inVal, 7);

			_gameboy.Flags.Clear(Flags.All);

			if (result == 0) _gameboy.Flags.Set(Flags.Z);
			if (_gameboy.Bit.Get(inVal, 0) == 0x1) _gameboy.Flags.Set(Flags.C);

			if (oldMsb == 0x1) _gameboy.Bit.Set(ref result, 7); else _gameboy.Bit.Clear(ref result, 7);

			_gameboy.Cpu.Cycles += cycles;
			return result;
		}

		public void Sr8Mem(u16 address, int cycles)
		{
			u8 data = _gameboy.Memory.ReadByte(address);

			data = Sr8(data, cycles);
			_gameboy.Memory.WriteByte(address, data);
		}

		public u8 Src8(u8 inVal, int cycles)
		{
			u8 result = (u8)(inVal >> 1);

			_gameboy.Flags.Clear(Flags.All);

			if (result == 0) _gameboy.Flags.Set(Flags.Z);
			if (_gameboy.Bit.Get(inVal, 0) == 0x1) _gameboy.Flags.Set(Flags.C);

			_gameboy.Cpu.Cycles += cycles;
			return result;
		}

		public void Src8Mem(u16 address, int cycles)
		{
			u8 data = _gameboy.Memory.ReadByte(address);

			data = Src8(data, cycles);
			_gameboy.Memory.WriteByte(address, data);
		}
	}
}
