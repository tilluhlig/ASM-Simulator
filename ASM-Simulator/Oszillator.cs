using System.Collections.Generic;

namespace ASM_Simulator
{
    public class Oszillator
    {
        private List<int> Daten = new List<int>();
        private short Pos = 0;
        public bool Aktiv = false;

        public int Anzahl = 0;

        public void Add(bool Zustand)
        {
            if (Pos == 32) { Daten.Add(0); Pos = 0; }
            if (Zustand) Daten[Daten.Count - 1] = Daten[Daten.Count - 1] | (1 << Pos);
            Pos++;
        }

        public bool Get(int Position)
        {
            int Bit = (int)(Position % 32);
            Position = Position / 32;
            return (Daten[Position] & (1 << Bit)) > 0 ? true : false;
        }
    }
}