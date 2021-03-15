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

	public class Memory
	{
		public struct Address
		{
			public const u16 LCDC = 0xFF40;
			public const u16 STAT = 0xFF41;
			public const u16 SCY = 0xFF42;
			public const u16 SCX = 0xFF43;
			public const u16 WY = 0xFF4A;
			public const u16 WX = 0xFF4B;
			public const u16 LY = 0xFF44;
			public const u16 LYC = 0xFF45;
			public const u16 BGP = 0xFF47;
			public const u16 OP0 = 0xFF48;
			public const u16 OP1 = 0xFF49;
			public const u16 DMA = 0xFF46;
			public const u16 P1 = 0xFF00;
			public const u16 SERIAL = 0xFF01;
			public const u16 SERIAL_CTRL = 0xFF02;
			public const u16 DIV = 0xFF04;
			public const u16 TIMA = 0xFF05;
			public const u16 TMA = 0xFF06;
			public const u16 TAC = 0xFF07;
			public const u16 IE = 0xFFFF;
			public const u16 IF = 0xFF0F;
			public const u16 NR10 = 0xFF10;
			public const u16 NR11 = 0xFF11;
			public const u16 NR12 = 0xFF12;
			public const u16 NR14 = 0xFF14;
			public const u16 NR21 = 0xFF16;
			public const u16 NR22 = 0xFF17;
			public const u16 NR24 = 0xFF19;
			public const u16 NR30 = 0xFF1A;
			public const u16 NR31 = 0xFF1B;
			public const u16 NR32 = 0xFF1C;
			public const u16 NR33 = 0xFF1E;
			public const u16 NR41 = 0xFF20;
			public const u16 NR42 = 0xFF21;
			public const u16 NR43 = 0xFF22;
			public const u16 NR50 = 0xFF24;
			public const u16 NR51 = 0xFF25;
			public const u16 NR52 = 0xFF26;
			public const u16 WRAM_START = 0xC000;
			public const u16 WRAM_END = 0xDDFF;
			public const u16 ERAM_START = 0xE000;
			public const u16 ERAM_END = 0xFDFF;
			public const u16 PROT_MEM_START = 0xFEA0;
			public const u16 PROT_MEM_END = 0xFEFF;
			public const u16 HRAM_START = 0xFF80;
			public const u16 HRAM_END = 0xFFFE;
			public const u16 UNMAPPED_START = 0xFF4C;
			public const u16 UNMAPPED_END = 0xFF7F;
			public const u16 ROM_BK0_START = 0x0000;
			public const u16 ROM_BK0_END = 0x3FFF;
			public const u16 ROM_BK1_START = 0x4000;
			public const u16 ROM_BK1_END = 0x7FFF;
			public const u16 EXTRAM_START = 0xA000;
			public const u16 EXTRAM_END = 0xBFFF;
			public const u16 ROM_NAME_START = 0x0134;
			public const u16 ROM_NAME_END = 0x0143;
			public const u16 ROM_TYPE = 0x0147;
			public const u16 ROM_SIZE = 0x0148;
			public const u16 ROM_RAM_SIZE = 0x0149;
		}
		public bool UseRomBank { get; set; }
		public bool UseRamBank { get; set; }
		private u8[] _mem;
		private readonly Gameboy _gameboy;

		public Memory(Gameboy gameboy)
		{
			_gameboy = gameboy;
			Init();
		}

		public void Init()
		{
			UseRomBank = true;
			UseRamBank = false;
			_mem = new u8[0x10000];
			_mem[Address.DIV] = 0xAB;
			_mem[Address.TIMA] = 0x00;
			_mem[Address.TMA] = 0x00;
			_mem[Address.TAC] = 0xF8;
			_mem[Address.LCDC] = 0x91;
			_mem[Address.STAT] = 0x85;
			_mem[Address.SCY] = 0x00;
			_mem[Address.SCX] = 0x00;
			_mem[Address.LY] = 0x90;
			_mem[Address.LYC] = 0x00;
			_mem[Address.BGP] = 0xBF;
			_mem[Address.OP0] = 0xFF;
			_mem[Address.OP1] = 0xFF;
			_mem[Address.WY] = 0x00;
			_mem[Address.WX] = 0x00;
			_mem[Address.P1] = 0xFF;
			_mem[Address.IF] = 0xE1;
			_mem[Address.IE] = 0x00;
			_mem[Address.NR10] = 0x80;
			_mem[Address.NR11] = 0xBF;
			_mem[Address.NR12] = 0xF3;
			_mem[Address.NR14] = 0xBF;
			_mem[Address.NR21] = 0x3F;
			_mem[Address.NR22] = 0x00;
			_mem[Address.NR24] = 0xBF;
			_mem[Address.NR30] = 0x7F;
			_mem[Address.NR31] = 0xFF;
			_mem[Address.NR32] = 0x9F;
			_mem[Address.NR33] = 0xBF;
			_mem[Address.NR41] = 0xFF;
			_mem[Address.NR42] = 0xFF;
			_mem[Address.NR43] = 0x00;
			_mem[Address.NR50] = 0x77;
			_mem[Address.NR51] = 0xF3;
			_mem[Address.NR52] = 0xF1;
		}

		public u8[] Get()
		{
			return _mem;
		}

		// responsible for reading a byte from a specific memory location
		public u8 ReadByte(u16 address)
		{
			switch (address)
			{
				case u16 addr when addr >= Address.ROM_BK1_START && addr <= Address.ROM_BK1_END:
					{
						int bankAddr = ((_gameboy.Rom.RomBank * 0x4000) + (address - 0x4000));

						if (!UseRomBank)
						{
							bankAddr = (((_gameboy.Rom.RomBank & _gameboy.Mbc.GetMaxBankSize()) * 0x4000) + (address - 0x4000));
						}

						return _gameboy.Rom.Read(bankAddr);
					}
				case u16 addr when addr >= Address.EXTRAM_START && addr <= Address.EXTRAM_END:
					{
						if (UseRamBank)
						{
							u8 ramBank = (_gameboy.Rom.CurrentMode == 0x0) ? (u8)0x0 : (u8)_gameboy.Rom.RamBank;

							return _gameboy.Rom.Ram[((ramBank * 0x2000) + (address - Address.EXTRAM_START))];
						}
						else
						{
							return 0xFF;
						}
					}
				case u16 addr when addr >= Address.PROT_MEM_START && addr <= Address.PROT_MEM_END:
					{
						return 0xFF;
					}
				case Address.P1: return _gameboy.Input.GetKey(_mem[address]);
				// sound registers
				case Address.NR10: return 0xFF;
				case Address.NR11: return 0xFF;
				case Address.NR12: return 0xFF;
				case Address.NR14: return 0xFF;
				case Address.NR21: return 0xFF;
				case Address.NR22: return 0xFF;
				case Address.NR24: return 0xFF;
				case Address.NR30: return 0xFF;
				case Address.NR31: return 0xFF;
				case Address.NR32: return 0xFF;
				case Address.NR33: return 0xFF;
				case Address.NR41: return 0xFF;
				case Address.NR42: return 0xFF;
				case Address.NR43: return 0xFF;
				case Address.NR50: return 0xFF;
				case Address.NR51: return 0xFF;
				case Address.NR52: return 0xFF;
				default:
					break;
			}

			return _mem[address];
		}

		public u16 ReadWord(u16 address)
		{
			if (address >= Address.ROM_BK1_START && address <= Address.ROM_BK1_END)
			{
				int bankAddr = ((_gameboy.Rom.RomBank * 0x4000) + (address - 0x4000));

				if (!UseRomBank)
				{
					bankAddr = (((_gameboy.Rom.RomBank & _gameboy.Mbc.GetMaxBankSize()) * 0x4000) + (address - 0x4000));
				}

				return (u16)((_gameboy.Rom.Read(bankAddr + 1) << 8) | (_gameboy.Rom.Read(bankAddr)));
			}

			return (u16)((_mem[address + 1] << 8) | (_mem[address]));
		}

		public void WriteByte(u16 address, u8 data)
		{
			switch (address)
			{
				// disable writes to protected memory
				case u16 addr when addr >= Address.PROT_MEM_START && addr <= Address.PROT_MEM_END: break;

				// if writing to work ram, write to echo ram
				case u16 addr when addr >= Address.WRAM_START && addr <= Address.WRAM_END:
					_mem[address] = data;
					_mem[address + 0x2000] = data;
					break;

				// if writing to echo ram, write to work ram
				case u16 addr when addr >= Address.ERAM_START && addr <= Address.ERAM_END:
					_mem[address] = data;
					_mem[address - 0x2000] = data;
					break;

				// if writing specific data to unmapped memory
				case u16 addr when addr >= Address.UNMAPPED_START && addr <= Address.UNMAPPED_END:
					// copy rom back over bios
					if (data == 0x1)
					{
						_gameboy.Rom.Reload();
					}
					break;

				// handle enabling ram banking
				case u16 addr when addr >= 0x0000 && addr <= 0x1FFF:
					UseRamBank = ((data & 0xF) == 0xA) ? true : false;
					break;

				// rom banking
				case u16 addr when addr >= 0x2000 && addr <= 0x3FFF:
					_gameboy.Mbc.RomBanking(address, data);
					break;

				// manage rom banking
				case u16 addr when addr >= 0x4000 && addr <= 0x7FFF:
					_gameboy.Mbc.ManageBanking(address, data);
					break;

				// handle external ram
				case u16 addr when addr >= Address.EXTRAM_START && addr <= Address.EXTRAM_END:
					if (UseRamBank)
					{
						u8 ramBank = (_gameboy.Rom.CurrentMode == 0x0) ? (u8)0x0 : _gameboy.Rom.RamBank;
						_gameboy.Rom.Ram[((ramBank * 0x2000) + (address - Address.EXTRAM_START))] = data;
					}
					break;

				// disable writes to LY (should never happen...)
				case Address.LY: break;

				// handle DMA writes
				case Address.DMA:
					{
						u16 location = (u16)(data << 8);

						for (u16 i = 0; i < 0xA0; i++)
						{
							_mem[0xFE00 + i] = (u8)ReadByte((u16)(location + i));
						}
					}
					break;

				// reset DIV if it is written to
				case Address.DIV:
					{
						_mem[address] = 0x00;
					}
					break;

				// read from the serial port (useful for blarggs cpu tests)
				case Address.SERIAL_CTRL:
					{
						if (data == 0x81)
						{
							Console.Write("{0}", (char)ReadByte(Address.SERIAL));
						}

						_mem[address] = data;
					}
					break;

				// everything should be ok to write to memory here...
				default:
					_mem[address] = data;
					break;
			}
		}

		public void WriteWord(u16 address, Cpu.Register reg)
		{
			_mem[address] = reg.Lo;
			_mem[address + 1] = reg.Hi;
		}

		public u16 Pop()
		{
			u16 data = ReadWord(_gameboy.Cpu.SP.Reg);
			_gameboy.Cpu.SP.Reg += 2;

			return data;
		}

		public void Push(Cpu.Register reg)
		{
			_gameboy.Cpu.SP.Reg -= 1;
			_mem[_gameboy.Cpu.SP.Reg] = reg.Hi;
			_gameboy.Cpu.SP.Reg -= 1;
			_mem[_gameboy.Cpu.SP.Reg] = reg.Lo;
		}
	}
}
