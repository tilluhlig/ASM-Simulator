using System;

namespace ASM_Simulator
{
    // Simuliert USART
    public class USART
    {
        private int ID;
        private int Sleep;
        private int ANZ;

        private bool Parity;
        private int StopBits;
        private int OperationMode;
        private int DataBits;
        private int BAUDRATE;
        private bool DoubleSpeed;

        #region Def

        private int UDR = -1;
        private int UBRRH;
        private int UCSRC;
        private int UCSRA;
        private int UCSRB;
        private int UBRRL;

        private Byte UDR0;
        private Byte UDR1;
        private Byte UDR2;
        private Byte UDR3;
        private Byte UDR4;
        private Byte UDR5;
        private Byte UDR6;
        private Byte UDR7;
        private Byte USR;
        private Byte MPCM;
        private Byte U2X;
        private Byte UPE;
        private Byte PE;
        private Byte DOR;
        private Byte FE;
        private Byte UDRE;
        private Byte TXC;
        private Byte RXC;
        private Byte UCR;
        private Byte TXB8;
        private Byte RXB8;
        private Byte UCSZ2;
        private Byte CHR9;
        private Byte TXEN;
        private Byte RXEN;
        private Byte UDRIE;
        private Byte TXCIE;
        private Byte RXCIE;
        private Byte UCPOL;
        private Byte UCSZ0;
        private Byte UCSZ1;
        private Byte USBS;
        private Byte UPM0;
        private Byte UPM1;
        private Byte UMSEL;
        private Byte URSEL;

        #endregion Def

        public void Reload_INC(Atmega Main)
        {
            string ADD = ANZ > 1 ? ID.ToString() : "";
            string ADD2 = ANZ > 1 ? ID.ToString() + "_" : "";
            UDR = Main.getIORegister("UDR" + ADD);
            UBRRH = Main.getIORegister("UBRR" + ADD + "H");
            UCSRC = Main.getIORegister("UCSR" + ADD + "C");
            UCSRA = Main.getIORegister("UCSR" + ADD + "A");
            UCSRB = Main.getIORegister("UCSR" + ADD + "B");
            UBRRL = Main.getIORegister("UBRR" + ADD + "L");

            UDR0 = (Byte)Main.getVar("UDR" + ADD2 + "0");
            UDR1 = (Byte)Main.getVar("UDR" + ADD2 + "1");
            UDR2 = (Byte)Main.getVar("UDR" + ADD2 + "2");
            UDR3 = (Byte)Main.getVar("UDR" + ADD2 + "3");
            UDR4 = (Byte)Main.getVar("UDR" + ADD2 + "4");
            UDR5 = (Byte)Main.getVar("UDR" + ADD2 + "5");
            UDR6 = (Byte)Main.getVar("UDR" + ADD2 + "6");
            UDR7 = (Byte)Main.getVar("UDR" + ADD2 + "7");
            USR = (Byte)Main.getVar("USR" + ADD);
            MPCM = (Byte)Main.getVar("MPCM" + ADD);
            U2X = (Byte)Main.getVar("U2X" + ADD);
            UPE = (Byte)Main.getVar("UPE" + ADD);
            PE = (Byte)Main.getVar("PE" + ADD);
            DOR = (Byte)Main.getVar("DOR" + ADD);
            FE = (Byte)Main.getVar("FE" + ADD);
            UDRE = (Byte)Main.getVar("UDRE" + ADD);
            TXC = (Byte)Main.getVar("TXC" + ADD);
            RXC = (Byte)Main.getVar("RXC" + ADD);
            UCR = (Byte)Main.getVar("UCR" + ADD);
            TXB8 = (Byte)Main.getVar("TXB8" + ADD);
            RXB8 = (Byte)Main.getVar("RXB8" + ADD);
            UCSZ2 = (Byte)Main.getVar("UCSZ" + ADD + "2");
            CHR9 = (Byte)Main.getVar("CHR9" + ADD);
            TXEN = (Byte)Main.getVar("TXEN" + ADD);
            RXEN = (Byte)Main.getVar("RXEN" + ADD);
            UDRIE = (Byte)Main.getVar("UDRIE" + ADD);
            TXCIE = (Byte)Main.getVar("TXCIE" + ADD);
            RXCIE = (Byte)Main.getVar("RXCIE" + ADD);
            UCPOL = (Byte)Main.getVar("UCPOL" + ADD);
            UCSZ0 = (Byte)Main.getVar("UCSZ" + ADD + "0");
            UCSZ1 = (Byte)Main.getVar("UCSZ" + ADD + "1");
            USBS = (Byte)Main.getVar("USBS" + ADD);
            UPM0 = (Byte)Main.getVar("UPM" + ADD + "0");
            UPM1 = (Byte)Main.getVar("UPM" + ADD + "1");
            UMSEL = (Byte)Main.getVar("UMSEL" + ADD);
            URSEL = (Byte)Main.getVar("URSEL" + ADD);
        }

