using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace RoutinePoliceWork.Callouts
{
    [CalloutInfo("Loitering", CalloutProbability.High)]
    public class Loitering : Callout
    {
        // Probability values
        private int PROBABILITY_SUSPECT_EXISTS = 90; 
        private int PROBABILITY_SUSPECT_WANTED = 45;
        private int PROBABILITY_SUSPECT_HOSTILE = 0;
        private int PROBABILITY_SUSPECT_RUNNER = 20;

        // Instance variables for situation
        private Ped SuspectPed;
        private Vector3 SpawnPoint;
        private Blip SuspectBlip;
        private Blip RadiusBlip;
        private LHandle Pursuit;
        private bool ShouldSayEndingAudio = false;

        // Situation determining variables
        private bool SuspectExists = false;
        private bool SuspectIsWanted = false;
        private bool SuspectIsHostile = false;
        private bool SuspectIsRunner = false;

        // Internal processing variables
        private bool DismissNotifDisplayed = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            // Create a spawn point near the player (for now)

            SpawnPoint = new Vector3(1398.15F, 3598.32F, 35.007F);

            // Show some blipperoonies
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(20f, SpawnPoint);

            // Set some LSPDFR shits and giggles
            CalloutMessage = "Citizen Loitering";
            CalloutPosition = SpawnPoint;

            // Gotta have our lady friend dispatch this shit
            Functions.PlayScannerAudioUsingPosition("WE_HAVE ASSISTANCE_REQUIRED IN_OR_ON_POSITION", SpawnPoint);

            // important!
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            // Yay!
            ShouldSayEndingAudio = true;
            // Determine our scenario
            Random random = new Random();
            // Does the suspect even exist?
            if (random.Next(0, 101) < PROBABILITY_SUSPECT_EXISTS) SuspectExists = true;
            // Is the suspect wanted?
            if (random.Next(0, 101) < PROBABILITY_SUSPECT_WANTED) SuspectIsWanted = true;
            // Is the suspect hostile?
            if (random.Next(0, 101) < PROBABILITY_SUSPECT_HOSTILE) SuspectIsHostile = true;
            // Is the suspect a runner?
            if (random.Next(0, 101) < PROBABILITY_SUSPECT_RUNNER) SuspectIsRunner = true;

            // Create a person... if there's even a person
            if (SuspectExists)
            {
                // Create a person if there's supposed to be one
                SuspectPed = new Ped(SpawnPoint);

                // Make him not disappear
                SuspectPed.IsPersistent = true;
                SuspectPed.BlockPermanentEvents = true;

                // Place a blip on him!
                SuspectBlip = SuspectPed.AttachBlip();
            } else
            {
                // There's not supposed to be a person! lmao just create a blip!
                SuspectBlip = new Blip(SpawnPoint);
            }

            // Set a waypoint to the blip
            SuspectBlip.IsRouteEnabled = true;


            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            // First scenario: Suspect does not exist
            if (!SuspectExists)
            {
                if(Game.LocalPlayer.Character.DistanceTo(SpawnPoint) < 10f)
                {
                    Game.DisplayNotification("The call appears to be a false alarm!");
                    End();
                }
            }
            else if(SuspectExists)
            {
                // First off, set them as wanted if they're wanted.
                if (SuspectIsWanted) SuspectPed.SetWanted(true);

                // Now branch
                if (SuspectIsHostile) // If the suspect is hostile
                {
                    //SuspectPed.SetWanted(true);

                } else if (SuspectIsRunner) // If the suspect is supposed to run
                {
                    if(Game.LocalPlayer.Character.DistanceTo(SuspectPed) < 20f)
                    {
                        // Remove control from our plugin
                        // SuspectPed.Dismiss();
                        // Create a pursuit and add our suspect
                        Pursuit = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(Pursuit, SuspectPed);

                        // Display a notification
                        Game.DisplayNotification("The suspect became alarmed and is fleeing! Arrest him!");
                        // End();
                    }
                } else // Suspect is niether hostile nor a runner! Tell him to fuck off.
                {
                    if(!DismissNotifDisplayed && Game.LocalPlayer.Character.DistanceTo(SuspectPed) < 20f)
                    {
                        Game.DisplayNotification("Press 0 to ask the suspect to leave.");
                        DismissNotifDisplayed = true;
                    } else if(DismissNotifDisplayed && Game.LocalPlayer.Character.DistanceTo(SuspectPed) < 20f)
                    {
                        if (Game.IsKeyDown(Keys.D0))
                        {
                            Game.DisplaySubtitle("Suspect: Sorry officer, I'll be leaving now");
                            //SuspectPed.Dismiss();
                            End();
                        }
                    }
                }

                if (SuspectPed.IsDead || Functions.IsPedArrested(SuspectPed))
                {
                    Game.DisplayNotification("Good work!");
                    End();
                }
            }
        }

        public override void End()
        {
            base.End();
            if(ShouldSayEndingAudio)
            {
                Functions.PlayScannerAudio("CODE_4");
                Game.DisplayNotification("Situation code 4");
            }
            

            if (SuspectPed.Exists())
            {
                if (Functions.IsPedArrested(SuspectPed) && !SuspectPed.IsDead)
                {
                    if(SuspectPed.IsWanted())
                    {
                        Game.DisplayNotification("Wanted suspect arrested! Good job!");
                    } else
                    {
                        Game.DisplayNotification("Suspect was not wanted and should not have been arrested!");
                    }
                } else
                {
                    if(SuspectPed.IsWanted())
                    {
                        Game.DisplayNotification("Suspect was wanted and should have been arrested!");
                    }

                }
                SuspectPed.Dismiss();
            }
            if (SuspectBlip.Exists()) SuspectBlip.Delete();
            if (RadiusBlip.Exists()) RadiusBlip.Delete();
        }
    }
}
