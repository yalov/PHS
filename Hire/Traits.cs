using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using CommunityTraitIcons;
using UnityEngine;
using KSP.Localization;
using Experience;

namespace Hire
{

    public class Traits
    {
        internal List<string> traits = new List<string>();
        internal string[] KCareerStrings;
        internal int KCareerCnt = 0;
        internal GUIContent[] KCareerGrid;

        bool Loaded = false;

        string GetLocalizedCareerString(string s)
        {
            if (s == "Pilot")
                return Localizer.Format("#autoLOC_500101");
            if (s == "Engineer")
                return Localizer.Format("#autoLOC_500103");
            if (s == "Scientist")
                return Localizer.Format("#autoLOC_500105");

            if (s == "Unknown")
                return Localizer.Format("#autoLOC_168872");
            if (s == "Tourist")
                return Localizer.Format("#autoLOC_476080");

            // The following is from the Community Trait Icons mod
            if (s == "Colonist")
                return Localizer.Format("#TRPHire_Colonist");
            if (s == "Geologist")
                return Localizer.Format("#TRPHire_Geologist");
            if (s == "Miner")
                return Localizer.Format("#TRPHire_Miner");
            if (s == "Mechanic")
                return Localizer.Format("#TRPHire_Mechanic");
            if (s == "Technician")
                return Localizer.Format("#TRPHire_Technician");
            if (s == "Biologist")
                return Localizer.Format("#TRPHire_Biologist");
            if (s == "Farmer")
                return Localizer.Format("#TRPHire_Farmer");
            if (s == "Medic")
                return Localizer.Format("#TRPHire_Medic");
            if (s == "Quartermaster")
                return Localizer.Format("#TRPHire_Quartermaster");
            if (s == "Kolonist")
                return Localizer.Format("#TRPHire_Kolonist");
            if (s == "Scout")
                return Localizer.Format("#TRPHire_Scout");


            return Localizer.Format(s);

        }
        public Traits()
        {
            InitTraits();
            CTIWrapper.initCTIWrapper();

            KCareerStrings = new string[traits.Count()];
            for (int i = 0; i < traits.Count(); i++)
            {
                KCareerStrings[i] = traits[i];
            }

            KCareerCnt = Math.Min(4, KCareerStrings.Count());
            KCareerGrid = new GUIContent[KCareerCnt];
            for (int i = 0; i < KCareerCnt; i++)
            {
                GUIContent gc;
                if (CTIWrapper.CTI != null && CTIWrapper.CTI.Loaded)
                {
                    var t = CTIWrapper.CTI.getTrait(KCareerStrings[i]);
                    if (t != null)
                        gc = new GUIContent(GetLocalizedCareerString(KCareerStrings[i]), t.Icon);
                    else
                        gc = new GUIContent(GetLocalizedCareerString(KCareerStrings[i]));
                }
                else
                {
                    gc = new GUIContent(GetLocalizedCareerString(KCareerStrings[i]));
                }
                KCareerGrid[i] = gc;
            }

        }


        public void InitTraits()
        {
            if (!Loaded)
            {
                Loaded = true;

                ExperienceSystemConfig esc = new ExperienceSystemConfig();

                esc.LoadTraitConfigs();


                foreach (var s in esc.TraitNames)
                {
                    traits.Add(s);
                }

                int cnt = traits.Count();
                for (int i = 0; i < cnt; i++)
                    if (traits[i] == "Tourist" || traits[i] == "Unknown")
                    {
                        traits.Remove(traits[i]);
                        cnt--;
                        i--;
                    }

            }
        }
    }
}
