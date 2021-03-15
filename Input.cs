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

	public class Input
	{
		public const u8 P15 = 5; // buttons
		public const u8 P14 = 4; // directional keys
		public const u8 ButtonStart = 7;
		public const u8 ButtonSelect = 6;
		public const u8 ButtonB = 5;
		public const u8 ButtonA = 4;
		public const u8 DirectionDown = 3;
		public const u8 DirectionUp = 2;
		public const u8 DirectionLeft = 1;
		public const u8 DirectionRight = 0;
		public u8 Buttons = 0xFF;
		private EmulatorFrontend.Input _frontEndInput => _gameboy.Window.Input;
		private readonly Gameboy _gameboy;

		public Input(Gameboy gameboy)
		{
			_gameboy = gameboy;
			Init();
		}

		public void Init()
		{
			Buttons = 0xFF;
		}

		// responsible for pressing a directional key
		public void PressDirection(u8 bit, u8 keyType)
		{
			bool wasNotSet = _gameboy.Bit.Get(Buttons, bit) == 0;

			switch (bit)
			{
				case DirectionDown:
					ReleaseKey(DirectionUp);
					break;
				case DirectionUp:
					ReleaseKey(DirectionDown);
					break;
				case DirectionLeft:
					ReleaseKey(DirectionRight);
					break;
				case DirectionRight:
					ReleaseKey(DirectionLeft);
					break;
			}

			_gameboy.Cpu.Stopped = false;
			_gameboy.Bit.Clear(ref Buttons, bit);

			if (wasNotSet)
			{
				_gameboy.Interrupts.Request((int)Interrupts.Types.Joypad);
			}
		}

		// responsible for pressing a button key
		public void PressButton(u8 bit, u8 keyType)
		{
			bool wasSet = _gameboy.Bit.Get(Buttons, bit) == 1;

			_gameboy.Cpu.Stopped = false;
			_gameboy.Bit.Clear(ref Buttons, bit);

			if (wasSet)
			{
				_gameboy.Interrupts.Request((int)Interrupts.Types.Joypad);
			}
		}

		// responsible for releasing a key
		public void ReleaseKey(u8 bit)
		{
			_gameboy.Bit.Set(ref Buttons, bit);
		}

		// responsible for retrieving the currently pressed key
		public u8 GetKey(u8 data)
		{
			// buttons
			if (_gameboy.Bit.Get(data, P15) == 0)
			{
				return (u8)((Buttons >> 4) | (0xF0));
			}
			// directional keys
			if (_gameboy.Bit.Get(data, P14) == 0)
			{
				return (u8)(Buttons & 0x0F);
			}

			return 0xFF;
		}

		// responsible for handling keyboard/gamepad input
		public void Listen()
		{
			bool controllerConnected = _frontEndInput.IsControllerConnected(0);

			// controller/gamepad
			if (controllerConnected)
			{
				// Check the device for Player One
				bool hasLeftThumbstick = _frontEndInput.HasLeftThumbstick(0);
				bool analogLeft = false;
				bool analogRight = false;
				bool analogUp = false;
				bool analogDown = false;

				if (hasLeftThumbstick)
				{
					analogLeft = _frontEndInput.IsAnalogDirectionHeld(EmulatorFrontend.Input.AnalogDirection.Left, 0);
					analogRight = _frontEndInput.IsAnalogDirectionHeld(EmulatorFrontend.Input.AnalogDirection.Right, 0);
					analogUp = _frontEndInput.IsAnalogDirectionHeld(EmulatorFrontend.Input.AnalogDirection.Up, 0);
					analogDown = _frontEndInput.IsAnalogDirectionHeld(EmulatorFrontend.Input.AnalogDirection.Down, 0);
					// prevent simulataneous left + up / left + down input etc
					analogLeft = analogLeft && !analogUp && !analogDown;
					analogRight = analogRight && !analogUp && !analogDown;
				}

				for (int i = 0; i < _gameboy.Window.Emulator.Buttons.Count; i++)
				{
					EmulatorFrontend.Input.ButtonDefinitions button = _gameboy.Window.Emulator.Buttons[i];

					switch (button.ButtonName)
					{
						case "DPad Left":
							if (_frontEndInput.IsButtonHeld(button.Button) || analogLeft)
							{
								PressDirection(DirectionLeft, P14);
							}
							else
							{
								if (!analogLeft)
								{
									ReleaseKey(DirectionLeft);
								}
							}
							break;
						case "DPad Right":
							if (_frontEndInput.IsButtonHeld(button.Button) || analogRight)
							{
								PressDirection(DirectionRight, P14);
							}
							else
							{
								if (!analogRight)
								{
									ReleaseKey(DirectionRight);
								}
							}
							break;
						case "DPad Up":
							if (_frontEndInput.IsButtonHeld(button.Button) || analogUp)
							{
								PressDirection(DirectionUp, P14);
							}
							else
							{
								if (!analogUp)
								{
									ReleaseKey(DirectionUp);
								}
							}
							break;
						case "DPad Down":
							if (_frontEndInput.IsButtonHeld(button.Button) || analogDown)
							{
								PressDirection(DirectionDown, P14);
							}
							else
							{
								if (!analogDown)
								{
									ReleaseKey(DirectionDown);
								}
							}
							break;
						case "A":
							if (_frontEndInput.IsButtonHeld(button.Button))
							{
								PressButton(ButtonA, P15);
							}
							else
							{
								ReleaseKey(ButtonA);
							}
							break;
						case "B":
							if (_frontEndInput.IsButtonHeld(button.Button))
							{
								PressButton(ButtonB, P15);
							}
							else
							{
								ReleaseKey(ButtonB);
							}
							break;
						case "Select":
							if (_frontEndInput.IsButtonHeld(button.Button))
							{
								PressButton(ButtonSelect, P15);
							}
							else
							{
								ReleaseKey(ButtonSelect);
							}
							break;
						case "Start":
							if (_frontEndInput.IsButtonHeld(button.Button))
							{
								PressButton(ButtonStart, P15);
							}
							else
							{
								ReleaseKey(ButtonStart);
							}
							break;
					}
				}
			}
			else
			{
				// keyboard
				for (int i = 0; i < _gameboy.Window.Emulator.Keys.Count; i++)
				{
					EmulatorFrontend.Input.KeyDefinitions key = _gameboy.Window.Emulator.Keys[i];

					switch (key.KeyName)
					{
						case "DPad Left":
							if (_frontEndInput.IsKeyHeld(key.Key))
							{
								PressDirection(DirectionLeft, P14);
							}
							else
							{
								ReleaseKey(DirectionLeft);
							}
							break;
						case "DPad Right":
							if (_frontEndInput.IsKeyHeld(key.Key))
							{
								PressDirection(DirectionRight, P14);
							}
							else
							{
								ReleaseKey(DirectionRight);
							}
							break;
						case "DPad Up":
							if (_frontEndInput.IsKeyHeld(key.Key))
							{
								PressDirection(DirectionUp, P14);
							}
							else
							{
								ReleaseKey(DirectionUp);
							}
							break;
						case "DPad Down":
							if (_frontEndInput.IsKeyHeld(key.Key))
							{
								PressDirection(DirectionDown, P14);
							}
							else
							{
								ReleaseKey(DirectionDown);
							}
							break;
						case "A":
							if (_frontEndInput.IsKeyHeld(key.Key))
							{
								PressButton(ButtonA, P15);
							}
							else
							{
								ReleaseKey(ButtonA);
							}
							break;
						case "B":
							if (_frontEndInput.IsKeyHeld(key.Key))
							{
								PressButton(ButtonB, P15);
							}
							else
							{
								ReleaseKey(ButtonB);
							}
							break;
						case "Select":
							if (_frontEndInput.IsKeyHeld(key.Key))
							{
								PressButton(ButtonSelect, P15);
							}
							else
							{
								ReleaseKey(ButtonSelect);
							}
							break;
						case "Start":
							if (_frontEndInput.IsKeyHeld(key.Key))
							{
								PressButton(ButtonStart, P15);
							}
							else
							{
								ReleaseKey(ButtonStart);
							}
							break;
					}
				}
			}
		}
	}
}
