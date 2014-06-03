using System;

namespace ASM_Simulator
{
    // Simuliert die Timer 8Bit und 16Bit
    public class TIMER
    {
        private const int SREGC = 1;
        private const int SREGZ = 2;
        private const int SREGN = 4;
        private const int SREGV = 8;
        private const int SREGS = 16;
        private const int SREGH = 32;
        private const int SREGT = 64;
        private const int SREGI = 128;

        private const int SREGNOC = 254;
        private const int SREGNOZ = 253;
        private const int SREGNON = 251;
        private const int SREGNOV = 247;
        private const int SREGNOS = 239;
        private const int SREGNOH = 223;
        private const int SREGNOT = 191;
        private const int SREGNOI = 127;

        public int ID;
        public int Typ;
        public int Sprungziel;
        public int Sleep;
        public int ANZ;

        #region Def

        private int PRESC_MASKE;
        private int TCNT = -1;
        private int TCNTL;
        private int TCNTH;
        private int TCCR;
        private int TCCRA;
        private int TCCRB;
        private int OCRAL;
        private int OCRAH;
        private int OCRBL;
        private int OCRBH;
        private int OCR;

        private Byte TOIE;
        private Byte OCIE;
        private Byte TOV;
        private Byte OCF;
        private Byte CS0;
        private Byte CS1;
        private Byte CS2;
        private Byte WGM1;
        private Byte CTC;
        private Byte COM0;
        private Byte COM1;
        private Byte WGM0;
        private Byte PWM;
        private Byte FOC;
        private Byte OCIEA;
        private Byte OCIEB;
        private Byte OCFA;
        private Byte OCFB;

        #endregion Def

        public void Reload_INC(Atmega Main)
        {
            if (TCNT != -1) return;
            TCNT = Main.getIORegister("TCNT" + ID);
            TCNTL = Main.getIORegister("TCNT" + ID + "L");
            TCNTH = Main.getIORegister("TCNT" + ID + "H");
            TCCR = Main.getIORegister("TCCR" + ID);
            TCCRA = Main.getIORegister("TCCR" + ID + "A");
            TCCRB = Main.getIORegister("TCCR" + ID + "B");
            OCRAL = Main.getIORegister("OCR" + ID + "AL");
            OCRAH = Main.getIORegister("OCR" + ID + "AH");
            OCRBL = Main.getIORegister("OCR" + ID + "BL");
            OCRBH = Main.getIORegister("OCR" + ID + "BH");
            OCR = Main.getIORegister("OCR" + ID);

            TOIE = (Byte)Main.INT("TOIE" + ID);
            TOV = (Byte)Main.INT("TOV" + ID);
            OCF = (Byte)Main.INT("OCF" + ID);
            CS0 = (Byte)Main.INT("CS" + ID + "0");
            CS1 = (Byte)Main.INT("CS" + ID + "1");
            CS2 = (Byte)Main.INT("CS" + ID + "2");
            WGM1 = (Byte)Main.INT("WGM" + ID + "1");
            CTC = (Byte)Main.INT("CTC" + ID);
            COM0 = (Byte)Main.INT("COM" + ID + "0");
            COM1 = (Byte)Main.INT("COM" + ID + "1");
            WGM0 = (Byte)Main.INT("WGM" + ID + "0");
            PWM = (Byte)Main.INT("PWM" + ID);
            FOC = (Byte)Main.INT("FOC" + ID);
            OCIE = (Byte)Main.INT("OCIE" + ID);
            OCIEA = (Byte)Main.INT("OCIE" + ID + "A");
            OCIEB = (Byte)Main.INT("OCIE" + ID + "B");
            OCFA = (Byte)Main.INT("OCF" + ID + "A");
            PRESC_MASKE = (((0 | (1 << CS0)) | (1 << CS1)) | (1 << CS2));
            OCFB = (Byte)Main.INT("OCF" + ID + "B");
        }

        public TIMER(int Identifikation, int Sorte, int Anzahl)
        {
            ID = Identifikation;
            Typ = Sorte;
            Sprungziel = 0;
            Sleep = 0;
            ANZ = Anzahl;
        }

        private int[] Prescaler = { 0, 1, 8, 64, 256, 1024, 0, 0 };

