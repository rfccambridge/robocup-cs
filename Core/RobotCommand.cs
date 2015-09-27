using System;
using System.Collections.Generic;
using System.Text;
using RFC.Utilities;
using RFC.Core;
using RFC.InterProcessMessaging;
using System.Linq;

namespace RFC.Core
{
    public abstract class RobotCommand : IByteSerializable<RobotCommand>
    {
        public enum Command
        {
            MOVE,                       // Send wheelspeeds for 4 wheels
            KICK,                       // Charge & plunge, regardless of break-beam
            START_CHARGING,             // Start charging to full capacitor voltage (XXX: deprecated)
            //START_VARIABLE_CHARGING,    // Start charging to a set target voltage
            //STOP_CHARGING,              // Stop charging and maintain charge
            //BREAKBEAM_KICK,             // Charge to full voltage & plunge whenever beam broken (XXX: deprecated)
            FULL_BREAKBEAM_KICK,        // Charge to target voltage & plunge whenever targetV reached and beam broken
            //MIN_BREAKBEAM_KICK,         // Charge to target voltage & plunge whenever V > minV and beam broken
            START_DRIBBLER,             // Start the dribbler with a set speed
            STOP_DRIBBLER,              // Stop the dribbler
            //DISCHARGE,                  // Discharge capacitors without plunging
            //RESET,                      // Reset brushless board PIC
            //SET_PID,                    // Set PID constants for single-speed control (TOOD: check if deprecated?)
            //SET_CFG_FLAGS               // Set configuration flags
        };

        static CRCTool _crcTool;

        // Equal to capacitor voltage we charge to / 10

        public const int MAX_KICKER_STRENGTH = 25;
        public const int MIN_KICKER_STRENGTH = 1;

        public int ID;
        public abstract byte Source { get; }
        public abstract byte Port { get; }

        protected byte serialId => (byte)('0' + ID);

        static private Dictionary<Command, Type> enumToTypeMap;


        #region Brushless board commands
        public abstract class OtherBoardCommand : RobotCommand
        {
            public override byte Source => (byte)'w';
            public OtherBoardCommand(int ID) : base(ID) { }
            public override byte[] ToPacket()
            {
                return base.createPacket(Constants.RadioProtocol.SEND_BRUSHLESSBOARD_CHECKSUM, serialId, Source, Port);
            }
        }

        public sealed class MoveCommand : OtherBoardCommand
        {
            public override byte Port => (byte)'w';
            public static readonly Command command = Command.MOVE;

            public WheelSpeeds Speeds;

            public MoveCommand(int ID, WheelSpeeds speeds) : base(ID)
            {
                Speeds = speeds;
            }

            private int clampWheelSpeed(double speed)
            {
                const int MAXSPEED = 127;
                int s = (int)Math.Round((double)speed);
                if (s > MAXSPEED)
                    s = MAXSPEED;
                if (s < -MAXSPEED)
                    s = -MAXSPEED;
                return s;
            }

            protected override void serializeData(ref byte[] buff)
            {
                int lf = clampWheelSpeed(Speeds.lf),
                           rf = clampWheelSpeed(Speeds.rf),
                           lb = clampWheelSpeed(Speeds.lb),
                           rb = clampWheelSpeed(Speeds.rb);

                buff[4] = (byte)(Speeds.rf < 0 ? (byte)Math.Abs(rf) | 0x80 : (byte)rf);
                buff[5] = (byte)(Speeds.lf < 0 ? (byte)Math.Abs(lf) | 0x80 : (byte)lf);
                buff[6] = (byte)(Speeds.lb < 0 ? (byte)Math.Abs(lb) | 0x80 : (byte)lb);
                buff[7] = (byte)(Speeds.rb < 0 ? (byte)Math.Abs(rb) | 0x80 : (byte)rb);
            }

            protected override void deserializeData(ref byte[] buff)
            {
                Speeds = new WheelSpeeds((buff[4] & 0x80) > 0 ? -1 * (buff[4] & 0x7F) : buff[4],
                                         (buff[5] & 0x80) > 0 ? -1 * (buff[5] & 0x7F) : buff[5],
                                         (buff[6] & 0x80) > 0 ? -1 * (buff[6] & 0x7F) : buff[6],
                                         (buff[7] & 0x80) > 0 ? -1 * (buff[7] & 0x7F) : buff[7]);
            }

            public override byte[] ToPacket()
            {
                int lf = clampWheelSpeed(Speeds.lf),
                        rf = clampWheelSpeed(Speeds.rf),
                        lb = clampWheelSpeed(Speeds.lb),
                        rb = clampWheelSpeed(Speeds.rb);

                // board bugs out if we send an unescaped slash
                if (lb == '\\') lb++;
                if (lf == '\\') lf++;
                if (rf == '\\') rf++;
                if (rb == '\\') rb++;

                //Console.WriteLine("id " + ID + ": setting speeds to: " + Speeds.ToString());

                //robots expect wheel powers in this order:
                //rf lf lb rb                                       
                
                return createPacket(
                    Constants.RadioProtocol.SEND_BRUSHLESSBOARD_CHECKSUM, serialId, Source, Port,
                    (byte)rf, (byte)lf, (byte)lb, (byte)rb);
            }
        }

