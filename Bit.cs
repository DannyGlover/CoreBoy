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

namespace CoreBoy
{
	using u8 = Byte;
	using u16 = UInt16;

	public class Bit
	{
		private readonly Gameboy _gameboy;

		public Bit(Gameboy gameboy)
		{
			_gameboy = gameboy;
		}

		// responsible for getting a bits value
		public u8 Get(u8 val, u8 bit)
		{
			u8 mask = (u8)(1 << bit);
			return ((val & mask) == mask) ? (u8)1 : (u8)0;
		}

		// responsible for setting a bit
		public void Set(ref u8 val, u8 bit)
		{
			u8 mask = (u8)(1 << bit);
			val |= mask;
		}

		public void SetMemory(u16 address, u8 bit)
		{
			u8 mask = (u8)(1 << bit);
			u8 val = _gameboy.Memory.ReadByte(address);
			val |= mask;

			_gameboy.Memory.WriteByte(address, val);
		}

		// responsible for clearing a bit
		public void Clear(ref u8 val, u8 bit)
		{
			u8 mask = (u8)(1 << bit);
			val &= (u8)~mask;
		}

		public void ClearMemory(u16 address, u8 bit)
		{
			u8 mask = (u8)(1 << bit);
			u8 val = _gameboy.Memory.ReadByte(address);
			val &= (u8)~mask;

			_gameboy.Memory.WriteByte(address, val);
		}

		// responsible for determining if we half-carried
		public bool DidHalfCarry(u8 val, u8 add, u8 mask)
		{
			return (((val & mask) + (add & mask)) > mask);
		}

		// responsible for determining if we half-carried
		public bool DidHalfCarry(u16 val, u16 val2, u16 mask)
		{
			return (((val & mask) + (val2 & mask)) > mask);
		}

		// responsible for determining if we carried
		public bool DidCarry(u16 val, u16 mask)
		{
			return (val > mask);
		}

		// responsible for determining if we carried
		public bool DidCarry(int val, u16 mask)
		{
			return (val > mask);
		}
	}
}
