using System;

namespace ASM_Simulator
{
    public class ZAEHLER
    {
        public int Wert = 0;
        private int Min = -1;
        private int Max = -1;
        private Int64 Gesamt = 0;
        public int Anzahl = 0;
        public bool Aktiv = false;
        public String Name = "";
        public int Begin = -1; // Zeile, die Zaehler startet
        public int Begin_Length = 0; // Namenslänge des Zählers
        public int Ende = -1; // Zeile, die Zaehler stoppt
        public int Ende_Length = 0; // Namenslänge des Zählers

        public void Start()
        {
            if (Begin == -1 || Ende == -1) return;
            Wert = 1;
            Aktiv = true;
        }

        public void Stopp()
        {
            if (Begin == -1 || Ende == -1) return;
            Aktiv = false;
            Anzahl++;
            Gesamt += Wert;
            if (Min == -1 || Wert < Min)
            {
                Min = Wert;
            }
            if (Max == -1 || Wert > Max)
            {
                Max = Wert;
            }
        }

        public int GetMin()
        {
            if (Min == -1) return 0;
            return Min;
        }

        public int GetMax()
        {
            if (Max == -1) return 0;
            return Max;
        }

        public int GetDurchschnitt()
        {
            if (Anzahl == 0) return 0;
            return (int)Gesamt / Anzahl;
        }
    }
}