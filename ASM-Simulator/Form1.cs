using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace ASM_Simulator
{
    public partial class Form1 : Form
    {
        List<Atmega> Main = new List<Atmega>();
        List<Atmega> CopyMain = new List<Atmega>();
        public int selected = -1;
        List<Task> Simulator = new List<Task>();
        Task Looping;
        List<int> Simulator_anz = new List<int>();
        Task ControlStep = null;
        int Prog = 0;
        String Projekt = "";
        int sel = 0;
        public bool ausschalten = false;

        // Szenarien
        List<SZENARIEN> global_scenery = new List<SZENARIEN>();

        List<String> Minianwendungen = new List<String>();
        List<String> Minianwendungen_Datei = new List<String>();


        public void Start_Progress(int max)
        {
            progressBar1.Value = 0;
            progressBar1.Maximum = max;
            Prog = 0;
            progressBar1.Show();
            progressBar1.Refresh();
        }

        public void Stop_Progress()
        {
            Prog = 0;
            progressBar1.Hide();
            progressBar1.Refresh();
        }

        public void Step(int add)
        {
            //     Action<object> action2 = (object obj) =>
            // {
            progressBar1.Value += add;
            // };
        }

        public Form1()
        {
            InitializeComponent();
            Original_Pos[0] = dataGridView1.Location.X;
            Original_Pos[1] = dataGridView2.Location.X;
            Original_Pos[2] = dataGridView4.Location.X;
            Original_Pos[3] = dataGridView5.Location.X;
            Original_Pos[4] = dataGridView9.Location.X;
            Original_Pos[5] = richTextBox1.Location.X;
            comboBox6.SelectedIndex = 0;
            comboBox7.SelectedIndex = 0;
            //Simulator.Add(new Task(sim, "alpha"));
            Eingabe.Parent = this;
            Eingabe.BringToFront();
            reload_geraete_table();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            timer2.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Main == null) return;
            openFileDialog1.InitialDirectory = Application.StartupPath;
            openFileDialog1.FileName = "";
            openFileDialog1.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.InitialDirectory = Application.StartupPath;
            openFileDialog2.FileName = "";
            openFileDialog2.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            Start_Progress(100);
            if (Projekt != "")
            {
                if (sel < Main.Count)
                {
                    Main[sel].ZAEHLER.Clear();
                    for (int i = 0; i < Main[sel].Program[0].Count; i++) { Main[sel].Program[0][i].Zaehler_Ende.Clear(); Main[sel].Program[0][i].Zaehler_Start.Clear(); }
                }
            }
            String dat = openFileDialog1.FileName;
            Main[sel].QuellDatei = dat;

            Step(50);
            StreamReader datei = new StreamReader(dat);
            List<String> data = new List<String>();
            while (datei.Peek() != -1)
            {
                data.Add(datei.ReadLine());
            }
            label13.Show();
            label13.Text = openFileDialog1.SafeFileName;
            datei.Close();
            // Reset Software
            Main[sel].clearRegDef();
            Main[sel].clearVarDef();
            Main[sel].clearLabelDef();
            Main[sel].clearSRAMDef();
            Main[sel].ClearStack();
            for (; Main[sel].Counter.Count > 1; ) Main[sel].Counter.RemoveAt(1);
            for (; Main[sel].Quelltext.Count > 1; ) Main[sel].Quelltext.RemoveAt(1);
            for (; Main[sel].Program.Count > 1; ) Main[sel].Program.RemoveAt(1);
            for (; Main[sel].Label.Count > 1; ) Main[sel].Label.RemoveAt(1);
            for (; Main[sel].LabelPos.Count > 1; ) Main[sel].LabelPos.RemoveAt(1);
            for (; Main[sel].Watch_Minianwendungen.Count > 1; ) Main[sel].Watch_Minianwendungen.RemoveAt(1);
            for (; Main[sel].Watch_Takte.Count > 1; ) Main[sel].Watch_Takte.RemoveAt(1);
            Main[sel].Load_Content(data, true);
            button5_Click(null, null);
            reload_geraete_table();
            for (int i = 0; i < Main[sel].Timer.Count; i++) Main[sel].Timer[i].Reload_INC(Main[sel]);
            for (int i = 0; i < Main[sel].USART.Count; i++) Main[sel].USART[i].Reload_INC(Main[sel]);
            Stop_Progress();
        }

        public String get(String[] data, String Bez)
        {
            Bez = Bez.ToUpper();
            for (int i = 0; i < data.Length; i++)
            {
                String[] mom = data[i].Split('=');
                if (mom[0].ToUpper() == Bez)
                {
                    return mom[1];
                }
            }
            return "";
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            timer3.Enabled = false;
            //  
            String dat = openFileDialog2.FileName;
            int anz = 0;
            StreamReader datei = new StreamReader(dat);
            while (datei.Peek() != -1) { datei.ReadLine(); anz++; }
            datei.Close();
            Prog = 10;
            String[] data = new String[anz];
            datei = new StreamReader(dat);
            int i = 0;
            while (datei.Peek() != -1)
            {
                data[i] = datei.ReadLine();
                i++;
            }
            datei.Close();
            // interpretieren

            String[] P = get(data, "Ports").Split(',');
            if (P.Length == 0) { P = new String[1]; P[0] = get(data, "Ports"); }
            for (int b = 0; b < P.Length; b++) P[b] = Help.free(P[b]);
            int reg = Convert.ToInt32(get(data, "Register"));
            String OldNAME = "";
            String OldFile = "";
            if (sel >= Main.Count) { Main.Add(new Atmega(reg, listBox1.SelectedIndex)); } else { OldFile = Main[sel].QuellDatei; OldNAME = Main[sel].Name; Main[sel] = new Atmega(reg, listBox1.SelectedIndex); }
            Prog = 30;
            Main[sel].Bez = get(data, "Name");
            int por = P.Length;
            for (int b = 0; b < P.Length; b++)
            {
                int an = Convert.ToInt32(get(data, P[b]));
                Main[sel].Ports.Add(new PORT("PORT" + P[b], an));
                for (int c = 0; c < an; c++)
                {
                    String q = get(data, P[b] + c);
                    String[] temp = q.Split(',');
                    if (temp.Count() == 0) { temp = new String[1]; temp[0] = q; }
                    for (int d = 0; d < temp.Count(); d++)
                    {
                        temp[d] = Help.free(temp[d]);
                        if (temp[d] != "")
                        {
                            Main[sel].Ports[Main[sel].getIORegister("PORT" + P[b])].set_konf(c, temp[d]);
                        }
                    }
                }
            }


            for (int b = por; b < 2 * por; b++)
            {
                int an = Convert.ToInt32(get(data, P[b - por]));
                Main[sel].Ports.Add(new PORT("PIN" + P[b - por], an));
                Main[sel].Ports[Main[sel].getIORegister("PIN" + P[b - por])].set(0);
            }

            for (int b = 2 * por; b < 3 * por; b++)
            {
                int an = Convert.ToInt32(get(data, P[b - 2 * por]));
                Main[sel].Ports.Add(new PORT("DDR" + P[b - 2 * por], an));
            }

            Prog = 80;
            // Timer einrichten
            int max = Convert.ToInt32(get(data, "Timer"));
            for (i = 0; i < max; i++)
            {
                String q = "Timer" + i.ToString();
                String q2 = get(data, q).ToUpper();
                q2 = Help.free(q2);
                if (q2 == "8BIT")
                {
                    Main[sel].Generate_Timer(i, 0, max);
                }
                else
                    if (q2 == "16BIT")
                    {
                        Main[sel].Generate_Timer(i, 1, max);
                    }
            }

            // Watchdog einrichten
            if (Help.free(get(data, "Watchdog")) == "TRUE")
            {
                Main[sel].WATCHDOG = new WATCHDOG();
                Main[sel].Ports.Add(new PORT("WDTCR", 5));
            }

            // USART einrichten
            if (Help.IsNumber(Help.free(get(data, "USART"))))
            {
                int anz3 = Convert.ToInt32(Help.free(get(data, "USART")));
                for (int i2 = 0; i2 < anz3; i2++)
                {
                    string ADD = anz3 > 1 ? i2.ToString() : "";
                    Main[sel].USART.Add(new USART(i2, anz3));
                    Main[sel].Ports.Add(new PORT("UDR" + ADD, 8));
                    Main[sel].Ports.Add(new PORT("UBRR" + ADD + "H", 8));
                    Main[sel].Ports.Add(new PORT("UCSR" + ADD + "C", 8));
                    Main[sel].Ports.Add(new PORT("UCSR" + ADD + "A", 8));
                    Main[sel].Ports.Add(new PORT("UCSR" + ADD + "B", 8));
                    Main[sel].Ports.Add(new PORT("UBRR" + ADD + "L", 8));
                }
            }

            // EEPROM einrichten
            int anz2 = Help.IsNumber(Help.free(get(data, "EEPROM"))) ? Convert.ToInt32(Help.free(get(data, "EEPROM"))) : 0;
            if (anz2 > 0)
            {
                Main[sel].EEPROM = new EEPROM(anz2);
                Main[sel].Ports.Add(new PORT("EEARL", 8));
                Main[sel].Ports.Add(new PORT("EEARH", 1));
                Main[sel].Ports.Add(new PORT("EEDR", 8));
                Main[sel].Ports.Add(new PORT("EECR", 4));
            }

            // SLEEP einrichten
            Main[sel].Ports.Add(new PORT("MCUCR", 8));

            if (Main[sel].Name == "") Main[sel].Name = "neu";
            Main[sel].Datei = dat;
            if (OldNAME != "")
            {
                Main[sel].Name = OldNAME;
                Main[sel].QuellDatei = OldFile;
                openFileDialog1.FileName = Main[sel].QuellDatei;
                if (openFileDialog1.FileName != "") openFileDialog1_FileOk(null, null);
            }

            listBox1.Items[Main[sel].ID] = Main[sel].Name;
            comboBox5.Items[Main[sel].ID] = Main[sel].Name;
            comboBox4.Items[Main[sel].ID] = Main[sel].Name;
            comboBox16.Items[Main[sel].ID] = Main[sel].Name;
            if (comboBox5.SelectedIndex < 0) comboBox5.SelectedIndex = Main[sel].ID;
            if (comboBox4.SelectedIndex < 0) comboBox4.SelectedIndex = Main[sel].ID;
            if (comboBox16.SelectedIndex < 0) comboBox16.SelectedIndex = Main[sel].ID;
            listBox1.SelectedIndex = Main[sel].ID;
            sel = listBox1.SelectedIndex;
            label12.Show();
            label12.Text = Main[sel].Bez;
            reload_geraete_table();
            //button5.Show();
            Prog = 90;
        }

        private void richTextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            /*if (e.Button == MouseButtons.Right)
            {
                
            }*/
        }

        public bool Load_Quelltext(int id)
        {
            if (Main[id].Text != null) return false;
            Main[id].Text = new RichTextBox();
            Main[id].Text.Parent = this;
            Main[id].Text.Show();
            Main[id].Text.Clear();
            for (int i = 0; i < Main[id].Quelltext[0].Count(); i++)
            {
                Text = Main[id].Quelltext[0][i];
                Main[id].Text.AppendText(Text + "\n");
                Main[id].Text.Select(Main[id].Text.Text.Length - Text.Length - 1, Text.Length + 1);
                Main[id].Text.SelectionColor = Color.Black;
            }
            return true;
        }

        public void change_selected()
        {
            if (sel >= Main.Count || sel<0) return;
            //richTextBox1.Lines = Main[sel].Text.Lines;
            if (Main[sel].Text == null) return;
            richTextBox1.Rtf = Main[sel].Text.Rtf;
            dataGridView1.Rows.Clear();
            dataGridView1.Rows.Add(Main[sel].Register.Length - dataGridView1.Rows.Count);
            for (int i = 0; i < Main[sel].Register.Length; i++)
            {
                dataGridView1.Rows[i].HeaderCell.Value = "R" + i;
            }

            for (int i = 0; i < Main[sel].Regs.Count(); i++)
            {
                dataGridView1.Rows[Main[sel].RegsPos[i]].HeaderCell.Value = dataGridView1.Rows[Main[sel].RegsPos[i]].HeaderCell.Value + "|" + Main[sel].Regs[i];
            }

            dataGridView4.Rows.Clear();
            dataGridView4.Rows.Add(Main[sel].Ports.Count - dataGridView4.Rows.Count);
            for (int i = 0; i < Main[sel].Ports.Count; i++)
            {
                dataGridView4.Rows[i].HeaderCell.Value = Main[sel].Ports[i].Bezeichnung;
            }

            dataGridView5.Rows.Clear();
            dataGridView5.Rows.Add(Main[sel].EEPROM.SPEICHER.Count() - dataGridView5.Rows.Count);
            for (int i = 0; i < Main[sel].EEPROM.SPEICHER.Count(); i++)
            {
                dataGridView5.Rows[i].HeaderCell.Value = i.ToString();
            }

            dataGridView2.Rows.Clear();
            int a = Main[sel].SRAM.Count() - dataGridView2.Rows.Count;
            if (a > 0) dataGridView2.Rows.Add(a);
            for (int i = 0; i < Main[sel].SRAM.Count(); i++)
            {
                dataGridView2.Rows[i].HeaderCell.Value = Main[sel].SRAM_name[i];
            }

            dataGridView9.Rows.Clear();
            a = Main[sel].PM.Count() - dataGridView9.Rows.Count;
            if (a > 0) dataGridView9.Rows.Add(a);
            for (int i = 0; i < Main[sel].PM.Count(); i++)
            {
                dataGridView9.Rows[i].HeaderCell.Value = Main[sel].PM_name[i];
            }
            reload_table();
            reload_quelltext();
            reload_flags();
            reload_SRAM();
            reload_Ports();
            reload_eeprom();
            reload_PM();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bool found = false;
            for (int i = 0; i < Main.Count; i++)
            {
                if (Load_Quelltext(i)) found = true;
                general_quelltext(i);
                Main[i].INC.Reload(Main[i]);
            }

            button3.Show();
            if (found) change_selected();
            timer1.Enabled = true;
            button4.Show();
            button16.Show();
            button13.Show();
            button14.Show();
        }

        public void reload_table()
        {
            if (checkBox1.Checked == false) return;
            if (sel >= Main.Count) return;
            for (int i = 0; i < Main[sel].Register.Length; i++)
            {
                if (i >= dataGridView1.Rows.Count) continue;
                if (dataGridView1.Rows[i].Cells[1].Value == null || dataGridView1.Rows[i].Cells[1].Value.ToString() == "INT")
                {
                    dataGridView1.Rows[i].Cells[0].Value = Main[sel].Register[i].ToString();
                }
                else
                    if (dataGridView1.Rows[i].Cells[1].Value.ToString() == "BIN")
                    {
                        String a = Convert.ToString(Main[sel].Register[i], 2);
                        for (int b = a.Length; b < 8; b++) a = "0" + a;
                        dataGridView1.Rows[i].Cells[0].Value = a;
                    }
                    else
                        if (dataGridView1.Rows[i].Cells[1].Value.ToString() == "HEX")
                        {
                            String a = Convert.ToString(Main[sel].Register[i], 16).ToUpper();
                            if (a.Length <= 1) a = "0" + a;
                            dataGridView1.Rows[i].Cells[0].Value = dataGridView1.Rows[i].Cells[0].Value = a;
                        }
                        else
                            if (dataGridView1.Rows[i].Cells[1].Value.ToString() == "ASC")
                            {
                                String a = Help.ConvertToAscii(Main[sel].Register[i]);
                                if (a.Length < 1) a = "[None]";
                                dataGridView1.Rows[i].Cells[0].Value = dataGridView1.Rows[i].Cells[0].Value = a;
                            }
            }
        }

        public void reload_eeprom()
        {
            if (sel >= Main.Count) return;
            if (Main[sel].EEPROM == null) return;
            if (checkBox4.Checked == false) return;
            for (int i = 0; i < Main[sel].EEPROM.SPEICHER.Count(); i++)
            {
                if (i >= dataGridView5.Rows.Count) continue;
                if (dataGridView5.Rows[i].Cells[1].Value == null || dataGridView5.Rows[i].Cells[1].Value.ToString() == "INT")
                {
                    dataGridView5.Rows[i].Cells[0].Value = Main[sel].EEPROM.SPEICHER[i].ToString();
                }
                else
                    if (dataGridView5.Rows[i].Cells[1].Value.ToString() == "BIN")
                    {
                        String a = Convert.ToString(Main[sel].EEPROM.SPEICHER[i], 2);
                        for (int b = a.Length; b < 8; b++) a = "0" + a;
                        dataGridView5.Rows[i].Cells[0].Value = a;
                    }
                    else
                        if (dataGridView5.Rows[i].Cells[1].Value.ToString() == "HEX")
                        {
                            String a = Convert.ToString(Main[sel].EEPROM.SPEICHER[i], 16).ToUpper();
                            if (a.Length <= 1) a = "0" + a;
                            dataGridView5.Rows[i].Cells[0].Value = dataGridView5.Rows[i].Cells[0].Value = a;
                        }
                        else
                            if (dataGridView5.Rows[i].Cells[1].Value.ToString() == "ASC")
                            {
                                String a = Help.ConvertToAscii(Main[sel].EEPROM.SPEICHER[i]);
                                if (a.Length < 1) a = "[None]";
                                dataGridView5.Rows[i].Cells[0].Value = dataGridView5.Rows[i].Cells[0].Value = a;
                            }
            }
        }

        public void reload_Ports()
        {
            if (checkBox3.Checked == false) return;
            if (sel >= Main.Count) return;
            for (int i = 0; i < Main[sel].Ports.Count; i++)
            {
                if (i >= dataGridView4.Rows.Count) continue;
                if (dataGridView4.Rows[i].Cells[1].Value == null || dataGridView4.Rows[i].Cells[1].Value.ToString() == "INT")
                {
                    dataGridView4.Rows[i].Cells[0].Value = Main[sel].Ports[i].get().ToString();
                }
                else
                    if (dataGridView4.Rows[i].Cells[1].Value.ToString() == "BIN")
                    {
                        String a = Convert.ToString(Main[sel].Ports[i].get(), 2);
                        for (int b = a.Length; b < 8; b++) a = "0" + a;
                        dataGridView4.Rows[i].Cells[0].Value = a;
                    }
                    else
                        if (dataGridView4.Rows[i].Cells[1].Value.ToString() == "HEX")
                        {
                            String a = Convert.ToString(Main[sel].Ports[i].get(), 16).ToUpper();
                            if (a.Length <= 1) a = "0" + a;
                            dataGridView4.Rows[i].Cells[0].Value = dataGridView4.Rows[i].Cells[0].Value = a;
                        }
                        else
                            if (dataGridView4.Rows[i].Cells[1].Value.ToString() == "ASC")
                            {
                                String a = Help.ConvertToAscii(Main[sel].Ports[i].get());
                                if (a.Length < 1) a = "[None]";
                                dataGridView4.Rows[i].Cells[0].Value = dataGridView4.Rows[i].Cells[0].Value = a;
                            }
            }
        }

        public void reload_flags()
        {
            if (sel >= Main.Count) return;
            if (Main[sel].GetBitIOPort(Main[sel].INC.SREG, Main[sel].INC.Z))
            {
                panel1.BorderStyle = BorderStyle.Fixed3D;
            }
            else
            {
                panel1.BorderStyle = BorderStyle.FixedSingle;
            }

            if (Main[sel].GetBitIOPort(Main[sel].INC.SREG, Main[sel].INC.C))
            {
                panel2.BorderStyle = BorderStyle.Fixed3D;
            }
            else
            {
                panel2.BorderStyle = BorderStyle.FixedSingle;
            }

            if (Main[sel].GetBitIOPort(Main[sel].INC.SREG, Main[sel].INC.N))
            {
                panel3.BorderStyle = BorderStyle.Fixed3D;
            }
            else
            {
                panel3.BorderStyle = BorderStyle.FixedSingle;
            }

            if (Main[sel].GetBitIOPort(Main[sel].INC.SREG, Main[sel].INC.V))
            {
                panel4.BorderStyle = BorderStyle.Fixed3D;
            }
            else
            {
                panel4.BorderStyle = BorderStyle.FixedSingle;
            }

            if (Main[sel].GetBitIOPort(Main[sel].INC.SREG, Main[sel].INC.H))
            {
                panel5.BorderStyle = BorderStyle.Fixed3D;
            }
            else
            {
                panel5.BorderStyle = BorderStyle.FixedSingle;
            }

            if (Main[sel].GetBitIOPort(Main[sel].INC.SREG, Main[sel].INC.I))
            {
                panel6.BorderStyle = BorderStyle.Fixed3D;
            }
            else
            {
                panel6.BorderStyle = BorderStyle.FixedSingle;
            }

            if (Main[sel].GetBitIOPort(Main[sel].INC.SREG, Main[sel].INC.T))
            {
                panel7.BorderStyle = BorderStyle.Fixed3D;
            }
            else
            {
                panel7.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        public void general_quelltext(int id)
        {
            bool[] set = new bool[Main[id].Quelltext[0].Count()];
            for (int i = 0; i < Main[id].Program[0].Count(); i++) set[Main[id].Program[0][i].Original] = true;
            int summ = 0;
            int pos;
            int old_pos = 0;
            for (int i = 0; i < Main[id].Quelltext[0].Count(); i++)
            {
                if (set[i]) continue;
                pos = i;
                for (int b = old_pos; b < pos; b++) summ += Main[id].Text.Lines[b].Length + 1;
                //summ = Main[id].Text.GetFirstCharIndexFromLine(pos);
                Main[id].Text.Select(summ, Main[id].Text.Lines[pos].Length);
                Main[id].Text.SelectionBackColor = this.richTextBox1.BackColor;
                Main[id].Text.SelectionColor = Color.Gray;
                old_pos = pos;
            }
        }

        public void reload_SRAM()
        {
            if (checkBox2.Checked == false) return;
            if (sel >= Main.Count) return;
            for (int i = 0; i < Main[sel].SRAM.Count(); i++)
            {
                if (i >= dataGridView2.Rows.Count) continue;
                if (dataGridView2.Rows[i].Cells[1].Value == null || dataGridView2.Rows[i].Cells[1].Value.ToString() == "INT")
                {
                    dataGridView2.Rows[i].Cells[0].Value = Main[sel].SRAM[i].ToString();
                }
                else
                    if (dataGridView2.Rows[i].Cells[1].Value.ToString() == "BIN")
                    {
                        String a = Convert.ToString(Main[sel].SRAM[i], 2);
                        for (int b = a.Length; b < 8; b++) a = "0" + a;
                        dataGridView2.Rows[i].Cells[0].Value = a;
                    }
                    else
                        if (dataGridView2.Rows[i].Cells[1].Value.ToString() == "HEX")
                        {
                            String a = Convert.ToString(Main[sel].SRAM[i], 16).ToUpper();
                            if (a.Length <= 1) a = "0" + a;
                            dataGridView2.Rows[i].Cells[0].Value = dataGridView2.Rows[i].Cells[0].Value = a;
                        }
                        else
                            if (dataGridView2.Rows[i].Cells[1].Value.ToString() == "ASC")
                            {
                                String a = Help.ConvertToAscii(Main[sel].SRAM[i]);
                                if (a.Length < 1) a = "[None]";
                                dataGridView2.Rows[i].Cells[0].Value = dataGridView2.Rows[i].Cells[0].Value = a;
                            }
            }
        }

        public void reload_PM()
        {
            if (checkBox5.Checked == false) return;
            if (sel >= Main.Count) return;
            for (int i = 0; i < Main[sel].PM.Count(); i++)
            {
                if (i >= dataGridView9.Rows.Count) continue;
                if (dataGridView9.Rows[i].Cells[1].Value == null || dataGridView9.Rows[i].Cells[1].Value.ToString() == "INT")
                {
                    dataGridView9.Rows[i].Cells[0].Value = Main[sel].PM[i].ToString();
                }
                else
                    if (dataGridView9.Rows[i].Cells[1].Value.ToString() == "BIN")
                    {
                        String a = Convert.ToString(Main[sel].PM[i], 2);
                        for (int b = a.Length; b < 8; b++) a = "0" + a;
                        dataGridView9.Rows[i].Cells[0].Value = a;
                    }
                    else
                        if (dataGridView9.Rows[i].Cells[1].Value.ToString() == "HEX")
                        {
                            String a = Convert.ToString(Main[sel].PM[i], 16).ToUpper();
                            if (a.Length <= 1) a = "0" + a;
                            dataGridView9.Rows[i].Cells[0].Value = dataGridView9.Rows[i].Cells[0].Value = a;
                        }
                        else
                            if (dataGridView9.Rows[i].Cells[1].Value.ToString() == "ASC")
                            {
                                String a = Help.ConvertToAscii(Main[sel].PM[i]);
                                if (a.Length < 1) a = "[None]";
                                dataGridView9.Rows[i].Cells[0].Value = dataGridView9.Rows[i].Cells[0].Value = a;
                            }
            }
        }

        public void Draw_Zaehler_In_Line(RichTextBox TextBox, int MainID, int Line)
        {
            int real = 0;
            for (int i = 0; i < Main[MainID].Program[0].Count; i++)
            {
                if (Main[MainID].Program[0][i].Original == Line) { real = i; break; }
            }

            int len = 0;
            int start = 0;
            start = TextBox.GetFirstCharIndexFromLine(Line);
            if (TextBox != richTextBox1)
            {
                start = 0;
                for (int i = 0; i < Line; i++) start += TextBox.Lines[i].Length + 1;
            }

            if (Line != selected || selected == -1)
            {
                len = TextBox.Lines[Line].Length;
                //start = TextBox.GetFirstCharIndexFromLine(Line);
                TextBox.Select(start, len);
                TextBox.SelectionBackColor = richTextBox1.BackColor;
                TextBox.SelectionColor = Color.Black;
            }
            else
            {
                len = TextBox.Lines[Line].Length;

                TextBox.Select(start, len);
                TextBox.SelectionColor = Color.White;
                TextBox.SelectionBackColor = Color.Black;
            }

            len = 0;
            for (int i = 0; i < Main[MainID].Program[0][real].Zaehler_Start.Count; i++) len += Main[MainID].ZAEHLER[Main[MainID].Program[0][real].Zaehler_Start[i]].Begin_Length;
            if (len == 0) goto ende;
            // start = TextBox.GetFirstCharIndexFromLine(Line);
            TextBox.Select(start, len);
            TextBox.SelectionBackColor = richTextBox1.BackColor;
            TextBox.SelectionColor = Color.Olive;

        ende:
            len = 0;
            for (int i = 0; i < Main[MainID].Program[0][real].Zaehler_Ende.Count; i++) len += Main[MainID].ZAEHLER[Main[MainID].Program[0][real].Zaehler_Ende[i]].Ende_Length;
            if (len == 0) return;
            /*start = TextBox.GetFirstCharIndexFromLine(Line);
            if (TextBox != richTextBox1)
            {
                start = 0;
                for (int i = 0; i < Line; i++) start += TextBox.Lines[i].Length + 1;
            }*/
            TextBox.Select(start + TextBox.Lines[Line].Length - len, len);
            TextBox.SelectionBackColor = richTextBox1.BackColor;
            TextBox.SelectionColor = Color.Navy;
            TextBox.Select(start, 0);
        }

        public void reload_quelltext()
        {
            if (sel >= Main.Count) return;
            if (Main[sel].Counter[0] >= Main[sel].Program[0].Count) return;
            int pos;
            int laste = 0;
            int q = selected;
            pos = Main[sel].Program[0][Main[sel].Counter[0]].Original;
            selected = pos;

            if (q != -1)
            {
                if (q >= richTextBox1.Lines.Count()) return;
                Draw_Zaehler_In_Line(richTextBox1, sel, q);
            }

            laste = selected - 12;
            if (laste < 0) laste = 0;
            laste = richTextBox1.GetFirstCharIndexFromLine(laste);

            Draw_Zaehler_In_Line(richTextBox1, sel, selected);

            richTextBox1.Select(laste, 1);
            richTextBox1.ScrollToCaret();

        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            String[] data = { "INT", "BIN", "HEX", "ASC" };
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[1].Value == null)
                {
                    dataGridView1.Rows[e.RowIndex].Cells[1].Value = data[1];
                    reload_table();
                    return;
                }

                String dat = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                for (int i = 0; i < data.Count(); i++)
                {
                    if (data[i] == dat)
                    {
                        dataGridView1.Rows[e.RowIndex].Cells[1].Value = data[(i + 1) % data.Count()];
                        reload_table();
                        break;
                    }
                }
            }
            else
                if (e.ColumnIndex == 0 && e.RowIndex >= 0)
                {
                    // Eintragen
                    Eingabe.Show();
                }
        }

        public void Starte_Step(int id, int anz)
        {
            //if (button16.Text != "Loop") return;

            Action<object> action = (object obj) =>
            {
                bool aktiv = false;
                for (; ; )
                {
                    if (Simulator_anz[id] == 0 && !In_Loop)
                    {
                        Thread.Sleep(1);
                    }

                    if (Stop_Looping) { In_Loop = false; Simulator_anz[id] = 0; }
                    if (Simulator_anz[id] > 0) aktiv = false; //  || In_Loop
                    for (; Simulator_anz[id] > 0; Simulator_anz[id]--) //  || In_Loop
                    {
                        if (Stop_Looping) { In_Loop = false; Simulator_anz[id] = 0; break; } //  || (!In_Loop && Simulator_anz[id] <= 0)

                        if (!Main[id].Step(global_scenery)) {  ausschalten = true;Stop_Looping = true; In_Loop = false;/* Simulator_anz[id] = 0;*/ } // bei Fehler, simulator anhalten
                    }
                    if (aktiv == false && Simulator_anz[id] == 0 && !In_Loop)
                    {
                        aktiv = true;
                        reload_komm = true;
                        bool get = false;
                        for (int i = 0; i < Simulator.Count; i++) if (Simulator_anz[i] > 0) { get = true; break; }
                        if (!get) timer4.Enabled = false;
                    }
                }
            };

            if (id < Simulator.Count)
            {
                if (Simulator[id] == null)
                {
                    Simulator[id] = new Task(action, "Step" + id.ToString());
                    Simulator[id].Start();
                }
                Simulator_anz[id] = anz;
                    return;        
            }

            for (; ; )
            {
                if (id >= Simulator.Count)
                {
                    Simulator_anz.Add(0); Simulator.Add(null);
                }
                else
                    break;
            }
            if (Simulator[id] == null)
            {
                Simulator[id] = new Task(action, "Step" + id.ToString());
                Simulator_anz[id] = anz;
                Simulator[id].Start();
            }

        }

        public void Control_Step(int anz, bool all, bool synchron)
        {
            if (sel >= Main.Count) return;
            if (!Help.IsNumber(this.textBox8.Text)) return;
            int sync = Convert.ToInt32(this.textBox8.Text);
            if (!synchron) { sync = anz; }
            Action<object> action = (object obj) =>
            {
                if (all)
                {
                    for (; anz > 0; anz -= sync)
                    {
                        if (Stop_Looping) {  In_Loop = false; break; }

                        if (anz < sync) sync = anz;
                        for (int b = 0; b < Main.Count; b++)
                        {
                            Starte_Step(b, sync);
                        }

                        for (; ; )
                        {
                            if (!synchron)
                            {
                                Thread.Sleep(100);
                            }

                            if (Stop_Looping) { In_Loop = false; break; }

                            bool not = false;
                            for (int b = 0; b < Main.Count; b++)
                            {
                                if (Simulator_anz[b] > 0)
                                {
                                    not = true;
                                    break;
                                }
                            }
                            if (!not) break;
                        }
                    }


                }
                else
                {
                    Starte_Step(sel, anz);
                }
                reload_komm = true;
            };

            ControlStep = new Task(action, "ControlStep");
            ControlStep.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (sel >= Main.Count) return;
            if (ControlStep != null) if (!ControlStep.IsCompleted) return;
            Stop_Looping = false;
            In_Loop = false;
            int anz = Convert.ToInt32(textBox2.Text);
            timer4.Enabled = true;
            Control_Step(anz, checkBox6.Checked, !checkBox7.Checked);

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Form1.ActiveForm != null) Form1.ActiveForm.Text = "Atmel ASM Simulator";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            openFileDialog3.InitialDirectory = Application.StartupPath;
            openFileDialog3.FileName = "";
            openFileDialog3.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            String dat = saveFileDialog1.FileName;
            StreamWriter datei = new StreamWriter(dat);
            if (Main.Count > 0)
            {
                for (int b = 0; b < Main.Count; b++)
                {
                    if (Main[b].Datei.Length >= Application.StartupPath.Length) if (Main[b].Datei.Substring(0, Application.StartupPath.Length) == Application.StartupPath) Main[b].Datei = Main[sel].Datei.Substring(Application.StartupPath.Length + 1, Main[b].Datei.Length - Application.StartupPath.Length - 1);
                    if (Main[b].QuellDatei.Length >= Application.StartupPath.Length) if (Main[b].QuellDatei.Substring(0, Application.StartupPath.Length) == Application.StartupPath) Main[b].QuellDatei = Main[b].QuellDatei.Substring(Application.StartupPath.Length + 1, Main[b].QuellDatei.Length - Application.StartupPath.Length - 1);
                    datei.WriteLine(Main[b].Name);
                    datei.WriteLine(Main[b].Datei);
                    datei.WriteLine(Main[b].QuellDatei);
                }
                datei.WriteLine("[STOPUHREN]");
                for (int b = 0; b < Main.Count; b++)
                {
                    if (Main[b].ZAEHLER.Count == 0) continue;
                    datei.WriteLine("");
                    datei.WriteLine(b.ToString());
                    for (int i = 0; i < Main[b].ZAEHLER.Count; i++)
                    {
                        datei.WriteLine(Main[b].ZAEHLER[i].Name);
                        datei.WriteLine(Main[b].ZAEHLER[i].Begin);
                        datei.WriteLine(Main[b].ZAEHLER[i].Begin_Length);
                        datei.WriteLine(Main[b].ZAEHLER[i].Ende);
                        datei.WriteLine(Main[b].ZAEHLER[i].Ende_Length);
                    }
                }
                datei.WriteLine("[SZENARIEN]");
                for (int i = 0; i < global_scenery.Count; i++)
                {
                    datei.WriteLine((0).ToString());
                    datei.WriteLine(global_scenery[i].Name);
                    datei.WriteLine(global_scenery[i].Events.Count);
                    for (int c = 0; c < global_scenery[i].Events.Count; c++)
                    {
                        datei.WriteLine(global_scenery[i].Events[c].Ziel_ID);
                        datei.WriteLine(global_scenery[i].Events[c].Time);
                        datei.WriteLine(global_scenery[i].Events[c].Random_Time);
                        datei.WriteLine(global_scenery[i].Events[c].Random_Time_From);
                        datei.WriteLine(global_scenery[i].Events[c].Random_Time_To);
                        datei.WriteLine(global_scenery[i].Events[c].Param1);
                        datei.WriteLine(global_scenery[i].Events[c].Param1_Random);
                        datei.WriteLine(global_scenery[i].Events[c].Param1_From);
                        datei.WriteLine(global_scenery[i].Events[c].Param1_To);
                        datei.WriteLine(global_scenery[i].Events[c].Param2);
                        datei.WriteLine(global_scenery[i].Events[c].Param2_Random);
                        datei.WriteLine(global_scenery[i].Events[c].Param2_From);
                        datei.WriteLine(global_scenery[i].Events[c].Param2_To);
                        datei.WriteLine(global_scenery[i].Events[c].Param3);
                        datei.WriteLine(global_scenery[i].Events[c].Param2_Random);
                        datei.WriteLine(global_scenery[i].Events[c].Param3_From);
                        datei.WriteLine(global_scenery[i].Events[c].Param3_To);
                        datei.WriteLine(global_scenery[i].Events[c].Typ);
                    }
                }

                for (int b = 0; b < Main.Count; b++)
                {
                    for (int i = 0; i < Main[b].scenery.Count; i++)
                    {
                        datei.WriteLine((b + 1).ToString());
                        datei.WriteLine(Main[b].scenery[i].Name);
                        datei.WriteLine(Main[b].scenery[i].Events.Count);
                        for (int c = 0; c < Main[b].scenery[i].Events.Count; c++)
                        {
                            datei.WriteLine(Main[b].scenery[i].Events[c].Ziel_ID);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Time);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Random_Time);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Random_Time_From);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Random_Time_To);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param1);
                            datei.WriteLine(global_scenery[i].Events[c].Param1_Random);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param1_From);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param1_To);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param2);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param2_Random);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param2_From);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param2_To);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param3);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param3_Random);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param3_From);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param3_To);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Typ);
                        }
                    }
                }

                datei.WriteLine("[ANWENDUNGEN]");
                for (int i = 0; i < Minianwendungen.Count; i++)
                {
                    datei.WriteLine(Minianwendungen[i]);
                    datei.WriteLine(Minianwendungen_Datei[i]);
                }
            }
            else
            {
                // datei.WriteLine("");
                //datei.WriteLine("");
                // datei.WriteLine("");
            }
            Projekt = dat;
            label13.Text = Path.GetFileName(saveFileDialog1.FileName);
            label13.Show();
            datei.Close();
        }

        private void openFileDialog3_FileOk(object sender, CancelEventArgs e)
        {
            Start_Progress(100);
            listBox1.Items.Clear();
            Projekt = "";
            sel = -1;
            String dat = openFileDialog3.FileName;
            StreamReader datei = new StreamReader(dat);
            List<String> list = new List<String>();
            for (; datei.Peek() != -1; ) list.Add(datei.ReadLine());
            int i = 0;
            for (; i < list.Count; i += 3)
            {
                if (list[i] == "[STOPUHREN]") break;
                if (list[i] == "[SZENARIEN]") break;
                if (list[i] == "[ANWENDUNGEN]") break;
                openFileDialog2.FileName = list[i + 1];
                openFileDialog1.FileName = list[i + 2];
                sel = i;
                button9_Click(null, null);
                if (openFileDialog2.FileName != "") openFileDialog2_FileOk(null, null);
                if (openFileDialog1.FileName != "") openFileDialog1_FileOk(null, null);
                if (openFileDialog2.FileName != "") Main[sel].Name = list[i];
                listBox1.Items[sel] = Main[sel].Name;
                comboBox5.Items[sel] = Main[sel].Name;
                comboBox4.Items[sel] = Main[sel].Name;
                comboBox16.Items[sel] = Main[sel].Name;
            }

            if (i < list.Count)
                if (list[i] == "[STOPUHREN]")
                {
                    i++;
                    for (; i < list.Count; i++)
                    {
                        if (list[i] == "[SZENARIEN]") break;
                        if (list[i] == "[ANWENDUNGEN]") break;
                        if (list[i] == "") { sel = Convert.ToInt32(list[i + 1]); i++; continue; }

                        // Startpunkt
                        int ib = Convert.ToInt32(list[i + 1]);
                        if (ib != -1)
                        {
                            int line = Main[sel].Program[0][ib].Original;

                            int len = Main[sel].Quelltext[0][line].Length;
                            int start = Main[sel].Text.GetFirstCharIndexFromLine(line);
                            Main[sel].AddZaehler(list[i], ib);

                            // Endpunkt
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

                            }
                        }

                        i += 4;
                    }
                }

            if (i < list.Count)
                if (list[i] == "[SZENARIEN]")
                {
                    i++;
                    for (; i < list.Count; )
                    {
                        if (list[i] == "[ANWENDUNGEN]") break;
                        if (i + 20 >= list.Count) break;
                        int owner = Convert.ToInt32(list[i]);
                        if (owner == 0)
                        {
                            global_scenery.Add(new SZENARIEN(null));
                            int id = global_scenery.Count - 1;
                            global_scenery[id].Name = list[i + 1];
                            int anz = Convert.ToInt32(list[i + 2]);
                            i += 3;
                            for (int c = 0; c < anz; c++, i += 18)
                            {
                                global_scenery[id].AddLine();
                                int id2 = global_scenery[id].Events.Count - 1;
                                global_scenery[id].Events[id2].Ziel_ID = Convert.ToInt32(list[i]);
                                global_scenery[id].Events[id2].Time = Convert.ToInt32(list[i + 1]);
                                global_scenery[id].Events[id2].Random_Time = Convert.ToBoolean(list[i + 2]);
                                global_scenery[id].Events[id2].Random_Time_From = Convert.ToInt32(list[i + 3]);
                                global_scenery[id].Events[id2].Random_Time_To = Convert.ToInt32(list[i + 4]);
                                global_scenery[id].Events[id2].Param1 = Convert.ToInt32(list[i + 5]);
                                global_scenery[id].Events[id2].Param1_Random = Convert.ToBoolean(list[i + 6]);
                                global_scenery[id].Events[id2].Param1_From = Convert.ToInt32(list[i + 7]);
                                global_scenery[id].Events[id2].Param1_To = Convert.ToInt32(list[i + 8]);
                                global_scenery[id].Events[id2].Param2 = Convert.ToInt32(list[i + 9]);
                                global_scenery[id].Events[id2].Param2_Random = Convert.ToBoolean(list[i + 10]);
                                global_scenery[id].Events[id2].Param2_From = Convert.ToInt32(list[i + 11]);
                                global_scenery[id].Events[id2].Param2_To = Convert.ToInt32(list[i + 12]);
                                global_scenery[id].Events[id2].Param3 = Convert.ToInt32(list[i + 13]);
                                global_scenery[id].Events[id2].Param3_Random = Convert.ToBoolean(list[i + 14]);
                                global_scenery[id].Events[id2].Param3_From = Convert.ToInt32(list[i + 15]);
                                global_scenery[id].Events[id2].Param3_To = Convert.ToInt32(list[i + 16]);
                                global_scenery[id].Events[id2].Typ = Convert.ToInt32(list[i + 17]);
                            }
                        }
                        else
                        {
                            owner--;
                            Main[owner].scenery.Add(new SZENARIEN(null));
                            int id = Main[owner].scenery.Count - 1;
                            Main[owner].scenery[id].Name = list[i + 1];
                            int anz = Convert.ToInt32(list[i + 2]);
                            i += 3;
                            for (int c = 0; c < anz; c++, i += 18)
                            {
                                Main[owner].scenery[id].AddLine();
                                int id2 = Main[owner].scenery[id].Events.Count - 1;
                                Main[owner].scenery[id].Events[id2].Ziel_ID = Convert.ToInt32(list[i]);
                                Main[owner].scenery[id].Events[id2].Time = Convert.ToInt32(list[i + 1]);
                                Main[owner].scenery[id].Events[id2].Random_Time = Convert.ToBoolean(list[i + 2]);
                                Main[owner].scenery[id].Events[id2].Random_Time_From = Convert.ToInt32(list[i + 3]);
                                Main[owner].scenery[id].Events[id2].Random_Time_To = Convert.ToInt32(list[i + 4]);
                                Main[owner].scenery[id].Events[id2].Param1 = Convert.ToInt32(list[i + 5]);
                                Main[owner].scenery[id].Events[id2].Param1_Random = Convert.ToBoolean(list[i + 6]);
                                Main[owner].scenery[id].Events[id2].Param1_From = Convert.ToInt32(list[i + 7]);
                                Main[owner].scenery[id].Events[id2].Param1_To = Convert.ToInt32(list[i + 8]);
                                Main[owner].scenery[id].Events[id2].Param2 = Convert.ToInt32(list[i + 9]);
                                Main[owner].scenery[id].Events[id2].Param2_Random = Convert.ToBoolean(list[i + 10]);
                                Main[owner].scenery[id].Events[id2].Param2_From = Convert.ToInt32(list[i + 11]);
                                Main[owner].scenery[id].Events[id2].Param2_To = Convert.ToInt32(list[i + 12]);
                                Main[owner].scenery[id].Events[id2].Param3 = Convert.ToInt32(list[i + 13]);
                                Main[owner].scenery[id].Events[id2].Param3_Random = Convert.ToBoolean(list[i + 14]);
                                Main[owner].scenery[id].Events[id2].Param3_From = Convert.ToInt32(list[i + 15]);
                                Main[owner].scenery[id].Events[id2].Param3_To = Convert.ToInt32(list[i + 16]);
                                Main[owner].scenery[id].Events[id2].Typ = Convert.ToInt32(list[i + 17]);
                            }
                        }

                    }
                }

            if (i < list.Count)
                if (list[i] == "[ANWENDUNGEN]")
                {
                    i++;
                    for (; i < list.Count; i += 2)
                    {
                        Minianwendungen.Add(list[i]);
                        Minianwendungen_Datei.Add(list[i + 1]);
                        dataGridView12.Rows.Add();
                        int id = dataGridView12.Rows.Count - 1;
                        dataGridView12.Rows[id].HeaderCell.Value = Minianwendungen[id];
                        dataGridView12.Rows[id].Cells[0].Value = Minianwendungen_Datei[id];
                    }
                }

            sel = Main.Count - 1;
            selected = -1;

            // Begin Malen
            for (int MainID = 0; MainID < Main.Count; MainID++)
            {
                RichTextBox TextBox = Main[MainID].Text;
                int start = 0;
                int old_line = 0;
                for (int c = 0; c < Main[MainID].Program[0].Count; c++)
                {
                    int Line = Main[MainID].Program[0][c].Original;
                    int real = c;
                    int len = 0;

                    for (int t = old_line; t < Line; t++)
                    {
                        start += TextBox.Lines[t].Length + 1;
                    }
                    old_line = Line;

                    len = 0;
                    for (int t = 0; t < Main[MainID].Program[0][real].Zaehler_Start.Count; t++) len += Main[MainID].ZAEHLER[Main[MainID].Program[0][real].Zaehler_Start[t]].Begin_Length;
                    if (len == 0) goto ende2;
                    TextBox.Select(start, len);
                    TextBox.SelectionBackColor = richTextBox1.BackColor;
                    TextBox.SelectionColor = Color.Olive;

                ende2:
                    len = 0;
                    for (int t = 0; t < Main[MainID].Program[0][real].Zaehler_Ende.Count; t++) len += Main[MainID].ZAEHLER[Main[MainID].Program[0][real].Zaehler_Ende[t]].Ende_Length;
                    if (len == 0) continue;
                    TextBox.Select(start + TextBox.Lines[Line].Length - len, len);
                    TextBox.SelectionBackColor = richTextBox1.BackColor;
                    TextBox.SelectionColor = Color.Navy;
                    TextBox.Select(start, 0);
                }
            }
            // Ende Malen
            change_selected();

            label24.Text = Path.GetFileName(openFileDialog3.FileName);
            label24.Show();
            Projekt = dat; // legt Projektnamen fest
            if (sel >= 0)
            {
                if (comboBox5.SelectedIndex < 0) comboBox5.SelectedIndex = Main[sel].ID;
                if (comboBox4.SelectedIndex < 0) comboBox4.SelectedIndex = Main[sel].ID;
                if (comboBox16.SelectedIndex < 0) comboBox16.SelectedIndex = Main[sel].ID;
                listBox1.SelectedIndex = Main[sel].ID;
                sel = listBox1.SelectedIndex;
                reload_geraete_table();
            }
            datei.Close();

            // Lade Minianwendungen
            for (int d = 0; d < Main.Count; d++)
                for (int b = 0; b < Main[d].scenery.Count; b++)
                    for (int c = 0; c < Main[d].scenery[b].Events.Count; c++)
                    {
                        if (Main[d].scenery[b].Events[c].Typ == 5 && Main[d].scenery[b].Events[c].Param1 + 1 >= Main[d].Program.Count)
                        {
                            // muss neu eingelesen werden
                            int anwendung = Main[d].scenery[b].Events[c].Param1;
                            String dat2 = Minianwendungen_Datei[anwendung];
                            datei = new StreamReader(dat2);
                            List<String> data = new List<String>();
                            while (datei.Peek() != -1)
                            {
                                data.Add(datei.ReadLine());
                            }
                            datei.Close();
                            Main[d].Selected_Software = anwendung + 1;
                            Main[d].Load_Content(data, false);
                            Main[d].Selected_Software = 0;
                        }
                    }

            Stop_Progress();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (Projekt == "")
            {
                saveFileDialog1.InitialDirectory = Application.StartupPath;
                saveFileDialog1.FileName = "";
                saveFileDialog1.ShowDialog();
            }
            else
            {
                String dat = Projekt;
                StreamWriter datei = new StreamWriter(dat);
                for (int b = 0; b < Main.Count; b++)
                {
                    if (Main[b].Datei.Length >= Application.StartupPath.Length) if (Main[b].Datei.Substring(0, Application.StartupPath.Length) == Application.StartupPath) Main[b].Datei = Main[sel].Datei.Substring(Application.StartupPath.Length + 1, Main[b].Datei.Length - Application.StartupPath.Length - 1);
                    if (Main[b].QuellDatei.Length >= Application.StartupPath.Length) if (Main[b].QuellDatei.Substring(0, Application.StartupPath.Length) == Application.StartupPath) Main[b].QuellDatei = Main[b].QuellDatei.Substring(Application.StartupPath.Length + 1, Main[b].QuellDatei.Length - Application.StartupPath.Length - 1);
                    datei.WriteLine(Main[b].Name);
                    datei.WriteLine(Main[b].Datei);
                    datei.WriteLine(Main[b].QuellDatei);
                }
                datei.WriteLine("[STOPUHREN]");
                for (int b = 0; b < Main.Count; b++)
                {
                    if (Main[b].ZAEHLER.Count == 0) continue;
                    datei.WriteLine("");
                    datei.WriteLine(b.ToString());
                    for (int i = 0; i < Main[b].ZAEHLER.Count; i++)
                    {
                        datei.WriteLine(Main[b].ZAEHLER[i].Name);
                        datei.WriteLine(Main[b].ZAEHLER[i].Begin);
                        datei.WriteLine(Main[b].ZAEHLER[i].Begin_Length);
                        datei.WriteLine(Main[b].ZAEHLER[i].Ende);
                        datei.WriteLine(Main[b].ZAEHLER[i].Ende_Length);
                    }
                }
                datei.WriteLine("[SZENARIEN]");
                for (int i = 0; i < global_scenery.Count; i++)
                {
                    datei.WriteLine((0).ToString());
                    datei.WriteLine(global_scenery[i].Name);
                    datei.WriteLine(global_scenery[i].Events.Count);
                    for (int c = 0; c < global_scenery[i].Events.Count; c++)
                    {
                        datei.WriteLine(global_scenery[i].Events[c].Ziel_ID);
                        datei.WriteLine(global_scenery[i].Events[c].Time);
                        datei.WriteLine(global_scenery[i].Events[c].Random_Time);
                        datei.WriteLine(global_scenery[i].Events[c].Random_Time_From);
                        datei.WriteLine(global_scenery[i].Events[c].Random_Time_To);
                        datei.WriteLine(global_scenery[i].Events[c].Param1);
                        datei.WriteLine(global_scenery[i].Events[c].Param1_Random);
                        datei.WriteLine(global_scenery[i].Events[c].Param1_From);
                        datei.WriteLine(global_scenery[i].Events[c].Param1_To);
                        datei.WriteLine(global_scenery[i].Events[c].Param2);
                        datei.WriteLine(global_scenery[i].Events[c].Param2_Random);
                        datei.WriteLine(global_scenery[i].Events[c].Param2_From);
                        datei.WriteLine(global_scenery[i].Events[c].Param2_To);
                        datei.WriteLine(global_scenery[i].Events[c].Param3);
                        datei.WriteLine(global_scenery[i].Events[c].Param2_Random);
                        datei.WriteLine(global_scenery[i].Events[c].Param3_From);
                        datei.WriteLine(global_scenery[i].Events[c].Param3_To);
                        datei.WriteLine(global_scenery[i].Events[c].Typ);
                    }
                }

                for (int b = 0; b < Main.Count; b++)
                {
                    for (int i = 0; i < Main[b].scenery.Count; i++)
                    {
                        datei.WriteLine((b + 1).ToString());
                        datei.WriteLine(Main[b].scenery[i].Name);
                        datei.WriteLine(Main[b].scenery[i].Events.Count);
                        for (int c = 0; c < Main[b].scenery[i].Events.Count; c++)
                        {
                            datei.WriteLine(Main[b].scenery[i].Events[c].Ziel_ID);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Time);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Random_Time);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Random_Time_From);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Random_Time_To);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param1);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param1_Random);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param1_From);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param1_To);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param2);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param2_Random);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param2_From);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param2_To);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param3);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param3_Random);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param3_From);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Param3_To);
                            datei.WriteLine(Main[b].scenery[i].Events[c].Typ);
                        }
                    }
                }

                datei.WriteLine("[ANWENDUNGEN]");
                for (int i = 0; i < Minianwendungen.Count; i++)
                {
                    datei.WriteLine(Minianwendungen[i]);
                    datei.WriteLine(Minianwendungen_Datei[i]);
                }

                datei.Close();
            }
        }

        private void textBox3_KeyUp(object sender, KeyEventArgs e)
        {
            Main[sel].Name = textBox3.Text;
            int id = listBox1.SelectedIndex;
            listBox1.Items[id] = Main[sel].Name;
            comboBox4.Items[id] = Main[sel].Name;
            comboBox5.Items[id] = Main[sel].Name;
            comboBox16.Items[id] = Main[sel].Name;
        }

        public void reload_geraete_table()
        {
            if (sel >= Main.Count() || sel<0)
            {
                textBox1.Hide();
                textBox3.Hide();
                label12.Hide();
                label13.Hide();
                label24.Hide();
                label2.Hide();
                label22.Hide();
                checkBox18.Hide();
                checkBox17.Hide();
                checkBox16.Hide();
                checkBox15.Hide();
                checkBox14.Hide();
                return;
            }

            checkBox18.Show();
            checkBox17.Show();
            checkBox16.Show();
            checkBox15.Show();
            checkBox14.Show();
            textBox1.Show();
            textBox3.Show();
            label12.Show();
            label13.Show();
            if (Projekt != "") label24.Show();
            label2.Show();
            label22.Show();
            int id = listBox1.SelectedIndex;
            textBox3.Text = Main[sel].Name;
            label13.Text = Path.GetFileName(Main[sel].QuellDatei);
            label12.Text = Path.GetFileName(Main[sel].Datei);
            int pos = 0;
            dataGridView3.Rows.Clear();
            for (int i = 0; i < Main[sel].Ports.Count; i++)
            {
                String a = Main[sel].Ports[i].Bezeichnung;
                if (a.Length >= 4)
                    if (a.Substring(0, 4) == "PORT")
                    {
                        String q = a.Substring(4, a.Length - 4);
                        int anz = Main[sel].Ports[i].Anz;
                        if (dataGridView3.Rows.Count < pos + anz) dataGridView3.Rows.Add(pos + anz - dataGridView3.Rows.Count);
                        for (int c = 0; c < anz; c++, pos++)
                        {
                            dataGridView3.Rows[pos].HeaderCell.Value = q + "[" + c + "]";
                            String add = Main[sel].Ports[i].Typ[c][0];
                            for (int d = 1; d < Main[sel].Ports[i].Typ[c].Count(); d++) add = add + "," + Main[sel].Ports[i].Typ[c][d];
                            dataGridView3.Rows[pos].Cells[0].Value = add;
                        }
                    }
            }

            checkBox18.Checked = Main[sel].EEPROM_AKTIV;
            checkBox17.Checked = Main[sel].Timer_AKTIV;
            checkBox16.Checked = Main[sel].USART_AKTIV;
            checkBox15.Checked = Main[sel].Watchdog_AKTIV;
            checkBox14.Checked = Main[sel].Szenarien_AKTIV;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            progressBar1.Value = Prog;
        }

        private void dataGridView4_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            String[] data = { "INT", "BIN", "HEX", "ASC" };
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                if (dataGridView4.Rows[e.RowIndex].Cells[1].Value == null)
                {
                    dataGridView4.Rows[e.RowIndex].Cells[1].Value = data[1];
                    reload_Ports();
                    return;
                }

                String dat = dataGridView4.Rows[e.RowIndex].Cells[1].Value.ToString();
                for (int i = 0; i < data.Count(); i++)
                {
                    if (data[i] == dat)
                    {
                        dataGridView4.Rows[e.RowIndex].Cells[1].Value = data[(i + 1) % data.Count()];
                        reload_Ports();
                        break;
                    }
                }
            }
            else
                if (e.ColumnIndex == 0 && e.RowIndex >= 0)
                {
                    // Eintragen
                    Eingabe.Show();
                }
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            textBox5.Text = Help.free(textBox5.Text);
            label26.Text = Help.Arithmetic(new Atmega(0, 0), textBox5.Text, true);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            toolStripTextBox1.Text = "";
            toolStripComboBox1.Items.Clear();
            toolStripComboBox2.Items.Clear();
            for (int i = 0; i < Main[sel].ZAEHLER.Count; i++)
            {
                if (Main[sel].ZAEHLER[i].Begin > -1)
                {
                    toolStripComboBox2.Items.Add((object)"S-" + Main[sel].ZAEHLER[i].Name);
                }
                if (Main[sel].ZAEHLER[i].Ende > -1)
                {
                    toolStripComboBox2.Items.Add((object)"E-" + Main[sel].ZAEHLER[i].Name);
                }
                if (Main[sel].ZAEHLER[i].Begin > -1 && Main[sel].ZAEHLER[i].Ende == -1)
                {
                    toolStripComboBox1.Items.Add((object)Main[sel].ZAEHLER[i].Name);
                }
            }

            if (toolStripComboBox1.Items.Count > 0)
            {
                toolStripComboBox1.Visible = true;
                toolStripSeparator1.Visible = true;
                toolStripComboBox1.Text = "Neue Endmarkierung";
            }
            else
            {
                toolStripComboBox1.Visible = false;
                toolStripSeparator1.Visible = false;
            }

            if (toolStripComboBox2.Items.Count > 0)
            {
                toolStripComboBox2.Visible = true;
                toolStripSeparator2.Visible = true;
                toolStripComboBox2.Text = "Markierung löschen";
            }
            else
            {
                toolStripComboBox2.Visible = false;
                toolStripSeparator2.Visible = false;
            }
        }

        public bool isProgram(int id, int MainID)
        {
            for (int i = 0; i < Main[MainID].Program[0].Count; i++)
            {
                if (Main[MainID].Program[0][i].Original == id) return true;
            }
            return false;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (toolStripTextBox1.Text == "") return;
            int start = richTextBox1.SelectionStart;
            int line = -1;
            line = richTextBox1.GetLineFromCharIndex(start);

            if (line != -1 && isProgram(line, sel))
            {
                int ib = -1;
                for (int i = 0; i < Main[sel].Program[0].Count; i++)
                {
                    if (Main[sel].Program[0][i].Original == line) { ib = i; break; }
                }

                if (ib == -1) return;
                int len = Main[sel].Quelltext[0][line].Length;
                start = richTextBox1.GetFirstCharIndexFromLine(line);
                richTextBox1.Select(start, len);

                if (Main[sel].AddZaehler(toolStripTextBox1.Text, ib))
                {
                    richTextBox1.SelectedText = Main[sel].Quelltext[0][line];
                    Draw_Zaehler_In_Line(richTextBox1, sel, line);
                    Draw_Zaehler_In_Line(Main[sel].Text, sel, line);
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_CursorChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            // label27.Text = richTextBox1.SelectionStart.ToString();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (Help.IsNumber(textBox1.Text))
            {
                Main[sel].Frequenz = Convert.ToInt32(textBox1.Text);
            }
        }

        int[] Original_Pos = new int[6];
        bool[] Original_Show = { true, true, true, true, true, true };

        public void Anordnen()
        {
            int aktual = 0;
            if (Original_Show[0])
            {
                dataGridView1.Left = Original_Pos[aktual];
                label11.Left = Original_Pos[aktual];
                aktual++;
            }
            if (Original_Show[1])
            {
                dataGridView2.Left = Original_Pos[aktual];
                label1.Left = Original_Pos[aktual];
                aktual++;
            }
            if (Original_Show[2])
            {
                dataGridView4.Left = Original_Pos[aktual];
                label25.Left = Original_Pos[aktual];
                aktual++;
            }
            if (Original_Show[3])
            {
                dataGridView5.Left = Original_Pos[aktual];
                label28.Left = Original_Pos[aktual];
                aktual++;
            }
            if (Original_Show[4])
            {
                dataGridView9.Left = Original_Pos[aktual];
                label14.Left = Original_Pos[aktual];
                aktual++;
            }
            if (Original_Show[5])
            {
                richTextBox1.Left = Original_Pos[aktual];
                richTextBox1.Width = tabPage2.Width - richTextBox1.Left;
                aktual++;
            }


        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                dataGridView5.Show();
                label28.Show();
                Original_Show[3] = true;
                reload_eeprom();
            }
            else
            {
                dataGridView5.Hide();
                label28.Hide();
                Original_Show[3] = false;
            }
            Anordnen();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                dataGridView4.Show();
                label25.Show();
                Original_Show[2] = true;
                reload_Ports();
            }
            else
            {
                dataGridView4.Hide();
                label25.Hide();
                Original_Show[2] = false;
            }
            Anordnen();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                dataGridView2.Show();
                label1.Show();
                Original_Show[1] = true;
                reload_SRAM();
            }
            else
            {
                dataGridView2.Hide();
                label1.Hide();
                Original_Show[1] = false;
            }
            Anordnen();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                dataGridView1.Show();
                label11.Show();
                Original_Show[0] = true;
                reload_table();
            }
            else
            {
                dataGridView1.Hide();
                label11.Hide();
                Original_Show[0] = false;
            }
            Anordnen();

        }

        private void dataGridView5_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            String[] data = { "INT", "BIN", "HEX", "ASC" };
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                if (dataGridView5.Rows[e.RowIndex].Cells[1].Value == null)
                {
                    dataGridView5.Rows[e.RowIndex].Cells[1].Value = data[1];
                    reload_eeprom();
                    return;
                }

                String dat = dataGridView5.Rows[e.RowIndex].Cells[1].Value.ToString();
                for (int i = 0; i < data.Count(); i++)
                {
                    if (data[i] == dat)
                    {
                        dataGridView5.Rows[e.RowIndex].Cells[1].Value = data[(i + 1) % data.Count()];
                        reload_eeprom();
                        break;
                    }
                }
            }
            else
                if (e.ColumnIndex == 0 && e.RowIndex >= 0)
                {
                    // Eintragen
                    Eingabe.Show();
                }
        }

        private void tabPage4_Enter(object sender, EventArgs e)
        {
            if (Main.Count == 0) return;
            if (sender != null)
            {
                dataGridView6.Rows.Clear();

                int summ = 0;
                for (int b = 0; b < Main.Count; b++) summ += Main[b].ZAEHLER.Count();
                int a = summ - dataGridView6.Rows.Count;
                if (a > 0) dataGridView6.Rows.Add(a);
            }

            int old = 0;
            for (int b = 0; b < Main.Count; b++)
                for (int i = 0; i < Main[b].ZAEHLER.Count(); i++, old++)
                {
                    dataGridView6.Rows[old].HeaderCell.Value = Main[b].Name + ": " + Main[b].ZAEHLER[i].Name;
                    dataGridView6.Rows[old].Cells[0].Value = Main[b].ZAEHLER[i].Anzahl.ToString();
                    dataGridView6.Rows[old].Cells[1].Value = Main[b].ZAEHLER[i].GetDurchschnitt().ToString();
                    dataGridView6.Rows[old].Cells[2].Value = Main[b].ZAEHLER[i].GetMin().ToString();
                    dataGridView6.Rows[old].Cells[3].Value = Main[b].ZAEHLER[i].GetMax().ToString();
                }

            if (sender != null)
            {
                dataGridView8.Rows.Clear();
                int a = (Main.Count + Minianwendungen.Count) - dataGridView8.Rows.Count;
                if (a > 0) dataGridView8.Rows.Add(a);
            }

            double fact = 1000L * 1000L * 1000L / Stopwatch.Frequency;
            for (int b = 0; b < Main.Count; b++)
            {
                dataGridView8.Rows[b].HeaderCell.Value = Main[b].Name;
                dataGridView8.Rows[b].Cells[0].Value = Main[b].Time.ToString();
                dataGridView8.Rows[b].Cells[1].Value = Main[b].EEPROM.Anz_Read.ToString();
                dataGridView8.Rows[b].Cells[2].Value = Main[b].EEPROM.Anz_Write.ToString();
                if (Main[b].Time == 0)
                {
                    dataGridView8.Rows[b].Cells[3].Value = "-";
                }
                else
                    dataGridView8.Rows[b].Cells[3].Value = Math.Round((double)(Main[b].hauptWatch.ElapsedTicks * fact) / Main[b].Time, 4) + " ns";
            }
            int dist = Main.Count;
            for (int b = 0; b < Minianwendungen.Count; b++)
            {
                dataGridView8.Rows[dist + b].HeaderCell.Value = Minianwendungen[b];
                long summe = 0;
                int summe2 = 0;
                for (int c = 0; c < Main.Count; c++)
                    if (Main[c].Watch_Minianwendungen.Count > b)
                    {
                        summe += Main[c].Watch_Minianwendungen[b].ElapsedTicks;
                        summe2 += Main[c].Watch_Takte[b];
                    }

                dataGridView8.Rows[dist + b].Cells[0].Value = summe2.ToString();
                if (summe2 > 0)
                {
                    dataGridView8.Rows[dist + b].Cells[3].Value = Math.Round((double)(summe * fact) / summe2, 4) + " ns";
                }
                else
                    dataGridView8.Rows[dist + b].Cells[3].Value = "-";

                dataGridView8.Rows[dist + b].Cells[1].Value = "-";
                dataGridView8.Rows[dist + b].Cells[2].Value = "-";

            }

            button19_Click(null, null);
        }

        bool reload_komm = false;
        bool Stop_Looping = false;
        bool In_Loop = false;

        private void button13_Click(object sender, EventArgs e)
        {
            if (!Help.IsNumber(textBox7.Text) || !Help.IsNumber(textBox2.Text)) return;
            timer6.Enabled = true;
            if (button13.Text == "Starte Alle" && button16.Text == "Loop")
            {
                button13.Text = "Stop Alle";
                button16.Text = "Stop Loop";
                timer3.Enabled = true;
                // timer4.Enabled = true;
                In_Loop = true;
                Stop_Looping = false;
                int Schritte = sender == button13 ? Convert.ToInt32(textBox7.Text) : Convert.ToInt32(textBox2.Text);
                Schritte = 1000000000;
                for (int b = 0; b < Main.Count; b++) Starte_Step(b, 0);

                Action<object> action2 = (object obj) =>
                 {
                     for (; ; )
                     {
                         if (Stop_Looping) { In_Loop = false; reload_komm = true; break; }
                         // Loop(Schritte);
                         Control_Step(Schritte, checkBox6.Checked, !checkBox7.Checked);
                         ControlStep.Wait();
                         // reload_komm = true;
                         Thread.Sleep(750);
                         //reload_komm = true;
                     }
                 };
                Looping = new Task(action2, "loop");
                Looping.Start();

            }
            else
            {
                button16.Text = "Loop";
                button13.Text = "Starte Alle";
                Stop_Looping = true;
                In_Loop = false;
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (reload_komm)
            {
                reload_komm = false;
                if (sel >= Main.Count) return;
                if (tabControl1.SelectedIndex == 3) { tabPage4_Enter(null, null); }
                if (tabControl1.SelectedIndex == 2) tabPage3_Enter(null, null);
                if (tabControl1.SelectedIndex == 1)
                {
                    reload_table();
                    reload_quelltext();
                    reload_SRAM();
                    reload_Ports();
                    reload_eeprom();
                    reload_flags();
                    reload_PM();
                    textBox18.Text = Main[sel].Time.ToString();
                }
            }
        }

        private void tabPage3_Enter(object sender, EventArgs e)
        {
            if (Main == null) return;
            if (sel == -1) return;
            if (sel >= Main.Count) return;
            int pos = 0;
            for (int i = 0; i < Main[sel].Ports.Count; i++)
            {
                String a = Main[sel].Ports[i].Bezeichnung;
                if (a.Length >= 4)
                    if (a.Substring(0, 4) == "PORT")
                    {
                        String q = a.Substring(4, a.Length - 4);
                        int anz = Main[sel].Ports[i].Anz;
                        if (dataGridView7.Rows.Count < pos + anz) dataGridView7.Rows.Add(pos + anz - dataGridView7.Rows.Count);
                        for (int c = 0; c < anz; c++, pos++)
                        {
                            dataGridView7.Rows[pos].HeaderCell.Value = q + "[" + c + "]";
                            //for (int d = 0; d < Main.Ports[i].Typ[c].Count(); d++)
                            //{
                            if (Main[sel].GetBitIOPort("DDR" + q, (Byte)c))
                            { // Ausgang
                                dataGridView7.Rows[pos].Cells[0].Value = "-";
                                dataGridView7.Rows[pos].Cells[0].Style.ForeColor = Color.Black;
                                dataGridView7.Rows[pos].Cells[1].Value = Main[sel].GetBitIOPort("PORT" + q, (Byte)c) ? "O" : "X";
                                dataGridView7.Rows[pos].Cells[1].Style.ForeColor = Main[sel].GetBitIOPort("PORT" + q, (Byte)c) ? Color.Green : Color.Red;
                            }
                            else
                                if (!Main[sel].GetBitIOPort("DDR" + q, (Byte)c))
                                {// Eingang
                                    dataGridView7.Rows[pos].Cells[1].Value = "-";
                                    dataGridView7.Rows[pos].Cells[1].Style.ForeColor = Color.Black;
                                    dataGridView7.Rows[pos].Cells[0].Value = Main[sel].GetBitIOPort("PIN" + q, (Byte)c) ? "O" : "X";
                                    dataGridView7.Rows[pos].Cells[0].Style.ForeColor = Main[sel].GetBitIOPort("PIN" + q, (Byte)c) ? Color.Green : Color.Red;
                                    //new Font(FontFamily.GenericMonospace, 5.0f);
                                }
                            //}
                        }
                    }
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            tabPage3_Enter(null, null);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            tabPage4_Enter(null, null);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add("neu");
            comboBox5.Items.Add("neu");
            comboBox4.Items.Add("neu");
            comboBox16.Items.Add("neu");
            textBox3.Text = "neu";
            comboBox5.SelectedIndex = listBox1.Items.Count - 1;
            comboBox4.SelectedIndex = listBox1.Items.Count - 1;
            comboBox16.SelectedIndex = listBox1.Items.Count - 1;
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            sel = listBox1.SelectedIndex;
            reload_geraete_table();
        }

        private void dataGridView2_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            String[] data = { "INT", "BIN", "HEX", "ASC" };
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                if (dataGridView2.Rows[e.RowIndex].Cells[1].Value == null)
                {
                    dataGridView2.Rows[e.RowIndex].Cells[1].Value = data[1];
                    reload_SRAM();
                    return;
                }

                String dat = dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString();
                for (int i = 0; i < data.Count(); i++)
                {
                    if (data[i] == dat)
                    {
                        dataGridView2.Rows[e.RowIndex].Cells[1].Value = data[(i + 1) % data.Count()];
                        reload_SRAM();
                        break;
                    }
                }
            }
            else
                if (e.ColumnIndex == 0 && e.RowIndex >= 0)
                {
                    // Eintragen
                    Eingabe.Show();
                }
        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            sel = listBox1.SelectedIndex;
            comboBox5.SelectedIndex = sel;
            comboBox4.SelectedIndex = sel;
            comboBox16.SelectedIndex = sel;
            if (Main.Count >= sel) reload_geraete_table();
        }

        private void comboBox5_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Enter(object sender, EventArgs e)
        {
            if (timer3.Enabled == false) timer3.Enabled = true;
            //if (timer4.Enabled == false) timer4.Enabled = true;
            //button5_Click(null, null);
            reload_komm = true;
        }

        private void button11_Click_1(object sender, EventArgs e)
        {
            // if (reload_komm == true) return;
            sel--;

            listBox1.SelectedIndex = sel;
            comboBox5.SelectedIndex = sel;
            comboBox4.SelectedIndex = sel;
            comboBox16.SelectedIndex = sel;
            change_selected();
            reload_komm = true;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            //if (reload_komm == true) return;
            sel++;

            listBox1.SelectedIndex = sel;
            comboBox5.SelectedIndex = sel;
            comboBox4.SelectedIndex = sel;
            comboBox16.SelectedIndex = sel;
            change_selected();
            reload_komm = true;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                dataGridView9.Show();
                label14.Show();
                Original_Show[4] = true;
                reload_PM();
            }
            else
            {
                dataGridView9.Hide();
                label14.Hide();
                Original_Show[4] = false;
            }
            Anordnen();
        }

        private void dataGridView9_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            String[] data = { "INT", "BIN", "HEX", "ASC" };
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {

                for (int b = 0; b < dataGridView9.SelectedRows.Count; b++)
                {
                    int ID = dataGridView9.SelectedRows[b].Index;

                    if (dataGridView9.Rows[ID].Cells[1].Value == null)
                    {
                        dataGridView9.Rows[ID].Cells[1].Value = data[1];
                        reload_PM();
                        return;
                    }

                    String dat = dataGridView9.Rows[ID].Cells[1].Value.ToString();
                    for (int i = 0; i < data.Count(); i++)
                    {
                        if (data[i] == dat)
                        {
                            dataGridView9.Rows[ID].Cells[1].Value = data[(i + 1) % data.Count()];
                            reload_PM();
                            break;
                        }
                    }
                }
            }
            else
                if (e.ColumnIndex == 0 && e.RowIndex >= 0)
                {
                    // Eintragen
                    Eingabe.Show();
                }


        }

        private void comboBox5_SelectedValueChanged(object sender, EventArgs e)
        {
            sel = comboBox5.SelectedIndex;
            comboBox4.SelectedIndex = sel;
            comboBox16.SelectedIndex = sel;
            listBox1.SelectedIndex = sel;
            change_selected();
            tabPage3_Enter(null, null);
        }

        private void toolStripTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                toolStripMenuItem1_Click(null, null);
                contextMenuStrip1.Hide();
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DataGridView[] List = { dataGridView1, dataGridView2, dataGridView4, dataGridView5, dataGridView9 };
            ToolStripMenuItem[] List2 = { toolStripMenuItem2, toolStripMenuItem3, toolStripMenuItem4, toolStripMenuItem5 };
            String[] data = { "INT", "BIN", "HEX", "ASC" };
            for (int i = 0; i < data.Count(); i++)
            {
                if (sender == List2[i])
                {

                    for (int b = 0; b < List[open_context].SelectedRows.Count; b++)
                    {
                        int ID = List[open_context].SelectedRows[b].Index;

                        List[open_context].Rows[ID].Cells[1].Value = data[i];
                        if (open_context == 4) reload_PM();
                        if (open_context == 3) reload_eeprom();
                        if (open_context == 2) reload_Ports();
                        if (open_context == 1) reload_SRAM();
                        if (open_context == 0) reload_table();
                    }
                    break;
                }
            }
        }

        int open_context = 0;

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            open_context = 0;
        }

        private void dataGridView2_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            open_context = 1;
        }

        private void dataGridView4_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            open_context = 2;
        }

        private void dataGridView5_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            open_context = 3;
        }

        private void dataGridView9_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            open_context = 4;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
            {
                textBox8.Enabled = false;
            }
            else
            {
                textBox8.Enabled = true;
            }
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            reload_komm = true;
        }

        private void textBox9_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Eingabe.Hide();
                DataGridView[] List = { dataGridView1, dataGridView2, dataGridView4, dataGridView5, dataGridView9 };
                int Resultat = 0;
                if (textBox9.Text == "") return;

                for (int i = 0; i < List[open_context].SelectedRows.Count; i++)
                {
                    int row = List[open_context].SelectedRows[i].Index;
                    if (List[open_context].Rows[row].Cells[1].Value == null || List[open_context].Rows[row].Cells[1].Value.ToString() == "INT")
                    {
                        Resultat = Main[sel].INT(textBox9.Text);
                    }
                    else
                        if (List[open_context].Rows[row].Cells[1].Value.ToString() == "BIN")
                        {
                            Resultat = (int)Convert.ToInt64(textBox9.Text, 2);
                        }
                        else
                            if (List[open_context].Rows[row].Cells[1].Value.ToString() == "HEX")
                            {
                                Resultat = (int)Convert.ToInt64(textBox9.Text, 16);
                            }
                            else
                                if (dataGridView1.Rows[row].Cells[1].Value.ToString() == "ASC")
                                {
                                    Resultat = (int)textBox9.Text[0];
                                }
                    if (open_context == 4) { Main[sel].PM[row] = (Byte)Resultat; reload_PM(); }
                    if (open_context == 3) { Main[sel].EEPROM.SPEICHER[row] = (Byte)Resultat; reload_eeprom(); }
                    if (open_context == 2) { Main[sel].Ports[row].set((Byte)Resultat); reload_Ports(); }
                    if (open_context == 1) { Main[sel].SRAM[row] = (Byte)Resultat; reload_SRAM(); }
                    if (open_context == 0) { Main[sel].Register[row] = (Byte)Resultat; reload_table(); }
                }
                if (open_context == 4) { reload_PM(); }
                if (open_context == 3) { reload_eeprom(); }
                if (open_context == 2) { reload_Ports(); }
                if (open_context == 1) { reload_SRAM(); }
                if (open_context == 0) { reload_table(); }

            }
        }

        private void Eingabe_VisibleChanged(object sender, EventArgs e)
        {
            if (Eingabe.Visible == true)
            {
                /* if (open_context == 4) {textBox9.Text = Main[sel].PM[this.dataGridView9.SelectedRows[0].Index]; }
                 if (open_context == 3) { Main[sel].EEPROM.SPEICHER[row] = (Byte)Resultat; reload_eeprom(); }
                 if (open_context == 2) { Main[sel].Ports[row].set((Byte)Resultat); reload_Ports(); }
                 if (open_context == 1) { Main[sel].SRAM[row] = (Byte)Resultat; reload_SRAM(); }
                 if (open_context == 0) { Main[sel].Register[row] = (Byte)Resultat; reload_table(); }*/

                textBox9.Focus();
                textBox9.Text = "";
                tabControl1.Enabled = false;
                Eingabe.Enabled = true;
            }
            else
                tabControl1.Enabled = true;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (sel >= Main.Count || sel >= CopyMain.Count) return;
            if (checkBox6.Checked)
            {
                for (int i = 0; i < Main.Count; i++)
                {
                    Main[i] = CopyMain[i];
                }
            }
            else
                Main[sel] = CopyMain[sel];
            reload_komm = true;
        }

        private void toolStripComboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.contextMenuStrip1.Hide();
                if (toolStripComboBox1.Text == "") return;
                String Name = toolStripComboBox1.Text;
                int start = richTextBox1.SelectionStart;
                int line = -1;
                line = richTextBox1.GetLineFromCharIndex(start);

                if (line != -1 && isProgram(line, sel))
                {
                    int found = -1;
                    for (int i = 0; i < Main[sel].ZAEHLER.Count(); i++)
                    {
                        if (Main[sel].ZAEHLER[i].Name == Name) { found = i; break; }
                    }
                    if (found == -1) return;

                    int ib = 0;
                    for (int i = 0; i < Main[sel].Program[0].Count; i++)
                    {
                        if (Main[sel].Program[0][i].Original == line) { ib = i; break; }
                    }

                    int len3 = Main[sel].Quelltext[0][line].Length;
                    int start3 = richTextBox1.GetFirstCharIndexFromLine(line);
                    richTextBox1.Select(start3, len3);

                    Main[sel].ZAEHLER[found].Ende = ib;

                    start = richTextBox1.GetFirstCharIndexFromLine(line);
                    Main[sel].Program[0][ib].Zaehler_Ende.Add((short)found);

                    Main[sel].Text.Select(start, Main[sel].Quelltext[0][line].Length); // geändert

                    Main[sel].Quelltext[0][line] = Main[sel].Quelltext[0][line] + "[E-" + Name + "]";
                    int len = ((String)"[E-" + Name + "]").Length;
                    int len2 = Main[sel].Text.Lines[line].Length;
                    //start = Main[sel].Text.GetFirstCharIndexFromLine(line);

                    Main[sel].Text.SelectedText = Main[sel].Quelltext[0][line];
                    Main[sel].ZAEHLER[found].Ende_Length = len;
                    richTextBox1.SelectedText = Main[sel].Quelltext[0][line];
                    Draw_Zaehler_In_Line(richTextBox1, sel, line);
                    Draw_Zaehler_In_Line(Main[sel].Text, sel, line);
                    //Main[sel].ZAEHLER[found].Ende_Length = 0;

                }
            }
        }

        private void comboBox4_SelectedValueChanged(object sender, EventArgs e)
        {
            sel = comboBox4.SelectedIndex;
            comboBox5.SelectedIndex = sel; ;
            listBox1.SelectedIndex = sel;
            comboBox16.SelectedIndex = sel;
            change_selected();
            reload_komm = true;
            //tabPage2_Enter(null, null);
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            /*  timer5.Enabled = false;
              selected = -1;
              for (int b = 0; b < Main.Count; b++)
                  for (int c = 0; c < Main[b].Program.Count; c++) 
                      Draw_Zaehler_In_Line(Main[b].Text, b, Main[b].Program[c].Original);
              change_selected();*/

        }

        private void button11_Click_2(object sender, EventArgs e)
        {
            timer5.Enabled = true;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                Eingabe.Show();
            }
        }

        bool blocked = false;

        public void reload_scenery_top()
        {
            comboBox8.Items.Clear();
            comboBox8.Items.Add("Global");
            for (int i = 0; i < Main.Count; i++) comboBox8.Items.Add(Main[i].Name);
            comboBox8.SelectedIndex = 0;
        }

        public void reload_scenery_bottom()
        {
            int id = comboBox8.SelectedIndex;
            comboBox9.Items.Clear();
            if (id == 0)
            {
                for (int i = 0; i < global_scenery.Count; i++) comboBox9.Items.Add(global_scenery[i].Name);
            }
            else
            {
                id--;
                for (int i = 0; i < Main[id].scenery.Count; i++) comboBox9.Items.Add(Main[id].scenery[i].Name);
            }

            if (comboBox9.Items.Count > 0)
            {
                comboBox9.SelectedIndex = 0;
                comboBox9.Show();
                groupBox13.Show();
            }
            else
            {
                comboBox9.Hide();
                groupBox13.Hide();
            }
        }

        public String[] summaryLine(SZENARIEN temp, int i)
        {
            bool isglobal = false;
            for (int b = 0; b < global_scenery.Count; b++) if (temp == global_scenery[b]) { isglobal = true; break; }
            int target = temp.Events[i].Ziel_ID == -1 ? (isglobal ? 0 : -1) : temp.Events[i].Ziel_ID;
            if (target == -1)
            {
                // MainElement
                for (int b = 0; b < Main.Count; b++)
                    for (int c = 0; c < Main[b].scenery.Count; c++)
                        if (Main[b].scenery[c] == temp)
                        {
                            target = b;
                        }
            }
            temp.Events[i].Ziel = Main[target];

            String ADD = "[" + Main[target].Name + "]  ";
            String ADD2 = "";
            if (!temp.Events[i].Random_Time)
            {
                ADD2 = ADD2 + "+" + temp.Events[i].Time + "";
            }
            else
            {
                ADD2 = ADD2 + "+" + temp.Events[i].Random_Time_From + ">>" + temp.Events[i].Random_Time_To + "";
            }

            switch (temp.Events[i].Typ)
            {
                case 0: // Arbeitsregister
                    if (!temp.Events[i].Param1_Random)
                    {
                        ADD = ADD + "  ARBREG  " + "R" + temp.Events[i].Param1;
                    }
                    else
                    {
                        ADD = ADD + "  ARBREG  " + "R" + temp.Events[i].Param1_From + ">>R" + temp.Events[i].Param1_To;
                    }
                    break;
                case 1: // SRAM
                    if (!temp.Events[i].Param1_Random)
                    {
                        ADD = ADD + "  SRAM  " + Main[target].SRAM_name[temp.Events[i].Param1];
                    }
                    else
                    {
                        ADD = ADD + "  SRAM  " + Main[target].SRAM_name[temp.Events[i].Param1_From] + ">>" + Main[target].SRAM_name[temp.Events[i].Param1_To];
                    }
                    break;
                case 2: // Ports
                    if (!temp.Events[i].Param1_Random)
                    {
                        ADD = ADD + "  REGS  " + Main[target].Ports[temp.Events[i].Param1].Bezeichnung;
                    }
                    else
                    {
                        ADD = ADD + "  REGS  " + Main[target].Ports[temp.Events[i].Param1_From].Bezeichnung + ">>" + Main[target].Ports[temp.Events[i].Param1_To].Bezeichnung;
                    }
                    break;
                case 3: // EEPORM
                    if (!temp.Events[i].Param1_Random)
                    {
                        ADD = ADD + "  EEPROM  " + "EEPORM[" + temp.Events[i].Param1 + "]";
                    }
                    else
                    {
                        ADD = ADD + "  EEPROM  " + "EEPORM[" + temp.Events[i].Param1 + "]>>EEPORM[" + temp.Events[i].Param1_From + "]";
                    }
                    break;
                case 4: // PM
                    if (!temp.Events[i].Param1_Random)
                    {
                        ADD = ADD + "  PM  " + Main[target].PM_name[temp.Events[i].Param1];
                    }
                    else
                    {
                        ADD = ADD + "  PM  " + Main[target].PM_name[temp.Events[i].Param1_From] + ">>" + Main[target].PM_name[temp.Events[i].Param1_To];
                    }
                    break;
                case 5: // Minianwendung

                    if (temp.Events[i].Param1 < Minianwendungen.Count) ADD = ADD + "  ANW  " + Minianwendungen[temp.Events[i].Param1] + "  " + temp.Events[i].Param2;
                    break;
            }

            if (temp.Events[i].Typ != 5)
            {
                if (!temp.Events[i].Param2_Random)
                {
                    ADD = ADD + "  " + temp.Events[i].Param2;
                }
                else
                {
                    ADD = ADD + "  " + temp.Events[i].Param2_From + ">>" + temp.Events[i].Param2_To;
                }
            }
            String[] result = { ADD, ADD2 };
            return result;
        }

        public void refresh_scenery()
        {
            int id = comboBox8.SelectedIndex;
            int scene = comboBox9.SelectedIndex;
            if (scene == -1)
            {
                dataGridView10.Hide(); groupBox14.Hide();
                return;
            }
            else
            {
                dataGridView10.Show();
                groupBox14.Show();
            }
            //                    
            SZENARIEN temp = null;
            dataGridView10.Rows.Clear();
            if (id == 0)
            {
                temp = global_scenery[scene];

            }
            else
            {
                id--;
                temp = Main[id].scenery[scene];
            }

            if (temp != null)
            {
                int a = temp.Events.Count - dataGridView10.Rows.Count;
                if (a > 0) dataGridView10.Rows.Add(a);
                for (int i = 0; i < temp.Events.Count; i++)
                {
                    String[] res = summaryLine(temp, i);
                    dataGridView10.Rows[i].HeaderCell.Value = res[1];
                    dataGridView10.Rows[i].Cells[0].Value = res[0];

                }
                if (a == 0)
                {
                    dataGridView10.Hide(); groupBox14.Hide();
                    return;
                }
                else
                {
                    dataGridView10.Show(); groupBox14.Show();
                }
            }

            blocked = false;
        }

        private void tabPage5_Enter(object sender, EventArgs e)
        {
            reload_scenery_top();
            reload_scenery_bottom();
            refresh_scenery();
        }

        private void button11_Click_3(object sender, EventArgs e)
        {
            int id = comboBox8.SelectedIndex;
            if (id == 0)
            {
                global_scenery.Add(new SZENARIEN(null));
            }
            else
            {
                id--;
                Main[id].scenery.Add(new SZENARIEN(Main[id]));
            }
            reload_scenery_bottom();
            comboBox9.SelectedIndex = comboBox9.Items.Count - 1;
            refresh_scenery();
        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            int id = comboBox8.SelectedIndex;
            int id2 = comboBox9.SelectedIndex;
            if (id2 < 0) return;
            if (id == 0)
            {
                global_scenery.RemoveAt(id2);
            }
            else
            {
                id--;
                Main[id].scenery.RemoveAt(id2);
            }

            reload_scenery_bottom();
            refresh_scenery();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            int id = comboBox8.SelectedIndex;
            int id2 = comboBox9.SelectedIndex;
            if (id2 < 0) return;
            if (id == 0)
            {
                global_scenery[id2].AddLine();
            }
            else
            {
                id--;
                Main[id].scenery[id2].AddLine();
            }
            refresh_scenery();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            int id = comboBox8.SelectedIndex;
            int id2 = comboBox9.SelectedIndex;

            if (id2 < 0) return;

            for (int i = 0; i < dataGridView10.SelectedRows.Count; i++)
            {
                if (id == 0)
                {
                    global_scenery[id2].Events.RemoveAt(dataGridView10.SelectedRows[i].Index);
                }
                else
                {
                    id--;
                    Main[id].scenery[id2].Events.RemoveAt(dataGridView10.SelectedRows[i].Index);
                }

                refresh_scenery();
            }
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            blocked = true;
            reload_scenery_bottom();
            refresh_scenery();
            textBox11.Text = comboBox9.Text;
            int id = comboBox8.SelectedIndex;
            int id2 = comboBox9.SelectedIndex;

            if (id2 < 0) return;
            if (id == 0)
            {
                checkBox8.Checked = global_scenery[id2].Aktiv;
                checkBox9.Checked = global_scenery[id2].Loop;
                textBox10.Text = global_scenery[id2].Replays.ToString();
            }
            else
            {
                id--;
                checkBox8.Checked = Main[id].scenery[id2].Aktiv;
                checkBox9.Checked = Main[id].scenery[id2].Loop;
                textBox10.Text = Main[id].scenery[id2].Replays.ToString();
            }

        }

        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!blocked) refresh_scenery();
            int id = comboBox8.SelectedIndex;
            int id2 = comboBox9.SelectedIndex;
            if (id2 < 0) return;
            if (id == 0)
            {
                checkBox8.Checked = global_scenery[id2].Aktiv;
                checkBox9.Checked = global_scenery[id2].Loop;
                textBox10.Text = global_scenery[id2].Replays.ToString();
            }
            else
            {
                id--;
                checkBox8.Checked = Main[id].scenery[id2].Aktiv;
                checkBox9.Checked = Main[id].scenery[id2].Loop;
                textBox10.Text = Main[id].scenery[id2].Replays.ToString();
            }
        }

        private void textBox11_KeyUp(object sender, KeyEventArgs e)
        {
            if (comboBox9.SelectedIndex < 0) return;
            comboBox9.Items[comboBox9.SelectedIndex] = textBox11.Text;
            int id = comboBox8.SelectedIndex;
            if (id == 0)
            {
                global_scenery[comboBox9.SelectedIndex].Name = textBox11.Text;
            }
            else
            {
                id--;
                Main[id].scenery[comboBox9.SelectedIndex].Name = textBox11.Text;
            }
        }

        public void Reload_Zeile(int id)
        {
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == -1 || ident2 == -1 || ident3 == -1) return;
            if (id == 0)
            {
                comboBox10.Items.Clear();
                for (int i = 0; i < Main.Count; i++) comboBox10.Items.Add(Main[i].Name);
                comboBox10.Visible = comboBox10.Items.Count > 0 ? true : false;
                int temp = ident == 0 ? global_scenery[ident2].Events[ident3].Ziel_ID : Main[ident - 1].scenery[ident2].Events[ident3].Ziel_ID;
                comboBox10.SelectedIndex = temp == -1 ? (ident == 0 ? 0 : ident - 1) : temp;
                //Reload_Zeile(1);
            }
            else
                if (id == 1)
                {

                    comboBox11.Visible = comboBox10.Visible ? true : false;
                    comboBox11.SelectedIndex = ident == 0 ? global_scenery[ident2].Events[ident3].Typ : Main[ident - 1].scenery[ident2].Events[ident3].Typ;
                    //Reload_Zeile(2);
                }
                else
                    if (id == 2)
                    {
                        int identer = comboBox10.SelectedIndex;
                        comboBox12.Items.Clear();
                        switch (comboBox11.SelectedIndex)
                        {
                            case 0: // Arbeitsregister
                                for (int i = 0; i < Main[identer].Register.Count(); i++) comboBox12.Items.Add("R" + i);
                                break;
                            case 1: // SRAM
                                for (int i = 0; i < Main[identer].SRAM.Count(); i++) comboBox12.Items.Add(Main[identer].SRAM_name[i]);
                                break;
                            case 2: // Ports
                                for (int i = 0; i < Main[identer].Ports.Count(); i++) comboBox12.Items.Add(Main[identer].Ports[i].Bezeichnung);
                                break;
                            case 3: // EEPORM
                                for (int i = 0; i < Main[identer].EEPROM.SPEICHER.Count(); i++) comboBox12.Items.Add("EEPORM[" + i + "]");
                                break;
                            case 4: // PM
                                for (int i = 0; i < Main[identer].PM_name.Count; i++) comboBox12.Items.Add(Main[identer].PM_name[i]);
                                break;
                            case 5: // Minianwendung
                                for (int i = 0; i < Minianwendungen.Count; i++) comboBox12.Items.Add(Minianwendungen[i]);
                                if (comboBox12.SelectedIndex > Minianwendungen.Count) if (comboBox12.Items.Count > 0)
                                    {
                                        comboBox12.SelectedIndex = 0;
                                        global_scenery[ident2].Events[ident3].Param1 = 0;
                                    }
                                // for (int i = 0; i < Main[identer].PM_name.Count; i++) comboBox12.Items.Add(Main[identer].PM_name[i]);
                                break;
                        }
                        comboBox12.Visible = comboBox11.Visible ? true : false;

                        int temp = ident == 0 ? global_scenery[ident2].Events[ident3].Param1 : Main[ident - 1].scenery[ident2].Events[ident3].Param1; //comboBox12.Items.Count > 0 ? 0 : -1;
                        if (temp >= comboBox12.Items.Count) temp = comboBox12.Items.Count - 1;
                        comboBox12.SelectedIndex = temp;
                        bool q = checkBox10.Checked;
                        checkBox10.Checked = ident == 0 ? global_scenery[ident2].Events[ident3].Param1_Random : Main[ident - 1].scenery[ident2].Events[ident3].Param1_Random;
                        if (checkBox10.Checked == q) Reload_Zeile(3); // um reload sicherzustellen

                        textBox16.Text = ident == 0 ? global_scenery[ident2].Events[ident3].Param2.ToString() : Main[ident - 1].scenery[ident2].Events[ident3].Param2.ToString();
                        q = checkBox11.Checked;
                        checkBox11.Checked = ident == 0 ? global_scenery[ident2].Events[ident3].Param2_Random : Main[ident - 1].scenery[ident2].Events[ident3].Param2_Random;
                        if (checkBox11.Checked == q) Reload_Zeile(4); // um reload sicherzustellen

                        textBox17.Text = ident == 0 ? global_scenery[ident2].Events[ident3].Time.ToString() : Main[ident - 1].scenery[ident2].Events[ident3].Time.ToString();
                        q = checkBox12.Checked;
                        checkBox12.Checked = ident == 0 ? global_scenery[ident2].Events[ident3].Random_Time : Main[ident - 1].scenery[ident2].Events[ident3].Random_Time;
                        if (checkBox12.Checked == q) Reload_Zeile(5); // um reload sicherzustellen

                    }
                    else
                        if (id == 3)
                        {
                            int identer = comboBox10.SelectedIndex;
                            if (comboBox11.SelectedIndex == 5)
                            {
                                checkBox10.Enabled = false;
                                label33.Text = "Anwendung:";
                                label34.Enabled = false;
                                comboBox13.Enabled = false;
                                comboBox14.Enabled = false;
                            }
                            else
                                if (checkBox10.Checked && comboBox12.SelectedIndex >= 0)
                                {
                                    comboBox12.Enabled = false;
                                    comboBox13.Items.Clear(); comboBox14.Items.Clear();
                                    for (int i = 0; i < comboBox12.Items.Count; i++)
                                    {
                                        comboBox13.Items.Add(comboBox12.Items[i]); comboBox14.Items.Add(comboBox12.Items[i]);
                                    }
                                    comboBox13.SelectedIndex = ident == 0 ? global_scenery[ident2].Events[ident3].Param1_From : Main[ident - 1].scenery[ident2].Events[ident3].Param1_From;
                                    comboBox14.SelectedIndex = ident == 0 ? global_scenery[ident2].Events[ident3].Param1_To : Main[ident - 1].scenery[ident2].Events[ident3].Param1_To;
                                    label34.Enabled = true;
                                    comboBox13.Enabled = true;
                                    comboBox14.Enabled = true;
                                    checkBox10.Enabled = true;
                                    label33.Text = "Zelle:";
                                }
                                else
                                {
                                    label33.Text = "Zelle:";
                                    comboBox12.Enabled = true;
                                    label34.Enabled = false; ;
                                    comboBox13.Enabled = false;
                                    comboBox14.Enabled = false;
                                    checkBox10.Enabled = true;
                                    comboBox12.Enabled = true;
                                }
                        }
                        else
                            if (id == 4)
                            {
                                int identer = comboBox10.SelectedIndex;
                                if (comboBox11.SelectedIndex == 5)
                                {
                                    textBox16.Enabled = true;
                                    label36.Text = "Timeout:";
                                    label35.Enabled = false; ;
                                    textBox15.Enabled = false;
                                    textBox14.Enabled = false;
                                    checkBox11.Enabled = false;
                                    comboBox12.Enabled = true;
                                }
                                else
                                    if (checkBox11.Checked && comboBox12.SelectedIndex >= 0)
                                    {
                                        this.textBox16.Enabled = false;
                                        textBox15.Text = ident == 0 ? global_scenery[ident2].Events[ident3].Param2_From.ToString() : Main[ident - 1].scenery[ident2].Events[ident3].Param2_From.ToString();
                                        textBox14.Text = ident == 0 ? global_scenery[ident2].Events[ident3].Param2_To.ToString() : Main[ident - 1].scenery[ident2].Events[ident3].Param2_To.ToString();
                                        label35.Enabled = true;
                                        textBox15.Enabled = true;
                                        textBox14.Enabled = true;
                                        checkBox11.Enabled = true; label36.Text = "Wert:";
                                    }
                                    else
                                    {
                                        textBox16.Text = ident == 0 ? global_scenery[ident2].Events[ident3].Param2.ToString() : Main[ident - 1].scenery[ident2].Events[ident3].Param2.ToString();
                                        textBox16.Enabled = true;
                                        label35.Enabled = false; ;
                                        textBox15.Enabled = false;
                                        textBox14.Enabled = false;
                                        checkBox11.Enabled = true; label36.Text = "Wert:";
                                    }
                            }
                            else
                                if (id == 5)
                                {
                                    int identer = comboBox10.SelectedIndex;
                                    if (checkBox12.Checked && comboBox12.SelectedIndex >= 0)
                                    {
                                        this.textBox17.Enabled = false;
                                        textBox13.Text = ident == 0 ? global_scenery[ident2].Events[ident3].Random_Time_From.ToString() : Main[ident - 1].scenery[ident2].Events[ident3].Random_Time_From.ToString();
                                        textBox12.Text = ident == 0 ? global_scenery[ident2].Events[ident3].Random_Time_To.ToString() : Main[ident - 1].scenery[ident2].Events[ident3].Random_Time_To.ToString();
                                        label38.Enabled = true;
                                        textBox13.Enabled = true;
                                        textBox12.Enabled = true;
                                    }
                                    else
                                    {
                                        textBox17.Text = ident == 0 ? global_scenery[ident2].Events[ident3].Time.ToString() : Main[ident - 1].scenery[ident2].Events[ident3].Time.ToString();
                                        textBox17.Enabled = true;
                                        label38.Enabled = false; ;
                                        textBox13.Enabled = false;
                                        textBox12.Enabled = false;
                                    }
                                }
        }

        private void dataGridView10_SelectionChanged(object sender, EventArgs e)
        {
            Reload_Zeile(0);
            Reload_Zeile(1);
            Reload_Zeile(2);
            Reload_Zeile(3);
            Reload_Zeile(4);
            Reload_Zeile(5);
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Ziel = Main[comboBox10.SelectedIndex];
                global_scenery[ident2].Events[ident3].Ziel_ID = comboBox10.SelectedIndex;
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Ziel = Main[comboBox10.SelectedIndex];
                Main[ident - 1].scenery[ident2].Events[ident3].Ziel_ID = comboBox10.SelectedIndex;
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
            Reload_Zeile(1);
        }

        private void comboBox11_SelectedIndexChanged(object sender, EventArgs e)
        {
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Typ = comboBox11.SelectedIndex;
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Typ = comboBox11.SelectedIndex;
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
            Reload_Zeile(2);
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Param2_Random = checkBox11.Checked;
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Param2_Random = checkBox11.Checked;
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
            Reload_Zeile(4);
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Param1_Random = checkBox10.Checked;
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Param1_Random = checkBox10.Checked;
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
            Reload_Zeile(3);
        }

        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Param1 = comboBox12.SelectedIndex;
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Param1 = comboBox12.SelectedIndex;
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
            Reload_Zeile(3);
            Reload_Zeile(4);
            Reload_Zeile(5);

            ident = comboBox10.SelectedIndex;
            //ident2 = comboBox9.SelectedIndex;
            //ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (comboBox11.SelectedIndex == 5)
            {
                // Lade Anwendung in den Speicher
                int anwendung = comboBox12.SelectedIndex;
                if (anwendung + 1 >= Main[ident].Program.Count)
                {
                    // muss neu eingelesen werden
                    String dat = Minianwendungen_Datei[anwendung];
                    StreamReader datei = new StreamReader(dat);
                    List<String> data = new List<String>();
                    while (datei.Peek() != -1)
                    {
                        data.Add(datei.ReadLine());
                    }
                    datei.Close();
                    Main[ident].Selected_Software = anwendung + 1;
                    Main[ident].Load_Content(data, false);
                    Main[ident].Selected_Software = 0;
                }
            }
        }

        private void comboBox13_SelectedIndexChanged(object sender, EventArgs e)
        {
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Param1_From = comboBox13.SelectedIndex;
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Param1_From = comboBox13.SelectedIndex;
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
        }

        private void comboBox14_SelectedIndexChanged(object sender, EventArgs e)
        {
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Param1_To = comboBox14.SelectedIndex;
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Param1_To = comboBox14.SelectedIndex;
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
        }

        private void textBox16_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Help.IsNumber(textBox16.Text)) return;
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Param2 = Convert.ToInt32(textBox16.Text);
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Param2 = Convert.ToInt32(textBox16.Text);
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
        }

        private void textBox15_TextChanged(object sender, EventArgs e)
        {
            if (!Help.IsNumber(textBox15.Text)) return;
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Param2_From = Convert.ToInt32(textBox15.Text);
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Param2_From = Convert.ToInt32(textBox15.Text);
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            if (!Help.IsNumber(textBox14.Text)) return;
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Param2_To = Convert.ToInt32(textBox14.Text);
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Param2_To = Convert.ToInt32(textBox14.Text);
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
        }

        private void textBox17_TextChanged(object sender, EventArgs e)
        {
            if (!Help.IsNumber(textBox17.Text)) return;
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Time = Convert.ToInt32(textBox17.Text) < 1 ? 0 : Convert.ToInt32(textBox17.Text);
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Time = Convert.ToInt32(textBox17.Text) < 1 ? 0 : Convert.ToInt32(textBox17.Text);
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Random_Time = checkBox12.Checked;
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Random_Time = checkBox12.Checked;
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
            Reload_Zeile(5);
        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            if (!Help.IsNumber(textBox13.Text)) return;
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Random_Time_From = Convert.ToInt32(textBox13.Text) < 0 ? 0 : Convert.ToInt32(textBox13.Text);
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Random_Time_From = Convert.ToInt32(textBox13.Text) < 0 ? 0 : Convert.ToInt32(textBox13.Text);
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            if (!Help.IsNumber(textBox12.Text)) return;
            int ident = comboBox8.SelectedIndex;
            int ident2 = comboBox9.SelectedIndex;
            int ident3 = dataGridView10.SelectedRows.Count > 0 ? dataGridView10.SelectedRows[0].Index : -1;
            if (ident == 0)
            {
                global_scenery[ident2].Events[ident3].Random_Time_To = Convert.ToInt32(textBox12.Text) < 0 ? 0 : Convert.ToInt32(textBox12.Text);
            }
            else
            {
                Main[ident - 1].scenery[ident2].Events[ident3].Random_Time_To = Convert.ToInt32(textBox12.Text) < 0 ? 0 : Convert.ToInt32(textBox12.Text);
                ident--;
            }
            String[] res = summaryLine(comboBox8.SelectedIndex == 0 ? global_scenery[ident2] : Main[ident].scenery[ident2], ident3);
            dataGridView10.Rows[ident3].HeaderCell.Value = res[1];
            dataGridView10.Rows[ident3].Cells[0].Value = res[0];
        }

        bool Order_Analyse_Init = false;

        private void button19_Click(object sender, EventArgs e)
        {
            if (sel >= Main.Count) return;
            if (!Order_Analyse_Init)
            {
                dataGridView11.Rows.Clear();
                int a = 108 - dataGridView11.Rows.Count;
                if (a > 0) dataGridView11.Rows.Add(a);
                for (int i = 0; i < 108; i++)
                {
                    dataGridView11.Rows[i].HeaderCell.Value = Help.Orders[i];
                }
                Order_Analyse_Init = true;
            }

            double fact = 1000L * 1000L * 1000L / Stopwatch.Frequency;
            for (int i = 0; i < 108; i++)
            {
                dataGridView11.Rows[i].Cells[0].Value = Main[sel].Count_Orders[i];
                if (Main[sel].Watches != null && Main[sel].Count_Orders[i] != 0)
                { // Main[sel].Watches[i].ElapsedTicks /  

                    dataGridView11.Rows[i].Cells[1].Value = Math.Round((double)(Main[sel].Watches[i].ElapsedTicks * fact) / Main[sel].Count_Orders[i], 4) + " ns";
                }
                else
                    dataGridView11.Rows[i].Cells[1].Value = "-";
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            int id = comboBox8.SelectedIndex;
            int id2 = comboBox9.SelectedIndex;

            if (id2 < 0) return;
            if (id == 0)
            {
                global_scenery[id2].Aktiv = checkBox8.Checked;
            }
            else
            {
                id--;
                Main[id].scenery[id2].Aktiv = checkBox8.Checked;

            }
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            int id = comboBox8.SelectedIndex;
            int id2 = comboBox9.SelectedIndex;

            if (id2 < 0) return;
            if (id == 0)
            {
                global_scenery[id2].Loop = checkBox9.Checked;
            }
            else
            {
                id--;
                Main[id].scenery[id2].Loop = checkBox9.Checked;
            }
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            int id = comboBox8.SelectedIndex;
            int id2 = comboBox9.SelectedIndex;

            if (id2 < 0) return;
            if (id == 0)
            {
                global_scenery[id2].Replays = Help.IsNumber(textBox10.Text) ? Convert.ToInt32(textBox10.Text) : 0;
            }
            else
            {
                id--;
                Main[id].scenery[id2].Replays = Help.IsNumber(textBox10.Text) ? Convert.ToInt32(textBox10.Text) : 0;
            }
        }

        private void comboBox16_SelectedIndexChanged(object sender, EventArgs e)
        {
            sel = comboBox16.SelectedIndex;
            comboBox4.SelectedIndex = sel;
            comboBox5.SelectedIndex = sel;
            listBox1.SelectedIndex = sel;
            change_selected();
            reload_komm = true;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            for (int i = dataGridView12.SelectedRows.Count - 1; i >= 0; i--)
            {
                int id = dataGridView12.SelectedRows[i].Index;
                Minianwendungen.RemoveAt(id);
                Minianwendungen_Datei.RemoveAt(id);
                dataGridView12.Rows.RemoveAt(id);
            }
        }

        private void button19_Click_1(object sender, EventArgs e)
        {
            openFileDialog4.InitialDirectory = Application.StartupPath;
            openFileDialog4.FileName = "";
            openFileDialog4.ShowDialog();
        }

        private void openFileDialog4_FileOk(object sender, CancelEventArgs e)
        {

            if (openFileDialog4.FileName.Length >= Application.StartupPath.Length) if (openFileDialog4.FileName.Substring(0, Application.StartupPath.Length) == Application.StartupPath) openFileDialog4.FileName = openFileDialog4.FileName.Substring(Application.StartupPath.Length + 1, openFileDialog4.FileName.Length - Application.StartupPath.Length - 1);
            Minianwendungen.Add(Path.GetFileName(openFileDialog4.FileName));
            Minianwendungen_Datei.Add(openFileDialog4.FileName);
            dataGridView12.Rows.Add();
            int id = dataGridView12.Rows.Count - 1;
            dataGridView12.Rows[id].HeaderCell.Value = Minianwendungen[id];
            dataGridView12.Rows[id].Cells[0].Value = Minianwendungen_Datei[id];
        }

        private void Draw_Oszi()
        {
            comboBox17.Items.Clear();
            for (int i = 0; i < Main[sel].Ports.Count; i++)
            {
                comboBox17.Items.Add(Main[sel].Ports[i].Bezeichnung);
            }
            if (comboBox17.Items.Count > 0) comboBox17.SelectedIndex = 0;
        }

        private void Draw_Oszi2()
        {
            comboBox18.Items.Clear();
            int selected = comboBox17.SelectedIndex;
            if (selected >= 0)
            {
                for (int i = 0; i < Main[sel].Ports[selected].Anz; i++)
                {
                    comboBox18.Items.Add(i.ToString());
                }
                comboBox18.Enabled = true;
                if (comboBox18.Items.Count > 0) comboBox18.SelectedIndex = 0;
            }
            else
            {
                comboBox18.Enabled = false;
            }
        }
        private void Draw_Oszi3()
        {
            int selected = comboBox17.SelectedIndex;
            int selected2 = comboBox18.SelectedIndex;
            if (selected >= 0 && selected2 >= 0)
            {

                checkBox13.Checked = Main[sel].Ports[selected].Oszillatoren[selected2].Aktiv;
                checkBox13.Enabled = true;
            }
            else
            {
                checkBox13.Enabled = false;
            }
        }

        private void tabPage6_Enter(object sender, EventArgs e)
        {
            Draw_Oszi();
            Draw_Oszi2();
            Draw_Oszi3();
        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            int selected = comboBox17.SelectedIndex;
            int selected2 = comboBox18.SelectedIndex;
            if (checkBox13.Checked)
            {
                bool found = false;
                for (int i = 0; i < Main[sel].ActiveOszi.Count; i++)
                {
                    if (Main[sel].ActiveOszi[i] == Main[sel].Ports[selected] && Main[sel].ActiveOsziPin[i] == selected2)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Main[sel].ActiveOszi.Add(Main[sel].Ports[selected]);
                    Main[sel].ActiveOsziPin.Add((short)selected2);
                }
            }
            else
            {
                for (int i = 0; i < Main[sel].ActiveOszi.Count; i++)
                {
                    if (Main[sel].ActiveOszi[i] == Main[sel].Ports[selected] && Main[sel].ActiveOsziPin[i] == selected2)
                    {
                        Main[sel].ActiveOszi.RemoveAt(i);
                        Main[sel].ActiveOsziPin.RemoveAt(i);
                        break;
                    }
                }
            }
            Main[sel].Ports[selected].Oszillatoren[selected2].Aktiv = checkBox13.Checked;
        }

        private void comboBox17_SelectedIndexChanged(object sender, EventArgs e)
        {
            Draw_Oszi2();
        }

        private void comboBox18_SelectedIndexChanged(object sender, EventArgs e)
        {
            Draw_Oszi3();
        }

        public String Erweitern(String Text)
        {
            for (; Text.Length < 3; Text = "0" + Text) ;
            return Text;
        }

        String[] Befehl = { "ADD", "SUB", "ADC-O", "ADC-M", "SBC-O", "SBC-M", "EOR", "OR", "AND", "COM", "NEG", "INC", "DEC", "TST", "CLR", "SER", "MUL", "CPC-O", "CP", "CPC-M", "MOV", "LSL", "LSR", "ROL", "ROR", "ASR", "SWAP", "SUBI", "SBCI-O", "SBCI-M", "ORI", "ANDI", "CPI" };
        String[] Text2 = { "ADD r0, r1", "SUB r0, r1", "ADC r0, r1", "ADC r0, r1", "SBC r0, r1", "SBC r0, r1", "EOR r0, r1", "OR r0, r1", "AND r0, r1", "COM r0", "NEG r0", "INC r0", "DEC r0", "TST r0", "CLR r0", "SER r0", "MUL r0, r1", "CPC r0, r1", "CP r0, r1", "CPC r0, r1", "MOV r0, r1", "LSL r0", "LSR r0", "ROL r0", "ROR r0", "ASR r0", "SWAP r0", "SUBI r0, ", "SBCI r0, ", "SBCI r0, ", "ORI r0, ", "ANDI r0, ", "CPI r0, " };
        bool[] C = { false, false, false, true, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, true, false, false, false };
        String[] Datei = { "ADD", "SUB", "ADC-O", "ADC-M", "SBC-O", "SBC-M", "EOR", "OR", "AND", "COM", "NEG", "INC", "DEC", "TST", "CLR", "SER", "MUL", "CPC-O", "CP", "CPC-M", "MOV", "LSL", "LSR", "ROL", "ROR", "ASR", "SWAP", "SUB", "SBC-O", "SBC-M", "OR", "AND", "CP" };
        bool[] ONE = { false, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, false, false, false, false, false, true, true, true, true, true, true, false, false, false, false, false, false };

        private void button21_Click(object sender, EventArgs e)
        {
            label39.Hide();
            label39.Text = "---";
            List<String> Ausgabe = new List<String>();
            if (Befehl.Count() != Text2.Count() || Befehl.Count() != C.Count() || Befehl.Count() != Datei.Count() || Befehl.Count() != ONE.Count()) { Ausgabe.Add("Daten fehlerhaft!!!"); goto over; }

            for (int t = 0; t < Befehl.Count(); t++)
            {

                Atmega System = new Atmega(32, 0);
                List<String> data = new List<String>();
                List<String> Vergleich = new List<String>();
                StreamReader datei = new StreamReader("Befehle\\" + Datei[t] + ".txt");
                while (datei.Peek() != -1)
                {
                    Vergleich.Add(datei.ReadLine());
                }
                datei.Close();

                data.Add(Text2[t]);
                System.Load_Content(data, true);
                System.Selected_Software = 0;
                System.EEPROM_AKTIV = false;
                System.Timer_AKTIV = false;
                System.Szenarien_AKTIV = false;
                System.Watchdog_AKTIV = false;
                System.USART_AKTIV = false;
                int i = 0;
                for (int a = 0; (a < 1) || (!ONE[t] && a < 256); a++)
                    for (int b = 0; b < 256; b++)
                    {
                        System.Register[0] = (Byte)b;
                        System.Register[1] = (Byte)a;
                        System.SREG = 0;
                        if (C[t]) System.SREG = (Byte)(System.SREG | 1);
                        if (t >= 27) { System.Program[0][0].Param2 = a; }
                        System.Counter[0] = 0;
                        System.Sleep = 1;
                        System.Step(null);
                        if (System.SREG != Convert.ToInt32(Vergleich[i].Substring(0, 3)) || System.Register[0] != Convert.ToInt32(Vergleich[i].Substring(3, 3)) || System.Register[1] != Convert.ToInt32(Vergleich[i].Substring(6, 3)))
                        {
                            label39.Text = "Fehler";
                            label39.Show();
                            Ausgabe.Add(Befehl[t] + " (" + b.ToString() + "," + a.ToString() + "): " + Erweitern(System.SREG.ToString()) + Erweitern(System.Register[0].ToString()) + Erweitern(System.Register[1].ToString()) + "-->" + Vergleich[i]);
                        }
                        i++;
                    }

            }

        over:
            StreamWriter datei2 = new StreamWriter("Result.txt");
            for (int b = 0; b < Ausgabe.Count; b++) datei2.WriteLine(Ausgabe[b]);
            datei2.Close();

            if (Ausgabe.Count == 0) { label39.Text = "OK"; label39.Show(); }

        }

        private void button22_Click(object sender, EventArgs e)
        {
            label40.Hide();
            label40.Text = "---";
            Stopwatch Zeit = new Stopwatch();
            if (Befehl.Count() != Text2.Count() || Befehl.Count() != C.Count() || Befehl.Count() != Datei.Count() || Befehl.Count() != ONE.Count()) { return; }

            int t = 0;
            for (; t < Befehl.Count() && Befehl[t] != textBox19.Text.ToUpper(); t++) ;
            if (t == Befehl.Count()) return;

            Atmega System = new Atmega(32, 0);
            List<String> data = new List<String>();

            data.Add(Text2[t]);
            System.Load_Content(data, true);
            System.Selected_Software = 0;
            System.EEPROM_AKTIV=false;
            System.Timer_AKTIV = false;
            System.Szenarien_AKTIV = false;
            System.Watchdog_AKTIV = false;
            System.USART_AKTIV = false;
            int anz = 1000000;
            Zeit.Start();
            for (int i = 0; i < anz; )
            {

                for (int a = 0; ((a < 1) || (!ONE[t] && a < 256)) && i < anz; a++)
                    for (int b = 0; b < 256 && i < anz; b++)
                    {
                        System.Register[0] = (Byte)b;
                        System.Register[1] = (Byte)a;
                        System.SREG = 0;
                        if (C[t]) System.SREG = (Byte)(System.SREG | 1);
                        if (t >= 27) { System.Program[0][0].Param2 = a; }
                        System.Counter[0] = 0;
                        System.Sleep = 1;
                        System.Step(null);
                        i++;
                    }
            }
            Zeit.Stop();
            double fact = 1000L * 1000L * 1000L / Stopwatch.Frequency;
            label40.Text = Math.Round((double)((double)System.Watches[System.Program[0][0].Typ].ElapsedTicks * fact) / anz, 4) + " ns";
            label40.Show();

        }

        private void checkBox18_CheckedChanged(object sender, EventArgs e)
        {
            Main[sel].EEPROM_AKTIV = checkBox18.Checked;
        }

        private void checkBox17_CheckedChanged(object sender, EventArgs e)
        {
            Main[sel].Timer_AKTIV = checkBox17.Checked;
        }

        private void checkBox16_CheckedChanged(object sender, EventArgs e)
        {
            Main[sel].USART_AKTIV = checkBox16.Checked;
        }

        private void checkBox15_CheckedChanged(object sender, EventArgs e)
        {
            Main[sel].Watchdog_AKTIV = checkBox15.Checked;
        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            Main[sel].Szenarien_AKTIV = checkBox14.Checked;
        }

        private void timer6_Tick(object sender, EventArgs e)
        {
            if (ausschalten)
            {
                ausschalten = false;
            button16.Text = "Loop";
            button13.Text = "Starte Alle";
            Stop_Looping = true;
            In_Loop = false;
            }
        }


    }

}

