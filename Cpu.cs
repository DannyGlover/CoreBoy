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
using System.Collections.Generic;

namespace CoreBoy
{
	using u8 = Byte;
	using u16 = UInt16;

	public class Cpu
	{
		public class Register
		{
			public u16 Reg
			{
				get
				{
					return (u16)(Hi << 8 | Lo);
				}
				set
				{
					Lo = (u8)(value & 0xFF);
					Hi = (u8)(value >> 8);
				}
			}
			public u8 Lo { get; set; }
			public u8 Hi { get; set; }
		}

		public Register AF { get; set; }
		public Register BC { get; set; }
		public Register DE { get; set; }
		public Register HL { get; set; }
		public Register SP { get; set; }
		public Register PC { get; set; }
		public u8 A { get => AF.Hi; set => AF.Hi = value; }
		public u8 F { get => AF.Lo; set => AF.Lo = value; }
		public u8 B { get => BC.Hi; set => BC.Hi = value; }
		public u8 C { get => BC.Lo; set => BC.Lo = value; }
		public u8 D { get => DE.Hi; set => DE.Hi = value; }
		public u8 E { get => DE.Lo; set => DE.Lo = value; }
		public u8 H { get => HL.Hi; set => HL.Hi = value; }
		public u8 L { get => HL.Lo; set => HL.Lo = value; }
		public int Cycles { get; set; }
		public int InstructionsRan { get; set; }
		public bool Halted { get; set; }
		public bool HaltBug { get; set; }
		public bool Stopped { get; set; }
		public bool PendingInterrupt { get; set; }
		public bool DidLoadBios { get; set; }
		public List<string> Log { get; set; }
		private CpuOps.CpuOps _cpuOps;
		private Memory _memory => _gameboy.Memory;
		private Flags _flags => _gameboy.Flags;
		private readonly Gameboy _gameboy;

		public Cpu(Gameboy gameboy)
		{
			_gameboy = gameboy;
			AF = new Register();
			BC = new Register();
			DE = new Register();
			HL = new Register();
			SP = new Register();
			PC = new Register();
			Log = new List<string>();
			_cpuOps = new CpuOps.CpuOps(_gameboy);

			Init();
		}

		public void Init()
		{
			if (DidLoadBios)
			{
				AF.Reg = 0x0000;
				BC.Reg = 0x0000;
				DE.Reg = 0x0000;
				HL.Reg = 0x0000;
				SP.Reg = 0x0000;
				PC.Reg = 0x0000;
			}
			else
			{
				AF.Reg = 0x01B0;
				BC.Reg = 0x0013;
				DE.Reg = 0x00D8;
				HL.Reg = 0x014D;
				SP.Reg = 0xFFFE;
				PC.Reg = 0x100;
			}
			Cycles = 0;
			InstructionsRan = 0;
			Halted = false;
			HaltBug = false;
			Stopped = false;
			PendingInterrupt = false;
		}

