using System;
using System.Linq;

namespace ASM_Simulator
{
    public class Def
    {
        // PORT Defintionen
        public int SREG;

        public int SPH;
        public int SPL;
        public int TIMSK;
        public int TIFR;
        public int WDTCR;
        public int UBRRH;
        public int UBRRL;
        public int EEARH;
        public int EEARL;
        public int EEDR;
        public int EECR;
        public int UDR;
        public int UCSRA;
        public int UCSRB;
        public int UCSRC;
        public int MCUCR;

        // Bit Defintionen
        public Byte I;

        public Byte T;
        public Byte H;
        public Byte S;
        public Byte V;
        public Byte N;
        public Byte Z;
        public Byte C;
        public Byte SP10;
        public Byte SP9;
        public Byte SP8;
        public Byte SP7;
        public Byte SP6;
        public Byte SP5;
        public Byte SP4;
        public Byte SP3;
        public Byte SP2;
        public Byte SP1;
        public Byte SP0;
        public Byte EEDR0;
        public Byte EEDR1;
        public Byte EEDR2;
        public Byte EEDR3;
        public Byte EEDR4;
        public Byte EEDR5;
        public Byte EEDR6;
        public Byte EEDR7;
        public Byte EERE;
        public Byte EEWE;
        public Byte EEMWE;
        public Byte EEWEE;
        public Byte EERIE;
        public Byte UDR0;
        public Byte UDR1;
        public Byte UDR2;
        public Byte UDR3;
        public Byte UDR4;
        public Byte UDR5;
        public Byte UDR6;
        public Byte UDR7;
        public Byte USR;
        public Byte MPCM;
        public Byte U2X;
        public Byte UPE;
        public Byte PE;
        public Byte DOR;
        public Byte FE;
        public Byte UDRE;
        public Byte TXC;
        public Byte RXC;
        public Byte UCR;
        public Byte TXB8;
        public Byte RXB8;
        public Byte UCSZ2;
        public Byte CHR9;
        public Byte TXEN;
        public Byte RXEN;
        public Byte UDRIE;
        public Byte TXCIE;
        public Byte RXCIE;
        public Byte UCPOL;
        public Byte UCSZ0;
        public Byte UCSZ1;
        public Byte USBS;
        public Byte UPM0;
        public Byte UPM1;
        public Byte UMSEL;
        public Byte URSEL;
        public Byte WDE;
        public Byte WDTCSR;
        public Byte WDP0;
        public Byte WDP1;
        public Byte WDP2;
        public Byte WDCE;
        public Byte WDTOE;
        public Byte SM0;
        public Byte SM1;
        public Byte SM2;
        public Byte SE;

        public Byte XH;
        public Byte XL;
        public Byte YH;
        public Byte YL;
        public Byte ZH;
        public Byte ZL;

