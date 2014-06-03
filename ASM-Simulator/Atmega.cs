using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ASM_Simulator
{
    public class Atmega
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

        public bool EEPROM_AKTIV = true;
        public bool Timer_AKTIV = true;
        public bool USART_AKTIV = true;
        public bool Watchdog_AKTIV = true;
        public bool Szenarien_AKTIV = true;

        public List<PORT> Ports;
        public int ID;
        public Byte[] Register;
        public int Frequenz = 16000000;
        public List<int> STACK;
        public List<TIMER> Timer;
        public Byte SREG = 0; // SREG zur beschleunigung extra nutzen

        public List<List<Zeile>> Program = new List<List<Zeile>>();
        public List<String[]> Quelltext = new List<String[]>();
        public List<Stopwatch> Watch_Minianwendungen = new List<Stopwatch>();
        public List<int> Watch_Takte = new List<int>();
        public List<int> Counter = new List<int>();
        public int Sleep;
        public int Time;
        public String Datei;
        public String QuellDatei;
        public String Name;
        public String Bez;
        public WATCHDOG WATCHDOG = null;
        public EEPROM EEPROM = null;
        public RichTextBox Text = null;
        public List<USART> USART = new List<USART>();
        public List<SZENARIEN> scenery = new List<SZENARIEN>();
        public SLEEP SLEEP = new SLEEP();
        public List<PORT> ActiveOszi = new List<PORT>();
        public List<short> ActiveOsziPin = new List<short>();

        public int[] Count_Orders = new int[108];
        public Stopwatch[] Watches = null;
        public Stopwatch hauptWatch = new Stopwatch();
        public int Selected_Software = 0;

        public Def INC = new Def();

        // Temporäre daten
        public List<String> Regs = new List<String>();

        public List<int> RegsPos = new List<int>();

        // Label / Sprungmarken
        public List<List<String>> Label = new List<List<String>>();

        public List<List<int>> LabelPos = new List<List<int>>();

        // SRAM, Arbeitsspeicher
        public List<Byte> SRAM;

        public List<String> SRAM_name = new List<String>();

        // Programmemory, LPM, SPM
        public List<Byte> PM = new List<Byte>();

        public List<String> PM_name = new List<String>();

        // Variablen aus .INC
        public List<String> Var_name = new List<String>();

        public List<int> Var = new List<int>();
        public List<String> Interrupt_name = new List<String>();
        public List<int> Interrupt = new List<int>(); // Ziel
        public List<int> Interrupt_Def = new List<int>(); // nach INC definiert
        public List<ZAEHLER> ZAEHLER = new List<ZAEHLER>();

        public PIN GetTypPin(String Typ) // Gibt den Pin zurück, der den Typ besitzt. z.B. GetTypPin("RXD");
        {
            for (int i = 0; i < Ports.Count; i++)
            {
                if (Ports[i].Bezeichnung.Length < 4) continue;
                if (Ports[i].Bezeichnung.Substring(0, 4) != "PORT") continue;
                for (int b = 0; b < Ports[i].Typ.Count(); b++)
                {
                    for (int c = 0; c < Ports[i].Typ[b].Count(); c++)
                    {
                        if (Typ == Ports[i].Typ[b][c]) return new PIN(b, Ports[i].Bezeichnung.Substring(4, Ports[i].Bezeichnung.Length - 4), i);
                    }
                }
            }
            return null;
        }

        public void Generate_Timer(int id, int Typ, int Anzahl) // erstellt einen neuen Timer
        {
            Timer.Add(new TIMER(id, Typ, Anzahl));
            if (Typ == 0)
            {
                Ports.Add(new PORT("OCR" + id, 8));
                Ports.Add(new PORT("TCNT" + id, 8));
                Ports.Add(new PORT("TCCR" + id, 8));
            }
            else
                if (Typ == 1)
                {
                    Ports.Add(new PORT("OCR" + id + "AH", 8));
                    Ports.Add(new PORT("OCR" + id + "AL", 8));
                    Ports.Add(new PORT("OCR" + id + "BH", 8));
                    Ports.Add(new PORT("OCR" + id + "BL", 8));
                    Ports.Add(new PORT("TCNT" + id + "H", 8));
                    Ports.Add(new PORT("TCNT" + id + "L", 8));
                    Ports.Add(new PORT("TCCR" + id + "A", 8));
                    Ports.Add(new PORT("TCCR" + id + "B", 8));
                }
        }

        public bool AddZaehler(String Name, int start) // Fügt einen Zähler hinzu
        {
            for (int i = 0; i < ZAEHLER.Count; i++)
            {
                if (Name == ZAEHLER[i].Name) return false;
            }

            ZAEHLER.Add(new ZAEHLER());
            int id = ZAEHLER.Count - 1;
            ZAEHLER[id].Name = Name;
            ZAEHLER[id].Begin = start; // Programzeile
            int line = Program[0][start].Original;
            Program[0][start].Zaehler_Start.Add((short)id);
            int summ = 0;// Text.GetFirstCharIndexFromLine(Program[start].Original);
            for (int b = 0; b < line; b++) summ += Text.Lines[b].Length + 1;

            Text.Select(summ, Quelltext[0][line].Length);
            Quelltext[0][line] = "[S-" + Name + "]" + Quelltext[0][line];
            int len = ((String)"[S-" + Name + "]").Length;
            Text.SelectedText = Quelltext[0][line];
            ZAEHLER[id].Begin_Length = len;
            return true;
        }

        public Atmega(int REGISTER, int Ident)
        {
            // Konstruktor
            Datei = "";
            QuellDatei = "";
            Name = "";
            ID = Ident;
            Time = 0;
            Counter.Add(0);
            Program.Add(new List<Zeile>());
            Label.Add(new List<String>());
            LabelPos.Add(new List<int>());
            Quelltext.Add(new String[1]);
            Watch_Minianwendungen.Add(new Stopwatch());
            Watch_Minianwendungen[Watch_Minianwendungen.Count - 1].Reset();
            Watch_Takte.Add(0);
            SRAM = new List<Byte>();
            STACK = new List<int>();
            Register = new Byte[REGISTER];
            Ports = new List<PORT>();
            Timer = new List<TIMER>();
            Ports.Add(new PORT("NONE", 8));
            Ports.Add(new PORT("SREG", 8));
            Ports.Add(new PORT("SPH", 3));
            Ports.Add(new PORT("SPL", 8));
            Ports.Add(new PORT("TIMSK", 8));
            Ports.Add(new PORT("TIFR", 8));
            Sleep = 1;
        }

        public void ClearStack() // löscht den Stack
        {
            STACK.Clear();
        }

        public void clearVarDef()
        {
            Var.Clear();
            Var_name.Clear();
        }

        public void clearPMDef()
        {
            PM.Clear();
            PM_name.Clear();
        }

        public void clearInterruptDef() // löscht Interrupt Definitionen
        {
            Interrupt.Clear();
            Interrupt_name.Clear();
            this.Interrupt_Def.Clear();
        }

        public void addStack(int wert) // Push eines wertes auf den Stack
        {
            STACK.Add(wert);
        }

        public int getStack() // Pop eines Wertes vom Stack
        {
            if (STACK.Count == 0) return 0;
            int a = STACK[STACK.Count - 1];
            STACK.RemoveAt(STACK.Count - 1);
            return a;
        }

        public int getRegister(String text) // macht aus einem String eine Register ID
        {
            text = text.ToUpper();
            for (int i = 0; i < Register.Count(); i++)
            {
                if (text == "R" + i)
                {
                    return i;
                }
            }

            for (int i = 0; i < Regs.Count(); i++)
            {
                if (text == Regs[i])
                {
                    return RegsPos[i];
                }
            }
            return 0;
        }

        public int getSRAM(String text) // macht aus einem String eine SRAM ID
        {
            text = text.ToUpper();
            for (int i = 0; i < SRAM.Count(); i++)
            {
                if (text == SRAM_name[i])
                {
                    return i;
                }
            }

            return 0;
        }

        public int getPM(String text) // macht aus einem String eine SRAM ID
        {
            text = text.ToUpper();
            for (int i = 0; i < PM.Count(); i++)
            {
                if (text == PM_name[i])
                {
                    return i;
                }
            }

            return 0;
        }

        public int getLabel(String text) // macht aus einem String eine Label ID
        {
            text = text.ToUpper();
            for (int i = 0; i < Label[Selected_Software].Count(); i++)
            {
                if (text == Label[Selected_Software][i])
                {
                    return i;
                }
            }

            return 0;
        }

        public int INT(String text)
        {
            text = Help.free(text);
            text = text.ToUpper();

            if (Help.IsNumber(text))
            {
                return Convert.ToInt32(text);
            }
            else
                if (text.Length >= 4 && text.Substring(0, 4).ToUpper() == "LOW(")
                {
                    text = text.Substring(3, text.Length - 3);
                    String[] temp = text.Split(')');
                    text = temp.Count() == 0 ? text.Substring(0, text.Length - 1) : temp[0].Substring(1, temp[0].Length - 1);
                    return Help.Low(INT(text));
                }
                else
                    if (text.Length >= 5 && text.Substring(0, 5).ToUpper() == "HIGH(")
                    {
                        text = text.Substring(4, text.Length - 4);
                        String[] temp = text.Split(')');
                        text = temp.Count() == 0 ? text.Substring(0, text.Length - 1) : temp[0].Substring(1, temp[0].Length - 1);
                        return Help.High(INT(text));
                    }
                    else
                        if (text.Length > 2 && text.Substring(0, 2) == "0B")
                        {
                            String temp = text.Split(';')[0];
                            text = temp.Length == 0 ? text : temp;
                            temp = text.Split('/')[0];
                            text = temp.Length == 0 ? text : temp;
                            return (int)Convert.ToInt64(text.Substring(2, text.Length - 2), 2);
                        }
                        else
                            if (text.Length > 2 && text.Substring(0, 2) == "0X")
                            {
                                String temp = text.Split(';')[0];
                                text = temp.Length == 0 ? text : temp;
                                temp = text.Split('/')[0];
                                text = temp.Length == 0 ? text : temp;
                                return (int)Convert.ToInt64(text.Substring(2, text.Length - 2), 16);
                            }
                            else
                            {
                                for (int i = 0; i < PM.Count; i++)
                                {
                                    if (text == PM_name[i]) return i;
                                }

                                for (int i = 0; i < SRAM.Count; i++)
                                {
                                    if (text == SRAM_name[i]) return i;
                                }

                                for (int i = 0; i < Label[Selected_Software].Count; i++)
                                {
                                    if (text == Label[Selected_Software][i]) return i; // gibt das Label zurück, das gemeint ist
                                }

                                for (int i = 0; i < Var.Count; i++)
                                {
                                    if (text == Var_name[i]) return Var[i];
                                }
                            }

            String res = Help.Arithmetic(this, text, true);
            if (Help.IsNumber(res))
            {
                return Convert.ToInt32(res);
            }
            else
                return 0;
        }

        public void clearRegDef()
        {
            Regs.Clear();
            RegsPos.Clear();
        }

        public void clearLabelDef()
        {
            Label[Selected_Software].Clear();
            LabelPos[Selected_Software].Clear();
        }

        public void clearPorts()
        {
            Ports.Clear();
        }

        public void clearSRAMDef()
        {
            SRAM_name.Clear();
            SRAM.Clear();
        }

        public void AddRegDef(int id, String Text)
        {
            Text = Text.ToUpper();
            for (int i = 0; i < Regs.Count; i++)
            {
                if (Text == Regs[i]) return;
            }
            Regs.Add(Text.ToUpper());
            RegsPos.Add(id);
        }

        public void AddInterruptDef(int wert, String Text)
        {
            Text = Text.ToUpper();
            for (int i = 0; i < Interrupt.Count; i++)
            {
                if (Text == Interrupt_name[i]) return;
            }
            Interrupt_name.Add(Text.ToUpper());
            Interrupt.Add(wert);
            Interrupt_Def.Add(getVar(Text));
        }

        public bool ExecuteInterrupt(String Name) // löst einen Interrupt aus
        {
            Name = Name.ToUpper();
            for (int i = 0; i < Interrupt.Count; i++)
            {
                if (Name == Interrupt_name[i])
                {
                    if (Interrupt[i] > -1)
                    {
                        addStack(Counter[0]);
                        Counter[0] = Interrupt[i];
                        Sleep++;
                        SLEEP.IN = false;
                        return true;
                    }
                    else
                        return false;
                }
            }
            return false;
        }

        public void Test_Set_IOPort(String Name, String Bit, bool Zustand) // Verhalten beim Schreiben in IORegister prüfen
        {
        }

        public void SetBitIOPort(int Name, Byte Bit, bool Zustand) // Gibt zurück, ob das Bit im IOPort gesetzt ist
        {
            // Byte temp = (Byte)(1 << pos);
            // Summe = (Byte)(zustand ? Summe | temp : Summe & ~temp);

            if (Name == INC.SREG) { SREG = (Byte)(Zustand ? SREG | (1 << Bit) : SREG & ~(1 << Bit)); return; }
            Ports[Name].set(Bit, Zustand);
        }

        public void SetBitIOPort(String Name, Byte Bit, bool Zustand) // Gibt zurück, ob das Bit im IOPort gesetzt ist
        {
            if (Name == "SREG") { SREG = (Byte)(Zustand ? SREG | (1 << Bit) : SREG & ~(1 << Bit)); return; }
            int a = getIORegister(Name);
            Ports[a].set(Bit, Zustand);
        }

        public void SetBitIOPort(String Name, String Bit, bool Zustand) // Gibt zurück, ob das Bit im IOPort gesetzt ist
        {
            if (Name == "SREG") { SREG = (Byte)(Zustand ? SREG | (1 << Convert.ToInt32(Bit)) : SREG & ~(1 << Convert.ToInt32(Bit))); return; }
            int a = getIORegister(Name);
            Ports[a].set(INT(Bit), Zustand);
            Test_Set_IOPort(Name, Bit, Zustand);
        }

        public bool GetBitIOPort(int Name, Byte Bit) // Gibt zurück, ob das Bit im IOPort gesetzt ist
        {
            if (Name == INC.SREG) return (SREG & (1 << Bit)) > 0;
            return Ports[Name].getBit(Bit);
        }

        public bool GetBitIOPort(String Name, Byte Bit) // Gibt zurück, ob das Bit im IOPort gesetzt ist
        {
            if (Name == "SREG") return (SREG & (1 << Bit)) > 0;
            int a = getIORegister(Name);
            return Ports[a].getBit(Bit);
        }

        public bool GetBitIOPort(String Name, String Bit) // Gibt zurück, ob das Bit im IOPort gesetzt ist
        {
            if (Name == "SREG") return (SREG & (1 << Convert.ToByte(Bit))) > 0;
            int a = getIORegister(Name);
            return Ports[a].getBit(INT(Bit));
        }

        public void AddVarDef(int id, String Text)
        {
            Text = Text.ToUpper();
            for (int i = 0; i < Var_name.Count; i++)
            {
                if (Text == Var_name[i]) return;
            }
            Var_name.Add(Text.ToUpper());
            Var.Add(id);
        }

        public String[] Zerlege_Data(String[] Data)
        {
            List<String> res = new List<String>();
            for (int i = 0; i < Data.Count(); i++)
            {
                String dat = Data[i].Trim();
                if (dat.Length >= 1 && dat.Substring(0, 1) == "'")
                {
                    dat = dat.Substring(1, dat.Length - 2);
                    if (dat.Length >= 1)
                    {
                        for (int b = 0; b < dat.Length; b++)
                        {
                            res.Add(((int)dat[b]).ToString());
                        }
                    }
                }
                else
                    if (dat.Length >= 1 && dat.Substring(0, 1) == "\"")
                    {
                        dat = dat.Substring(1, dat.Length - 2);
                        if (dat.Length >= 1)
                        {
                            for (int b = 0; b < dat.Length; b++)
                            {
                                res.Add(((int)dat[b]).ToString());
                            }
                        }
                    }
                    else
                        res.Add(dat);
            }

            String[] resultat = new String[res.Count];
            for (int i = 0; i < res.Count; i++) resultat[i] = res[i];
            return resultat;
        }

        public void AddPMDef(String[] Data, String Text)
        {
            Data = Zerlege_Data(Data);
            Text = Text.ToUpper();
            for (int i = 0; i < PM_name.Count; i++)
            {
                if (PM_name[i].Length >= Text.Length)
                    if (Text == PM_name[i].Substring(0, Text.Length)) return;
            }

            for (int i = 0; i < Data.Count(); i++)
            {
                if (i > 0)
                {
                    PM_name.Add(Text.ToUpper() + "[" + i + "]");
                }
                else
                    PM_name.Add(Text.ToUpper());
                PM.Add((Byte)INT(Data[i]));
            }
        }

        public void InsertPMDef(String[] Data, int Line)
        {
            Data = Zerlege_Data(Data);
            String Text = "";
            bool found = false;
            int zaehl = 0;
            // if (PM_name.Count>=1){
            // Text=(PM_name[PM_name.Count - 1].Split('['))[0];
            Text = "";
            for (int i = 1; i < Label[Selected_Software].Count(); i++)
            {
                if (LabelPos[Selected_Software][i] - 100000 > Line) Text = Label[Selected_Software][i - 1];
            }
            if (Text == "") Text = Label[Selected_Software][Label[Selected_Software].Count() - 1];

            Text = Text.ToUpper();
            for (int i = 0; i < PM_name.Count; i++)
            {
                if (PM_name[i].Length >= Text.Length)
                    if (Text == PM_name[i])
                    {
                        zaehl++;
                        found = true;
                    }
                    else
                        if (Text + "[" == PM_name[i].Substring(0, Text.Length) + "[")
                        {
                            zaehl++;
                            found = true;
                        }
            }

            if (found)
            {
                for (int i = 0; i < Data.Count(); i++)
                {
                    PM_name.Add(Text.ToUpper() + "[" + (zaehl + i).ToString() + "]");
                    PM.Add((Byte)INT(Data[i]));
                }
            }
            else
            {
                for (int i = 0; i < Data.Count(); i++)
                {
                    if (i > 0)
                    {
                        PM_name.Add(Text.ToUpper() + "[" + i + "]");
                    }
                    else
                        PM_name.Add(Text.ToUpper());
                    PM.Add((Byte)INT(Data[i]));
                }
            }
        }

        public void AddSRAMDef(int anz, String Text)
        {
            Text = Text.ToUpper();
            for (int i = 0; i < SRAM_name.Count; i++)
            {
                if (SRAM_name[i].Length >= Text.Length)
                    if (Text == SRAM_name[i].Substring(0, Text.Length)) return;
            }

            for (int i = 0; i < anz; i++)
            {
                if (i > 0)
                {
                    SRAM_name.Add(Text.ToUpper() + "[" + i + "]");
                }
                else
                    SRAM_name.Add(Text.ToUpper());
                SRAM.Add(0);
            }
        }

        public void AddLabelDef(int id, String Text)
        {
            Text = Text.ToUpper();
            for (int i = 0; i < Label[Selected_Software].Count; i++)
            {
                if (Text == Label[Selected_Software][i]) return;
            }
            Label[Selected_Software].Add(Text);
            LabelPos[Selected_Software].Add(id);
        }

        public int getIORegister(String text)
        {
            text = text.ToUpper();
            for (int i = 0; i < Ports.Count; i++)
            {
                if (Ports[i].Bezeichnung == text) return i;
            }
            return 0;
        }

        public int getVar(String text)
        {
            text = text.ToUpper();
            for (int i = 0; i < Var_name.Count; i++)
            {
                if (Var_name[i] == text) return Var[i];
            }
            return 0;
        }

        public void Load_Content(List<String> Source, bool Hauptsoftware) // analysiert und speichert Source
        {
            // clearRegDef();
            // clearVarDef();
            // clearLabelDef();
            //  clearSRAMDef();
            // ClearStack();
            for (; Selected_Software >= Program.Count; )
            {
                Program.Add(new List<Zeile>());
                Label.Add(new List<String>());
                LabelPos.Add(new List<int>());
                Quelltext.Add(new String[1]);
                Watch_Minianwendungen.Add(new Stopwatch());
                Watch_Minianwendungen[Watch_Minianwendungen.Count - 1].Reset();
                Watch_Takte.Add(0);
                Counter.Add(0);
            }

            if (Hauptsoftware)
            {
                // includes finden
                for (int i = 0; i < Source.Count; i++)
                {
                    String a = Source[i];
                    if (a.Length >= 8)
                        if (a.Substring(0, 8).ToUpper() == ".INCLUDE")
                        {
                            a = a.Substring(8, a.Length - 8);
                            a = a.Split(';')[0];
                            a = a.Split('/')[0];
                            a = Help.free(a);
                            a = a.Substring(1, a.Length - 2);
                            String b = Path.GetExtension(a).ToUpper();
                            if (b == ".INC")
                            {
                                // interpretieren
                                String dat = Application.StartupPath + "\\def\\" + a;
                                StreamReader datei = new StreamReader(dat);
                                while (datei.Peek() != -1)
                                {
                                    a = datei.ReadLine();
                                    if (a.Length >= 4 && a.Substring(0, 4).ToUpper() == ".EQU")
                                    {
                                        a = a.Substring(4, a.Length - 4);
                                        a = Help.free(a);
                                        String[] temp = a.Split(';');
                                        a = temp.Count() > 0 ? temp[0] : Source[i];
                                        temp = a.Split('/');
                                        a = temp.Count() > 0 ? temp[0] : Source[i];

                                        String[] list = a.Split('=');
                                        AddVarDef(INT(list[1]), list[0]);
                                    }
                                    else
                                        if (a.Length >= 4 && a.Substring(0, 4).ToUpper() == ".DEF")
                                        {
                                            String[] ap2 = a.Substring(4, a.Length - 4).Split('=');
                                            for (int c = 0; c < ap2.Count(); c++)
                                            {
                                                String u = Help.free(ap2[c].ToUpper());
                                                if (u == "error")
                                                    ap2[c] = "error";
                                                ap2[c] = u;
                                            }
                                            AddRegDef(getRegister(ap2[1]), ap2[0]);
                                            continue;
                                        }
                                }
                                datei.Close();
                            }
                            else
                                if (b == ".ASM")
                                {
                                    // einfügen
                                    Source.RemoveAt(i);
                                    String dat = Path.GetDirectoryName(QuellDatei) + "\\" + a;
                                    StreamReader datei = new StreamReader(dat);
                                    for (int c = 0; datei.Peek() != -1; c++)
                                    {
                                        a = datei.ReadLine();
                                        Source.Insert(i + c, a);
                                    }

                                    datei.Close();
                                }
                        }
                }
            }

            Quelltext[Selected_Software] = new String[Source.Count];
            for (int i = 0; i < Source.Count; i++)
            {
                Quelltext[Selected_Software][i] = Source[i];
            }

            // kommentare aus Source löschen
            bool found = false;
            for (int i = 0; i < Source.Count; i++)
            {
                for (int b = 0; b < Source[i].Length - 1; b++)
                {
                    if (Source[i].Length >= 2 && (Source[i].Substring(b, 2) == "/*" && found == false))
                    {
                        Source[i] = Source[i].Substring(0, b);
                        b = 0;
                        found = true;
                    }
                    else
                        if (Source[i].Length >= 2 && (Source[i].Substring(b, 2) == "*/" && found == true))
                        {
                            Source[i] = Source[i].Substring(b + 2, Source[i].Length - (b + 2));
                            found = false;
                        }
                }
                if (found == true)
                {
                    Source[i] = "";
                }
            }

            // Label finden
            for (int i = 0; i < Source.Count; i++)
            {
                String q = Source[i];
                if (q.Length < 2) continue;
                String[] temp = q.Split(':');
                if (temp.Count() >= 2)
                {
                    temp[0] = Help.free(temp[0]);
                    AddLabelDef(100000 + i, temp[0]);
                }
                if (temp.Count() > 1)
                {
                    temp[1] = temp[1].Trim();
                    if (temp[1].Length >= 3)
                        if (temp[1].Substring(0, 3).ToUpper() == ".DB")
                        {
                            // PM gefunden
                            temp[1] = temp[1].Substring(3, temp[1].Length - 3);
                            String[] temp2 = temp[1].Split(',');
                            //if (temp2.Count()==0){temp2 = new String[1];temp2[0]=temp[1];}
                            if (temp2[0] != "")
                                AddPMDef(temp2, temp[0]);
                        }
                }
                else
                    if (temp.Count() == 1)
                    {
                        q = q.Trim();
                        if (q.Length >= 3)
                            if (q.Substring(0, 3).ToUpper() == ".DB")
                            {
                                // PM gefunden
                                q = q.Substring(3, q.Length - 3);
                                String[] temp2 = q.Split(',');
                                // if (temp2.Count() == 0) { temp2 = new String[1]; temp2[0] = q; }
                                if (temp2[0] != "")
                                    InsertPMDef(temp2, i);
                                temp2[0] = "";
                            }
                    }
            }

            if (Hauptsoftware)
            {
                // Arbeitsspeicher finden
                // noch machen
                // noch machen
                // noch machen
                for (int i = 0; i < Source.Count; i++)
                {
                    if (Source[i].Length >= 5)
                        if (Source[i].Substring(0, 5).ToUpper() == ".DSEG")
                        {
                            Source[i] = "";
                            for (int b = i + 1; b < Source.Count; b++)
                            {
                                String[] list = Source[b].Trim().Split(':');
                                if (list.Count() > 1)
                                {
                                    list[1] = list[1].Trim();
                                    String[] list2 = list[1].Split(' ');
                                    if (list2.Count() > 1)
                                    {
                                        AddSRAMDef(INT(list2[1]), list[0]);
                                        int a = INT(list2[1]);
                                    }
                                }
                            }

                            break;
                        }
                }
            }

            Program[Selected_Software].Clear();
            bool setbreak = false;
            String setzaehler = "";
            for (int i = 0; i < Source.Count; i++)
            {
                Source[i] = Source[i].Trim();
                String[] temp = Source[i].Split(';');
                if (temp.Count() > 1)
                {
                    for (int e = 1; e < temp.Count(); e++)
                        if (Help.free(temp[e]).ToUpper() == "#BREAK#")
                        {
                            setbreak = true;
                            break;
                        }

                    for (int e = 1; e < temp.Count(); e++)
                        if (Help.free(temp[e]).ToUpper().Count() >= 2)
                            if (Help.free(temp[e]).ToUpper().Substring(0, 2) == "#[")
                            {
                                // neuen Start eines Zaehlers setzen
                                setzaehler = Help.free(temp[e]).ToUpper();
                            }
                }
                Source[i] = temp[0];
                Source[i] = Source[i].Split('/')[0];

                String[] ab = Source[i].Split(' ');
                if (ab.Count() == 0) { ab = new String[1]; ab[0] = Source[i]; }

                String Befehl = Help.free(ab[0]).ToUpper();
                String inhalt = "";
                for (int b = 1; b < ab.Length; b++) inhalt = inhalt + ab[b];
                // Sonderbefehle testen
                /* if (Befehl == ".ORG")
                 {
                     i++;
                     continue;
                 }
                 else*/
                if (Befehl == ".DEF")
                {
                    String[] ap2 = inhalt.Split('=');
                    for (int b = 0; b < ap2.Count(); b++)
                    {
                        String u = Help.free(ap2[b].ToUpper());
                        if (u == "error")
                            ap2[b] = "error";
                        ap2[b] = u;
                    }
                    AddRegDef(getRegister(ap2[1]), ap2[0]);
                    continue;
                }

                // Normaler Befehl
                String[] ap = inhalt.Split(',');
                String[] param = new String[ap.Count() + 1];
                for (int b = 1; b <= ap.Length; b++) param[b] = ap[b - 1];

                if (param.Count() <= 1) { param = new String[2]; param[1] = inhalt; }
                param[0] = "";
                for (int b = 0; b < param.Count(); b++)
                {
                    String u = Help.free(param[b]);
                    if (u == "error")
                        u = "error";
                    param[b] = u;
                }

                int old_befehlanzahl = Program[Selected_Software].Count;
                switch (Befehl)
                {
                    case "ADD":
                        Program[Selected_Software].Add(new Zeile(0, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "ADC":
                        Program[Selected_Software].Add(new Zeile(1, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "ADIW":
                        Program[Selected_Software].Add(new Zeile(2, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "SUB":
                        Program[Selected_Software].Add(new Zeile(3, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "SUBI":
                        Program[Selected_Software].Add(new Zeile(4, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "SBC":
                        Program[Selected_Software].Add(new Zeile(5, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "SBCI":
                        Program[Selected_Software].Add(new Zeile(6, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "SBIW":
                        Program[Selected_Software].Add(new Zeile(7, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "AND":
                        Program[Selected_Software].Add(new Zeile(8, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "ANDI":
                        Program[Selected_Software].Add(new Zeile(9, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "OR":
                        Program[Selected_Software].Add(new Zeile(10, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "ORI":
                        Program[Selected_Software].Add(new Zeile(11, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "EOR":
                        Program[Selected_Software].Add(new Zeile(12, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "COM":
                        Program[Selected_Software].Add(new Zeile(13, getRegister(param[1]), 0, 0, i));
                        break;

                    case "NEG":
                        Program[Selected_Software].Add(new Zeile(14, getRegister(param[1]), 0, 0, i));
                        break;

                    case "SBR":
                        Program[Selected_Software].Add(new Zeile(15, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "CBR":
                        Program[Selected_Software].Add(new Zeile(16, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "INC":
                        Program[Selected_Software].Add(new Zeile(17, getRegister(param[1]), 0, 0, i));
                        break;

                    case "DEC":
                        Program[Selected_Software].Add(new Zeile(18, getRegister(param[1]), 0, 0, i));
                        break;

                    case "TST":
                        Program[Selected_Software].Add(new Zeile(19, getRegister(param[1]), 0, 0, i));
                        break;

                    case "CLR":
                        Program[Selected_Software].Add(new Zeile(20, getRegister(param[1]), 0, 0, i));
                        break;

                    case "SER":
                        Program[Selected_Software].Add(new Zeile(21, getRegister(param[1]), 0, 0, i));
                        break;

                    case "MUL":
                        Program[Selected_Software].Add(new Zeile(22, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "MULS":
                        Program[Selected_Software].Add(new Zeile(23, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "MULSU":
                        Program[Selected_Software].Add(new Zeile(24, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "FMUL":
                        Program[Selected_Software].Add(new Zeile(25, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "FMULS":
                        Program[Selected_Software].Add(new Zeile(26, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "FMULSU":
                        Program[Selected_Software].Add(new Zeile(27, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "RJMP":
                        Program[Selected_Software].Add(new Zeile(28, INT(param[1]), 0, 0, i));
                        break;

                    case "IJMP":
                        Program[Selected_Software].Add(new Zeile(29, 0, 0, 0, i));
                        break;

                    case "RCALL":
                        Program[Selected_Software].Add(new Zeile(30, INT(param[1]), 0, 0, i));
                        break;

                    case "ICALL":
                        Program[Selected_Software].Add(new Zeile(31, 0, 0, 0, i));
                        break;

                    case "RET":
                        Program[Selected_Software].Add(new Zeile(32, 0, 0, 0, i));
                        break;

                    case "RETI":
                        Program[Selected_Software].Add(new Zeile(33, 0, 0, 0, i));
                        break;

                    case "CPSE":
                        Program[Selected_Software].Add(new Zeile(34, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "CP":
                        Program[Selected_Software].Add(new Zeile(35, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "CPC":
                        Program[Selected_Software].Add(new Zeile(36, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "CPI":
                        Program[Selected_Software].Add(new Zeile(37, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "SBRC":
                        Program[Selected_Software].Add(new Zeile(38, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "SBRS":
                        Program[Selected_Software].Add(new Zeile(39, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "SBIC":
                        Program[Selected_Software].Add(new Zeile(40, getIORegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "SBIS":
                        Program[Selected_Software].Add(new Zeile(41, getIORegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "BRBS":
                        Program[Selected_Software].Add(new Zeile(42, INT(param[1]), INT(param[2]), 0, i));
                        break;

                    case "BRBC":
                        Program[Selected_Software].Add(new Zeile(43, INT(param[1]), INT(param[2]), 0, i));
                        break;

                    case "BREQ":
                        Program[Selected_Software].Add(new Zeile(44, INT(param[1]), 0, 0, i));
                        break;

                    case "BRNE":
                        int tt = INT(param[1]);
                        Program[Selected_Software].Add(new Zeile(45, INT(param[1]), 0, 0, i));
                        break;

                    case "BRCS":
                        Program[Selected_Software].Add(new Zeile(46, INT(param[1]), 0, 0, i));
                        break;

                    case "BRCC":
                        Program[Selected_Software].Add(new Zeile(47, INT(param[1]), 0, 0, i));
                        break;

                    case "BRSH":
                        Program[Selected_Software].Add(new Zeile(48, INT(param[1]), 0, 0, i));
                        break;

                    case "BRLO":
                        Program[Selected_Software].Add(new Zeile(49, INT(param[1]), 0, 0, i));
                        break;

                    case "BRMI":
                        Program[Selected_Software].Add(new Zeile(50, INT(param[1]), 0, 0, i));
                        break;

                    case "BRPL":
                        Program[Selected_Software].Add(new Zeile(51, INT(param[1]), 0, 0, i));
                        break;

                    case "BRGE":
                        Program[Selected_Software].Add(new Zeile(52, INT(param[1]), 0, 0, i));
                        break;

                    case "BRLT":
                        Program[Selected_Software].Add(new Zeile(53, INT(param[1]), 0, 0, i));
                        break;

                    case "BRHS":
                        Program[Selected_Software].Add(new Zeile(54, INT(param[1]), 0, 0, i));
                        break;

                    case "BRHC":
                        Program[Selected_Software].Add(new Zeile(55, INT(param[1]), 0, 0, i));
                        break;

                    case "BRTS":
                        Program[Selected_Software].Add(new Zeile(56, INT(param[1]), 0, 0, i));
                        break;

                    case "BRTC":
                        Program[Selected_Software].Add(new Zeile(57, INT(param[1]), 0, 0, i));
                        break;

                    case "BRVS":
                        Program[Selected_Software].Add(new Zeile(58, INT(param[1]), 0, 0, i));
                        break;

                    case "BRVC":
                        Program[Selected_Software].Add(new Zeile(59, INT(param[1]), 0, 0, i));
                        break;

                    case "BRIE":
                        Program[Selected_Software].Add(new Zeile(60, INT(param[1]), 0, 0, i));
                        break;

                    case "BRID":
                        Program[Selected_Software].Add(new Zeile(61, INT(param[1]), 0, 0, i));
                        break;

                    case "MOV":
                        Program[Selected_Software].Add(new Zeile(62, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "MOVW":
                        Program[Selected_Software].Add(new Zeile(63, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "LDI":
                        Program[Selected_Software].Add(new Zeile(64, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "LD":
                        {
                            int a6 = 0;
                            int a7 = 0;
                            switch (param[2])
                            {
                                case "X":
                                    a6 = 0;
                                    a7 = 0;
                                    break;

                                case "X+":
                                    a6 = 0;
                                    a7 = 1;
                                    break;

                                case "-X":
                                    a6 = 0;
                                    a7 = -1;
                                    break;

                                case "Y":
                                    a6 = 1;
                                    a7 = 0;
                                    break;

                                case "Y+":
                                    a6 = 1;
                                    a7 = 1;
                                    break;

                                case "-Y":
                                    a6 = 1;
                                    a7 = -1;
                                    break;

                                case "Z":
                                    a6 = 2;
                                    a7 = 0;
                                    break;

                                case "Z+":
                                    a6 = 2;
                                    a7 = 1;
                                    break;

                                case "-Z":
                                    a6 = 2;
                                    a7 = -1;
                                    break;
                            }
                            Program[Selected_Software].Add(new Zeile(65, getRegister(param[1]), a6, a7, i));
                        }
                        break;

                    case "LDD":
                        //Program.Add(new Zeile(66, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "LDS":
                        Program[Selected_Software].Add(new Zeile(67, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "ST":
                        {
                            int a6 = 0;
                            int a7 = 0;
                            switch (param[1])
                            {
                                case "X":
                                    a6 = 0;
                                    a7 = 0;
                                    break;

                                case "X+":
                                    a6 = 0;
                                    a7 = 1;
                                    break;

                                case "-X":
                                    a6 = 0;
                                    a7 = -1;
                                    break;

                                case "Y":
                                    a6 = 1;
                                    a7 = 0;
                                    break;

                                case "Y+":
                                    a6 = 1;
                                    a7 = 1;
                                    break;

                                case "-Y":
                                    a6 = 1;
                                    a7 = -1;
                                    break;

                                case "Z":
                                    a6 = 2;
                                    a7 = 0;
                                    break;

                                case "Z+":
                                    a6 = 2;
                                    a7 = 1;
                                    break;

                                case "-Z":
                                    a6 = 2;
                                    a7 = -1;
                                    break;
                            }
                            Program[Selected_Software].Add(new Zeile(68, a6, a7, getRegister(param[2]), i));
                        }
                        break;

                    case "STD": //
                        //Program.Add(new Zeile(69, getRegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "STS": //
                        Program[Selected_Software].Add(new Zeile(70, INT(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "LPM": //
                        {
                            int a6 = 0;
                            switch (param[2])
                            {
                                case "Z":
                                    a6 = 0;
                                    param[1] = "R0";
                                    break;

                                case "Z+":
                                    a6 = 1;
                                    break;

                                case "":
                                    a6 = 2;
                                    break;
                            }
                            Program[Selected_Software].Add(new Zeile(71, getRegister(param[1]), a6, 0, i));
                        }
                        break;

                    case "SPM":
                        Program[Selected_Software].Add(new Zeile(72, 0, 0, 0, i));
                        break;

                    case "IN":
                        Program[Selected_Software].Add(new Zeile(73, getRegister(param[1]), getIORegister(param[2]), 0, i));
                        break;

                    case "OUT":
                        Program[Selected_Software].Add(new Zeile(74, getIORegister(param[1]), getRegister(param[2]), 0, i));
                        break;

                    case "PUSH":
                        Program[Selected_Software].Add(new Zeile(75, getRegister(param[1]), 0, 0, i));
                        break;

                    case "POP":
                        Program[Selected_Software].Add(new Zeile(76, getRegister(param[1]), 0, 0, i));
                        break;

                    case "SBI":
                        Program[Selected_Software].Add(new Zeile(77, getIORegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "CBI":
                        Program[Selected_Software].Add(new Zeile(78, getIORegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "LSL":
                        Program[Selected_Software].Add(new Zeile(79, getRegister(param[1]), 0, 0, i));
                        break;

                    case "LSR":
                        Program[Selected_Software].Add(new Zeile(80, getRegister(param[1]), 0, 0, i));
                        break;

                    case "ROL":
                        Program[Selected_Software].Add(new Zeile(81, getRegister(param[1]), 0, 0, i));
                        break;

                    case "ROR":
                        Program[Selected_Software].Add(new Zeile(82, getRegister(param[1]), 0, 0, i));
                        break;

                    case "ASR":
                        Program[Selected_Software].Add(new Zeile(83, getRegister(param[1]), 0, 0, i));
                        break;

                    case "SWAP":
                        Program[Selected_Software].Add(new Zeile(84, getRegister(param[1]), 0, 0, i));
                        break;

                    case "BSET":
                        Program[Selected_Software].Add(new Zeile(85, INT(param[1]), 0, 0, i));
                        break;

                    case "BCLR":
                        Program[Selected_Software].Add(new Zeile(86, INT(param[1]), 0, 0, i));
                        break;

                    case "BST":
                        Program[Selected_Software].Add(new Zeile(87, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "BLD":
                        Program[Selected_Software].Add(new Zeile(88, getRegister(param[1]), INT(param[2]), 0, i));
                        break;

                    case "SEC":
                        Program[Selected_Software].Add(new Zeile(89, 0, 0, 0, i));
                        break;

                    case "CLC":
                        Program[Selected_Software].Add(new Zeile(90, 0, 0, 0, i));
                        break;

                    case "SEN":
                        Program[Selected_Software].Add(new Zeile(91, 0, 0, 0, i));
                        break;

                    case "CLN":
                        Program[Selected_Software].Add(new Zeile(92, 0, 0, 0, i));
                        break;

                    case "SEZ":
                        Program[Selected_Software].Add(new Zeile(93, 0, 0, 0, i));
                        break;

                    case "CLZ":
                        Program[Selected_Software].Add(new Zeile(94, 0, 0, 0, i));
                        break;

                    case "SEI":
                        Program[Selected_Software].Add(new Zeile(95, 0, 0, 0, i));
                        break;

                    case "CLI":
                        Program[Selected_Software].Add(new Zeile(96, 0, 0, 0, i));
                        break;

                    case "SES":
                        Program[Selected_Software].Add(new Zeile(97, 0, 0, 0, i));
                        break;

                    case "CLS":
                        Program[Selected_Software].Add(new Zeile(98, 0, 0, 0, i));
                        break;

                    case "SEV":
                        Program[Selected_Software].Add(new Zeile(99, 0, 0, 0, i));
                        break;

                    case "CLV":
                        Program[Selected_Software].Add(new Zeile(100, 0, 0, 0, i));
                        break;

                    case "SET":
                        Program[Selected_Software].Add(new Zeile(101, 0, 0, 0, i));
                        break;

                    case "CLT":
                        Program[Selected_Software].Add(new Zeile(102, 0, 0, 0, i));
                        break;

                    case "SEH":
                        Program[Selected_Software].Add(new Zeile(103, 0, 0, 0, i));
                        break;

                    case "CLH":
                        Program[Selected_Software].Add(new Zeile(104, 0, 0, 0, i));
                        break;

                    case "NOP":
                        Program[Selected_Software].Add(new Zeile(105, 0, 0, 0, i));
                        break;

                    case "WDR":
                        Program[Selected_Software].Add(new Zeile(106, 0, 0, 0, i));
                        break;

                    case "SLEEP":
                        Program[Selected_Software].Add(new Zeile(107, 0, 0, 0, i));
                        break;

                    default:

                        break;
                }
                if (old_befehlanzahl < Program[Selected_Software].Count && (setbreak || setzaehler != ""))
                {
                    if (setbreak)
                    {
                        Program[Selected_Software][Program[Selected_Software].Count - 1].SetBreakpoint();
                        setbreak = false;
                    }
                    if (setzaehler != "")
                    {
                        setzaehler = setzaehler.Substring(2, setzaehler.Count() - 4);
                        String[] temper = setzaehler.Split(',');
                        for (int e = 0; e < temper.Count(); e++)
                        {
                            if (temper[e].Substring(0, 1) == "S")
                            {
                                temper[e] = temper[e].Substring(2, temper[e].Length - 2);
                                // Startpunkt
                                int ib = Program[Selected_Software].Count - 1;
                                if (ib != -1)
                                {
                                    //int line = Program[0][ib].Original;

                                    // int len = Quelltext[0][line].Length;
                                    // int start =Text.GetFirstCharIndexFromLine(line);
                                    // AddZaehler(temper[e], ib);

                                    /*// Endpunkt
                                    String Name = list[i];
                                    ib = Convert.ToInt32(list[i + 3]);
                                    if (ib != -1 && ib < Main[sel].Program[0].Count)
                                    {
                                        line = Main[sel].Program[0][ib].Original;
                                        int found = Main[sel].ZAEHLER.Count - 1;
                                        Main[sel].ZAEHLER[found].Ende = ib;

                                        start = ib;
                                        int len2 = Main[sel].Quelltext[0][line].Length;

                                        Main[sel].Program[0][start].Zaehler_Ende.Add((short)found);
                                        Main[sel].Quelltext[0][line] = Main[sel].Quelltext[0][line] + "[E-" + Name + "]";
                                        len = ((String)"[E-" + Name + "]").Length;

                                        start = 0;
                                        for (int b = 0; b < line; b++) start += Main[sel].Text.Lines[b].Length + 1;

                                        Main[sel].Text.Select(start, len2);
                                        Main[sel].Text.SelectedText = Main[sel].Quelltext[0][line];
                                        Main[sel].ZAEHLER[found].Ende_Length = len;
                                    }*/
                                }
                            }
                            else
                                if (temper[e].Substring(0, 1) == "E")
                                {
                                    temper[e] = temper[e].Substring(2, temper[e].Length - 2);
                                }
                        }
                        setzaehler = "";
                    }
                }
            }

            // label im Program suchen und ersetzen
            int[] Pos = new int[Label[Selected_Software].Count];
            for (int i = 0; i < Label[Selected_Software].Count; i++)
            {
                for (int b = 0; b < Program[Selected_Software].Count; b++)
                {
                    if (Program[Selected_Software][b].Original >= LabelPos[Selected_Software][i] - 100000)
                    {
                        Pos[i] = b;
                        break;
                    }
                }

                for (int b = 0; b < Program[Selected_Software].Count; b++)
                {
                    if (Program[Selected_Software][b].Param1 == LabelPos[Selected_Software][i]) Program[Selected_Software][b].Param1 = Pos[i];
                    if (Program[Selected_Software][b].Param2 == LabelPos[Selected_Software][i]) Program[Selected_Software][b].Param2 = Pos[i];
                    if (Program[Selected_Software][b].Param3 == LabelPos[Selected_Software][i]) Program[Selected_Software][b].Param3 = Pos[i];
                }
                LabelPos[Selected_Software][i] = Pos[i];
            }

            if (Hauptsoftware)
            {
                int Vector_Size = INT("INT_VECTORS_SIZE");
                for (int i = 0; i < Vector_Size; i++)
                {
                    AddInterruptDef(-1, i.ToString());
                }

                for (int i = 0; i < Source.Count; i++)
                {
                    Source[i] = Source[i].Trim();
                    if (Source[i].Length == 0) continue;
                    String[] ab = Source[i].Split(' ');
                    if (ab.Count() == 0) { ab = new String[1]; ab[0] = Source[i]; }

                    String Befehl = Help.free(ab[0]).ToUpper();
                    String inhalt = "";
                    for (int b = 1; b < ab.Length; b++) inhalt = inhalt + ab[b];
                    // Sonderbefehle testen
                    if (Befehl == ".ORG")
                    {
                        inhalt = Help.free(inhalt);
                        int id = INT(inhalt);
                        String temp = inhalt;
                        if (id == 0) inhalt = "RESET";
                        Interrupt_name[id] = inhalt.ToUpper();
                        // Programmposition finden
                        for (int b = 0; b < Program[Selected_Software].Count; b++)
                        {
                            if (Program[Selected_Software][b].Original >= i)
                            {
                                Interrupt[id] = b;
                                break;
                            }
                        }
                        Interrupt_Def[id] = getVar(temp);
                        i++;
                        continue;
                    }
                }
            }

            Optimierer A = new Optimierer();
            Program[Selected_Software] = A.Generate(Program[Selected_Software]);
        }

        public void Unset_Flags() // wird nicht benutzt
        {
            /*  Z = false;
              N = false;
              V = false;
              C = false;
              H = false;
              I = false;
              T = false;
              S = false;*/
        }

        public void Zero(Byte Wert, Zeile A)
        {
            if ((A.SetFlags & 2) == 0) return;
            if (Wert == 0)
            {
                SREG = (Byte)(SREG | SREGZ);
            }
            else
                SREG = (Byte)(SREG & SREGNOZ);
        }

        public void Signed(Zeile A)
        {
            if ((A.SetFlags & 16) == 0) return;
            if ((((SREG & 8) >> 1) ^ (SREG & 4)) > 0)
            {
                SREG = (Byte)(SREG | SREGS);
            }
            else
                SREG = (Byte)(SREG & SREGNOS);
        }

        public void Negativ(Byte Wert, Zeile A)
        {
            if ((A.SetFlags & 4) == 0) return;
            if (Wert >= 128)
            {
                SREG = (Byte)(SREG | SREGN);
            }
            else
                SREG = (Byte)(SREG & SREGNON);
        }

        public void HalfCarry(Byte A, Byte B, Byte Result)
        {
            bool A_Set = (A & 8) > 0 ? true : false;
            bool B_Set = (B & 8) > 0 ? true : false;
            bool Result_Set = (Result & 8) > 0 ? true : false;
            if ((A_Set && B_Set) || (B_Set && !Result_Set) || (A_Set && !Result_Set))
            {
                SREG = (Byte)(SREG | SREGH);
            }
            else
                SREG = (Byte)(SREG & SREGNOH);
        }

        public void INVHalfCarry(Byte A, Byte B, Byte Result)
        {
            bool A_Set = (A & 8) > 0 ? true : false;
            bool B_Set = (B & 8) > 0 ? true : false;
            bool Result_Set = (Result & 8) > 0 ? true : false;
            if ((!A_Set && B_Set) || (B_Set && Result_Set) || (!A_Set && Result_Set))
            {
                SREG = (Byte)(SREG | SREGH);
            }
            else
                SREG = (Byte)(SREG & SREGNOH);
        }

        public void Carry(Byte A, Byte B, Byte Result)
        {
            bool A_Set = A >= 128 ? true : false;
            bool B_Set = B >= 128 ? true : false;
            bool Result_Set = Result >= 128 ? true : false;
            if ((A_Set && B_Set) || (B_Set && !Result_Set) || (A_Set && !Result_Set))
            {
                SREG = (Byte)(SREG | SREGC);
            }
            else
                SREG = (Byte)(SREG & SREGNOC);
        }

        public void NewCarry(int Result)
        {
            if (Result >= 256)
            {
                SREG = (Byte)(SREG | SREGC);
            }
            else
                SREG = (Byte)(SREG & SREGNOC);
        }

        public void INVCarry(Byte A, Byte B, Byte Result)
        {
            bool A_Set = A >= 128 ? true : false;
            bool B_Set = B >= 128 ? true : false;
            bool Result_Set = Result >= 128 ? true : false;
            if ((!A_Set && B_Set) || (B_Set && Result_Set) || (!A_Set && Result_Set))
            {
                SREG = (Byte)(SREG | SREGC);
            }
            else
                SREG = (Byte)(SREG & SREGNOC);
        }

        public void TwoOver(Byte A, Byte B, Byte Result)
        {
            bool A_Set = A >= 128 ? true : false;
            bool B_Set = B >= 128 ? true : false;
            bool Result_Set = Result >= 128 ? true : false;
            if ((A_Set && B_Set && !Result_Set) || (!A_Set && !B_Set && Result_Set))
            {
                SREG = (Byte)(SREG | SREGV);
            }
            else
                SREG = (Byte)(SREG & SREGNOV);
        }

        public void INVTwoOver(Byte A, Byte B, Byte Result)
        {
            bool A_Set = (A & 128) > 0 ? true : false;
            bool B_Set = (B & 128) > 0 ? true : false;
            bool Result_Set = (Result & 128) > 0 ? true : false;
            if ((A_Set && !B_Set && !Result_Set) || (!A_Set && B_Set && Result_Set))
            {
                SREG = (Byte)(SREG | SREGV);
            }
            else
                SREG = (Byte)(SREG & SREGNOV);
        }

        public int LowHigh(Byte a, Byte b) // fügt beide Bytes zu zusammen
        {
            return a + (b << 8);
        }

        public bool Step(List<SZENARIEN> global_scenery)
        {
            bool Hauptsoftware = Selected_Software == 0 ? true : false;
            if (Watches == null)
            {
                Watches = new Stopwatch[108];
                for (int i = 0; i < 108; i++) { Watches[i] = new Stopwatch(); Watches[i].Reset(); }
            }

            if (Program[Selected_Software] == null) return false;
            if (Counter[Selected_Software] >= Program[Selected_Software].Count) return false;
            if (Counter[Selected_Software] < 0) return false;
            if (Hauptsoftware)
            {
                Zeile line = Program[Selected_Software][Counter[Selected_Software]];
                if (line.Breakpoint && !line.Breakover) { Program[Selected_Software][Counter[Selected_Software]].Breakover = true; return false; }
                if (line.Breakpoint && line.Breakover && Sleep == 1) { Program[Selected_Software][Counter[Selected_Software]].Breakover = false; }

                Time++;
                // Szenarien prüfen
                if (Szenarien_AKTIV) for (int i = 0; i < scenery.Count; i++) scenery[i].Update();
                // for (int i = 0; i < global_scenery.Count; i++) if (global_scenery[i].Vater == this) global_scenery[i].Update();

                hauptWatch.Start();
                // Zaehler prüfen
                for (int i = 0; i < ZAEHLER.Count; i++) if (ZAEHLER[i].Aktiv) ZAEHLER[i].Wert++;

                //   Timer prüfen
                if (Timer_AKTIV) for (int i = 0; i < Timer.Count; i++) Timer[i].update(this, INC);

                // USART prüfen
                if (USART_AKTIV) for (int i = 0; i < USART.Count; i++) USART[i].update(this);

                if (Watchdog_AKTIV) if (WATCHDOG != null) WATCHDOG.update(this, INC);
                if (EEPROM_AKTIV) if (EEPROM != null) EEPROM.update(this, INC);

                if (Sleep > 1) { Sleep--; goto end_step; }
            }
            else
            {
                Watch_Minianwendungen[Selected_Software - 1].Start();
                Watch_Takte[Selected_Software - 1]++;
            }

            if ((!(SLEEP.Aktiv && SLEEP.IN) || !Hauptsoftware))
            {
                int temp = 0;
                Byte Res = 0;

                // Befehl interpretieren
                Zeile line = Program[Selected_Software][Counter[Selected_Software]];

                // Zaehler setzen
                if (Hauptsoftware)
                {
                    for (int i = 0; i < line.Zaehler_Start.Count; i++) ZAEHLER[line.Zaehler_Start[i]].Start();
                    for (int i = 0; i < line.Zaehler_Ende.Count; i++) ZAEHLER[line.Zaehler_Ende[i]].Stopp();
                    Count_Orders[line.Typ]++;
                    Watches[line.Typ].Start();
                }

                switch (line.Typ)
                {
                    case 0: // ADD
                        int Res2 = (Register[line.Param1] + Register[line.Param2]);
                        Res = (Byte)Res2;
                        NewCarry(Res2);
                        HalfCarry(Register[line.Param1], Register[line.Param2], Res);
                        TwoOver(Register[line.Param1], Register[line.Param2], Res);
                        Register[line.Param1] = Res;
                        Zero(Res, line);
                        Negativ(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 1: // ADC
                        Res = (Byte)(Register[line.Param1] + Register[line.Param2] + ((SREG & SREGC) > 0 ? 1 : 0));
                        Carry(Register[line.Param1], Register[line.Param2], Res);
                        HalfCarry(Register[line.Param1], Register[line.Param2], Res);
                        TwoOver(Register[line.Param1], Register[line.Param2], Res);
                        Register[line.Param1] = Res;
                        Zero(Res, line);
                        Negativ(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 2: // ADIW
                        Sleep = 2;
                        temp = (LowHigh(Register[line.Param1], Register[line.Param1 + 1]));
                        temp += line.Param2;
                        SetBitIOPort(INC.SREG, INC.N, (temp >= 32768) ? true : false);
                        SetBitIOPort(INC.SREG, INC.Z, temp == 0 ? true : false);
                        SetBitIOPort(INC.SREG, INC.V, (Register[line.Param1] & 128) == 0 && (temp & 32768) > 0 ? true : false);
                        SetBitIOPort(INC.SREG, INC.C, (Register[line.Param1] & 128) > 0 && (temp & 32768) == 0 ? true : false);
                        Signed(line);
                        Register[line.Param1] = Help.Low(temp);
                        Register[line.Param1 + 1] = Help.High(temp);

                        break;

                    case 3: // SUB
                        Sleep = 1;
                        Res = (Byte)(Register[line.Param1] - Register[line.Param2]);
                        INVHalfCarry(Register[line.Param1], Register[line.Param2], Res);
                        INVTwoOver(Register[line.Param1], Register[line.Param2], Res);
                        SREG = Res >= 128 ? (Byte)(SREG | SREGN) : (Byte)(SREG & SREGNON);
                        INVCarry(Register[line.Param1], Register[line.Param2], Res);
                        Zero(Res, line);
                        Signed(line);
                        Register[line.Param1] = Res;

                        break;

                    case 4: // SUBI
                        Sleep = 1;
                        Res = (Byte)(Register[line.Param1] - line.Param2);
                        INVHalfCarry(Register[line.Param1], (Byte)line.Param2, Res);
                        INVTwoOver(Register[line.Param1], (Byte)line.Param2, Res);
                        SREG = Res >= 128 ? (Byte)(SREG | SREGN) : (Byte)(SREG & SREGNON);
                        INVCarry(Register[line.Param1], (Byte)line.Param2, Res);
                        Zero(Res, line);
                        Signed(line);
                        Register[line.Param1] = Res;

                        break;

                    case 5: // SBC
                        Res = (Byte)(Register[line.Param1] - Register[line.Param2] - ((SREG & SREGC) > 0 ? 1 : 0));
                        INVHalfCarry(Register[line.Param1], Register[line.Param2], Res);
                        INVTwoOver(Register[line.Param1], Register[line.Param2], Res);
                        SREG = Res >= 128 ? (Byte)(SREG | SREGN) : (Byte)(SREG & SREGNON);
                        INVCarry(Register[line.Param1], Register[line.Param2], Res);
                        SREG = Res == 0 && (SREG & SREGZ) > 0 ? (Byte)(SREG | SREGZ) : (Byte)(SREG & SREGNOZ);
                        Signed(line);
                        Register[line.Param1] = Res;
                        Sleep = 1;
                        break;

                    case 6: // SBCI

                        Res = (Byte)(Register[line.Param1] - line.Param2 - ((SREG & SREGC) > 0 ? 1 : 0));
                        INVHalfCarry(Register[line.Param1], (Byte)line.Param2, Res);
                        INVTwoOver(Register[line.Param1], (Byte)line.Param2, Res);
                        SREG = Res >= 128 ? (Byte)(SREG | SREGN) : (Byte)(SREG & SREGNON);
                        INVCarry(Register[line.Param1], (Byte)line.Param2, Res);
                        SREG = Res == 0 && (SREG & SREGZ) > 0 ? (Byte)(SREG | SREGZ) : (Byte)(SREG & SREGNOZ);
                        Signed(line);
                        Register[line.Param1] = Res;
                        Sleep = 1;
                        break;

                    case 7: // SBIW

                        temp = (LowHigh(Register[line.Param1], Register[line.Param1 + 1]));
                        temp -= line.Param2;
                        SetBitIOPort(INC.SREG, INC.N, (temp >= 32768) ? true : false);
                        SetBitIOPort(INC.SREG, INC.Z, temp == 0 ? true : false);
                        SetBitIOPort(INC.SREG, INC.V, (Register[line.Param1] & 128) > 0 && (temp & 32768) == 0 ? true : false);
                        SetBitIOPort(INC.SREG, INC.C, (Register[line.Param1] & 128) == 0 && (temp & 32768) > 0 ? true : false);
                        Signed(line);
                        Register[line.Param1] = Help.Low(temp);
                        Register[line.Param1 + 1] = Help.High(temp);
                        Sleep = 2;
                        break;

                    case 8: // AND

                        Res = (Byte)(Register[line.Param1] & Register[line.Param2]);
                        Register[line.Param1] = Res;
                        SREG = (Byte)(SREG & SREGNOV);
                        Negativ(Res, line);
                        Zero(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 9: // ANDI

                        Res = (Byte)(Register[line.Param1] & line.Param2);
                        Register[line.Param1] = Res;
                        SREG = (Byte)(SREG & SREGNOV);
                        Negativ(Res, line);
                        Zero(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 10: // OR

                        Res = (Byte)(Register[line.Param1] | Register[line.Param2]);
                        Register[line.Param1] = Res;
                        SREG = (Byte)(SREG & SREGNOV);
                        Negativ(Res, line);
                        Zero(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 11: // ORI

                        Res = (Byte)(Register[line.Param1] | line.Param2);
                        Register[line.Param1] = Res;
                        SREG = (Byte)(SREG & SREGNOV);
                        Negativ(Res, line);
                        Zero(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 12: // EOR

                        Res = (Byte)(Register[line.Param1] ^ Register[line.Param2]);
                        Register[line.Param1] = Res;
                        SREG = (Byte)(SREG & SREGNOV);
                        Negativ(Res, line);
                        Zero(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 13: // COM
                        Res = (Byte)(~Register[line.Param1]);
                        Register[line.Param1] = Res;
                        if (Res >= 128)
                        {
                            SREG = (Byte)((Ports[INC.SREG].get() & 224) | 21);
                        }
                        else
                            SREG = Res == 0 ? (Byte)((SREG & 224) | 3) : (Byte)((SREG & 224) | 1);
                        Sleep = 1;
                        break;

                    case 14: // NEG

                        Res = (Byte)(0 - Register[line.Param1]);
                        SREG = (Register[line.Param1] & 8) > 0 || (Res & 8) > 0 ? (Byte)(SREG | SREGH) : (Byte)(SREG & SREGNOH);
                        SREG = Res == 128 ? (Byte)(SREG | SREGV) : (Byte)(SREG & SREGNOV);
                        Negativ(Res, line);
                        SREG = Res > 0 ? (Byte)(SREG | SREGC) : (Byte)(SREG & SREGNOC);
                        Zero(Res, line);
                        Signed(line);
                        Register[line.Param1] = Res;
                        Sleep = 1;
                        break;

                    case 15: // SBR

                        Res = (Byte)(Register[line.Param1] | line.Param2);
                        Register[line.Param1] = Res;
                        SREG = (Byte)(SREG & SREGNOV);
                        Negativ(Res, line);
                        Zero(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 16: // CBR

                        Res = (Byte)(Register[line.Param1] & (255 - line.Param2));
                        Register[line.Param1] = Res;
                        SREG = (Byte)(SREG & 247);
                        Negativ(Res, line);
                        Zero(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 17: // INC
                        ++Register[line.Param1];
                        Res = Register[line.Param1];
                        SREG = Res == 128 ? (Byte)(SREG | SREGV) : (Byte)(SREG & SREGNOV);
                        Negativ(Res, line);
                        Zero(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 18: // DEC
                        --Register[line.Param1];
                        Res = Register[line.Param1];
                        SREG = Res == 127 ? (Byte)(SREG | SREGV) : (Byte)(SREG & SREGNOV);
                        Negativ(Res, line);
                        Signed(line);
                        Zero(Res, line);
                        Sleep = 1;
                        break;

                    case 19: // TST

                        Res = (Byte)(Register[line.Param1] & Register[line.Param1]);
                        Register[line.Param1] = Res;
                        SREG = (Byte)(SREG & SREGNOV);
                        Negativ(Res, line);
                        Zero(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 20: // CLR
                        Register[line.Param1] = 0;
                        SREG = (Byte)(SREG | SREGZ);
                        Sleep = 1;
                        break;

                    case 21: // SER
                        Register[line.Param1] = 255;
                        Sleep = 1;
                        break;

                    case 22: // MUL
                        temp = Register[line.Param1] * Register[line.Param2];
                        if (temp == 0) { SREG = (Byte)(SREG | SREGZ); } else SREG = (Byte)(SREG & SREGNOZ);
                        Register[0] = Help.Low(temp);
                        Register[1] = Help.High(temp);
                        if ((temp & 32768) > 0) { SetBitIOPort(INC.SREG, INC.C, true); } else SetBitIOPort(INC.SREG, INC.C, false);
                        Sleep = 2;
                        break;

                    case 23: // MULS

                        Sleep = 2;
                        break;

                    case 24: // MULSU

                        Sleep = 2;
                        break;

                    case 25: // FMUL

                        Sleep = 2;
                        break;

                    case 26: // FMULS

                        Sleep = 2;
                        break;

                    case 27: // FMULSU

                        Sleep = 2;
                        break;

                    case 28: // RJMP
                        Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1;
                        Sleep = 2;
                        break;

                    case 29: // IJMP

                        Sleep = 2;
                        break;

                    case 30: // RCALL
                        addStack(Counter[Selected_Software] + 1);
                        Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1;
                        Sleep = 3;
                        break;

                    case 31: // ICALL

                        Sleep = 3;
                        break;

                    case 32: // RET
                        Counter[Selected_Software] = getStack() - 1;
                        Sleep = 4;
                        break;

                    case 33: // RETI
                        SREG = (byte)(SREG | SREGI);
                        Counter[Selected_Software] = getStack() - 1;
                        if (SLEEP.Aktiv) SLEEP.IN = true;
                        Sleep = 4;
                        break;

                    case 34: // CPSE
                        Sleep = 1;
                        if (Register[line.Param1] == Register[line.Param2]) { Counter[Selected_Software]++; Sleep++; }
                        break;

                    case 35: // CP

                        Res = (Byte)(Register[line.Param1] - Register[line.Param2]);
                        INVHalfCarry(Register[line.Param1], Register[line.Param2], Res);
                        INVTwoOver(Register[line.Param1], Register[line.Param2], Res);
                        Negativ(Res, line);
                        INVCarry(Register[line.Param1], Register[line.Param2], Res);
                        Zero(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 36: // CPC

                        Res = (Byte)(Register[line.Param1] - Register[line.Param2] - (GetBitIOPort(INC.SREG, INC.C) ? 1 : 0));
                        INVHalfCarry(Register[line.Param1], Register[line.Param2], Res);
                        INVTwoOver(Register[line.Param1], Register[line.Param2], Res);
                        Negativ(Res, line);
                        INVCarry(Register[line.Param1], Register[line.Param2], Res);
                        SREG = Res == 0 && (SREG & SREGZ) > 0 ? (Byte)(SREG | SREGZ) : (Byte)(SREG & SREGNOZ);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 37: // CPI

                        Res = (Byte)(Register[line.Param1] - line.Param2);
                        INVHalfCarry(Register[line.Param1], (Byte)line.Param2, Res);
                        INVTwoOver(Register[line.Param1], (Byte)line.Param2, Res);
                        Negativ(Res, line);
                        INVCarry(Register[line.Param1], (Byte)line.Param2, Res);
                        Zero(Res, line);
                        Signed(line);
                        Sleep = 1;
                        break;

                    case 38: // SBRC
                        Sleep = 1;
                        if ((Register[line.Param1] & (1 << line.Param2)) == 0) { Counter[Selected_Software]++; Sleep++; }
                        break;

                    case 39: // SBRS
                        Sleep = 1;
                        if ((Register[line.Param1] & (1 << line.Param2)) > 0) { Counter[Selected_Software]++; Sleep++; }
                        break;

                    case 40: // SBIC
                        Sleep = 1;
                        if (!Ports[line.Param1].getBit(line.Param2)) { Counter[Selected_Software]++; Sleep++; }
                        break;

                    case 41: // SBIS
                        Sleep = 1;
                        if (Ports[line.Param1].getBit(line.Param2)) { Counter[Selected_Software]++; Sleep++; }
                        break;

                    case 42: // BRBS
                        Sleep = 1;
                        if (GetBitIOPort(INC.SREG, (Byte)line.Param1)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param2] - 1; Sleep++; }
                        break;

                    case 43: // BRBC
                        Sleep = 1;
                        if (!GetBitIOPort(INC.SREG, (Byte)line.Param1)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param2] - 1; Sleep++; }
                        break;

                    case 44: // BREQ
                        Sleep = 1;
                        if (GetBitIOPort(INC.SREG, INC.Z)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 45: // BRNE
                        Sleep = 1;
                        if (!GetBitIOPort(INC.SREG, INC.Z)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 46: // BRCS
                        Sleep = 1;
                        if (GetBitIOPort(INC.SREG, INC.C)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 47: // BRCC
                        Sleep = 1;
                        if (!GetBitIOPort(INC.SREG, INC.C)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 48: // BRSH
                        Sleep = 1;
                        if (!GetBitIOPort(INC.SREG, INC.C)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 49: // BRLO
                        Sleep = 1;
                        if (GetBitIOPort(INC.SREG, INC.C)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 50: // BRMI
                        Sleep = 1;
                        if (GetBitIOPort(INC.SREG, INC.N)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 51: // BRPL
                        Sleep = 1;
                        if (!GetBitIOPort(INC.SREG, INC.N)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 52: // BRGE
                        Sleep = 1;
                        if ((GetBitIOPort(INC.SREG, INC.N) ^ GetBitIOPort(INC.SREG, INC.V)) == false) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 53: // BRLT
                        Sleep = 1;
                        if ((GetBitIOPort(INC.SREG, INC.N) ^ GetBitIOPort(INC.SREG, INC.V)) == true) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 54: // BRHS
                        Sleep = 1;
                        if (GetBitIOPort(INC.SREG, INC.H)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 55: // BRHC
                        Sleep = 1;
                        if (!GetBitIOPort(INC.SREG, INC.H)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 56: // BRTS
                        Sleep = 1;
                        if (GetBitIOPort(INC.SREG, INC.T)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 57: // BRTC
                        Sleep = 1;
                        if (!GetBitIOPort(INC.SREG, INC.T)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 58: // BRVS
                        Sleep = 1;
                        if (GetBitIOPort(INC.SREG, INC.V)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 59: // BRVC
                        Sleep = 1;
                        if (!GetBitIOPort(INC.SREG, INC.V)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 60: // BRIE
                        Sleep = 1;
                        if (GetBitIOPort(INC.SREG, INC.I)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 61: // BRID
                        Sleep = 1;
                        if (!GetBitIOPort(INC.SREG, INC.I)) { Counter[Selected_Software] = LabelPos[Selected_Software][line.Param1] - 1; Sleep++; }
                        break;

                    case 62: // MOV
                        Register[line.Param1] = Register[line.Param2];
                        Sleep = 1;
                        break;

                    case 63: // MOVW
                        Register[line.Param1] = Register[line.Param2];
                        Register[line.Param1 + 1] = Register[line.Param2 + 1];
                        Sleep = 1;
                        break;

                    case 64: // LDI
                        Register[line.Param1] = (Byte)line.Param2;
                        Sleep = 1;
                        break;

                    case 65: // LD
                        int a3 = 0;
                        int b3 = 0;
                        if (line.Param2 == 0)
                        { //X
                            a3 = INC.XL;
                            b3 = INC.XH;
                        }
                        else
                            if (line.Param2 == 1)
                            { //Y
                                a3 = INC.YL;
                                b3 = INC.YH;
                            }
                            else
                                if (line.Param2 == 2)
                                { //Z
                                    a3 = INC.ZL;
                                    b3 = INC.ZH;
                                }
                        int q3 = LowHigh(Register[a3], Register[b3]);
                        if (line.Param3 == -1) { q3--; Register[a3] = Help.Low(q3); Register[b3] = Help.High(q3); }
                        Register[line.Param1] = SRAM[q3];
                        if (line.Param3 == 1) { q3++; Register[a3] = Help.Low(q3); Register[b3] = Help.High(q3); }
                        Sleep = 2;
                        break;

                    case 66: // LDD
                        Sleep = 2;
                        break;

                    case 67: // LDS
                        //  if (line.Param2 >= SRAM.Count) return false;
                        Register[line.Param1] = SRAM[line.Param2];
                        Sleep = 2;
                        break;

                    case 68: // ST
                        int a2 = 0;
                        int b2 = 0;
                        if (line.Param1 == 0)
                        { //X
                            a2 = INC.XL;
                            b2 = INC.XH;
                        }
                        else
                            if (line.Param1 == 1)
                            { //Y
                                a2 = INC.YL;
                                b2 = INC.YH;
                            }
                            else
                                if (line.Param1 == 2)
                                { //Z
                                    a2 = INC.ZL;
                                    b2 = INC.ZH;
                                }
                        // int q = ;
                        int q2 = LowHigh(Register[a2], Register[b2]);
                        if (line.Param2 == -1) { q2--; Register[a2] = Help.Low(q2); Register[b2] = Help.High(q2); }
                        if (q2 >= SRAM.Count) goto end_step;
                        SRAM[q2] = Register[line.Param3];
                        if (line.Param2 == 1) { q2++; Register[a2] = Help.Low(q2); Register[b2] = Help.High(q2); }
                        Sleep = 2;
                        break;

                    case 69: // STD

                        Sleep = 2;
                        break;

                    case 70: // STS
                        SRAM[line.Param1] = Register[line.Param2];
                        Sleep = 2;
                        break;

                    case 71: // LPM
                        int temp3 = LowHigh(Register[INC.ZL], Register[INC.ZH]);
                        temp = temp3 / 2;
                        Register[line.Param1] = PM[temp];
                        if (line.Param2 == 1) { temp3 += 2; Register[INC.ZL] = Help.Low(temp3); Register[INC.ZH] = Help.High(temp3); }
                        Sleep = 3;
                        break;

                    case 72: // SPM
                        // nicht genutzt
                        Sleep = 1;
                        break;

                    case 73: // IN
                        Register[line.Param1] = Ports[line.Param2].get();
                        Sleep = 1;
                        break;

                    case 74: // OUT
                        Ports[line.Param1].set(Register[line.Param2]);
                        Sleep = 1;
                        break;

                    case 75: // PUSH
                        addStack(Register[line.Param1]);
                        Sleep = 2;
                        break;

                    case 76: // POP
                        Register[line.Param1] = (Byte)getStack();
                        Sleep = 2;
                        break;

                    case 77: // SBI
                        Ports[line.Param1].set(line.Param2, true);
                        Sleep = 2;
                        break;

                    case 78: // CBI
                        Ports[line.Param1].set(line.Param2, false);
                        Sleep = 2;
                        break;

                    case 79: // LSL
                        Res = (Byte)(Register[line.Param1] << 1);
                        if ((Register[line.Param1]) >= 128) { SREG = (Byte)(SREG | SREGC); } else SREG = (Byte)(SREG & SREGNOC);
                        if ((Register[line.Param1] & 8) > 0) { SREG = (Byte)(SREG | SREGH); } else SREG = (Byte)(SREG & SREGNOH);
                        Negativ(Res, line);
                        Zero(Res, line);
                        if (((SREG & 4) >> 2 ^ (SREG & 1)) > 0) { SREG = (Byte)(SREG | SREGV); } else SREG = (Byte)(SREG & SREGNOV);
                        Signed(line);
                        Register[line.Param1] = Res;
                        Sleep = 1;
                        break;

                    case 80: // LSR
                        Res = (Byte)(Register[line.Param1] >> 1);
                        if ((Register[line.Param1] & 1) > 0) { SREG = (Byte)(SREG | SREGC); } else SREG = (Byte)(SREG & SREGNOC);
                        SREG = (Byte)(SREG & SREGNON);
                        Zero(Res, line);
                        if (((SREG & 4) >> 2 ^ (SREG & 1)) > 0) { SREG = (Byte)(SREG | SREGV); } else SREG = (Byte)(SREG & SREGNOV);
                        Signed(line);
                        Register[line.Param1] = Res;
                        Sleep = 1;
                        break;

                    case 81: // ROL
                        Res = (Byte)((Register[line.Param1] << 1) + ((SREG & SREGC) > 0 ? 1 : 0));
                        if ((Register[line.Param1] & 8) > 0) { SREG = (Byte)(SREG | SREGH); } else SREG = (Byte)(SREG & SREGNOH);
                        SetBitIOPort(INC.SREG, INC.C, Register[line.Param1] >= 128 ? true : false);
                        Negativ(Res, line);
                        Zero(Res, line);
                        if (((SREG & 4) >> 2 ^ (SREG & 1)) > 0) { SREG = (Byte)(SREG | SREGV); } else SREG = (Byte)(SREG & SREGNOV);
                        Signed(line);
                        Register[line.Param1] = Res;
                        Sleep = 1;
                        break;

                    case 82: // ROR
                        Res = (Byte)((Register[line.Param1] >> 1) + ((SREG & SREGC) > 0 ? 128 : 0));//+ ((Register[line.Param1] & 1) > 0 ? 128 : 0))
                        if ((Register[line.Param1] & 1) > 0) { SREG = (Byte)(SREG | SREGC); } else SREG = (Byte)(SREG & SREGNOC);
                        Negativ(Res, line);
                        Zero(Res, line);
                        if (((SREG & 4) >> 2 ^ (SREG & 1)) > 0) { SREG = (Byte)(SREG | SREGV); } else SREG = (Byte)(SREG & SREGNOV);
                        Signed(line);
                        Register[line.Param1] = Res;
                        Sleep = 1;
                        break;

                    case 83: // ASR
                        Res = (Byte)((Register[line.Param1] >> 1) + ((Register[line.Param1] & 128) > 0 ? 128 : 0));
                        Negativ(Res, line);
                        Zero(Res, line);
                        if ((Register[line.Param1] & 1) > 0) { SREG = (Byte)(SREG | SREGC); } else SREG = (Byte)(SREG & SREGNOC);
                        if (((SREG & 4) >> 2 ^ (SREG & 1)) > 0) { SREG = (Byte)(SREG | SREGV); } else SREG = (Byte)(SREG & SREGNOV);
                        Signed(line);
                        Register[line.Param1] = Res;
                        Sleep = 1;
                        break;

                    case 84: // SWAP
                        Res = (Byte)((Register[line.Param1] << 4) + (Register[line.Param1] >> 4));
                        Register[line.Param1] = Res;
                        Sleep = 1;
                        break;

                    case 85: // BSET
                        SetBitIOPort(INC.SREG, (Byte)line.Param1, true);
                        Sleep = 1;
                        break;

                    case 86: // BCLR
                        SetBitIOPort(INC.SREG, (Byte)line.Param1, false);
                        Sleep = 1;
                        break;

                    case 87: // BST
                        SetBitIOPort(INC.SREG, INC.T, (Register[line.Param1] & (1 << line.Param2)) > 0 ? true : false);
                        Sleep = 1;
                        break;

                    case 88: // BLD
                        Register[line.Param1] = (Byte)(Register[line.Param1] | ((GetBitIOPort(INC.SREG, INC.T) ? 1 : 0) << line.Param2));
                        Sleep = 1;
                        break;

                    case 89: // SEC
                        SREG = (Byte)(SREG | SREGC);
                        Sleep = 1;
                        break;

                    case 90: // CLC
                        SREG = (Byte)(SREG & SREGNOV);
                        Sleep = 1;
                        break;

                    case 91: // SEN
                        SREG = (Byte)(SREG | SREGN);
                        Sleep = 1;
                        break;

                    case 92: // CLN;
                        SREG = (Byte)(SREG & SREGNON);
                        Sleep = 1;
                        break;

                    case 93: // SEZ
                        SREG = (Byte)(SREG | SREGZ);
                        Sleep = 1;
                        break;

                    case 94: // CLZ
                        SREG = (Byte)(SREG & SREGNOZ);
                        Sleep = 1;
                        break;

                    case 95: // SEI
                        SREG = (Byte)(SREG | SREGI);
                        Sleep = 1;
                        break;

                    case 96: // CLI
                        SREG = (Byte)(SREG & SREGNOI);
                        Sleep = 1;
                        break;

                    case 97: // SES
                        SREG = (Byte)(SREG | SREGS);
                        Sleep = 1;
                        break;

                    case 98: // CLS
                        SREG = (Byte)(SREG & SREGNOS);
                        Sleep = 1;
                        break;

                    case 99: // SEV
                        SREG = (Byte)(SREG | SREGV);
                        Sleep = 1;
                        break;

                    case 100: // CLV
                        SREG = (Byte)(SREG & SREGNOV);
                        Sleep = 1;
                        break;

                    case 101: // SET
                        SREG = (Byte)(SREG | SREGT);
                        Sleep = 1;
                        break;

                    case 102: // CLT
                        SREG = (Byte)(SREG & SREGNOT);
                        Sleep = 1;
                        break;

                    case 103: // SEH
                        SREG = (Byte)(SREG | SREGH);
                        Sleep = 1;
                        break;

                    case 104: // CLH
                        SREG = (Byte)(SREG & SREGNOH);
                        Sleep = 1;
                        break;

                    case 105: // NOP
                        Sleep = 1;
                        break;

                    case 106: // WDR
                        if (WATCHDOG != null) WATCHDOG.Reset(this);
                        Sleep = 1;
                        break;

                    case 107: // SLEEP
                        if (GetBitIOPort(INC.MCUCR, INC.SE))
                        {
                            // konfigurieren
                            SLEEP.Mode = (GetBitIOPort(INC.MCUCR, INC.SM0) ? 1 : 0) + (GetBitIOPort(INC.MCUCR, INC.SM1) ? 2 : 0) + (GetBitIOPort(INC.MCUCR, INC.SM2) ? 4 : 0);
                            SLEEP.Aktiv = true;
                            SLEEP.IN = true;
                            Counter[Selected_Software]--;
                        }
                        Sleep = 1;
                        break;

                    default:
                        if (Hauptsoftware) Watches[line.Typ].Stop();
                        // if (Hauptsoftware) { hauptWatch.Stop(); } else Watch_Minianwendungen[Selected_Software - 1].Stop();
                        goto end_step;
                }

                Counter[Selected_Software]++;
                if (Hauptsoftware) { Watches[line.Typ].Stop(); } else Watch_Minianwendungen[Selected_Software - 1].Stop();
            }

        end_step:
            for (int i = 0; i < ActiveOszi.Count; i++)
            {
                ActiveOszi[i].Store_Port(ActiveOsziPin[i]);
            }

            if (Hauptsoftware) { hauptWatch.Stop(); } else Watch_Minianwendungen[Selected_Software - 1].Stop();

            return true;
        }
    }
}