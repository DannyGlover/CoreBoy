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

	public class Mbc
	{
		public class MBCAddressRange
		{
			public u8 Start { get; set; }
			public u8 End { get; set; }
		}
		public MBCAddressRange MBC1AddressRange { get; set; }
		public MBCAddressRange MBC2AddressRange { get; set; }
		public MBCAddressRange MBC3AddressRange { get; set; }
		public MBCAddressRange MBC5AddressRange { get; set; }
		private readonly u16[] _maxSize;
		private readonly Mbc1 _mbc1;
		private readonly Gameboy _gameboy;

		public Mbc(Gameboy gameboy)
		{
			_gameboy = gameboy;
			MBC1AddressRange = new MBCAddressRange { Start = 0x1, End = 0x3 };
			MBC2AddressRange = new MBCAddressRange { Start = 0x5, End = 0x6 };
			MBC3AddressRange = new MBCAddressRange { Start = 0x12, End = 0x13 };
			MBC5AddressRange = new MBCAddressRange { Start = 0x19, End = 0x1E };
			_maxSize = new u16[0x9]
			{
				0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200
			};
			_mbc1 = new Mbc1(gameboy);
		}

		// responsible for returning the rom banks maximum size
		public u16 GetMaxBankSize()
		{
			return (u16)(_maxSize[_gameboy.Rom.RomSize] - 0x1);
		}

		// responsible for managing rom banking
		public void RomBanking(u16 address, u8 data)
		{
			switch (_gameboy.Rom.MbcType)
			{
				// MBC1
				case u8 mcbAddr when mcbAddr >= MBC1AddressRange.Start && mcbAddr <= MBC1AddressRange.End:
					_mbc1.RomBanking(address, data);
					break;

				default:
					Console.WriteLine("WARNING: RomBanking() MBC type not handled");
					break;
			}
		}

		// responsible for managing banking
		public void ManageBanking(u16 address, u8 data)
		{
			switch (address)
			{
				// handle selecting ram bank/upper two bits of rom bank
				case u16 addr when addr >= 0x4000 && addr <= 0x5FFF:
					switch (_gameboy.Rom.MbcType)
					{
						case u8 mcbAddr when mcbAddr >= MBC1AddressRange.Start && mcbAddr <= MBC1AddressRange.End:
							_mbc1.ManageSelection(data);
							break;

						default:
							Console.WriteLine("WARNING: ManageBanking() MBC type not handled");
							break;
					}
					break;

				// handle rom/ram mode
				case u16 addr when addr >= 0x6000 && addr <= 0x7FFF:
					switch (_gameboy.Rom.MbcType)
					{
						case u8 mcbAddr when mcbAddr >= MBC1AddressRange.Start && mcbAddr <= MBC1AddressRange.End:
							_mbc1.ManageMode(data);
							break;

						default:
							Console.WriteLine("WARNING: ManageBanking() MBC type not handled");
							break;
					}
					break;
			}
		}
	}
}