		public void ExecuteOpcode()
		{
			u8 opcode = _memory.ReadByte(PC.Reg);

			//string log = String.Format("{0:X4}:{1:X2}:{2:X4}:{3:X4}:{4:X4}:{5:X4}:{6:X4}:Z{7}:N{8}:H{9}:C{10}", PC.Reg, opcode, AF.Reg, BC.Reg, DE.Reg, HL.Reg, SP.Reg, _flags.Get(Flags.Z), _flags.Get(Flags.N), _flags.Get(Flags.H), _flags.Get(Flags.C));
			//Log.Add(log);

			if (Halted)
			{
				Cycles += 4;
				return;
			}
			else
			{
				PC.Reg += 1;
				InstructionsRan += 1;

				if (HaltBug)
				{
					PC.Reg -= 1;
					HaltBug = false;
				}
			}

			switch (opcode)
			{
				case 0x00: _cpuOps.Nop(4); break; // NOP
				case 0x01: BC.Reg = _cpuOps.Load16(_memory.ReadWord(PC.Reg), 12); PC.Reg += 2; break; // LD BC,d16
				case 0x02: _cpuOps.Write8(BC.Reg, A, 8); break; // LD (BC),A
				case 0x03: BC.Reg = _cpuOps.Inc16(BC.Reg, 8); break; // INC BC
				case 0x04: B = _cpuOps.Inc8(B, 4); break; // INC B
				case 0x05: B = _cpuOps.Dec8(B, 4); break; // DEC B
				case 0x06: B = _cpuOps.Load8(_memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // LD B,d8
				case 0x07: A = _cpuOps.Rlc8(A, false, 4); break; // RLCA
				case 0x08: _memory.WriteWord(_memory.ReadWord(PC.Reg), SP); PC.Reg += 2; Cycles += 20; break; // LD (a16),SP
				case 0x09: HL.Reg = _cpuOps.Add16(HL.Reg, BC.Reg, 8); break; // ADD HL,BC
				case 0x0A: A = _cpuOps.Load8(_memory.ReadByte(BC.Reg), 8); break; // LD A,(BC)
				case 0x0B: BC.Reg = _cpuOps.Dec16(BC.Reg, 8); break; // DEC BC
				case 0x0C: C = _cpuOps.Inc8(C, 4); break; // INC C
				case 0x0D: C = _cpuOps.Dec8(C, 4); break; // DEC C
				case 0x0E: C = _cpuOps.Load8(_memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // LD C,d8
				case 0x0F: A = _cpuOps.Rrc8(A, false, 4); break; // RRCA
				case 0x10: _cpuOps.Stop(4); break; // STOP
				case 0x11: DE.Reg = _cpuOps.Load16(_memory.ReadWord(PC.Reg), 12); PC.Reg += 2; break; // LD DE,d16
				case 0x12: _cpuOps.Write8(DE.Reg, A, 8); break; // LD (DE),A
				case 0x13: DE.Reg = _cpuOps.Inc16(DE.Reg, 8); break; // INC DE
				case 0x14: D = _cpuOps.Inc8(D, 4); break; // INC D
				case 0x15: D = _cpuOps.Dec8(D, 4); break; // DEC D
				case 0x16: D = _cpuOps.Load8(_memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // LD D,d8
				case 0x17: A = _cpuOps.Rl8(A, false, 4); break; // RLA
				case 0x18: _cpuOps.JmpRel(true, 8); break; // JR r8
				case 0x19: HL.Reg = _cpuOps.Add16(HL.Reg, DE.Reg, 8); break; // ADD HL,DE
				case 0x1A: A = _cpuOps.Load8(_memory.ReadByte(DE.Reg), 8); break; // LD A,(DE)
				case 0x1B: DE.Reg = _cpuOps.Dec16(DE.Reg, 8); break; // DEC DE
				case 0x1C: E = _cpuOps.Inc8(E, 4); break; // INC E
				case 0x1D: E = _cpuOps.Dec8(E, 4); break; // DEC E
				case 0x1E: E = _cpuOps.Load8(_memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // LD E,d8
				case 0x1F: A = _cpuOps.Rr8(A, false, 4); break; // RRA
				case 0x20: _cpuOps.JmpRel(_flags.Get(Flags.Z) == 0, 8); break; // JR NZ,r8
				case 0x21: HL.Reg = _cpuOps.Load16(_memory.ReadWord(PC.Reg), 12); PC.Reg += 2; break; // LD HL,d16
				case 0x22: _cpuOps.Write8(HL.Reg, A, 8); HL.Reg += 1; break; // LD (HL+),A
				case 0x23: HL.Reg = _cpuOps.Inc16(HL.Reg, 8); break; // INC HL
				case 0x24: H = _cpuOps.Inc8(H, 4); break; // INC H
				case 0x25: H = _cpuOps.Dec8(H, 4); break; // DEC H
				case 0x26: H = _cpuOps.Load8(_memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // LD H,d8
				case 0x27: _cpuOps.Daa(4); break; // DAA
				case 0x28: _cpuOps.JmpRel(_flags.Get(Flags.Z) == 1, 8); break; // JR Z,r8
				case 0x29: HL.Reg = _cpuOps.Add16(HL.Reg, HL.Reg, 8); break; // ADD HL,HL
				case 0x2A: A = _cpuOps.Load8(_memory.ReadByte(HL.Reg), 8); HL.Reg += 1; break; // LD A,(HL+)
				case 0x2B: HL.Reg = _cpuOps.Dec16(HL.Reg, 8); break; // DEC HL
				case 0x2C: L = _cpuOps.Inc8(L, 4); break; // INC L
				case 0x2D: L = _cpuOps.Dec8(L, 4); break; // DEC L
				case 0x2E: L = _cpuOps.Load8(_memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // LD L,d8
				case 0x2F: _cpuOps.CmplA(4); break; // CPL A
				case 0x30: _cpuOps.JmpRel(_flags.Get(Flags.C) == 0, 8); break; // JR NC,r8
				case 0x31: SP.Reg = _cpuOps.Load16(_memory.ReadWord(PC.Reg), 12); PC.Reg += 2; break; // LD SP,d16
				case 0x32: _cpuOps.Write8(HL.Reg, A, 8); HL.Reg -= 1; break; // LD (HL-),A
				case 0x33: SP.Reg = _cpuOps.Inc16(SP.Reg, 8); break; // INC SP
				case 0x34: _cpuOps.Inc8Mem(HL.Reg, 12); break; // INC (HL)
				case 0x35: _cpuOps.Dec8Mem(HL.Reg, 12); break; // DEC (HL)
				case 0x36: _cpuOps.Write8(HL.Reg, _memory.ReadByte(PC.Reg), 12); PC.Reg += 1; break; // LD (HL),d8
				case 0x37: _cpuOps.Scf(4); break; // SCF
				case 0x38: _cpuOps.JmpRel(_flags.Get(Flags.C) == 1, 8); break; // JR C,r8
				case 0x39: HL.Reg = _cpuOps.Add16(HL.Reg, SP.Reg, 8); break; // ADD HL,SP
				case 0x3A: A = _cpuOps.Load8(_memory.ReadByte(HL.Reg), 8); HL.Reg -= 1; break; // LD A,(HL-)
				case 0x3B: SP.Reg = _cpuOps.Dec16(SP.Reg, 8); break; // DEC SP
				case 0x3C: A = _cpuOps.Inc8(A, 4); break; // INC A
				case 0x3D: A = _cpuOps.Dec8(A, 4); break; // DEC A
				case 0x3E: A = _cpuOps.Load8(_memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // LD A,d8
				case 0x3F: _cpuOps.Ccf(4); break; // CCF
				case 0x40: B = _cpuOps.Load8(B, 4); break; // LD B,B
				case 0x41: B = _cpuOps.Load8(C, 4); break; // LD B,C
				case 0x42: B = _cpuOps.Load8(D, 4); break; // LD B,D
				case 0x43: B = _cpuOps.Load8(E, 4); break; // LD B,E
				case 0x44: B = _cpuOps.Load8(H, 4); break; // LD B,H
				case 0x45: B = _cpuOps.Load8(L, 4); break; // LD B,L
				case 0x46: B = _cpuOps.Load8(_memory.ReadByte(HL.Reg), 8); break; // LD B,(HL)
				case 0x47: B = _cpuOps.Load8(A, 4); break; // LD B,A
				case 0x48: C = _cpuOps.Load8(B, 4); break; // LD C,B
				case 0x49: C = _cpuOps.Load8(C, 4); break; // LD C,C
				case 0x4A: C = _cpuOps.Load8(D, 4); break; // LD C,D
				case 0x4B: C = _cpuOps.Load8(E, 4); break; // LD C,E
				case 0x4C: C = _cpuOps.Load8(H, 4); break; // LD C,H
				case 0x4D: C = _cpuOps.Load8(L, 4); break; // LD C,L
				case 0x4E: C = _cpuOps.Load8(_memory.ReadByte(HL.Reg), 8); break; // LD C,(HL)
				case 0x4F: C = _cpuOps.Load8(A, 4); break; // LD C,A
				case 0x50: D = _cpuOps.Load8(B, 4); break; // LD D,B
				case 0x51: D = _cpuOps.Load8(C, 4); break; // LD D,C
				case 0x52: D = _cpuOps.Load8(D, 4); break; // LD D,D
				case 0x53: D = _cpuOps.Load8(E, 4); break; // LD D,E
				case 0x54: D = _cpuOps.Load8(H, 4); break; // LD D,H
				case 0x55: D = _cpuOps.Load8(L, 4); break; // LD D,L
				case 0x56: D = _cpuOps.Load8(_memory.ReadByte(HL.Reg), 8); break; // LD D,(HL)
				case 0x57: D = _cpuOps.Load8(A, 4); break; // LD D,A
				case 0x58: E = _cpuOps.Load8(B, 4); break; // LD E,B
				case 0x59: E = _cpuOps.Load8(C, 4); break; // LD E,C
				case 0x5A: E = _cpuOps.Load8(D, 4); break; // LD E,D
				case 0x5B: E = _cpuOps.Load8(E, 4); break; // LD E,E
				case 0x5C: E = _cpuOps.Load8(H, 4); break; // LD E,H
				case 0x5D: E = _cpuOps.Load8(L, 4); break; // LD E,L
				case 0x5E: E = _cpuOps.Load8(_memory.ReadByte(HL.Reg), 8); break; // LD E,(HL)
				case 0x5F: E = _cpuOps.Load8(A, 4); break; // LD E,A
				case 0x60: H = _cpuOps.Load8(B, 4); break; // LD H,B
				case 0x61: H = _cpuOps.Load8(C, 4); break; // LD H,C
				case 0x62: H = _cpuOps.Load8(D, 4); break; // LD H,D
				case 0x63: H = _cpuOps.Load8(E, 4); break; // LD H,E
				case 0x64: H = _cpuOps.Load8(H, 4); break; // LD H,H
				case 0x65: H = _cpuOps.Load8(L, 4); break; // LD H,L
				case 0x66: H = _cpuOps.Load8(_memory.ReadByte(HL.Reg), 8); break; // LD H,(HL)
				case 0x67: H = _cpuOps.Load8(A, 4); break; // LD H,A
				case 0x68: L = _cpuOps.Load8(B, 4); break; // LD L,B
				case 0x69: L = _cpuOps.Load8(C, 4); break; // LD L,C
				case 0x6A: L = _cpuOps.Load8(D, 4); break; // LD L,D
				case 0x6B: L = _cpuOps.Load8(E, 4); break; // LD L,E
				case 0x6C: L = _cpuOps.Load8(H, 4); break; // LD L,H
				case 0x6D: L = _cpuOps.Load8(L, 4); break; // LD L,L
				case 0x6E: L = _cpuOps.Load8(_memory.ReadByte(HL.Reg), 8); break; // LD L,(HL)
				case 0x6F: L = _cpuOps.Load8(A, 4); break; // LD L,A
				case 0x70: _cpuOps.Write8(HL.Reg, B, 8); break; // LD (HL),B
				case 0x71: _cpuOps.Write8(HL.Reg, C, 8); break; // LD (HL),C
				case 0x72: _cpuOps.Write8(HL.Reg, D, 8); break; // LD (HL),D
				case 0x73: _cpuOps.Write8(HL.Reg, E, 8); break; // LD (HL),E
				case 0x74: _cpuOps.Write8(HL.Reg, H, 8); break; // LD (HL),H
				case 0x75: _cpuOps.Write8(HL.Reg, L, 8); break; // LD (HL),L
				case 0x76: _cpuOps.Halt(4); break; // HALT
				case 0x77: _cpuOps.Write8(HL.Reg, A, 8); break; // LD (HL),A
				case 0x78: A = _cpuOps.Load8(B, 4); break; // LD A,B
				case 0x79: A = _cpuOps.Load8(C, 4); break; // LD A,C
				case 0x7A: A = _cpuOps.Load8(D, 4); break; // LD A,D
				case 0x7B: A = _cpuOps.Load8(E, 4); break; // LD A,E
				case 0x7C: A = _cpuOps.Load8(H, 4); break; // LD A,H
				case 0x7D: A = _cpuOps.Load8(L, 4); break; // LD A,L
				case 0x7E: A = _cpuOps.Load8(_memory.ReadByte(HL.Reg), 8); break; // LD A,(HL)
				case 0x7F: A = _cpuOps.Load8(A, 4); break; // LD A,A
				case 0x80: A = _cpuOps.Add8(A, B, 4); break; // ADD A,B
				case 0x81: A = _cpuOps.Add8(A, C, 4); break; // ADD A,C
				case 0x82: A = _cpuOps.Add8(A, D, 4); break; // ADD A,D
				case 0x83: A = _cpuOps.Add8(A, E, 4); break; // ADD A,E
				case 0x84: A = _cpuOps.Add8(A, H, 4); break; // ADD A,H
				case 0x85: A = _cpuOps.Add8(A, L, 4); break; // ADD A,L
				case 0x86: A = _cpuOps.Add8(A, _memory.ReadByte(HL.Reg), 8); break; // ADD A,(HL)
				case 0x87: A = _cpuOps.Add8(A, A, 4); break; // ADD A,A
				case 0x88: A = _cpuOps.Adc8(A, B, 4); break; // ADC A,B
				case 0x89: A = _cpuOps.Adc8(A, C, 4); break; // ADC A,C
				case 0x8A: A = _cpuOps.Adc8(A, D, 4); break; // ADC A,D
				case 0x8B: A = _cpuOps.Adc8(A, E, 4); break; // ADC A,E
				case 0x8C: A = _cpuOps.Adc8(A, H, 4); break; // ADC A,H
				case 0x8D: A = _cpuOps.Adc8(A, L, 4); break; // ADC A,L
				case 0x8E: A = _cpuOps.Adc8(A, _memory.ReadByte(HL.Reg), 8); break; // ADC A,(HL)
				case 0x8F: A = _cpuOps.Adc8(A, A, 4); break; // ADC A,A
				case 0x90: A = _cpuOps.Sub8(A, B, 4); break; // SUB A,B
				case 0x91: A = _cpuOps.Sub8(A, C, 4); break; // SUB A,C
				case 0x92: A = _cpuOps.Sub8(A, D, 4); break; // SUB A,D
				case 0x93: A = _cpuOps.Sub8(A, E, 4); break; // SUB A,E
				case 0x94: A = _cpuOps.Sub8(A, H, 4); break; // SUB A,H
				case 0x95: A = _cpuOps.Sub8(A, L, 4); break; // SUB A,L
				case 0x96: A = _cpuOps.Sub8(A, _memory.ReadByte(HL.Reg), 8); break; // SUB A,(HL)
				case 0x97: A = _cpuOps.Sub8(A, A, 4); break; // SUB A,A
				case 0x98: A = _cpuOps.Sbc8(A, B, 4); break; // SBC A,B
				case 0x99: A = _cpuOps.Sbc8(A, C, 4); break; // SBC A,C
				case 0x9A: A = _cpuOps.Sbc8(A, D, 4); break; // SBC A,D
				case 0x9B: A = _cpuOps.Sbc8(A, E, 4); break; // SBC A,E
				case 0x9C: A = _cpuOps.Sbc8(A, H, 4); break; // SBC A,H
				case 0x9D: A = _cpuOps.Sbc8(A, L, 4); break; // SBC A,L
				case 0x9E: A = _cpuOps.Sbc8(A, _memory.ReadByte(HL.Reg), 8); break; // SBC A,(HL)
				case 0x9F: A = _cpuOps.Sbc8(A, A, 4); break; // SBC A,A
				case 0xA0: A = _cpuOps.And8(A, B, 4); break; // AND A,B
				case 0xA1: A = _cpuOps.And8(A, C, 4); break; // AND A,C
				case 0xA2: A = _cpuOps.And8(A, D, 4); break; // AND A,D
				case 0xA3: A = _cpuOps.And8(A, E, 4); break; // AND A,E
				case 0xA4: A = _cpuOps.And8(A, H, 4); break; // AND A,H
				case 0xA5: A = _cpuOps.And8(A, L, 4); break; // AND A,L
				case 0xA6: A = _cpuOps.And8(A, _memory.ReadByte(HL.Reg), 8); break; // AND A,(HL)
				case 0xA7: A = _cpuOps.And8(A, A, 4); break; // AND A,A
				case 0xA8: A = _cpuOps.Xor8(A, B, 4); break; // XOR A,B
				case 0xA9: A = _cpuOps.Xor8(A, C, 4); break; // XOR A,C
				case 0xAA: A = _cpuOps.Xor8(A, D, 4); break; // XOR A,D
				case 0xAB: A = _cpuOps.Xor8(A, E, 4); break; // XOR A,E
				case 0xAC: A = _cpuOps.Xor8(A, H, 4); break; // XOR A,H
				case 0xAD: A = _cpuOps.Xor8(A, L, 4); break; // XOR A,L
				case 0xAE: A = _cpuOps.Xor8(A, _memory.ReadByte(HL.Reg), 8); break; // XOR A,(HL)
				case 0xAF: A = _cpuOps.Xor8(A, A, 4); break; // XOR A,A
				case 0xB0: A = _cpuOps.Or8(A, B, 4); break; // OR A,B
				case 0xB1: A = _cpuOps.Or8(A, C, 4); break; // OR A,C
				case 0xB2: A = _cpuOps.Or8(A, D, 4); break; // OR A,D
				case 0xB3: A = _cpuOps.Or8(A, E, 4); break; // OR A,E
				case 0xB4: A = _cpuOps.Or8(A, H, 4); break; // OR A,H
				case 0xB5: A = _cpuOps.Or8(A, L, 4); break; // OR A,L
				case 0xB6: A = _cpuOps.Or8(A, _memory.ReadByte(HL.Reg), 8); break; // OR A,(HL)
				case 0xB7: A = _cpuOps.Or8(A, A, 4); break; // OR A,A
				case 0xB8: _cpuOps.Cmp8(A, B, 4); break; // CP A,B
				case 0xB9: _cpuOps.Cmp8(A, C, 4); break; // CP A,C
				case 0xBA: _cpuOps.Cmp8(A, D, 4); break; // CP A,D
				case 0xBB: _cpuOps.Cmp8(A, E, 4); break; // CP A,E
				case 0xBC: _cpuOps.Cmp8(A, H, 4); break; // CP A,H
				case 0xBD: _cpuOps.Cmp8(A, L, 4); break; // CP A,L
				case 0xBE: _cpuOps.Cmp8(A, _memory.ReadByte(HL.Reg), 8); break; // CP A,(HL)
				case 0xBF: _cpuOps.Cmp8(A, A, 4); break; // CP A,A
				case 0xC0: _cpuOps.Ret(_flags.Get(Flags.Z) == 0, 8); break; // RET NZ
				case 0xC1: BC.Reg = _memory.Pop(); Cycles += 12; break; // POP BC
				case 0xC2: _cpuOps.JmpImm(_flags.Get(Flags.Z) == 0, 12); break; // JP NZ,a16
				case 0xC3: _cpuOps.JmpImm(true, 12); break; // JP a16
				case 0xC4: _cpuOps.Call(_flags.Get(Flags.Z) == 0, 12); break; // CALL NZ,a16
				case 0xC5: _memory.Push(BC); Cycles += 16; break; // PUSH BC
				case 0xC6: A = _cpuOps.Add8(A, _memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // ADD A,d8
				case 0xC7: _cpuOps.Rst(0x00, 16); break; // RST 00H
				case 0xC8: _cpuOps.Ret(_flags.Get(Flags.Z) == 1, 8); break; // RET Z
				case 0xC9: _cpuOps.Ret(true, 8); break; // RET
				case 0xCA: _cpuOps.JmpImm(_flags.Get(Flags.Z) == 1, 12); break; // JP Z,a16
				case 0xCB: ExecuteExtendedOpcode(); Cycles += 4; break; // PREFIX CB
				case 0xCC: _cpuOps.Call(_flags.Get(Flags.Z) == 1, 12); break; // CALL Z,a16
				case 0xCD: _cpuOps.Call(true, 12); break; // CALL a16
				case 0xCE: A = _cpuOps.Adc8(A, _memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // ADC A,d8
				case 0xCF: _cpuOps.Rst(0x08, 16); break; // RST 08H
				case 0xD0: _cpuOps.Ret(_flags.Get(Flags.C) == 0, 8); break; // RET NC
				case 0xD1: DE.Reg = _memory.Pop(); Cycles += 12; break; // POP DE
				case 0xD2: _cpuOps.JmpImm(_flags.Get(Flags.C) == 0, 12); break; // JP NC,a16
				case 0xD4: _cpuOps.Call(_flags.Get(Flags.C) == 0, 12); break; // CALL NC,a16
				case 0xD5: _memory.Push(DE); Cycles += 16; break; // PUSH DE
				case 0xD6: A = _cpuOps.Sub8(A, _memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // SUB A, d8
				case 0xD7: _cpuOps.Rst(0x10, 16); break; // RST 10H
				case 0xD8: _cpuOps.Ret(_flags.Get(Flags.C) == 1, 8); break; // RET C
				case 0xD9: _cpuOps.Ret(true, 8); _gameboy.Interrupts.Ime = true; break; // RETI
				case 0xDA: _cpuOps.JmpImm(_flags.Get(Flags.C) == 1, 12); break; // JP C,a16
				case 0xDC: _cpuOps.Call(_flags.Get(Flags.C) == 1, 12); break; // CALL C,a16
				case 0xDE: A = _cpuOps.Sbc8(A, _memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // SBC A,d8
				case 0xDF: _cpuOps.Rst(0x18, 16); break; // RST 18H
				case 0xE0: _cpuOps.Write8((u16)(0xFF00 | _memory.ReadByte(PC.Reg)), A, 12); PC.Reg += 1; break; // LDH (a8),A
				case 0xE1: HL.Reg = _memory.Pop(); Cycles += 12; break; // POP HL
				case 0xE2: _cpuOps.Write8((u16)(0xFF00 | C), A, 8); break; // LD (C),A
				case 0xE5: _memory.Push(HL); Cycles += 16; break; // PUSH HL
				case 0xE6: A = _cpuOps.And8(A, _memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // AND A, d8
				case 0xE7: _cpuOps.Rst(0x20, 16); break; // RST 20H
				case 0xE8: _cpuOps.AddSpR8(16); PC.Reg += 1; break; // ADD SP,r8
				case 0xE9: PC.Reg = HL.Reg; Cycles += 4; break; // JP (HL)
				case 0xEA: _cpuOps.Write8(_memory.ReadWord(PC.Reg), A, 16); PC.Reg += 2; break; // LD (a16),A
				case 0xEE: A = _cpuOps.Xor8(A, _memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // XOR A, d8
				case 0xEF: _cpuOps.Rst(0x28, 16); break; // RST 28H
				case 0xF0: A = _cpuOps.Load8(_memory.ReadByte((u16)(0xFF00 | _memory.ReadByte(PC.Reg))), 12); PC.Reg += 1; break; // LDH A,(a8)
				case 0xF1: AF.Reg = (u16)(_memory.Pop() & ~0xF); Cycles += 12; break; // POP AF
				case 0xF2: A = _cpuOps.Load8(_memory.ReadByte((u16)(0xFF00 | C)), 8); break; // LD A,(C)
				case 0xF3: _cpuOps.DI(4); break; // DI
				case 0xF5: _memory.Push(AF); Cycles += 16; break; // PUSH AF
				case 0xF6: A = _cpuOps.Or8(A, _memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // OR A, d8
				case 0xF7: _cpuOps.Rst(0x30, 16); break; // RST 30H
				case 0xF8: _cpuOps.LoadHlSpR8(12); PC.Reg += 1; break; // LD HL,SP+r8
				case 0xF9: SP.Reg = _cpuOps.Load16(HL.Reg, 8); break; // LD SP,HL
				case 0xFA: A = _cpuOps.Load8(_memory.ReadByte(_memory.ReadWord(PC.Reg)), 16); PC.Reg += 2; break; // LD A,(a16)
				case 0xFB: _cpuOps.EI(4); break; // EI
				case 0xFE: _cpuOps.Cmp8(A, _memory.ReadByte(PC.Reg), 8); PC.Reg += 1; break; // CP A, d8
				case 0xFF: _cpuOps.Rst(0x38, 16); break; // RST 38H
				default:
					//Debugger::stopMachine = true;
					//Log::Critical("Unimplemented opcode %02X", opcode);
					break;
			}

			if (PendingInterrupt)
			{
				if (_gameboy.Interrupts.PendingCount >= 2)
				{
					_gameboy.Interrupts.Ime = true;
					_gameboy.Interrupts.PendingCount = -1;
					PendingInterrupt = false;
				}

				_gameboy.Interrupts.PendingCount += 1;
			}

			_gameboy.Interrupts.ShouldExecute = true;
			_gameboy.Interrupts.ClearIf = true;
		}

		// responsible for executing extended opcodes (prefix CB)
		public void ExecuteExtendedOpcode()
		{
			u8 opcode = _memory.ReadByte(PC.Reg);
			InstructionsRan += 1;
			PC.Reg += 1;

			switch (opcode)
			{
				case 0x00: B = _cpuOps.Rlc8(B, true, 8); break; // RLC B
				case 0x01: C = _cpuOps.Rlc8(C, true, 8); break; // RLC C
				case 0x02: D = _cpuOps.Rlc8(D, true, 8); break; // RLC D
				case 0x03: E = _cpuOps.Rlc8(E, true, 8); break; // RLC E
				case 0x04: H = _cpuOps.Rlc8(H, true, 8); break; // RLC H
				case 0x05: L = _cpuOps.Rlc8(L, true, 8); break; // RLC L
				case 0x06: _cpuOps.Rlc8Mem(HL.Reg, true, 16); break; // RLC (HL)
				case 0x07: A = _cpuOps.Rlc8(A, true, 8); break; // RLC A
				case 0x08: B = _cpuOps.Rrc8(B, true, 8); break; // RRC B
				case 0x09: C = _cpuOps.Rrc8(C, true, 8); break; // RRC C
				case 0x0A: D = _cpuOps.Rrc8(D, true, 8); break; // RRC D
				case 0x0B: E = _cpuOps.Rrc8(E, true, 8); break; // RRC E
				case 0x0C: H = _cpuOps.Rrc8(H, true, 8); break; // RRC H
				case 0x0D: L = _cpuOps.Rrc8(L, true, 8); break; // RRC L
				case 0x0E: _cpuOps.Rrc8Mem(HL.Reg, true, 16); break; // RRC (HL)
				case 0x0F: A = _cpuOps.Rrc8(A, true, 8); break; // RRC A
				case 0x10: B = _cpuOps.Rl8(B, true, 8); break; // RL B
				case 0x11: C = _cpuOps.Rl8(C, true, 8); break; // RL C
				case 0x12: D = _cpuOps.Rl8(D, true, 8); break; // RL D
				case 0x13: E = _cpuOps.Rl8(E, true, 8); break; // RL E
				case 0x14: H = _cpuOps.Rl8(H, true, 8); break; // RL H
				case 0x15: L = _cpuOps.Rl8(L, true, 8); break; // RL L
				case 0x16: _cpuOps.Rl8Mem(HL.Reg, true, 16); break; // RL (HL)
				case 0x17: A = _cpuOps.Rl8(A, true, 8); break; // RL A
				case 0x18: B = _cpuOps.Rr8(B, true, 8); break; // RR B
				case 0x19: C = _cpuOps.Rr8(C, true, 8); break; // RR C
				case 0x1A: D = _cpuOps.Rr8(D, true, 8); break; // RR D
				case 0x1B: E = _cpuOps.Rr8(E, true, 8); break; // RR E
				case 0x1C: H = _cpuOps.Rr8(H, true, 8); break; // RR H
				case 0x1D: L = _cpuOps.Rr8(L, true, 8); break; // RR L
				case 0x1E: _cpuOps.Rr8Mem(HL.Reg, true, 16); break; // RR (HL)
				case 0x1F: A = _cpuOps.Rr8(A, true, 8); break; // RR A
				case 0x20: B = _cpuOps.Slc8(B, 8); break; // SLA B
				case 0x21: C = _cpuOps.Slc8(C, 8); break; // SLA C
				case 0x22: D = _cpuOps.Slc8(D, 8); break; // SLA D
				case 0x23: E = _cpuOps.Slc8(E, 8); break; // SLA E
				case 0x24: H = _cpuOps.Slc8(H, 8); break; // SLA H
				case 0x25: L = _cpuOps.Slc8(L, 8); break; // SLA L
				case 0x26: _cpuOps.Slc8Mem(HL.Reg, 16); break; // SLA (HL)
				case 0x27: A = _cpuOps.Slc8(A, 8); break; // SLA A
				case 0x28: B = _cpuOps.Sr8(B, 8); break; // SRA B
				case 0x29: C = _cpuOps.Sr8(C, 8); break; // SRA C
				case 0x2A: D = _cpuOps.Sr8(D, 8); break; // SRA D
				case 0x2B: E = _cpuOps.Sr8(E, 8); break; // SRA E
				case 0x2C: H = _cpuOps.Sr8(H, 8); break; // SRA H
				case 0x2D: L = _cpuOps.Sr8(L, 8); break; // SRA L
				case 0x2E: _cpuOps.Sr8Mem(HL.Reg, 16); break; // SRA (HL)
				case 0x2F: A = _cpuOps.Sr8(A, 8); break; // SRA A
				case 0x30: B = _cpuOps.BitSwap(B, 8); break; // SWAP B
				case 0x31: C = _cpuOps.BitSwap(C, 8); break; // SWAP C
				case 0x32: D = _cpuOps.BitSwap(D, 8); break; // SWAP D
				case 0x33: E = _cpuOps.BitSwap(E, 8); break; // SWAP E
				case 0x34: H = _cpuOps.BitSwap(H, 8); break; // SWAP H
				case 0x35: L = _cpuOps.BitSwap(L, 8); break; // SWAP L
				case 0x36: _cpuOps.BitSwapMem(HL.Reg, 16); break; // SWAP (HL)
				case 0x37: A = _cpuOps.BitSwap(A, 8); break; // SWAP A
				case 0x38: B = _cpuOps.Src8(B, 8); break; // SRL B
				case 0x39: C = _cpuOps.Src8(C, 8); break; // SRL C
				case 0x3A: D = _cpuOps.Src8(D, 8); break; // SRL D
				case 0x3B: E = _cpuOps.Src8(E, 8); break; // SRL E
				case 0x3C: H = _cpuOps.Src8(H, 8); break; // SRL H
				case 0x3D: L = _cpuOps.Src8(L, 8); break; // SRL L
				case 0x3E: _cpuOps.Src8Mem(HL.Reg, 16); break; // SRL (HL)
				case 0x3F: A = _cpuOps.Src8(A, 8); break; // SRL A

				default:
					//Debugger::stopMachine = true;
					//Log::Critical("Unimplemented (prefix-CB) opcode %02X", opcode);
					break;
			}

			if (opcode >= 0x40 && opcode <= 0x7F)
			{
				u8 bit = (u8)((opcode >> 3) & 0x7);

				switch (opcode & 0xF)
				{
					case 0x0: _cpuOps.BitTest(B, bit, 8); break; // BIT B,x
					case 0x1: _cpuOps.BitTest(C, bit, 8); break; // BIT C,x
					case 0x2: _cpuOps.BitTest(D, bit, 8); break; // BIT D,x
					case 0x3: _cpuOps.BitTest(E, bit, 8); break; // BIT E,x
					case 0x4: _cpuOps.BitTest(H, bit, 8); break; // BIT H,x
					case 0x5: _cpuOps.BitTest(L, bit, 8); break;  // BIT L,x
					case 0x6: _cpuOps.BitTestMem(HL.Reg, bit, 12); break; // BIT HL,x
					case 0x7: _cpuOps.BitTest(A, bit, 8); break; // BIT A,x
					case 0x8: _cpuOps.BitTest(B, bit, 8); break; // BIT B,x
					case 0x9: _cpuOps.BitTest(C, bit, 8); break; // BIT C,x
					case 0xA: _cpuOps.BitTest(D, bit, 8); break; // BIT D,x
					case 0xB: _cpuOps.BitTest(E, bit, 8); break; // BIT E,x
					case 0xC: _cpuOps.BitTest(H, bit, 8); break; // BIT H,x
					case 0xD: _cpuOps.BitTest(L, bit, 8); break; // BIT L,x
					case 0xE: _cpuOps.BitTestMem(HL.Reg, bit, 12); break; // BIT HL,x
					case 0xF: _cpuOps.BitTest(A, bit, 8); break; // BIT A,x
				}
			}
			else if (opcode >= 0x80 && opcode <= 0xBF)
			{
				u8 bit = (u8)((opcode >> 3) & 0x7);

				switch (opcode & 0xF)
				{
					case 0x0: B = _cpuOps.BitClear(B, bit, 8); break; // RES B,x
					case 0x1: C = _cpuOps.BitClear(C, bit, 8); break; // RES C,x
					case 0x2: D = _cpuOps.BitClear(D, bit, 8); break; // RES D,x
					case 0x3: E = _cpuOps.BitClear(E, bit, 8); break; // RES E,x
					case 0x4: H = _cpuOps.BitClear(H, bit, 8); break; // RES H,x
					case 0x5: L = _cpuOps.BitClear(L, bit, 8); break;  // RES L,x
					case 0x6: _cpuOps.BitClearMem(HL.Reg, bit, 16); break; // RES HL,x
					case 0x7: A = _cpuOps.BitClear(A, bit, 8); break; // RES A,x
					case 0x8: B = _cpuOps.BitClear(B, bit, 8); break; // RES B,x
					case 0x9: C = _cpuOps.BitClear(C, bit, 8); break; // RES C,x
					case 0xA: D = _cpuOps.BitClear(D, bit, 8); break; // RES D,x
					case 0xB: E = _cpuOps.BitClear(E, bit, 8); break; // RES E,x
					case 0xC: H = _cpuOps.BitClear(H, bit, 8); break; // RES H,x
					case 0xD: L = _cpuOps.BitClear(L, bit, 8); break; // RES L,x
					case 0xE: _cpuOps.BitClearMem(HL.Reg, bit, 16); break; // RES HL,x
					case 0xF: A = _cpuOps.BitClear(A, bit, 8); break; // RES A,x
				}
			}
			else if (opcode >= 0xC0 && opcode <= 0xFF)
			{
				u8 bit = (u8)((opcode >> 3) & 0x7);

				switch (opcode & 0xF)
				{
					case 0x0: B = _cpuOps.BitSet(B, bit, 8); break; // SET B,x
					case 0x1: C = _cpuOps.BitSet(C, bit, 8); break; // SET C,x
					case 0x2: D = _cpuOps.BitSet(D, bit, 8); break; // SET D,x
					case 0x3: E = _cpuOps.BitSet(E, bit, 8); break; // SET E,x
					case 0x4: H = _cpuOps.BitSet(H, bit, 8); break; // SET H,x
					case 0x5: L = _cpuOps.BitSet(L, bit, 8); break;  // SET L,x
					case 0x6: _cpuOps.BitSetMem(HL.Reg, bit, 16); break; // SET HL,x
					case 0x7: A = _cpuOps.BitSet(A, bit, 8); break; // SET A,x
					case 0x8: B = _cpuOps.BitSet(B, bit, 8); break; // SET B,x
					case 0x9: C = _cpuOps.BitSet(C, bit, 8); break; // SET C,x
					case 0xA: D = _cpuOps.BitSet(D, bit, 8); break; // SET D,x
					case 0xB: E = _cpuOps.BitSet(E, bit, 8); break; // SET E,x
					case 0xC: H = _cpuOps.BitSet(H, bit, 8); break; // SET H,x
					case 0xD: L = _cpuOps.BitSet(L, bit, 8); break; // SET L,x
					case 0xE: _cpuOps.BitSetMem(HL.Reg, bit, 16); break; // SET HL,x
					case 0xF: A = _cpuOps.BitSet(A, bit, 8); break; // SET A,x
				}
			}
		}

		// responsible for executing a cpu step
		public void Step()
		{
			int cycleCount = Cycles;

			//if (InstructionsRan >= 100000)
			//{
			//Cycles = (4194304 / 60);
			//return;
			//}

			_gameboy.Interrupts.Service();
			ExecuteOpcode();
			_gameboy.Timer.Update(Cycles - cycleCount);
			_gameboy.Ppu.Update(Cycles - cycleCount);
		}
	}
}
