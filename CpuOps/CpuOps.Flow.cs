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
		public void JmpRel(bool condition, int cycles)
		{
			s8 r8 = (s8)_gameboy.Memory.ReadByte(_gameboy.Cpu.PC.Reg);

			if (condition)
			{
				_gameboy.Cpu.PC.Reg += (u16)r8;
				_gameboy.Cpu.Cycles += 4;
			}

			_gameboy.Cpu.PC.Reg += 1;
			_gameboy.Cpu.Cycles += cycles;
		}

		public void JmpImm(bool condition, int cycles)
		{
			if (condition)
			{
				_gameboy.Cpu.PC.Reg = _gameboy.Memory.ReadWord(_gameboy.Cpu.PC.Reg);
				_gameboy.Cpu.Cycles += (cycles + 4);
				return;
			}

			_gameboy.Cpu.PC.Reg += 2;
			_gameboy.Cpu.Cycles += cycles;
		}

		public void Call(bool condition, int cycles)
		{
			if (condition)
			{
				_gameboy.Cpu.PC.Reg += 2;
				_gameboy.Memory.Push(_gameboy.Cpu.PC);
				_gameboy.Cpu.PC.Reg = _gameboy.Memory.ReadWord(_gameboy.Cpu.PC.Reg -= 2);
				_gameboy.Cpu.Cycles += (cycles + 12);
				return;
			}

			_gameboy.Cpu.PC.Reg += 2;
			_gameboy.Cpu.Cycles += cycles;
		}

		public void Ret(bool condition, int cycles)
		{
			if (condition)
			{
				_gameboy.Cpu.PC.Reg = _gameboy.Memory.Pop();
				_gameboy.Cpu.Cycles += 12;
			}

			_gameboy.Cpu.Cycles += cycles;
		}

		public void Rst(u16 address, int cycles)
		{
			_gameboy.Memory.Push(_gameboy.Cpu.PC);

			_gameboy.Cpu.PC.Reg = address;
			_gameboy.Cpu.Cycles += cycles;
		}

		// # misc # //

		public void Nop(int cycles)
		{
			_gameboy.Cpu.Cycles += cycles;
		}

		public void Stop(int cycles)
		{
			_gameboy.Cpu.Stopped = true;
			_gameboy.Cpu.Cycles += cycles;
		}

		public void Halt(int cycles)
		{
			u8 IF = _gameboy.Memory.ReadByte(Memory.Address.IF);
			u8 IE = _gameboy.Memory.ReadByte(Memory.Address.IE);

			if (!_gameboy.Interrupts.Ime)
			{
				// HALT mode is entered. It works like the IME = 1 case
				if (((IE & IF) & 0x1F) == 0)
				{
					_gameboy.Cpu.Halted = true;
					_gameboy.Interrupts.ClearIf = false;
					_gameboy.Interrupts.ShouldExecute = false;
				}
				// HALT mode is not entered. HALT bug occurs
				else
				{
					_gameboy.Interrupts.ClearIf = false;
					_gameboy.Interrupts.ShouldExecute = true;
					_gameboy.Cpu.Halted = false;
					_gameboy.Cpu.HaltBug = true;
				}
			}
			// HALT executed normally
			else
			{
				_gameboy.Cpu.Halted = true;
				_gameboy.Interrupts.ClearIf = true;
				_gameboy.Interrupts.ShouldExecute = true;
			}

			_gameboy.Cpu.Cycles += cycles;
		}

		public void DI(int cycles)
		{
			_gameboy.Interrupts.Ime = false;
			_gameboy.Cpu.Cycles += cycles;
		}

		public void EI(int cycles)
		{
			_gameboy.Cpu.PendingInterrupt = true;
			_gameboy.Cpu.Cycles += cycles;
		}
	}
}
