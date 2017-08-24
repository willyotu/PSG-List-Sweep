using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agilent.AgilentRfSigGen.Interop;
using Ivi.RFSigGen.Interop;

namespace PSG_List_Sweep
{
    /// <summary>
    /// AgilentRfSigGen IVI-COM Driver Example Program
    /// 
    /// Creates a driver object, reads a few Identity interface properties, and checks the instrument error queue.
    /// May include additional instrument specific functionality.
    /// 
    /// See driver help topic "Programming with the IVI-COM Driver in Various Development Environments"
    /// for additional programming information.
    ///
    /// Runs in simulation mode without an instrument.
    /// 
    /// Requires a reference to the driver's interop or COM type library.
    /// 
    /// </summary>
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("  CS_Example1");
            Console.WriteLine();
            AgilentRfSigGen driver = null;


            try
            {
                // Create driver instance
                driver = new AgilentRfSigGen();

                // Class compliant interface (implemented by Agilent's interface)

                IIviRFSigGen drvr = (IIviRFSigGen)driver;


                // Edit resource and options as needed.  Resource is ignored if option Simulate=true
                string resourceDesc = "TCPIP0::10.114.110.55::inst0::INSTR";
                //resourceDesc = "TCPIP0::<ip or hostname>::INSTR";

                string initOptions = "QueryInstrStatus=true, Simulate=false, DriverSetup= Model=, Trace=false, TraceName=c:\\temp\\traceOut";

                bool idquery = true;
                bool reset = true;

                // Initialize the driver.  See driver help topic "Initializing the IVI-COM Driver" for additional information
                driver.Initialize(resourceDesc, idquery, reset, initOptions);
                Console.WriteLine("Driver Initialized");


                //  Exercise driver methods and properties
                Console.WriteLine("Presetting the source.");
                driver.Utility.Reset();

                Console.WriteLine("Setting output signal to 1GHz/0dBm");
                driver.RF.Frequency = 1E9;      // set frequency to 1GHz
                driver.RF.Level = 0;            // set level to 0dBm
                driver.RF.OutputEnabled = true; // output on

                Console.WriteLine("Frequecy=" + driver.RF.Frequency);
                Console.WriteLine("Level=" + driver.RF.Level);
                Console.WriteLine("OutputEnabled=" + driver.RF.OutputEnabled);

                SetupListSweep(drvr);

                // Check instrument for errors
                int errorNum = -1;
                string errorMsg = null;
                Console.WriteLine();
                while (errorNum != 0)
                {
                    driver.Utility.ErrorQuery(ref errorNum, ref errorMsg);
                    Console.WriteLine("ErrorQuery: {0}, {1}", errorNum, errorMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (driver != null && driver.Initialized)
                {
                    // Close the driver
                    driver.Close();
                    Console.WriteLine("Driver Closed");
                }
            }

            Console.WriteLine("Done - Press Enter to Exit");
            Console.ReadLine();
        }
        
        /// <summary>
        /// Sets up List Sweeps
        /// </summary>
        static void SetupListSweep(IIviRFSigGen drvr)
        {
            Console.WriteLine("Setting up LIST SWEEP");

            try
            {
                Console.WriteLine("Create lists and set up sweep parameters");

                string listName = "ivilist";
               
               
                double[] sourceFreqs = new double[50];
                double[] sourceAmpls = new double[50];
                
                for (int i = sourceAmpls.GetLowerBound(0); i <= sourceAmpls.GetUpperBound(0); i++)
                {
                    sourceAmpls[i] = -20.0 + 0.1 * i;
                }
                for (int i = sourceFreqs.GetLowerBound(0); i <= sourceFreqs.GetUpperBound(0); i++)
                {
                    sourceFreqs[i] = 2e6 + 40e6 * ((double)i);
                }

                double[] dwelltime = new [] { 0.01, 0.02, 0.03, 0.04, 0.05 };

                drvr.Sweep.List.Reset();
               
                drvr.Sweep.List.CreateFrequencyPower(listName, ref sourceFreqs, ref sourceAmpls);
                drvr.Sweep.List.ConfigureDwell(false, dwelltime[0]);
                drvr.Sweep.TriggerSource = IviRFSigGenSweepTriggerSourceEnum.IviRFSigGenSweepTriggerSourceImmediate;

           
                Console.WriteLine("Select list to sweep freq & power");
                drvr.Sweep.List.SelectedName = listName;

                Console.WriteLine("Turn sweep off");
                drvr.Sweep.Mode = IviRFSigGenSweepModeEnum.IviRFSigGenSweepModeNone;
                Console.WriteLine("Turn sweep on");
                drvr.Sweep.Mode = IviRFSigGenSweepModeEnum.IviRFSigGenSweepModeList;
                foreach (double value in dwelltime)
                {
                    drvr.Sweep.List.Dwell = value; 
                }
                // read selected properties
                string selList = drvr.Sweep.List.SelectedName;
                bool ssEnable = drvr.Sweep.List.SingleStepEnabled;
               // double dwell = drvr.Sweep.List.Dwell;

                // clean up
                drvr.Sweep.Mode = IviRFSigGenSweepModeEnum.IviRFSigGenSweepModeNone;
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException Error:\n  " + e.Message);
            }
        }
    
       
    }
}
