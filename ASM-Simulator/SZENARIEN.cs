using System;
using System.Collections.Generic;

namespace ASM_Simulator
{
    public class SZENARIEN
    {
        public List<EVENT> Events = new List<EVENT>();
        public Atmega Vater = null;
        public int Pos = 0;
        public int Time = 1;
        public bool Aktiv = false;
        public bool Loop = false;
        public int Replays = 0;
        public String Name = "";

        public SZENARIEN(Atmega Besitzer)
        {
            Name = "[None]";
            Vater = Besitzer;
        }

        public void Reset()
        {
            Pos = 0;
            Time = 1;
            Aktiv = false;
            for (int i = 0; i < Events.Count; i++) Events[i].Reset();
        }

        public void AddLine()
        {
            Events.Add(new EVENT());
        }

        public void Start(bool Endlos, int Wiederholungen)
        {
            // Reset();
            Aktiv = true;
            Loop = Endlos;
            Replays = Wiederholungen;
            Pos = 0;
            Time = 1;
        }

        public void Stop()
        {
            Aktiv = false;
            Replays = 0;
            Pos = 0;
            Loop = false;
            Time = 1;
        }

        public void Update()
        {
            if (!Aktiv && !Loop) return;

            if (Pos < Events.Count)
            {
                if (!Events[Pos].setted) { Events[Pos].setted = true; Events[Pos].Reset(); }
                Time++;
                if (Events[Pos].Time <= Time)
                {
                    Events[Pos].Execute();
                    Events[Pos].setted = false;
                    Pos++;
                    Time = 0;

                    if (Pos >= Events.Count)
                    {
                        if (Replays > 0)
                        {
                            Replays--;
                            //Reset();
                            Start(Loop, Replays);
                        }
                        else
                            if (Loop)
                            {
                                // Reset();
                                Start(Loop, Replays);
                            }
                            else
                                Stop();
                    }

                    Update();
                }
            }
            else
            {
                if (Replays > 0)
                {
                    Replays--;
                    //Reset();
                    Start(Loop, Replays);
                }
                else
                    if (Loop)
                    {
                        // Reset();
                        Start(Loop, Replays);
                    }
                    else
                        Stop();
            }
        }
    }
}