        public void Reload(Atmega Main)
        {
            SREG = Main.getIORegister("SREG");
            SPH = Main.getIORegister("SPH");
            SPL = Main.getIORegister("SPL");
            TIMSK = Main.getIORegister("TIMSK");
            TIFR = Main.getIORegister("TIFR");
            WDTCR = Main.getIORegister("WDTCR");
            UBRRH = Main.getIORegister("UBRRH");
            UBRRL = Main.getIORegister("UBRRL");
            EEARH = Main.getIORegister("EEARH");
            EEARL = Main.getIORegister("EEARL");
            EEDR = Main.getIORegister("EEDR");
            EECR = Main.getIORegister("EECR");
            UDR = Main.getIORegister("UDR");
            UCSRA = Main.getIORegister("UCSRA");
            UCSRB = Main.getIORegister("UCSRB");
            UCSRC = Main.getIORegister("UCSRC");
            MCUCR = Main.getIORegister("MCUCR");

            WDTCSR = (Byte)Main.getVar("WDTCSR");
            WDP0 = (Byte)Main.getVar("WDP0");
            WDP1 = (Byte)Main.getVar("WDP1");
            WDP2 = (Byte)Main.getVar("WDP2");
            WDCE = (Byte)Main.getVar("WDCE");
            WDTOE = (Byte)Main.getVar("WDTOE");
            WDE = (Byte)Main.getVar("WDE");
            I = (Byte)Main.getVar("SREG_I");
            T = (Byte)Main.getVar("SREG_T");
            H = (Byte)Main.getVar("SREG_H");
            S = (Byte)Main.getVar("SREG_S");
            V = (Byte)Main.getVar("SREG_V");
            N = (Byte)Main.getVar("SREG_N");
            Z = (Byte)Main.getVar("SREG_Z");
            C = (Byte)Main.getVar("SREG_C");
            SP10 = (Byte)Main.getVar("SP10");
            SP9 = (Byte)Main.getVar("SP9");
            SP8 = (Byte)Main.getVar("SP8");
            SP7 = (Byte)Main.getVar("SP7");
            SP6 = (Byte)Main.getVar("SP6");
            SP5 = (Byte)Main.getVar("SP5");
            SP4 = (Byte)Main.getVar("SP4");
            SP3 = (Byte)Main.getVar("SP3");
            SP2 = (Byte)Main.getVar("SP2");
            SP1 = (Byte)Main.getVar("SP1");
            SP0 = (Byte)Main.getVar("SP0");
            EEDR0 = (Byte)Main.getVar("EEDR0");
            EEDR1 = (Byte)Main.getVar("EEDR1");
            EEDR2 = (Byte)Main.getVar("EEDR2");
            EEDR3 = (Byte)Main.getVar("EEDR3");
            EEDR4 = (Byte)Main.getVar("EEDR4");
            EEDR5 = (Byte)Main.getVar("EEDR5");
            EEDR6 = (Byte)Main.getVar("EEDR6");
            EEDR7 = (Byte)Main.getVar("EEDR7");
            EERE = (Byte)Main.getVar("EERE");
            EEWE = (Byte)Main.getVar("EEWE");
            EEMWE = (Byte)Main.getVar("EEMWE");
            EEWEE = (Byte)Main.getVar("EEWEE");
            EERIE = (Byte)Main.getVar("EERIE");
            UDR0 = (Byte)Main.getVar("UDR0");
            UDR1 = (Byte)Main.getVar("UDR1");
            UDR2 = (Byte)Main.getVar("UDR2");
            UDR3 = (Byte)Main.getVar("UDR3");
            UDR4 = (Byte)Main.getVar("UDR4");
            UDR5 = (Byte)Main.getVar("UDR5");
            UDR6 = (Byte)Main.getVar("UDR6");
            UDR7 = (Byte)Main.getVar("UDR7");
            USR = (Byte)Main.getVar("USR");
            MPCM = (Byte)Main.getVar("MPCM");
            U2X = (Byte)Main.getVar("U2X");
            UPE = (Byte)Main.getVar("UPE");
            PE = (Byte)Main.getVar("PE");
            DOR = (Byte)Main.getVar("DOR");
            FE = (Byte)Main.getVar("FE");
            UDRE = (Byte)Main.getVar("UDRE");
            TXC = (Byte)Main.getVar("TXC");
            RXC = (Byte)Main.getVar("RXC");
            UCR = (Byte)Main.getVar("UCR");
            TXB8 = (Byte)Main.getVar("TXB8");
            RXB8 = (Byte)Main.getVar("RXB8");
            UCSZ2 = (Byte)Main.getVar("UCSZ2");
            CHR9 = (Byte)Main.getVar("CHR9");
            TXEN = (Byte)Main.getVar("TXEN");
            RXEN = (Byte)Main.getVar("RXEN");
            UDRIE = (Byte)Main.getVar("UDRIE");
            TXCIE = (Byte)Main.getVar("TXCIE");
            RXCIE = (Byte)Main.getVar("RXCIE");
            UCPOL = (Byte)Main.getVar("UCPOL");
            UCSZ0 = (Byte)Main.getVar("UCSZ0");
            UCSZ1 = (Byte)Main.getVar("UCSZ1");
            USBS = (Byte)Main.getVar("USBS");
            UPM0 = (Byte)Main.getVar("UPM0");
            UPM1 = (Byte)Main.getVar("UPM1");
            UMSEL = (Byte)Main.getVar("UMSEL");
            URSEL = (Byte)Main.getVar("URSEL");
            SM0 = (Byte)Main.getVar("SM0");
            SM1 = (Byte)Main.getVar("SM1");
            SM2 = (Byte)Main.getVar("SM2");
            SE = (Byte)Main.getVar("SE");

            XH = (Byte)Main.getRegister("XH");
            XL = (Byte)Main.getRegister("XL");
            YH = (Byte)Main.getRegister("YH");
            YL = (Byte)Main.getRegister("YL");
            ZH = (Byte)Main.getRegister("ZH");
            ZL = (Byte)Main.getRegister("ZL");
        }
    }

