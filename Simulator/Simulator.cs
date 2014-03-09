using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Simulator
{
    public class Simulator
    {
        const bool DO_REF = true;
        const string REF_IP = "";
        const int REF_PORT = 0;
        const string VISION_IP = "";
        const int VISION_PORT = 0;
        const int NUM_ROBOTS = 6;
        const bool SHOOTOUT = false;
        const bool NOISY_VISION = false;

        private bool _simRunning = false;
        private PhysicsEngine _physicsEngine = new PhysicsEngine();

        public Simulator()
        {
        }

        public void Start()
        {
            try
            {
                if (!_simRunning)
                {
                    // For convenience reload constants on every restart
                    //ConstantsRaw.Load();
                    _physicsEngine.LoadConstants();


                    SimulatedScenario normal = new NormalGameScenario("Normal game", _physicsEngine);
                    SimulatedScenario shootout = new ShootoutGameScenario("Shootout", _physicsEngine);
                    if (SHOOTOUT)
                        _physicsEngine.SetScenario(shootout);
                    else
                        _physicsEngine.SetScenario(normal);

                    _physicsEngine.SetNoisyVision(NOISY_VISION);

                    try
                    {
                        _physicsEngine.StartVision(VISION_IP, VISION_PORT);

                        if (DO_REF)
                            _physicsEngine.StartReferee(REF_IP, REF_PORT);
                    }
                    catch (System.Net.Sockets.SocketException sock_exc)
                    {
                        
                        return;
                    }


                    _physicsEngine.Start(NUM_ROBOTS, NUM_ROBOTS);

                    _simRunning = true;
                }
            }
            catch (ApplicationException ex)
            {

            }
        }

        public void Stop() {
            try {
                if(_simRunning)
                {
                    _physicsEngine.Stop();
                    
                    if(DO_REF)
                        _physicsEngine.StopReferee();
                    
                    _physicsEngine.StopVision();

                    _simRunning = false;
                }
            }
            catch (ApplicationException except)
            {
                return;
            }
        }

        public void Reset()
        {
            _physicsEngine.ResetScenarioScene();
        }
    }
}
