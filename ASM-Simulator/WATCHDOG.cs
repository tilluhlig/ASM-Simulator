namespace ASM_Simulator
{
    // Simuliert einen Watchdog
    public class WATCHDOG
    {
        private int Sleep;
        private int Max = 0;

        public void Reset(Atmega Main)
        {
            int[] Prescaler = { 16384, 32768, 65536, 131072, 262144, 524288, 1048576, 2097152 };
            Max = Prescaler[(Main.GetBitIOPort(Main.INC.WDTCR, Main.INC.WDP0) ? 1 : 0) + (Main.GetBitIOPort(Main.INC.WDTCR, Main.INC.WDP1) ? 2 : 0) + (Main.GetBitIOPort(Main.INC.WDTCR, Main.INC.WDP2) ? 4 : 0)] * (Main.Frequenz / 1000000);
        }

        public void update(Atmega Main, Def INC)
        {
            if (Main.GetBitIOPort(INC.WDTCR, INC.WDE))
            {
                if (Sleep < Max) Sleep++;
                if (Sleep >= Max)
                {
                    // Zuende
                    Reset(Main);
                    Main.ExecuteInterrupt("RESET");
                }
            }
            else
                Sleep = 0;
        }
    }
}