        /*
        public sealed class SetCfgFlagsCommand : OtherBoardCommand
        {
            public static readonly Command command = Command.SET_CFG_FLAGS;
            public override byte Port => (byte)'c';

            public readonly byte BoardID;
            public readonly byte Flags;
            public SetCfgFlagsCommand(int ID, byte boardID, byte flags) : base(ID)
            {
                Flags = flags;
                BoardID = boardID;
            }
            
            public override byte[] ToPacket() => createPacket(Constants.RadioProtocol.SEND_BRUSHLESSBOARD_CHECKSUM, serialId, Source, Port,
                BoardID, Flags);
        }
        public sealed class ResetCommand : OtherBoardCommand
        {
            public static readonly Command command = Command.RESET;
            public override byte Port => (byte)'r';

            public ResetCommand(int ID) : base(ID) { }
        }
        */
        #endregion

        #region Aux board commands
        public abstract class AuxBoardCommand : RobotCommand
        {
            public override byte Source => (byte)'v';
            public AuxBoardCommand(int ID) : base(ID) { }
            public override byte[] ToPacket()
            {
                return base.createPacket(Constants.RadioProtocol.SEND_AUXBOARD_CHECKSUM, serialId, Source, Port);
            }
        }

        public sealed class KickCommand : AuxBoardCommand
        {
            public static readonly Command command = Command.KICK;
            public override byte Port => (byte)'k';

            public KickCommand(int ID) : base(ID) { }
            public override byte[] ToPacket() =>
                createPacket(Constants.RadioProtocol.SEND_AUXBOARD_CHECKSUM, serialId, Source, Port, (byte)0xff, (byte)0);
        }

        public sealed class StartDribblerCommand : AuxBoardCommand
        {
            public static readonly Command command = Command.START_DRIBBLER;
            public override byte Port => (byte)'d';

            public byte DribblerSpeed;
            public StartDribblerCommand(int ID, byte dribblerSpeed = 5) : base(ID) { DribblerSpeed = dribblerSpeed; }
            public override byte[] ToPacket() => createPacket(Constants.RadioProtocol.SEND_AUXBOARD_CHECKSUM, serialId, Source, Port, (byte)('0' + DribblerSpeed));

            protected override void serializeData(ref byte[] buff)   { buff[4] = DribblerSpeed; }
            protected override void deserializeData(ref byte[] buff) { DribblerSpeed = buff[4]; }
        }
        public sealed class StopDribblerCommand : AuxBoardCommand
        {
            public static readonly Command command = Command.STOP_DRIBBLER;
            public override byte Port => (byte)'d';

            public StopDribblerCommand(int ID) : base(ID) { }
            public override byte[] ToPacket() => createPacket(Constants.RadioProtocol.SEND_AUXBOARD_CHECKSUM, serialId, Source, Port, (byte)'0');
        }
        /*
        public sealed class StartVariableChargingCommand : AuxBoardCommand
        {
            public static readonly Command command = Command.START_VARIABLE_CHARGING;
            public override byte Port => (byte)'v';

            public StartVariableChargingCommand(int ID) : base(ID) { }
            public override byte[] ToPacket()
            {
                if (KickerStrength > MAX_KICKER_STRENGTH) KickerStrength = MAX_KICKER_STRENGTH;
                if (KickerStrength < MIN_KICKER_STRENGTH) KickerStrength = MIN_KICKER_STRENGTH;
                return createPacket(Constants.RadioProtocol.SEND_AUXBOARD_CHECKSUM, serialId, Source, Port, (byte)KickerStrength);
            }
        }
        */
        public sealed class FullBreakbeamKickCommand : AuxBoardCommand
        {
            public static readonly Command command = Command.FULL_BREAKBEAM_KICK;
            public override byte Port => (byte)'k';

            public byte KickerStrength;

            public FullBreakbeamKickCommand(int ID, byte kickerStrength = MAX_KICKER_STRENGTH) : base(ID)
            {
                KickerStrength = kickerStrength;
            }
            public override byte[] ToPacket()
            {
                if (KickerStrength > MAX_KICKER_STRENGTH) KickerStrength = MAX_KICKER_STRENGTH;
                if (KickerStrength < MIN_KICKER_STRENGTH) KickerStrength = MIN_KICKER_STRENGTH;

                return createPacket(Constants.RadioProtocol.SEND_AUXBOARD_CHECKSUM, serialId, Source, Port, (byte)KickerStrength, (byte)(1));
            }

            protected override void serializeData(ref byte[] buff) { buff[4] = KickerStrength; }
            protected override void deserializeData(ref byte[] buff) { KickerStrength = buff[4]; }
        }

