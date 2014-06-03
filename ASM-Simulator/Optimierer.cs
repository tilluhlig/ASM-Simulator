using System;
using System.Collections.Generic;
using System.Linq;

namespace ASM_Simulator
{
    internal class Optimierer
    {
        private static int[] Z_Generator = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 35, 36, 37, 79, 80, 81, 82, 83 };
        private static int[] Z_Akzeptor = { 44, 45 };
        private static int[] N_Generator = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 35, 36, 37, 79, 80, 81, 82, 83 };
        private static int[] N_Akzeptor = { 50, 51, 52, 53 };

        private bool IsElement(int a, int[] Liste)
        {
            for (int i = 0; i < Liste.Count(); i++)
                if (Liste[i] == a)
                    return true;
            return false;
        }

        public List<Zeile> Generate(List<Zeile> Program)
        {
            for (int i = 0; i < Program.Count - 1; i++)
            {
                //Program[i].SetFlags = (Byte)(Program[i].SetFlags & 239);
                if (IsElement(Program[i].Typ, Z_Generator) && IsElement(Program[i + 1].Typ, Z_Generator))
                {
                    Program[i].SetFlags = (Byte)(Program[i].SetFlags & 253);
                }

                if (IsElement(Program[i].Typ, N_Generator) && IsElement(Program[i + 1].Typ, N_Generator))
                {
                    Program[i].SetFlags = (Byte)(Program[i].SetFlags & 251);
                }
            }

            return Program;
        }
    }
}