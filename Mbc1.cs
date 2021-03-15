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

	public class Mbc1
	{
		private readonly Gameboy _gameboy;

		public Mbc1(Gameboy gameboy)
		{
			_gameboy = gameboy;
		}

		// responsible for managing MBC1 rom banking
		public void RomBanking(u16 address, u8 data)
		{
			u8 bankNo = (u8)(data & 0x1F);

			if (bankNo == 0x00 || bankNo == 0x20 || bankNo == 0x40 || bankNo == 0x60)
			{
				bankNo += 0x1;
			}

			_gameboy.Rom.RomBank = bankNo;
		}

		// responsible for managing the bank selection(s)
		public void ManageSelection(u8 data)
		{
			// 0 = 16/8 mode || 1 = 4/32 mode
			if (_gameboy.Rom.CurrentMode == 0x0)
			{
				u16 romBankMask = _gameboy.Mbc.GetMaxBankSize();
				_gameboy.Rom.RomBank &= romBankMask;
				_gameboy.Rom.RomBank |= (u16)(((data & 0x3) << 5) & romBankMask);
			}
			else
			{
				// only ram sizes 0x3 and 0x4 have more than one ram bank
				if (_gameboy.Rom.RamSize > 0x2) _gameboy.Rom.RamBank = (u8)(data & 0x3);
			}
		}

		// responsible for managing the bank mode(s)
		public void ManageMode(u8 data)
		{
			_gameboy.Rom.CurrentMode = (u8)(data & 0x1);
			_gameboy.Memory.UseRomBank = (_gameboy.Rom.CurrentMode == 0x0);
		}
	}
}