        /*
        public sealed class MinBreakBeamKickCommand : AuxBoardCommand
        {
            public static readonly Command command = Command.MIN_BREAKBEAM_KICK;
            public override byte Port => (byte)'m';

            public MinBreakBeamKickCommand(int ID) : base(ID) { }
            public override byte[] ToPacket()
            {
                if (KickerStrength > MAX_KICKER_STRENGTH) KickerStrength = MAX_KICKER_STRENGTH;
                if (KickerStrength < MIN_KICKER_STRENGTH) KickerStrength = MIN_KICKER_STRENGTH;
                if (MinKickerStrength > MAX_KICKER_STRENGTH) MinKickerStrength = MAX_KICKER_STRENGTH;
                if (MinKickerStrength < MIN_KICKER_STRENGTH) MinKickerStrength = MIN_KICKER_STRENGTH;

                return createPacket(Constants.RadioProtocol.SEND_AUXBOARD_CHECKSUM, serialId, Source, Port, (byte)KickerStrength, (byte)MinKickerStrength);
            }
        }
        public sealed class StartChargingCommand : AuxBoardCommand
        {
            public static readonly Command command = Command.MIN_BREAKBEAM_KICK;
            public override byte Port => (byte)'c';

            public StartChargingCommand(int ID) : base(ID) { }
        }

        public sealed class StopChargingCommand : AuxBoardCommand
        {
            public static readonly Command command = Command.STOP_CHARGING_COMMAND;
            public override byte Port => (byte)'s';

            public StopChargingCommand(int ID) : base(ID) { }
        }
        public sealed class BreakbeamKickCommand : AuxBoardCommand
        {
            public static readonly Command command = Command.BREAKBEAM_KICK;
            public override byte Port => (byte)'b';

            public BreakbeamKickCommand(int ID) : base(ID) { }
            public override byte[] ToPacket() => createPacket(Constants.RadioProtocol.SEND_AUXBOARD_CHECKSUM, serialId, Source, Port);
        }
        public sealed class DischargeCommand : AuxBoardCommand
        {
            public static readonly Command command = Command.DISCHARGE;
            public override byte Port => (byte)'p';

            public DischargeCommand(int ID) : base(ID) { }
            public override byte[] ToPacket() => createPacket(Constants.RadioProtocol.SEND_AUXBOARD_CHECKSUM, serialId, Source, Port);
        }*/
        #endregion

        static RobotCommand()
        {
            _crcTool = new CRCTool();
            _crcTool.Init(CRCTool.CRCCode.CRC8);

            
            enumToTypeMap = (
                 from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                 from assemblyType in domainAssembly.GetTypes()
                 where typeof(RobotCommand).IsAssignableFrom(assemblyType) && assemblyType.IsSealed
                 select assemblyType
            ).ToDictionary(
                x => (Command) x.GetField("command", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null),
                x => x
            );

        }

        protected RobotCommand(int ID)
        {
            this.ID = ID;
        }

        /// <summary>
        /// Create a packet follwing the EE/CS protocol
        /// </summary>
        /// <param name="use_checksum">Send a checksum after the header bytes</param>
        /// <param name="values">Packet payload</param>
        /// <returns>Ready-to-transmit packet</returns>
        private byte[] createPacket(bool use_checksum, params byte[] values)
        {
            List<byte> curr_packet = new List<byte>();
            // add header
            curr_packet.Add((byte)'\\');
            curr_packet.Add((byte)'H');

            // add checksum byte if necessary
            if (use_checksum)
            {
                byte checksum = Checksum.Compute(values);
                curr_packet.Add(checksum);
            }

            // add the actual packet
            curr_packet.AddRange(values);

            // add footer
            curr_packet.Add((byte)'\\');
            curr_packet.Add((byte)'E');

            return curr_packet.ToArray();
        }

        public abstract byte[] ToPacket();

        #region IByteSerializable<RobotCommand> Members

        // Serialized format: 
        // Byte 0: Command
        // Byte 1: ID
        // Byte 2-3: <unused>
        // Byte 4-7: Data


        const int SERIALIZED_LENGTH = 8; // bytes
        protected virtual void deserializeData(ref byte[] buff) { };
        public RobotCommand Deserialize(System.Net.Sockets.NetworkStream stream)
        {
            byte[] buff = new byte[SERIALIZED_LENGTH]; // I don't think we need to lock this
  
            int read = stream.Read(buff, 0, buff.Length);
            if (read > 0 && read != buff.Length)
            {
                throw new ApplicationException(String.Format("RobotCommand.Deserialize: read {0:G} " +
                    "but expecting {1:G}.", read, buff.Length));
            }
            Command command = (Command) buff[0];
            byte ID = buff[1];
            RobotCommand c = (RobotCommand) enumToTypeMap[command].GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { ID });

            c.deserializeData(ref buff);
            return c;
        }

        protected virtual void serializeData(ref byte[] buff) { };
        public void Serialize(System.Net.Sockets.NetworkStream stream)
        {
            byte[] buff = new byte[SERIALIZED_LENGTH]; // I don't think we need to lock this
            buff.Initialize(); // Clear

            buff[0] = (byte)command;
            buff[1] = (byte)ID;
            serializeData(ref buff);

            stream.Write(buff, 0, buff.Length);
        }

        #endregion
    }
}
