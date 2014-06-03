using System;
using System.Linq;

namespace ASM_Simulator
{
    public class EVENT
    {
        public Atmega Ziel = null;
        public int Ziel_ID = -1;
        public int Typ = 0;

        public bool setted = false;
        public int Time = 1;
        public bool Random_Time = false;
        public int Random_Time_From = 1;
        public int Random_Time_To = 1;

        public int Param1 = 0;
        public bool Param1_Random = false;
        public int Param1_From = 0;
        public int Param1_To = 0;

        public int Param2 = 0;
        public bool Param2_Random = false;
        public int Param2_From = 0;
        public int Param2_To = 0;

        public int Param3 = 0;
        public bool Param3_Random = false;
        public int Param3_From = 0;
        public int Param3_To = 0;

        public EVENT()
        {
        }

        public void Execute()
        {
            if (Ziel == null) return;
            int a = Param1; // !Param1_Random ? Param1 : Help.Zufall.Next(Param1_From, Param1_To);
            int b = Param2; // !Param2_Random ? Param2 : Help.Zufall.Next(Param2_From, Param2_To);
            switch (Typ)
            {
                case 0: // Arbeitsregister
                    Ziel.Register[a] = (Byte)b;
                    break;

                case 1: // SRAM
                    Ziel.SRAM[a] = (Byte)b;
                    break;

                case 2: // Ports
                    Ziel.Ports[a].set((Byte)b);
                    break;

                case 3: // EEPORM
                    Ziel.EEPROM.SPEICHER[a] = (Byte)b;
                    break;

                case 4: // PM
                    Ziel.PM[a] = (Byte)b;
                    break;

                case 5: // minianwendungen
                    int dat = Param1 + 1;
                    if (dat >= Ziel.Counter.Count) break;
                    Ziel.Selected_Software = dat;
                    Ziel.Counter[dat] = 0;
                    int temp = Ziel.Sleep;
                    byte[] Register = new byte[Ziel.Register.Count()];
                    for (int i = 0; i < Ziel.Register.Count(); i++) Register[i] = Ziel.Register[i];
                    for (int i = 0; i < Param2 && Ziel.Step(null); i++) { }
                    for (int i = 0; i < Ziel.Register.Count(); i++) Ziel.Register[i] = Register[i];
                    Ziel.Sleep = temp;
                    Ziel.Selected_Software = 0;
                    break;
            }
        }

        public void Reset()
        {
            if (Random_Time) Time = Help.Zufall.Next(Random_Time_From, Random_Time_To);
            if (Typ != 5)
            {
                if (Param1_Random) Param1 = Help.Zufall.Next(Param1_From, Param1_To);
                if (Param2_Random) Param2 = Help.Zufall.Next(Param2_From, Param2_To);
                if (Param3_Random) Param3 = Help.Zufall.Next(Param3_From, Param3_To);
            }
        }
    }
}