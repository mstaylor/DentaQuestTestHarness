using System;

namespace DentaQuestTestHarness
{
    public static class TestTransactions
    {
        /// <summary>
        /// Sample 270 (Eligibility Inquiry) transaction for testing
        /// </summary>
        public static string GetSample270Transaction()
        {
            return "ISA*00*          *00*          *ZZ*TESTTRADER      *ZZ*DENTAQUEST      *210507*1430*^*00501*000000001*0*T*:~" +
                   "GS*HS*TESTTRADER*DENTAQUEST*20210507*1430*1*X*005010X279A1~" +
                   "ST*270*0001*005010X279A1~" +
                   "BHT*0022*13*10001234*20210507*1430~" +
                   "HL*1**20*1~" +
                   "PRV*BI*PXC*123456789~" +
                   "NM1*PR*2*DENTAQUEST*****PI*DENTAQUEST~" +
                   "HL*2*1*21*1~" +
                   "NM1*1P*2*TEST DENTAL OFFICE*****XX*1234567890~" +
                   "HL*3*2*22*0~" +
                   "TRN*1*1*1234567890~" +
                   "NM1*IL*1*DOE*JOHN*A***MI*123456789~" +
                   "DMG*D8*19800101*M~" +
                   "EQ*30~" +
                   "SE*12*0001~" +
                   "GE*1*1~" +
                   "IEA*1*000000001~";
        }

        /// <summary>
        /// Sample 276 (Claim Status Inquiry) transaction for testing
        /// </summary>
        public static string GetSample276Transaction()
        {
            return "ISA*00*          *00*          *ZZ*TESTTRADER      *ZZ*DENTAQUEST      *210507*1430*^*00501*000000002*0*T*:~" +
                   "GS*HI*TESTTRADER*DENTAQUEST*20210507*1430*2*X*005010X214~" +
                   "ST*276*0001*005010X214~" +
                   "BHT*0010*13*10001235*20210507*1430~" +
                   "HL*1**20*1~" +
                   "PRV*BI*PXC*123456789~" +
                   "NM1*PR*2*DENTAQUEST*****PI*DENTAQUEST~" +
                   "HL*2*1*21*1~" +
                   "NM1*41*2*TEST DENTAL OFFICE*****XX*1234567890~" +
                   "HL*3*2*19*1~" +
                   "NM1*1P*2*TEST PROVIDER*****XX*9876543210~" +
                   "HL*4*3*PT*0~" +
                   "NM1*QC*1*DOE*JANE*B***MI*987654321~" +
                   "TRN*1*12345*1234567890~" +
                   "REF*1K*CLM123456~" +
                   "DTP*472*D8*20210501~" +
                   "AMT*T3*150.00~" +
                   "SE*14*0001~" +
                   "GE*1*2~" +
                   "IEA*1*000000002~";
        }

        /// <summary>
        /// Validates that EDI content doesn't contain CRLF (as required by DentaQuest)
        /// </summary>
        public static string CleanEdiContent(string ediContent)
        {
            return ediContent?.Replace("\r", "").Replace("\n", "") ?? string.Empty;
        }
    }
}