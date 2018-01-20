using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using CommunityTraitIcons;
using UnityEngine;

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

        public Traits()
        {
            InitTraits();
            CTIWrapper.initCTIWrapper();

            KCareerStrings = new string[traits.Count()];
            for (int i = 0; i < traits.Count(); i++)
                KCareerStrings[i] = traits[i];


            KCareerCnt = Math.Min(4, KCareerStrings.Count());
            KCareerGrid = new GUIContent[KCareerCnt];
            for (int i = 0; i < KCareerCnt; i++)
            {
                GUIContent gc;
                if (CTIWrapper.CTI != null && CTIWrapper.CTI.Loaded)
                {
                    var t = CTIWrapper.CTI.getTrait(KCareerStrings[i]);
                    if (t != null)
                        gc = new GUIContent(KCareerStrings[i], t.Icon);
                    else
                        gc = new GUIContent(KCareerStrings[i]);
                }
                else
                {
                    gc = new GUIContent(KCareerStrings[i]);
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
