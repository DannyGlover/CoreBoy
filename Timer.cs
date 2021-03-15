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

	public class Timer
	{
		// definitions
		public u8 Tima
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.TIMA);
			set => _gameboy.Memory.Get()[Memory.Address.TIMA] = value;
		}
		public u8 Tac
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.TAC);
			set => _gameboy.Memory.WriteByte(Memory.Address.TAC, value);
		}
		public u8 Tma
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.TMA);
			set => _gameboy.Memory.WriteByte(Memory.Address.TMA, value);
		}
		public u8 Div
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.DIV);
			set => _gameboy.Memory.Get()[Memory.Address.DIV] = value;
		}

		// init vars
		public int TimerCounter { get; set; }
		public int DivCounter { get; set; }
		public readonly u16[] Frequencies = { 1024, 16, 64, 256 };
		private readonly Gameboy _gameboy;

		public Timer(Gameboy gameboy)
		{
			_gameboy = gameboy;
			Init();
		}

		public void Init()
		{
			TimerCounter = 0;
			DivCounter = 0;
		}

		// responsible for getting the current frequency
		public u16 GetFrequency()
		{
			return Frequencies[Tac & 0x3];
		}

		// responsible for determining if the timer is enabled
		public bool Enabled()
		{
			return _gameboy.Bit.Get(Tac, 2) == 1;
		}

		// responsible for updating div
		public void UpdateDiv(int cycles)
		{
			DivCounter += cycles;

			if (DivCounter > 255)
			{
				Div += 1;
				DivCounter -= 256;
			}
		}

		// responsible for updating the timer
		public void Update(int cycles)
		{
			UpdateDiv(cycles);

			if (!Enabled()) return;

			u16 currentFrequency = GetFrequency();
			TimerCounter += cycles;

			if (TimerCounter >= currentFrequency)
			{
				if (((u16)Tima + 1) > 255)
				{
					Tima = Tma;
					_gameboy.Interrupts.Request((int)Interrupts.Types.Timer);
				}

				Tima += 1;
				TimerCounter -= currentFrequency;
			}
		}
	}
}