    public static class Help
    {
        public static Random Zufall = new Random();

        public static String[] Orders = { "ADD", "ADC", "ADIW", "SUB", "SUBI", "SBC", "SBCI", "SBIW", "AND", "ANDI", "OR", "ORI", "EOR", "COM", "NEG", "SBR", "CBR", "INC", "DEC", "TST", "CLR", "SER", "MUL", "MULS", "MULSU", "FMUL", "FMULS", "FMULSU", "RJMP", "IJMP", "RCALL", "ICALL", "RET", "RETI", "CPSE", "CP", "CPC", "CPI", "SBRC", "SBRS", "SBIC", "SBIS", "BRBS", "BRBC", "BREQ", "BRNE", "BRCS", "BRCC", "BRSH", "BRLO", "BRMI", "BRPL", "BRGE", "BRLT", "BRHS", "BRHC", "BRTS", "BRTC", "BRVS", "BRVC", "BRIE", "BRID", "MOV", "MOVW", "LDI", "LD", "LDD", "LDS", "ST", "STD", "STS", "LPM", "SPM", "IN", "OUT", "PUSH", "POP", "SBI", "CBI", "LSL", "LSR", "ROL", "ROR", "ASR", "SWAP", "BSET", "BCLR", "BST", "BLD", "SEC", "CLC", "SEN", "CLN", "SEZ", "CLZ", "SEI", "CLI", "SES", "CLS", "SEV", "CLV", "SET", "CLT", "SEH", "CLH", "NOP", "WDR", "SLEEP" };

        public static String free(String text) // befreit den Text von Leerzeichen
        {
            if (text == null) return "error";
            for (int i = 0; i < text.Length; i++)
            {
                if (text.Substring(i, 1) == " " || text.Substring(i, 1) == "\t")
                {
                    if (i == text.Length - 1)
                    {
                        text = text.Substring(0, i);
                    }
                    else
                        if (i > 0)
                        {
                            text = text.Substring(0, i) + text.Substring(i + 1, text.Length - (i + 1));
                        }
                        else
                            if (i == 0 && text.Length == 1)
                            {
                                text = ",";
                            }
                            else
                                if (i == 0)
                                {
                                    text = text.Substring(1, text.Length - 1);
                                }
                    i--;
                }
            }
            return text;
        }

        public static bool IsNumber(String text) // gibt zurück, ob der Text eine umwandelbare Zahl darstellt
        {
            if (text == "") return false;
            for (int i = 0; i < text.Length; i++)
                if (text[i] < 48 || text[i] > 57)
                    return false;
            return true;
        }

