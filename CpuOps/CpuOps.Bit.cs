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
		public void BitTest(u8 inVal, u8 bit, int cycles)
		{
			_gameboy.Flags.Clear(Flags.Z | Flags.N);
			_gameboy.Flags.Set(Flags.H);

			if (_gameboy.Bit.Get(inVal, bit) == 0) _gameboy.Flags.Set(Flags.Z);

			_gameboy.Cpu.Cycles += cycles;
		}

		public void BitTestMem(u16 address, u8 bit, int cycles)
		{
			u8 data = _gameboy.Memory.ReadByte(address);

			BitTest(data, bit, cycles);
			_gameboy.Memory.WriteByte(address, data);
		}

		public u8 BitSet(u8 inVal, u8 bit, int cycles)
		{
			u8 result = inVal;
			_gameboy.Bit.Set(ref result, bit);
			_gameboy.Cpu.Cycles += cycles;
			return result;
		}

		public void BitSetMem(u16 address, u8 bit, int cycles)
		{
			u8 data = _gameboy.Memory.ReadByte(address);

			data = BitSet(data, bit, cycles);
			_gameboy.Memory.WriteByte(address, data);
		}

		public u8 BitClear(u8 inVal, u8 bit, int cycles)
		{
			u8 result = inVal;
			_gameboy.Bit.Clear(ref result, bit);

			_gameboy.Cpu.Cycles += cycles;
			return result;
		}

		public void BitClearMem(u16 address, u8 bit, int cycles)
		{
			u8 data = _gameboy.Memory.ReadByte(address);

			data = BitClear(data, bit, cycles);
			_gameboy.Memory.WriteByte(address, data);
		}

		public u8 BitSwap(u8 inVal, int cycles)
		{
			u8 result = (u8)(((inVal & 0xF0) >> 4) | ((inVal & 0x0F) << 4));

			_gameboy.Flags.Clear(Flags.All);

			if (result == 0) _gameboy.Flags.Set(Flags.Z);

			_gameboy.Cpu.Cycles += cycles;
			return result;
		}

		public void BitSwapMem(u16 address, int cycles)
		{
			u8 data = _gameboy.Memory.ReadByte(address);

			data = BitSwap(data, cycles);
			_gameboy.Memory.WriteByte(address, data);
		}
	}
}
