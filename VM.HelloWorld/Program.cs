﻿using System;
using System.Diagnostics;
using System.Text;

namespace VM.HelloWorld
{
    class Program
    {
        const ushort CONSOLEADDRESS = 0xf000;
        const byte WIDTH = 80;
        const byte HEIGHT = 25;

        static void Main(string[] args)
        {
            var memory = new Memory(0x10000);
            var console = new IO.SystemConsole(memory, CONSOLEADDRESS, WIDTH, HEIGHT);
            var processor = new Processor(memory, 0x800);
            var flasher = new Flasher(memory);

            // Initialize registers
            flasher.WriteInstruction(Instruction.LDVR, CONSOLEADDRESS, Register.R0);
            flasher.WriteInstruction(Instruction.LDVR, CONSOLEADDRESS + (WIDTH * HEIGHT), Register.R1);

            var loopAddress = flasher.Address;

            // write '#' to entire console
            flasher.WriteInstruction(Instruction.SBVR, (byte)'#', Register.R0);
            flasher.WriteInstruction(Instruction.INC, Register.R0);
            flasher.WriteInstruction(Instruction.CMP, Register.R0, Register.R1);
            flasher.WriteInstruction(Instruction.JNE, loopAddress);

            // write string to Console
            var address = CONSOLEADDRESS;

            foreach (var b in Encoding.ASCII.GetBytes("Hello, World!\0"))
            {
                flasher.WriteInstruction(Instruction.SBVA, b, address++);
            }

            // Increment R0 from 0 -> 65,535
            flasher.WriteInstruction(Instruction.LDVR, (ushort)Flags.CARRY, Register.R1);

            loopAddress = flasher.Address;

            flasher.WriteInstruction(Instruction.INC, Register.R0);
            flasher.WriteInstruction(Instruction.AND, Register.FLAG, Register.R1);
            flasher.WriteInstruction(Instruction.JZ, loopAddress);

            // Halt program
            flasher.WriteInstruction(Instruction.HALT);

            var instructions = 0;

            processor.Tick += (o, e) => instructions++;
            processor.Halt += (o, e) => console.Close();

            var start = DateTime.Now;
            processor.Run();
            var end = DateTime.Now;

            var duration = end - start;

            var instructionsPerSecond = instructions / (duration.TotalMilliseconds / 1000.0);

            System.Console.WriteLine($"Ran {instructions} instructions in {duration}");
            System.Console.WriteLine($"{instructionsPerSecond} instructions per second");

            if (Debugger.IsAttached)
            {
                System.Console.ReadLine();
            }
        }
    }
}