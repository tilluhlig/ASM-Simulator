using System;
using System.Collections.Generic;

namespace ASM_Simulator
{
    public class Zeile
    {
        public Zeile(int typ, int Para1, int Para2, int Para3, int Origina)
        {
            Typ = typ;
            Param1 = Para1;
            Param2 = Para2;
            Param3 = Para3;
            Original = Origina;
        }

        public void SetBreakpoint()
        {
            Breakpoint = true;
        }

        public void UnsetBreakpoint()
        {
            Breakpoint = false;
        }

        public bool Breakpoint = false;
        public bool Breakover = false;
        public int Typ; // Befehlsart
        public int Param1; // Parameter1
        public int Param2; // Parameter2
        public int Param3; // Parameter3
        public int Original; // Originalzeile im Quelltext
        public Byte SetFlags = 255;
        public List<short> Zaehler_Start = new List<short>(); // zu startende Zähler
        public List<short> Zaehler_Ende = new List<short>(); // zu stoppende Zähler
    }
}