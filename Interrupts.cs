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

	public class Interrupts
	{
		public class InterruptType
		{
			public u8 Bit { get; set; }
			public u16 Address { get; set; }
		}

		public u8 If
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.IF);
			set => _gameboy.Memory.Get()[Memory.Address.IF] = value;
		}
		public u8 Ie
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.IE);
			set => _gameboy.Memory.Get()[Memory.Address.IE] = value;
		}
		public enum Types { Vblank, Lcd, Timer, Serial, Joypad };
		public InterruptType Vblank { get; set; }
		public InterruptType Lcd { get; set; }
		public InterruptType Timer { get; set; }
		public InterruptType Serial { get; set; }
		public InterruptType Joypad { get; set; }
		public InterruptType[] InterruptList { get; set; }
		public bool Ime { get; set; }
		public bool ClearIf { get; set; }
		public bool ShouldExecute { get; set; }
		public int PendingCount { get; set; }
		static bool WasHalted { get; set; }
		private readonly Gameboy _gameboy;

		public Interrupts(Gameboy gameboy)
		{
			_gameboy = gameboy;

			Init();
		}

		public void Init()
		{
			Vblank = new InterruptType { Bit = 0, Address = 0x40 };
			Lcd = new InterruptType { Bit = 1, Address = 0x48 };
			Timer = new InterruptType { Bit = 2, Address = 0x50 };
			Serial = new InterruptType { Bit = 3, Address = 0x58 };
			Joypad = new InterruptType { Bit = 4, Address = 0x60 };
			InterruptList = new InterruptType[] { Vblank, Lcd, Timer, Serial, Joypad };
			If = 0xE1;
			Ie = 0x00;
			Ime = false;
			ClearIf = true;
			ShouldExecute = true;
			PendingCount = 0;
			WasHalted = false;
		}

		// responsible for resetting a pending interrupt
		public void Reset(int id)
		{
			if (!ClearIf)
			{
				return;
			}

			_gameboy.Bit.ClearMemory(Memory.Address.IF, InterruptList[id].Bit);
		}

		// responsible for detecting if an interrupt has been requested
		public bool IsRequested(int id)
		{
			return _gameboy.Bit.Get(If, InterruptList[id].Bit) == 1;
		}

		// responsible for detecting if an interrupt has been enabled
		public bool IsEnabled(int id)
		{
			return _gameboy.Bit.Get(Ie, InterruptList[id].Bit) == 1;
		}

		// responsible for requesting an interrupt
		public void Request(int id)
		{
			_gameboy.Bit.SetMemory(Memory.Address.IF, InterruptList[id].Bit);
			If |= 0xE0;
		}

		// responsible for returning the id of the requested interrupt (if enabled)
		public int RequestedId()
		{
			for (int i = 0; i < 5; i++)
			{
				if (IsRequested(i) && IsEnabled(i))
				{
					WasHalted = _gameboy.Cpu.Halted;
					_gameboy.Cpu.Halted = false;
					return i;
				}
			}

			return -1;
		}

		// responsible for servicing an interrupt
		public void Service()
		{
			int id = RequestedId();

			if (id >= 0 && Ime)
			{
				if (ShouldExecute)
				{
					Reset(id);
					_gameboy.Memory.Push(_gameboy.Cpu.PC);

					_gameboy.Cpu.Cycles += (WasHalted) ? 24 : 20;
					_gameboy.Cpu.PC.Reg = InterruptList[id].Address;
					WasHalted = false;
					Ime = false;
				}
			}
		}
	}
}