        public static bool IsArithmetic(String text) // gibt zurück, ob der text eine Arithmetische Berechechnung darstellt
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] < '0' || text[i] > '9')
                {
                    if (!(text[i] == '+' || text[i] == '-' || text[i] == '*' || text[i] == '/' || text[i] == '<' || text[i] == '>' || text[i] == '|' || text[i] == '&' || text[i] == '(' || text[i] == ')'))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static Byte Low(int wert) // gibt das low-Byte
        {
            return (Byte)wert;
        }

        public static Byte High(int wert) // gibt das high-Byte
        {
            return (Byte)(wert >> 8);
        }

        public static String Arithmetic(Atmega Main, String Text, bool check_klammern) // Führt die Berechnung durch
        {
            int begin_klammer = -1;
            int anz_in = 0;
            int end_klammer = -1;
            if (IsNumber(Text)) return Text;
            if (check_klammern == true)
            {
                for (int i = 0; i < Text.Length; i++) // check ob noch klammern existieren
                {
                    if (Text[i] == '(' && begin_klammer == -1) begin_klammer = i;
                    if (Text[i] == '(') anz_in++;
                    if (Text[i] == ')' && anz_in > 1) anz_in--;
                    else
                        if (Text[i] == ')') { end_klammer = i; break; }
                }
            }

            if (begin_klammer != -1 && end_klammer != -1)
            {
                String p = Text.Substring(0, begin_klammer);
                String p2 = Text.Substring(end_klammer + 1, Text.Length - end_klammer - 1);
                String p3 = Text.Substring(begin_klammer + 1, end_klammer - begin_klammer - 1);
                Text = Arithmetic(Main, p + Arithmetic(Main, p3, true) + p2, true);
            }
            else
            {
                // operationen checken, da keine klammern mehr
                char[] sep = "<<".ToCharArray();
                String[] temp = Text.Split(sep);
                int res;
                if (temp.Count() >= 2)
                {
                    res = Main.INT(Arithmetic(Main, temp[0], false));
                    for (int i = 1; i < temp.Count(); i++)
                    {
                        if (temp[i] == ",") continue;
                        res = res << Main.INT(Arithmetic(Main, temp[i], false));
                    }
                    return res.ToString();
                }

                sep = ">>".ToCharArray();
                temp = Text.Split(sep);
                if (temp.Count() >= 2)
                {
                    res = Main.INT(Arithmetic(Main, temp[0], false));
                    for (int i = 1; i < temp.Count(); i++)
                    {
                        if (temp[i] == ",") continue;
                        res = res >> Main.INT(Arithmetic(Main, temp[i], false));
                    }
                    return res.ToString();
                }

                sep = "|".ToCharArray();
                temp = Text.Split(sep);
                if (temp.Count() >= 2)
                {
                    Text = ",";
                    res = Main.INT(Arithmetic(Main, temp[0], false));
                    for (int i = 1; i < temp.Count(); i++) res = res | Main.INT(Arithmetic(Main, temp[i], false));
                    return res.ToString();
                }

                sep = "&".ToCharArray();
                temp = Text.Split(sep);
                if (temp.Count() >= 2)
                {
                    Text = ",";
                    res = Main.INT(Arithmetic(Main, temp[0], false));
                    for (int i = 1; i < temp.Count(); i++) res = res & Main.INT(Arithmetic(Main, temp[i], false));
                    return res.ToString();
                }

                temp = Text.Split('+');
                if (temp.Count() >= 2)
                {
                    Text = ",";
                    res = 0;
                    for (int i = 0; i < temp.Count(); i++) res += Main.INT(Arithmetic(Main, temp[i], false));
                    return res.ToString();
                }

                temp = Text.Split('-');
                if (temp.Count() >= 2)
                {
                    res = Main.INT(Arithmetic(Main, temp[0], false));
                    for (int i = 1; i < temp.Count(); i++) res -= Main.INT(Arithmetic(Main, temp[i], false));
                    return res.ToString();
                }

                temp = Text.Split('*');
                if (temp.Count() >= 2)
                {
                    res = 1;
                    Text = ",";
                    for (int i = 0; i < temp.Count(); i++) res *= Main.INT(Arithmetic(Main, temp[i], false));
                    return res.ToString();
                }

                temp = Text.Split('/');
                if (temp.Count() >= 2)
                {
                    Text = ",";
                    res = Main.INT(Arithmetic(Main, temp[0], false));
                    for (int i = 1; i < temp.Count(); i++)
                    {
                        int reb = Main.INT(Arithmetic(Main, temp[i], false));
                        if (reb != 0) res /= reb;
                    }
                    return res.ToString();
                }
            }

            return Text;
        }

        public static String ConvertToAscii(Byte Wert)
        {
            if (Wert <= 31 || Wert >= 127) return ",";
            String res = ((char)Wert).ToString();
            return res;
        }
    }
}