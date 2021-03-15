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

	public class Flags
	{
		public const u8 Z = 0x80;
		public const u8 N = 0x40;
		public const u8 H = 0x20;
		public const u8 C = 0x10;
		public const u8 All = (Z | N | H | C);
		private u8 F
		{
			get => _gameboy.Cpu.F;
			set => _gameboy.Cpu.F = value;
		}
		private readonly Gameboy _gameboy;

		public Flags(Gameboy gameboy)
		{
			_gameboy = gameboy;
		}

		// responsible for getting a flags value
		public u8 Get(u8 flag)
		{
			return ((F & flag) == flag) ? (u8)1 : (u8)0;
		}

		// responsible for setting a flag
		public void Set(u8 flags)
		{
			F |= flags;
		}

		// responsible for clearing a flag
		public void Clear(u8 flags)
		{
			F &= (u8)~flags;
		}
	}
}
