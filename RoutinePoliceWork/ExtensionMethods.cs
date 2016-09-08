using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Engine.Scripting.Entities;
using Rage;

namespace RoutinePoliceWork
{
    public static class ExtensionMethods
    {
        public static Ped SetWanted(this Ped oPed, bool isWanted)
        {
            Persona oldPersona = Functions.GetPersonaForPed(oPed);
            Persona newPersona = new Persona(oPed, oldPersona.Gender, oldPersona.BirthDay, oldPersona.Citations, oldPersona.Forename, oldPersona.Surname, oldPersona.LicenseState, oldPersona.TimesStopped, isWanted, oldPersona.IsAgent, oldPersona.IsCop);
            Functions.SetPersonaForPed(oPed, newPersona);
            return oPed;
        }

        public static bool IsWanted(this Ped oPed)
        {
            Persona persona = Functions.GetPersonaForPed(oPed);
            return persona.Wanted;
        }
    }
}
