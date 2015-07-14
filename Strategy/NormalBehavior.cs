using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;

namespace RFC.Strategy
{
    public class NormalBehavior
    {
        Team team;
        ServiceManager msngr;
        HowOffensive fieldRating;
        DefenseStrategy defenseBehavior;
        OffenseStrategy offenseBehavior;
        MidfieldPlay midfieldBehavior;

        const double defense_threshold = .20;
        const double offense_threshold = .50;
        const double hysteresis = .05;

        public enum State
        {
            Offense,
            Midfield,
            Defense,
            Unknown
        }
        State state;

        public NormalBehavior(Team team, int goalie_id)
        {
            this.team = team;
            this.state = State.Midfield;

            this.msngr = ServiceManager.getServiceManager();
            this.fieldRating = new HowOffensive(team);

            defenseBehavior = new DefenseStrategy(team, goalie_id,DefenseStrategy.PlayType.Defense);
            offenseBehavior = new OffenseStrategy(team, goalie_id);
            midfieldBehavior = new MidfieldPlay(team, goalie_id);
        }

        public State Play(FieldVisionMessage msg)
        {
            state_switcher(fieldRating.Evaluate(msg));
            Console.WriteLine("NormalBehavior: " + state);
            State currentState = State.Unknown;
            switch(state)
            {
                case State.Offense:
                    offenseBehavior.Handle(msg);
                    currentState = State.Offense;
                    break;
                case State.Midfield:
                    midfieldBehavior.doMidfield(msg);
                    currentState = State.Midfield;
                    break;
                case State.Defense:
                    defenseBehavior.DefenseCommand(msg, 1, true);
                    currentState = State.Defense;
                    break;
            }
            return currentState;
        }

        // switched state with some hysteresis
        private void state_switcher(double score)
        {
            
            switch(state)
            {
                case State.Offense:
                    if (score < defense_threshold)
                        state = State.Defense;
                    else if (score < offense_threshold - hysteresis)
                        state = State.Midfield;
                    break;
                case State.Midfield:
                    if (score > offense_threshold + hysteresis)
                    {
                        state = State.Offense;
                        offenseBehavior.reset();
                    }
                        
                    else if (score < defense_threshold - hysteresis)
                        state = State.Defense;
                    break;
                case State.Defense:
                    if (score > offense_threshold)
                    {
                        state = State.Offense;
                        offenseBehavior.reset();
                    }
                    else if (score > defense_threshold + hysteresis)
                        state = State.Midfield;
                    break;
            }
            Console.WriteLine(state);
        }
    }
}
