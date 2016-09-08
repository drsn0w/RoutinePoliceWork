using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LSPD_First_Response.Mod.API;
using Rage;

namespace RoutinePoliceWork
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial("Routine Police Work v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " has been loaded!");
            Game.LogTrivial("Go on duty.");
        }

        public override void Finally()
        {
            throw new NotImplementedException();
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                RegisterCallouts();
            }
        }

        private static void RegisterCallouts()
        {
            // Register the Loitering callout
            Functions.RegisterCallout(typeof(Callouts.Loitering));

        }
    }
}
