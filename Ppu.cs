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
using Microsoft.Xna.Framework;

namespace CoreBoy
{
	using u8 = Byte;
	using s8 = SByte;
	using u16 = UInt16;
	using s16 = Int16;

	public class Ppu
	{
		public struct Rgb
		{
			public u8 R;
			public u8 G;
			public u8 B;
		}
		public u8 Ly
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.LY);
			set => _gameboy.Memory.Get()[Memory.Address.LY] = value;
		}
		public u8 Lyc
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.LYC);
			set => _gameboy.Memory.WriteByte(Memory.Address.LYC, value);
		}
		public u8 Lcdc
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.LCDC);
			set => _gameboy.Memory.Get()[Memory.Address.LCDC] = value;
		}
		public u8 Stat
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.STAT);
			set => _gameboy.Memory.Get()[Memory.Address.STAT] = value;
		}
		public u8 Bgp
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.BGP);
			set => _gameboy.Memory.Get()[Memory.Address.BGP] = value;
		}
		public u8 Op0
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.OP0);
			set => _gameboy.Memory.Get()[Memory.Address.OP0] = value;
		}
		public u8 Op1
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.OP1);
			set => _gameboy.Memory.Get()[Memory.Address.OP1] = value;
		}
		public u8 ScX
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.SCX);
			set => _gameboy.Memory.WriteByte(Memory.Address.SCX, value);
		}
		public u8 ScY
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.SCY);
			set => _gameboy.Memory.WriteByte(Memory.Address.SCY, value);
		}
		public u8 Wx
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.WX);
			set => _gameboy.Memory.WriteByte(Memory.Address.WX, value);
		}
		public u8 Wy
		{
			get => _gameboy.Memory.ReadByte(Memory.Address.WY);
			set => _gameboy.Memory.WriteByte(Memory.Address.WY, value);
		}
		public Color[] Screen { get; set; }
		public int ScanlineCounter { get; set; }
		public Rgb[] ColorPalette { get; set; }
		private const int _clockCycles = 456;
		private u8 _screenWidth => (u8)_gameboy.Window.Emulator.ScreenWidth;
		private u8 _screenHeight => (u8)_gameboy.Window.Emulator.ScreenHeight;
		private readonly Gameboy _gameboy;

		public Ppu(Gameboy gameboy)
		{
			_gameboy = gameboy;
			ColorPalette = new Rgb[]
			{
				new Rgb { R = 155, G = 188, B = 15 },
				new Rgb { R = 139, G = 172, B = 15},
				new Rgb { R = 48, G = 98, B = 48},
				new Rgb { R = 15, G = 56, B = 15}
			};
			Screen = new Color[_screenWidth * _screenHeight];

			Init();
		}

		public void Init()
		{
			Reset();

			if (_gameboy.Window.Emulator.ScreenTexture != null)
			{
				UpdateTexture();
			}
		}

		public void Reset()
		{
			ScanlineCounter = 0;

			for (int i = 0; i < Screen.Length; i++)
			{
				Screen[i] = new Color(ColorPalette[0].R, ColorPalette[0].G, ColorPalette[0].B, (u8)255);
			}
		}

		// responsible for determining if the Lcd display is enabled
		public bool Enabled()
		{
			return _gameboy.Bit.Get(Lcdc, 7) == 1;
		}

		// responsible for updating the Lcd controller
		public void Update(int cycles)
		{
			SetStatus();

			if (!Enabled()) return;

			ScanlineCounter += cycles;

			if (ScanlineCounter >= _clockCycles)
			{
				if (Ly >= 0 && Ly <= 143)
				{
					DrawScanline();
				}
				else if (Ly == 144)
				{
					UpdateTexture();
					_gameboy.Interrupts.Request((int)Interrupts.Types.Vblank);
				}
				else if (Ly == 154)
				{
					Ly = 0xFF;
				}

				Ly += 1;

				if (Ly == Lyc)
				{
					_gameboy.Bit.SetMemory(Memory.Address.STAT, 2);
				}
				else
				{
					_gameboy.Bit.ClearMemory(Memory.Address.STAT, 2);
				}

				if (_gameboy.Bit.Get(Stat, 2) == 1 && _gameboy.Bit.Get(Stat, 6) == 1)
				{
					_gameboy.Interrupts.Request((int)Interrupts.Types.Lcd);
				}

				Stat |= 0x80;
				ScanlineCounter -= _clockCycles;
			}
		}

		// responsible for setting the Lcd mode
		public u8 SetMode(u8 mode)
		{
			switch (mode)
			{
				case 0:
					{
						_gameboy.Bit.ClearMemory(Memory.Address.STAT, 0);
						_gameboy.Bit.ClearMemory(Memory.Address.STAT, 1);
					}
					break;
				case 1:
					{
						_gameboy.Bit.SetMemory(Memory.Address.STAT, 0);
						_gameboy.Bit.ClearMemory(Memory.Address.STAT, 1);
					}
					break;
				case 2:
					{
						_gameboy.Bit.ClearMemory(Memory.Address.STAT, 0);
						_gameboy.Bit.SetMemory(Memory.Address.STAT, 1);
					}
					break;
				case 3:
					{
						_gameboy.Bit.SetMemory(Memory.Address.STAT, 0);
						_gameboy.Bit.SetMemory(Memory.Address.STAT, 1);
					}
					break;
			}

			Stat |= 0x80;

			return mode;
		}

		// responsible for setting the Lcd status
		public void SetStatus()
		{
			bool requestInterrupt = false;
			u8 nextMode = 0;
			u8 currentMode = (u8)(Stat & 0x3);
			u8 vblankRange = 144;
			u16 oamRange = (u16)(_clockCycles - 80);
			u16 dataToLcd = (u16)(oamRange - 172);

			if (!Enabled())
			{
				Stat &= 0xF8;
				Ly = 0x00;
				ScanlineCounter = 0;
				return;
			}

			if (Ly >= vblankRange)
			{
				nextMode = SetMode(1);
				requestInterrupt = _gameboy.Bit.Get(Stat, 4) == 1;
			}
			else
			{
				if (ScanlineCounter >= oamRange && ScanlineCounter <= _clockCycles)
				{
					nextMode = SetMode(2);
					requestInterrupt = _gameboy.Bit.Get(Stat, 5) == 1;
				}
				else if (ScanlineCounter >= dataToLcd && ScanlineCounter <= (oamRange - 1))
				{
					nextMode = SetMode(3);
				}
				else
				{
					nextMode = SetMode(0);
					requestInterrupt = _gameboy.Bit.Get(Stat, 3) == 1;
				}
			}

			if (requestInterrupt && (nextMode != currentMode))
			{
				_gameboy.Interrupts.Request((int)Interrupts.Types.Lcd);
			}
		}

		// responsible for updating the screen texture
		public void UpdateTexture()
		{
			_gameboy.Window.Emulator.ScreenTexture.SetData(Screen);
		}

		// responsible for determining if the background is enabled
		public bool IsBackgroundEnabled()
		{
			return _gameboy.Bit.Get(Lcdc, 0) == 1;
		}

		// responsible for determining if the window is enabled
		public bool IsWindowEnabled()
		{
			return _gameboy.Bit.Get(Lcdc, 5) == 1;
		}

		// responsible for determining if sprites are enabled
		public bool IsSpritesEnabled()
		{
			return _gameboy.Bit.Get(Lcdc, 1) == 1;
		}

		// responsible for getting a color from a palette
		public Rgb GetColor(u8 palette, u8 bit)
		{
			u8 hi = (u8)((bit << 1) + 1);
			u8 lo = (u8)(bit << 1);
			u8 color = (u8)((_gameboy.Bit.Get(palette, hi) << 1) | (_gameboy.Bit.Get(palette, lo)));

			return ColorPalette[color];
		}

		// responsible for drawing the current scanline
		public void DrawScanline()
		{
			DrawBackground();
			DrawSprites();
		}

		// responsible for drawing the background
		public void DrawBackground()
		{
			if (!IsBackgroundEnabled()) return;

			u16 tileMemory = (_gameboy.Bit.Get(Lcdc, 3) == 1) ? (u16)0x9C00 : (u16)0x9800;
			u16 tileData = (_gameboy.Bit.Get(Lcdc, 4) == 1) ? (u16)0x8000 : (u16)0x8800;
			u16 tileMemoryAddress = tileMemory;
			u16 windowMemoryAddress = (_gameboy.Bit.Get(Lcdc, 6) == 1) ? (u16)0x9C00 : (u16)0x9800;
			bool unsignedTile = (_gameboy.Bit.Get(Lcdc, 4) == 1);
			bool windowEnabled = IsWindowEnabled();

			for (u8 x = 0; x < _screenWidth; x++)
			{
				u8 yPos = (u8)(ScY + Ly);
				u8 xPos = (u8)(ScX + x);

				if (windowEnabled && (Ly >= Wy) && (x >= (Wx - 7)))
				{
					tileMemory = windowMemoryAddress;
					yPos = (u8)(Ly - Wy);
					xPos = (u8)((Wx - 7) + x);
				}
				else
				{
					tileMemory = tileMemoryAddress;
				}

				u16 tileCol = (u16)(xPos / 8);
				u16 tileRow = (u16)((yPos / 8) * 32);
				u8 tileYLine = (u8)((yPos % 8) * 2);
				u16 tileAddress = (u16)(tileMemory + tileCol + tileRow);
				s16 tileNum = (s8)_gameboy.Memory.ReadByte(tileAddress);
				u16 tileLocation = (u16)((tileData) + ((tileNum + 128) * 16));

				if (unsignedTile)
				{
					tileNum = (u8)_gameboy.Memory.ReadByte(tileAddress);
					tileLocation = (u16)(tileData + (tileNum * 16));
				}

				u8 pixelData1 = _gameboy.Memory.ReadByte((u16)(tileLocation + tileYLine));
				u8 pixelData2 = _gameboy.Memory.ReadByte((u16)(tileLocation + tileYLine + 1));
				u8 colorBit = (u8)(((xPos % 8) - 7) * -1);
				u8 colorNum = (u8)((_gameboy.Bit.Get(pixelData2, colorBit) << 1) | (_gameboy.Bit.Get(pixelData1, colorBit)));
				Rgb pixelColor = GetColor(Bgp, colorNum);

				Screen[Ly * _screenWidth + x].R = pixelColor.R;
				Screen[Ly * _screenWidth + x].G = pixelColor.G;
				Screen[Ly * _screenWidth + x].B = pixelColor.B;
			}
		}

		// responsible for drawing sprites
		public void DrawSprites()
		{
			if (!IsSpritesEnabled()) return;

			const u16 spriteData = 0x8000;
			const u16 spriteAttributeData = 0xFE00;
			const u8 spriteWidth = 8;
			u8 spriteHeight = _gameboy.Bit.Get(Lcdc, 2) == 1 ? (u8)16 : (u8)8;
			const u8 spriteLimit = 40;

			for (int i = (spriteLimit - 1); i >= 0; i--)
			{
				u8 index = (u8)(i * 4);
				s8 yPos = (s8)(_gameboy.Memory.ReadByte((u16)(spriteAttributeData + index)) - 16);
				s8 xPos = (s8)(_gameboy.Memory.ReadByte((u16)(spriteAttributeData + index + 1)) - 8);
				u8 patternNo = _gameboy.Memory.ReadByte((u16)(spriteAttributeData + index + 2));
				u8 flags = _gameboy.Memory.ReadByte((u16)(spriteAttributeData + index + 3));

				if (spriteHeight == 16) _gameboy.Bit.Clear(ref patternNo, 0);

				u8 priority = _gameboy.Bit.Get(flags, 7);
				u8 yFlip = _gameboy.Bit.Get(flags, 6);
				u8 xFlip = _gameboy.Bit.Get(flags, 5);
				u8 palette = (_gameboy.Bit.Get(flags, 4) == 1) ? Op1 : Op0;
				u8 line = (yFlip == 1) ? (u8)((((Ly - yPos - spriteHeight) + 1) * -1) * 2) : (u8)((Ly - yPos) * 2);
				u8 pixelData1 = _gameboy.Memory.ReadByte((u16)(spriteData + (patternNo * 16) + line));
				u8 pixelData2 = _gameboy.Memory.ReadByte((u16)(spriteData + (patternNo * 16) + line + 1));

				// sprites at position 0 are not drawn
				if (xPos == 0 && yPos == 0) continue;

				if (Ly >= yPos && Ly < (yPos + spriteHeight))
				{
					for (int pixel = 7; pixel >= 0; pixel--)
					{
						u8 x = (u8)(xPos + pixel);
						u8 spritePixel = (xFlip == 1) ? (u8)pixel : (u8)((pixel - 7) * -1);
						bool isWhite = false;

						if (Ly * _screenWidth + x < Screen.Length)
						{
							isWhite = (Screen[Ly * _screenWidth + x].R == 155);
						}

						u8 colorNum = (u8)((_gameboy.Bit.Get(pixelData2, spritePixel) << 1) | (_gameboy.Bit.Get(pixelData1, spritePixel)));
						Rgb pixelColor = GetColor(palette, colorNum);

						// skip drawing off-screen sprites
						if (x >= 160) continue;
						// skip drawing transparent pixels
						if (colorNum == 0x0) continue;
						// with priority 0x1, if the background pixel isn't white, the sprite isn't drawn
						if (priority == 0x1 && !isWhite) continue;

						Screen[Ly * _screenWidth + x].R = pixelColor.R;
						Screen[Ly * _screenWidth + x].G = pixelColor.G;
						Screen[Ly * _screenWidth + x].B = pixelColor.B;
					}
				}
			}
		}
	}
}
