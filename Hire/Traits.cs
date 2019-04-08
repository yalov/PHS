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
        bool Loaded = false;
        internal List<TraitTitle> traitTitles = new List<TraitTitle>();
        internal GUIContent[] KCareerGrid;
        internal int KCareerPerRow = 0;

        public Traits()
        {
            InitTraits();
            CTIWrapper.initCTIWrapper();

            KCareerPerRow = Math.Min(4, traitTitles.Count());
            KCareerGrid = new GUIContent[traitTitles.Count()];
            bool useCTI = CTIWrapper.CTI != null && CTIWrapper.CTI.Loaded;
            for (int i = 0; i < traitTitles.Count(); i++)
            {
                GUIContent gc;
                if (useCTI)
                {
                    var t = CTIWrapper.CTI.getTrait(traitTitles[i].name);
                    if (t != null)
                        gc = new GUIContent(traitTitles[i].title, t.Icon);
                    else
                        gc = new GUIContent(traitTitles[i].title);
                }
                else
                {
                    gc = new GUIContent(traitTitles[i].title);
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

        public void InitTraits()
        {
            if (!Loaded)
            {
                Loaded = true;

                List<ExperienceTraitConfig> etcs = GameDatabase.Instance.ExperienceConfigs.Categories;
                for (int i = 0; i < etcs.Count; i++)
                {
                    string n = etcs[i].Name;
                    if (String.IsNullOrEmpty(n) || String.Equals(n, "Tourist"))
                        continue;

                    string t = etcs[i].Title;
                    if (String.IsNullOrEmpty(t))
                        t = n;

                    traitTitles.Add(new TraitTitle(n, t));
                }
            }
        }
    }
}
