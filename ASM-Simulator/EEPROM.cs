namespace ASM_Simulator
{
    public class EEPROM
    {
        private int Sleep;
        public int Anz_Read = 0;
        public int Anz_Write = 0;
        public byte[] SPEICHER = null;

        public EEPROM(int Anz)
        {
            SPEICHER = new byte[Anz];
        }

        public void update(Atmega Main, Def INC)
        {
            if (Sleep > 0)
            {
                Sleep--;
                if (Sleep == 0)
                {
                    Main.SetBitIOPort(INC.EECR, INC.EEWE, false);
                    if (Main.GetBitIOPort(INC.EECR, INC.EERIE) && Main.GetBitIOPort(INC.SREG, INC.I))
                    {
                        if (Main.ExecuteInterrupt("ERDYaddr"))
                        {
                            Main.SetBitIOPort(INC.SREG, INC.I, false);
                        }
                    }
                }
            }

            // Schreiben
            if (Main.GetBitIOPort(INC.EECR, INC.EEMWE) && Main.GetBitIOPort(INC.EECR, INC.EEWE))
            {
                Main.SetBitIOPort(INC.EECR, INC.EEMWE, false);
                SPEICHER[Main.LowHigh(Main.Ports[INC.EEARL].get(), Main.Ports[INC.EEARH].get())] = Main.Ports[INC.EEDR].get();
                Sleep = 8448 * (Main.Frequenz / 1000000);
                Main.Sleep += 2;
                Anz_Write++;
            }

            // Lesen
            if (Main.GetBitIOPort(INC.EECR, INC.EERE) && !(Main.GetBitIOPort(INC.EECR, INC.EEMWE) || Main.GetBitIOPort(INC.EECR, INC.EEWE)))
            {
                Main.Ports[INC.EEDR].set(SPEICHER[Main.LowHigh(Main.Ports[INC.EEARL].get(), Main.Ports[INC.EEARH].get())]);
                Main.Sleep += 4;
                Main.SetBitIOPort(INC.EECR, INC.EERE, false);
                Anz_Read++;
            }
        }
    }
}