        public USART(int Identifikation, int Anzahl)
        {
            ANZ = Anzahl;
            ID = Identifikation;
        }

        public void check_Konfiguration(Atmega Main)
        {
            OperationMode = Main.GetBitIOPort(UCSRC, UMSEL) ? 1 : 0;
            Parity = Main.GetBitIOPort(UCSRC, UPM1) || Main.GetBitIOPort(UCSRC, UPM0) ? true : false;
            StopBits = Main.GetBitIOPort(UCSRC, USBS) ? 2 : 1;
            int[] Data = { 5, 6, 7, 8, 0, 0, 0, 9 };
            DataBits = Data[(Main.GetBitIOPort(Main.INC.UCSRC, Main.INC.UCSZ0) ? 1 : 0) + (Main.GetBitIOPort(UCSRC, UCSZ1) ? 2 : 0) + (Main.GetBitIOPort(UCSRC, UCSZ2) ? 4 : 0)];
            DoubleSpeed = Main.GetBitIOPort(UCSRA, U2X) ? true : false;
            BAUDRATE = Main.Frequenz / (((Main.LowHigh(Main.Ports[UBRRL].get(), Main.Ports[UBRRH].get()) << 1) >> 1) + 1) * 16;
        }

        public void Receive(Atmega Main, int Text)
        {
            if (Main.GetBitIOPort(UCSRB, RXEN))
            {
                Main.SetBitIOPort(UCSRA, RXC, true);
                Main.Ports[Main.INC.UDR].set((Byte)Text); // in UDR laden
                if (DataBits == 9) Main.SetBitIOPort(UCSRB, RXB8, true); // 9tes bit setzen
                // Event abfragen
                if (Main.GetBitIOPort(UCSRB, RXCIE) && Main.GetBitIOPort(Main.INC.SREG, Main.INC.I) && Main.GetBitIOPort(UCSRA, RXC))
                {
                    if (Main.ExecuteInterrupt("URXCaddr"))
                    {
                        Main.SetBitIOPort(Main.INC.SREG, Main.INC.I, false);
                        Main.SetBitIOPort(UCSRA, RXC, false);
                    }
                }
            }
        }

        public void update(Atmega Main)
        {
            //  if (UDR == -1) Reload_INC(Main);

            if (Sleep > 0)
            {
                Sleep--;
                if (Sleep == 0)
                {
                }

                return;
            }

            string ADD = ANZ > 1 ? ID.ToString() : "";
            // Receiver
            if (Main.GetBitIOPort(UCSRB, RXEN))
            {
                PIN a = null; // Main.GetTypPin("RXD" + ADD);
                if (a == null) goto trans;
                //  if (Main.Ports[a.PORT].Typ[a.ID][0] != "RXD"+ADD)
                // {
                // for (int i = 1; i < Main.Ports[a.PORT].Typ[a.ID].Count; i++)
                //    if (Main.Ports[a.PORT].Typ[a.ID][i] == "RXD"+ADD)
                //  { Main.Ports[a.PORT].Typ[a.ID][i] = Main.Ports[a.PORT].Typ[a.ID][0]; Main.Ports[a.PORT].Typ[a.ID][0] = "RXD"+ADD; break; }

                //}
            }

        trans:
            // Transmitter
            if (Main.GetBitIOPort(UCSRB, TXEN))
            {
                PIN a = null; // Main.GetTypPin("TXD" + ADD);
                if (a == null) return;
                int Bits = DataBits + StopBits + 1 + (Parity ? 1 : 0);
                Sleep = BAUDRATE == 0 ? 0 : Main.Frequenz / BAUDRATE * Bits;
                Sleep = DoubleSpeed ? Sleep / 2 : Sleep;
            }
        }
    }
}