        public void update(Atmega Main, Def INC)
        {
            if (Sleep > 0) { Sleep--; return; }
            if (Typ == 0)
            {
                if ((Main.Ports[TCCR].get() & PRESC_MASKE) == 0) return;
                int a = TCNT;
                // Overflow prüfen
                if (Main.GetBitIOPort(INC.TIFR, TOV) && Main.GetBitIOPort(INC.TIMSK, TOIE) && (Main.SREG & SREGI) > 0)
                {
                    if (Main.ExecuteInterrupt("OVF" + ID + "addr"))
                    {
                        Main.SetBitIOPort(INC.TIFR, TOV, false);
                        //Main.SetBitIOPort(INC.SREG, INC.I, false);
                        Main.SREG = (Byte)(Main.SREG & SREGNOI);
                    }
                }

                // Compare Prüfen
                if (Main.GetBitIOPort(INC.TIFR, OCF) && Main.GetBitIOPort(INC.TIMSK, OCIE) && (Main.SREG & SREGI) > 0)
                {
                    if (Main.ExecuteInterrupt("OC" + ID + "Aaddr"))
                    {
                        Main.SetBitIOPort(INC.TIFR, OCF, false);
                        //Main.SetBitIOPort(INC.SREG, INC.I, false);
                        Main.SREG = (Byte)(Main.SREG & SREGNOI);
                        Main.Ports[a].set(0);
                    }
                }

                // Test ob Aktiv
                int presc = Prescaler[Main.Ports[TCCR].get() & PRESC_MASKE];
                // Counter erhöhen
                int res = Main.Ports[a].get();
                bool Over = false;
                res++;
                Main.Ports[a].set(Help.Low(res));
                if (Main.Ports[a].get() == 0) Over = true; // Overflow eingetreten
                Sleep = presc - 1;
                // Compare prüfen
                int c = OCR;

                if (Over)
                {// Overflow Treffer
                    Main.SetBitIOPort(INC.TIFR, TOV, true);
                }

                // Compare
                if (res == Main.Ports[c].get())
                {// Treffer
                    Main.SetBitIOPort(INC.TIFR, OCF, true);
                }
            }
            else
                if (Typ == 1)
                {
                    if ((Main.Ports[TCCRB].get() & PRESC_MASKE) == 0) return;
                    int a = TCNTL;
                    int b = TCNTH;

                    // Overflow prüfen
                    if (Main.GetBitIOPort(INC.TIFR, TOV) && Main.GetBitIOPort(INC.TIMSK, TOIE) && (Main.SREG & SREGI) > 0)
                    {
                        if (Main.ExecuteInterrupt("OVF" + ID + "addr"))
                        {
                            Main.SetBitIOPort(INC.TIFR, TOV, false);
                            //Main.SetBitIOPort(INC.SREG, INC.I, false);
                            Main.SREG = (Byte)(Main.SREG & SREGNOI);
                        }
                    }

                    // Compare A Prüfen
                    if (Main.GetBitIOPort(INC.TIFR, OCFA) && Main.GetBitIOPort(INC.TIMSK, OCIEA) && (Main.SREG & SREGI) > 0)
                    {
                        if (Main.ExecuteInterrupt("OC" + ID + "Aaddr"))
                        {
                            Main.SetBitIOPort(INC.TIFR, OCFA, false);
                            //Main.SetBitIOPort(INC.SREG, INC.I, false);
                            Main.SREG = (Byte)(Main.SREG & SREGNOI);
                            Main.Ports[a].set(0);
                            Main.Ports[b].set(0);
                        }
                    }

                    // Compare B Prüfen
                    if (Main.GetBitIOPort(INC.TIFR, OCFB) && Main.GetBitIOPort(INC.TIMSK, OCIEB) && (Main.SREG & SREGI) > 0)
                    {
                        if (Main.ExecuteInterrupt("OC1Baddr"))
                        {
                            Main.SetBitIOPort(INC.TIFR, OCFB, false);
                            // Main.SetBitIOPort(INC.SREG, INC.I, false);
                            Main.SREG = (Byte)(Main.SREG & SREGNOI);
                            Main.Ports[a].set(0);
                            Main.Ports[b].set(0);
                        }
                    }

                    // Test ob Aktiv
                    int presc = Prescaler[Main.Ports[TCCRB].get() & PRESC_MASKE];
                    // Counter erhöhen
                    int res = Main.LowHigh(Main.Ports[a].get(), Main.Ports[b].get());
                    bool Over = false;
                    res++;
                    Main.Ports[a].set(Help.Low(res));
                    Main.Ports[b].set(Help.High(res));
                    if (Main.Ports[a].get() == 0 && Main.Ports[b].get() == 0) Over = true; // Overflow eingetreten
                    Sleep = presc - 1;

                    // Compare prüfen

                    if (Over)
                    {// Overflow Treffer
                        Main.SetBitIOPort(INC.TIFR, TOV, true);
                    }

                    // A Compare
                    if (res == Main.LowHigh(Main.Ports[OCRAL].get(), Main.Ports[OCRAH].get()))
                    {// A Treffer
                        Main.SetBitIOPort(INC.TIFR, OCFA, true);
                    }

                    // B Compare
                    if (res == Main.LowHigh(Main.Ports[OCRBL].get(), Main.Ports[OCRBH].get()))
                    {// B Treffer
                        Main.SetBitIOPort(INC.TIFR, OCFB, true);
                    }
                }
        }
    }
}