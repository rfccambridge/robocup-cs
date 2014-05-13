/******************************************************************************
 * C# Joystick Library - Copyright (c) 2006 Mark Harris - MarkH@rris.com.au
 ******************************************************************************
 * You may use this library in your application, however please do give credit
 * to me for writing it and supplying it. If you modify this library you must
 * leave this notice at the top of this file. I'd love to see any changes you
 * do make, so please email them to me :)
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.DirectInput;
using System.Diagnostics;

namespace JoystickInterface
{
    /// <summary>
    /// Class to interface with a joystick device.
    /// </summary>
    public class JoystickWrapper
    {
        private Joystick joystick;
        private JoystickState state;

        private int axisCount;
        /// <summary>
        /// Number of axes on the joystick.
        /// </summary>
        public int AxisCount
        {
            get { return axisCount; }
        }
        
        private int axisA;
        /// <summary>
        /// The first axis on the joystick.
        /// </summary>
        public int AxisA
        {
            get { return axisA; }
        }
        
        private int axisB;
        /// <summary>
        /// The second axis on the joystick.
        /// </summary>
        public int AxisB
        {
            get { return axisB; }
        }
        
        private int axisC;
        /// <summary>
        /// The third axis on the joystick.
        /// </summary>
        public int AxisC
        {
            get { return axisC; }
        }
        
        private int axisD;
        /// <summary>
        /// The fourth axis on the joystick.
        /// </summary>
        public int AxisD
        {
            get { return axisD; }
        }
        
        private int axisE;
        /// <summary>
        /// The fifth axis on the joystick.
        /// </summary>
        public int AxisE
        {
            get { return axisE; }
        }
        
        private int axisF;
        /// <summary>
        /// The sixth axis on the joystick.
        /// </summary>
        public int AxisF
        {
            get { return axisF; }
        }
        private System.Windows.Forms.Control control;

        private bool[] buttons;
        /// <summary>
        /// Array of buttons availiable on the joystick. This also includes PoV hats.
        /// </summary>
        public bool[] Buttons
        {
            get { return buttons; }
        }

        private string[] systemJoysticks;

        /// <summary>
        /// Constructor for the class.
        /// </summary>
        /// <param name="conrol">Windowed control which the joystick will be "attached" to.</param>
        public JoystickWrapper(System.Windows.Forms.Control control)
        {
            this.control = control;
            axisA = -1;
            axisB = -1;
            axisC = -1;
            axisD = -1;
            axisE = -1;
            axisF = -1;
            axisCount = 0;
        }

        private void Poll()
        {
            try
            {
                // poll the joystick
                joystick.Poll();
                // update the joystick state field
                state = joystick.GetCurrentState();
            }
            catch (Exception err)
            {
                // we probably lost connection to the joystick
                // was it unplugged or locked by another application?
                Debug.WriteLine("Poll()");
                Debug.WriteLine(err.Message);
                Debug.WriteLine(err.StackTrace);
            }
        }

        /// <summary>
        /// Retrieves a list of joysticks attached to the computer.
        /// </summary>
        /// <example>
        /// [C#]
        /// <code>
        /// JoystickInterface.Joystick jst = new JoystickInterface.Joystick(this.Handle);
        /// string[] sticks = jst.FindJoysticks();
        /// </code>
        /// </example>
        /// <returns>A list of joysticks as an array of strings.</returns>
        public string[] FindJoysticks()
        {
            systemJoysticks = null;

            try
            {
                // Find all the GameControl devices that are attached.
                DirectInput dinput = new DirectInput();

                IList<DeviceInstance> gameControllerList = dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);

                // check that we have at least one device.
                if (gameControllerList.Count > 0)
                {
                    systemJoysticks = new string[gameControllerList.Count];
                    int i = 0;
                    // loop through the devices.
                    foreach (DeviceInstance deviceInstance in gameControllerList)
                    {
                        // create a device from this controller so we can retrieve info.
                        joystick = new Joystick(dinput, deviceInstance.InstanceGuid);
                        joystick.SetCooperativeLevel(control, CooperativeLevel.Background | CooperativeLevel.Nonexclusive);

                        systemJoysticks[i] = joystick.Information.InstanceName;

                        i++;
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("FindJoysticks()");
                Console.WriteLine(err.Message);
                Console.WriteLine(err.StackTrace);
            }

            return systemJoysticks;
        }

        /// <summary>
        /// Acquire the named joystick. You can find this joystick through the <see cref="FindJoysticks"/> method.
        /// </summary>
        /// <param name="name">Name of the joystick.</param>
        /// <returns>The success of the connection.</returns>
        public bool AcquireJoystick(string name)
        {
            try
            {
                DirectInput dinput = new DirectInput();

                IList<DeviceInstance> gameControllerList = dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);
                int i = 0;
                bool found = false;
                // loop through the devices.
                foreach (DeviceInstance deviceInstance in gameControllerList)
                {
                    if (deviceInstance.InstanceName == name)
                    {
                        found = true;
                        // create a device from this controller so we can retrieve info.
                        joystick = new Joystick(dinput, deviceInstance.InstanceGuid);
                        joystick.SetCooperativeLevel(control, CooperativeLevel.Background | CooperativeLevel.Nonexclusive);
                        break;
                    }

                    i++;
                }

                if (!found)
                    return false;
                
                // Finally, acquire the device.
                joystick.Acquire();

                // How many axes?
                // Find the capabilities of the joystick
                Capabilities cps = joystick.Capabilities;
                Debug.WriteLine("Joystick Axis: " + cps.AxesCount);
                Debug.WriteLine("Joystick Buttons: " + cps.ButtonCount);

                axisCount = cps.AxesCount;

                UpdateStatus();
            }
            catch (Exception err)
            {
                Debug.WriteLine("FindJoysticks()");
                Debug.WriteLine(err.Message);
                Debug.WriteLine(err.StackTrace);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Unaquire a joystick releasing it back to the system.
        /// </summary>
        public void ReleaseJoystick()
        {
            joystick.Unacquire();
        }

        /// <summary>
        /// Update the properties of button and axis positions.
        /// </summary>
        public void UpdateStatus()
        {
            Poll();

            int[] extraAxis = state.GetForceSliders();
            //Rz Rx X Y Axis1 Axis2
            axisA = state.RotationZ;
            axisB = state.RotationX;
            axisC = state.X;
            axisD = state.Y;
            axisE = extraAxis[0];
            axisF = extraAxis[1];

            buttons = state.GetButtons();
        }
    }
}
