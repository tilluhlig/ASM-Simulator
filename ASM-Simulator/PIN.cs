using System;

namespace ASM_Simulator
{
    public class PIN
    {
        public String PORTNAME;
        public int ID;
        public int PORT;

        public PIN()
        {
            PORTNAME = "";
            ID = 0;
            PORT = 0;
        }

        public PIN(int PinID, String PortName, int PORTID)
        {
            PORTNAME = PortName;
            ID = PinID;
            PORT = PORTID;
        }
    }
}