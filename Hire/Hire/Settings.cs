
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;


namespace Hire
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class HireSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "TRP Hire"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "TRP Hire"; } }
        public override string DisplaySection { get { return "TRP Hire"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }


        [GameParameters.CustomParameterUI("Use Stock Center cost for Kerbals",
            toolTip ="Using the Stock Center costs will result in varying base costs for each Kerbal, based on how many Kerbals are active")]
        public bool KStockCost = false;


        [GameParameters.CustomFloatParameterUI("Panda Center Base Cost ", minValue = 12500f, maxValue = 50000f,
            toolTip ="This is the base cost for each Kerbal when using the Panda Center Base Cost")]
        public double const_cost = 25000f;

        [GameParameters.CustomParameterUI("Disable all modifiers and discounts")]
        public bool disableAllModifiers = false;

        [GameParameters.CustomFloatParameterUI("Fearless Cost Modifier", minValue = 1.0f, maxValue = 3f)]
        public double fearless_coef = 2f;

        [GameParameters.CustomFloatParameterUI("Gender Cost Modifier", minValue = 0f, maxValue = 2f)]
        public double gender_coef = 1.25f;

        [GameParameters.CustomFloatParameterUI("Bulk (2-5) Discount (%)", minValue = 0f, maxValue = 40f)]
        public double bulk_discount1 = 15f;

        [GameParameters.CustomFloatParameterUI("Bulk (6-10) Discount (%)", minValue = 0.0f, maxValue = 40f)]
        public double bulk_discount2 = 30f;

        [GameParameters.CustomFloatParameterUI("Black Munday Discount (%)", minValue = 0f, maxValue = 20f)]
        public double black_discount = 10f;

        [GameParameters.CustomFloatParameterUI("New Year Discount (%)", minValue = 0f, maxValue = 75f)]
        public double new_year_discount = 50f;




        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            KStockCost = false;
            const_cost = 25000;
            fearless_coef = 2f;
            gender_coef = 1.25f;
            bulk_discount1 = 15f;
            bulk_discount2 = 30f;
            black_discount = 10f;
            new_year_discount = 50f;
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (bulk_discount2 < bulk_discount1)
                bulk_discount2 = bulk_discount1;
            if (KStockCost && member.Name == "const_cost")
                return false;
            if (member.Name == "KStockCost" || member.Name == "const_cost")
                return true;
            if (disableAllModifiers)
                return false;
            return true;
        }


        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }
}
