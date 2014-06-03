using System;
using System.Collections.Generic;

namespace ASM_Simulator
{
    public class PORT
    {
        public String Bezeichnung;
        public List<String>[] Typ; // Art
        public Byte Summe;
        public int Anz = 0;
        public List<Oszillator> Oszillatoren = new List<Oszillator>();

        public PORT(String bez, int a)
        {
            Bezeichnung = bez;
            Anz = a;
            Summe = 0;
            Generate_Ports(a);
        }

        public PORT()
        {
            Bezeichnung = "NONE";
            Anz = 8;
            Summe = 0;
            Generate_Ports(Anz);
        }

        public void set_name(String Name)
        {
            Bezeichnung = Name;
        }

        public void set_konf(int id, String typ)
        {
            Typ[id].Add(typ);
        }

        public void set_zustand(int id, bool zust)
        {
            Byte temp = (Byte)(1 << id);
            Summe = (Byte)(zust ? Summe | temp : Summe & ~temp);
        }

        public void Generate_Ports(int anz)
        {
            Typ = new List<String>[anz];
            for (int i = 0; i < anz; i++) Typ[i] = new List<String>();
            for (int q = 0; q < anz; q++) { Oszillatoren.Add(new Oszillator()); }
        }

        public void Store_Port(int Pin)
        {
            Oszillatoren[Pin].Add((Summe & (1 << Pin)) > 0 ? true : false);
        }

        public void set(Byte wert)
        {
            Summe = wert;
            return;
        }

        public void set(Byte pos, bool zustand)
        {
            Byte temp = (Byte)(1 << pos);
            Summe = (Byte)(zustand ? Summe | temp : Summe & ~temp);
        }

        public void set(int pos, bool zustand)
        {
            Byte temp = (Byte)(1 << pos);
            Summe = (Byte)(zustand ? Summe | temp : Summe & ~temp);
        }

        public Byte get()
        {
            return Summe;
        }

        public bool getBit(Byte i)
        {
            return (Summe & (1 << i)) > 0 ? true : false;
        }

        public bool getBit(int i)
        {
            return (Summe & (1 << i)) > 0 ? true : false;
        }

        public void set(int pos, int zustand)
        {
            Byte temp = (Byte)(1 << pos);
            Summe = (Byte)(zustand > 0 ? Summe | temp : Summe & ~temp);
        }
    }
}