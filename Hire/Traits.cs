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
            return Localizer.Format(traitTitles[s].title);

        }
        public Traits()
        {
            InitTraits();
            CTIWrapper.initCTIWrapper();

            KCareerStrings = new string[traitTitles.Count()];
            int i = 0;
            foreach (var tt in traitTitles.Values)
            {
                KCareerStrings[i++] = tt.name;
            }

            KCareerCnt = Math.Min(4, KCareerStrings.Count());
            KCareerGrid = new GUIContent[KCareerCnt];
            for (i = 0; i < KCareerCnt; i++)
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

        public class TraitTitle
        {
            public string name;
            public string title;

            public TraitTitle(string n, string t)
            {
                name = n;
                title = t;
            }
        }
        Dictionary<string, TraitTitle> traitTitles = new Dictionary<string, TraitTitle>();

        public void InitTraits()
        {
            if (!Loaded)
            {
                Loaded = true;

                ConfigNode[] configNodes = GameDatabase.Instance.GetConfigNodes("EXPERIENCE_TRAIT");
                for (int i = 0; i < configNodes.Count(); i++)
                {
                    ExperienceTraitConfig experienceTraitConfig = ExperienceTraitConfig.Create(configNodes[i]);
                    if (experienceTraitConfig != null)
                    {
                        TraitTitle tt;
                        if (experienceTraitConfig.Name != null
                             && experienceTraitConfig.Name != "Tourist" && experienceTraitConfig.Name != "Unknown")
                            
                            {
                            if (experienceTraitConfig.Title != null)
                                tt = new TraitTitle(experienceTraitConfig.Name, experienceTraitConfig.Title);
                            else
                                tt = new TraitTitle(experienceTraitConfig.Name, experienceTraitConfig.Name);
                            traitTitles.Add(experienceTraitConfig.Name, tt);
                        }
                    }
                }
            }
        }
    }
}
